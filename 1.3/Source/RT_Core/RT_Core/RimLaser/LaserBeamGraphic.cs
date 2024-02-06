using System;
using UnityEngine;
using Verse;

namespace RT_Core;

internal class LaserBeamGraphic : Thing
{
	public LaserBeamDef projDef;

	private int ticks;

	private int colorIndex = 2;

	private Vector3 a;

	private Vector3 b;

	public Matrix4x4 drawingMatrix = default(Matrix4x4);

	private Material materialBeam;

	private Mesh mesh;

	public float Opacity => (float)Math.Sin(Math.Pow(1.0 - 1.0 * (double)ticks / (double)projDef.lifetime, projDef.impulse) * Math.PI);

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref ticks, "ticks", 0);
		Scribe_Values.Look(ref colorIndex, "colorIndex", 0);
		Scribe_Values.Look(ref a, "a");
		Scribe_Values.Look(ref b, "b");
		Scribe_Defs.Look(ref projDef, "projectileDef");
	}

	public override void Tick()
	{
		if (def == null || ticks++ > projDef.lifetime)
		{
			Destroy();
		}
	}

	private void SetColor(Thing launcher)
	{
		IBeamColorThing beamColorThing = null;
		if (launcher is Pawn { equipment: not null } pawn)
		{
			beamColorThing = pawn.equipment.Primary as IBeamColorThing;
		}
		if (beamColorThing == null)
		{
			beamColorThing = launcher as IBeamColorThing;
		}
		if (beamColorThing != null && beamColorThing.BeamColor != -1)
		{
			colorIndex = beamColorThing.BeamColor;
		}
	}

	public void Setup(Thing launcher, Vector3 origin, Vector3 destination)
	{
		SetColor(launcher);
		a = origin;
		b = destination;
	}

	public void SetupDrawing()
	{
		if (!(mesh != null))
		{
			materialBeam = projDef.GetBeamMaterial(colorIndex) ?? def.graphicData.Graphic.MatSingle;
			float beamWidth = projDef.beamWidth;
			Quaternion q = Quaternion.LookRotation(b - a);
			Vector3 normalized = (b - a).normalized;
			float magnitude = (b - a).magnitude;
			Vector3 s = new(beamWidth, 1f, magnitude);
			Vector3 pos = (a + b) / 2f;
			drawingMatrix.SetTRS(pos, q, s);
			float num = 1f * (float)materialBeam.mainTexture.width / (float)materialBeam.mainTexture.height;
			float num2 = ((projDef.seam < 0f) ? num : projDef.seam);
			float num3 = beamWidth / num / 2f * num2;
			float sv = ((magnitude <= num3 * 2f) ? 0.5f : (num3 * 2f / magnitude));
			mesh = MeshMakerLaser.Mesh(num2, sv);
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (projDef == null || projDef.decorations == null || respawningAfterLoad)
		{
			return;
		}
		foreach (LaserBeamDecoration decoration in projDef.decorations)
		{
			float num = decoration.spacing * projDef.beamWidth;
			float num2 = decoration.initialOffset * projDef.beamWidth;
			Vector3 normalized = (b - a).normalized;
			float num3 = (b - a).AngleFlat();
			Vector3 vector = normalized * num;
			Vector3 exactPosition = a + vector * 0.5f + normalized * num2;
			float num4 = (b - a).magnitude - num;
			int num5 = 0;
			while (num4 > 0f && ThingMaker.MakeThing(decoration.mote) is MoteLaserDecoration moteLaserDecoration)
			{
				moteLaserDecoration.beam = this;
				moteLaserDecoration.airTimeLeft = projDef.lifetime;
				moteLaserDecoration.Scale = projDef.beamWidth;
				moteLaserDecoration.exactRotation = num3;
				moteLaserDecoration.exactPosition = exactPosition;
				moteLaserDecoration.SetVelocity(num3, decoration.speed);
				moteLaserDecoration.baseSpeed = decoration.speed;
				moteLaserDecoration.speedJitter = decoration.speedJitter;
				moteLaserDecoration.speedJitterOffset = decoration.speedJitterOffset * (float)num5;
				GenSpawn.Spawn(moteLaserDecoration, a.ToIntVec3(), map);
				exactPosition += vector;
				num4 -= num;
				num5++;
			}
		}
	}

	public override void Draw()
	{
		SetupDrawing();
		if (projDef.disableFading)
		{
			Graphics.DrawMesh(mesh, drawingMatrix, materialBeam, 0);
			return;
		}
		float opacity = Opacity;
		Graphics.DrawMesh(mesh, drawingMatrix, FadedMaterialPool.FadedVersionOf(materialBeam, opacity), 0);
	}
}

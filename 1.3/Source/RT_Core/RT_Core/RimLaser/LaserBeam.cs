using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RT_Core;

public class LaserBeam : Projectile
{
	private LaserBeamDef Def => base.def as LaserBeamDef;

	public override void Draw()
	{
	}

	private void TriggerEffect(EffecterDef effect, Vector3 position)
	{
		TriggerEffect(effect, IntVec3.FromVector3(position));
	}

	private void TriggerEffect(EffecterDef effect, IntVec3 dest)
	{
		if (effect != null)
		{
			TargetInfo targetInfo = new(dest, base.Map);
			Effecter effecter = effect.Spawn();
			effecter.Trigger(targetInfo, targetInfo);
			effecter.Cleanup();
		}
	}

	private void SpawnBeam(Vector3 a, Vector3 b)
	{
		if (ThingMaker.MakeThing(Def.beamGraphic) is LaserBeamGraphic laserBeamGraphic)
		{
			laserBeamGraphic.projDef = Def;
			laserBeamGraphic.Setup(launcher, a, b);
			GenSpawn.Spawn(laserBeamGraphic, origin.ToIntVec3(), base.Map);
		}
	}

	private void SpawnBeamReflections(Vector3 a, Vector3 b, int count)
	{
		for (int i = 0; i < count; i++)
		{
			Vector3 normalized = (b - a).normalized;
			Vector3 b2 = b - normalized.RotatedBy(Rand.Range(-22.5f, 22.5f)) * Rand.Range(1f, 4f);
			SpawnBeam(b, b2);
		}
	}

	public HashSet<IntVec3> MakeLine(IntVec3 start, IntVec3 end, Map map)
	{
		ShootLine shootLine = new(start, end);
		HashSet<IntVec3> hashSet = new();
		List<IntVec3> list = shootLine.Points().ToList();
		foreach (IntVec3 item in list)
		{
			hashSet.Add(item);
			IEnumerable<IntVec3> enumerable = GenAdj.CellsAdjacent8Way(new TargetInfo(item, map));
			foreach (IntVec3 item2 in enumerable)
			{
				if (item2.DistanceTo(item) <= Def.fireWidth && item2.DistanceTo(end) < list[0].DistanceTo(end))
				{
					hashSet.Add(item2);
				}
			}
		}
		return hashSet.Where((IntVec3 x) => x.DistanceTo(start) > Def.fireDistanceFromCaster).ToHashSet();
	}

	protected virtual void Impact(Thing hitThing)
	{
		LaserGunDef laserGunDef = equipmentDef as LaserGunDef;
		Vector3 normalized = (destination - origin).normalized;
		normalized.y = 0f;
		Vector3 vector = origin + normalized * (laserGunDef?.barrelLength ?? 0.9f);
        Vector3 vector2;
        if (hitThing == null)
		{
			vector2 = destination;
		}
		else if ((destination - hitThing.TrueCenter()).magnitude < 1f)
		{
			vector2 = destination;
		}
		else
		{
			vector2 = hitThing.TrueCenter();
			vector2.x += Rand.Range(-0.5f, 0.5f);
			vector2.z += Rand.Range(-0.5f, 0.5f);
		}
		vector.y = (vector2.y = Def.Altitude);
		SpawnBeam(vector, vector2);
		Pawn pawn = launcher as Pawn;
		if (Def.spawnFire)
		{
			foreach (IntVec3 item in MakeLine(origin.ToIntVec3(), destination.ToIntVec3(), pawn.Map))
			{
				if (!(item == pawn.Position) || !Def.dontSpawnFireOnCaster)
				{
					Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
					fire.fireSize = 1f;
					Thing newThing = ThingMaker.MakeThing(ThingDef.Named("RT_Dummy_Grass"));
					GenSpawn.Spawn(newThing, item, pawn.Map, Rot4.North);
					GenSpawn.Spawn(fire, item, pawn.Map, Rot4.North);
				}
			}
		}
		IDrawnWeaponWithRotation drawnWeaponWithRotation = null;
		if (pawn != null && pawn.equipment != null)
		{
			drawnWeaponWithRotation = pawn.equipment.Primary as IDrawnWeaponWithRotation;
		}
		if (drawnWeaponWithRotation != null)
		{
			float num = (vector2 - vector).AngleFlat() - (intendedTarget.CenterVector3 - vector).AngleFlat();
			drawnWeaponWithRotation.RotationOffset = (num + 180f) % 360f - 180f;
		}
		TriggerEffect(Def.explosionEffect, vector2);
		base.Impact(hitThing);
		BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new(launcher, hitThing, intendedTarget.Thing, ThingDef.Named("Gun_Autopistol"), Def, targetCoverDef);
		Find.BattleLog.Add(battleLogEntry_RangedImpact);
		if (hitThing != null)
		{
			DamageDef damageDef = Def.projectile.damageDef;
			float amount = base.DamageAmount;
			float armorPenetration = base.ArmorPenetration;
			float y = ExactRotation.eulerAngles.y;
			Thing instigator = launcher;
            DamageInfo dinfo = new(damageDef, amount, armorPenetration, y, instigator, null, null, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
			hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
			if (hitThing is Pawn { stances: not null } pawn2 && pawn2.BodySize <= Def.projectile.StoppingPower + 0.001f)
			{
				pawn2.stances.stagger.StaggerFor(95);
			}
		}
	}
}

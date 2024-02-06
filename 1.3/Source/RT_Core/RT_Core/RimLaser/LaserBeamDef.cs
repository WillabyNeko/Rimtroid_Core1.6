using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RT_Core;

public class LaserBeamDef : ThingDef
{
	public float capSize = 1f;

	public float capOverlap = 0.0171875f;

	public int lifetime = 30;

	public float impulse = 4f;

	public bool disableFading;

	public float beamWidth = 1f;

	public float fireWidth = 1f;

	public float fireDistanceFromCaster = 1f;

	public bool dontSpawnFireOnCaster = false;

	public bool spawnFire = false;

	public float shieldDamageMultiplier = 0.5f;

	public float seam = -1f;

	public List<LaserBeamDecoration> decorations;

	public EffecterDef explosionEffect;

	public EffecterDef hitLivingEffect;

	public ThingDef beamGraphic;

	public List<string> textures;

	private List<Material> materials = new();

	public bool IsWeakToShields => shieldDamageMultiplier < 1f;

	private void CreateGraphics()
	{
		for (int i = 0; i < textures.Count; i++)
		{
			materials.Add(MaterialPool.MatFrom(textures[i], ShaderDatabase.TransparentPostLight));
		}
	}

	public Material GetBeamMaterial(int index)
	{
		if (materials.Count == 0 && textures.Count != 0)
		{
			CreateGraphics();
		}
		if (materials.Count == 0)
		{
			return null;
		}
		if (index >= materials.Count || index < 0)
		{
			index = 0;
		}
		return materials[index];
	}
}

using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using System.Collections.Generic;

namespace RT_Core
{
	//Create explosion but no fire.
	public class Projectile_ExplodeNoFire : Projectile
	{
		protected virtual void Impact(Thing hitThing)
		{
			Ignite();
		}

		protected virtual void Ignite()
		{
			Map map = Map;
			Destroy();
			var radius = def.projectile.explosionRadius;
			var cellsToAffect = SimplePool<List<IntVec3>>.Get();
			cellsToAffect.Clear();
			cellsToAffect.AddRange(def.projectile.damageDef.Worker.ExplosionCellsToHit(Position, map, radius));

			FleckMaker.Static(Position, map, FleckDefOf.ExplosionFlash, radius * 4f);
			for (int i = 0; i < 4; i++)
			{
				FleckMaker.ThrowSmoke(Position.ToVector3Shifted() + Gen.RandomHorizontalVector(radius * 0.7f), map, radius * 0.6f);
			}


			//Fire explosion should be tiny.
			if (this.def.projectile.explosionEffect != null)
			{
				Effecter effecter = this.def.projectile.explosionEffect.Spawn();
				effecter.Trigger(new TargetInfo(this.Position, map, false), new TargetInfo(this.Position, map, false));
				effecter.Cleanup();
			}
			GenExplosion.DoExplosion(
                base.Position,
                map,
                def.projectile.explosionRadius,
                def.projectile.damageDef,
                launcher,
                def.projectile.GetDamageAmount(1, null),
                def.projectile.GetArmorPenetration(1, null),
                def.projectile.soundExplode,
                equipmentDef,
                def,
                (Thing)null,
                def.projectile.postExplosionSpawnThingDef,
                def.projectile.postExplosionSpawnChance,
                def.projectile.postExplosionSpawnThingCount,
				null,
                def.projectile.applyDamageToExplosionCellsNeighbors,
                def.projectile.preExplosionSpawnThingDef,
                def.projectile.preExplosionSpawnChance,
                def.projectile.preExplosionSpawnThingCount,
                def.projectile.explosionChanceToStartFire,
                def.projectile.explosionDamageFalloff);
		}

		public override Quaternion ExactRotation
		{
			get
			{
				var forward = destination - origin;
				forward.y = 0;
				return Quaternion.LookRotation(forward);
			}
		}
	}
}
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RT_Core;

public class AnimalProjectile : Projectile
{
	protected virtual void Impact(Thing hitThing)
	{
		Map map = base.Map;
		base.Impact(hitThing);
		BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new(launcher, hitThing, intendedTarget.Thing, ThingDef.Named("Gun_Autopistol"), def, targetCoverDef);
		Find.BattleLog.Add(battleLogEntry_RangedImpact);
		if (hitThing != null)
		{
			DamageDef damageDef = def.projectile.damageDef;
			float num = base.DamageAmount;
			float armorPenetration = base.ArmorPenetration;
			float y = ExactRotation.eulerAngles.y;
			Thing instigator = launcher;
            DamageInfo dinfo = new(damageDef, num, armorPenetration, y, instigator, null, null, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
			hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
			if (hitThing is Pawn { stances: not null } pawn && pawn.BodySize <= def.projectile.StoppingPower + 0.001f)
			{
				pawn.stances.stagger.StaggerFor(95);
			}
			if (def.defName == "RT_FrostWeb")
			{
				DamageInfo dinfo2 = new(DamageDefOf.Frostbite, num / 2f, armorPenetration, y, instigator, null, null, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
				hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
			}
			if (def.defName == "RT_FireWeb")
			{
				DamageInfo dinfo3 = new(DamageDefOf.Burn, num / 2f, armorPenetration, y, instigator, null, null, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
				hitThing.TakeDamage(dinfo3).AssociateWithLog(battleLogEntry_RangedImpact);
			}
			if (def.defName == "RT_AcidicWeb")
			{
				DamageInfo dinfo4 = new(DefDatabase<DamageDef>.GetNamed("RT_AcidSpit"), num / 2f, armorPenetration, y, instigator, null, null, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
				hitThing.TakeDamage(dinfo4).AssociateWithLog(battleLogEntry_RangedImpact);
			}
			if (def.defName == "RT_ExplodingWeb")
			{
				DamageInfo dinfo5 = new(DamageDefOf.Bomb, num / 2f, armorPenetration, y, instigator, null, null, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
				hitThing.TakeDamage(dinfo5).AssociateWithLog(battleLogEntry_RangedImpact);
			}
		}
		else
		{
			SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map));
			FleckMaker.Static(ExactPosition, map, FleckDefOf.ShotHit_Dirt);
			if (base.Position.GetTerrain(map).takeSplashes)
			{
				FleckMaker.WaterSplash(ExactPosition, map, Mathf.Sqrt(base.DamageAmount) * 1f, 4f);
			}
		}
	}
}

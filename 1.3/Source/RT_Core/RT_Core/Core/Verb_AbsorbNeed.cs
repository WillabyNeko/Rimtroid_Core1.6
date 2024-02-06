using RimWorld;
using UnityEngine;
using Verse;

namespace RT_Core;

internal class Verb_AbsorbNeed : Verb_MeleeAttackDamage
{
	public VerbProperties_AbsorbNeed AbsorbProps => (VerbProperties_AbsorbNeed)verbProps;

	protected override bool TryCastShot()
	{
		Pawn casterPawn = CasterPawn;
		if (currentTarget.Thing is Pawn pawn && casterPawn.needs?.food != null && pawn?.needs?.food != null)
		{
			if (pawn.needs == null || pawn.needs.food == null)
			{
				return false;
			}
			if (!casterPawn.Spawned || !pawn.Spawned)
			{
				return false;
			}
			if (casterPawn.needs.food.CurLevel >= AbsorbProps.stopFeedingAt)
			{
				return false;
			}
			if (casterPawn.needs.food.CurLevel >= 1f || pawn.needs.food.CurLevel <= 0f)
			{
				return false;
			}
			AbsorbNeed(casterPawn.needs.food, pawn.needs.food, AbsorbProps.absorbAmount);
			GainAge(casterPawn, AbsorbProps.ageDaysAmount);
		}
		return base.TryCastShot();
	}

	public void GainAge(Pawn caster, int ageDays)
	{
		caster.ageTracker.AgeBiologicalTicks += ageDays * 60000;
	}

	public void AbsorbNeed(Need casterFood, Need targetFood, float absorbtionRate = 0.03f)
	{
		float num = Mathf.Min(targetFood.MaxLevel * absorbtionRate, targetFood.CurLevel);
		casterFood.CurLevel += num;
	}
}

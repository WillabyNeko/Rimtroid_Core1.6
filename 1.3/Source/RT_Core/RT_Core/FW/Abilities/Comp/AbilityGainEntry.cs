using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RT_Core;

public class AbilityGainEntry
{
	public AbilityDef abilityDef;

	public HediffDef hediffDef;

	public BodyPartDef bodyPartDef;

	public List<AbilityGainCondition> conditions;

	public bool ShouldGainHediff => hediffDef != null;

	public bool ConditionsSatisfied(CompAbilityDefinition def)
	{
		if (!conditions.NullOrEmpty())
		{
			foreach (AbilityGainCondition condition in conditions)
			{
				if (!condition.IsSatisfied(def))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool ConditionsFulfilled(CompAbilityDefinition def)
	{
		if (!conditions.NullOrEmpty())
		{
			foreach (AbilityGainCondition condition in conditions)
			{
				if (!condition.IsFulfilled(def))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool HasAbility(Pawn pawn)
	{
		Ability ability = pawn.abilities.GetAbility(abilityDef);
		return pawn.abilities.GetAbility(abilityDef) != null;
	}

	public void GainAbility(Pawn pawn)
	{
		pawn.abilities.GainAbility(abilityDef);
	}

	public bool HasHediff(Pawn pawn)
	{
		return pawn.health.hediffSet.HasHediff(hediffDef);
	}

	public void GainHediff(Pawn pawn)
	{
		List<Hediff> list = new();
		List<BodyPartDef> partsToAffect = null;
		if (bodyPartDef != null)
		{
			partsToAffect = new List<BodyPartDef> { bodyPartDef };
		}
		HediffGiverUtility.TryApply(pawn, hediffDef, partsToAffect, canAffectAnyLivePart: false, 1, list);
		foreach (HediffComp_GrowthSeverityScaling item in list.Select((Hediff h) => h.TryGetComp<HediffComp_GrowthSeverityScaling>()).OfType<HediffComp_GrowthSeverityScaling>())
		{
			item.abilityDef = abilityDef;
		}
	}
}

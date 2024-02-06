using Verse;

namespace RT_Core;

public class AbilityGainAgeCondition : AbilityGainCondition
{
	public FloatRange ageRange;

	public override bool IsSatisfied(CompAbilityDefinition def)
	{
		return (float)def.SelfPawn.ageTracker.AgeBiologicalYears >= ageRange.TrueMin;
	}

	public override bool IsFulfilled(CompAbilityDefinition def)
	{
		return (float)def.SelfPawn.ageTracker.AgeBiologicalYears > ageRange.TrueMax;
	}
}

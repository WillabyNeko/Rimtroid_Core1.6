using Verse;

namespace RT_Core;

public class AbilityGainKillCondition : AbilityGainCondition
{
	public IntRange killCount;

	public override bool IsSatisfied(CompAbilityDefinition def)
	{
		return def.KillCounter >= killCount.TrueMin;
	}

	public override bool IsFulfilled(CompAbilityDefinition def)
	{
		return def.KillCounter > killCount.TrueMax;
	}
}

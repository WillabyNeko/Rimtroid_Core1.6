using Verse;

namespace RT_Core;

public class AbilityGainDamageCondition : AbilityGainCondition
{
	public FloatRange damageRange;

	public override bool IsSatisfied(CompAbilityDefinition def)
	{
		return def.DamageTotal >= damageRange.TrueMin;
	}

	public override bool IsFulfilled(CompAbilityDefinition def)
	{
		return def.DamageTotal > damageRange.TrueMax;
	}
}

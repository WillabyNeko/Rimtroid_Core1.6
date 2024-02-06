namespace RT_Core;

public abstract class AbilityGainCondition
{
	public abstract bool IsSatisfied(CompAbilityDefinition def);

	public abstract bool IsFulfilled(CompAbilityDefinition def);
}

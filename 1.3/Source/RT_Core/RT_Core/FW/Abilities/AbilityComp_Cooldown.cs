using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RT_Core;

public class AbilityComp_Cooldown : AbilityComp_Base
{
	public AbilityCompProperties_Cooldown VProps => props as AbilityCompProperties_Cooldown;

	public string CooldownPool => VProps.cooldownPool;

	public virtual bool IsIndependent => VProps.independent;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		IEnumerable<Ability> source = parent.pawn.abilities.abilities.Where((Ability ability) => ability != parent && ability.HasCooldown && ability.CompOfType<AbilityComp_Cooldown>() != null);
		source = source.Where((Ability ability) => !ability.CompOfType<AbilityComp_Cooldown>().IsIndependent);
		if (VProps.cooldownPool != null)
		{
			source = source.Where((Ability ability) => ability.CompOfType<AbilityComp_Cooldown>().CooldownPool == CooldownPool);
		}
		if (!VProps.resetsTimer)
		{
			source = source.Where((Ability ability) => ability.CooldownTicksRemaining <= 0);
		}
		foreach (Ability item in source)
		{
			if (VProps.cooldownTicksRange == default(IntRange))
			{
				item.StartCooldown(item.def.cooldownTicksRange.RandomInRange);
			}
			else
			{
				item.StartCooldown(VProps.cooldownTicksRange.RandomInRange);
			}
		}
	}
}

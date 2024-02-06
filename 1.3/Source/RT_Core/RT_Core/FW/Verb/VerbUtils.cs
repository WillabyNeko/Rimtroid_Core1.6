using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RT_Core;

public static class VerbUtils
{
	public static IEnumerable<Verb> GetVerbs(Pawn pawn)
	{
		IEnumerable<Verb> enumerable = new List<Verb>();
		if (pawn.equipment != null && pawn.equipment.AllEquipmentVerbs != null)
		{
			enumerable = enumerable.Concat(pawn.equipment.AllEquipmentVerbs);
		}
		if (pawn.VerbTracker != null && pawn.VerbTracker.AllVerbs != null)
		{
			enumerable = enumerable.Concat(pawn.VerbTracker.AllVerbs);
		}
		if (pawn.abilities != null && pawn.abilities.abilities != null)
		{
			foreach (VerbTracker item in from ability in pawn.abilities.abilities.OfType<Ability_Base>()
				where ability.CanAutoCast
				select ability into a
				select a.VerbTracker)
			{
				if (item != null && item.AllVerbs.Any())
				{
					enumerable = enumerable.Concat(item.AllVerbs);
				}
			}
		}
		return enumerable;
	}

	public static IEnumerable<Verb> GetPossibleVerbs(Pawn pawn)
	{
		return from verb in GetVerbs(pawn)
			where verb.Available()
			select verb;
	}

	public static IEnumerable<Verb> Filter_KeepMelee(this IEnumerable<Verb> verbs)
	{
		return verbs.Where((Verb verb) => verb.IsMeleeAttack);
	}

	public static IEnumerable<Verb> Filter_KeepRanged(this IEnumerable<Verb> verbs)
	{
		return verbs.Where((Verb verb) => !verb.IsMeleeAttack);
	}

	public static IEnumerable<Verb> Filter_KeepInRange(this IEnumerable<Verb> verbs, Thing target)
	{
		return verbs.Where((Verb verb) => verb.CanHitTarget(target));
	}

	public static IEnumerable<Verb> Sort_OrderByPreference(this IEnumerable<Verb> verbs)
	{
		return verbs.OrderByDescending((Verb verb) => verb.verbProps.commonality);
	}

	public static Verb Get_MostPreferred(this IEnumerable<Verb> verbs, bool primary)
	{
		if (primary)
		{
			return verbs.MaxByWithFallback((Verb verb) => Mathf.Pow(verb.verbProps.range - verb.verbProps.EffectiveMinRange(allowAdjacentShot: true), 2f));
		}
		return verbs.RandomElementByWeightWithFallback((Verb verb) => verb.verbProps.commonality);
	}

	public static float CalculateCommonality(this Verb verb, Pawn source, Thing target)
	{
		if (source.IsAdjacentToCardinalOrInside(target))
		{
			return verb.verbProps.commonality;
		}
		return verb.verbProps.commonality + verb.verbProps.EffectiveMinRange(target, source) + verb.verbProps.range;
	}
}

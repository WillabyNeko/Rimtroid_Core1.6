using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RT_Core;

public class CompAbilityDefinition : ThingComp
{
	private float damageTotal;

	private int killCounter;

	public CompProperties_AbilityDefinition Props => (CompProperties_AbilityDefinition)props;

	public Pawn SelfPawn => (Pawn)parent;

	public IEnumerable<AbilityGainEntry> GainableAbilities => Props.abilities.Where((AbilityGainEntry entry) => !entry.HasAbility(SelfPawn));

	public float DamageTotal => damageTotal;

	public int KillCounter => killCounter;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (respawningAfterLoad)
		{
			foreach (AbilityGainEntry item in Props.abilities.Where((AbilityGainEntry entry) => !entry.HasAbility(SelfPawn) && !entry.HasHediff(SelfPawn) && entry.ConditionsFulfilled(this)))
			{
				item.GainAbility(SelfPawn);
			}
			return;
		}
		if (!Props.spawnKillCount.EnumerableNullOrEmpty())
		{
			killCounter = Mathf.FloorToInt(Props.spawnKillCount.Evaluate(SelfPawn.ageTracker.AgeBiologicalYearsFloat));
		}
		if (!Props.spawnDamageTotal.EnumerableNullOrEmpty())
		{
			damageTotal = Props.spawnDamageTotal.Evaluate(SelfPawn.ageTracker.AgeBiologicalYearsFloat);
		}
		foreach (AbilityGainEntry item2 in Props.abilities.Where((AbilityGainEntry entry) => !entry.HasAbility(SelfPawn) && !entry.HasHediff(SelfPawn) && entry.ConditionsSatisfied(this)))
		{
			item2.GainAbility(SelfPawn);
		}
	}

	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		damageTotal += dinfo.Amount;
	}

	public override void Notify_KilledPawn(Pawn pawn)
	{
		killCounter++;
	}

	public override string CompInspectStringExtra()
	{
		return base.CompInspectStringExtra();
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref damageTotal, "damageTotal", 0f);
		Scribe_Values.Look(ref killCounter, "killCounter", 0);
	}

	public override void CompTickRare()
	{
		foreach (AbilityGainEntry item in Props.abilities.Where((AbilityGainEntry entry) => !entry.HasAbility(SelfPawn) && !entry.HasHediff(SelfPawn) && entry.ConditionsSatisfied(this)))
		{
			if (item.ShouldGainHediff)
			{
				if (item.ConditionsFulfilled(this))
				{
					item.GainAbility(SelfPawn);
				}
				else
				{
					item.GainHediff(SelfPawn);
				}
			}
			else
			{
				item.GainAbility(SelfPawn);
			}
		}
	}
}

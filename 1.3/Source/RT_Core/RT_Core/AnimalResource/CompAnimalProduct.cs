using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RT_Core;

public class CompAnimalProduct : CompHasGatherableBodyResource
{
	private System.Random rand = new();

	protected override int GatherResourcesIntervalDays => Props.gatheringIntervalDays;

	protected override int ResourceAmount => Props.resourceAmount;

	protected override string SaveKey => "resourceGrowth";

	public CompProperties_AnimalProduct Props => (CompProperties_AnimalProduct)props;

	protected override bool Active
	{
		get
		{
			if (!base.Active)
			{
				return false;
			}
			return parent is not Pawn pawn || pawn.ageTracker.CurLifeStage.shearable;
		}
	}

	protected override ThingDef ResourceDef => Props.resourceDef;

	public override string CompInspectStringExtra()
	{
		if (!Active)
		{
			return null;
		}
		if (!Props.customResourceString.NullOrEmpty())
		{
			return Props.customResourceString.Translate() + ": " + base.Fullness.ToStringPercent();
		}
		return "ResourceGrowth".Translate() + ": " + base.Fullness.ToStringPercent();
	}

	public void InformGathered(Pawn doer)
	{
		if (!Active)
		{
			Log.Error(doer?.ToString() + " gathered body resources while not Active: " + parent, ignoreStopLoggingLimit: false);
		}
		if (!Rand.Chance(StatExtension.GetStatValue((Thing)doer, StatDefOf.AnimalGatherYield, true)))
		{
			MoteMaker.ThrowText((doer.DrawPos + parent.DrawPos) / 2f, parent.Map, "TextMote_ProductWasted".Translate(), 3.65f);
		}
		else
		{
			int num = GenMath.RoundRandom((float)ResourceAmount * fullness);
			while (num > 0)
			{
				int num2 = Mathf.Clamp(num, 1, ResourceDef.stackLimit);
				num -= num2;
				Thing thing = ThingMaker.MakeThing(ResourceDef);
				thing.stackCount = num2;
				GenPlace.TryPlaceThing(thing, doer.Position, doer.Map, ThingPlaceMode.Near);
			}
			if (Props.hasAditional && rand.NextDouble() <= (double)((float)Props.additionalItemsProb / 100f))
			{
				Thing thing2 = ThingMaker.MakeThing(ThingDef.Named(Props.additionalItems.RandomElement()));
				thing2.stackCount = Props.additionalItemsNumber;
				GenPlace.TryPlaceThing(thing2, doer.Position, doer.Map, ThingPlaceMode.Near);
			}
		}
		fullness = 0f;
	}
}

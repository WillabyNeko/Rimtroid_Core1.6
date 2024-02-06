using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RT_Core;
using Verse;

namespace RT_Rimtroid;

public class CompEvolutionStage : ThingComp
{
	public static List<CompEvolutionStage> comps = new();

	public PawnKindDef pawnKindDefToEvolve;

	public int tickConversion;

	public List<HediffDef> hediffWhiteList;

	public Hediff evolutionSource;

	public int curEvolutionTryCount;

	public float nextEvolutionCheckYears;

	public CompProperties_EvolutionStage Props => props as CompProperties_EvolutionStage;

	public Pawn Metroid => parent as Pawn;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (!comps.Contains(this))
		{
			comps.Add(this);
		}
		if (!respawningAfterLoad && parent is Pawn)
		{
			HediffGiver();
		}
	}

	public void TransformPawn(PawnKindDef kindDef)
	{
		IntVec3 position = Metroid.Position;
		Faction faction = Metroid.Faction;
		Map map = Metroid.Map;
		RegionListersUpdater.DeregisterInRegions(Metroid, map);
		if (kindDef != null && kindDef != Metroid.kindDef)
		{
			Metroid.def = kindDef.race;
			Metroid.kindDef = kindDef;
			long ageBiologicalTicks = Metroid.ageTracker.AgeBiologicalTicks;
			long ageChronologicalTicks = Metroid.ageTracker.AgeChronologicalTicks;
			Metroid.ageTracker = new Pawn_AgeTracker(Metroid);
			Metroid.ageTracker.AgeBiologicalTicks = ageBiologicalTicks;
			Metroid.ageTracker.AgeChronologicalTicks = ageChronologicalTicks;
			if (Metroid.abilities?.abilities != null)
			{
				foreach (AbilityDef item in (from ability in Metroid.abilities.abilities.OfType<Ability_Base>()
					select ability.def).ToList())
				{
					Metroid.abilities.RemoveAbility(item);
				}
			}
			CompAbilityDefinition compAbilityDefinition = Metroid.TryGetComp<CompAbilityDefinition>();
			if (compAbilityDefinition != null)
			{
				Metroid.AllComps.Remove(compAbilityDefinition);
			}
			CompProperties compProperties = kindDef.race.CompDefFor<CompAbilityDefinition>();
			CompAbilityDefinition compAbilityDefinition2 = null;
			if (compProperties != null)
			{
				compAbilityDefinition2 = (CompAbilityDefinition)Activator.CreateInstance(compProperties.compClass);
				compAbilityDefinition2.parent = Metroid;
				Metroid.AllComps.Add(compAbilityDefinition2);
				compAbilityDefinition2.Initialize(compProperties);
			}
			if (compAbilityDefinition2 != null)
			{
				if (compAbilityDefinition != null)
				{
				}
				compAbilityDefinition2.CompTickRare();
			}
		}
		RegionListersUpdater.RegisterInRegions(Metroid, map);
		map.mapPawns.UpdateRegistryForPawn(Metroid);
		Metroid.Drawer.renderer.graphics.ResolveAllGraphics();
		if (!Metroid.health.hediffSet.hediffs.NullOrEmpty())
		{
			if (!hediffWhiteList.NullOrEmpty())
			{
				List<Hediff> hediffs = Metroid.health.hediffSet.hediffs;
				for (int num = hediffs.Count - 1; num >= 0; num--)
				{
					Hediff hediff = hediffs[num];
					if (!hediffWhiteList.Contains(hediff.def) && hediff != evolutionSource)
					{
						Metroid.health.RemoveHediff(hediff);
					}
				}
			}
			else
			{
				List<Hediff> hediffs2 = Metroid.health.hediffSet.hediffs;
				for (int num2 = hediffs2.Count - 1; num2 >= 0; num2--)
				{
					Hediff hediff2 = hediffs2[num2];
					if (hediff2 != evolutionSource)
					{
						Metroid.health.RemoveHediff(hediff2);
					}
				}
			}
		}
		parent.ExposeData();
		if (Metroid.Faction != faction)
		{
			Metroid.SetFaction(faction);
		}
		Metroid.needs.food.CurLevel = 1f;
		CompEvolutionStage compEvolutionStage = Metroid.TryGetComp<CompEvolutionStage>();
		if (compEvolutionStage != null)
		{
			Metroid.AllComps.Remove(compEvolutionStage);
		}
		CompProperties_EvolutionStage compProperties2 = kindDef.race.GetCompProperties<CompProperties_EvolutionStage>();
		CompEvolutionStage compEvolutionStage2 = null;
		if (compProperties2 != null)
		{
			compEvolutionStage2 = (CompEvolutionStage)Activator.CreateInstance(compProperties2.compClass);
			compEvolutionStage2.parent = Metroid;
			Metroid.AllComps.Add(compEvolutionStage2);
			compEvolutionStage2.Initialize(compProperties2);
			compEvolutionStage2.PostSpawnSetup(respawningAfterLoad: false);
		}
		pawnKindDefToEvolve = null;
	}

	public void HediffGiver()
	{
		if (Props.spawnStage != null)
		{
			Hediff hediff = Metroid.health.hediffSet.GetFirstHediffOfDef(Props.spawnStage);
			if (hediff == null)
			{
				BodyPartRecord partRecord = ((Props.partsToAffect != null) ? Enumerable.FirstOrDefault(Metroid.def.race.body.AllParts, (BodyPartRecord x) => x.def == Props.partsToAffect.RandomElement()) : null);
				hediff = HediffMaker.MakeHediff(Props.spawnStage, Metroid, partRecord);
			}
			Metroid.health.AddHediff(hediff);
		}
		if (Metroid.RaceProps.hediffGiverSets == null)
		{
			return;
		}
		foreach (HediffGiver item in Metroid.RaceProps.hediffGiverSets.SelectMany((HediffGiverSetDef set) => set.hediffGivers))
		{
			if (item is HediffGiver_AfterPeriod)
			{
				item.OnIntervalPassed(Metroid, null);
			}
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref curEvolutionTryCount, "curEvolutionTryCount", 0);
		Scribe_Values.Look(ref nextEvolutionCheckYears, "nextEvolutionCheckYears", 0f);
		Scribe_Defs.Look(ref pawnKindDefToEvolve, "pawnKindDefToEvolve");
		Scribe_Values.Look(ref tickConversion, "tickConversion", 0);
		Scribe_Collections.Look(ref hediffWhiteList, "hediffWhiteList", LookMode.Def);
		Scribe_References.Look(ref evolutionSource, "evolutionSource");
	}
}

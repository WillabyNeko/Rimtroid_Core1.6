﻿using Verse;
using RimWorld;
using System.Collections.Generic;
using RT_Rimtroid;
using System.Linq;

namespace RT_Core
{
    public class EvolutionPath
    {
        public float requiredAge;
        public HediffDef hediff;
        public List<BodyPartDef> partsToAffect;
        public List<HediffDef> hediffWhiteList;
        public PawnKindDef pawnKindDefToEvolve;
        public IntRange? ticksToConvert;
        public float weight;
    }
    public class HediffGiver_AfterPeriod : HediffGiver
    {
        private List<HediffDef> hediffsToPreventGrowth; //Hediff which if it exists, the pawn shouldn't transform.
        public float chance;
        public FloatRange yearsInterval;
        public int maxReroll;
        public List<EvolutionPath> possibleEvolutionPaths;
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            //If the pawn has the 'stunted' hediff.
            if (pawn.health.hediffSet.hediffs.Any(x => hediffsToPreventGrowth.Contains(x.def)))
            {
                //Don't continue executing this hediffgiver, just skip this pawn for now.
                return;
            }

            var comp = pawn.TryGetComp<CompEvolutionStage>();
            if (comp.nextEvolutionCheckYears > 0 && pawn.ageTracker.AgeBiologicalYearsFloat < comp.nextEvolutionCheckYears)
            {
                return;
            }
            if (maxReroll > 0 && maxReroll > comp.curEvolutionTryCount)
            {
                return;
            }
            if (chance > 0 && !Rand.Chance(chance))
            {
                comp.nextEvolutionCheckYears = pawn.ageTracker.AgeBiologicalYearsFloat + yearsInterval.RandomInRange;
                comp.curEvolutionTryCount++;
                return;
            }

            if (TryPerformMutation(pawn, comp))
            {
                comp.curEvolutionTryCount = 0;
            }
        }

        private bool TryPerformMutation(Pawn pawn, CompEvolutionStage comp)
        {
            var availableOptions = this.possibleEvolutionPaths.Where(x => pawn.ageTracker.AgeBiologicalYearsFloat >= x.requiredAge);
            if (availableOptions.TryRandomElementByWeight(x => x.weight, out var result))
            {
                foreach (var otherHediffDef in availableOptions.Select(x => x.hediff))
                {
                    if (!result.hediffWhiteList?.Contains(otherHediffDef) ?? false)
                    {
                        var otherHediff = pawn.health.hediffSet.GetFirstHediffOfDef(otherHediffDef);
                        if (otherHediff != null)
                        {
                            pawn.health.RemoveHediff(otherHediff);
                        }
                    }
                }

                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(result.hediff);
                if (hediff == null)
                {
                    var part = partsToAffect != null ? pawn.def.race.body.AllParts.FirstOrDefault(x => x.def == partsToAffect.RandomElement()) : null;
                    hediff = HediffMaker.MakeHediff(result.hediff, pawn, part);
                }
                comp.pawnKindDefToConvert = result.pawnKindDefToEvolve;
                comp.hediffWhiteList = result.hediffWhiteList;
                var ticksToConvert = result.ticksToConvert.HasValue ? result.ticksToConvert.Value.RandomInRange : 0;
                comp.tickConversion = Find.TickManager.TicksGame + ticksToConvert;
                pawn.health.AddHediff(hediff);

                return true;
            }
            return false;
        }
    }
}
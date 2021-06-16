using Verse;
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

            var comp = pawn.TryGetComp<CompEvolutionTime>();
            if (maxReroll > 0 && maxReroll > comp.curEvolutionTryCount)
            {
                return;
            }
            if (comp.nextEvolutionCheckTick == 0)
            {
                comp.nextEvolutionCheckTick = (int)(GenDate.TicksPerYear * yearsInterval.RandomInRange);
            }
            else if (Find.TickManager.TicksGame > comp.nextEvolutionCheckTick)
            {
                if (chance > 0 && !Rand.Chance(chance))
                {
                    comp.curEvolutionTryCount++;
                }
                else
                {
                    var availableOptions = this.possibleEvolutionPaths.Where(x => pawn.ageTracker.AgeBiologicalYearsFloat >= x.requiredAge);
                    if (availableOptions.TryRandomElementByWeight(x => x.weight, out var result))
                    {
                        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(result.hediff);
                        if (hediff == null)
                        {
                            var part = partsToAffect != null ? pawn.def.race.body.AllParts.FirstOrDefault(x => x.def == partsToAffect.RandomElement()) : null;
                            hediff = HediffMaker.MakeHediff(result.hediff, pawn, part);
                        }
                        comp.curEvolutionTryCount = 0;
                    }
                }
                comp.nextEvolutionCheckTick = (int)(GenDate.TicksPerYear * yearsInterval.RandomInRange);
            }
        }
    }
}
using System;
using System.Linq;
using RimWorld;
using RT_Core;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RT_Rimtroid
{
	public class CompEvolutionStage : ThingComp
	{
		public CompProperties_EvolutionStage Props => base.props as CompProperties_EvolutionStage;

		public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
            {

				if (parent is Pawn pawn)
				{
					if (Props.spawnStage != null)
                    {
						Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.spawnStage);
						if (hediff == null)
						{
							var part = Props.partsToAffect != null ? pawn.def.race.body.AllParts.FirstOrDefault(x => x.def == Props.partsToAffect.RandomElement()) : null;
							hediff = HediffMaker.MakeHediff(Props.spawnStage, pawn, part);
						}
						pawn.health.AddHediff(hediff);
					}

					if (pawn.RaceProps.hediffGiverSets != null)
                    {
						foreach (var hediffGiver in pawn.RaceProps.hediffGiverSets.SelectMany((HediffGiverSetDef set) => set.hediffGivers))
						{
							if (hediffGiver is HediffGiver_AfterPeriod)
							{
								hediffGiver.OnIntervalPassed(pawn, null);
							}
						}
					}

				}
			}
        }

        public int curEvolutionTryCount;
		public float nextEvolutionCheckYears;
		public Pawn Metroid => this.parent as Pawn;

        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Values.Look(ref curEvolutionTryCount, "curEvolutionTryCount");
			Scribe_Values.Look(ref nextEvolutionCheckYears, "nextEvolutionCheckYears");
		}
    }
}
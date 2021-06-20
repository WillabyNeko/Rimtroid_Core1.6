using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RT_Rimtroid
{
	public class CompEvolutionTime : ThingComp
	{
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
            {
				if (parent is Pawn pawn && pawn.RaceProps.hediffGiverSets != null)
				{
					foreach (var hediffGiver in pawn.RaceProps.hediffGiverSets.SelectMany((HediffGiverSetDef set) => set.hediffGivers))
					{
						hediffGiver.TryApply(pawn);
					}
				}
			}
        }

        public int curEvolutionTryCount;
		public float nextEvolutionCheckYears;
		public CompProperties_EvolutionTime Props
		{
			get
			{
				return (CompProperties_EvolutionTime)this.props;
			}
		}
		public Pawn Metroid => this.parent as Pawn;

		public override string CompInspectStringExtra()
		{
			return "RT_TimeToEvolution".Translate((int)(Props.timeInYears - Metroid.ageTracker.AgeBiologicalYears));
		}

        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Values.Look(ref curEvolutionTryCount, "curEvolutionTryCount");
			Scribe_Values.Look(ref nextEvolutionCheckYears, "nextEvolutionCheckYears");
		}
    }
}
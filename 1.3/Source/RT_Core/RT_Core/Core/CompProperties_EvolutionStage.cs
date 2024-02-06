using RimWorld;
using Verse;
using System;
using System.Collections.Generic;

namespace RT_Rimtroid
{
	public class CompProperties_EvolutionStage : CompProperties
	{
		public HediffDef spawnStage;
		public List<BodyPartDef> partsToAffect;
		public CompProperties_EvolutionStage()
		{
			this.compClass = typeof(CompEvolutionStage);
		}
	}
}

using System.Collections.Generic;
using Verse;

namespace RT_Rimtroid;

public class CompProperties_EvolutionStage : CompProperties
{
	public HediffDef spawnStage;

	public List<BodyPartDef> partsToAffect;

	public CompProperties_EvolutionStage()
	{
		compClass = typeof(CompEvolutionStage);
	}
}

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RT_Core;

public class AbilityCompProperties_CastVerb : CompProperties_AbilityEffect
{
	public List<VerbProperties> verbProperties = new();

	public List<Tool> tools;
}

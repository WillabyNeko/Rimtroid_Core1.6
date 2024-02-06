using System.Collections.Generic;
using Verse;

namespace RT_Core;

public class CompProperties_AbilityDefinition : CompProperties
{
	public SimpleCurve spawnKillCount;

	public SimpleCurve spawnDamageTotal;

	public List<AbilityGainEntry> abilities;
}

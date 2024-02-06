using RimWorld;
using Verse;

namespace RT_Core;

public class AbilityCompProperties_Cooldown : CompProperties_AbilityEffect
{
	public string cooldownPool;

	public IntRange cooldownTicksRange;

	public bool independent = false;

	public bool resetsTimer = true;
}

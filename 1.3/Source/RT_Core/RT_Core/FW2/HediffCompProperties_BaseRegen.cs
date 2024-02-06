using Verse;

namespace RT_Core;

public abstract class HediffCompProperties_BaseRegen : HediffCompProperties
{
	public virtual bool ShouldKeepWhileFighting(HediffDef def)
	{
		if (def.HasModExtension<RegenHediffModExtension>())
		{
			return def.GetModExtension<RegenHediffModExtension>().keepWhileFighting;
		}
		return false;
	}
}

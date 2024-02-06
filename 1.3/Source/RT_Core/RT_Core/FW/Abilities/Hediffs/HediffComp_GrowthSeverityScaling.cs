using RimWorld;
using Verse;

namespace RT_Core;

public class HediffComp_GrowthSeverityScaling : HediffComp
{
	public int ticks;

	public AbilityDef abilityDef;

	public HediffCompProperties_GrowthSeverityScaling Props => (HediffCompProperties_GrowthSeverityScaling)props;

	public override bool CompShouldRemove => !SeverityInRange || parent.pawn.abilities == null || (abilityDef != null && parent.pawn.abilities.GetAbility(abilityDef) != null);

	public bool SeverityInRange => Props.severityRange.TrueMin < parent.Severity && parent.Severity < Props.severityRange.TrueMax;

	public override string CompLabelInBracketsExtra => base.CompLabelInBracketsExtra + "(" + abilityDef?.ToString() + " " + (Props.GetTicksAt(Props.severityRange.TrueMax) - ticks).ToStringTicksToPeriodVague(vagueMin: false) + ")";

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref ticks, "ticks", 0);
		Scribe_Defs.Look(ref abilityDef, "abilityDef");
	}

	public override void CompPostPostRemoved()
	{
		base.CompPostPostRemoved();
		if (abilityDef != null && !SeverityInRange)
		{
			parent.pawn.abilities.GainAbility(abilityDef);
			Messages.Message("AbilityGainHediffMessage".Translate(parent.pawn.Named("PAWN"), abilityDef.LabelCap.Named("ABILITY")), parent.pawn, MessageTypeDefOf.PositiveEvent);
		}
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		ticks++;
		severityAdjustment = Props.GetSeverityAt(ticks) - parent.Severity;
	}

	public override string CompDebugString()
	{
		return "ticks=" + ticks + "/" + Props.GetTicksAt(Props.severityRange.TrueMax) + ((abilityDef != null) ? ("\nApply on Completion: " + abilityDef.defName) : "");
	}
}

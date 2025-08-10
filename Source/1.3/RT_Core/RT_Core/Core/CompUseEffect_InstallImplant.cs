using Verse;
using RimWorld;

namespace RT_Core
{
    public class CompApplyHediff_InstallImplant : CompUseEffect
    {
        public CompProperties_ApplyHediff_UseEffect Props => (CompProperties_ApplyHediff_UseEffect)props;

        public override void DoEffect(Pawn user)
        {
            Hediff firstHediffOfDef = user.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
            user.health.AddHediff(Props.hediffDef, null);
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            if ((!p.IsFreeColonist || p.HasExtraHomeFaction()) && !Props.allowNonColonists)
            {
                failReason = "InstallImplantNotAllowedForNonColonists".Translate();
                return false;
            }
            failReason = null;
            return true;
        }
    }
}
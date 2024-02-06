using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace RT_Core
{
    [StaticConstructorOnStartup]
    public static class Utils
    {
        public static void MakeFlee(Pawn pawn, Thing danger, int radius, List<Thing> dangers)
        {
            Job job = null;
            IntVec3 intVec;
            if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Flee)
            {
                intVec = pawn.CurJob.targetA.Cell;
            }
            else
            {
                intVec = CellFinderLoose.GetFleeDest(pawn, dangers, 24f);
            }

            if (intVec == pawn.Position)
            {
                intVec = GenRadial.RadialCellsAround(pawn.Position, radius <= 50 ? radius : 50, radius * 2 <= 50 ? radius * 2 : 50).RandomElement();
            }
            if (intVec != pawn.Position)
            {
                job = JobMaker.MakeJob(JobDefOf.Flee, intVec, danger);
            }
            if (job != null)
            {
                //Log.Message(pawn + " flee");
                pawn.jobs.TryTakeOrderedJob(job);
            }
        }
    }
    public class CompProperties_ApplyHediff_UseEffect : CompProperties_Usable
    {
        public HediffDef hediffDef;
        public bool allowNonColonists;

        public CompProperties_ApplyHediff_UseEffect()
        {
            compClass = typeof(CompApplyHediff_InstallImplant);
        }
    }
}

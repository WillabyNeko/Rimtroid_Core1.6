using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Verse.AI.Group;
using System;

namespace RT_Core
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.drazzii.rimworld.mod.RimtroidCore");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            DefDatabase<ThingDef>.GetNamed("RT_BanteeMetroid").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_MetroidLarvae").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_AlphaMetroid").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_GammaMetroid").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_ZetaMetroid").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_OmegaMetroid").race.predator = true;
            DefDatabase<ThingDef>.GetNamed("RT_QueenMetroid").race.predator = true;
        }
    }

    public class RT_DesiccatorExt : DefModExtension
    {
        public ThingDef RT_DesiccatedDef;
    }

    [HarmonyPatch(typeof(Pawn_JobTracker))]
    [HarmonyPatch("StartJob")]
    public static class StartJob_Patch
    {
        private static bool Prefix(Pawn ___pawn, Job newJob, JobCondition lastJobEndCondition)
        {
            if (newJob.def == JobDefOf.Vomit && ___pawn.IsAnyMetroid())
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Hediff), "Severity", MethodType.Setter)]
    public static class Severity_Patch
    {
        private static void Prefix(Hediff __instance, ref float value)
        {
            if (__instance.def == HediffDefOf.Malnutrition && (__instance.pawn?.IsAnyMetroid() ?? false) && value < 0)
            {
                value *= 3;
            }
        }
    }

    //[HarmonyPatch(typeof(Pawn), "Kill")]
    //public static class RT_Desiccator_Pawn_Kill_Patch
    //{
    //[HarmonyPostfix]
    //public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
    //{
    //if (dinfo.HasValue)
    //{
    //if (dinfo.Value.Instigator != null)
    //{
    //Thing inst = dinfo.Value.Instigator;
    //RT_DesiccatorExt desiccator = inst.def.GetModExtension<RT_DesiccatorExt>();
    //if (desiccator != null)
    //{
    //if (desiccator.RT_DesiccatedDef != null)
    //{
    //FieldInfo corpse = typeof(Pawn).GetField("Corpse", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
    //Traverse.Create(__instance);
    //corpse.SetValue(__instance, ThingMaker.MakeThing(desiccator.RT_DesiccatedDef));
    //}
    //else
    //{
    //CompRottable compRottable = __instance.Corpse.TryGetComp<CompRottable>();
    //compRottable.RotImmediately();
    // }
    //}
    //}
    //}
    //HediffDef def = DefDatabase<HediffDef>.GetNamed("RT_LifeDrainSickness");
    //if (__instance.health.hediffSet.HasHediff(def))
    //{
    //CompRottable compRottable = __instance.Corpse.TryGetComp<CompRottable>();
    //compRottable.RotImmediately();
    //}
    //if (__instance.Corpse.GetRotStage() == RotStage.Fresh)
    //{
    //Log.Message(__instance + " failed rot");
    //}
    /*
    else
    {
        Log.Message(__instance + " rotted by");
    }
    */
    //}
    //}
}
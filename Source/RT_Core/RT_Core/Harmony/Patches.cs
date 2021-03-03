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

    [HarmonyPatch(typeof(Need_Food), "NeedInterval")]
    public static class RT_NeedInterval_Patch
    {
        public static float GetBerserkChance(float curFoodLevel, Dictionary<float, float> hungerValues)
        {
            var keys = hungerValues.Keys.OrderByDescending(x => x);
            float result = 0;
            foreach (var key in keys)
            {
                if (key >= curFoodLevel)
                {
                    result = hungerValues[key];
                }
            }
            return result;
        }

        public static bool Prefix(Need_Food __instance, Pawn ___pawn)
        {
            if (___pawn.IsAnyMetroid() && ___pawn.CurJobDef == JobDefOf.LayDown)
            {
                return false;
            }
            return true;
        }
        public static void Postfix(Need_Food __instance, Pawn ___pawn)
        {
            var options = ___pawn.kindDef.GetModExtension<HungerBerserkOptions>();
            if (options != null)
            {
                var berserkChance = GetBerserkChance(__instance.CurLevelPercentage, options.hungerBerserkChanges);
                //Log.Message(___pawn + " has " + berserkChance + " berserk chance, cur food level: " + __instance.CurLevelPercentage, true);
                if (berserkChance > 0)
                {
                    if (!___pawn.InMentalState && Rand.Chance(berserkChance))
                    {
                        if (___pawn.CurJobDef != JobDefOf.LayDown && ___pawn.CurJobDef != RT_DefOf.RT_EatFromStation && ___pawn.CurJobDef != RT_DefOf.RT_AbsorbingEnergy && !InCombat(___pawn))
                        {
                            //Log.Message(___pawn + " gets berserk state", true);
                            if (___pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, forceWake: true))
                            {
                                if (___pawn.Faction == Faction.OfPlayer && Rand.Chance(options.chanceToBecomeWildIfBerserkAndTamed))
                                {
                                    ___pawn.SetFaction(null);
                                }
                            }
                        }
                    }
                }
                else if (___pawn.mindState.mentalStateHandler.CurStateDef == MentalStateDefOf.Berserk)
                {
                    //Log.Message(___pawn + " recovers from berserk state", true);
                    ___pawn.MentalState.RecoverFromState();
                }
            }
        }

        public static HashSet<JobDef> combatJobs = new HashSet<JobDef>
                                                    {
                                                        JobDefOf.AttackMelee,
                                                        JobDefOf.AttackStatic,
                                                        JobDefOf.FleeAndCower,
                                                        JobDefOf.ManTurret,
                                                        JobDefOf.Wait_Combat,
                                                        JobDefOf.Flee
                                                    };
        private static bool InCombat(Pawn pawn)
        {
            if (combatJobs.Contains(pawn.CurJobDef))
            {
                return true;
            }
            else if (pawn.mindState.duty?.def.alwaysShowWeapon ?? false)
            {
                return true;
            }
            else if (pawn.CurJobDef?.alwaysShowWeapon ?? false)
            {
                return true;
            }
            else if (pawn.mindState.lastEngageTargetTick > Find.TickManager.TicksGame - 1000)
            {
                return true;
            }
            else if (pawn.mindState.lastAttackTargetTick > Find.TickManager.TicksGame - 1000)
            {
                return true;
            }
            return false;
        }
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


    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", new Type[]
        {
            typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult)
        })]
    public static class AddHediff_Patch
    {
        private static HashSet<HediffDef> hediffDefs = new HashSet<HediffDef>
        {
            HediffDef.Named("SandInEyes"),
            HediffDef.Named("DirtInEyes"),
            HediffDef.Named("MudInEyes"),
            HediffDef.Named("GravelInEyes"),
            HediffDef.Named("WaterInEyes")
        };
        private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
        {
            if (hediffDefs.Contains(hediff.def) && ___pawn.IsAnyMetroid())
            {
                return false;
            }
            return true;
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
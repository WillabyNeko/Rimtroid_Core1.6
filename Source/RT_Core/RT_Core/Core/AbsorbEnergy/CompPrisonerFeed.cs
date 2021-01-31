using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using HarmonyLib;

namespace RT_Core
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_GetGizmos_Patch
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
        {
            if (__instance.IsPrisoner && __instance.Map.mapPawns.AllPawns.Any(x => x.IsAnyMetroid() && x.def.HasModExtension<RT_EnergyDrain>()))
            {
                List<Gizmo> list = __result.ToList<Gizmo>();
                var comp = __instance.GetComp<CompPrisonerFeed>();
                Command_Toggle command_Toggle = new Command_Toggle();
                command_Toggle.defaultLabel = "RT.PrisonersForEatingLabel".Translate();
                command_Toggle.defaultDesc = "RT.PrisonersForEatingDesc".Translate();
                command_Toggle.icon = ContentFinder<Texture2D>.Get("UI/Commands/TogglePrisoners");
                command_Toggle.isActive = (() => comp.canBeEaten);
                command_Toggle.toggleAction = delegate
                {
                    comp.canBeEaten = !comp.canBeEaten;
                };
                command_Toggle.hotKey = KeyBindingDefOf.Misc3;
                command_Toggle.turnOffSound = null;
                command_Toggle.turnOnSound = null;
                list.Add(command_Toggle);
                __result = list;
            }
        }
    }

    public class CompProperties_PrisonerFeed : CompProperties
    {
        public CompProperties_PrisonerFeed()
        {
            this.compClass = typeof(CompPrisonerFeed);
        }
    }
    public class CompPrisonerFeed : ThingComp
    {
        public CompProperties_LatchedMetroid Props => (CompProperties_LatchedMetroid)props;
        public bool canBeEaten;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref canBeEaten, "canBeEaten");
        }
    }
}
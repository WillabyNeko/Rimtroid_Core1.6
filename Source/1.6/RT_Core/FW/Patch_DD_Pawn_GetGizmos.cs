using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
/*
namespace RT_Core
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class DD_Pawn_GetGizmos
    {
        public static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if(__instance.Faction == null || !__instance.Faction.IsPlayer)
            {
                return;
            }

            if(__instance.InMentalState)
            {
                return;
            }

            if (__result.EnumerableNullOrEmpty())
            {
                __result = new List<Gizmo>();
            }

            if (__instance.abilities != null)
            {
                IEnumerable<Ability> abilities = __instance.abilities.abilities.OfType<Ability>().Where(ability => ability.CanShowGizmos);
                if (!abilities.EnumerableNullOrEmpty())
                {
                    foreach (Ability ability in abilities)
                    {
                        IEnumerable<Command> commands = ability.GetGizmos();
                        if(!commands.EnumerableNullOrEmpty())
                        {
                            __result = __result.Where(gizmo => !commands.Any(cmd => gizmo == cmd)).Concat(commands);
                        }
                    }
                }
            }

            CompHostileResponse comp = __instance.GetComp<CompHostileResponse>();
            if (comp != null)
            {
                IEnumerable<Gizmo> gizmos = comp.CompGetGizmosExtra();
                if(!gizmos.EnumerableNullOrEmpty())
                {
                    __result = __result.Where(gizmo => !gizmos.Any(g => g == gizmo)).Concat(gizmos);
                }
            }
        }
    }
}*/

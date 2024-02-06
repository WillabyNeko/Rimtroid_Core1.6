using HarmonyLib;
using Verse;
using Verse.AI;

namespace RT_Core;

[HarmonyPatch(typeof(Pawn_MindState), "CanStartFleeingBecauseOfPawnAction")]
public class Patch_DD_Pawn_MindState_CanStartFleeingBecauseOfPawnAction
{
	public static void Postfix(Pawn p, ref bool __result)
	{
		CompHostileResponse comp = p.GetComp<CompHostileResponse>();
		if (__result && comp != null && comp.Type != HostilityResponseType.Passive)
		{
			__result = !comp.Targets.EnumerableNullOrEmpty();
		}
	}
}

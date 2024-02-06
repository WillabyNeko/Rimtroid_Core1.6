using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace RT_Core;

public static class FoodMethod
{
	public static Pawn FindPawnTarget(this Pawn pawn, float distance)
	{
		Pawn pawn2 = null;
		return (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingFrame), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors), distance, Predicate);
		bool Predicate(Thing p)
		{
			return p != null && p != pawn && p.def != pawn.def && p is Pawn { Downed: false } pawn3 && pawn.CanReserve(p) && FoodUtility.IsAcceptablePreyFor(pawn, pawn3);
		}
	}

	public static Thing FindTarget(this Pawn pawn, float distance, Predicate<Thing> validator, ThingRequestGroup request)
	{
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(request), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors), distance, validator);
	}
}

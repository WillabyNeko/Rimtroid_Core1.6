using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace RT_Core
{
	public class JobGiver_GetLiveFood : ThinkNode_JobGiver
	{
		protected HungerCategory minCategory;

		private float maxLevelPercentage = 1f;

		public bool forceScanWholeMap;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GetLiveFood obj = (JobGiver_GetLiveFood)base.DeepCopy(resolve);
			obj.minCategory = minCategory;
			obj.maxLevelPercentage = maxLevelPercentage;
			obj.forceScanWholeMap = forceScanWholeMap;

			return obj;
		}

		public override float GetPriority(Pawn pawn)
		{
			Need_Food food = pawn.needs.food;
			if (food == null)
			{
				return 0f;
			}
			if ((int)pawn.needs.food.CurCategory < 3 && FoodUtility.ShouldBeFedBySomeone(pawn))
			{
				return 0f;
			}
			if ((int)food.CurCategory < (int)minCategory)
			{
				return 0f;
			}
			if (food.CurLevelPercentage > maxLevelPercentage)
			{
				return 0f;
			}
			if (food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat)
			{
				return 9.5f;
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{

			Need_Food food = pawn.needs.food;
			if (food == null || (int)food.CurCategory < (int)minCategory || food.CurLevelPercentage > maxLevelPercentage)
			{
				return null;
			}
			if (RT_DefOf.RT_EatFromStation.GetModExtension< MetroidFeedingStationOptions>().options.Where(x => x.defName == pawn.def.defName).Any())
            {
				var feedingStations = pawn.Map.listerThings.AllThings.Where(x => x.def == RT_DefOf.RT_FeedingStation && pawn.CanReserveAndReach(x, PathEndMode.InteractionCell, Danger.Deadly))
						.OrderBy(x => x.Position.DistanceTo(pawn.Position));
				if (feedingStations.Any())
				{
					Job job = JobMaker.MakeJob(RT_DefOf.RT_EatFromStation, feedingStations.First());
					Log.Message(pawn + " gets " + job);
					return job;
				}
			}
			Thing foodSource = FoodMethod.FindPawnTarget(pawn, 50f);
			Pawn pawn2 = foodSource as Pawn;
			if (pawn2 != null)
			{
				Log.Message("Metroid is hunting.");
				Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, pawn2);
				job.killIncappedTarget = false;
				return job;
			}
			return null;
		}
	}
}

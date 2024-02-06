using RimWorld;
using Verse;
using Verse.AI;

namespace RT_Core;

public class JobGiver_HostilityResponse : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.DestroyedOrNull())
		{
			return null;
		}
		CompHostileResponse comp = pawn.GetComp<CompHostileResponse>();
		if (comp == null)
		{
			return null;
		}
		if (comp.Type == HostilityResponseType.Passive)
		{
			return null;
		}
		if (!pawn.Awake() || pawn.IsBurning() || pawn.Drafted || pawn.Downed || pawn.Dead)
		{
			return null;
		}
		if (pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			return null;
		}
		if (pawn.jobs.startingNewJob)
		{
			return null;
		}
		if (pawn.IsFighting() || pawn.stances.FullBodyBusy)
		{
			return null;
		}
		if (!PawnUtility.EnemiesAreNearby(pawn, 9, false))
		{
			return null;
		}
		if (PawnUtility.PlayerForcedJobNowOrSoon(pawn) || !pawn.jobs.IsCurrentJobPlayerInterruptible())
		{
			return null;
		}
		IAttackTarget preferredTarget = comp.PreferredTarget;
		if (preferredTarget == null || preferredTarget.Thing == null)
		{
			return null;
		}
		Thing thing = preferredTarget.Thing;
		Verb verb = pawn.TryGetAttackVerb(thing, false);
		if (verb == null)
		{
			return null;
		}
		Job job;
		if (verb.IsMeleeAttack)
		{
			job = JobMaker.MakeJob(JobDefOf.AttackMelee, thing);
			job.maxNumMeleeAttacks = 1;
		}
		else
		{
			job = JobMaker.MakeJob(JobDefOf.AttackStatic, thing);
			job.maxNumStaticAttacks = 1;
			job.endIfCantShootInMelee = verb.verbProps.EffectiveMinRange(thing, pawn) > 1f;
		}
		job.verbToUse = verb;
		job.expireRequiresEnemiesNearby = true;
		job.killIncappedTarget = comp.Type == HostilityResponseType.Aggressive;
		job.playerForced = true;
		job.expiryInterval = 2000;
		return job;
	}
}

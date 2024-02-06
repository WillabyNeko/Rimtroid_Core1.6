using RimWorld;
using Verse;
using Verse.AI;

namespace RT_Core;

public class JobGiver_Manhunter_AnyVerb : JobGiver_Manhunter
{
	public float castRangeFactor = 0.75f;

	public float castCoverRange = 5f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		Verb currentEffectiveVerb = pawn.CurrentEffectiveVerb;
		if (currentEffectiveVerb != null && !currentEffectiveVerb.IsMeleeAttack && currentEffectiveVerb.Available())
		{
			Pawn pawn2 = AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, (Thing x) => x is Pawn && (int)x.def.race.intelligence >= 1, 0f, 9999f, default(IntVec3), float.MaxValue, canBashDoors: true, canTakeTargetsCloserThanEffectiveMinRange: true, canBashFences: true) as Pawn;
			CastPositionRequest castPositionRequest = default(CastPositionRequest);
			castPositionRequest.caster = pawn;
			castPositionRequest.target = pawn2;
			castPositionRequest.verb = currentEffectiveVerb;
			castPositionRequest.wantCoverFromTarget = currentEffectiveVerb.verbProps.range > castCoverRange;
			castPositionRequest.maxRangeFromCaster = currentEffectiveVerb.verbProps.range * castRangeFactor;
			castPositionRequest.maxRangeFromTarget = currentEffectiveVerb.verbProps.range * castRangeFactor;
			CastPositionRequest newReq = castPositionRequest;
			if (pawn2 != null)
			{
				if (currentEffectiveVerb.CanHitTargetFrom(pawn.Position, pawn2))
				{
					Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, pawn2);
					job.maxNumStaticAttacks = 1;
					job.expiryInterval = Rand.Range(100, 1000);
					job.endIfCantShootInMelee = true;
					job.ignoreForbidden = true;
					job.killIncappedTarget = true;
					job.canUseRangedWeapon = true;
					job.verbToUse = pawn.TryGetAttackVerb((Thing)pawn2, true) ?? currentEffectiveVerb;
					return job;
				}
				if (CastPositionFinder.TryFindCastPosition(newReq, out var dest))
				{
					Job job2 = JobMaker.MakeJob(JobDefOf.Goto, dest);
					job2.locomotionUrgency = LocomotionUrgency.Sprint;
					return job2;
				}
			}
		}
		return base.TryGiveJob(pawn);
	}
}

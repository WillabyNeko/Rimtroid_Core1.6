using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RT_Core;

public class CompHostileResponse : ThingComp
{
	private Dictionary<string, HostilityStatisticRecord> stats = new();

	private HostilityResponseType type;

	private Dictionary<HostilityResponseType, Command_Action> gizmos = new();

	public CompProperties_HostileResponse Props => (CompProperties_HostileResponse)props;

	public IAttackTargetSearcher Self => (IAttackTargetSearcher)parent;

	public Pawn SelfPawn => (Pawn)parent;

	public HostilityResponseType Type
	{
		get
		{
			if (parent is Pawn pawn)
			{
				if (pawn.InAggroMentalState || pawn.Faction.HostileTo(Faction.OfPlayer))
				{
					return HostilityResponseType.Aggressive;
				}
				if (pawn.InMentalState)
				{
					return Props.initialHostility;
				}
			}
			return type;
		}
		set
		{
			type = value;
		}
	}

	public bool CanInteractGizmo => parent.Faction != null && parent.Faction.IsPlayer && parent is Pawn pawn && pawn.InMentalState;

	public Command_Action Gizmo
	{
		get
		{
			Command_Action command_Action = null;
			List<HostileResponseOption> options = Props.options;
			int index = options.FindIndex((HostileResponseOption o) => o.type == Type);
			HostileResponseOption hostileResponseOption = options[index];
			if (!gizmos.ContainsKey(Type))
			{
				command_Action = new Command_Action
				{
					defaultLabel = hostileResponseOption.label,
					defaultDesc = hostileResponseOption.description,
					icon = hostileResponseOption.Texture,
					action = delegate
					{
						Type = options[(index + 1) % options.Count].type;
					}
				};
				gizmos.Add(Type, command_Action);
			}
			else
			{
				command_Action = gizmos[Type];
			}
			if (CanInteractGizmo == !command_Action.disabled)
			{
				command_Action.disabled = false;
				if (!CanInteractGizmo)
				{
					command_Action.Disable(hostileResponseOption.disableMessage);
				}
			}
			return command_Action;
		}
	}

	public IEnumerable<IAttackTarget> Targets => stats.Values.Select((HostilityStatisticRecord r) => r.Target).Where(IsValidTarget);

	public IEnumerable<IAttackTarget> TargetsPreferredOrder => from entry in stats
		where IsValidTarget(entry.Value.Target)
		select entry.Value into record
		orderby record.CalculatePoints(Type) / Self.Thing.Position.DistanceTo(record.Target.Thing.Position) descending
		select record.Target;

	public IAttackTarget PreferredTarget
	{
		get
		{
			IAttackTarget result = null;
			CastPositionRequest request = new()
            {
				caster = SelfPawn
			};
			Pair<IAttackTarget, float> pair = TargetsPreferredOrder.Select((IAttackTarget t) => CalculateScore(t, request)).RandomElementByWeightWithFallback((Pair<IAttackTarget, float> p) => p.Second);
			if (pair.First != null)
			{
				result = pair.First;
			}
			else
			{
				IAttackTarget attackTarget = AttackTargetFinder.BestAttackTarget(SelfPawn, TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.LOSBlockableByGas, delegate(Thing thing)
				{
					switch (Type)
					{
					case HostilityResponseType.Aggressive:
						return true;
					case HostilityResponseType.Defensive:
					{
						IAttackTarget attackTarget2 = thing as IAttackTarget;
						return attackTarget2.TargetCurrentlyAimingAt == SelfPawn;
					}
					default:
						return false;
					}
				});
				if (attackTarget != null)
				{
					result = attackTarget;
				}
			}
			return result;
		}
	}

	private bool IsValidTarget(IAttackTarget target)
	{
		if (target == null)
		{
			return false;
		}
		if (target.Thing.DestroyedOrNull())
		{
			return false;
		}
		if (target.Thing.Map != parent.Map)
		{
			return false;
		}
		if (target.Thing is Pawn pawn)
		{
			if (pawn.Downed && Type != 0)
			{
				return false;
			}
			if (pawn.Dead)
			{
				return false;
			}
		}
		if (target.ThreatDisabled(Self))
		{
			return false;
		}
		return true;
	}

	private Pair<IAttackTarget, float> CalculateScore(IAttackTarget target, CastPositionRequest request = default(CastPositionRequest))
	{
		float num = float.NegativeInfinity;
		if (Self.Thing is Pawn pawn)
		{
			IEnumerable<Verb> possibleVerbs = VerbUtils.GetPossibleVerbs(pawn);
			request.target = target.Thing;
			foreach (Verb item in possibleVerbs)
			{
				Verb verb = (request.verb = item);
				if (!CastPositionFinder.TryFindCastPosition(request, out var dest))
				{
					continue;
				}
				float num2 = Self.Thing.Position.DistanceTo(dest);
				float num3 = 0f;
				if (num2 != 0f)
				{
					num3 = GetTargetPoints(target) / num2;
					num3 *= verb.CalculateCommonality(pawn, target.Thing);
					if (float.IsPositiveInfinity(num3))
					{
						num3 = float.MaxValue;
					}
				}
				else
				{
					num3 = float.MaxValue;
				}
				if (num < num3)
				{
					num = num3;
				}
			}
		}
		return new Pair<IAttackTarget, float>(target, Mathf.Clamp(num, 0f, float.MaxValue));
	}

	private float GetTargetPoints(IAttackTarget target)
	{
		if (!stats.Keys.Contains(target.Thing.ThingID))
		{
			return 0f;
		}
		HostilityStatisticRecord hostilityStatisticRecord = stats[target.Thing.ThingID];
		float num = Self.Thing.Position.DistanceTo(hostilityStatisticRecord.Target.Thing.Position);
		if (num != 0f)
		{
			return hostilityStatisticRecord.CalculatePoints(Type) / num;
		}
		return float.MaxValue;
	}

	public override void Initialize(CompProperties props)
	{
		base.Initialize(props);
		type = Props.initialHostility;
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref type, "responseType", HostilityResponseType.Passive);
		Scribe_Collections.Look(ref stats, "responseStats", LookMode.Value, LookMode.Deep);
		if (stats == null)
		{
			stats = new Dictionary<string, HostilityStatisticRecord>();
		}
	}

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		stats.Clear();
	}

	public override void PostDeSpawn(Map map)
	{
		stats.Clear();
	}

	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (dinfo.Amount <= 0f || dinfo.Instigator == null || !(dinfo.Instigator is IAttackTarget attackTarget) || attackTarget == null)
		{
			return;
		}
		if (dinfo.IntendedTarget == Self && Type != HostilityResponseType.Passive)
		{
			if (attackTarget.Thing.Faction != null && attackTarget.Thing.Faction == SelfPawn.Faction)
			{
				if (attackTarget.Thing is Pawn { InMentalState: not false })
				{
					return;
				}
				if (Props.friendlyFireMentalState != null)
				{
					SelfPawn.mindState.mentalStateHandler.TryStartMentalState(Props.friendlyFireMentalState, "HostileResponseFriendlyFireMessage".Translate(SelfPawn.Named("PAWN"), attackTarget.Named("ATTACKER")), forceWake: true);
				}
			}
			ProcessDamage(attackTarget, dinfo);
		}
		else if (attackTarget.Thing.HostileTo(SelfPawn))
		{
			ProcessDamage(attackTarget, dinfo);
		}
	}

	private void ProcessDamage(IAttackTarget target, DamageInfo dinfo)
	{
		if (!stats.ContainsKey(target.Thing.ThingID))
		{
			stats.Add(target.Thing.ThingID, new HostilityStatisticRecord(target));
		}
		stats[target.Thing.ThingID].ProcessAttack(dinfo.Amount, dinfo.IntendedTarget == null || parent == dinfo.IntendedTarget);
	}

	public override void Notify_KilledPawn(Pawn pawn)
	{
		stats.Remove(pawn.ThingID);
	}

	public override string CompInspectStringExtra()
	{
		if (Props.debug)
		{
			int num = Targets.Count();
			if (num > 0)
			{
				return Type switch
				{
					HostilityResponseType.Aggressive => "Hostility: Aggressive [" + num + "]", 
					HostilityResponseType.Defensive => "Hostility: Defensive [" + num + "]", 
					HostilityResponseType.Passive => "Hostility: Passive [" + num + "]", 
					_ => "Hostility: Unknown [" + num + "]", 
				};
			}
		}
		return base.CompInspectStringExtra();
	}

	public override void CompTickRare()
	{
		if (stats.RemoveAll((KeyValuePair<string, HostilityStatisticRecord> entry) => entry.Value.Target == null || entry.Value.Target.Thing.DestroyedOrNull() || SelfPawn.Map != entry.Value.Target.Thing.Map) > 0 && (SelfPawn.CurJobDef == JobDefOf.AttackMelee || SelfPawn.CurJobDef == JobDefOf.AttackStatic))
		{
			IAttackTarget preferredTarget = PreferredTarget;
			if (preferredTarget != null)
			{
				SelfPawn.CurJob.targetA = preferredTarget.Thing;
			}
			else
			{
				SelfPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (Gizmo != null && Props.controllableGizmo)
		{
			yield return Gizmo;
		}
	}
}

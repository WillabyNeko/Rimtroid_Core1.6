using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RT_Core;

public class AbilityComp_CastVerb : AbilityComp_Base, IVerbOwner
{
	private VerbTracker tracker;

	private LocalTargetInfo target = LocalTargetInfo.Invalid;

	private LocalTargetInfo dest = LocalTargetInfo.Invalid;

	public AbilityCompProperties_CastVerb VProps => props as AbilityCompProperties_CastVerb;

	public VerbTracker VerbTracker
	{
		get
		{
			if (tracker == null)
			{
				tracker = new VerbTracker(this);
			}
			return tracker;
		}
	}

	public List<VerbProperties> VerbProperties => VProps.verbProperties;

	public List<Tool> Tools => VProps.tools;

	public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

	public Thing ConstantCaster => parent.pawn;

	public override bool CanCast => VerbTracker.AllVerbs.All((Verb v) => v.Available());

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		if (VerbTracker.AllVerbs.Any((Verb v) => v.Available() && v.IsUsableOn(target.Thing)))
		{
			this.target = target;
			this.dest = dest;
		}
		else
		{
			Log.Error(UniqueVerbOwnerID() + ": Activated with no verbs available");
		}
	}

	public string UniqueVerbOwnerID()
	{
		return "AbilityComp_" + parent.UniqueVerbOwnerID() + "_Verbs";
	}

	public bool VerbsStillUsableBy(Pawn p)
	{
		return !VerbTracker.AllVerbs.NullOrEmpty() && VerbTracker.AllVerbs.Any((Verb verb) => verb.Available());
	}

	public override bool CanActivateOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		return !VerbTracker.AllVerbs.NullOrEmpty() && VerbTracker.AllVerbs.Any((Verb v) => v.Available() && v.IsUsableOn(target.Thing));
	}

	public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		return base.CanApplyOn(target, dest) && CanActivateOn(target, dest);
	}

	public override void PostTick()
	{
		VerbTracker.VerbsTick();
		if (target.IsValid || dest.IsValid)
		{
			Verb verb = (from v in VerbTracker.AllVerbs
				where v.Available() && v.IsUsableOn(target.Thing)
				orderby v.verbProps.commonality descending
				select v).FirstOrFallback();
			if (verb != null)
			{
				Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStatic, target);
				job.verbToUse = verb;
				job.expiryInterval = 2000;
				parent.pawn.jobs.StartJob(job, JobCondition.InterruptForced, (ThinkNode)null, false, true, (ThinkTreeDef)null, (JobTag?)null, false, false);
			}
			else
			{
				parent.StartCooldown(0);
			}
			target = LocalTargetInfo.Invalid;
			dest = LocalTargetInfo.Invalid;
		}
	}

	public override void PostExposeData()
	{
		Scribe_Deep.Look(ref tracker, "castVerbs", this);
		Scribe_TargetInfo.Look(ref target, "currentTarget", LocalTargetInfo.Invalid);
		Scribe_TargetInfo.Look(ref dest, "currentDestination", LocalTargetInfo.Invalid);
	}
}

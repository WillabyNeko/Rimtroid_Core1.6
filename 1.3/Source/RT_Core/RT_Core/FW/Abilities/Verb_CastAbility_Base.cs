using RimWorld;
using Verse;

namespace RT_Core;

public class Verb_CastAbility_Base : Verb_CastAbility
{
	public override bool Available()
	{
		return ability.CanCast && base.Available();
	}

	public override void Reset()
	{
		base.Reset();
		if (ability is Ability_Base ability_Base)
		{
			ability_Base.Reset();
		}
	}

	public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
	{
		return targ == Caster || base.CanHitTargetFrom(root, targ);
	}

	protected override bool TryCastShot()
	{
		if (ability is Ability_Base ability_Base && !ability_Base.CanApplyOn(base.CurrentTarget) && !ability_Base.CanApplyOn(base.CurrentDestination))
		{
			return false;
		}
		if (ability.CanCast)
		{
			return base.TryCastShot();
		}
		return false;
	}
}

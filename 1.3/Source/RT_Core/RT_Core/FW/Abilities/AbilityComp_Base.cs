using RimWorld;
using Verse;

namespace RT_Core;

public class AbilityComp_Base : CompAbilityEffect
{
	public new virtual bool CanCast => true;

	public virtual Command Gizmo => null;

	public virtual bool CanActivateOn(LocalTargetInfo target, LocalTargetInfo dest)
	{
		return true;
	}

	public virtual void PostTick()
	{
	}

	public new virtual void PostExposeData()
	{
	}

	public virtual void PostReset()
	{
	}
}

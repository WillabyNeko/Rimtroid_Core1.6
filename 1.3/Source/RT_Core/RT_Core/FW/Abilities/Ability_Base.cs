using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RT_Core;

public class Ability_Base : Ability
{
	public virtual bool CanAutoCast => AbilityControl?.AutoUse ?? false;

	protected IEnumerable<AbilityComp_Base> BaseAbilityComps => CompsOfType<AbilityComp_Base>() ?? new AbilityComp_Base[0];

	protected AbilityComp_AbilityControl AbilityControl => CompOfType<AbilityComp_AbilityControl>();

	public override bool CanCast
	{
		get
		{
			if (!base.CanCast)
			{
				return false;
			}
			foreach (AbilityComp_Base baseAbilityComp in BaseAbilityComps)
			{
				if (!baseAbilityComp.CanCast)
				{
					return false;
				}
			}
			return true;
		}
	}

	public virtual bool CanShowGizmos => pawn.HomeFaction != null && pawn.HomeFaction.IsPlayer && !pawn.InMentalState;

	public Ability_Base(Pawn pawn)
		: base(pawn)
	{
	}

	public Ability_Base(Pawn pawn, AbilityDef def)
		: base(pawn, def)
	{
	}

	public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (base.CooldownTicksRemaining > 0)
		{
			return false;
		}
		foreach (AbilityComp_Base baseAbilityComp in BaseAbilityComps)
		{
			if (!baseAbilityComp.CanActivateOn(target, dest))
			{
				return false;
			}
		}
		return base.Activate(target, dest);
	}

	public override void AbilityTick()
	{
		base.AbilityTick();
		foreach (AbilityComp_Base baseAbilityComp in BaseAbilityComps)
		{
			baseAbilityComp.PostTick();
		}
	}

	public void InitializeComps()
	{
		if (def == null || def.comps.NullOrEmpty())
		{
			return;
		}
		comps = new List<AbilityComp>();
		for (int i = 0; i < def.comps.Count; i++)
		{
			AbilityComp abilityComp = null;
			try
			{
				abilityComp = (AbilityComp)Activator.CreateInstance(def.comps[i].compClass);
				abilityComp.parent = this;
				comps.Add(abilityComp);
				abilityComp.Initialize(def.comps[i]);
			}
			catch (Exception ex)
			{
				Log.Error("Could not instantiate or initialize an AbilityComp: " + ex);
				comps.Remove(abilityComp);
			}
		}
	}

	public override void ExposeData()
	{
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			base.ExposeData();
		}
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			InitializeComps();
		}
		foreach (AbilityComp_Base baseAbilityComp in BaseAbilityComps)
		{
			baseAbilityComp.PostExposeData();
		}
	}

	public virtual void Reset()
	{
		StartCooldown(0);
		foreach (AbilityComp_Base baseAbilityComp in BaseAbilityComps)
		{
			baseAbilityComp.PostReset();
		}
	}

	public override IEnumerable<Command> GetGizmos()
	{
		foreach (Command gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		foreach (AbilityComp_Base comp in BaseAbilityComps)
		{
			if (comp.Gizmo != null)
			{
				yield return comp.Gizmo;
			}
		}
	}
}

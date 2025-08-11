using RimWorld;
using RT_Rimtroid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RT_Core
{
    public class Verb_CastAbility_Base : Verb_CastAbility
    {
        public override bool Available()
        {
            return ability.CanCast && base.Available();
        }

        public override void Reset()
        {
            base.Reset();
            if (ability is Ability ab)
            {
                ab.ResetCooldown();
            }
        }
        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            return targ == Caster || base.CanHitTargetFrom(root, targ);
        }

        protected override bool TryCastShot()
        {
            if (ability is Ability ba)
            {
                if (!ba.CanApplyOn(CurrentTarget) && !ba.CanApplyOn(CurrentDestination))
                {
                    //Should be applicable on either the target or the destination.
                    return false;
                }
            }

            if (ability.CanCast)
            {
                return base.TryCastShot();
            }
            else
            {
                return false;
            }
        }
    }

    /*public class Verb_CastAbility_FireBeam : Verb_CastAbility_Base
    {
        private ExpandableProjectileDef ProjectileDef => this.verbProps.defaultProjectile as ExpandableProjectileDef;
        public override void OnGUI(LocalTargetInfo target)
        {
            if (target.IsValid && target.CenterVector3 != Vector3.zero)
            {
                var affectedCells = ExpandableProjectile.GetProjectileLine(ProjectileDef, target.CenterVector3, this.caster.DrawPos, target.CenterVector3, Find.CurrentMap).ToList();
                GenDraw.DrawFieldEdges(affectedCells);
            }
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            base.DrawHighlight(target);
            if (target.IsValid && target.CenterVector3 != Vector3.zero)
            {
                var affectedCells = ExpandableProjectile.GetProjectileLine(ProjectileDef, target.CenterVector3, this.caster.DrawPos, target.CenterVector3, Find.CurrentMap).ToList();
                GenDraw.DrawFieldEdges(affectedCells);
            }
        }
    }*/
}

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RT_Core
{
    public class Ability_AbsorbEnergy : RT_Core.Ability_Base
    {
        public Ability_AbsorbEnergy(Pawn pawn) : base(pawn) { }
        public Ability_AbsorbEnergy(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Pawn.BodySize <= 4f && !Utils.blackListRaces.Contains(target.Thing.def))
            {
                var hediff = HediffMaker.MakeHediff(RT_DefOf.RT_MetroidHunting, pawn);
                pawn.health.AddHediff(hediff);
                var job = JobMaker.MakeJob(RT_DefOf.RT_AbsorbingEnergy, target);
                pawn.jobs.TryTakeOrderedJob(job);
            }
            else
            {
                Messages.Message("RT.AnimalIsTooBig".Translate(target.Pawn.LabelCap), MessageTypeDefOf.CautionInput);
            }
            return base.Activate(target, dest);
        }
    }
}

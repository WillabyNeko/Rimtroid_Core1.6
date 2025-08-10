﻿using RimWorld;
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
    public class JobDriver_EatFromStation : JobDriver
    {
        private float workLeft = -1000f;
        private float originalPower = 0;

        private CompPowerTrader compPowerTrader => job.targetA.Thing.TryGetComp<CompPowerTrader>();
        private CompFlickable compFlickable => job.targetA.Thing.TryGetComp<CompFlickable>();
        private CompRefuelable compRefuelable => job.targetA.Thing.TryGetComp<CompRefuelable>();

        public MetroidFeedingOptions options => job.targetA.Thing.def.GetModExtension<MetroidFeedingStationOptions>().options.Where(x => x.defName == pawn.def.defName).FirstOrDefault();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = new Toil();
            doWork.initAction = delegate
            {
                var comp = job.targetA.Thing.TryGetComp<CompPowerTrader>();
                if (comp != null)
                {
                    this.originalPower = comp.powerOutputInt;
                    comp.powerOutputInt -= options.powerConsumption;
                }
                workLeft = options.ticksForConsumption;
            };
            doWork.tickAction = delegate
            {
                workLeft--;
                if (workLeft <= 0f)
                {
                    if (job.targetA.Thing.TryGetComp<CompPowerTrader>() != null)
                    {
                        job.targetA.Thing.TryGetComp<CompPowerTrader>().powerOutputInt = originalPower;
                    }
                    pawn.needs.food.CurLevel += pawn.needs.food.MaxLevel * 0.35f;
                    var hp = job.targetA.Thing.HitPoints;
                    hp -= options.durabilityDamage;
                    job.targetA.Thing.HitPoints = hp;
                    if (job.targetA.Thing.HitPoints <= 0)
                    {
                        job.targetA.Thing.Destroy(DestroyMode.KillFinalize);
                    }
                    ReadyForNextToil();
                }
            };
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            doWork.AddFinishAction(delegate ()
            {
                if (job.targetA.Thing?.TryGetComp<CompPowerTrader>() != null)
                {
                    job.targetA.Thing.TryGetComp<CompPowerTrader>().powerOutputInt = originalPower;
                }
            });
            this.FailOn(() => compPowerTrader != null && !compPowerTrader.PowerOn);
            this.FailOn(() => compRefuelable != null && !compRefuelable.HasFuel);
            this.FailOn(() => compFlickable != null && !compFlickable.SwitchIsOn);
            yield return doWork;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref workLeft, "workLeft", 0f);
            Scribe_Values.Look(ref originalPower, "originalPower", 0f);
        }
    }
}

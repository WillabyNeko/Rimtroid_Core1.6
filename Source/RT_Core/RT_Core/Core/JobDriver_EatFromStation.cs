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
    public class JobDriver_EatFromStation : JobDriver
    {
        private float workLeft = -1000f;
        public MetroidFeedingOptions options => job.def.GetModExtension<MetroidFeedingStationOptions>().options.Where(x => x.defName == pawn.def.defName).FirstOrDefault();

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
                workLeft = options.ticksForConsumption;
            };
            doWork.tickAction = delegate
            {
                workLeft--;
                if (workLeft <= 0f)
                {
                    pawn.needs.TryGetNeed<Need_Food>().CurLevel = 1;
                    //job.targetA.Thing.TryGetComp<CompPowerTrader>().powerOutputInt -= options.powerConsumption;
                    var hp = job.targetA.Thing.HitPoints;
                    hp -= options.durabilityDamage;
                    if (pawn.TryGetAttackVerb(job.targetA.Thing).TryStartCastOn(job.targetA))
                    {
                        job.targetA.Thing.HitPoints = hp;
                        if (job.targetA.Thing.HitPoints <= 0)
                        {
                            job.targetA.Thing.Destroy(DestroyMode.KillFinalize);
                        }
                    }
                    ReadyForNextToil();
                }
            };
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return doWork;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref workLeft, "workLeft", 0f);
        }
    }
}

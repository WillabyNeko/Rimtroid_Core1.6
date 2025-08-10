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
    public class JobDriver_AbsorbingEnergy : JobDriver
    {
        public RT_EnergyDrain options => pawn.def.GetModExtension<RT_EnergyDrain>();
        public Thing Target => this.TargetA.Thing;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            Toil doWork = new Toil();
            doWork.initAction = delegate ()
            {
                if (Target is Corpse corpse)
                {
                    var hediff = HediffMaker.MakeHediff(RT_DefOf.RT_LatchedMetroid, corpse.InnerPawn) as Hediff_LatchedMetroid;
                    hediff.latchedMetroid = this.pawn;
                    hediff.drainAgeFactor = 0;
                    hediff.drainFoodGain = options.drainFoodGain.RandomInRange;
                    hediff.drainOverlayDuration = options.drainOverlayDuration.RandomInRange;
                    hediff.drainStunDuration = options.drainStunDuration.RandomInRange;
                    hediff.drainSicknessSeverity = options.drainSicknessSeverity;
                    hediff.drainEnergyProcessing = options.drainEnergyProcessing.RandomInRange;
                    corpse.InnerPawn.health.AddHediff(hediff);
                    var hunting = this.pawn.health.hediffSet.GetFirstHediffOfDef(RT_DefOf.RT_MetroidHunting);
                    if (hunting != null)
                    {
                        this.pawn.health.RemoveHediff(hunting);
                    }
                    this.pawn.DeSpawn();
                }
                else if (Target is Pawn victim)
                {
                    if (victim.RaceProps.Animal && Rand.Chance(Mathf.Max(victim.def.race.manhunterOnDamageChance, 0.05f)))
                    {
                        Messages.Message("RT.AnimalDefendThemselves".Translate(victim.Label, this.pawn.Label), victim, MessageTypeDefOf.CautionInput);
                        var job = JobMaker.MakeJob(JobDefOf.AttackMelee, this.pawn);
                        job.expiryInterval = new IntRange(360, 480).RandomInRange;
                        job.checkOverrideOnExpire = true;
                        job.expireRequiresEnemiesNearby = true;
                        victim.jobs.TryTakeOrderedJob(job);
                    }
                    else if (victim.IsPrisoner && Rand.Chance(0.1f))
                    {
                        Messages.Message("RT.PrisonerDefendThemselves".Translate(victim.Label, this.pawn.Label), victim, MessageTypeDefOf.CautionInput);
                        var job = JobMaker.MakeJob(JobDefOf.AttackMelee, this.pawn);
                        job.expiryInterval = new IntRange(360, 480).RandomInRange;
                        job.checkOverrideOnExpire = true;
                        job.expireRequiresEnemiesNearby = true;
                        victim.jobs.TryTakeOrderedJob(job);
                    }
                    else if (Rand.Chance(0.1f))
                    {
                        Utils.MakeFlee(victim, this.pawn, 50, new List<Thing> { this.pawn });
                        Job stand = JobMaker.MakeJob(JobDefOf.Wait, 30);
                        pawn.jobs.jobQueue.EnqueueLast(stand);
                        Job job = JobMaker.MakeJob(RT_DefOf.RT_AbsorbingEnergy, victim);
                        pawn.jobs.jobQueue.EnqueueLast(job);
                    }
                    else
                    {
                        var hediff = HediffMaker.MakeHediff(RT_DefOf.RT_LatchedMetroid, victim) as Hediff_LatchedMetroid;
                        hediff.latchedMetroid = this.pawn;
                        hediff.drainAgeFactor = options.drainAgeFactor;
                        hediff.drainFoodGain = options.drainFoodGain.RandomInRange;
                        hediff.drainOverlayDuration = options.drainOverlayDuration.RandomInRange;
                        hediff.drainStunDuration = options.drainStunDuration.RandomInRange;
                        hediff.drainSicknessSeverity = options.drainSicknessSeverity;
                        hediff.drainEnergyProcessing = options.drainEnergyProcessing.RandomInRange;
                        victim.health.AddHediff(hediff);
                        var hunting = this.pawn.health.hediffSet.GetFirstHediffOfDef(RT_DefOf.RT_MetroidHunting);
                        if (hunting != null)
                        {
                            this.pawn.health.RemoveHediff(hunting);
                        }
                        this.pawn.DeSpawn();
                    }
                }
            };

            doWork.defaultCompleteMode = ToilCompleteMode.Instant;
            doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return doWork;
        }
    }
}

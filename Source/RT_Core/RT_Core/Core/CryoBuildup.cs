
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RT_Core
{
    public class Hediff_CryoBuildup : HediffWithComps
    {
        private int tickMax = 64;
        private int tickCounter = 0;



        public override void Tick()
        {
            base.Tick();
            tickCounter++;
            if (tickCounter > tickMax)
            {
                CompCryoImmunity comp = pawn.TryGetComp<CompCryoImmunity>();
                if (comp != null)
                {
                    tickCounter = 0;
                }
                else if (Rand.Chance(0.01f))
                {
                    pawn.TakeDamage(new DamageInfo(DefDatabase<DamageDef>.GetNamed("RT_SecondaryFrostBurn"), 1f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                    tickCounter = 0;

                }

            }


        }




    }
}
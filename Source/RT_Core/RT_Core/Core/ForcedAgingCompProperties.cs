using System;
using Verse;

namespace RT_Core
{
    public class HediffCompProperties_ModifyAge : HediffCompProperties
    {
        public ForcedAgeUtils.AgeUpdateMethod updateMethod;

        public float amountPerTick;

        public HediffCompProperties_ModifyAge()
        {
            compClass = typeof(HediffComp_ModifyAge);
        }

        public virtual float Amount
        {
            get
            {
                return amountPerTick;
            }
        }

        public virtual ForcedAgeUtils.AgeUpdateMethod Method
        {
            get
            {
                return updateMethod;
            }
        }
    }
}
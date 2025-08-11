using System;
using Verse;

namespace RT_Core
{
    public class HediffCompProperties_ModifyAge : HediffCompProperties
    {
        public ForcedAgeUtils.AgeUpdateMethod updateMethod;

        public int ticks;
        public HediffCompProperties_ModifyAge()
        {
            compClass = typeof(HediffComp_ModifyAge);
        }

        public virtual float Amount => ForcedAgeUtils.TicksToYears(ticks);
        public virtual ForcedAgeUtils.AgeUpdateMethod Method => updateMethod;
    }
}

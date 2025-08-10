using Verse;

namespace RT_Core
{
    public class HediffComp_ModifyAge : HediffComp
    {
        private HediffCompProperties_ModifyAge valProps => (HediffCompProperties_ModifyAge)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            switch (valProps.Method)
            {
                case ForcedAgeUtils.AgeUpdateMethod.AddAge:
                    ForcedAgeUtils.AddPawnAge(Pawn, valProps.Amount);
                    break;
                case ForcedAgeUtils.AgeUpdateMethod.SetAge:
                    ForcedAgeUtils.SetPawnAge(Pawn, valProps.Amount);
                    break;
            }
        }
    }
}

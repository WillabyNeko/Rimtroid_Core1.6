using RimWorld;
using Verse;


namespace RT_Core
{
    class Hediff_MetroidFoodInjection : HediffWithComps
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            if (pawn?.needs?.food is Need_Food food)
            {
                pawn.needs.food.CurLevel += pawn.needs.food.MaxLevel * 0.2f;
            }
        }
    }
}

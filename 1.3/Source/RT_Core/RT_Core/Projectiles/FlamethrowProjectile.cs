using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RT_Rimtroid;

public class FlamethrowProjectile : ExpandableProjectile
{
	public override void DoDamage(IntVec3 pos)
	{
		base.DoDamage(pos);
		try
		{
			if (!(pos != launcher.Position) || launcher.Map == null || !pos.InBounds(launcher.Map))
			{
				return;
			}
			List<Thing> list = launcher.Map.thingGrid.ThingsListAt(pos);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].def != base.Def && list[num] != launcher && list[num].def != ThingDefOf.Fire && !(list[num] is Mote) && !(list[num] is Filth))
				{
					if (!list.Where((Thing x) => x.def == ThingDefOf.Fire).Any())
					{
						CompAttachBase compAttachBase = list[num].TryGetComp<CompAttachBase>();
						Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
						fire.fireSize = 1f;
						GenSpawn.Spawn(fire, list[num].Position, list[num].Map, Rot4.North);
						if (compAttachBase != null)
						{
							fire.AttachTo(list[num]);
							if (list[num] is Pawn pawn)
							{
								pawn.jobs.StopAll();
								pawn.records.Increment(RecordDefOf.TimesOnFire);
							}
						}
					}
					customImpact = true;
					Impact(list[num]);
					customImpact = false;
				}
			}
		}
		catch
		{
		}
	}
}

﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RT_Rimtroid
{
	public class FlamethrowProjectile : ExpandableProjectile
	{
		public override void DoDamage(IntVec3 pos)
		{
			base.DoDamage(pos);
			try
			{
				if (pos != this.launcher.Position && this.launcher.Map != null && GenGrid.InBounds(pos, this.launcher.Map))
				{
					var list = this.launcher.Map.thingGrid.ThingsListAt(pos);
					for (int num = list.Count - 1; num >= 0; num--)
					{
						if (list[num].def != this.Def && list[num] != this.launcher && list[num].def != ThingDefOf.Fire && (!(list[num] is Mote) && (!(list[num] is Filth))))
						{
							if (!list.Where(x => x.def == ThingDefOf.Fire).Any())
							{
								CompAttachBase compAttachBase = list[num].TryGetComp<CompAttachBase>();
								Fire obj = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
								obj.fireSize = 1f;
								GenSpawn.Spawn(obj, list[num].Position, list[num].Map, Rot4.North);
								if (compAttachBase != null)
								{
									obj.AttachTo(list[num]);
									Pawn pawn = list[num] as Pawn;
									if (pawn != null)
									{
										pawn.jobs.StopAll();
										pawn.records.Increment(RecordDefOf.TimesOnFire);
									}
								}
							}
							this.customImpact = true;
							base.Impact(list[num]);
							this.customImpact = false;
						}
					}
				}
			}
			catch { };
		}
	}
}

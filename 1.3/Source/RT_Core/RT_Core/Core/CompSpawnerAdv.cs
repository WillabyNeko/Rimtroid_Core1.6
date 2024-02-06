using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RT_Core;

public class CompSpawnerAdv : ThingComp
{
	private int ticksUntilSpawn;

	private List<Pawn> spawnedPawns;

	private List<Thing> spawnedThings;

	public CompProperties_SpawnerAdv PropsSpawner => (CompProperties_SpawnerAdv)props;

	private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;

	public int SpawnedPawns
	{
		get
		{
			if (spawnedPawns == null)
			{
				return 0;
			}
			int num = 0;
			for (int num2 = spawnedPawns.Count - 1; num2 >= 0; num2--)
			{
				if (!spawnedPawns[num2].Dead && !spawnedPawns[num2].Destroyed)
				{
					num++;
				}
				else
				{
					spawnedPawns.RemoveAt(num2);
				}
			}
			return num;
		}
	}

	public int SpawnedThings
	{
		get
		{
			if (spawnedThings == null)
			{
				return 0;
			}
			int num = 0;
			for (int num2 = spawnedThings.Count - 1; num2 >= 0; num2--)
			{
				if (!spawnedThings[num2].Destroyed)
				{
					num++;
				}
				else
				{
					spawnedThings.RemoveAt(num2);
				}
			}
			return num;
		}
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!respawningAfterLoad)
		{
			ResetCountdown();
		}
	}

	public override void CompTick()
	{
		TickInterval(1);
	}

	public override void CompTickRare()
	{
		TickInterval(250);
	}

	private void TickInterval(int interval)
	{
		if (!parent.Spawned)
		{
			return;
		}
		CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
		if (comp != null)
		{
			if (!comp.Awake)
			{
				return;
			}
		}
		else if (parent.Position.Fogged(parent.Map))
		{
			return;
		}
		if (!PropsSpawner.requiresPower || PowerOn)
		{
			ticksUntilSpawn -= interval;
			CheckShouldSpawn();
		}
	}

	private void CheckShouldSpawn()
	{
		if (ticksUntilSpawn <= 0)
		{
			TryDoSpawn();
			ResetCountdown();
		}
	}

	public bool TryDoSpawn()
	{
		if (!parent.Spawned)
		{
			return false;
		}
		Log.Message(parent?.ToString() + " - " + PropsSpawner.pawnKindToSpawn, ignoreStopLoggingLimit: true);
		if (PropsSpawner.pawnKindToSpawn != null)
		{
			int num = 0;
			if (num < PropsSpawner.spawnCount)
			{
				if ((PropsSpawner.maxPawnCount > 0 && SpawnedPawns < PropsSpawner.maxPawnCount) || PropsSpawner.maxPawnCount == 0)
				{
					Faction faction = null;
					if (PropsSpawner.inheritFaction)
					{
						faction = parent.Faction;
					}
					Pawn pawn = PawnGenerator.GeneratePawn(PropsSpawner.pawnKindToSpawn, faction);
					GenSpawn.Spawn(pawn, parent.Position, parent.Map);
					if (PropsSpawner.maxPawnCount > 0)
					{
						spawnedPawns ??= new List<Pawn>();
						spawnedPawns.Add(pawn);
					}
					return true;
				}
				return false;
			}
		}
		else if (PropsSpawner.thingToSpawn != null)
		{
            IntVec3 result;
            if (PropsSpawner.spawnInPlace)
			{
				result = parent.Position;
			}
			else
			{
				TryFindSpawnCell(parent, PropsSpawner.thingToSpawn, PropsSpawner.spawnCount, out result);
			}
			if (result.IsValid)
			{
				if ((PropsSpawner.maxThingCount > 0 && SpawnedThings < PropsSpawner.maxThingCount) || PropsSpawner.maxThingCount == 0)
				{
					Thing thing = ThingMaker.MakeThing(PropsSpawner.thingToSpawn);
					thing.stackCount = PropsSpawner.spawnCount;
					if (thing == null)
					{
						Log.Error("Could not spawn anything for " + parent);
					}
					if (PropsSpawner.inheritFaction && thing.Faction != parent.Faction)
					{
						thing.SetFaction(parent.Faction);
					}
					GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Direct, out var lastResultingThing);
					if (PropsSpawner.spawnForbidden)
					{
						lastResultingThing.SetForbidden(value: true);
					}
					if (PropsSpawner.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
					{
						Messages.Message("MessageCompSpawnerSpawnedItem".Translate(PropsSpawner.thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent);
					}
					if (PropsSpawner.maxThingCount > 0)
					{
						spawnedThings ??= new List<Thing>();
						spawnedThings.Add(thing);
					}
				}
				return true;
			}
		}
		return false;
	}

	public static bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
	{
		foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(parent).InRandomOrder())
		{
			if (!item.Walkable(parent.Map))
			{
				continue;
			}
			Building edifice = item.GetEdifice(parent.Map);
			if ((edifice != null && thingToSpawn.IsEdifice()) || edifice is Building_Door { FreePassage: false } || (parent.def.passability != Traversability.Impassable && !GenSight.LineOfSight(parent.Position, item, parent.Map)))
			{
				continue;
			}
			bool flag = false;
			List<Thing> thingList = item.GetThingList(parent.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing.def.category == ThingCategory.Item && (thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				result = item;
				return true;
			}
		}
		result = IntVec3.Invalid;
		return false;
	}

	private void ResetCountdown()
	{
		ticksUntilSpawn = PropsSpawner.spawnIntervalRange.RandomInRange;
	}

	public override void PostExposeData()
	{
		string text = (PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (PropsSpawner.saveKeysPrefix + "_"));
		Scribe_Values.Look(ref ticksUntilSpawn, text + "ticksUntilSpawn", 0);
		Scribe_Collections.Look(ref spawnedPawns, "spawnedPawns", LookMode.Reference);
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (Prefs.DevMode)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEBUG: Spawn " + PropsSpawner.thingToSpawn.label,
				icon = TexCommand.DesirePower,
				action = delegate
				{
					TryDoSpawn();
					ResetCountdown();
				}
			};
		}
	}

	public override string CompInspectStringExtra()
	{
		if (PropsSpawner.writeTimeLeftToSpawn && (!PropsSpawner.requiresPower || PowerOn))
		{
			return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(PropsSpawner.thingToSpawn, null, PropsSpawner.spawnCount)) + ": " + GenDate.ToStringTicksToPeriod(ticksUntilSpawn, true, false, true, true);
		}
		return null;
	}
}

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RT_Rimtroid;

public class ExpandableProjectile : Bullet
{
	private int curDuration;

	private Vector3 startingPosition;

	private Vector3 prevPosition;

	private int curProjectileIndex;

	private int curProjectileFadeOutIndex;

	private bool stopped;

	private float maxRange;

	private int prevTick;

	public bool doFinalAnimations;

	public bool pawnMoved;

	public Vector3 curPosition;

	[TweakValue("0RM", 0f, 10f)]
	private static float widthTweak = 2.5f;

	protected bool customImpact;

	public List<Thing> hitThings;

	public new virtual int DamageAmount => Def.projectile.GetDamageAmount(weaponDamageMultiplier);

	public bool IsMoving
	{
		get
		{
			if (!stopped && DrawPos != prevPosition)
			{
				prevPosition = DrawPos;
				return true;
			}
			return false;
		}
	}

	public ExpandableProjectileDef Def => base.def as ExpandableProjectileDef;

	private Material ProjectileMat
	{
		get
		{
			if (!Def.graphicData.texPathFadeOut.NullOrEmpty() && (!doFinalAnimations || Def.lifeTimeDuration - curDuration > Def.graphicData.MaterialsFadeOut.Length - 1))
			{
				Material result = Def.graphicData.Materials[curProjectileIndex];
				if (prevTick != Find.TickManager.TicksAbs && Find.TickManager.TicksAbs - TickFrameRate >= prevTick)
				{
					if (curProjectileIndex == Def.graphicData.Materials.Length - 1)
					{
						curProjectileIndex = 0;
					}
					else
					{
						curProjectileIndex++;
					}
					prevTick = Find.TickManager.TicksAbs;
				}
				return result;
			}
			Material result2 = Def.graphicData.MaterialsFadeOut[curProjectileFadeOutIndex];
			if (prevTick != Find.TickManager.TicksAbs && Find.TickManager.TicksAbs - TickFrameRate >= prevTick)
			{
				if (Def.graphicData.MaterialsFadeOut.Length - 1 != curProjectileFadeOutIndex)
				{
					curProjectileFadeOutIndex++;
				}
				prevTick = Find.TickManager.TicksAbs;
			}
			return result2;
		}
	}

	public int TickFrameRate
	{
		get
		{
			if (!doFinalAnimations)
			{
				return Def.tickFrameRate;
			}
			if (Def.finalTickFrameRate > 0)
			{
				return Def.finalTickFrameRate;
			}
			return Def.tickFrameRate;
		}
	}

	public Vector3 StartingPosition
	{
		get
		{
			if (launcher is not Pawn)
			{
				startingPosition = launcher.OccupiedRect().CenterVector3;
			}
			else if (!pawnMoved && launcher is Pawn { Dead: false } pawn)
			{
				if (pawn.pather.MovingNow)
				{
					pawnMoved = true;
				}
				else
				{
					startingPosition = pawn.OccupiedRect().CenterVector3;
				}
			}
			return startingPosition;
		}
	}

	public Vector3 CurPosition
	{
		get
		{
			if (stopped)
			{
				return curPosition;
			}
			if (Def.reachMaxRangeAlways)
			{
				Vector3 a = new(launcher.TrueCenter().x, 0f, launcher.TrueCenter().z);
				Vector3 drawPos = DrawPos;
				float num = Vector3.Distance(a, drawPos);
				float num2 = maxRange - num;
				if (num2 < 0f)
				{
					if (!stopped)
					{
						StopMotion();
					}
					return curPosition;
				}
				return DrawPos;
			}
			return DrawPos;
		}
	}

	public void SetDestinationToMax(Thing equipment)
	{
		maxRange = equipment.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.range;
		Vector3 vector = new(launcher.TrueCenter().x, 0f, launcher.TrueCenter().z);
		Vector3 vector2 = new(destination.x, 0f, destination.z);
		float num = Vector3.Distance(vector, vector2);
		float num2 = maxRange - num;
		Vector3 normalized = (vector2 - vector).normalized;
		destination += normalized * num2;
		IntVec3 cell = destination.ToIntVec3();
		if (!cell.InBounds(base.Map))
		{
			destination = CellRect.WholeMap(base.Map).EdgeCells.OrderBy((IntVec3 x) => x.DistanceTo(cell)).First().ToVector3();
		}
		ticksToImpact = Mathf.CeilToInt(base.StartingTicksToImpact);
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad)
		{
			startingPosition = base.Position.ToVector3Shifted();
			startingPosition.y = 0f;
		}
	}

	public override void Draw()
	{
		DrawProjectile();
	}

	public void DrawProjectile()
	{
		Vector3 vector = CurPosition;
		vector.y = 0f;
		Vector3 vector2 = StartingPosition;
		vector2.y = 0f;
        Vector3 b = new(destination.x, destination.y, destination.z)
        {
            y = 0f
        };
        Quaternion q = Quaternion.LookRotation(vector - vector2);
		Vector3 pos = (vector2 + vector) / 2f;
		pos.y = 10f;
		pos += Quaternion.Euler(0f, (vector2 - vector).AngleFlat(), 0f) * Def.startingPositionOffset;
		float num = Vector3.Distance(vector2, vector) * Def.totalSizeScale;
		float num2 = Vector3.Distance(vector2, b);
		float num3 = num / num2;
		Vector3 s = new(num * Def.widthScaleFactor * num3, 0f, num * Def.heightScaleFactor);
		Matrix4x4 matrix = default;
		matrix.SetTRS(pos, q, s);
		Graphics.DrawMesh(MeshPool.plane10, matrix, ProjectileMat, 0);
	}

	public static float GetOppositeAngle(float angle)
	{
		return (angle + 180f) % 360f;
	}

	public static Direction8Way Direction8WayFromAngle(float angle)
	{
		if (angle >= 337.5f || angle < 22.5f)
		{
			return Direction8Way.North;
		}
		if (angle < 67.5f)
		{
			return Direction8Way.NorthEast;
		}
		if (angle < 112.5f)
		{
			return Direction8Way.East;
		}
		if (angle < 157.5f)
		{
			return Direction8Way.SouthEast;
		}
		if (angle < 202.5f)
		{
			return Direction8Way.South;
		}
		if (angle < 247.5f)
		{
			return Direction8Way.SouthWest;
		}
		if (angle < 292.5f)
		{
			return Direction8Way.West;
		}
		return Direction8Way.NorthWest;
	}

	public static IntVec3 IntVec3FromDirection8Way(Direction8Way source)
	{
		return source switch
		{
			Direction8Way.North => IntVec3.North, 
			Direction8Way.NorthEast => IntVec3.NorthEast, 
			Direction8Way.East => IntVec3.East, 
			Direction8Way.SouthEast => IntVec3.SouthEast, 
			Direction8Way.South => IntVec3.South, 
			Direction8Way.SouthWest => IntVec3.SouthWest, 
			Direction8Way.West => IntVec3.West, 
			Direction8Way.NorthWest => IntVec3.NorthWest, 
			_ => IntVec3.Invalid, 
		};
	}

	public static HashSet<IntVec3> GetProjectileLine(ExpandableProjectileDef projectileDef, Vector3 curPosition, Vector3 startingPosition, Vector3 end, Map map)
	{
		HashSet<IntVec3> hashSet = new();
		if (projectileDef.fixedShape != null)
		{
			SimpleCurve widthCurve = projectileDef.fixedShape.widthCurve;
			List<IntVec3> list = new ShootLine(startingPosition.ToIntVec3(), end.ToIntVec3()).Points().ToList();
			startingPosition.y = 0f;
			end.y = 0f;
			float angle = startingPosition.AngleToFlat(end) + 180f;
			Log.Clear();
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				IntVec3 intVec = list[i];
				if (!(intVec.DistanceTo(startingPosition.ToIntVec3()) >= projectileDef.minDistanceToAffect))
				{
					continue;
				}
				List<IntVec3> list2 = new()
                {
                    intVec
                };
				float num2 = widthCurve.Evaluate(num);
				if (num2 > 0f)
				{
					int num3 = 1;
					while ((float)list2.Count < num2)
					{
						if ((float)list2.Count < num2)
						{
							Direction8Way source = Direction8WayFromAngle(angle);
							IntVec3 intVec2 = IntVec3FromDirection8Way(source);
							string[] obj = new string[6]
							{
								"angle: ",
								angle.ToString(),
								", direction way: ",
								source.ToString(),
								" - facing cell: ",
								null
							};
							IntVec3 intVec3 = intVec2;
							obj[5] = intVec3.ToString();
							Log.Message(string.Concat(obj));
							list2.Add(intVec + intVec2 * num3);
						}
						if ((float)list2.Count < num2)
						{
							float oppositeAngle = GetOppositeAngle(angle);
							Direction8Way source2 = Direction8WayFromAngle(oppositeAngle);
							IntVec3 intVec4 = IntVec3FromDirection8Way(source2);
							string[] obj2 = new string[6]
							{
								"Opposite angle: ",
								oppositeAngle.ToString(),
								", direction way: ",
								source2.ToString(),
								" - facing cell: ",
								null
							};
							IntVec3 intVec3 = intVec4;
							obj2[5] = intVec3.ToString();
							Log.Message(string.Concat(obj2));
							list2.Add(intVec + intVec4 * num3);
						}
						num3++;
					}
					hashSet.AddRange(list2);
				}
				num++;
			}
		}
		else
		{
			IEnumerable<IntVec3> enumerable = new ShootLine(startingPosition.ToIntVec3(), end.ToIntVec3()).Points();
			foreach (IntVec3 item in enumerable)
			{
				foreach (IntVec3 item2 in GenRadial.RadialCellsAround(item, 5f, useCenter: true))
				{
					hashSet.Add(item);
					map.debugDrawer.FlashCell(item);
				}
			}
			Vector3 vector = curPosition;
			vector.y = 0f;
			startingPosition.y = 0f;
			Vector3 vector2 = new(vector.x, vector.y, vector.z);
			Vector3 vect = (startingPosition + vector) / 2f;
			vect.y = 10f;
			vect += Quaternion.Euler(0f, (startingPosition - vector).AngleFlat(), 0f) * projectileDef.startingPositionOffset;
			float num4 = Vector3.Distance(startingPosition, vector) * projectileDef.totalSizeScale;
			float num5 = Vector3.Distance(startingPosition, vector);
			float num6 = num4 / num5;
			float num7 = num4 * projectileDef.widthScaleFactor * num6;
			if (projectileDef.minWidth > 0f && projectileDef.minWidth > num7)
			{
				num7 = projectileDef.minWidth;
			}
			float num8 = num4 * projectileDef.heightScaleFactor;
			IntVec3 a = vect.ToIntVec3();
			IntVec3 a2 = startingPosition.ToIntVec3();
			IntVec3 b = curPosition.ToIntVec3();
			float radius = ((num8 > num7) ? num8 : num7);
			if (enumerable.Any())
			{
				foreach (IntVec3 cell in GenRadial.RadialCellsAround(startingPosition.ToIntVec3(), radius, useCenter: true))
				{
					if (a.DistanceToSquared(b) >= cell.DistanceToSquared(b) && a2.DistanceTo(cell) > projectileDef.minDistanceToAffect)
					{
						IntVec3 a3 = enumerable.MinBy((IntVec3 x) => x.DistanceToSquared(cell));
						if (num7 / num8 > (float)a3.DistanceToSquared(cell))
						{
							hashSet.Add(cell);
						}
					}
				}
				foreach (IntVec3 item3 in enumerable)
				{
					float num9 = a2.DistanceTo(item3);
					if (num9 > projectileDef.minDistanceToAffect && num9 <= a2.DistanceTo(b))
					{
						hashSet.Add(item3);
					}
				}
			}
		}
		return hashSet;
	}

	private void StopMotion()
	{
		if (!stopped)
		{
			stopped = true;
			curPosition = DrawPos;
			destination = curPosition;
		}
	}

	public override void Tick()
	{
		base.Tick();
		if (Find.TickManager.TicksGame % Def.tickDamageRate == 0)
		{
			HashSet<IntVec3> projectileLine = GetProjectileLine(Def, CurPosition, StartingPosition, DrawPos, base.Map);
			foreach (IntVec3 item in projectileLine)
			{
				DoDamage(item);
			}
		}
		if (!doFinalAnimations && (!IsMoving || pawnMoved))
		{
			doFinalAnimations = true;
			int num = Def.lifeTimeDuration - Def.graphicData.MaterialsFadeOut.Length;
			if (num > curDuration)
			{
			}
			if (!Def.reachMaxRangeAlways && pawnMoved)
			{
				StopMotion();
			}
		}
		if (Find.TickManager.TicksGame % TickFrameRate == 0 && Def.lifeTimeDuration > 0)
		{
			curDuration++;
			if (curDuration > Def.lifeTimeDuration)
			{
				Log.Message("curDuration: " + curDuration + " - def.lifeTimeDuration: " + Def.lifeTimeDuration);
				Destroy();
			}
		}
	}

	public virtual void DoDamage(IntVec3 pos)
	{
	}

	protected virtual void Impact(Thing hitThing)
	{
		if (Def.stopWhenHit && !stopped && !customImpact)
		{
			StopMotion();
		}
		hitThings ??= new List<Thing>();
		if (Def.dealsDamageOnce && hitThings.Contains(hitThing))
		{
			return;
		}
		hitThings.Add(hitThing);
		Map map = base.Map;
		IntVec3 position = base.Position;
		BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = ((equipmentDef != null) ? new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, Def, targetCoverDef) : new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, ThingDef.Named("Gun_Autopistol"), Def, targetCoverDef));
		Find.BattleLog.Add(battleLogEntry_RangedImpact);
		NotifyImpact(hitThing, map, position);
		if (hitThing == null || (Def.disableVanillaDamageMethod && (!customImpact || !Def.disableVanillaDamageMethod)))
		{
			return;
		}
		DamageInfo dinfo = new(Def.projectile.damageDef, DamageAmount, base.ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
		hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
		if (hitThing is Pawn { stances: not null } pawn && pawn.BodySize <= Def.projectile.StoppingPower + 0.001f)
		{
			pawn.stances.stagger.StaggerFor(95);
		}
		if (Def.projectile.extraDamages != null)
		{
			foreach (ExtraDamage extraDamage in Def.projectile.extraDamages)
			{
				if (Rand.Chance(extraDamage.chance))
				{
					DamageInfo dinfo2 = new(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
					hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
				}
			}
		}
		if (Def.stopWhenHitAt.Contains(hitThing.def.defName) && !stopped)
		{
			StopMotion();
		}
	}

	private void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
	{
		BulletImpactData bulletImpactData = default;
		bulletImpactData.bullet = this;
		bulletImpactData.hitThing = hitThing;
		bulletImpactData.impactPosition = position;
		BulletImpactData impactData = bulletImpactData;
		try
		{
			hitThing?.Notify_BulletImpactNearby(impactData);
		}
		catch
		{
		}
		int num = 9;
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = position + GenRadial.RadialPattern[i];
			if (!c.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (thingList[j] != hitThing)
				{
					try
					{
						thingList[j].Notify_BulletImpactNearby(impactData);
					}
					catch
					{
					}
				}
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref startingPosition, "startingPosition");
		Scribe_Values.Look(ref doFinalAnimations, "doFinalAnimations", defaultValue: false);
		Scribe_Values.Look(ref pawnMoved, "pawnMoved", defaultValue: false);
		Scribe_Values.Look(ref curDuration, "curDuration", 0);
		Scribe_Values.Look(ref curProjectileIndex, "curProjectileIndex", 0);
		Scribe_Values.Look(ref curProjectileFadeOutIndex, "curProjectileFadeOutIndex", 0);
		Scribe_Values.Look(ref prevTick, "prevTick", 0);
		Scribe_Values.Look(ref prevPosition, "prevPosition");
		Scribe_Values.Look(ref stopped, "stopped", defaultValue: false);
		Scribe_Values.Look(ref curPosition, "curPosition");
		Scribe_Values.Look(ref maxRange, "maxRange", 0f);
	}
}

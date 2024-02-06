using UnityEngine;
using Verse;
using Verse.AI;

namespace RT_Core;

public class HostilityStatisticRecord : IExposable
{
	private IAttackTarget target;

	private int lastTickAttacked;

	private int intendedHits;

	private int unintendedHits;

	private float damageTotal;

	public IAttackTarget Target => target;

	public int IntendedHitCount => intendedHits;

	public int UnintendedHitCount => unintendedHits;

	public float DamageTotal => damageTotal;

	public int TicksSinceAttack => GenTicks.TicksGame - lastTickAttacked;

	public bool IsRecent => TicksSinceAttack < 250;

	public bool IsOld => TicksSinceAttack > 2000;

	public float DamagePoints
	{
		get
		{
			float num = DamageTotal / Mathf.Log10(TicksSinceAttack);
			return float.IsNaN(num) ? 0f : num;
		}
	}

	public HostilityStatisticRecord()
	{
		lastTickAttacked = 0;
		intendedHits = 0;
		unintendedHits = 0;
		damageTotal = 0f;
	}

	public HostilityStatisticRecord(IAttackTarget target)
		: this()
	{
		this.target = target;
	}

	public void ProcessAttack(float damage, bool isIntended)
	{
		lastTickAttacked = GenTicks.TicksGame;
		damageTotal += damage;
		if (isIntended)
		{
			intendedHits++;
		}
		else
		{
			unintendedHits++;
		}
	}

	public float CalculatePoints(HostilityResponseType type)
	{
		float num = DamagePoints;
		switch (type)
		{
		case HostilityResponseType.Aggressive:
			num *= (float)(IntendedHitCount + UnintendedHitCount);
			break;
		case HostilityResponseType.Defensive:
			num *= (float)(IntendedHitCount + UnintendedHitCount / 2);
			break;
		}
		return num * (float)GenTicks.TicksGame;
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref target, "target");
		Scribe_Values.Look(ref lastTickAttacked, "lastTickAttacked", 0);
		Scribe_Values.Look(ref damageTotal, "damageTaken", 0f);
		Scribe_Values.Look(ref intendedHits, "intendedHits", 0);
		Scribe_Values.Look(ref unintendedHits, "unintendedHits", 0);
	}
}

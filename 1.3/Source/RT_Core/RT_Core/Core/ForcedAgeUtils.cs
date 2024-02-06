using Verse;

namespace RT_Core;

public static class ForcedAgeUtils
{
	public enum AgeUpdateMethod
	{
		AddAge,
		SetAge
	}

	public static long YearsToTicks(float years)
	{
		return (long)(years * 3600000f);
	}

	public static float TicksToYears(long ticks)
	{
		return (float)ticks / 3600000f;
	}

	private static void BatchedAddTicks(Pawn pawn, long ticks)
	{
		while (ticks != 0)
		{
			int num = 0;
			num = (int)((ticks >= int.MinValue) ? ((ticks <= int.MaxValue) ? ticks : int.MaxValue) : int.MinValue);
			ticks -= num;
			pawn.ageTracker.AgeTickMothballed(num);
		}
		pawn.ageTracker.AgeBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
		int curLifeStageIndex = pawn.ageTracker.CurLifeStageIndex;
	}

	public static void SetPawnAge(Pawn pawn, float years)
	{
		if (years < 0f)
		{
			years = 0f;
		}
		long num = YearsToTicks(years);
		num -= pawn.ageTracker.AgeBiologicalTicks;
		BatchedAddTicks(pawn, num);
	}

	public static void AddPawnAge(Pawn pawn, float years)
	{
		long num = YearsToTicks(years);
		if (pawn.ageTracker.AgeBiologicalTicks + num < 0)
		{
			SetPawnAge(pawn, 0f);
		}
		else
		{
			BatchedAddTicks(pawn, num);
		}
	}
}

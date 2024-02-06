using Verse;

namespace RT_Rimtroid;

public static class Utils
{
	public static IntVec3 North(this IntVec3 intVec3)
	{
		return intVec3 + IntVec3.North;
	}

	public static IntVec3 South(this IntVec3 intVec3)
	{
		return intVec3 + IntVec3.South;
	}

	public static IntVec3 West(this IntVec3 intVec3)
	{
		return intVec3 + IntVec3.West;
	}

	public static IntVec3 East(this IntVec3 intVec3)
	{
		return intVec3 + IntVec3.East;
	}
}

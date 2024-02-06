using Verse;

namespace RT_Core;

public class DisableTaming : HediffComp
{
	public override bool CompShouldRemove => false;

	public override string CompDebugString()
	{
		return "taming=false";
	}
}

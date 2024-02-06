using Verse;

namespace RT_Core;

public class LaserGunDef : ThingDef
{
	public static LaserGunDef defaultObj = new();

	public float barrelLength = 0.9f;

	public bool supportsColors = false;
}

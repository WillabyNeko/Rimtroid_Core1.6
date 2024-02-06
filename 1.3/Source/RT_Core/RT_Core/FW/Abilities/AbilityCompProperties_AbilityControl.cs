using RimWorld;

namespace RT_Core;

public class AbilityCompProperties_AbilityControl : CompProperties_AbilityEffect
{
	public bool autoUse = false;

	public TargetingParameters targetParms = null;

	public string gizmoOnText = "Breath Enabled";

	public string gizmoOffText = "Breath Disabled";

	public string gizmoOnIconPath;

	public string gizmoOffIconPath;

	public string gizmoDesc = "Toggle Dragon Breath Usage";
}

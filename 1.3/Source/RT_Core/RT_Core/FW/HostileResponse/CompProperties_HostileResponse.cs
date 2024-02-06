using System.Collections.Generic;
using Verse;

namespace RT_Core;

public class CompProperties_HostileResponse : CompProperties
{
	public bool debug = false;

	public bool controllableGizmo = true;

	public HostilityResponseType initialHostility = HostilityResponseType.Aggressive;

	public List<HostileResponseOption> options = new();

	public MentalStateDef friendlyFireMentalState;
}

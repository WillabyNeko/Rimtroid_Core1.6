using Verse;

namespace RT_Core;

public class Verb_Shoot_Cooldown : Verb_Shoot
{
	private new int lastShotTick = 0;

	private int Cooldown => (verbProps as VerbProperties_Cooldown).cooldown.SecondsToTicks();

	public bool Usable => lastShotTick + Cooldown < Find.TickManager.TicksGame;

	protected override bool TryCastShot()
	{
		if (Usable)
		{
			bool flag = base.TryCastShot();
			if (burstShotsLeft <= 1)
			{
				lastShotTick = Find.TickManager.TicksGame;
			}
			return true;
		}
		return false;
	}

	public override void Reset()
	{
		Refresh();
		((Verb)this).Reset();
	}

	public void Refresh()
	{
		lastShotTick = 0;
	}

	public override bool Available()
	{
		return Usable && base.Available();
	}
}

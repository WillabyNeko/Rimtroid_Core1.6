using Verse;

namespace RT_Core;

public class Verb_Shoot_Limited : Verb_Shoot
{
	public bool Usable { get; private set; }

	protected override bool TryCastShot()
	{
		if (Usable)
		{
			bool flag = base.TryCastShot();
			if (burstShotsLeft <= 1)
			{
				Usable = false;
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
		Usable = true;
	}

	public override bool Available()
	{
		return Usable && base.Available();
	}
}

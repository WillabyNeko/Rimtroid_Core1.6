using System.Linq;
using RimWorld;
using Verse;

namespace RT_Core;

public class Verb_MeleeAttackDamage_Resetting : Verb_MeleeAttackDamage
{
	protected override bool TryCastShot()
	{
		if (base.TryCastShot())
		{
			if (CasterIsPawn)
			{
				foreach (Verb_Shoot_Limited item in VerbUtils.GetVerbs(CasterPawn).OfType<Verb_Shoot_Limited>())
				{
					Log.Message(CasterPawn.ToStringSafe() + "'s " + item.ToStringSafe() + " was reset");
					item.Refresh();
				}
			}
			return true;
		}
		return false;
	}
}

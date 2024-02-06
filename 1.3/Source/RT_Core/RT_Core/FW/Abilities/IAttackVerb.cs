using RimWorld;
using Verse;

namespace RT_Core;

public interface IAttackVerb
{
	Verb Verb { get; }

	Ability Ability { get; set; }
}

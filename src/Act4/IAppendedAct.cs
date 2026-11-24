using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;

namespace SlaySpoon.Act4;

/// <summary>
/// An act the mod adds after act 3 (The Ending, the Spoon): its room set and its fixed map are built by the mod, never
/// by vanilla <c>GenerateRooms</c> or <c>StandardActMap</c>, which expect rooms these acts don't have.
/// </summary>
internal interface IAppendedAct
{
	/// <summary>Name of the act's own room Rng, so filling its rooms never draws from a vanilla Rng.</summary>
	string RoomsRngName { get; }

	/// <summary>Sets the room set directly from <paramref name="rng"/>.</summary>
	void FillRooms(Rng rng);

	/// <summary>The act's map; it draws from no Rng.</summary>
	ActMap CreateFixedMap();

	/// <summary>Type of <see cref="CreateFixedMap"/>'s map, to recognize it without building one.</summary>
	Type FixedMapType { get; }

	/// <summary>Short name for log lines ("act4", "spoon act").</summary>
	string LogName { get; }
}

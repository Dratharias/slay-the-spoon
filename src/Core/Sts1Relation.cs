namespace SlaySpoon.Core;

/// <summary>
/// How a StS1 card class relates to StS2 (spec §3.4). Namesakes classified "same, StS2 weaker or equal" have
/// no StS1 class at all: the vanilla card serves every mode.
/// </summary>
public enum Sts1Relation
{
	/// <summary>No StS2 namesake.</summary>
	Unique,
	/// <summary>Same effect, StS1 is weaker: replaces the vanilla card in StS1-only and Hybrid.</summary>
	SameKeepSts1,
	/// <summary>Different effect: both versions exist.</summary>
	Duplicate,
	/// <summary>
	/// Reclassified "same, StS2 weaker or equal" after release: the vanilla card serves every mode. The class stays so
	/// that runs and decks saved with it still load, but no pool, generation or version swap offers it.
	/// </summary>
	Retired,
}

namespace SlaySpoon.Core;

/// <summary>
/// The StS1 events the mod ports: the three that hand out event-only StS1 cards (step 3d plan), then the ones that hand
/// out StS1 special relics (backlog wave). The enum name, prefixed with <c>Sts1</c>, is the event's class name.
/// </summary>
public enum Sts1EventKind
{
	Vampires,
	Augmenter,
	Nest,
	GoldenIdolEvent,
	ForgottenAltar,
	MoaiHead,
	CursedTome,
}

/// <summary>
/// When the StS1 events may appear, and who may take their card. They exist only where StS1 cards are in play:
/// a StS1-only run, or players holding the Spire Souvenir. The relic events follow the same rule and, like the card
/// events, appear in act 2 only (owner decision: "ported like Vampires and Augmenter, act 2 injection").
/// </summary>
public static class Sts1EventRule
{
	/// <summary>StS1 had these events in act 2 (index 1).</summary>
	public const int ActIndex = 1;

	/// <summary>Chance that an act 2 event room becomes a StS1 event, while one is left to see.</summary>
	public const float ReplaceChance = 1f / 3f;

	/// <summary>
	/// The event may appear in a run of <paramref name="players"/> players, <paramref name="souvenirOwners"/> of whom
	/// hold the Spire Souvenir. Vampires replaces every Strike, so it waits until every player holds the relic.
	/// </summary>
	public static bool IsAllowed(Sts1EventKind kind, bool sts1Run, int players, int souvenirOwners)
	{
		if (sts1Run)
		{
			return true;
		}
		return kind == Sts1EventKind.Vampires
			? players > 0 && souvenirOwners == players
			: souvenirOwners > 0;
	}

	/// <summary>A player may take the event's StS1 card; the others see that option locked.</summary>
	public static bool CardUnlocked(bool sts1Run, bool ownsSouvenir)
	{
		return sts1Run || ownsSouvenir;
	}

	/// <summary>The next event room becomes one of <paramref name="candidates"/> StS1 events.</summary>
	public static bool ShouldReplace(int actIndex, int candidates, float roll)
	{
		return actIndex == ActIndex && candidates > 0 && roll < ReplaceChance;
	}

	/// <summary>StS1's Vampires: 30% of max HP, rounded up, never the last point.</summary>
	public static int VampiresMaxHpLoss(int maxHp)
	{
		int loss = (int)Math.Ceiling(maxHp * 0.3m);
		return Math.Min(loss, maxHp - 1);
	}
}

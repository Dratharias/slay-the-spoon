namespace SlaySpoon.Core;

public enum HeartBuffKind { Artifact, BeatOfDeath, PainfulStabs, Strength }

/// <summary>The extra buff the Heart gains on its Nth Buff move, on top of the base Strength.</summary>
public static class HeartBuffRule
{
	public const int BaseStrength = 2;

	public static (HeartBuffKind Kind, int Amount) Extra(int buffsDone) => buffsDone switch
	{
		0 => (HeartBuffKind.Artifact, 2),
		1 => (HeartBuffKind.BeatOfDeath, 1),
		2 => (HeartBuffKind.PainfulStabs, 1),
		3 => (HeartBuffKind.Strength, 10),
		_ => (HeartBuffKind.Strength, 50),
	};
}

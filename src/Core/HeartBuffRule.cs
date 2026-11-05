namespace SlaySpoon.Core;

public enum HeartBuffKind { Artifact, BeatOfDeath, PainfulStabs, Strength }

/// <summary>The extra buff the Heart gains on its Nth Buff move, on top of the base Strength.</summary>
public static class HeartBuffRule
{
	public const int BaseStrength = 2;

	/// <summary>StS1: negative Strength is first lifted to 0, then the base Strength is added, in one application.</summary>
	public static int StrengthGain(int currentStrength) => Math.Max(0, -currentStrength) + BaseStrength;

	public static (HeartBuffKind Kind, int Amount) Extra(int buffsDone) => buffsDone switch
	{
		< 0 => throw new ArgumentOutOfRangeException(nameof(buffsDone), buffsDone, "buff count cannot be negative"),
		0 => (HeartBuffKind.Artifact, 2),
		1 => (HeartBuffKind.BeatOfDeath, 1),
		2 => (HeartBuffKind.PainfulStabs, 1),
		3 => (HeartBuffKind.Strength, 10),
		_ => (HeartBuffKind.Strength, 50),
	};
}

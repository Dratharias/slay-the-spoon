namespace SlaySpoon.Core;

/// <summary>
/// Arithmetic behind the monster HP and damage multipliers.
/// Game-independent so it can be unit-tested without the game's assemblies.
/// </summary>
public static class ScalingMath
{
	public const int MinPercent = 25;
	public const int MaxPercent = 500;
	public const int StepPercent = 25;
	public const int DefaultPercent = 100;

	/// <summary>Every selectable percent, ascending: 25, 50, ..., 500.</summary>
	public static IReadOnlyList<int> AllPercents { get; } =
		Enumerable.Range(0, (MaxPercent - MinPercent) / StepPercent + 1)
			.Select(i => MinPercent + i * StepPercent)
			.ToArray();

	/// <summary>Next grid value in <paramref name="direction"/> (+1 or -1), clamped to [25, 500].</summary>
	public static int Step(int percent, int direction)
	{
		int next = direction > 0
			? (percent / StepPercent + 1) * StepPercent
			: (percent + StepPercent - 1) / StepPercent * StepPercent - StepPercent;
		return Math.Clamp(next, MinPercent, MaxPercent);
	}

	/// <summary>Scaled HP, rounded half away from zero, never below 1.</summary>
	public static int ScaleHp(int baseHp, int percent)
	{
		return (int)ScaleHp((decimal)baseHp, percent);
	}

	/// <summary>Same rounding for the game's decimal HP values (e.g. Creature.ScaleHpForMultiplayer).</summary>
	public static decimal ScaleHp(decimal baseHp, int percent)
	{
		decimal scaled = Math.Round(baseHp * percent / 100m, MidpointRounding.AwayFromZero);
		return Math.Max(1m, scaled);
	}

	public static decimal DamageMultiplier(int percent)
	{
		return percent / 100m;
	}
}

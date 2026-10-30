namespace SlaySpoon.Core;

public enum EmeraldBuffKind { Strength, MaxHp, Metallicize, Regenerate }

/// <summary>StS1 burning elite rules, with zero-based act indexes.</summary>
public static class EmeraldRule
{
	public const int LastAct = 2;

	public static (int Col, int Row)? Pick(IReadOnlyList<(int Col, int Row)> elites, int roll)
	{
		if (elites.Count == 0) return null;
		ArgumentOutOfRangeException.ThrowIfNegative(roll);
		return elites.OrderBy(p => p.Row).ThenBy(p => p.Col).ElementAt(roll % elites.Count);
	}

	public static (EmeraldBuffKind Kind, int Amount) Buff(int roll, int actIndex) => roll switch
	{
		0 => (EmeraldBuffKind.Strength, actIndex + 2),
		1 => (EmeraldBuffKind.MaxHp, 25),
		2 => (EmeraldBuffKind.Metallicize, 2 * actIndex + 4),
		3 => (EmeraldBuffKind.Regenerate, 2 * actIndex + 3),
		_ => throw new ArgumentOutOfRangeException(nameof(roll)),
	};

	public static int MaxHpGain(int maxHp) => (int)Math.Round(maxHp / 4m, MidpointRounding.AwayFromZero);

	public static bool MarkAct(Act4Keys keys, int actIndex, IReadOnlyList<(int, int)> elites, int roll)
	{
		if (actIndex < 0 || actIndex > LastAct || keys.Has(Act4Key.Emerald) || keys.EmeraldNode(actIndex) != null)
			return false;
		if (Pick(elites, roll) is not { } point) return false;
		keys.MarkEmerald(actIndex, point.Col, point.Row);
		return true;
	}
}

namespace SlaySpoon.Core;

/// <summary>
/// The numbers of the StS1 relic events and their relics, from StS1's source. StS1's Ascension 15 raises are not
/// carried over: StS2 has no matching level, so every run uses StS1's base values (builder call).
/// </summary>
public static class Sts1EventMath
{
	/// <summary>libGDX MathUtils.round: floor(x + 0.5).</summary>
	public static int Round(decimal value) => (int)Math.Floor(value + 0.5m);

	/// <summary>Golden Idol: combat gold rewards gain 25% (StS1 rounds the bonus).</summary>
	public static int GoldenIdolGold(int gold) => gold + Round(gold * 0.25m);

	/// <summary>Bloody Idol: HP healed whenever its holder gains gold.</summary>
	public const int BloodyIdolHeal = 5;

	/// <summary>Golden Idol event, Smash: 25% of max HP as damage, truncated.</summary>
	public static int IdolSmashDamage(int maxHp) => (int)(maxHp * 0.25m);

	/// <summary>Golden Idol event, Hide: 8% of max HP, truncated, at least 1.</summary>
	public static int IdolHideMaxHpLoss(int maxHp) => Math.Max(1, (int)(maxHp * 0.08m));

	/// <summary>Forgotten Altar, Sacrifice: 25% of max HP, rounded, taken after gaining <see cref="AltarMaxHpGain"/>.</summary>
	public static int AltarHpLoss(int maxHp) => Round(maxHp * 0.25m);

	public const int AltarMaxHpGain = 5;

	/// <summary>Moai Head, Jump Inside: 12.5% of max HP, rounded, never the last point.</summary>
	public static int MoaiMaxHpLoss(int maxHp) => Math.Min(Round(maxHp * 0.125m), Math.Max(0, maxHp - 1));

	public const int MoaiGold = 333;

	/// <summary>Necronomicon: an Attack that costs at least this much (or an X cost paid with as much) plays twice.</summary>
	public const int NecronomiconCost = 2;

	public static bool NecronomiconTriggers(int energyPaid) => energyPaid >= NecronomiconCost;

	/// <summary>Cursed Tome: HP lost on each page turned (1, 2, 3), then on taking the book or closing it.</summary>
	public static int TomePageHpLoss(int page) => page;

	public const int TomeTakeHpLoss = 10;
	public const int TomeStopHpLoss = 3;
}

using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using SlaySpoon.Watcher.Powers;

namespace SlaySpoon.Watcher;

/// <summary>Hover tips shared by Watcher cards: Scry and Stance (no native tips, text in the card_selection table) and the stances.</summary>
public static class WatcherTips
{
	public static IHoverTip Scry() => new HoverTip(
		new LocString("card_selection", "SPOON_SCRY_TIP.title"),
		new LocString("card_selection", "SPOON_SCRY_TIP.description"));

	/// <summary>StS1's generic Stance keyword, for cards that name no particular stance.</summary>
	public static IHoverTip Stance() => new HoverTip(
		new LocString("card_selection", "SPOON_STANCE_TIP.title"),
		new LocString("card_selection", "SPOON_STANCE_TIP.description"));

	public static IEnumerable<IHoverTip> Stances() => new IHoverTip[]
	{
		HoverTipFactory.FromPower<Sts1CalmStancePower>(),
		HoverTipFactory.FromPower<Sts1WrathStancePower>(),
		HoverTipFactory.FromPower<Sts1DivinityStancePower>(),
	};
}

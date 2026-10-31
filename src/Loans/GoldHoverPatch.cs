using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.sts2.Core.Nodes.TopBar;

namespace SlaySpoon.Loans;

/// <summary>Hovering the top-bar gold under Merchant Loans also lists what is left to repay and to collect, per player.</summary>
[HarmonyPatch(typeof(NTopBarGold), "OnFocus")]
internal static class GoldHoverPatch
{
	private static bool Prefix(NTopBarGold __instance)
	{
		var run = MerchantLoans.Run;
		if (MerchantLoans.ModifierOf(run) is not { } loans || MerchantLoans.Me(run) is not { } me) return true;
		int index = MerchantLoans.IndexOf(me);
		var lines = new List<string>();
		foreach (var (other, amount) in loans.Ledger.Balances(index, borrower: true)) lines.Add(Line("hover_owe", run!, other, amount));
		foreach (var (other, amount) in loans.Ledger.Balances(index, borrower: false)) lines.Add(Line("hover_owed", run!, other, amount));
		if (lines.Count == 0) return true;
		var tips = new IHoverTip[]
		{
			new HoverTip(new LocString("static_hover_tips", "MONEY_POUCH.title"), new LocString("static_hover_tips", "MONEY_POUCH.description")),
			new HoverTip(LoanPanel.Loc("title"), string.Join("\n", lines)),
		};
		NHoverTipSet.CreateAndShow(__instance, tips)?.SetGlobalPosition(__instance.GlobalPosition + new Vector2(0f, __instance.Size.Y + 20f));
		return false;
	}

	private static string Line(string key, MegaCrit.Sts2.Core.Runs.IRunState run, int other, int amount)
	{
		var text = LoanPanel.Loc(key);
		text.Add("Amount", amount);
		text.Add("Player", MerchantLoans.PlayerAt(run, other) is { } player ? MerchantLoans.NameOf(player) : "?");
		return text.GetFormattedText();
	}
}

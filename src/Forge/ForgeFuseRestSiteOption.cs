using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace SlaySpoon.Forge;

/// <summary>
/// The Forge's rest option: pick the card to keep, then the copy to sacrifice, then confirm on a preview of the result
/// (one card per allowed version). Every step is a vanilla synced choice, so co-op clients replay the same fusion.
/// Backing out at any step cancels the option and spends nothing.
/// </summary>
public sealed class ForgeFuseRestSiteOption(Player owner) : RestSiteOption(owner)
{
	public static string IconImage => ImageHelper.GetImagePath("ui/rest_site/option_smith.png");

	private CardModel? _result;

	public override string OptionId => "SPOON_FORGE_FUSE";
	public override IEnumerable<string> AssetPaths => [IconImage];
	public override bool IsEnabled => ForgeFusion.CanFuseAny(Owner);

	public override LocString Description => new("rest_site_ui", "OPTION_" + OptionId + (IsEnabled ? ".description" : ".descriptionDisabled"));

	private static CardSelectorPrefs Prefs(string key) => new(new LocString("card_selection", key), 1)
	{
		Cancelable = true,
		RequireManualConfirmation = true,
	};

	public override async Task<bool> OnSelect()
	{
		var deck = PileType.Deck.GetPile(Owner).Cards.ToList();
		var kept = (await CardSelectCmd.FromDeckGeneric(Owner, Prefs("SPOON_FORGE_KEEP"), c => ForgeFusion.HasPartner(c, deck)))
			.FirstOrDefault();
		if (kept == null) return false;

		var sacrificed = (await CardSelectCmd.FromDeckGeneric(Owner, Prefs("SPOON_FORGE_SACRIFICE"), c => ForgeFusion.CanFuse(kept, c)))
			.FirstOrDefault();
		if (sacrificed == null) return false;

		var versions = ForgeFusion.Versions(kept, sacrificed);
		var previews = versions.Select(v => ForgeFusion.Build(kept, sacrificed, v)).ToList();
		var picked = await ForgeFusion.Preview(Owner, previews);
		if (picked == null) return false;

		_result = await ForgeFusion.Apply(kept, sacrificed, versions[previews.IndexOf(picked)]);
		return true;
	}

	public override async Task DoLocalPostSelectVfx(CancellationToken ct = default)
	{
		if (_result != null) NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(NCardSmithVfx.Create([_result]));
		await Cmd.CustomScaledWait(1f, 2f, ignoreCombatEnd: false, ct);
	}
}

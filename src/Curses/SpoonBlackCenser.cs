using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>Black Censer (Cursed Deck): whenever a curse joins the holder's deck, it is upgraded. Art: Pixabay (docs/backlog.md).</summary>
public sealed class SpoonBlackCenser : RelicModel
{
	private const string ArtRoot = "res://SlaySpoon/images/relics/spoon/spoon_black_censer";

	public override RelicRarity Rarity => RelicRarity.Event;
	public override string PackedIconPath => ArtRoot + ".png";
	protected override string PackedIconOutlinePath => ArtRoot + "_outline.png";
	protected override string BigIconPath => ArtRoot + "_large.png";

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
	{
		if (card.Owner == Owner && card.Type == CardType.Curse && card.Pile is { Type: PileType.Deck } && card.IsUpgradable)
		{
			Flash();
			CardCmd.Upgrade(card);
		}
		return Task.CompletedTask;
	}
}

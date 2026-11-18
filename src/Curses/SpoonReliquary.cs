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

/// <summary>Reliquary (Cursed Deck): at the end of each combat, 3 gold per curse in the deck. Art: Pixabay (docs/backlog.md).</summary>
public sealed class SpoonReliquary : RelicModel
{
	private const string ArtRoot = "res://SlaySpoon/images/relics/spoon/spoon_reliquary";

	public override RelicRarity Rarity => RelicRarity.Event;
	public override string PackedIconPath => ArtRoot + ".png";
	protected override string PackedIconOutlinePath => ArtRoot + "_outline.png";
	protected override string BigIconPath => ArtRoot + "_large.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new GoldVar(CurseRule.ReliquaryGoldPerCurse) };

	public override async Task AfterCombatVictory(CombatRoom room)
	{
		if (Owner.Creature.IsDead) return;
		int gold = CurseRule.ReliquaryGold(Owner.Deck.Cards.Count(c => c.Type == CardType.Curse));
		if (gold <= 0) return;
		Flash();
		await PlayerCmd.GainGold(gold, Owner);
	}
}

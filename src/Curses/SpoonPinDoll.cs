using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using SlaySpoon.Core;

namespace SlaySpoon.Curses;

/// <summary>Pin Doll (Cursed Deck): at the start of each combat, 1 Strength per 3 curses in the deck. Art: Pixabay (docs/backlog.md).</summary>
public sealed class SpoonPinDoll : RelicModel
{
	private const string ArtRoot = "res://SlaySpoon/images/relics/spoon/spoon_pin_doll";

	public override RelicRarity Rarity => RelicRarity.Event;
	public override string PackedIconPath => ArtRoot + ".png";
	protected override string PackedIconOutlinePath => ArtRoot + "_outline.png";
	protected override string BigIconPath => ArtRoot + "_large.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Curses", CurseRule.PinDollCursesPerStrength), new PowerVar<StrengthPower>(CurseRule.PinDollStrengthPerGroup) };

	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (room is not CombatRoom) return;
		int amount = CurseRule.PinDollStrength(Owner.Deck.Cards.Count(c => c.Type == CardType.Curse));
		if (amount <= 0) return;
		Flash();
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, amount, Owner.Creature, null);
	}
}

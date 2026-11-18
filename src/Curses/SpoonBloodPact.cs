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

/// <summary>Blood Pact (Cursed Deck): whenever the holder draws a curse, gain <see cref="CurseRule.BloodPactBlock"/> Block. Art: Pixabay (docs/backlog.md).</summary>
public sealed class SpoonBloodPact : RelicModel
{
	private const string ArtRoot = "res://SlaySpoon/images/relics/spoon/spoon_blood_pact";

	public override RelicRarity Rarity => RelicRarity.Event;
	public override string PackedIconPath => ArtRoot + ".png";
	protected override string PackedIconOutlinePath => ArtRoot + "_outline.png";
	protected override string BigIconPath => ArtRoot + "_large.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new BlockVar(CurseRule.BloodPactBlock, ValueProp.Unpowered) };

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card.Owner != Owner || card.Type != CardType.Curse) return;
		Flash();
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);
	}
}

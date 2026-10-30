using Godot;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Cards;
using SlaySpoon.Cards.Pools;

namespace SlaySpoon.Characters.Pools;

/// <summary>
/// Every Watcher StS1 card. Reachable only through <see cref="Sts1Watcher"/>: the character is not in
/// ModelDb.AllCharacters, so no vanilla reward, shop, transform or random source sees this pool. Visual members copy
/// the vanilla Necrobinder pool (purple frame, purple energy), the closest StS2 colour to the Watcher's.
/// </summary>
public sealed class Sts1WatcherCardPool : CardPoolModel
{
	public override string Title => "sts1_watcher";

	public override string EnergyColorName => "necrobinder";

	public override string CardFrameMaterialPath => "card_frame_pink";

	public override Color DeckEntryCardColor => new Color("CD4EED");

	public override Color EnergyOutlineColor => new Color("803367");

	public override bool IsColorless => false;

	protected override CardModel[] GenerateAllCards() => Sts1CardTypes.Of<Sts1WatcherCard>();
}

using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using SlaySpoon.Core;

namespace SlaySpoon.Cards;

/// <summary>
/// A Slay the Spire 1 card ported to StS2. Lives in our own StS1 pools (never in ModelDb.AllCardPools, so
/// the card library and vanilla pools stay untouched) and draws its art from our PCK.
/// </summary>
public abstract class Sts1Card : CardModel
{
	protected Sts1Card(int cost, CardType type, CardRarity rarity, TargetType target)
		: base(cost, type, rarity, target)
	{
	}

	/// <summary>How this card relates to its StS2 namesake, if any.</summary>
	public virtual Sts1Relation Relation => Sts1Relation.Unique;

	/// <summary>The StS2 namesake this card replaces or duplicates; null for <see cref="Sts1Relation.Unique"/>.</summary>
	public virtual Type? Sts2Counterpart => null;

	/// <summary>Folder under res://SlaySpoon/images/cards/ holding this character's portraits.</summary>
	protected abstract string PortraitFolder { get; }

	/// <summary>
	/// True when the card has its own StS1 "can't play" line (key <c>&lt;ID&gt;.cantPlay</c> in the cards table), shown
	/// instead of the generic one when its own condition blocks it (<see cref="CantPlayMessagePatch"/>).
	/// </summary>
	public virtual bool HasCantPlayMessage => false;

	public LocString CantPlayMessage => new("cards", Id.Entry + ".cantPlay");

	public override string PortraitPath =>
		$"res://SlaySpoon/images/cards/{PortraitFolder}/{Id.Entry.ToLowerInvariant()}.png";
}

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;
using SlaySpoon.Relics.Events;

namespace SlaySpoon.Events;

/// <summary>
/// StS1's Forgotten Altar: offer the Golden Idol for the Bloody Idol, gain 5 max HP for 25% of max HP, or take Decay.
/// The Humanity option StS1 always showed locked is left out.
/// </summary>
public sealed class Sts1ForgottenAltar : Sts1Event
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("MaxHp", Sts1EventMath.AltarMaxHpGain),
		new DynamicVar("HpLoss", 0m),
	};

	public override Sts1EventKind Kind => Sts1EventKind.ForgottenAltar;

	public override void CalculateVars()
	{
		DynamicVars["HpLoss"].BaseValue = Sts1EventMath.AltarHpLoss(Owner!.Creature.MaxHp);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions() => new EventOption[]
	{
		Owner!.GetRelic<Sts1GoldenIdol>() != null
			? new(this, Offer, "STS1_FORGOTTEN_ALTAR.pages.INITIAL.options.OFFER", HoverTipFactory.FromRelic<Sts1BloodyIdol>())
			: new(this, null, "STS1_FORGOTTEN_ALTAR.pages.INITIAL.options.OFFER_LOCKED", HoverTipFactory.FromRelic<Sts1GoldenIdol>()),
		new EventOption(this, Sacrifice, "STS1_FORGOTTEN_ALTAR.pages.INITIAL.options.SACRIFICE").ThatDoesDamage(DynamicVars["HpLoss"].BaseValue),
		new(this, Desecrate, "STS1_FORGOTTEN_ALTAR.pages.INITIAL.options.DESECRATE", HoverTipFactory.FromCardWithCardHoverTips<Decay>()),
	};

	/// <summary>As in StS1, a player who already holds a Bloody Idol keeps the Golden Idol and gets a Circlet.</summary>
	private async Task Offer()
	{
		Sts1GoldenIdol? idol = Owner!.GetRelic<Sts1GoldenIdol>();
		if (Owner.GetRelic<Sts1BloodyIdol>() != null || idol == null)
		{
			await RelicCmd.Obtain<Circlet>(Owner);
		}
		else
		{
			await RelicCmd.Replace(idol, ModelDb.Relic<Sts1BloodyIdol>().ToMutable());
		}
		SetEventFinished(L10NLookup("STS1_FORGOTTEN_ALTAR.pages.OFFER.description"));
	}

	private async Task Sacrifice()
	{
		await CreatureCmd.GainMaxHp(Owner!.Creature, DynamicVars["MaxHp"].BaseValue);
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["HpLoss"].BaseValue,
			ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		SetEventFinished(L10NLookup("STS1_FORGOTTEN_ALTAR.pages.SACRIFICE.description"));
	}

	private async Task Desecrate()
	{
		await CardPileCmd.AddCurseToDeck<Decay>(Owner!);
		SetEventFinished(L10NLookup("STS1_FORGOTTEN_ALTAR.pages.DESECRATE.description"));
	}
}

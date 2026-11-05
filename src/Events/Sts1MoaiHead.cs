using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SlaySpoon.Core;
using SlaySpoon.Relics.Events;

namespace SlaySpoon.Events;

/// <summary>StS1's Moai Head: lose 12.5% max HP and heal to full, trade the Golden Idol for 333 gold, or leave.</summary>
public sealed class Sts1MoaiHead : Sts1Event
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("MaxHpLoss", 0m),
		new GoldVar(Sts1EventMath.MoaiGold),
	};

	public override Sts1EventKind Kind => Sts1EventKind.MoaiHead;

	public override void CalculateVars()
	{
		DynamicVars["MaxHpLoss"].BaseValue = Sts1EventMath.MoaiMaxHpLoss(Owner!.Creature.MaxHp);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions() => new EventOption[]
	{
		new EventOption(this, Jump, "STS1_MOAI_HEAD.pages.INITIAL.options.JUMP").ThatDecreasesMaxHp(DynamicVars["MaxHpLoss"].BaseValue),
		Owner!.GetRelic<Sts1GoldenIdol>() != null
			? new(this, Offer, "STS1_MOAI_HEAD.pages.INITIAL.options.OFFER", HoverTipFactory.FromRelic<Sts1GoldenIdol>())
			: new(this, null, "STS1_MOAI_HEAD.pages.INITIAL.options.OFFER_LOCKED", HoverTipFactory.FromRelic<Sts1GoldenIdol>()),
		new(this, Leave, "STS1_MOAI_HEAD.pages.INITIAL.options.LEAVE"),
	};

	private async Task Jump()
	{
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars["MaxHpLoss"].BaseValue, isFromCard: false);
		await CreatureCmd.Heal(Owner.Creature, Owner.Creature.MaxHp - Owner.Creature.CurrentHp);
		SetEventFinished(L10NLookup("STS1_MOAI_HEAD.pages.JUMP.description"));
	}

	private async Task Offer()
	{
		if (Owner!.GetRelic<Sts1GoldenIdol>() is { } idol)
		{
			await RelicCmd.Remove(idol);
			await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner);
		}
		SetEventFinished(L10NLookup("STS1_MOAI_HEAD.pages.OFFER.description"));
	}

	private Task Leave()
	{
		SetEventFinished(L10NLookup("STS1_MOAI_HEAD.pages.LEAVE.description"));
		return Task.CompletedTask;
	}
}

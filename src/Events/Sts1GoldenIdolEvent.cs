using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;
using SlaySpoon.Relics.Events;

namespace SlaySpoon.Events;

/// <summary>
/// StS1's Golden Idol: take the idol and escape the boulder (Injury, 25% max HP as damage, or 8% max HP), or leave.
/// A holder of the idol gets a Circlet instead, as in StS1.
/// </summary>
public sealed class Sts1GoldenIdolEvent : Sts1Event
{
	public override Sts1EventKind Kind => Sts1EventKind.GoldenIdolEvent;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Damage", 0m),
		new DynamicVar("MaxHpLoss", 0m),
	};

	public override void CalculateVars()
	{
		int maxHp = Owner!.Creature.MaxHp;
		DynamicVars["Damage"].BaseValue = Sts1EventMath.IdolSmashDamage(maxHp);
		DynamicVars["MaxHpLoss"].BaseValue = Sts1EventMath.IdolHideMaxHpLoss(maxHp);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions() => new EventOption[]
	{
		new(this, Take, "STS1_GOLDEN_IDOL_EVENT.pages.INITIAL.options.TAKE", HoverTipFactory.FromRelic<Sts1GoldenIdol>()),
		new(this, Leave, "STS1_GOLDEN_IDOL_EVENT.pages.INITIAL.options.LEAVE"),
	};

	private async Task Take()
	{
		if (Owner!.GetRelic<Sts1GoldenIdol>() != null)
		{
			await RelicCmd.Obtain<Circlet>(Owner);
		}
		else
		{
			await RelicCmd.Obtain<Sts1GoldenIdol>(Owner);
		}
		SetEventState(L10NLookup("STS1_GOLDEN_IDOL_EVENT.pages.BOULDER.description"), new EventOption[]
		{
			new(this, Outrun, "STS1_GOLDEN_IDOL_EVENT.pages.BOULDER.options.OUTRUN", HoverTipFactory.FromCardWithCardHoverTips<Injury>()),
			new EventOption(this, Smash, "STS1_GOLDEN_IDOL_EVENT.pages.BOULDER.options.SMASH").ThatDoesDamage(DynamicVars["Damage"].BaseValue),
			new EventOption(this, Hide, "STS1_GOLDEN_IDOL_EVENT.pages.BOULDER.options.HIDE").ThatDecreasesMaxHp(DynamicVars["MaxHpLoss"].BaseValue),
		});
	}

	private async Task Outrun()
	{
		await CardPileCmd.AddCurseToDeck<Injury>(Owner!);
		SetEventFinished(L10NLookup("STS1_GOLDEN_IDOL_EVENT.pages.OUTRUN.description"));
	}

	private async Task Smash()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars["Damage"].BaseValue,
			ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		SetEventFinished(L10NLookup("STS1_GOLDEN_IDOL_EVENT.pages.SMASH.description"));
	}

	private async Task Hide()
	{
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars["MaxHpLoss"].BaseValue, isFromCard: false);
		SetEventFinished(L10NLookup("STS1_GOLDEN_IDOL_EVENT.pages.HIDE.description"));
	}

	private Task Leave()
	{
		SetEventFinished(L10NLookup("STS1_GOLDEN_IDOL_EVENT.pages.LEAVE.description"));
		return Task.CompletedTask;
	}
}

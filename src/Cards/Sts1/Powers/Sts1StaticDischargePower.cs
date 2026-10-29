using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Powers;

public sealed class Sts1StaticDischargePower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(StormPower);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
	{
		HoverTipFactory.Static(StaticHoverTip.Channeling),
		HoverTipFactory.FromOrb<LightningOrb>(),
	};

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
		DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != Owner || Owner.IsDead || dealer == null || dealer == Owner ||
			!props.IsPoweredAttack() || result.UnblockedDamage <= 0)
		{
			return;
		}
		Flash();
		int orbs = Amount;
		for (int i = 0; i < orbs; i++)
		{
			await OrbCmd.Channel<LightningOrb>(choiceContext, Owner.Player!);
		}
	}
}

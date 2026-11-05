using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;


namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>
/// Whenever the owner loses HP to its own effects (its cards, Brutality, Combust…), gain Amount Strength at once,
/// on any turn. StS2's Rupture counts any HP loss on your turn instead.
/// </summary>
public sealed class Sts1RupturePower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(RupturePower);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<StrengthPower>() };

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == Owner && dealer == Owner && result.UnblockedDamage > 0)
		{
			Flash();
			await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
		}
	}
}

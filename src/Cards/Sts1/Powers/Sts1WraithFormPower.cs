using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SlaySpoon.Cards.Sts1.Powers;

/// <summary>At the end of its owner's turn, lose Amount Dexterity (vanilla Wraith Form does it at the start).</summary>
public sealed class Sts1WraithFormPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(WraithFormPower);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<DexterityPower>() };

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(Owner))
		{
			Flash();
			await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, -Amount, Owner, null);
		}
	}
}

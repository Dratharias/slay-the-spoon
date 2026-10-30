using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Act4;

/// <summary>StS1 monster regeneration: heals each turn without losing stacks.</summary>
public sealed class Sts1RegenerateMonsterPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	public Type IconSource => typeof(RegenPower);

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(Owner) && !Owner.IsDead)
		{
			Flash();
			await CreatureCmd.Heal(Owner, Amount);
		}
	}
}

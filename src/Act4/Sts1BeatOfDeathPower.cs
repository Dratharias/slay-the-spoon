using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Combat;

namespace SlaySpoon.Act4;

/// <summary>StS1 Heart retaliation after every card; only the card's owner takes the damage in co-op.</summary>
public sealed class Sts1BeatOfDeathPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	public Type IconSource => typeof(GalvanicPower);

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		var target = cardPlay.Card.Owner.Creature;
		if (Owner.IsDead || target.IsDead) return;
		Flash();
		EnemyEffectDamage.Enter(target, out var previous);
		try
		{
			// Unpowered means Strength does not apply, as in StS1 THORNS damage; Block and Monster Damage still do.
			await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Unpowered, null, null);
		}
		finally { EnemyEffectDamage.Exit(previous); }
	}
}

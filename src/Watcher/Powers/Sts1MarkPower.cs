using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Watcher.Powers;

/// <summary>
/// StS1 Mark, put on an enemy by Pressure Points. It never decays; each Pressure Points played makes every marked enemy lose
/// that much HP. It sits on the enemy, so in co-op every player's Pressure Points cashes in the same marks.
/// </summary>
public sealed class Sts1MarkPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(DoomPower);

	/// <summary>Every living marked enemy loses its Mark amount in HP (no Block, no powers).</summary>
	public static async Task TriggerAll(PlayerChoiceContext choiceContext, IEnumerable<Creature> enemies)
	{
		foreach (Creature enemy in enemies.ToList())
		{
			Sts1MarkPower? mark = enemy.GetPower<Sts1MarkPower>();
			if (mark == null || enemy.IsDead)
			{
				continue;
			}
			mark.Flash();
			await CreatureCmd.Damage(choiceContext, enemy, mark.Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		}
	}
}

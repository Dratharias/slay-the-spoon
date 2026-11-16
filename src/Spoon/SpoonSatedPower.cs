using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Spoon;

/// <summary>
/// Sated (Wooden Spoon, cutlery spec): while the Spoon holds it, its Appetite adds no damage, so its next turn hits at
/// base (owner, 2026-11-16); the stacks are kept. Each stack lasts one of its turns: SpoonMonster spends one after each
/// move.
/// </summary>
public sealed class SpoonSatedPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public Type IconSource => typeof(SlowPower);
}

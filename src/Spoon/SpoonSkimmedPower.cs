using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Spoon;

/// <summary>
/// Skimmed (Skimmer, cutlery spec): the Spoon's Banquet eats none of this player's max HP. SpoonMonster removes it from
/// every player at the end of each of its turns.
/// </summary>
public sealed class SpoonSkimmedPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Type IconSource => typeof(BufferPower);
}

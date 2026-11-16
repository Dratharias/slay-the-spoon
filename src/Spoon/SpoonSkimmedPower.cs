using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SlaySpoon.Cards.Sts1.Powers;

namespace SlaySpoon.Spoon;

/// <summary>
/// Skimmed prevents the next Banquet's fixed damage and max HP loss. Persists until that Banquet consumes it.
/// </summary>
public sealed class SpoonSkimmedPower : PowerModel, IBorrowedIcon
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Type IconSource => typeof(BufferPower);
}

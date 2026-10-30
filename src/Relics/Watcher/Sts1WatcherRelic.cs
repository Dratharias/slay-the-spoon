using MegaCrit.Sts2.Core.Models;

namespace SlaySpoon.Relics.Watcher;

public abstract class Sts1WatcherRelic : RelicModel
{
	private string Art => $"res://SlaySpoon/images/relics/watcher/{Id.Entry.ToLowerInvariant()}";
	public override string PackedIconPath => Art + ".png";
	protected override string PackedIconOutlinePath => Art + "_outline.png";
	protected override string BigIconPath => Art + "_large.png";
}

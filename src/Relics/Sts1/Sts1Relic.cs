using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Relics.Sts1;

public abstract class Sts1Relic : RelicModel
{
	private string ArtRoot => $"res://SlaySpoon/images/relics/{Id.Entry.ToLowerInvariant()}";
	public override string PackedIconPath => ArtRoot + ".png";
	protected override string PackedIconOutlinePath => ArtRoot + "_outline.png";
	protected override string BigIconPath => ArtRoot + "_big.png";
	public override RelicRarity Rarity => RelicRarity.Rare;
	public override bool IsAllowed(IRunState runState) => Offered(runState);
	internal static bool Offered(IRunState run) => Sts1RelicRule.Offered(run.GameMode == GameMode.Custom,
		run.Modifiers.Any(m => m is SpoonCardSetSts1Only), run.Modifiers.Any(m => m is SpoonSpireSouvenirModifier));
}

/// <summary>Imported effects only. No source grants these relics until the owner designs their activation.</summary>
public abstract class Sts1InactiveBossRelic : Sts1Relic
{
	// StS2 has no Boss rarity. Event keeps these out of ordinary grab bags without inventing a new rarity.
	public override RelicRarity Rarity => RelicRarity.Event;
	public override bool IsAllowed(IRunState runState) => false;
	public override bool IsAllowedInShops => false;
}

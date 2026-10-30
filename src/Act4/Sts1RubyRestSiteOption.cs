using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>Spends this player's rest action to recall the team's Ruby key.</summary>
public sealed class Sts1RubyRestSiteOption(Player owner) : RestSiteOption(owner)
{
	public override string OptionId => "SPOON_RUBY";
	public override bool IsEnabled => SpoonAct4.Of(Owner.RunState) is { } act4 && !act4.Keys.Has(Act4Key.Ruby);
	public override IEnumerable<string> AssetPaths => [KeySprites.Path(Act4Key.Ruby, large: true)];

	public override Task<bool> OnSelect()
	{
		SpoonAct4.Of(Owner.RunState)?.Keys.Take(Act4Key.Ruby);
		// Concurrent choices can arrive in either order. Both players must spend their action on every peer.
		return Task.FromResult(true);
	}
}

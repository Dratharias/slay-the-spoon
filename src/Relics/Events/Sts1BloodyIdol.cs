using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SlaySpoon.Core;

namespace SlaySpoon.Relics.Events;

/// <summary>StS1's Bloody Idol, the Golden Idol offered at the Forgotten Altar: heal 5 HP whenever you gain gold.</summary>
public sealed class Sts1BloodyIdol : Sts1EventRelic
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new HealVar(Sts1EventMath.BloodyIdolHeal) };

	public override async Task AfterGoldGained(Player player)
	{
		if (player == Owner && !Owner.Creature.IsDead)
		{
			Flash();
			await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);
		}
	}
}

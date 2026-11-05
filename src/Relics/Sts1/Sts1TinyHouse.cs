using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.Relics.Sts1;

public sealed class Sts1TinyHouse : Sts1BossRelic
{
	public override bool HasUponPickupEffect => true;
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new GoldVar(50), new MaxHpVar(5m) };

	public override async Task AfterObtained()
	{
		// StS1 order: upgrade a random card, raise max HP, then a reward screen with the gold, a potion and a card.
		var upgradable = PileType.Deck.GetPile(Owner).Cards.Where(c => c != null && c.IsUpgradable).ToList()
			.StableShuffle(Owner.RunState.Rng.Niche).Take(1);
		foreach (var card in upgradable) CardCmd.Upgrade(card);
		await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars.MaxHp.BaseValue);
		await RewardsCmd.OfferCustom(Owner, new List<Reward>
		{
			new GoldReward(DynamicVars.Gold.IntValue, Owner),
			new PotionReward(Owner),
			new CardReward(CardCreationOptions.ForRoom(Owner, RoomType.Monster), 3, Owner),
		});
	}
}

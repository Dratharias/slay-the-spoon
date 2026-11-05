using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rewards;
using SlaySpoon.Core;
using SlaySpoon.Relics.Events;

namespace SlaySpoon.Events;

/// <summary>
/// StS1's Mind Bloom. I am War: fight a random act 1 boss, with a Rare relic added to that fight's rewards. I am Rich:
/// 999 gold and two Normality. I am Awake: upgrade every card and obtain Mark of the Bloom (no more healing).
/// StS1's I am Healthy only replaces Rich past floor 40, which act 2 never reaches, so it is left out.
/// The fight needs a shared event (StS2 enters event combats for the whole team), so the team votes, and a Souvenir run
/// only shows the event once every player holds the relic (<see cref="Sts1EventRule.IsAllowed"/>).
/// </summary>
public sealed class Sts1MindBloom : Sts1Event
{
	private const string Key = "STS1_MIND_BLOOM.pages.";

	public override Sts1EventKind Kind => Sts1EventKind.MindBloom;

	public override bool IsShared => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Gold", Sts1EventMath.MindBloomGold),
		new DynamicVar("Curses", Sts1EventMath.MindBloomNormalities),
	};

	protected override IReadOnlyList<EventOption> GenerateInitialOptions() => new EventOption[]
	{
		new(this, War, Key + "INITIAL.options.WAR"),
		new(this, Rich, Key + "INITIAL.options.RICH", HoverTipFactory.FromCardWithCardHoverTips<Normality>()),
		new(this, Awake, Key + "INITIAL.options.AWAKE", HoverTipFactory.FromRelic<Sts1MarkOfTheBloom>()),
	};

	private Task War()
	{
		EncounterModel[] bosses = Owner!.RunState.Acts[0].AllBossEncounters.ToArray();
		EncounterModel boss = bosses[Rng.NextInt(bosses.Length)];
		EnterCombatWithoutExitingEvent(boss.ToMutable(), new Reward[] { new RelicReward(RelicRarity.Rare, Owner) },
			shouldResumeAfterCombat: false);
		return Task.CompletedTask;
	}

	private async Task Rich()
	{
		await PlayerCmd.GainGold(Sts1EventMath.MindBloomGold, Owner!);
		for (int i = 0; i < Sts1EventMath.MindBloomNormalities; i++) await CardPileCmd.AddCurseToDeck<Normality>(Owner!);
		SetEventFinished(L10NLookup(Key + "RICH.description"));
	}

	private async Task Awake()
	{
		foreach (CardModel card in PileType.Deck.GetPile(Owner!).Cards.Where(c => c?.IsUpgradable ?? false).ToList())
		{
			CardCmd.Upgrade(card);
		}
		await RelicCmd.Obtain(ModelDb.Relic<Sts1MarkOfTheBloom>().ToMutable(), Owner!);
		SetEventFinished(L10NLookup(Key + "AWAKE.description"));
	}
}

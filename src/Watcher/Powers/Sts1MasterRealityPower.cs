using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Cards.Sts1.Powers;
using SlaySpoon.Cards.Sts1.WatcherTokens;
using SlaySpoon.Core;

namespace SlaySpoon.Watcher.Powers;

/// <summary>StS1 Master Reality: eligible cards the holder creates in combat are upgraded; Status, Curse and Foreign Influence are excluded.</summary>
public sealed class Sts1MasterRealityPower : PowerModel, IBorrowedIcon
{
	private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<CardModel, object> Excluded = new();
	internal static void ExcludeForeignInfluence(CardModel card) => Excluded.GetValue(card, _ => new object());
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public Type IconSource => typeof(ImprovementPower);

	public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
	{
		if (WatcherFidelityRule.MasterReality(creator?.Creature == Owner, card.IsUpgradable,
			card.Type is CardType.Status or CardType.Curse, Excluded.TryGetValue(card, out _)))
		{
			Flash();
			CardCmd.Upgrade(card);
		}
		return Task.CompletedTask;
	}
}

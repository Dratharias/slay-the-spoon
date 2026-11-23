using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Config;
using SlaySpoon.Core;
using SlaySpoon.Midas;
using SlaySpoon.Ui;

namespace SlaySpoon.Forge;

/// <summary>
/// The Forge's merchant service (spec decision 4): once per visit, a +0 copy of a deck card for its rarity price
/// (<see cref="ForgeRule.DuplicatePrice"/>). The button queues an action; every client then runs the same synced deck
/// choice for the buyer, as vanilla does for card removal, so the gold and the new card land everywhere.
/// </summary>
public static class ForgeDuplicate
{
	public static ForgeRarity RarityOf(CardModel card) => card.Rarity switch
	{
		CardRarity.Basic or CardRarity.Common or CardRarity.Curse => ForgeRarity.Common,
		CardRarity.Uncommon => ForgeRarity.Uncommon,
		_ => ForgeRarity.Rare,
	};

	private static bool Inflation => AscensionHelper.HasAscension(AscensionLevel.Inflation);

	public static int PriceOf(SpoonForge forge, Player player, ForgeRarity rarity) =>
		ForgeRule.DuplicatePrice(rarity, Inflation, forge.Purchases(player));

	public static int PriceOf(SpoonForge forge, Player player, CardModel card) => PriceOf(forge, player, RarityOf(card));

	/// <summary>Curses only in Forge + Cursed Deck (priced like a Common card), never Status cards.</summary>
	public static bool Eligible(CardModel card) => card.Type is not (CardType.Curse or CardType.Status)
		|| SlaySpoon.Curses.Forged.ForgedCurses.Fusable(card);

	private static bool Affordable(Player player, int price) => price <= player.Gold
		&& (!SpoonRoiMidas.IsActive(player.RunState) || MidasMelt.CanSpend(player, price));

	public static bool CanBuy(SpoonForge forge, Player player) => !forge.UsedHere(player)
		&& PileType.Deck.GetPile(player).Cards.Any(c => Eligible(c) && Affordable(player, PriceOf(forge, player, c)));

	internal static async Task Run(Player player)
	{
		if (SpoonForge.Of(player.RunState) is not { } forge || forge.UsedHere(player)) return;
		var prompt = new LocString("card_selection", "SPOON_FORGE_DUPLICATE");
		prompt.Add("Common", PriceOf(forge, player, ForgeRarity.Common));
		prompt.Add("Uncommon", PriceOf(forge, player, ForgeRarity.Uncommon));
		prompt.Add("Rare", PriceOf(forge, player, ForgeRarity.Rare));
		var prefs = new CardSelectorPrefs(prompt, 1) { Cancelable = true, RequireManualConfirmation = true };
		var card = (await CardSelectCmd.FromDeckGeneric(player, prefs,
			c => Eligible(c) && Affordable(player, PriceOf(forge, player, c)))).FirstOrDefault();
		if (card == null) return;
		int price = PriceOf(forge, player, card);
		if (!Affordable(player, price)) return;
		await PlayerCmd.LoseGold(price, player, GoldLossType.Spent);
		var copy = card.CardScope!.CreateCard(ModelDb.GetById<CardModel>(card.Id), player);
		await CardPileCmd.Add(copy, PileType.Deck);
		forge.Purchased(player);
		SpoonLog.Info($"forge: duplicated {card.Id.Entry} for {price} gold");
		Changed?.Invoke();
	}

	public static event Action? Changed;

	public static void Request()
	{
		var run = ForgeLevels.CurrentRun;
		if (run == null || LocalContext.GetMe(run) is not { } me) return;
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new ForgeDuplicateAction(me, new NetForgeDuplicate()));
	}
}

public struct NetForgeDuplicate : INetAction, IPacketSerializable
{
	public GameAction ToGameAction(Player player) => new ForgeDuplicateAction(player, this);

	public void Serialize(PacketWriter writer) => writer.WriteBool(true);

	public void Deserialize(PacketReader reader) => reader.ReadBool();

	public override string ToString() => "forge duplicate";
}

public class ForgeDuplicateAction(Player player, NetForgeDuplicate action) : GameAction
{
	public override ulong OwnerId => player.NetId;

	public override GameActionType ActionType => GameActionType.NonCombat;

	protected override Task ExecuteAction() => ForgeDuplicate.Run(player);

	public override INetAction ToNetAction() => action;

	public override string ToString() => $"ForgeDuplicateAction for player {player.NetId}";
}

/// <summary>The merchant room's Duplicate button, under the loan button's spot.</summary>
[HarmonyPatch(typeof(NMerchantRoom), nameof(NMerchantRoom._Ready))]
public static class ForgeDuplicateButtonPatch
{
	public static void Postfix(NMerchantRoom __instance)
	{
		var run = ForgeLevels.CurrentRun;
		if (SpoonForge.Of(run) is not { } forge || LocalContext.GetMe(run!) is not { } me) return;
		var button = NativeTradePopup.Button(__instance.Inventory, Text(forge, me), ForgeDuplicate.Request, disabled: !ForgeDuplicate.CanBuy(forge, me));
		button.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
		button.Position += new Vector2(-340, 130);
		void Refresh()
		{
			if (!GodotObject.IsInstanceValid(button)) return;
			button.SetText(Text(forge, me));
			if (ForgeDuplicate.CanBuy(forge, me)) button.Enable();
			else button.Disable();
		}
		ForgeDuplicate.Changed += Refresh;
		me.GoldChanged += Refresh;
		__instance.TreeExiting += () =>
		{
			ForgeDuplicate.Changed -= Refresh;
			me.GoldChanged -= Refresh;
		};
	}

	private static string Text(SpoonForge forge, Player me) =>
		new LocString("merchant_room", forge.UsedHere(me) ? "SPOON_FORGE_DUPLICATE_USED" : "SPOON_FORGE_DUPLICATE").GetFormattedText();
}

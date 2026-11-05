using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Campfire;

/// <summary>
/// Campfire Gifts: spend this player's rest action to move a deck card into an ally's deck; the ally pays
/// <see cref="CampfireGiftRule.Price"/> gold. The card pick goes through the vanilla deck selector (synchronized), the
/// ally pick through Mend's targeting and one synchronized player choice. Cancelling either step keeps the action.
/// </summary>
public sealed class SpoonGiftRestSiteOption(Player owner) : RestSiteOption(owner)
{
	public const string Id = "SPOON_GIFT";

	/// <summary>Mend's icon: the gift is Mend's sibling, aimed at an ally.</summary>
	public static string IconImage => ImageHelper.GetImagePath("ui/rest_site/option_mend.png");

	private LocString? _description;

	public override string OptionId => Id;

	public override IEnumerable<string> AssetPaths => [IconImage];

	public override LocString Description
	{
		get
		{
			if (_description == null)
			{
				_description = base.Description;
				_description.Add("HasTarget", variable: false);
				_description.Add("Name", "");
				_description.Add("Price", CampfireGiftRule.Price);
			}
			return _description;
		}
	}

	public override bool IsEnabled => Giftable(Owner).Any() && Owner.RunState.Players.Any(p => CanReceive(p));

	private bool CanReceive(Player? player) =>
		player != null && CampfireGiftRule.CanReceive(player == Owner, !player.Creature.IsDead, player.Gold);

	private static IEnumerable<CardModel> Giftable(Player player) =>
		PileType.Deck.GetPile(player).Cards.Where(IsGiftable);

	private static bool IsGiftable(CardModel card) => CampfireGiftRule.CanGift(card.Type.ToString(), card.IsRemovable);

	public override async Task<bool> OnSelect()
	{
		var prompt = new LocString("card_selection", "SPOON_TO_GIFT");
		var prefs = new CardSelectorPrefs(prompt, 1) { Cancelable = true };
		CardModel? card = (await CardSelectCmd.FromDeckGeneric(Owner, prefs, IsGiftable)).FirstOrDefault();
		if (card == null) return false;

		uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(Owner);
		Player? target;
		if (LocalContext.IsMe(Owner))
		{
			target = await PickAlly();
			RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(Owner, choiceId, PlayerChoiceResult.FromPlayerId(target?.NetId));
		}
		else
		{
			ulong? id = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(Owner, choiceId)).AsPlayerId();
			target = id.HasValue ? Owner.RunState.GetPlayer(id.Value) : null;
		}
		Description.Add("HasTarget", variable: false);
		NRestSiteRoom.Instance?.GetButtonForOption(this)?.RefreshTextState();
		// Every peer runs this with the same choices; a target that can no longer pay cancels on all of them.
		if (!CanReceive(target) || card.Pile?.Type != PileType.Deck) return false;

		var copy = CardModel.FromSerializable(card.ToSerializable());
		await CardPileCmd.RemoveFromDeck(card);
		target!.RunState.AddCard(copy, target);
		await CardPileCmd.Add(copy, PileType.Deck);
		await PlayerCmd.LoseGold(CampfireGiftRule.Price, target, GoldLossType.Spent);
		SpoonLog.Info($"campfire: {Owner.NetId} gave {copy.Id.Entry} to {target.NetId} for {CampfireGiftRule.Price} gold");
		return true;
	}

	/// <summary>Mend's targeting, limited to allies who can pay.</summary>
	private async Task<Player?> PickAlly()
	{
		if (NRestSiteRoom.Instance is not { } room || room.GetButtonForOption(this) is not { } button) return null;
		room.AnimateDescriptionDown();
		Vector2 start = button.GlobalPosition + button.Size / 2f;
		bool controller = NControllerManager.Instance?.IsUsingController == true;
		NTargetManager targets = NTargetManager.Instance;
		targets.StartTargeting(TargetType.AnyPlayer, start, controller ? TargetMode.Controller : TargetMode.ClickMouseToTarget,
			ShouldCancel, node => CanReceive(NodeToPlayer(node)));
		List<NRestSiteCharacter> allies = room.characterAnims.Where(c => CanReceive(c.Player)).ToList();
		if (controller)
		{
			for (int i = 0; i < allies.Count; i++)
			{
				Control hitbox = allies[i].Hitbox;
				hitbox.SetFocusMode(Control.FocusModeEnum.All);
				hitbox.FocusNeighborTop = hitbox.GetPath();
				hitbox.FocusNeighborBottom = hitbox.GetPath();
				hitbox.FocusNeighborLeft = allies[(i + allies.Count - 1) % allies.Count].Hitbox.GetPath();
				hitbox.FocusNeighborRight = allies[(i + 1) % allies.Count].Hitbox.GetPath();
			}
			allies.FirstOrDefault()?.Hitbox.TryGrabFocus();
		}
		var hovered = Callable.From<Node>(OnHovered);
		var unhovered = Callable.From<Node>(OnUnhovered);
		targets.Connect(NTargetManager.SignalName.NodeHovered, hovered);
		targets.Connect(NTargetManager.SignalName.NodeUnhovered, unhovered);
		try
		{
			return NodeToPlayer(await targets.SelectionFinished());
		}
		finally
		{
			targets.Disconnect(NTargetManager.SignalName.NodeHovered, hovered);
			targets.Disconnect(NTargetManager.SignalName.NodeUnhovered, unhovered);
			if (controller)
			{
				foreach (NRestSiteCharacter character in room.characterAnims)
					character.Hitbox.SetFocusMode(Control.FocusModeEnum.None);
			}
			if (GodotObject.IsInstanceValid(room)) room.AnimateDescriptionUp();
		}
	}

	private void OnHovered(Node node)
	{
		if (NodeToPlayer(node) is not { } player) return;
		Description.Add("HasTarget", variable: true);
		Description.Add("Name", PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, player.NetId));
		NRestSiteRoom.Instance?.GetButtonForOption(this)?.RefreshTextState();
	}

	private void OnUnhovered(Node _)
	{
		Description.Add("HasTarget", variable: false);
		NRestSiteRoom.Instance?.GetButtonForOption(this)?.RefreshTextState();
	}

	private static Player? NodeToPlayer(Node? node) => node switch
	{
		NMultiplayerPlayerState state => state.Player,
		NRestSiteCharacter character => character.Player,
		_ => null,
	};

	private static bool ShouldCancel() => NOverlayStack.Instance?.ScreenCount > 0 || NCapstoneContainer.Instance?.InUse == true;
}

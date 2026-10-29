using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using SlaySpoon.Cards.Pools;
using SlaySpoon.Cards;

namespace SlaySpoon.DevConsole;

/// <summary>
/// Console command <c>sts1card</c>: the vanilla <c>card</c> command only searches <c>ModelDb.AllCards</c>, which
/// leaves out the hidden StS1 pools. Same behaviour, StS1 cards only, so play-testers can spawn any of them.
/// </summary>
public sealed class Sts1CardConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "sts1card";

	public override string Args => "<card-id:string> [pileName:string]";

	public override string Description => "Spawns a Slay the Spire 1 card into a pile (hand in combat, deck otherwise), e.g. 'sts1card STS1_HEAVY_BLADE'.";

	public override bool IsNetworked => true;

	private static IEnumerable<CardModel> Sts1Cards => Sts1CardTypes.Of<Sts1Card>();

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length == 0)
		{
			return new CmdResult(success: false, "No card name specified.");
		}
		if (!RunManager.Instance.IsInProgress || issuingPlayer == null)
		{
			return new CmdResult(success: false, "A run is currently not in progress!");
		}
		// Out of combat there is no hand: default to the deck instead of throwing like the vanilla command.
		PileType pile = CombatManager.Instance.IsInProgress ? PileType.Hand : PileType.Deck;
		if (args.Length >= 2 && !TryParseEnum(args[1], out pile))
		{
			return new CmdResult(success: false, "Unknown pile '" + args[1] + "'. Valid piles: " + string.Join(", ", Enum.GetNames<PileType>()));
		}
		if (pile == PileType.Hand && PileType.Hand.GetPile(issuingPlayer).Cards.Count >= CardPile.MaxCardsInHand)
		{
			return new CmdResult(success: false, "The hand is full.");
		}
		string cardName = args[0].ToUpperInvariant();
		CardModel? canonical = Sts1Cards.FirstOrDefault(c => c.Id.Entry == cardName);
		if (canonical == null)
		{
			return new CmdResult(success: false, "Card '" + cardName + "' not found");
		}
		ICardScope? scope = pile.IsCombatPile()
			? CombatManager.Instance.DebugOnlyGetState()
			: RunManager.Instance.DebugOnlyGetState();
		if (scope == null)
		{
			return new CmdResult(success: false, "No combat is in progress for pile '" + pile + "'.");
		}
		CardModel card = scope.CreateCard(canonical, issuingPlayer);
		return new CmdResult(CardPileCmd.Add(card, pile), success: true, $"Added card '{canonical.Id.Entry}' to '{pile}'");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			return CompleteArgument(Sts1Cards.Select(c => c.Id.Entry), Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		if (args.Length == 2)
		{
			return CompleteArgument(Enum.GetNames<PileType>(), new[] { args[0] }, args[1]);
		}
		return base.GetArgumentCompletions(player, args);
	}
}

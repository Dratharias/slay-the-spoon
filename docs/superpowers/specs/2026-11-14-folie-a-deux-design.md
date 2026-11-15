# Folie à deux — design

Status: design agreed with the owner (2026-11-14), not planned or implemented yet. Tracked in `docs/backlog.md`.

## Intent

A multiplayer game mode built to sow discord. Every card a player would keep goes into a shared pot at a treasure
chest; the players bid on each other's rewards, and ties are fought with the vanilla rock-paper-scissors. The owner of a
reward has no priority: to keep their own cards they fight for them like everyone else.

The name is **"Folie à deux" in both English and French** (not translated).

## Goals

- A fourth card on the multiplayer host screen (`NMultiplayerHostSubmenu`, beside Standard, Daily and Custom), with the
  colorless **Madness** card art as its background. It starts a **Standard run carrying a hidden `SpoonFolieADeux` flag**,
  the same way Act 4 rides into Standard (`src/Act4/StandardRunModifiersPatch.cs`). Ascension, unlocks and stats count as
  Standard. This is a new owner-approved Standard exception (after Spire Souvenir, Act 4 and the solo Watcher): the
  plain Standard card stays vanilla.
- The same rule is a **Custom Run checkbox** (`SpoonFolieADeux` modifier).
- **Every card reward is bid on.** No source a player keeps a card from bypasses the chest.

## Non-goals

- Cards created during combat (potions, card effects that put a card in hand) and transforms (no choice) are not bid on.
- Gold, potion and relic rewards are unchanged; only cards go through the chest.
- Players do not choose their rock-paper-scissors move: it is drawn at random and animated, as in vanilla.
- Daily stays vanilla.

## Anti-goals

- A player left with an empty or crippled starting deck (the consolation rule below exists for this).
- A co-op desync: the mode lives on the network, so it must reuse the game's synchronized chest flow, not a new one.

## The rule

1. When a card source fires, a chest opens for the whole team with the relic chest's opening animation.
2. Above the chest, one **head per player** (the character head icons, e.g. Ironclad, Silent, Ironclad). Under each head,
   the **list of cards** at stake for that player.
3. Each player **locks one bid** on one head (their own included). The lock is final and **visible**: the bidders'
   markers show under the head they picked, so later bidders see where the others went. Nobody abstains at this step.
4. When **every player has locked**, the duels start: a head with one bidder goes to them; a head with several goes to a
   random rock-paper-scissors fight (vanilla `RelicPickingResult.GenerateRelicFight`); a head with no bidder is lost.
5. Losers get nothing, **except for deck-clearing sources** (below).
6. What the winner does depends on the source:
   - **Combat rewards and Draft rounds:** the stake is the whole reward (3 cards); the winner picks one card or skips
     (skip loses it, like a vanilla skip; Draft rounds keep vanilla's no-skip).
   - **Event card choices and Sealed Deck:** the player chooses first, as in vanilla (the card, or the 10-card deck);
     what they chose is the stake.
7. The vanilla rewards screen (gold, potions, relics) follows, without the card reward.

Cards keep their own character: a Silent reward holds Silent cards, whoever wins it. The Hybrid version swap button
(StS1/StS2) still works on what is shown.

### Deck-clearing sources: consolation

Sealed Deck and Draft clear the starting deck (`ClearsPlayerDeck`). For them the vanilla consolation prize stays:
losers receive the stakes nobody won, so every player ends with exactly one stake per chest.

- **Sealed Deck:** each player builds their 10-card deck from their 30 cards; then the chest shows the built decks and
  the team bids on them. The decks are redistributed as a permutation: everyone leaves with exactly one deck (an
  Ironclad may play a Silent deck). Losers take the unclaimed decks, by rock-paper-scissors when several losers remain
  (vanilla consolation order).
- **Draft:** each of the 10 rounds is a chest; losers take the unbid rewards, so every player ends with 10 cards.

### Sources in scope

Everything that offers a card to keep, including: combat rewards (normal, elite, boss), event card offers (e.g.
Colorful Philosophers, Trial, Brain Leech, The Future of Potions, Crystal Sphere), relics that grant card rewards
(Orrery, Prayer Wheel, Dream Catcher, Glass Eye, Lost Coffer, White Star, Kaleidoscope…), The Hunt, Neow/Custom starting
modifiers that offer cards (Draft, Sealed Deck), and the mod's own card offers (StS1 events, Mind Bloom, etc.). The
builder inventories the full list from `new CardReward(` and `CardSelectCmd.FromSimpleGridForRewards` call sites.

## Approaches

- **A. Reuse the vanilla relic chest synchronizer (chosen by the owner).** `TreasureRoomRelicSynchronizer` already does
  one final, visible vote per player, random rock-paper-scissors on ties and consolation for losers. Replace the relic
  slots with player stakes (head + card list), keep its network sync, vote hands, fight animation and chest opening;
  drop the consolation outside deck-clearing sources. Pure rules (stake resolution, consolation permutation) live in
  `src/Core/` with xUnit tests. Most reliable in co-op: this code already runs in multiplayer and the mod already bends it
  for the Sapphire chest rematch (`src/Act4/ChestRematchPatch.cs`, `ChestRematchRule`).
- **B. Own screen and own sync** (like Campfire Gifts), reusing only visuals. Full freedom, but votes, disconnects and
  message order would be rebuilt from scratch, untestable in co-op without the owner. Rejected.
- Rerouting the existing rewards screen was dropped: it only shows the local player's cards.

## Constraints

- English and French text for every string the mode adds (`localization/eng/` and `fra/`); the title stays
  "Folie à deux" in both.
- Never write the regular save. Standard stays selectable and vanilla through its own card.
- Seeds: rolls the mode adds (fight moves, consolation order) use the synchronizer's own RNG or a stream seeded from the
  run seed, identical on every client; vanilla reward generation is untouched.
- `ChestRematchPatch` (Sapphire) prefixes the same synchronizer: the two must recognise each other so a real treasure
  chest keeps its Sapphire behaviour and a Folie à deux chest never triggers the rematch.
- The vanilla synchronizer throws past its slot count and the treasure room scene has slots for up to 4 relics (the mod
  added a fifth for the Sapphire key); 4 players means 4 stakes.

## Left to the builder

- Solo: the host card is multiplayer only; in a solo Custom Run the checkbox has no effect (or is hidden). Builder call.
- Event timing in co-op: the chest waits until every player can join it (other players finish their current screen);
  how a per-player event pauses the others is the builder's call.
- Where the chest is shown outside a treasure room (combat end, event, Neow): an overlay over the current room, like
  `src/Ui/ChestOpeningOverlay.cs` for Starting Chest.
- Madness art path, head icon source (the multiplayer player head icons), card list layout under each head for 3 cards
  and for a 10-card deck.
- Interaction with the Hybrid reward swap (`RewardSwapPatch`) and Starting Chest / Neow's Blessings.
- An in-game checklist in `docs/testing/` (co-op sync first).

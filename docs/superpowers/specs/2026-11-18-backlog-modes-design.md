# Backlog modes — design

Owner goal (2026-11-18): "Implémente tous les modes de jeu du backlog". The owner picked seven modes in the mode
brainstorm (`docs/backlog.md`, "Future Custom Run modes picked by the owner"). Under that goal the builder makes the
calls below and documents them; the owner reviews them when they play. Each mode is one Custom Run modifier, off by
default, Custom Run only (Standard and Daily untouched), with EN/FR text and its pure rules in `src/Core/`.

The owner's compatibility rule applies: every pair of a new mode with an existing mode is listed in
"Compatibility" with its decision (combine, order, or mutually exclusive).

## Mutation (`SpoonMutation`)

- When act 2 and every later act start (`AfterActEntered`, act index ≥ 1; appended acts count too), every deck card
  that is not upgraded and that the game lets transform (`IsTransformable`: Eternal cards stay) is transformed at
  random, like the vanilla transform events (`CardCmd.Transform`, the transform pool and preview).
- RNG: one stream per act and player slot from the run seed (`MutationRule.Stream`), so seeds stay vanilla.
- The last mutated act is saved (`SpoonMutationAct`): a reload at the start of an act never mutates twice.
- Builder call: "at the end of each act" is read as "when the next act starts", so the last act never mutates and
  the player sees the result on the new act's map.

## Deck of 10 (`SpoonDeckOfTen`, relic `SpoonDeckOfTenRelic`)

- Owner rework (2026-11-18): the rule lives in a relic, **Deck of 10**. The Custom Run mode gives it to every player at
  the start (kept out of every grab bag); in a Standard run Neow can offer it (Standard exception, below).
- The holder's deck holds at most 10 cards (`DeckOfTenRule.Limit`); each dropped card gives +1 max HP (and heals 1, like
  any max HP gain). Players without the relic are unaffected.
- The first trim uses the Sealed Deck grid: the player picks the cards to **keep** (not skippable, manual confirmation,
  Sealed Deck's rarity/title order). It runs when the relic is obtained, or for the mode, when the first room is entered.
  Later trims (a card joins a full deck) pick the cards to drop on the vanilla removal screen, exactly the excess; the
  new card may be the one dropped.
- Neow (Standard runs, and wherever Neow draws its vanilla options): the cursed option is replaced by Deck of 10 one time
  in 11 (`DeckOfTenRule.NeowOdds`: an equal share with the 10 vanilla cursed relics), rolled on the mod's own stream from
  the run seed and the player slot after vanilla drew its options, so vanilla seeds don't change. Never offered to a
  player who already holds it.
- Builder calls:
  - Neow's cursed slot rather than a fourth option: the relic is a trade (lose cards, gain max HP), like the cursed relics.
  - The keep grid only for the first trim: picking 10 cards to keep is right for a whole deck, but tedious when one card
    goes over.
  - Unremovable cards (Eternal...) count toward the 10 and are not shown in either screen. If the excess can't be met,
    the deck stays above 10.
  - A card that joins during combat is checked when combat ends.
  - Curses count, so a curse added to a full deck is a free removal at the cost of another card: kept, it's the trade.
  - Icon borrowed from Precarious Shears.

## Blood (`SpoonBlood`)

- Gold no longer exists: the player's gold is set to 0 at the start and every gold gain is 0 (combat gold rewards
  are dropped).
- Every gold price is paid in current HP at a fixed rate: 1 HP = 5 gold (`BloodRule.Rate`). The game reads a Blood
  player's gold as `(current HP − 1) × 5`, so every purchase check (merchant, card removal, events, campfire gifts,
  campfire sales, Forge duplicate) works unchanged, and paying N gold costs `ceil(N / 5)` HP. Payments never kill:
  the spendable gold keeps 1 HP.
- Gold losses that aren't payments (thieves, events that "lose all gold") cost HP the same way.
- The vampirism relic: **Blood Fang** (`SpoonBloodFang`, given to every player at the start, kept out of every grab
  bag): when an attack you play deals unblocked damage, heal 20 % of it, rounded down (`BloodRule.Leech`).
- Builder call: the top bar gold shows the spendable gold equivalent.

## Trophies (`SpoonTrophies`)

- When an elite fight is won, each player gets one **Trophy** card: an Attack built from one attack move of one elite in
  the fight, chosen on a seeded RNG (run seed, floor, player slot).
- The card is named "<Monster>: <Move>" from the game's monster and move titles (EN/FR come from the game). It deals the
  move's damage at the run's current ascension, as many times as the move hits.
- Its cost is 1 when the total damage is under 15, 2 under 30, else 3 (`TrophyRule.Cost`). Its upgrade adds 25 % damage,
  rounded up.
- One card class (`SpoonTrophyCard`) carries the monster, move, damage and hits as saved properties.
- Builder calls:
  - Elites with no attack move give a 1-cost, 10-damage "<Monster>: Trophy".
  - The moves and their damage are read when the fight starts (`BeforeCombatStart`), so the card shows the damage the
    move had at the start of the fight, before any Strength the elite gains.

## Forced Trade (`SpoonForcedTrade`, co-op)

- After each combat won, before the rewards, each player picks one card of their deck (synced removal screen,
  not skippable, removable cards only), and it moves to a random other player (seeded: run seed, floor, slot).
- Solo: the mode does nothing.
- Builder calls:
  - The giver picks the card, the receiver is random; a player with no removable card gives nothing.
  - Moving a card keeps its upgrade and enchantment (the Campfire Gifts move).

## Conductor (`SpoonConductor`, co-op)

- After each combat won, one player, drawn on a seeded RNG and never the previous one when there are two or more
  players, is the Conductor.
- Every player's rewards are handed out by the Conductor: for each card, potion and relic reward, the Conductor picks
  which player receives it.
- Builder calls:
  - The recipient pick is a synced player choice, shown as a small panel of player names.
  - Card rewards keep each original owner's character pool: the cards are the ones that player would have been offered.
  - Solo: the mode does nothing.
  - As built: the Conductor assigns every reward up front, one small panel per reward, then each player's own vanilla
    reward screen opens with what they received. One merged screen would have meant rewriting the reward screen.
  - Gold is not redistributed: every player already gets the same amount, so splitting it changes nothing.
  - A relic the recipient already holds stays with its original owner.

## Useless Relics (`SpoonUselessRelics`)

- In combat, a relic that is spent for the fight can be thrown. Clicking it starts targeting (like a potion), and the
  chosen enemy takes 10 unblockable, unpowered damage.
- A thrown relic is greyed out (Disabled status, the melted-wax tint) and stops working until the combat ends, then it
  comes back.
- "Spent" is a hand list in `src/Core/UselessRelicRule.cs`: relics whose combat effect happens once per fight (start of
  combat, first turn, first attack...). They count as spent once the relic has flashed this combat.
- Co-op: the throw is a synced action (like a potion use).
- Builder calls:
  - A hand list rather than a generic signal. The game has no "used up" flag; most once-per-fight relics check the
    turn number. The list: start-of-combat relics (Akabeko, Anchor, Bag of Marbles, Lantern...) and "first time in a
    fight" relics (Centennial Puzzle, Throwing Axe, Vambrace...).
  - A thrown relic leaves the hook listener lists (combat and run) until the combat ends, so it stops working; it is
    not melted (melting is saved and tints the relic red).
  - The throw is a play-phase action, like a potion; the relic's inspect screen opens as usual when it can't be thrown.

## Compatibility

| New mode | Existing mode | Decision |
|----------|---------------|----------|
| Mutation | The Forge | Combine: forged cards are upgraded and stay. |
| Mutation | Hybrid / StS1-only | Combine: the transform pool follows the card set mode. |
| Mutation | Spoon | Combine: the Spoonful is Eternal and stays. |
| Deck of 10 | The Forge | Combine: fusing removes a card, the Duplicate adds one, then the deck is trimmed. |
| Deck of 10 | Matriochka | Combine: every floor's card reward forces a drop. |
| Deck of 10 | Folie à deux, Campfire Gifts, Forced Trade | Order: the card moves first, then the receiver trims. |
| Deck of 10 | King Midas | Combine (Midas gives no cards). |
| Blood | Merchant Loans | Mutually exclusive: loans lend gold that Blood erases. |
| Blood | King Midas | Mutually exclusive: Midas is a gold economy. |
| Blood | Spoon | Combine: HP is both the price and the threshold. |
| Trophies | Folie à deux | Order: Trophies are given straight to each player and are not bid on. |
| Trophies | King Midas | Combine: Trophies are not card rewards. |
| Forced Trade | Folie à deux | Order: the trade happens at combat victory, before the chests. |
| Forced Trade | Campfire Gifts | Combine. |
| Conductor | Folie à deux | Mutually exclusive: both own the reward screen. |
| Conductor | King Midas | Combine (no card rewards to hand out). |
| Conductor | Matriochka | Mutually exclusive: Matriochka's unskippable per-player card rewards. |
| Useless Relics | every mode | Combine: combat-only. |

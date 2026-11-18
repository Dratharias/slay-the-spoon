# Cursed Deck — design

Owner idea (2026-11-18): a Custom Run mode where curses are a resource. The deck fills with curses, the player plays
them, and the relics on offer are mostly cursed ones that pay off for carrying curses.

## Owner decisions (2026-11-18)

- No StS1 relics by default (no Du-Vu Doll, no Blue Candle).
- Every player starts with the **Unholy Grimoire** (`SpoonUnholyGrimoire`, already built: 5 damage to every enemy per
  curse exhausted). In this mode it also makes curses **playable for 1 energy**; a played curse is exhausted, so it
  triggers the Grimoire.
- The Grimoire also **turns the curse's own effect against the enemy**, when it has one: Bad Luck makes the monster lose
  13 HP; a curse with no effect (Clumsy) only deals the Grimoire's 5 damage. Table below.
- **Upgraded curses** exist in this mode (the game caps every curse at +0). The Black Censer upgrades a curse.
- Every "?" event gets **one extra option**: take a random curse in exchange for a reward. The event's own options don't
  change.
- Relic offers are **2 in 3 cursed**: each relic drawn for a chest, elite or merchant is a cursed relic 2 times in 3,
  from the cursed pool below. The pool gets **4 custom relics**. Once a player holds every cursed relic, the draws are
  vanilla.

## The cursed pool

The game's 7: Calling Bell, Distinguished Cape, Neow's Bones, Cursed Pearl, Blood-Soaked Rose, Preserved Fog (Ancient
rarity) and Darkstone Periapt (Event rarity). Their pickup effects run as usual (Calling Bell's curse and 3 relics, etc.).

Custom relics (proposal, art from Pixabay like the Grimoire):

| Relic | Effect |
|-------|--------|
| **Blood Pact** (Pacte de sang) | Whenever you draw a curse, gain 4 Block. |
| **Pin Doll** (Poupée d'épingles) | At the start of each combat, gain 1 Strength for every 3 curses in your deck. |
| **Black Censer** (Encensoir noir) | Whenever you obtain a curse, upgrade it. |
| **Reliquary** (Reliquaire) | At the end of each combat, gain 3 gold for each curse in your deck. |

## Curses against the enemy (owner decisions 2026-11-18)

Played with the Grimoire, a curse **targets one enemy**: it is exhausted (the Grimoire's damage to ALL enemies), then its
effect hits the chosen enemy only. Left in hand at end of turn, it hits the player as in vanilla.

An upgraded curse is stronger **on both sides**: Bad Luck+ costs the player 20 HP if it stays in hand, and deals −20 HP to
the enemy when played. A curse with no effect gets nothing on either side but its Grimoire damage goes to **12** when
upgraded (5 otherwise). Curses with an effect keep the Grimoire at 5.

| Curse | Left in hand (you) | Played (chosen enemy) | Upgraded (+), both sides |
|-------|--------------------|-----------------------|--------------------------|
| Bad Luck | lose 13 HP | loses 13 HP | 20 |
| Decay | take 2 damage | takes 2 damage | 4 |
| Regret | lose 1 HP per card in hand | loses 1 HP per card in your hand | 2 per card |
| Doubt | gain 1 Weak | gains 1 Weak | 2 |
| Shame | gain 1 Frail | gains 1 Vulnerable (Frail does little to monsters) | 2 |
| Debt | lose 10 gold | you gain 10 gold (the monster pays) | 15 |
| Normality, Clumsy, Injury, Writhe, Guilty, Enthralled | vanilla | — | Grimoire 12 |
| Greed, Folly, Curse of the Bell, Poor Sleep, Spore Mind, Ascender's Bane | vanilla | — | Grimoire 12 |

- An upgraded curse costs 0 to play (1 otherwise).
- Curses upgrade like other cards in this mode (Smith, Armaments, Apotheosis...), at most +1; the Black Censer upgrades
  every curse that joins the deck. The title gets the usual "+".

The full list, reviewed curse by curse like the Forge's cards: `data/curses/curses.json` (hand table, read at
runtime), `tools/curse_table.py` (checks every curse of the game and the mod has a row, writes the owner page
`docs/review/cursed-curses.md`). Notable rows: Pain (StS1) hits the enemy for 1 HP per card played this turn;
Normality, Enthralled and Spore Mind have no played effect; the Necronomicurse keeps cost 1 upgraded (at 0 it would loop
forever, since it returns to the hand when exhausted).

## Builder calls (proposal)

- Mode: `SpoonCursedDeck`, Custom Run only, off by default; Standard and Daily untouched. Title "Cursed Deck" /
  "Deck maudit".
- The Grimoire in other runs (console, future sources) keeps only its damage effect; curses become playable only in
  this mode.
- Start: the Grimoire and **1 random curse**, so the Grimoire works from the first fight.
- Playable curses: the Unplayable keyword is ignored for the Grimoire holder's curses, their cost becomes 1 (0 when
  upgraded) and they target one enemy; playing one exhausts it, then the Grimoire's damage and the curse's enemy effect. Its end-of-turn effect (Regret, Decay...)
  doesn't happen if it left the hand. Exhausted curses come back next combat, like any exhausted card.
- Event option "Embrace the curse": add 1 random curse, then one reward drawn on a seeded stream (run seed, floor,
  player slot): a cursed relic not held (if any are left), 75 gold, or a heal of 25 % of max HP. Ancients (Neow,
  the act Ancients) don't get the option. The added option comes last, before any "Leave" option.
- The 2-in-3 roll uses the mod's own stream, so vanilla draws stay in their order (no seed drift for the other third).
- Random curses come from the game's curse pool (the curses vanilla events give), never Ascender's Bane-style special
  curses.

## Compatibility

| Mode | Decision |
|------|----------|
| Deck of 10 | Combine: curses take slots; dropping them is a free way out, at the cost of the Grimoire's fuel. |
| King Midas | Combine: Midas events also get the option; Midas relics stay gold. |
| Blood | Combine: the event's gold reward is replaced by the heal. |
| Folie à deux | Combine: cursed relics replace relic draws before the chests. |
| The Forge | Combine: curses never fuse. |
| Every other mode | Combine. |

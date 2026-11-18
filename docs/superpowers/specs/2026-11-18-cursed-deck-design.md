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

## Curses against the enemy (proposal)

Played with the Grimoire, a curse **targets one enemy** (owner decision 2026-11-18): it is exhausted (5 damage to ALL
enemies), then its effect hits the chosen enemy.
The player-side effect is the vanilla one, untouched (it happens only if the curse stays in hand at end of turn).

| Curse | Vanilla (on you) | Against the enemy | Upgraded (+) |
|-------|------------------|-------------------|--------------|
| Bad Luck | lose 13 HP | loses 13 HP | 20 |
| Decay | take 2 damage | takes 2 damage | 4 |
| Regret | lose 1 HP per card in hand | loses 1 HP per card in your hand | 2 per card |
| Doubt | gain 1 Weak | gains 1 Weak | 2 |
| Shame | gain 1 Frail | gains 1 Vulnerable (Frail does little to monsters) | 2 |
| Debt | lose 10 gold | you gain 10 gold (the monster pays) | 15 |
| Normality | max 3 cards this turn | — | Grimoire +5 |
| Clumsy, Injury, Writhe, Guilty, Enthralled | — | — | Grimoire +5 |
| Greed, Folly, Curse of the Bell, Poor Sleep, Spore Mind, Ascender's Bane | (relic/event curses) | — | Grimoire +5 |

- An upgraded curse costs 0 and its enemy number goes up (column "+"); a curse with no enemy effect deals the Grimoire's
  damage +5 when upgraded (10). The player-side number doesn't change (owner decision 2026-11-18: upgrades are on the
  enemy side only).
- Curses upgrade like other cards in this mode (Smith, Armaments, Apotheosis...), at most +1; the Black Censer upgrades
  every curse it sees join the deck. The title gets the usual "+".

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

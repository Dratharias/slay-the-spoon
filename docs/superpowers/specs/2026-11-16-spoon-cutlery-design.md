# The Spoon's cutlery — short design (2026-11-16)

Amends `2026-11-14-spoon-design.md` §5 (the fight). Owner decisions unless marked *builder call*.

## Why

In play tests the fight was spent blocking: the Spoon's damage scaled three ways (team max HP ratio, Strength from
growth, Appetite), and nothing in a normal deck answers its mechanics. The owner wants a kit of cutlery cards, each one
countering one of the Spoon's mechanics, handed out by the Spoon itself so ignoring it gets out of hand.

## Approaches

- **A. Fight-only kit (chosen).** The Spoon adds cutlery to the fighters' draw piles during the fight; combat-only
  cards, nothing reaches the deck. Simple to balance, tested at once with "Test: The Spoon".
- B. Run-long kit: cutlery in rewards, merchant, Feast. Richer, much more work; may come later.

## The cycle (4 turns, repeating)

1. **Sets the table** (*builder call: title*): Frail 2 (vanilla `FrailPower`) on every fighter (*builder call: 2*),
   and the cutlery kit shuffled into every player's draw pile, **every cycle** (unplayed cards pile up: the deck gets
   unmanageable if the kit is ignored).
2. **Mouthful**: 35 damage (*builder call*); the Spoon gains Block equal to the HP its hit took past the players'
   Block (*builder call: unblocked damage*).
3. **Feast**: Vulnerable 2 on every fighter (*builder call*), each fighter loses 10 max HP without an attack (lost,
   the Spoon doesn't gain them, *builder call*), then it heals 60 + 10 per player past the first (60, 70, 80, 90).
4. **Ladle Smash**: 55 damage (*builder call*). Owner, 2026-11-16: it also steals max HP from each living player hit,
   a quarter of the damage past their Block (rounded down, the player keeps at least 1; all blocked steals nothing); the
   Spoon gains them and 1 Strength per 5 stolen (owner, 2026-11-16).

1.3.2 (owner, 2026-11-16): Feast also deals a light hit (owner: "dégâts légers"), 12 damage (*builder call*).

Unchanged: supporters are never targeted and deal it nothing; growth on unblocked damage (that much max HP, no heal).

## Strength from max HP

Strength = (max HP − max HP at fight start) / 5, truncated toward zero, negative below the start (owner: "1 Strength
per 5 max HP and vice versa"). Recomputed whenever its max HP changes. Replaces "+1 per 10 grown". The team max HP
damage ratio stays removed.

1.3.2 (owner, 2026-11-16): the max HP grown from its hits no longer counts. Growth fed Strength, which fed bigger hits
and more growth (a play test reached 200+ per hit). Only the max HP it loses (Melon Baller) changes its Strength.

## Appetite and Spoonful

Appetite: +2 after each of the Spoon's moves, +5 % damage per stack (1.3.2, was 10 %). Spoonful stays the normal deck
card that manages it: each one played on the Spoon removes 3.

1.3.2: players can hold Appetite too (same power, +5 % damage per stack, fight only), taken with the kit's Stolen
Spoonful. Each time the Spoon gains Appetite it takes 1 back from every player holding some whose Block that move pierced
(owner, 2026-11-16; otherwise only its usual +2).

Owner, 2026-11-16 (later): the Spoon gains Appetite only after Sets the Table (turn 1 of the cycle), 2 per living
player, supporters included, since they get the kit too. Appetite can be negative for the Spoon and the players
(−5 % damage per stack, never below 0); the removing cards always remove their full amount. After a move that
pierced a player's Block it takes 1 from that player, below 0 too.

## The cutlery (Exhaust, combat-only, `SpoonCard`s outside every card source)

| Card | Cost | Effect |
|---|---|---|
| Teaspoon (Cuillère à café) | 0 | Remove 2 Appetite. Draw 1. |
| Wooden Spoon (Cuillère en bois) | 1 | 1.3.2: an attack dealing 10 % of your max HP; the Spoon's Appetite does nothing on its next turn (normal damage; stacks kept, gain still happens). |
| Skimmer (Écumoire) | 1 | Gain 6 Block (*builder call*). This turn, Feast eats none of your max HP. |
| Melon Baller (Cuillère parisienne) | 2 | Take 12 max HP from the Spoon (fixed: no damage, no Strength); you gain them. |
| Stolen Spoonful (Cuillerée volée) | 1 | 1.3.2: an attack, 10 damage (12 upgraded); steal up to 3 Appetite from the Spoon. |

Targeted like vanilla cards (owner): Teaspoon, Wooden Spoon and Melon Baller target the Spoon, Skimmer the player.
"This turn" for the Skimmer means until the end of the Spoon's next turn. Played on the Spoon (Wooden Spoon, Melon
Baller, Teaspoon target it); a supporter's copy works too.

## Kit (1.3.2, owner, 2026-11-16)

Every Sets the Table gives each living player one of each of the five spoons. From 25 cards in the deck, 1 extra kit
spoon at random, plus 1 per further 5 cards (25 → +1, 30 → +2). The extras come from an `Rng` named after the run seed,
the cycle and the player's slot, so co-op clients agree (*builder call*). This replaces the 1.3.0 table by deck size.

Every number not marked as the owner's is tuned during balancing (owner, 2026-11-16).

## Left to the builder

Pure rules (cycle, kit counts, Strength from max HP, heal) in `src/Core/SpoonRule.cs` with tests; card portraits are
placeholders; EN/FR text; spec, checklist and backlog updates.

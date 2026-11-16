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
4. **Ladle Smash**: 55 damage (*builder call*).

Unchanged: supporters are never targeted and deal it nothing; growth on unblocked damage (that much max HP, no heal).

## Strength from max HP

Strength = (max HP − max HP at fight start) / 5, truncated toward zero, negative below the start (owner: "1 Strength
per 5 max HP and vice versa"). Recomputed whenever its max HP changes. Replaces "+1 per 10 grown". The team max HP
damage ratio stays removed.

## Appetite and Spoonful

Appetite is unchanged: +2 after each of the Spoon's moves, +10 % damage per stack. Spoonful stays the normal deck card
that manages it (the fifth spoon, owner): each one played on the Spoon removes 3.

## The cutlery (Exhaust, combat-only, `SpoonCard`s outside every card source)

| Card | Cost | Effect |
|---|---|---|
| Teaspoon (Cuillère à café) | 0 | Remove 2 Appetite. Draw 1. |
| Wooden Spoon (Cuillère en bois) | 1 | Prevents the Spoon's next Appetite gain. |
| Skimmer (Écumoire) | 1 | Gain 6 Block (*builder call*). This turn, Feast eats none of your max HP. |
| Melon Baller (Cuillère parisienne) | 2 | Take 15 max HP from the Spoon (fixed: no damage, no Strength); you gain them. |

"This turn" for the Skimmer means until the end of the Spoon's next turn. Played on the Spoon (Wooden Spoon, Melon
Baller, Teaspoon target it); a supporter's copy works too.

## Kit by deck size (cards in the player's deck at fight start)

| Deck | Kit |
|---|---|
| ≤ 9 | 1 Teaspoon |
| 10–14 | 1 Teaspoon, Wooden Spoon |
| 15–19 | 2 Teaspoons, Wooden Spoon |
| 20–24 | + Skimmer |
| 25–29 | + Melon Baller |
| 30+ | + 2 Teaspoons (4 in all), and 2 more per further 15 cards (*builder call*) |

Every number not marked as the owner's is tuned during balancing (owner, 2026-11-16).

## Left to the builder

Pure rules (cycle, kit counts, Strength from max HP, heal) in `src/Core/SpoonRule.cs` with tests; card portraits are
placeholders; EN/FR text; spec, checklist and backlog updates.

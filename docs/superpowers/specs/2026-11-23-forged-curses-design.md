# Forged curses: The Forge + Cursed Deck (design)

## Intent

With The Forge and Cursed Deck both ticked, curses fuse at the rest site like other cards, up to +9. The owner wants them
"completely broken": high risk, high reward. A forged curse hits harder on both sides (played on an enemy, or left in
hand on its owner) and picks up **defects** as it climbs, restrictions that make it harder to play, so landing one is
devastating and holding one is dangerous.

## Owner decisions (2026-11-23)

- A real compatibility module, not just "curses never fuse" (the 1.6.0 rule this replaces).
- **High risk, high reward**: each Forge level strengthens the curse on both sides, as Cursed Deck's +1 already does.
  Bad Luck+9 costs the enemy about 76 HP when played, and its owner the same if it stays in hand.
- **Defects** gained on the way up: one at +3, one at +6, one at +9 (three at most). The list below is approved as is.
- Numbers (approved with the approach):
  - each level past +1 repeats the +0 → +1 step: `amount + (amountPlus − amount) × level` (Bad Luck 13, 20, 27 … 76 at
    +9; Decay 2, 4 … 18 at +8, 20 at +9);
  - curses with no played effect: Grimoire damage 12 at +1, then +3.5 per level, rounded down (40 at +9);
  - Normality and Guilty keep weakening: Normality allows one more card per level (4 at +1, 11 at +8, 12 at +9);
    Guilty leaves one combat sooner per level (3 at +1, at least 1);
  - the Forge ladder applies to curses too: Retain at +6 (a trap: it hits its owner every end of turn it stays),
    Innate at +9. Cost −1 at +7 does nothing on a curse already at 0.
- **Duplicate** (merchant button) accepts curses in this combo, since curses arrive at random and fusion needs a pair.

## Approaches

1. **The numbers live in the curse table (picked).** `data/curses/curses.json` keeps owning both sides of every curse;
   Cursed Deck computes any level from `amount`/`amountPlus` (or the Grimoire and `self` steps). The Forge only lifts its
   two curse locks (fusion, Duplicate) and the +1 cap in the combo. One table, the "both sides" rule cannot drift, and
   `docs/review/cursed-curses.md` shows every level.
2. Rows in the Forge table (`data/forge/cards.json`). Curses have no vanilla `OnUpgrade` for the Forge to replay and
   Cursed Deck patches the player side itself: two tables that must agree. Rejected.

## Defects

Drawn at random from this list when a fusion reaches +3, +6 or +9. The card shows each one on its own purple line, like
the Forge's orange "Forged" line. "Unplayable" holds even with the Unholy Grimoire (or any other way to play curses).

| Defect (EN / FR) | Effect |
|---|---|
| Odd / Impaire | Unplayable on odd turns |
| Even / Paire | Unplayable on even turns |
| Late / Tardive | Unplayable before turn 3 |
| Heavy / Lourde | Costs 2 more |
| Demanding / Exigeante | Playable only after 2 other cards were played this turn |
| Bloody / Sanglante | Playing it costs its owner HP equal to its level |
| Spiteful / Rancunière | Left in hand at end of turn, its in-hand effect hits its owner twice |
| Contagious / Contagieuse | Playing it shuffles a random curse into its owner's draw pile |
| Ravenous / Vorace | Playing it exhausts a random card from its owner's hand |
| Echo / Écho | Playing it also hits its owner with half its played effect |

Rules:

- The number of defects follows the level: 1 from +3, 2 from +6, 3 at +9.
- A fusion keeps the kept copy's defects, then the sacrificed copy's, up to that number; missing ones are drawn.
- Odd and Even never sit on the same card (it could never be played). No defect twice on one card.
- "Turn" is the combat's round number, counted from 1.

## Constraints

- Only when both modes are active; either mode alone is unchanged. Ordinary upgrades (Smith, Armaments, Black Censer…)
  still stop at +1; only fusion goes past.
- Defects and levels survive save/reload and match on every co-op client. Draws use the mod's own seeded stream;
  vanilla RNG untouched.
- EN and FR text for every defect and the new card lines.
- Version 1.10.0 on `main`.

## Left to the builder

- How defects are stored on the card (must persist and sync; no new vanilla `[SavedProperty]` on vanilla classes).
- Duplicate price for a curse (Curse rarity has none in `ForgeRule.DuplicatePrice`).
- Which curse Contagious adds (Cursed Deck's random curse source, at +0, is the natural pick).
- Echo on effects that have no "half" (Weak 1 → 0 is fine, rounded down; Debt: its owner loses half the gold).
- Pure rules (levels → numbers, defect count, draw, exclusions, play checks) in `src/Core/` with xUnit tests;
  `tools/curse_table.py` extended to write levels +1 to +9 in the review page.

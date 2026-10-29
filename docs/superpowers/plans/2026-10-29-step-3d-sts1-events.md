# Step 3d — StS1 card events

Goal: Bite, J.A.X. and Ritual Dagger get their StS1 events back (Vampires, Augmenter, The Nest), plus the
Mutagenic Strength relic that Augmenter hands out. Work on `step-3`, version 0.6.0; no install or publication.

## Owner decisions (2026-10-29)

- The events exist only when StS1 cards are in play: a "Slay the Spire 1 Cards" run, or a player holding the
  Spire Souvenir.
- Vampires appears only if every player holds the Spire Souvenir (or in a StS1-only run).
- The Nest and Augmenter appear as soon as one player holds the relic. Players without it see the event
  but their card option is locked; the other options stay open ("Visible, carte réservée").
- Mutagenic Strength is ported as a mod relic with a borrowed vanilla icon, obtainable only from Augmenter.

## Builder calls

- Act 2 only, as in StS1. Each act 2 event room has a 1 in 3 chance to become an eligible StS1 event not seen
  this run. The roll uses its own `Rng` seeded from the run seed, act and visited event count: no vanilla RNG
  stream moves, so seeds without StS1 cards stay vanilla. An event another hook already replaced (Lantern Key)
  is left alone.
- StS1 numbers without Ascension variants: Vampires loses 30% max HP (rounded up, at least 1 HP left) and
  replaces every basic Strike with 5 Bites; its Blood Vial option trades the relic instead of max HP.
  Augmenter: J.A.X., transform 2 cards (locked under 2 transformable cards), or Mutagenic Strength.
  The Nest: 99 gold, or 6 HP for Ritual Dagger. Mutagenic Strength: 3 temporary Strength each combat.
- StS1 text and art (EN/FR). The French Augmenter keeps StS1's own name, "Dealer", with "D.O.P.E.".
- Event portraits are StS1's 600×600 JPEGs converted to PNG under `godot/SlaySpoon/images/events/`; a postfix
  on `EventModel`'s private portrait path points the three events there.
- Each player decides alone (vanilla unshared events); no extra co-op vote.
- `sts1event <id>` console command for testing, like the vanilla `event` command.

## Tasks

1. [x] `Sts1EventRule` in `src/Core/` (eligibility, card lock, replacement roll, Vampires max HP loss) with
   xUnit tests.
2. [x] Event art export in `tools/sts1/art.py` (`--events`) with pytest; run it.
3. [x] Events, relic, temporary Strength power, pool and portrait patches, event injection, console command,
   EN/FR text; asset test for every event. Build 0/0.
4. [x] Checklist `docs/testing/step-3d-checklist.md`, README, CLAUDE.md, backlog, version 0.6.0.

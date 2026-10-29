# Step 3c — StS1 colorless cards

Goal: colorless sources follow StS1-only for every character and Hybrid for the player holding
Spire Souvenir. Character pools and the relic's existing character eligibility stay unchanged.
Target: StS2 v0.107.1. Work on `step-3`; commit per task, no install or publication.

Spec: `docs/superpowers/specs/2026-10-28-slay-the-spoon-design.md` §3.3, §3.4 and §6;
per-player Hybrid follows the Spire Souvenir amendment.

## Recon and constraints

- StS1 has 39 colorless classes: 35 ordinary cards and four special cards (Apparition, Bite,
  JAX, RitualDagger). Special cards must remain outside ordinary reward and generation pools.
  New event creation is outside this port's scope; retain dev-console access for testing.
- Owner amendment: native StS2 event-only cards (Apotheosis, Apparition, Enlightenment and
  Metamorphosis) remain event-only in StS1-only and Hybrid too. Native event grants must be audited
  for effective-mode handling; never promote these cards into ordinary offers or generation.
- The first automatic comparison finds 22 namesakes and 17 unique cards. Classifications are drafts,
  not implementation decisions: literal values, computed damage and pool membership need inspection.
- Apotheosis, Apparition, Enlightenment and Metamorphosis have StS2 namesakes outside the ordinary
  colorless pool. A namesake existing in ModelDb does not mean Compose can retrieve it from that pool.
- Keep EN/FR text and portraits for every implemented card. Namesakes use the StS2 French title.
- Do not commit decompiled sources. Read local sources to verify commands, upgrade behavior and RNG.
- All selection and random generation must use synced game commands/RNG; no local random choices.
- Preserve vanilla pools for players without the relic and outside StS1-only. Regent/Necrobinder
  character cards remain vanilla; their colorless sources follow the effective mode too.

## Task 1 — catalog (complete)

- [x] Extract all 39 cards, retaining their special rarity and EN/FR source text.
- [x] Generate draft classifications, owner review page and namesake ids.
- [x] Add `make sts1-colorless-data`, which regenerates only the catalog, not unported skeletons.
- [x] Verify catalog coverage and special-card boundary with tests; run the existing test suite.

## Task 2 — classify namesakes (complete)

- [x] Inspect both versions of all 22 namesakes, including upgrades and actual pool membership.
- [x] Mark builder-reviewed rows `checked: true`, retaining `draft: true` for owner review.
- [x] Regenerate the review page and namesake ids. Record final counts and card batches here.

### Reviewed result and implementation batches

All 22 namesakes have `checked: true`; `draft: true` remains for owner review. Local card
sources, relevant actions/powers and ColorlessCardPool/EventCardPool were compared against
StS2 v0.107.1. No decompiled source is included in the repository.

- 17 unique, 6 same-sts1, 8 duplicates, 8 same-sts2: **31 classes to implement**, including
  four source-Special cards. After the owner's event-only amendment, ordinary StS1-only offers
  comprise 24 custom classes and 8 native cards (32 total). Seven custom classes stay outside
  ordinary pools: Apotheosis, Apparition, Bite, Enlightenment, JAX, Metamorphosis, RitualDagger.
- Task 4, attacks (6): Bite (Special), DramaticEntrance, FlashOfSteel, MindBlast,
  RitualDagger (Special), SwiftStrike.
- Task 5, skills (22): Apotheosis, Apparition (Special), BandageUp, Blind, Chrysalis,
  DeepBreath, Discovery, Enlightenment, Finesse, Forethought, GoodInstincts, JAX (Special),
  JackOfAllTrades, Madness, Metamorphosis, Panacea, Purity, SecretTechnique, SecretWeapon,
  Transmutation, Trip, Violence.
- Task 6, Powers (3): Magnetism, Panache, SadisticNature. Panache needs one shared countdown
  with stacking damage, unlike native independent instances.
- Reuse native DarkShackles, HandOfGreed, Impatience, MasterOfStrategy, Mayhem, PanicButton,
  TheBomb and ThinkingAhead. Rarity differences alone do not create duplicates.

`same-sts1` also records availability exceptions: Apparition and Metamorphosis have the same
core effect as their native versions but retain StS1 classes for event/console access, without
version swaps. Their source rarities stay intact in the catalog; rarity must not be the sole
criterion for ordinary pool membership.

Owner amendment: Apotheosis, Apparition, Enlightenment and Metamorphosis belong to native
EventCardPool and must remain event-only in every mode, including StS1-only. Do not supply
missing Hybrid slots for them. Task 3 must exclude these classes explicitly from ordinary
pools and combat generation, along with Bite, JAX and RitualDagger. Audit native event grants
(which may construct cards directly) so effective-mode handling and duplicate partners work
without adding EventCardPool to the ordinary colorless pair. Event-only cards are obtained
through events; console access is a testing facility. No new events are requested here.

Additional differences requiring custom classes: Discovery's mandatory choice vs native skip;
JackOfAllTrades+'s independent rolls vs native distinct results; SecretTechnique/SecretWeapon's
playability condition when the draw pile has no matching type. Preserve these behaviors when
porting, with synced selection and RNG. Native Fatal and card-source Block conventions remain
the engine adaptation for reused HandOfGreed and PanicButton.

## Task 3 — infrastructure

- [ ] Add Sts1ColorlessCard and its hidden pool with vanilla colorless frame/energy metadata.
- [ ] Pair ordinary ColorlessCardPool in CardSetPools; handle all seven event-only classes outside that pool.
- [ ] Audit player context for merchant, events, transforms and random colorless generation.
- [ ] Extend the dev console to include special cards without exposing them to ordinary rewards.
- [ ] Generate portraits and skeletons only for final classifications; integrate regeneration into
  `make sts1-data` after the base classes exist. Build with zero warnings/errors.

## Task 4 — attacks

- [ ] Port attacks and special attacks, localization and checklist entries. Verify permanent growth
  persistence and fatal conditions for Ritual Dagger. Build and commit.

## Task 5 — skills

- [ ] Port skills and special skills, localization and checklist entries. Verify selection cancellation,
  cost duration, X-cost and generated cards with the effective mode. Build and commit.

## Task 6 — Powers

- [ ] Port missing Powers and supporting power models, EN/FR text and borrowed icons; add checklist
  entries covering turn timing and co-op ownership. Build and commit.

## Task 7 — validation and release

- [ ] Tests pass, no unported cards or missing assets. Build with zero warnings/errors.
- [ ] Checklist covers all five characters, one-holder co-op, save/resume, shops and generated cards,
  special-card exclusion and Hybrid swaps. Leave in-game checks unchecked for the owner.
- [ ] Update README and CLAUDE.md, version 0.5.0, then commit. No install, merge, push or publication.

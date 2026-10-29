# Step 3b — Defect's StS1 cards — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The Defect's StS1 cards follow the card set modes like the Ironclad's and the Silent's: StS1-only offers StS1 cards, a Spire Souvenir holder mixes both games, duplicates are one slot with the swap button.

**Architecture:** Same pipeline as step 3a: `make sts1-data` (now looping over `blue:Defect` too) extracts the blue cards, drafts `data/duplicates/defect.json`, cuts portraits and writes skeletons; the builder classifies every namesake by hand, then ports every `unique`, `same-sts1` and `duplicate` card in `src/Cards/Sts1/Defect/`. A `Sts1DefectCardPool` paired with `DefectCardPool` in `CardSetPools.Pairs`, and `DefectRelicPool` in `SouvenirPoolPatch.Pools`, are the only wiring.

**Tech Stack:** C# / .NET 9, Harmony, Godot 4.5.1 (PCK), xUnit; Python 3.12 + pytest in the `slayspoon-tools` Podman image.

**Spec:** `docs/superpowers/specs/2026-10-28-slay-the-spoon-design.md` (§3.3, §3.3b, §3.4, §6, §7 step 3), amended by `docs/superpowers/specs/2026-10-29-spire-souvenir-relic-design.md` §7.

## Completion status

Implementation is complete on `step-3` in version 0.4.0. Tasks 1–6 delivered 38 Defect cards;
Task 7 and the Task 8 final gate pass (61 xUnit tests, 32 pytest tests, build with zero warnings
and errors). README, CLAUDE.md and the in-game checklist are updated. The checklist and namesake
review still require the owner's in-game validation and review; no install, merge or publication
was performed. Earlier task descriptions below preserve the original plan.

## Recon

- StS1 ships a `blue/Impulse` class it never uses (no text, no art, nothing creates it). The extractor skips classes with no English name: 75 blue cards.
- The Defect's StS1 starter deck equals StS2's (4 Strike, 4 Defend, Zap, Dualcast). Zap is drafted `duplicate` only because of wording; both cost 1, channel 1 Lightning, upgrade to cost 0. StS1-only keeps the vanilla starter.
- StS2 has every StS1 orb (Lightning, Frost, Dark, Plasma) through `OrbCmd.Channel<T>` / `OrbCmd.EvokeNext`, plus `FocusPower`. StS1 cards channel StS2's orbs: orb numbers are a character mechanic, not a card, and stay vanilla.
- Draft from `match.py`: 24 `unique`, 31 `same-sts2`, 1 `same-sts1`, 19 `duplicate`.

## Global Constraints

- Target StS2 v0.107.1; mod id `SlaySpoon`; all docs/comments in English; mod text in `eng` and `fra` only.
- StS1 card classes are prefixed `Sts1` (ModelDb id `STS1_*`). Never rename one after release.
- Decompiled code (Java or C#) never enters the repo, not even as comments.
- With no card set modifier and no Spire Souvenir, every card pool is exactly vanilla. Daily runs stay vanilla.
- Card candidate lists are built in a deterministic order (co-op clients roll the same RNG on them).
- Regent and Necrobinder keep their vanilla pool in every mode. Colorless stays vanilla until 3c.
- Namesakes take StS2's French title (3a ruling).
- Podman for every tool, heavy jobs under `nice -n 19`. No `make up` while `pgrep -x SlayTheSpire2` finds the game.
- Work on branch `step-3`. Commit per task. No push.

## Review Focus

1. **Ironclad and Silent unchanged:** `make sts1-data` leaves their data, review pages, matches and skeletons byte-identical (Task 1, `git status` on them).
2. **Defect in StS1-only:** the pool holds StS1 cards and the StS1 namesakes and keeps the starter cards (composer tests; `Sts1Assets_AreComplete` over the Defect folder).
3. **Spire Souvenir for the Defect:** the relic drops for the Defect in Standard runs and a Spire Souvenir run starts the Defect with it; `DefectRelicPool` declares its own `GenerateAllRelics` and `GetUnlockedRelics` (else the patch reaches the base pool) (Task 2).
4. **Orb cards with no orb slot or no orbs:** evoke and orb-count cards (Recursion, Multi-Cast-like X cards, Barrage, Fission, Consume) must do nothing harmful at 0 orbs or 0 slots, as in StS1.
5. **Cards that count or scale across a combat** (Claw-likes, Streamline, Steam Barrier, Genetic Algorithm's permanent growth): the growth stays on the combat copy, or on the deck card only where StS1 makes it permanent (Genetic Algorithm).

---

### Task 1: Defect card data

**Files:** `Makefile` (`sts1-data` loop += `blue:Defect`), `tools/sts1/extract.py` + `tools/sts1/tests/test_extract.py` (skip classes with no English name; fixture `FakeCut.java`).

- [ ] **Step 1:** Test `test_skips_cut_cards_without_text`. Run pytest: FAIL.
- [ ] **Step 2:** Filter in `extract()`. Run pytest: PASS. Run `make sts1-data`. Expected: Ironclad and Silent outputs unchanged; new `data/sts1/defect.json` (75 cards), `data/duplicates/defect.json`, `docs/review/defect-duplicates.md`, `src/CardSets/Sts1Matches.Defect.g.cs`, portraits and skeletons.
- [ ] **Step 3:** Commit `feat: Defect StS1 card data` (xUnit stays red on the Defect skeletons until Task 7).

### Task 2: Defect card infrastructure and Spire Souvenir

**Files:** create `src/Cards/Sts1DefectCard.cs`, `src/Cards/Pools/Sts1DefectCardPool.cs` (copy the Silent ones; frame, energy name and colours from vanilla `DefectCardPool`); modify `src/CardSets/CardSetPools.cs` (`Pairs` += Defect), `src/Relics/SouvenirPoolPatch.cs` (`Pools` += `DefectRelicPool`), the Spire Souvenir modifier text (names the Defect too, game wording), `README`/checklist later.

- [ ] **Step 1:** Add them. `make build`: 0 warnings.
- [ ] **Step 2:** Commit `feat: Defect StS1 pool and Spire Souvenir for the Defect`.

### Task 3: Classify the Defect namesakes

- [ ] **Step 1:** Read each of the 51 namesakes against both texts and set `relation` and a one-line `note` in `data/duplicates/defect.json` (`draft` stays `true`). Rules (spec §3.4): numbers only → `same-sts2` when StS2 is weaker or equal, `same-sts1` when StS1 is weaker; a different effect or keyword → `duplicate`; rarity alone never makes a duplicate; a StS2 card outside the Defect's vanilla pool (Event, colorless, Ancient-only) → `same-sts1` so StS1-only keeps it.
- [ ] **Step 2:** Re-run `make sts1-data`; delete portraits and skeletons of drafts that ended `same-sts2` (art.py and codegen never remove files).
- [ ] **Step 3:** Commit `feat: classify Defect namesakes for owner review`.

### Tasks 4–7: Port the Defect cards

As in 3a: read the StS1 java, copy the closest StS2 card or power pattern, typed vars renamed in both `cards.json`, `ExtraHoverTips` (orbs: `HoverTipFactory.Static(StaticHoverTip.Channeling)` / `Evoke` + `FromOrb<T>()`), new powers in `src/Cards/Sts1/Powers/` with `powers.json` in both languages and a borrowed icon. `make build` 0 warnings; one checklist line per card in `docs/testing/step-3b-checklist.md`; commit `feat: port StS1 Defect <batch>`.

- **Task 4 — attacks.**
- **Task 5 — skills.**
- **Task 6 — powers.**
- **Task 7 — the gate:** `NoUnportedCards` passes; `make test` green.

### Task 8: Wrap-up

- [x] **Step 1:** Checklist header and mode lines (StS1-only Defect rewards; Spire Souvenir run starts the Defect with the relic; Hybrid swap on a Defect duplicate; co-op Defect + another character); README; CLAUDE.md; version `0.4.0`.
- [x] **Step 2:** `make test` and `make build` green. Commit `docs: step 3b wrap-up`.

# Step 3a — Silent's StS1 cards — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The Silent's StS1 cards follow the card set modes like the Ironclad's: StS1-only offers StS1 cards, a Spire Souvenir holder mixes both games, duplicates are one slot with the swap button.

**Architecture:** Same pipeline as step 2: `make sts1-data` extracts the green cards, drafts `data/duplicates/silent.json`, cuts portraits and writes skeletons; the builder classifies every namesake by hand, then ports every `unique`, `same-sts1` and `duplicate` card in `src/Cards/Sts1/Silent/`. A `Sts1SilentCardPool` paired with `SilentCardPool` in `CardSetPools.Pairs` is the only wiring the card set code needs; it already composes per pool. The Spire Souvenir joins the Silent relic pool.

**Tech Stack:** C# / .NET 9, Harmony, Godot 4.5.1 (PCK), xUnit; Python 3.12 + pytest in the `slayspoon-tools` Podman image.

**Spec:** `docs/superpowers/specs/2026-10-28-slay-the-spoon-design.md` (§3.3, §3.3b, §3.4, §6, §7 step 3), amended by `docs/superpowers/specs/2026-10-29-spire-souvenir-relic-design.md` §7.

## Scope split (builder's call)

Spec step 3 (Silent, Defect, Colorless) is split in three plans, each a usable build:
- **3a (this plan):** tooling for several characters, the Silent.
- **3b:** the Defect (orbs; Zap is a starter card drafted `duplicate`, so the hybrid starting-card question lands there).
- **3c:** StS1 colorless cards, for every character (spec §3.3); with them, every character becomes eligible for the Spire Souvenir.

Recon that shrinks 3a:
- The Silent's StS1 starter deck equals StS2's (5 Strike, 5 Defend, Neutralize, Survivor all drafted `same-sts2`). StS1-only keeps the vanilla starter; Archaic Tooth and Amalgamator need nothing.
- Draft from `match.py`: 23 `unique`, 34 `same-sts2`, 3 `same-sts1`, 15 `duplicate`. The builder's hand review (Task 3) settles the final split; Ironclad's 44 drafts ended at 41/2/1.

## Global Constraints

- Target StS2 v0.107.1; mod id `SlaySpoon`; all docs/comments in English; mod text in `eng` and `fra` only.
- StS1 card classes are prefixed `Sts1` (ModelDb id `STS1_*`). Never rename one after release.
- Decompiled code (Java or C#) never enters the repo, not even as comments.
- With no card set modifier and no Spire Souvenir, every card pool is exactly vanilla. Daily runs stay vanilla.
- Card candidate lists are built in a deterministic order (co-op clients roll the same RNG on them).
- Regent and Necrobinder keep their vanilla pool in every mode. Colorless stays vanilla until 3c.
- Podman for every tool, heavy jobs under `nice -n 19`. No `make up` while `pgrep -x SlayTheSpire2` finds the game.
- Work on branch `step-3` (cut from the unmerged `step-2`). Commit per task. No push.

## Review Focus

1. **Silent in StS1-only:** the pool must hold StS1 cards and the StS1 namesakes, never be empty, and keep the starter cards. Covered by the composer tests already; checked by `Sts1Assets_AreComplete` running over Silent too (Task 2).
2. **Ironclad unchanged:** moving to several characters must not change Ironclad's generated files. `make sts1-data` leaves `data/duplicates/ironclad.json`, the Ironclad matches and skeletons byte-identical (Task 1, `git diff --exit-code` on them).
3. **Spire Souvenir for the Silent:** the relic now drops for the Silent in Standard runs and a Spire Souvenir run starts the Silent with it; the grab bag patch must insert it for any character whose relic pool holds it (Task 2).
4. **A ported card with a raw text key or a mismatched placeholder:** the existing text/var tests run over every character folder (Task 2).
5. **Duplicate pairs for Silent cards:** the Hybrid swap and random version read `CardSetPools.PartnerOf`, which must find Silent pairs (Task 2: `Partners` built from every pair, not only Ironclad).

---

### Task 1: Tooling for several characters

**Files:**
- Modify: `tools/sts1/match.py` (review page title and table path from `--character`), `tools/sts1/tests/test_match.py`
- Move: `src/CardSets/Sts1Matches.g.cs` → `src/CardSets/Sts1Matches.Ironclad.g.cs`
- Modify: `Makefile` (`sts1-data` loops over `red:Ironclad green:Silent`; matches go to `src/CardSets/Sts1Matches.<Character>.g.cs`, review to `docs/review/<character>-duplicates.md`)

- [ ] **Step 1:** Test `test_review_title_names_the_character`: `review_md(..., character="Silent")` starts with `# Silent — StS1 / StS2 duplicate table` and cites `data/duplicates/silent.json`.
- [ ] **Step 2:** Run pytest. Expected: FAIL (no `character` parameter).
- [ ] **Step 3:** Add the parameter; pass `args.character`.
- [ ] **Step 4:** Run pytest (PASS). Run `make sts1-data`. Expected: Ironclad outputs unchanged (`git diff --exit-code data/duplicates/ironclad.json docs/review/ironclad-duplicates.md src/CardSets/Sts1Matches.Ironclad.g.cs`); new `data/sts1/silent.json`, `data/duplicates/silent.json`, `docs/review/silent-duplicates.md`, `src/CardSets/Sts1Matches.Silent.g.cs`, portraits in `godot/SlaySpoon/images/cards/silent/`, skeletons in `src/Cards/Sts1/Silent/` for the 23 `unique` cards.
- [ ] **Step 5:** Commit `feat: StS1 tooling for several characters; Silent card data`.

### Task 2: Silent card infrastructure and Spire Souvenir

**Files:**
- Create: `src/Cards/Sts1SilentCard.cs`, `src/Cards/Pools/Sts1SilentCardPool.cs` (copy the Ironclad ones: frame `card_frame_green`, energy `silent`; colours from vanilla `SilentCardPool`)
- Modify: `src/CardSets/CardSetPools.cs` (`Pairs` += `(SilentCardPool, Sts1SilentCardPool, Sts1Matches.Silent)`; `Partners` over every pair)
- Modify: `src/Relics/SouvenirPoolPatch.cs` (pool postfix also on `SilentRelicPool.GenerateAllRelics`; grab bag check "the player's character relic pool holds the Souvenir" instead of `is IroncladRelicPool`; hide-from-shuffle on Silent's `GetUnlockedRelics` too)
- Modify: `tests/Sts1AssetsTests.cs` (every test that reads `CardFiles("Ironclad")` reads every folder under `src/Cards/Sts1/` except `Powers`, and portraits from the folder's lower-case name)

- [ ] **Step 1:** Change the asset tests to iterate the character folders. Run `make test`. Expected: FAIL on `NoUnportedCards` (Silent skeletons throw `NotImplementedException`) — if the Silent skeletons are already compiled and texts and portraits exist, that is the only failure.
- [ ] **Step 2:** Add the base class, pool, pair and Souvenir pools. Run `make build`. Expected: 0 warnings.
- [ ] **Step 3:** Commit `feat: Silent StS1 pool and Spire Souvenir for the Silent` (the red `NoUnportedCards` stays until Task 7).

### Task 3: Classify the Silent namesakes

- [ ] **Step 1:** Read each of the 52 drafted namesakes against both texts (`docs/review/silent-duplicates.md`, the StS1 java and the StS2 decompile) and set `relation` and a one-line `note` by hand in `data/duplicates/silent.json`; `draft` stays `true` until the owner approves. Rules (spec §3.4): numbers only → `same-sts2` when StS2 is weaker or equal, `same-sts1` when StS1 is weaker; a different effect or keyword → `duplicate`; rarity alone never makes a duplicate.
- [ ] **Step 2:** Re-run `make sts1-data` (keeps hand edits), then `codegen.py --relations same-sts1,duplicate` for the Silent to get their skeletons.
- [ ] **Step 3:** Commit `feat: classify Silent namesakes for owner review`.

### Tasks 4–7: Port the Silent cards

For each card: read the StS1 java and its action; find the closest StS2 card or power and copy its pattern; fill in `OnPlay`; prefer typed vars (`PowerVar<T>`, `CardsVar`, `EnergyVar`) and rename them in both `cards.json`; add `ExtraHoverTips`; new powers go in `src/Cards/Sts1/Powers/` with `powers.json` text in both languages (borrowed icon through `IBorrowedIcon`). `make build` after each batch, 0 new warnings; one checklist line per card in `docs/testing/step-3a-checklist.md`; commit `feat: port StS1 Silent <batch>`.

- **Task 4 — attacks:** the `unique` and namesake attacks (Bane, Choke, DieDieDie, Eviscerate, FlyingKnee, GlassKnife, HeelHook, MasterfulStab, QuickSlash, RiddleWithHoles, SneakyStrike, Unload, AllOutAttack, EndlessAgony, plus namesake attacks kept).
- **Task 5 — skills:** Catalyst, Concentrate, CorpseExplosion, CripplingPoison, Doppelganger, PhantasmalKiller, Setup, Terror, plus namesake skills kept.
- **Task 6 — powers:** AThousandCuts, plus namesake powers kept (e.g. WraithForm, AfterImage, Envenom if `duplicate`).
- **Task 7 — the rest and the gate:** remaining cards; `NoUnportedCards` passes; `make test` green.

### Task 8: Wrap-up

- [ ] **Step 1:** Checklist header and mode lines (StS1-only Silent rewards; Spire Souvenir run starts the Silent with the relic; Hybrid swap on a Silent duplicate; co-op Silent + Ironclad); README (Silent cards, Spire Souvenir for Silent); CLAUDE.md (Where we are, code map); version `0.3.0` in `mod/SlaySpoon.json` and `src/ModEntry.cs`.
- [ ] **Step 2:** `make test` and `make build` green. Commit `docs: step 3a wrap-up`.

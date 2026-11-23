# Forged Curses Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** With The Forge and Cursed Deck both active, curses fuse up to +9, grow on both sides per level and pick up defects at +3, +6 and +9.

**Architecture:** All numbers and defect logic live in a new pure `src/Core/ForgedCurseRule.cs` (xUnit-tested). The curse table
(`data/curses/curses.json`) stays the only source of per-curse numbers. Game glue lives in `src/Curses/Forged/`: a mode check,
the lifted Forge locks (cap, step, fusion, Duplicate), level-aware Cursed Deck patches, defect storage on the card (BaseLib
`SavedSpireField`), play checks, on-play effects and the purple text line.

**Tech Stack:** C# .NET 9, Harmony, BaseLib 3.4.8 (`SavedSpireField`), xUnit, pytest (`tools/curse_table.py`).

**Spec:** `docs/superpowers/specs/2026-11-23-forged-curses-design.md`

## Global Constraints

- Active only when `SpoonForge` and `SpoonCursedDeck` are both active; either mode alone stays exactly as in 1.9.0.
- Ordinary upgrades (Smith, Armaments, Black Censer…) still stop at +1; only fusion (and reload of a saved level) goes past.
- Formula past +1: `amount + (amountPlus − amount) × level`; Grimoire no-effect damage `floor(12 + 3.5 × (level − 1))`
  for level ≥ 1 (5 at +0, 40 at +9); Normality limit `3 + level` (12 at +9); Guilty combats `max(1, 3 − (level − 1))` from +1.
- Ladder applies: Retain at +6, cost −1 at +7, Innate at +9 (`ForgeRule.BonusAt`).
- Defect count: 0 below +3, 1 from +3, 2 from +6, 3 at +9. Odd and Even never together; no defect twice.
- Fusion keeps the kept copy's defects, then the sacrificed copy's, up to the count; missing ones are drawn on a mod seeded
  stream. Vanilla RNG untouched.
- "Turn" is `CombatState.RoundNumber` (from 1). "Unplayable" defects hold even with the Grimoire.
- Levels and defects survive save/reload and match on co-op clients.
- All text in `godot/SlaySpoon/localization/eng/` and `fra/`. Repo text in English.
- Version **1.10.0** in `mod/SlaySpoon.json` and `src/ModEntry.cs`.
- Never commit decompiled code. Commits end with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Builder calls (spec "Left to the builder")

- **Storage:** one BaseLib `SavedSpireField<CardModel, int>` named `SpoonCurseDefects` holding a `CurseDefect` bit mask,
  `CopyOnClone()`. The level itself is the card's vanilla `CurrentUpgradeLevel` (already saved and synced).
- **Duplicate price for a curse:** the Common band (`ForgeRarity.Common`).
- **Contagious:** a random curse from `CursedDeckCurses.Pool`, at +0, into the draw pile at a random position, drawn on the
  defect stream (not the vanilla combat RNG).
- **Echo:** half the played amount, rounded down, same effect kind on the owner (HP loss, damage, Weak, Vulnerable, max HP);
  Debt: the owner loses half the gold amount. Zero means nothing happens.
- **Bloody:** unblockable HP loss equal to the card's level, before the played effect.
- **Spiteful:** Cursed Deck's in-hand end-of-turn effect (Regret, Pain, Decay, Doubt, Shame…) runs a second time.
- **Ravenous:** exhausts a random other card from the owner's hand (defect stream), after the played effect.

## Review Focus

1. A saved +7 curse reloaded (solo and as the co-op client) keeps level, vars, Retain and its defects. → Task 6 checklist
   + Task 3 `LoadKeepsLevelAboveOne` rule test (`ForgedCurseRule.LoadLevel`).
2. Black Censer / Smith on a +3 curse in the combo does nothing (stays +3, no new defect). → Task 3 test `GateStopsOrdinaryUpgradePastOne`.
3. Fusing two curses whose defects together exceed the count (kept {Odd}, sacrificed {Even, Heavy}, result +6) gives
   {Odd, Heavy}: Even is dropped as incompatible, not drawn later. → Task 1 test `MergeSkipsIncompatibleSacrificedDefect`.
4. Echo/Bloody can kill the owner; Bloody on a 1 HP owner must still resolve (the owner dies, no exception). → Task 5 checklist line.
5. A curse with the Heavy defect and no Grimoire stays vanilla unplayable; Heavy's +2 never shows on a non-playable curse's orb.
   → Task 1 test `HeavyOnlyWhenPlayable` on `ForgedCurseRule.CostWithDefects`.

---

### Task 1: Pure rules (`ForgedCurseRule`)

**Files:**
- Create: `src/Core/ForgedCurseRule.cs`
- Modify: `src/Core/CurseRule.cs` (level-taking overloads), `tests/CurseRuleTests.cs` (unchanged tests must still pass)
- Test: `tests/ForgedCurseRuleTests.cs`

**Interfaces:**
- Produces:
  - `[Flags] enum CurseDefect { None = 0, Odd = 1, Even = 2, Late = 4, Heavy = 8, Demanding = 16, Bloody = 32, Spiteful = 64, Contagious = 128, Ravenous = 256, Echo = 512 }`
  - `static IReadOnlyList<CurseDefect> ForgedCurseRule.All` (the ten, in table order)
  - `int AmountAt(CurseRow row, int level)` (level 0 → Amount, 1 → AmountPlus, then the formula)
  - `int GrimoireAt(CurseRow? row, int level)` (rows with a played effect: `CurseRule.GrimoireDamage`; otherwise 5 / formula)
  - `int NormalityLimitAt(int level)`, `int GuiltyCombatsAt(int level)` (+0 → 5)
  - `int DefectCount(int level)`
  - `CurseDefect Merge(CurseDefect kept, CurseDefect sacrificed, int level, Func<int, int> roll)` — `roll(n)` returns 0..n−1
  - `bool Compatible(CurseDefect set, CurseDefect add)`
  - `bool BlocksPlay(CurseDefect set, int round, int otherCardsPlayedThisTurn)`
  - `int CostWithDefects(CurseDefect set, int cost, bool playable)` (+2 when Heavy and playable)
  - `int EchoAmount(int played)`, `int BloodyHp(int level)`, `int SpitefulTimes(CurseDefect set)`
  - `int MaxLevelFor(bool combo, bool inScope, int vanillaMax)`, `int LoadLevel(int saved, bool comboActive)`
  - `static string DefectStream(int floor, int deckIndex, int draw)` → `"forged_curse_defect_{floor}_{deckIndex}_{draw}"`
  - In `CurseRule`: `EnemyAmount(CurseRow, int level)`, `PlayedAmount(CurseRow, int level, int hand, int played)`,
    `GrimoireDamageFor(CurseRow?, int level)`; the `bool upgraded` versions call them with `upgraded ? 1 : 0`.

- [ ] **Step 1: Write the failing tests** in `tests/ForgedCurseRuleTests.cs`:
  - `AmountAt`: Bad Luck row (13, 20) → level 0: 13, 1: 20, 2: 27, 9: 76; Decay (2, 4) → 8: 18, 9: 20.
  - `GrimoireAt` no-effect row: 0 → 5, 1 → 12, 2 → 15, 3 → 19, 9 → 40; a Bad Luck row → 5 at every level.
  - `NormalityLimitAt`: 0 → 3, 1 → 4, 8 → 11, 9 → 12. `GuiltyCombatsAt`: 0 → 5, 1 → 3, 2 → 2, 3 → 1, 9 → 1.
  - `DefectCount`: 2 → 0, 3 → 1, 5 → 1, 6 → 2, 8 → 2, 9 → 3.
  - `MergeKeepsKeptFirst`: kept {Late}, sacrificed {Heavy, Echo}, level 6 → {Late, Heavy} (sacrificed in `All` order).
  - `MergeSkipsIncompatibleSacrificedDefect`: kept {Odd}, sacrificed {Even, Heavy}, level 6 → {Odd, Heavy}.
  - `MergeDrawsMissing`: kept None, sacrificed None, level 9, `roll = n => 0` → three distinct defects, never Odd+Even,
    same result for the same roll function; level 2 → None.
  - `MergeNeverShrinksBelowKept`: kept {Odd, Late} at level 3 (count 1) → {Odd, Late} (a fusion never removes defects).
  - `BlocksPlay`: Odd blocks rounds 1, 3; Even blocks 2; Late blocks rounds 1–2, not 3; Demanding blocks with 0 or 1 other
    cards, not 2; None blocks nothing.
  - `HeavyOnlyWhenPlayable`: `CostWithDefects(Heavy, 1, true)` = 3, `(Heavy, 1, false)` = 1, `(None, 0, true)` = 0.
  - `EchoAmount`: 13 → 6, 1 → 0. `BloodyHp(7)` = 7. `SpitefulTimes(Spiteful)` = 2, `(None)` = 1.
  - `GateStopsOrdinaryUpgradePastOne`: `MaxLevelFor(false, *, 0)` = 1, `(true, false, 0)` = 1, `(true, true, 0)` = 9.
  - `LoadKeepsLevelAboveOne`: `LoadLevel(7, true)` = 7, `(7, false)` = 1, `(12, true)` = 9.
  - In `CurseRuleTests`: `PlayedAmount(badLuck, level: 9, …)` = 76; `PlayedAmount(regret, 3, hand: 4, 0)` = 4 × `AmountAt(regret, 3)`.
- [ ] **Step 2: Run** `make test` — Expected: the new tests fail to compile (`ForgedCurseRule` missing).
- [ ] **Step 3: Implement** `ForgedCurseRule` and the `CurseRule` level overloads. Grimoire uses integer maths:
  `12 + 7 × (level − 1) / 2`. `Merge`: take kept's flags; add sacrificed's in `All` order while under the count and
  `Compatible`; then draw from the compatible remainder with `roll(remaining.Count)` until the count is reached.
- [ ] **Step 4: Run** `make test` — Expected: all xUnit and pytest pass.
- [ ] **Step 5: Commit** `git commit -m "Forged curses: pure level and defect rules"`.

### Task 2: Level-aware Cursed Deck numbers

**Files:**
- Modify: `src/Curses/CurseUpgradePatch.cs` (`VarPatch`, `NormalityShouldPlayPatch`, `NormalityGlowPatch`, `GuiltyCountPatch`,
  `GuiltyLeavePatch`, `RegretPatch`, `DescriptionPatch`), `src/Curses/SpoonUnholyGrimoire.cs` (`AfterCardExhausted`,
  `AfterCardPlayed`), every other caller of the `bool upgraded` overloads (grep `EnemyAmount(\|PlayedAmount(\|GrimoireDamageFor(`)

**Interfaces:**
- Consumes: Task 1 level overloads, `AmountAt`, `NormalityLimitAt`, `GuiltyCombatsAt`, `GrimoireAt`.

- [ ] **Step 1:** Make `VarPatch` idempotent by level: set the row's var to `AmountAt(row, card.CurrentUpgradeLevel)` (Normality
  `CalculationBase` and multiplier to `NormalityLimitAt(level)`, Guilty `Combats` to `GuiltyCombatsAt(level) − CombatsSeen`)
  instead of adding the +0→+1 delta. At level 1 this equals 1.9.0's result.
- [ ] **Step 2:** Replace every fixed `row.Self[1]` / `row.AmountPlus` in the listed patches and every `card.IsUpgraded` in the
  Grimoire with the card's `CurrentUpgradeLevel` through the level overloads. Regret+ / Normality+ text reads the level value.
- [ ] **Step 3: Run** `make build test` — Expected: 0 warnings, all tests pass.
- [ ] **Step 4: Commit** `git commit -m "Cursed Deck: curse numbers follow the card level"`.

### Task 3: Curses climb past +1 in the combo

**Files:**
- Create: `src/Curses/Forged/ForgedCurses.cs` (`static bool IsActive(IRunState?)`, `static bool Applies(CardModel)`:
  curse type and both modes active for the owner's run, else `ForgeLevels.CurrentRun`)
- Modify: `src/Curses/CurseUpgradePatch.cs` (`MaxLevelPatch`, `LoadPatch`), `src/Forge/ForgeLevels.cs` (`Fusable`, `StepPatch`,
  `Limit`, `Cap`, new `internal static bool InScope => _openScopes > 0`), `src/Forge/ForgeDuplicate.cs` (`Eligible`, `RarityOf`)

**Interfaces:**
- Consumes: `ForgedCurseRule.MaxLevelFor`, `LoadLevel`, `ForgeLevels.Open()`.
- Produces: `ForgedCurses.Applies(CardModel) : bool` used by Tasks 4–5.

- [ ] **Step 1:** `CurseUpgrade.MaxLevelPatch` returns `MaxLevelFor(ForgedCurses.Applies(card), ForgeLevels.InScope, vanilla)`
  (keeping 1.9.0's 0→1 when only Cursed Deck is active). `CurseUpgrade.LoadPatch` raises a saved curse to
  `LoadLevel(saved, combo)` inside `ForgeLevels.Open()`.
- [ ] **Step 2:** `ForgeLevels.Fusable`, `Limit` and `Cap` return true / 9 / 9 for `ForgedCurses.Applies` cards. `StepPatch`
  treats such a curse like a base-max card past +1 (OnUpgrade runs, so `VarPatch` sets the level's numbers; cost stays; the
  ladder adds Retain at +6 and Innate at +9). `ForgeDuplicate.Eligible` accepts curses when `ForgedCurses.Applies`; their
  price uses the Common band.
- [ ] **Step 3: Run** `make build test` — Expected: 0 warnings, all pass.
- [ ] **Step 4: Commit** `git commit -m "Forged curses: fusion, Duplicate and levels past +1 in Forge + Cursed Deck"`.

### Task 4: Defects: storage, draw at fusion, play checks, text

**Files:**
- Create: `src/Curses/Forged/CurseDefects.cs`, `src/Curses/Forged/CurseDefectPlayPatch.cs`, `src/Curses/Forged/CurseDefectTextPatch.cs`
- Modify: `src/Forge/ForgeFusion.cs` (`Apply`, `Build`), `src/Curses/PlayableCursePatch.cs` (`PlayableCurseCostPatch`),
  `godot/SlaySpoon/localization/{eng,fra}/cards.json`

**Interfaces:**
- Consumes: `ForgedCurseRule.Merge`, `BlocksPlay`, `CostWithDefects`, `DefectStream`, `ForgedCurses.Applies`, `CursePlay.IsPlayableCurse`.
- Produces: `CurseDefects.Get(CardModel) : CurseDefect`, `CurseDefects.Set(CardModel, CurseDefect)`.

- [ ] **Step 1:** `CurseDefects`: `static readonly SavedSpireField<CardModel, int> Field = new(() => 0, "SpoonCurseDefects")`
  with `.CopyOnClone()`, created from `ModEntry` init (BaseLib registers fields at construction); log that it registered.
- [ ] **Step 2:** In `ForgeFusion.Apply` (and `Build`), when the result is a curse and `ForgedCurses.Applies`, set its defects to
  `Merge(Get(kept), Get(sacrificed), result.CurrentUpgradeLevel, n => rng.NextInt(n))`, `rng` being the mod's seeded `Rng`
  for `DefectStream(floor, deck index of kept, 0)` (same helper as the callers of `CurseRule.StartStream`; fusion is a synced
  rest site action, so every client draws the same). Log `forge: curse defects X -> Y`.
- [ ] **Step 3:** Play check: a postfix on the `CardModel` playability method vanilla's Unplayable check answers (find it in
  `~/Projects/sts-decomp/sts2-0.111.1/`) returns false when `BlocksPlay(Get(card), combat.RoundNumber, other cards the owner
  played this turn)`, with a can't-play line (`CantPlayMessagePatch` pattern, key `SPOON_CURSE_DEFECT.cantPlay`).
  `PlayableCurseCostPatch` passes its result through `CostWithDefects(defects, cost, playable: true)`.
- [ ] **Step 4:** Text: a `GetDescriptionForPile` postfix (like `ForgeBonusTextPatch`) appends one `[purple]` line per defect,
  keys `SPOON_CURSE_DEFECT.<Name>` in `cards.json`, EN/FR from the spec table ("Odd: Unplayable on odd turns." /
  "Impaire : injouable aux tours impairs."). Only for `ForgedCurses.Applies` cards with defects.
- [ ] **Step 5: Run** `make build test` — Expected: 0 warnings, all pass.
- [ ] **Step 6: Commit** `git commit -m "Forged curses: defects drawn at fusion, play checks and card text"`.

### Task 5: Defect effects when played and in hand

**Files:**
- Create: `src/Curses/Forged/CurseDefectEffects.cs`
- Modify: `src/Curses/SpoonUnholyGrimoire.cs` (`AfterCardPlayed`), `src/Curses/CurseUpgradePatch.cs` (`RegretPatch`), the
  vanilla curse end-of-turn effects for Spiteful

**Interfaces:**
- Consumes: `CurseDefects.Get`, `EchoAmount`, `BloodyHp`, `SpitefulTimes`, `CursedDeckCurses.Pool`, `DefectStream`.

- [ ] **Step 1:** In `AfterCardPlayed`, for a defective curse: Bloody first (`CreatureCmd.Damage` on the owner,
  `Unblockable | Unpowered`, `BloodyHp(level)`), then the played effect, then Echo (same switch on the owner with
  `EchoAmount(amount)`; GainGold → `PlayerCmd.LoseGold`), Contagious, Ravenous. Bloody, Contagious and Ravenous also work on
  "None" rows (which today return early); Echo does nothing there.
- [ ] **Step 2:** Spiteful: the curse's end-of-turn in-hand effect runs `SpitefulTimes` times (Regret's patch multiplies its
  HP loss; other curses: a postfix awaiting the original turn-end task a second time).
- [ ] **Step 3: Run** `make build test` — Expected: 0 warnings, all pass.
- [ ] **Step 4: Commit** `git commit -m "Forged curses: Bloody, Echo, Contagious, Ravenous and Spiteful effects"`.

### Task 6: Review page, checklist, docs, version, Workshop

**Files:**
- Modify: `tools/curse_table.py`, `tools/tests/test_curse_table.py`, `docs/review/cursed-curses.md` (regenerated),
  `mod/SlaySpoon.json`, `src/ModEntry.cs`, `CLAUDE.md` (1.10.0 section), `README.md`, `workshop/description*.txt`
- Create: `docs/testing/forged-curses-checklist.md`

- [ ] **Step 1:** Test in `test_curse_table.py`: the Bad Luck row lists 13, 20, 27 … 76; a no-effect curse lists Grimoire
  5, 12, 15, 19 … 40 (the C# formulas mirrored in Python). Run `make test` — FAIL; implement; run again — PASS.
- [ ] **Step 2:** Checklist (unchecked): fuse two Bad Luck to +2 → 27; Duplicate a curse; +3 → one defect line; +6 Retain;
  +9 Innate and three defects; Odd/Even/Late/Demanding refuse with a message; Heavy cost; Bloody at 1 HP; Echo on Debt;
  Contagious; Ravenous; Spiteful Regret; save/reload at +7 with defects; co-op client sees the same defects; Smith/Black
  Censer on a +3 curse does nothing; Forge alone and Cursed Deck alone unchanged.
- [ ] **Step 3:** Bump to 1.10.0; `make build test workshop`; check `build/workshop/` holds no bot code
  (`grep -ril "autoslay\|botrun" build/workshop` empty).
- [ ] **Step 4: Commit** `git commit -m "1.10.0: forged curses (Forge + Cursed Deck)"`; Steam changenote draft in the final
  report. No push, no upload.

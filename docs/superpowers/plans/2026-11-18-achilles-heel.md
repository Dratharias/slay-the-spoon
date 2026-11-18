# Achilles' Heel Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the Custom Run mode `SpoonAchillesHeel`. Every player gets the Achilles' Heel relic, which carries four
rules: the heel (unblocked attack damage kills), Dexterity per act, Block hits and Block-leaning card offers. Every
player also gets Anchor, Oddly Smooth Stone and Gorget.

**Architecture:** The pure numbers live in `src/Core/AchillesRule.cs`, with xUnit tests. A relic class in
`src/Achilles/` applies the heel power and Dexterity at combat start and adds the damage bonus. A modifier in
`src/Config/` hands out the four relics. The offer lean extends the existing `OfferWeights` and `OfferWeightPatch`.

**Tech Stack:** C# .NET 9, Harmony, StS2 0.111.1 (decompile in `~/Projects/sts-decomp/sts2-0.111.1/`), xUnit, Godot
PCK for text and art.

**Spec:** `docs/superpowers/specs/2026-11-18-achilles-heel-design.md`

## Global Constraints

- Custom Run only, off by default; Standard and Daily untouched; vanilla seeds unchanged (no vanilla RNG consumed).
- HP, max HP and healing are never changed by the mode.
- Every text in EN and FR (`godot/SlaySpoon/localization/eng|fra/`); French "Talon d'Achille", matching the game's
  French terms (Armure, Dextérité, Placage).
- Harmony helper methods never named Prefix/Postfix/Transpiler/Finalizer/Prepare.
- Build 0 warnings; `make test` green; commits end with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Relic art: a Pixabay photo of a Greek war sandal (Pixabay licence), source URL recorded in `docs/backlog.md`.

## Review Focus

- A player without the relic (other players in co-op, or runs without the mode): no heel, no Dexterity, no bonus,
  vanilla offer draws. Task 1 test `NoRelicMeansUniformWeights`; Task 2 tests gate every hook on `Owner`.
- Heel only on powered attack damage with `UnblockedDamage > 0`: Poison and HP loss never kill. Task 2 test
  `HeelTriggersOnlyOnUnblockedPoweredAttack`.
- Act index on appended acts (The Ending, the Spoon act): act 4 gives 3 Dexterity. Task 1 test `DexterityByAct`.
- Save and reload in the middle of a run: the relic is saved, and the start relics are not given twice. Task 3 checks
  `AfterRunCreated` runs once per run, with a guard that skips relics already held.
- Block bonus with 0–4 Block is 0, never negative. Task 1 test `BlockBonus`.

---

### Task 1: AchillesRule and the Block offer lean

**Files:**
- Create: `src/Core/AchillesRule.cs`, `tests/AchillesRuleTests.cs`
- Modify: `src/Core/OfferWeights.cs` (Block lean factor), `tests/OfferWeightsTests.cs` (or the existing weights test file)

**Interfaces:**
- Produces:
  - `AchillesRule.BlockPerBonus = 5`
  - `AchillesRule.BlockLean = 2.0`
  - `AchillesRule.StartRelics`: the entries `ANCHOR`, `ODDLY_SMOOTH_STONE` and `GORGET`
  - `int AchillesRule.DexterityFor(int actIndex)`: 0-based act index, returns max(0, actIndex)
  - `int AchillesRule.BlockBonus(int block)`: returns floor(max(0, block) / 5)
  - `OfferWeights.Compute(cards, lean, completers, ISet<string>? blockCards = null)`: a card whose id is in
    `blockCards` gets its weight × `AchillesRule.BlockLean`. A null set changes nothing.

- [ ] **Step 1: Write the failing tests**

```csharp
[Theory] [InlineData(0,0)] [InlineData(1,1)] [InlineData(2,2)] [InlineData(3,3)]
public void DexterityByAct(int act, int dex) => Assert.Equal(dex, AchillesRule.DexterityFor(act));
[Theory] [InlineData(0,0)] [InlineData(4,0)] [InlineData(5,1)] [InlineData(14,2)] [InlineData(-3,0)]
public void BlockBonus(int block, int bonus) => Assert.Equal(bonus, AchillesRule.BlockBonus(block));
[Fact] public void BlockCardsWeighDouble() { /* two Neutral cards, blockCards={"A"} → weights [2,1] */ }
[Fact] public void NoRelicMeansUniformWeights() { /* blockCards null → IsUniform */ }
```

- [ ] **Step 2: Run `make test` and check that the new tests fail.**
- [ ] **Step 3: Implement `AchillesRule` and the extra `Compute` parameter.**
- [ ] **Step 4: Run `make test`. Expected: all tests pass.**
- [ ] **Step 5: Commit** `feat(achilles): heel rules and Block offer lean`

### Task 2: the relic, the heel power and the offer hook

**Files:**
- Create: `src/Achilles/SpoonAchillesHeel.cs` (the relic), `src/Achilles/SpoonHeelPower.cs`
- Create: the relic art under `godot/SlaySpoon/images/relics/` (Pixabay, sized like the Unholy Grimoire's files; see
  how `src/Curses/SpoonUnholyGrimoire.cs` points to its art)
- Modify: `src/CardSets/OfferWeightPatch.cs` (`Weights` passes `blockCards` when `player` holds the relic)
- Modify: EN/FR `relics.json` and `powers.json`; `docs/backlog.md` (art source)

**Interfaces:**
- Consumes: `AchillesRule` (Task 1)
- Produces:
  - `SpoonAchillesHeel : RelicModel` (Rarity Event, kept out of pools by Task 3)
  - `SpoonHeelPower : PowerModel`, `PowerType.Buff`, Single stack. A Buff, so Artifact never blocks it. It has no
    removal path.
  - `bool AchillesBlock.IsBlockCard(CardModel)`: `GainsBlock`, or a `BlockVar` among its `DynamicVars`.

- [ ] **Step 1: Write the heel power.** Copy the trigger of vanilla `TheGambitPower.AfterDamageReceived`: when
  `target == Owner && props.IsPoweredAttack() && result.UnblockedDamage > 0`, flash, then
  `await CreatureCmd.Kill(Owner)`. Unlike vanilla, the power is not removed, so the heel stays after a revive.
  Borrow a vanilla power's icon, as `IBorrowedIcon` does for the StS1 powers; pick The Gambit's icon if it has one.
- [ ] **Step 2: Write the relic.** At combat start, through the hook that Oddly Smooth Stone uses (check
  `OddlySmoothStone.cs`), apply `SpoonHeelPower` (1). Also apply Dexterity of
  `AchillesRule.DexterityFor(<current act index>)` when that is above 0. Read the act index from the run state the
  same way `MutationModifier` does.
  For damage, add `ModifyDamageAdditive`: when the dealer is the owner's creature and `props.IsPoweredAttack()`, add
  `AchillesRule.BlockBonus(Owner.Creature.Block)`. Use the exact hook signature from the decompile.
- [ ] **Step 3: Offer hook.** In `OfferWeightPatch.Weights`, when the player holds `SpoonAchillesHeel`, build
  `blockCards` from `cards.Where(AchillesBlock.IsBlockCard)` and pass it to `Compute`. The early return
  `completers.Count == 0 && lean == null` must also check that `blockCards` is null.
- [ ] **Step 4: Add the text.**
  - Relic title: "Achilles' Heel" / "Talon d'Achille".
  - Relic description, EN: "If you take unblocked attack damage, die. Gain 1 [gold]Dexterity[/gold] per act past
    the first. Your attacks deal 1 more damage per {BlockPerBonus} [gold]Block[/gold] you have." FR in the game's
    wording.
  - Power: title and description EN/FR.
  - Add a short flavor line in both languages.
- [ ] **Step 5: Add a test for the trigger.** If `TheGambitPower`'s condition can be put in a pure method, test
  `HeelTriggersOnlyOnUnblockedPoweredAttack` in Core: a bool helper
  `AchillesRule.HeelKills(bool poweredAttack, int unblocked)`.
- [ ] **Step 6: Run `make build` and `make test`. Expected: 0 warnings, all tests pass.**
- [ ] **Step 7: Commit** `feat(achilles): Achilles' Heel relic and heel power`

### Task 3: the mode, docs and version

**Files:**
- Create: `src/Config/AchillesHeelModifier.cs` (`SpoonAchillesHeel` is the relic name, so the modifier class is
  `SpoonAchillesHeelMode`, saved id `SPOON_ACHILLES_HEEL_MODE`)
- Modify: `src/CustomRunUi/ModifierListPatch.cs`, `src/TopBar/TopBarPercentPatch.cs` flag list, EN/FR `modifiers.json`
- Create: `docs/testing/achilles-heel-checklist.md`
- Modify: `CLAUDE.md` (a new section), `mod/SlaySpoon.json` and `src/ModEntry.cs` (version: next minor after the
  current one)

**Interfaces:**
- Consumes: `SpoonAchillesHeel`, `AchillesRule.StartRelics`
- Produces: `SpoonAchillesHeelMode.IsActive(IRunState?)`

- [ ] **Step 1: Write the modifier.** Follow `src/Config/DeckOfTenModifier.cs`. In `AfterRunCreated`, for each
  player, obtain `SpoonAchillesHeel` and the 3 `StartRelics`, skipping any the player already holds. Then remove all
  four from `player.RelicGrabBag` and `SharedRelicGrabBag`.
- [ ] **Step 2: Register the mode and add its text.**
  - Register it in `ModifierListPatch` and in the top-bar flag list.
  - `modifiers.json` EN/FR: the title, a short description (memory: menu texts stay short) and the `.short` label.
    FR title "Talon d'Achille".
- [ ] **Step 3: Write the checklist.** It covers these in-game checks:
  - the heel kills on an unblocked hit;
  - Poison doesn't kill;
  - Dexterity is 1 in act 2;
  - the damage bonus shows on the attack intent and preview;
  - the start relics are given once, and not again after a reload;
  - offers lean toward Block cards;
  - in co-op, a killed player comes back after combat.
- [ ] **Step 4: Update the docs and the version.** Add a CLAUDE.md section (version, files, "Not tested in game")
  and bump the version.
- [ ] **Step 5: Run `make build` and `make test`. Expected: 0 warnings, all tests pass.**
- [ ] **Step 6: Commit** `feat(achilles): Achilles' Heel Custom Run mode`

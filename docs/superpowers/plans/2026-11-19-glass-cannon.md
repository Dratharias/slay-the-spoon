# Glass Cannon Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship the Glass Cannon Custom Run mode (version 1.8.0): a start relic that applies Frail 99 and the Frenzy power
every combat, plus Vajra and Girya, and the combined Achilles' Heel offer lean.

**Architecture:** Same shape as Achilles' Heel. A `ModifierModel` (`SpoonGlassCannon`) hands out the relics at run
creation. The relic (`SpoonGlassHeart`) applies the powers at combat start. The power (`SpoonFrenzyPower`) holds the
stacks and streak. All numbers and state transitions live in a pure rule in `src/Core/` with xUnit tests.

**Tech Stack:** C# .NET 9, Harmony, StS2 0.111.1 models (decompile in `~/Projects/sts-decomp/sts2-0.111.1/`), xUnit,
`make build` / `make test` (Podman).

**Spec:** `docs/superpowers/specs/2026-11-19-glass-cannon-design.md`

## Global Constraints

- Every text the mod adds goes in both `godot/SlaySpoon/localization/eng/` and `.../fra/`. French uses the game's words:
  "Armure" (Block), "Fragile" (Frail), "Force" (Strength), apostrophe `’`.
- Menu and relic texts stay short (owner rule: let players discover).
- Custom Run only, off by default. Standard and Daily untouched. Never write the unmodded save.
- Vanilla RNG untouched: no vanilla `Rng` draw is added or removed.
- Names: mode `SpoonGlassCannon` ("Glass Cannon" / "Canon de verre"), relic `SpoonGlassHeart` ("Glass Heart" /
  "Cœur de verre"), power `SpoonFrenzyPower` ("Frenzy" / "Frénésie").
- Numbers: +10 % damage per stack (linear), Frail 99, streak Strength = streak length, start relics `VAJRA`, `GIRYA`.
- Harmony applies any method named `Prefix`, `Postfix`, `Transpiler`, `Finalizer` or `Prepare` in a patch class:
  helpers need other names.
- Build must stay at 0 warnings / 0 errors.
- Commit on branch `glass-cannon`. No push, no Workshop upload.

## Review Focus

- **Replay and autoplay** (Burst, Havoc-style, Replay N): each `CardPlay` counts as one played card, so a replayed
  card gives one stack per play. A replayed Block Skill resets once per play (harmless). Pinned in Task 1 by
  `CardPlayedCountsEveryPlay`.
- **A Block Skill played with stacks and a streak**: stacks and streak both go to 0, the turn gives no Strength,
  Strength already gained stays. Pinned in Task 1 by `BlockSkillResetsStacksAndStreak` and `BlockTurnGivesNoStrength`.
- **Powers that carry a `BlockVar`** (Pillar of Creation, Shroud) and Attacks that gain Block (Iron Wave) never break the
  combo. Pinned in Task 1 by `OnlyNonAttackNonPowerBlockCardsBreak`.
- **Damage from Osty vs. other allies**: Frenzy multiplies the holder's and the holder's Osty's powered attacks only, not
  another player's or their Osty. Checked in the in-game checklist (game types, not unit-testable); Task 2 states the
  dealer check exactly.
- **Achilles + Glass Cannon offer lean**: a card that is both an Attack and a Block card weighs ×2, not ×4. Pinned in
  Task 3 by `CombinedLeanCountsACardOnce`.

---

### Task 1: Pure rule `GlassCannonRule`

**Files:**
- Create: `src/Core/GlassCannonRule.cs`
- Test: `tests/GlassCannonRuleTests.cs`

**Interfaces:**
- Produces:
  - `public readonly record struct FrenzyState(int Stacks, int Streak, bool BlockThisTurn)` with
    `public static FrenzyState Start => new(0, 0, false);`
  - `public static class GlassCannonRule` with:
    - `const decimal BonusPerStack = 0.10m;`
    - `const int FrailAmount = 99;`
    - `static readonly IReadOnlyList<string> StartRelics = ["VAJRA", "GIRYA"];`
    - `static decimal DamageMultiplier(int stacks)`: `1 + BonusPerStack * max(0, stacks)`
    - `static bool BreaksFrenzy(bool isAttack, bool isPower, bool blockCard)`: `blockCard && !isAttack && !isPower`
    - `static FrenzyState CardPlayed(FrenzyState s, bool breaks)`: breaks → `(0, 0, true)`; else `Stacks + 1`
    - `static (FrenzyState Next, int Strength) TurnEnded(FrenzyState s)`: no Block card this turn → streak + 1 and
      Strength = new streak; else Strength 0, streak stays 0. `BlockThisTurn` is false in `Next`.

- [ ] **Step 1: Write the failing tests** in `tests/GlassCannonRuleTests.cs` (style of `tests/AchillesRuleTests.cs`)

```csharp
[Theory] [InlineData(0, 1.0)] [InlineData(1, 1.1)] [InlineData(10, 2.0)] [InlineData(15, 2.5)] [InlineData(-2, 1.0)]
public void MultiplierIsLinear(int stacks, double expected) => Assert.Equal((decimal)expected, GlassCannonRule.DamageMultiplier(stacks));

[Theory]
[InlineData(false, false, true, true)]   // Block Skill
[InlineData(true, false, true, false)]   // Iron Wave
[InlineData(false, true, true, false)]   // Pillar of Creation, Shroud
[InlineData(false, false, false, false)] // plain Skill
public void OnlyNonAttackNonPowerBlockCardsBreak(bool attack, bool power, bool block, bool breaks) =>
	Assert.Equal(breaks, GlassCannonRule.BreaksFrenzy(attack, power, block));

[Fact] public void CardPlayedCountsEveryPlay()  // 3 plays (e.g. a Replay 2 card) → 3 stacks
[Fact] public void BlockSkillResetsStacksAndStreak()  // (7, 2, false) + breaking card → (0, 0, true)
[Fact] public void StreakGivesOneTwoThree()  // three clean turns → Strength 1, 2, 3 (sum 6), stacks kept across turns
[Fact] public void BlockTurnGivesNoStrength()  // turn with a Block Skill → Strength 0; next clean turn → Strength 1
[Fact] public void TurnEndClearsBlockFlag()
[Fact] public void StartRelicsAreVajraAndGirya() => Assert.Equal(new[] { "VAJRA", "GIRYA" }, GlassCannonRule.StartRelics);
[Fact] public void FrailIs99() => Assert.Equal(99, GlassCannonRule.FrailAmount);
```

- [ ] **Step 2: Run** `make test` — expected: compile failure, `GlassCannonRule` not found.
- [ ] **Step 3: Implement `src/Core/GlassCannonRule.cs`** with the interfaces above (pure, no game types, doc comment
  pointing at the spec).
- [ ] **Step 4: Run** `make test` — expected: all pass, including the new tests.
- [ ] **Step 5: Commit** `feat(glass-cannon): pure rule for Frenzy stacks and streak`

### Task 2: Power, relic, mode and text

**Files:**
- Create: `src/GlassCannon/SpoonFrenzyPower.cs`, `src/GlassCannon/SpoonGlassHeart.cs` (relic + event pool patch, as
  `src/Achilles/SpoonAchillesHeel.cs`), `src/GlassCannon/GlassCannonBlock.cs`, `src/Config/GlassCannonModifier.cs`
- Create: `godot/SlaySpoon/images/relics/spoon/spoon_glass_heart.png`, `_outline.png`, `_large.png` (+ Godot `.import`
  files are produced by the PCK build; copy the Achilles ones' layout)
- Modify: `src/CustomRunUi/ModifierListPatch.cs` (append after `SpoonAchillesHeelMode`),
  `src/TopBar/TopBarPercentPatch.cs:49` (add `or SpoonGlassCannon`)
- Modify: `godot/SlaySpoon/localization/{eng,fra}/{modifiers,relics,powers}.json`
- Modify: `docs/backlog.md` (art source under "Curse deck relics and art sources" list style)

**Interfaces:**
- Consumes: Task 1's `GlassCannonRule`, `FrenzyState`.
- Produces:
  - `SlaySpoon.GlassCannon.GlassCannonBlock.BreaksFrenzy(CardModel card) : bool` =
    `GlassCannonRule.BreaksFrenzy(card.Type == CardType.Attack, card.Type == CardType.Power, AchillesBlock.IsBlockCard(card))`
  - `SlaySpoon.GlassCannon.SpoonGlassHeart : RelicModel` (Event rarity)
  - `SlaySpoon.Config.SpoonGlassCannon : ModifierModel` with `public static bool IsActive(IRunState? run)`

- [ ] **Step 1: `SpoonFrenzyPower`**: `PowerType.Buff`, `PowerStackType.Single`, `IBorrowedIcon` (a vanilla
  offensive power's icon, builder's pick). Holds a `FrenzyState` (start `FrenzyState.Start`). `DisplayAmount` = bonus
  percent (stacks × 10), refreshed with `InvokeDisplayAmountChanged()` as `SlowPower` does.
  - `AfterCardPlayed`: only when `cardPlay.Player == Owner.Player`; `CardPlayed(state, GlassCannonBlock.BreaksFrenzy(cardPlay.Card))`.
  - `ModifyDamageMultiplicative`: returns `GlassCannonRule.DamageMultiplier(stacks)` when `props.IsPoweredAttack()` and
    `dealer == Owner || dealer == Owner.Player?.Osty`; else `1m`. Flash in `AfterModifyingDamageAmount` like Slow.
  - Turn end: `AfterSideTurnEnd` for the player side containing `Owner`: `TurnEnded`, then
    `PowerCmd.Apply<StrengthPower>` for `Strength > 0` (on the holder only, never Osty).
  - Not removed on the holder's death in co-op (`ShouldPowerBeRemovedAfterOwnerDeath() => false`, as `SpoonHeelPower`).
- [ ] **Step 2: `SpoonGlassHeart`**: in `AfterRoomEntered` for a `CombatRoom`: `Flash()`, apply `FrailPower`
  `GlassCannonRule.FrailAmount`, then `SpoonFrenzyPower` 1. Hover tips: Frenzy and Frail. Event-pool postfix like
  `AchillesHeelPoolPatch`.
- [ ] **Step 3: `SpoonGlassCannon`**: copy `SpoonAchillesHeelMode.AfterRunCreated` with
  `SpoonGlassHeart` + `GlassCannonRule.StartRelics` (remove from each `RelicGrabBag` and `SharedRelicGrabBag`, skip
  relics already held). Register it in `ModifierListPatch` and `TopBarPercentPatch`.
- [ ] **Step 4: Text (EN/FR)**: keys `SPOON_GLASS_CANNON.{title,description,short}`, `SPOON_GLASS_HEART.{title,description,flavor}`,
  `SPOON_FRENZY_POWER.{title,description,smartDescription}`. Exact copy:
  - Mode: "Everyone starts with Glass Heart, Vajra and Girya." / "Tout le monde commence avec le Cœur de verre, le Vajra
    et le Girya." Short: "Glass" / "Verre".
  - Relic: "At the start of each combat, gain 99 [gold]Frail[/gold] and [gold]Frenzy[/gold]." / "Au début de chaque
    combat, gagnez 99 [gold]Fragile[/gold] et [gold]Frénésie[/gold]." Flavor: "\"Hit first. Hit hard. Hit last.\"" /
    "« Frapper en premier. Frapper fort. Frapper en dernier. »"
  - Power: "Each card played adds 10% attack damage. Each turn without a Block Skill gives more [gold]Strength[/gold]
    (1, 2, 3...). Playing a Skill that gives [gold]Block[/gold] resets both." / "Chaque carte jouée ajoute 10 % de dégâts
    d’attaque. Chaque tour sans Compétence d’[gold]Armure[/gold] donne plus de [gold]Force[/gold] (1, 2, 3...). Jouer une
    Compétence qui donne de l’[gold]Armure[/gold] remet tout à zéro." Check the French name of "Skill" in the game PCK
    (`grep -a -o '"SKILL[^"]*": *"[^"]*"' SlayTheSpire2.pck`) and use it.
- [ ] **Step 5: Art**: placeholder relic icon (a Pixabay photo or illustration of glass: cracked glass heart, glass
  cannonball…) under the Pixabay Content License, resized like `spoon_achilles_heel*.png` (same sizes; outline = the
  Achilles outline method). Record the URL in `docs/backlog.md`. If no image can be fetched, derive one with `magick`
  from an existing mod relic and say so in the backlog.
- [ ] **Step 6: Run** `make build && make test` — expected: 0 warnings / 0 errors; all tests pass, including
  `LocalizationTablesTests` and `RelicTextPlaceholderTests`.
- [ ] **Step 7: Commit** `feat(glass-cannon): Glass Heart relic, Frenzy power and the mode`

### Task 3: Combined Achilles + Glass Cannon offer lean

**Files:**
- Modify: `src/CardSets/OfferWeightPatch.cs:92-96` (`Weights`)
- Test: `tests/OfferWeightsTests.cs`

**Interfaces:**
- Consumes: `SpoonGlassHeart` (Task 2), `AchillesBlock.IsBlockCard`, `OfferWeights.Compute(..., ISet<string>? blockCards)`.
- Produces: no new API. A player holding both `SpoonAchillesHeel` and `SpoonGlassHeart` passes as `blockCards` the set of
  cards that are Attacks **or** `AchillesBlock.IsBlockCard`. Achilles alone: unchanged. Glass Heart alone: no lean.

- [ ] **Step 1: Write the failing test** `CombinedLeanCountsACardOnce` in `tests/OfferWeightsTests.cs`: three cards
  (an Attack that gains Block, a Block Skill, a plain Power) with `blockCards` = {first, second} → weights
  `{AchillesRule.BlockLean, AchillesRule.BlockLean, 1.0}`. (It passes against `Compute` as is; it pins that the union is
  passed once, never two sets multiplied.)
- [ ] **Step 2: Change `Weights`**: build the set with
  `c => AchillesBlock.IsBlockCard(c) || (holdsGlass && c.Type == CardType.Attack)` when the player holds the Achilles relic.
- [ ] **Step 3: Run** `make build && make test` — expected: 0 warnings; all pass.
- [ ] **Step 4: Commit** `feat(glass-cannon): Attack lean with Achilles' Heel`

### Task 4: Release docs and version 1.8.0

**Files:**
- Create: `docs/testing/glass-cannon-checklist.md` (style of `docs/testing/achilles-heel-checklist.md`)
- Create: `workshop/changelog-1.8.0.txt`
- Modify: `mod/SlaySpoon.json`, `src/ModEntry.cs` (version `1.8.0`), `workshop/description.txt` and `README.md` (one
  short mode line, like Achilles'), `CLAUDE.md` (a "1.8.0 — Glass Cannon" section like 1.7.0's), `docs/backlog.md`
  (replace the "Glass Cannon (owner works the idea)" line with a built pointer, minors found during the work)

- [ ] **Step 1: Checklist** covers: relics at start (and not in later rewards); Frail 99 and Frenzy each combat;
  stacks per card and the displayed percent; Block Skill resets, Iron Wave and Pillar of Creation don't; streak
  Strength 1/2/3 at end of turn; Osty damage multiplied, another player's not; Artifact blocks Frail; Girya lift; the
  Heart (3 turns ≤ A9); co-op per-player state; combined Achilles lean; save and reload mid-combat (record what the Frenzy state does).
- [ ] **Step 2: Version bump, changelog, README/description, CLAUDE.md, backlog.**
- [ ] **Step 3: Run** `make build && make test && make workshop` — expected: 0 warnings, all tests pass, staging prints
  `version 1.8.0`.
- [ ] **Step 4: Commit** `docs(glass-cannon): 1.8.0 checklist, changelog and notes`

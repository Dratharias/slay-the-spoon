# Spoon Cutlery Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The Spoon fights on a 4-turn cycle and pollutes every player's draw pile each cycle with a cutlery kit
(Teaspoon, Wooden Spoon, Skimmer, Melon Baller) sized by deck; its Strength follows its max HP.

**Architecture:** Numbers and rules in `src/Core/SpoonRule.cs` (xUnit-tested); four `SpoonCard`s and two small powers
in `src/Spoon/`; `SpoonMonster` gets the new move machine and calls back into `SpoonRule`.

**Tech Stack:** C# .NET 9, StS2 models, Harmony; `make build`, `make test` (Podman).

**Spec:** `docs/superpowers/specs/2026-11-16-spoon-cutlery-design.md`

## Global Constraints

- Every text in EN and FR (`godot/SlaySpoon/localization/{eng,fra}/`); repo text in English.
- Harmony helpers never named Prefix/Postfix/Transpiler/Finalizer/Prepare.
- Cutlery is combat-only (`AddGeneratedCardsToCombat`), never reaches a deck; outside every card source (`SpoonCard`, token rarity, `CanBeGeneratedInCombat => false`).
- Heal on Feast: 60 + 10 per player past the first (60, 70, 80, 90).
- Strength = (max HP − max HP at fight start) / 5, truncated toward zero, negative allowed (`StrengthPower.AllowNegative`).
- Kit table exactly as the spec's "Kit by deck size".
- Commits end with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`. No push.

## Review Focus

- A dead player gets no kit; a supporter does (spec: every player's draw pile). Test in Task 3 by reading the loop.
- Melon Baller on a Spoon with ≤ 15 max HP: takes at most max HP − 1, never kills through max HP (`SpoonRule.BallerTake`).
- Several Wooden Spoons in one turn stack: each blocks one future Appetite gain (Counter power).
- Strength recomputed after every max HP change the mod makes (growth, Melon Baller); never drifts (set by delta to the target).
- Skimmer protection ends after the Spoon's turn even when the Spoon's move was not Feast.

---

### Task 1: Rules in SpoonRule

**Files:**
- Modify: `src/Core/SpoonRule.cs`
- Test: `tests/SpoonRuleTests.cs`

**Interfaces:**
- Produces:
  - `enum SpoonMove { SetTable, Bouchee, Feast, Ladle }`; `SpoonMove Move(int turn)` (turn 0-based: 0 SetTable, 1 Bouchee, 2 Feast, 3 Ladle, repeating).
  - constants `BoucheeDamage = 35`, `LadleDamage = 55`, `FeastMaxHp = 10` (rename of BoucheeMaxHp), `FrailAmount = 2`, `VulnerableAmount = 2`, `FeastHealBase = 60`, `FeastHealPerPlayer = 10`, `MaxHpPerStrength = 5`, `TeaspoonAppetite = 2`, `SkimmerBlock = 6`, `BallerMaxHp = 15`.
  - `int StrengthFor(int maxHp, int startMaxHp)`; `int FeastHeal(int players)`; `int BallerTake(int spoonMaxHp)` = clamp(15, 0, maxHp − 1).
  - `record struct Kit(int Teaspoons, bool Wooden, bool Skimmer, bool Baller)`; `Kit KitFor(int deckSize)`.
- Removes: `StrengthFromGrowth`, `BoucheeMaxHp` (and their tests).

- [ ] **Step 1: Write the failing tests**

```csharp
[Theory]
[InlineData(0, SpoonRule.SpoonMove.SetTable)] [InlineData(1, SpoonRule.SpoonMove.Bouchee)]
[InlineData(2, SpoonRule.SpoonMove.Feast)] [InlineData(3, SpoonRule.SpoonMove.Ladle)] [InlineData(4, SpoonRule.SpoonMove.SetTable)]
public void CycleOfFour(int turn, SpoonRule.SpoonMove move) => Assert.Equal(move, SpoonRule.Move(turn));

[Theory]
[InlineData(2000, 2000, 0)] [InlineData(2009, 2000, 1)] [InlineData(2010, 2000, 2)]
[InlineData(1996, 2000, 0)] [InlineData(1995, 2000, -1)] [InlineData(1985, 2000, -3)]
public void StrengthFollowsMaxHp(int hp, int start, int s) => Assert.Equal(s, SpoonRule.StrengthFor(hp, start));

[Theory] [InlineData(1, 60)] [InlineData(2, 70)] [InlineData(3, 80)] [InlineData(4, 90)]
public void FeastHealByPlayers(int players, int heal) => Assert.Equal(heal, SpoonRule.FeastHeal(players));

[Theory] [InlineData(2000, 15)] [InlineData(16, 15)] [InlineData(10, 9)] [InlineData(1, 0)]
public void BallerTakeIsCapped(int hp, int take) => Assert.Equal(take, SpoonRule.BallerTake(hp));

[Theory]
[InlineData(0, 1, false, false, false)] [InlineData(9, 1, false, false, false)]
[InlineData(10, 1, true, false, false)] [InlineData(15, 2, true, false, false)]
[InlineData(20, 2, true, true, false)] [InlineData(25, 2, true, true, true)]
[InlineData(30, 4, true, true, true)] [InlineData(44, 4, true, true, true)]
[InlineData(45, 6, true, true, true)] [InlineData(60, 8, true, true, true)]
public void KitBySize(int deck, int tea, bool wood, bool skim, bool baller) =>
	Assert.Equal(new SpoonRule.Kit(tea, wood, skim, baller), SpoonRule.KitFor(deck));
```

- [ ] **Step 2: Run** `make test` — expected: compile failure on the missing members.
- [ ] **Step 3: Implement** the members above in `SpoonRule.cs`; delete `StrengthFromGrowth`, its theory, and the old `Move` theory. `SpoonMonster` will not compile until Task 3: keep the old names it uses as `[Obsolete]`-free aliases only if needed to build, or do Tasks 1–3 before running `make build` (preferred: Task 1 tests run through `make test`, which builds the test project only).
- [ ] **Step 4: Run** `make test` — expected: all xUnit pass.
- [ ] **Step 5: Commit** `feat(spoon): cycle, kit, Strength and Feast rules`.

### Task 2: Cutlery cards and powers

**Files:**
- Create: `src/Spoon/SpoonCutlery.cs` (`SpoonTeaspoon`, `SpoonWoodenSpoon`, `SpoonSkimmer`, `SpoonMelonBaller`), `src/Spoon/SpoonSatedPower.cs`, `src/Spoon/SpoonSkimmedPower.cs`
- Modify: `src/Spoon/SpoonCardPool.cs` (add the four), `godot/SlaySpoon/localization/{eng,fra}/{cards,powers}.json`, `tests/SpoonTextTests.cs`
- Placeholder portraits: copy `spoon_spoonful.png` to `images/cards/spoon/<id>.png` for the four ids (replaced in Task 4).

**Interfaces:**
- Consumes: `SpoonFight.Find(ICombatState?)`, Task 1 constants.
- Produces: `SpoonSatedPower` (Buff, Counter, on the Spoon; each stack blocks one Appetite gain), `SpoonSkimmedPower` (Buff, Single, on a player; Feast skips their max HP loss), `SpoonMonster.SyncStrength()` is called by the Melon Baller (defined in Task 3; add a stub here returning `Task.CompletedTask`).

Cards (all `CardKeyword.Exhaust`, `TargetType.None`, act on `SpoonFight.Find(CombatState)`; no-op when it is null or dead):
- Teaspoon: cost 0 Skill; Appetite −min(2, amount) via `PowerCmd.ModifyAmount`; draw 1.
- Wooden Spoon: cost 1 Skill; apply `SpoonSatedPower` 1 to the Spoon.
- Skimmer: cost 1 Skill; `BlockVar(6)` gain; apply `SpoonSkimmedPower` 1 to the player.
- Melon Baller: cost 2 Skill; `take = BallerTake(spoon.MaxHp)`; `CreatureCmd.SetMaxHp(spoon, max − take)` (clamp current HP by the game), `CreatureCmd.GainMaxHp(owner.Creature, take)`, then `spoon.SyncStrength()`.
Upgrades (builder): Teaspoon draws 2, Wooden Spoon cost 0, Skimmer Block 9, Melon Baller takes 20 (`BallerMaxHp` stays the base; pass the amount).

Text — titles EN/FR: Teaspoon/Cuillère à café, Wooden Spoon/Cuillère en bois, Skimmer/Écumoire, Melon Baller/Cuillère parisienne; powers Sated/Rassasiée ("Its next {Amount} Appetite gain(s) do nothing"), Skimmed/Écumé ("Feast eats none of your max HP this turn"). Match vanilla wording for Block/draw (grep the PCK).

- [ ] **Step 1:** extend `SpoonTextTests.Cards_have_text` ids with `SPOON_TEASPOON`, `SPOON_WOODEN_SPOON`, `SPOON_SKIMMER`, `SPOON_MELON_BALLER`, add a `Powers_have_text` theory for `SPOON_SATED_POWER`, `SPOON_SKIMMED_POWER` (title, description, smartDescription), and the four portraits to `Spoon_art_exists`.
- [ ] **Step 2: Run** `make test` — expected FAIL on the missing keys/files.
- [ ] **Step 3:** implement the classes, pool entries, text, placeholder portraits.
- [ ] **Step 4: Run** `make build` (0 warnings) and `make test` — expected PASS.
- [ ] **Step 5: Commit** `feat(spoon): cutlery cards, Sated and Skimmed`.

### Task 3: The Spoon's cycle

**Files:**
- Modify: `src/Spoon/SpoonMonster.cs`, `godot/SlaySpoon/localization/{eng,fra}/monsters.json`, `tests/SpoonTextTests.cs` (`Boss_has_text` keys)

**Interfaces:**
- Consumes: Task 1 and Task 2.
- Produces: `internal Task SyncStrength()` on `SpoonMonster`.

Behaviour:
- `_startMaxHp` recorded in `BeforeCombatStart`.
- Moves (state machine via `SpoonRule.Move`): `SET_TABLE_MOVE` (intents `DebuffIntent`, `StatusIntent(kit size of the first fighter)`): Frail 2 on each fighter; for every living player, the kit for `player.Deck.Cards.Count` (`CombatState.CreateCard<T>(player)` per card) via `CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Draw, player, CardPilePosition.Random)`. `BOUCHEE_MOVE` (`SingleAttackIntent(35)`, `DefendIntent`): attack, then `CreatureCmd.GainBlock(Creature, unblocked, ValueProp.Unpowered, null)` when > 0. `FEAST_MOVE` (`DebuffIntent`, `HealIntent`): Vulnerable 2 on fighters; `LoseMaxHp(FeastMaxHp)` on each fighter without `SpoonSkimmedPower`; `Heal(FeastHeal(players.Count))`. `LADLE_MOVE` (`SingleAttackIntent(55)`). Animations: SetTable and Feast use `Recoil`-free `Grow` pulse (builder), Bouchée swoop, Ladle smash.
- After each move: `Grow(attack)` for attacks (max HP only, then `SyncStrength`), `GainAppetite()` — skipped and one Sated stack removed when Sated is present — then remove every `SpoonSkimmedPower` from players.
- `SyncStrength()`: target = `StrengthFor(MaxHp, _startMaxHp)`, apply the delta to `StrengthPower` (nothing when 0).
- Remove the Mouthful max HP eat and its heal.
- Text EN/FR: `SET_TABLE` "Sets the Table"/"Met le couvert", `FEAST` "Feast"/"Festin"; Bouchée, Ladle unchanged.

- [ ] **Step 1:** add the two move title keys to `Boss_has_text`; **Run** `make test` — FAIL.
- [ ] **Step 2:** implement; **Run** `make build` (0 warnings) and `make test` — PASS.
- [ ] **Step 3: Commit** `feat(spoon): four-turn cycle, cutlery each cycle, Strength from max HP`.

### Task 4: Pixabay portraits (owner request)

**Files:** `godot/SlaySpoon/images/cards/spoon/spoon_{teaspoon,wooden_spoon,skimmer,melon_baller}.png`, `docs/backlog.md` (Art sources)

- [ ] **Step 1:** find one Pixabay photo per card (Pixabay Content License): teaspoon, wooden spoon, skimmer, melon baller. Download into the scratchpad (own directory).
- [ ] **Step 2:** crop/scale to 250×190 RGBA PNG like `spoon_spoonful.png` (ImageMagick `convert -resize 250x190^ -gravity center -extent 250x190`); overwrite the placeholders.
- [ ] **Step 3:** list each URL under "Art sources" in `docs/backlog.md`; `make test` PASS; commit `art(spoon): cutlery portraits from Pixabay`.

### Task 5: Docs, install

**Files:** `docs/superpowers/specs/2026-11-14-spoon-design.md` (point to the cutlery spec), `docs/testing/spoon-checklist.md` (cycle, kit, each card, Strength sign, Feast heal per player count), `CLAUDE.md` (1.3.0 section: one line on the cutlery)

- [ ] **Step 1:** update the three docs.
- [ ] **Step 2:** `make build && make test`; `pgrep -x SlayTheSpire2` empty → `make up`.
- [ ] **Step 3: Commit** `docs(spoon): cutlery checklist and notes`.

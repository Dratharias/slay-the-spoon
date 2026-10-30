# Step 6 WP4-6: The Ending (Act 4) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make StS1's Act 4 playable in StS2: once the team holds all three keys, the run continues after the act 3 boss into rest site, merchant, Spire Shield and Spear, then the Corrupt Heart, and the run is won after the Heart. Then release it (Standard unlock and a Custom Run "Act 4" option).

**Architecture:** The Ending is a mod `ActModel` (`Sts1EndingAct`). It is appended to `RunState.Acts` only when the run carries `SpoonAct4` and the team holds all keys, so a run without keys stays a vanilla three-act run. Since the act list and its rooms are part of the vanilla run save, save/reload and co-op resume carry them. Its fixed four-node map replaces the generated one through `SpoonAct4.ModifyGeneratedMapLate`. The three monsters are hand-written `MonsterModel`s. They choose moves with the pure `EndingMoveRule` and their own `Rng`, and they are drawn as static PNGs rendered from the StS1 Spine 3.4 setup pose by a Python tool. The vanilla `RunManager.EnterNextAct` → `TheArchitect` → `WinRun` path stays the victory contract.

**Tech Stack:** C# .NET 9, Harmony, StS2 v0.107.1 (Godot 4.5.1), xUnit; Python 3.12 + Pillow in the `slayspoon-tools` Podman image, pytest.

**Spec:** `docs/superpowers/plans/2026-10-29-step-6-act4.md` (WP4, WP5, WP6). Mechanics source of truth: `data/sts1/act4/mechanics.json` and `docs/review/act4-mechanics.md`. Previous plan: `docs/superpowers/plans/2026-10-29-step-6-act4-wp3-keys-unlock.md`.

## Global Constraints

Owner rules (CLAUDE.md):
- The owner doesn't write C#. They make the architecture calls and play-test. Ask them about design choices only. Report in French. Everything written to the repo is in English.
- The mod ships in **English and French only**. Every text the mod adds goes in both `godot/SlaySpoon/localization/eng/` and `.../fra/`.
- The mod must **never write the regular (unmodded) save**. Progress sync only reads it.
- Standard and Daily modes remain available; never lock the modes themselves. Daily stays vanilla. Standard exceptions are Spire Souvenir and the approved Act 4 unlock documented above. Lobby activation options live in Custom Run.
- Bug reports are not support requests (see README). Don't promise fixes.
- Never commit decompiled game code.
- Commit on a step branch. Push and publish only on request.

Step 6 spec constraints (verbatim):
- Standard runs use the permanent unlock above; Custom Runs use their activation option. Daily runs remain unchanged.
- No regular-save writes. Progression and keys must survive modded save/reload and co-op resume.
- No install, push or Workshop upload as part of this import.
- EN/FR for all new UI. Extracted StS1 formatting is source data, not ready-to-use StS2 localization.
- Existing monster HP/damage modifiers must apply exactly once to the imported enemies.
- Co-op key ownership (owner decision, 2026-10-29): one team-wide set, as in StS1. A key collected by any player counts for the whole team.
- Enemy mechanics must be verified from local StS1 code before porting, including ascension changes. Do not guess the Heart's attack order, Invincible cap or scaling rules.

Owner decisions from `docs/review/act4-mechanics.md` (verbatim):
1. Ascension mapping. StS1 HP raises (A8/A9) go to StS2 Tough Enemies (A8), damage raises (A3/A4) to Deadly Enemies (A9), and the StS1 A18/A19 changes (Invincible 200, Beat of Death 2, Artifact 2, Smash 99 Block, Burns in the draw pile) to A10 (Double Boss, the highest level).
2. Invincible in co-op. The cap scales like the Heart's HP, with StS2's formula: 300 (or 200) × player count × 1.3, so a co-op fight takes about as many turns as a solo one. Every player's damage shares that one cap.
3. Multiplayer HP. Act 4 enemies follow StS2's formula (HP × player count × act factor), with act 3's factors.

Plan-wide rules:
- Seeds stay vanilla: every random choice the mod adds uses its own `Rng(run.Rng.Seed, "<SlaySpoon name>")` or the monster's own synced RNG, never a vanilla shared RNG.
- Mod localization files must be named after vanilla tables. The game only merges vanilla table names, and `tests/LocalizationTablesTests.cs` enforces this.
- EN/FR for all text.
- Never commit decompiled code. The decompiled StS2 source in `~/Projects/sts-decomp/sts2/` and the StS1 source in `~/Projects/sts-decomp/sts1/` are for reading only. Do not copy from them into the repo.
- No push and no Workshop upload.
- Build and test with `nice -n 19 make build` (expected: 0 warnings / 0 errors) and `nice -n 19 make test` (expected: all xUnit and pytest pass). Do not run `make up`; the owner installs and plays.
- Pure logic goes in `src/Core/` so that `tests/SlaySpoon.Tests.csproj` can link it.
- Every task adds its in-game items to `docs/testing/step-6-checklist.md` under a new heading `## <task title> (WP4-6 Task N)`.

## Review Focus

1. **Save/reload and co-op resume after the keys complete.** A save taken in act 3 with all keys, or anywhere in act 4, must reload with exactly four acts, the same act 4 map, and the same monster moves. The route rule refuses a fifth act (Task 8: `ShouldAppend_NeverAddsASecondEndingAct`). The monster RNG is rebuilt identically on reload (Task 2: `Brain_ReplaysIdenticallyFromTheSameRolls`).
2. **Co-op divergence from a move state that rolls more than once per turn.** If the game asks `EndingBranchState.GetNextState` twice, peers advance differently. The brain only moves in `Advance` (Task 2: `Brain_OnlyAdvanceChangesState`), and Task 4 logs each transition so the owner can compare both peers' logs.
3. **Runs without all keys.** These must end exactly as vanilla after the act 3 boss (Task 8: `ShouldAppend_RequiresAct4RunAndAllKeys`). Daily runs never carry `SpoonAct4` (Task 10: `CustomRunOption_IsNotOfferedInDaily`).
4. **Monster HP/Damage modifiers (25-500 %).** They apply once to all three monsters and to Beat of Death, and never to the Invincible budget (Task 1: `BeatOfDeathIsScaledOnceAsAnEnemyEffect`; Task 5: `InvincibleBudgetIgnoresMonsterHpPercent`).
5. **Invincible across multi-hit attacks, poison, Thorns-like damage and several players.** All of these share one budget that refills at the enemy turn start (Task 1: `HeartInvincibleRuleTests`, already written, plus `PoisonTicksDrainTheSameBudget`).

## File Map

| File | Task | Responsibility |
|------|------|----------------|
| `src/Core/EndingMoveRule.cs` (adopt) | 1 | Move order for Heart, Shield, Spear |
| `src/Core/HeartInvincibleRule.cs` (adopt) | 1 | Invincible cap and budget maths |
| `src/Act4/Sts1InvinciblePower.cs` (adopt) | 1 | Heart's per-turn HP-loss budget |
| `src/Act4/Sts1BeatOfDeathPower.cs` (adopt) | 1 | Damage to the card's player after each card |
| `src/Act4/EndingBranchState.cs` (adopt) | 1 | `MonsterState` that defers to a closure |
| `src/Act4/Act4MultiplayerScalingPatch.cs` (adopt) | 1 | Act index 3 uses act 3's multiplayer factors |
| `src/Core/EndingNumbers.cs` | 2 | Every HP/damage/block/amount by ascension |
| `src/Core/HeartBuffRule.cs` | 2 | The Heart's escalating buff |
| `src/Core/EndingMoveBrain.cs` | 2 | Turn counter + previous move + rule, advanced once per turn |
| `tools/sts1/ending_art.py` | 3 | Spine 3.4 setup pose → static PNG |
| `godot/SlaySpoon/images/ending/*.png` | 3 | Heart, Shield, Spear sprites; Heart map node + outline |
| `src/Act4/EndingVisuals.cs` | 4 | Code-built `NCreatureVisuals` around a static sprite |
| `src/Act4/Sts1SpireShield.cs`, `Sts1SpireSpear.cs` | 4 | Elite monsters |
| `src/Act4/Sts1ShieldAndSpearElite.cs` | 4 | Elite encounter + code-built slot scene |
| `src/Act4/Sts1CorruptHeart.cs`, `Sts1CorruptHeartBoss.cs` | 5 | Boss monster and encounter |
| `src/Act4/Sts1EndingAct.cs`, `EndingActAssetsPatch.cs` | 6 | Act model, rooms, Glory asset redirection |
| `src/Core/EndingMapLayout.cs`, `src/Act4/EndingActMap.cs` | 7 | Fixed rest → shop → elite → boss map |
| `src/Core/EndingRouteRule.cs`, `src/Act4/Act4Routing.cs` | 8 | Append the act when the keys are complete |
| `src/Act4/EndingStingerPatch.cs`, `godot/SlaySpoon/audio/ending_stinger.ogg` | 9 | Optional StS1 stinger at the victory room |
| `godot/SlaySpoon/localization/{eng,fra}/{monsters,encounters,acts}.json` | 4-6 | New text tables (vanilla names) |
| `tests/EndingTextTests.cs` | 4-6 | Every Ending model has EN and FR text |
| `src/CustomRunUi/ModifierListPatch.cs`, `src/Core/Act4UnlockRule.cs`, `mod/SlaySpoon.json`, `src/ModEntry.cs`, `CLAUDE.md`, `README.md` | 10 | Release |

---

### Task 1: Adopt the early WP5 work (rules, Invincible, Beat of Death)

**Files:**
- Modify: `src/Core/EndingMoveRule.cs`, `src/Core/HeartInvincibleRule.cs`, `src/Act4/Sts1InvinciblePower.cs`, `src/Act4/Sts1BeatOfDeathPower.cs`, `src/Act4/EndingBranchState.cs`, `src/Act4/Act4MultiplayerScalingPatch.cs`, `godot/SlaySpoon/localization/{eng,fra}/powers.json`
- Test: `tests/EndingMoveRuleTests.cs`, `tests/HeartInvincibleRuleTests.cs`, `tests/MonsterDamageRuleTests.cs` (add one test; create the file if it does not exist)

**Interfaces:**
- Consumes: `MonsterDamageRule.Scales(bool dealerIsEnemy, bool targetIsEnemy, bool enemyStatus, bool enemyEffect)`, `EnemyEffectDamage.Enter/Exit`.
- Produces: `enum HeartMove { Debilitate, BloodShots, Echo, Buff }`, `enum ShieldMove { Bash, Fortify, Smash }`, `enum SpearMove { BurnStrike, Piercer, Skewer }`; `EndingMoveRule.Heart(int turn, HeartMove previous, int roll) -> HeartMove`, `.Shield(int turn, ShieldMove previous, int roll) -> ShieldMove`, `.Spear(int turn, SpearMove previous, int roll) -> SpearMove` (turn is zero-based; roll is 0 or 1). `HeartInvincibleRule.InitialCap(int ascension, int players) -> int`, `.DamageCap(int remaining, int block, bool unblockable) -> int`, `.AfterHpLoss(int remaining, int hpLost) -> int`. `Sts1InvinciblePower`, `Sts1BeatOfDeathPower` (ids `STS1_INVINCIBLE_POWER`, `STS1_BEAT_OF_DEATH_POWER`). `internal sealed class EndingBranchState(Func<string> choose) : MonsterState`.

Review findings to fix (I did not edit these files):
- `EndingMoveRule.Heart` does not compile (CS0019). The `switch` binds to `(turn - 1)` alone, not to `(turn - 1) % 3`. Parenthesize as `((turn - 1) % 3) switch`. Check that the Shield and Spear `turn % 3 switch` expressions compile as written.
- FR `STS1_BEAT_OF_DEATH_POWER.title` is "Battement mortel". StS1's French name is "Pulsation de la mort" (`data/sts1/act4/localization/fra.json`). Use it.
- `Sts1InvinciblePower` caps damage only in `ModifyDamageCap` (before block, with Block added back), so damage that skips `Hook.ModifyDamage` is not capped. Make the authoritative clamp a `ModifyHpLostAfterOsty` override (after block, the hook vanilla `IntangiblePower` uses for the same job), returning `min(amount, remaining)` for the owner. Keep `ModifyDamageCap` only for intent previews and block loss, with a comment that says so. Lower the budget in `AfterDamageReceived` by `result.UnblockedDamage` (already done), and `Flash()` when the clamp bites.
- `PowerModel.SetAmount(0)` only sets the counter. It does not remove the power (checked in the decompiled source), so a budget of 0 stays on the Heart until its next turn. Keep that behavior.
- `_turnCap` is an unsaved field. StS2 restarts the combat when a save loads, and `AfterApplied` runs again, so this is fine. Add a one-line comment saying so.
- `Sts1BeatOfDeathPower` damages with `ValueProp.Unpowered`, the same as vanilla Galvanic minus `Move`. Keep it, and add a one-line comment: Unpowered means Strength does not apply, as in StS1's THORNS damage.

- [ ] **Step 1: Fix the compile error and run the existing tests to see where they stand**

Run: `nice -n 19 make build` and then `nice -n 19 make test`
Expected: build 0 warnings / 0 errors once `((turn - 1) % 3)` is parenthesized. `EndingMoveRuleTests` (3 tests) and `HeartInvincibleRuleTests` (4 tests) pass. If any fail, fix the rule against `mechanics.json`, not the test.

- [ ] **Step 2: Add the failing tests**

```csharp
// tests/HeartInvincibleRuleTests.cs
[Fact] public void PoisonTicksDrainTheSameBudget()
{
	int left = HeartInvincibleRule.AfterHpLoss(300, 280);
	Assert.Equal(20, HeartInvincibleRule.DamageCap(left, block: 50, unblockable: true));
	Assert.Equal(0, HeartInvincibleRule.AfterHpLoss(left, 20));
}
[Fact] public void SoloCapIgnoresTheMultiplayerFactor() => Assert.Equal(300, HeartInvincibleRule.InitialCap(0, 1));

// tests/MonsterDamageRuleTests.cs
[Fact] public void BeatOfDeathIsScaledOnceAsAnEnemyEffect()
{
	Assert.True(MonsterDamageRule.Scales(dealerIsEnemy: false, targetIsEnemy: false, enemyStatus: false, enemyEffect: true));
	Assert.True(MonsterDamageRule.Scales(dealerIsEnemy: true, targetIsEnemy: false, enemyStatus: false, enemyEffect: false));
}
```

The second assertion pins that a dealer-less enemy effect and an enemy dealer both scale. If `Scales` has a different parameter order, check its signature and keep the meaning.

- [ ] **Step 3: Run the tests**

Run: `nice -n 19 make test`
Expected: PASS. These pin existing behavior, so no new production code is needed. If one fails, the rule is wrong; fix the rule.

- [ ] **Step 4: Apply the review fixes listed above** (`ModifyHpLostAfterOsty` clamp, the FR title, the comments)

- [ ] **Step 5: Build and test**

Run: `nice -n 19 make build && nice -n 19 make test`
Expected: 0 warnings / 0 errors; all tests pass.

- [ ] **Step 6: Checklist** (heading `## Heart powers (WP4-6 Task 1)`). These are tested in Task 5's fight, so list them here:
  - Invincible: the Heart's HP loss stops at 300 per turn solo, 200 at A10, and 780 with two players. Block on the Heart is not counted. Poison counts.
  - Beat of Death: each card you play deals 1 damage to you (2 at A10). Strength does not change it. Monster Damage 200 % doubles it once.

- [ ] **Step 7: Commit**

```bash
git add src/Core/EndingMoveRule.cs src/Core/HeartInvincibleRule.cs src/Act4/Sts1InvinciblePower.cs src/Act4/Sts1BeatOfDeathPower.cs \
  src/Act4/EndingBranchState.cs src/Act4/Act4MultiplayerScalingPatch.cs godot/SlaySpoon/localization/eng/powers.json \
  godot/SlaySpoon/localization/fra/powers.json tests/EndingMoveRuleTests.cs tests/HeartInvincibleRuleTests.cs tests/MonsterDamageRuleTests.cs \
  docs/testing/step-6-checklist.md
git commit -m "feat(act4): Ending move rules, Invincible and Beat of Death powers"
```

---

### Task 2: Ending numbers, Heart buff and the move brain

**Files:**
- Create: `src/Core/EndingNumbers.cs`, `src/Core/HeartBuffRule.cs`, `src/Core/EndingMoveBrain.cs`
- Test: `tests/EndingNumbersTests.cs`, `tests/HeartBuffRuleTests.cs`, `tests/EndingMoveBrainTests.cs`

**Interfaces:**
- Consumes: `EndingMoveRule` (Task 1).
- Produces:
  - `static class EndingNumbers`, every method taking `int ascension` (StS2 level). It thresholds at 8 (Tough), 9 (Deadly) and 10 (Double Boss):
    - `ShieldHp`, `ShieldArtifact`, `ShieldBash`, `ShieldSmash`, `int? ShieldSmashFixedBlock`, `const FortifyBlock = 30`;
    - `SpearHp`, `SpearArtifact`, `SpearBurnStrike`, `const SpearBurnStrikeHits = 2`, `const SpearBurns = 2`, `bool SpearBurnsToDrawPile`, `const PiercerStrength = 2`, `SpearSkewer`, `SpearSkewerHits`;
    - `HeartHp`, `HeartBeatOfDeath`, `HeartBloodShot`, `HeartBloodShotHits`, `HeartEcho`, `const DebilitateAmount = 2`.
  - `enum HeartBuffKind { Artifact, BeatOfDeath, PainfulStabs, Strength }`; `HeartBuffRule.Extra(int buffsDone) -> (HeartBuffKind Kind, int Amount)`; `const HeartBuffRule.BaseStrength = 2`.
  - `enum BashDebuff { Strength, Focus }`; `EndingMoveRule.BashDebuffFor(bool hasOrbSlots, int roll) -> BashDebuff` (added to the Task 1 file).
  - `sealed class EndingMoveBrain<TMove>(Func<int, TMove, int, TMove> rule, TMove initialPrevious) where TMove : struct, Enum`, with `int Turn`, `TMove Current`, `TMove Advance(int roll)`. `Advance` computes `rule(Turn, Current, roll)`, stores it in `Current`, increments `Turn`, and returns it. The first call (Turn 0) sets the opening move.

Before you write the values, read `data/sts1/act4/mechanics.json` and check each one. If a number below disagrees with it, the json wins: note it in the commit body.

- [ ] **Step 1: Write the failing tests**

```csharp
// EndingNumbersTests
[Fact] public void ShieldScalesAtToughDeadlyAndDoubleBoss()
{ Assert.Equal(110, EndingNumbers.ShieldHp(7)); Assert.Equal(125, EndingNumbers.ShieldHp(8));
  Assert.Equal(12, EndingNumbers.ShieldBash(8)); Assert.Equal(14, EndingNumbers.ShieldBash(9));
  Assert.Equal(34, EndingNumbers.ShieldSmash(8)); Assert.Equal(38, EndingNumbers.ShieldSmash(9));
  Assert.Equal(1, EndingNumbers.ShieldArtifact(9)); Assert.Equal(2, EndingNumbers.ShieldArtifact(10));
  Assert.Null(EndingNumbers.ShieldSmashFixedBlock(9)); Assert.Equal(99, EndingNumbers.ShieldSmashFixedBlock(10)); }
[Fact] public void SpearScalesAtToughDeadlyAndDoubleBoss()
{ Assert.Equal(160, EndingNumbers.SpearHp(7)); Assert.Equal(180, EndingNumbers.SpearHp(8));
  Assert.Equal(5, EndingNumbers.SpearBurnStrike(8)); Assert.Equal(6, EndingNumbers.SpearBurnStrike(9));
  Assert.Equal(10, EndingNumbers.SpearSkewer(9)); Assert.Equal(3, EndingNumbers.SpearSkewerHits(8)); Assert.Equal(4, EndingNumbers.SpearSkewerHits(9));
  Assert.False(EndingNumbers.SpearBurnsToDrawPile(9)); Assert.True(EndingNumbers.SpearBurnsToDrawPile(10));
  Assert.Equal(2, EndingNumbers.SpearArtifact(10)); }
[Fact] public void HeartScalesAtToughDeadlyAndDoubleBoss()
{ Assert.Equal(750, EndingNumbers.HeartHp(7)); Assert.Equal(800, EndingNumbers.HeartHp(8));
  Assert.Equal(2, EndingNumbers.HeartBloodShot(9)); Assert.Equal(12, EndingNumbers.HeartBloodShotHits(8)); Assert.Equal(15, EndingNumbers.HeartBloodShotHits(9));
  Assert.Equal(40, EndingNumbers.HeartEcho(8)); Assert.Equal(45, EndingNumbers.HeartEcho(9));
  Assert.Equal(1, EndingNumbers.HeartBeatOfDeath(9)); Assert.Equal(2, EndingNumbers.HeartBeatOfDeath(10)); }

// HeartBuffRuleTests
[Fact] public void BuffsEscalateThenRepeatPlusFifty()
{ Assert.Equal((HeartBuffKind.Artifact, 2), HeartBuffRule.Extra(0));
  Assert.Equal((HeartBuffKind.BeatOfDeath, 1), HeartBuffRule.Extra(1));
  Assert.Equal((HeartBuffKind.PainfulStabs, 1), HeartBuffRule.Extra(2));
  Assert.Equal((HeartBuffKind.Strength, 10), HeartBuffRule.Extra(3));
  Assert.Equal((HeartBuffKind.Strength, 50), HeartBuffRule.Extra(4));
  Assert.Equal((HeartBuffKind.Strength, 50), HeartBuffRule.Extra(9)); }

// EndingMoveRuleTests (add)
[Fact] public void BashTakesFocusOnlyFromOrbUsersOnTheRoll()
{ Assert.Equal(BashDebuff.Strength, EndingMoveRule.BashDebuffFor(false, 1));
  Assert.Equal(BashDebuff.Strength, EndingMoveRule.BashDebuffFor(true, 0));
  Assert.Equal(BashDebuff.Focus, EndingMoveRule.BashDebuffFor(true, 1)); }

// EndingMoveBrainTests
[Fact] public void Brain_OpensWithTheRulesTurnZeroMove()
{ var b = new EndingMoveBrain<HeartMove>(EndingMoveRule.Heart, HeartMove.Debilitate);
  Assert.Equal(HeartMove.Debilitate, b.Advance(1)); Assert.Equal(1, b.Turn); }
[Fact] public void Brain_OnlyAdvanceChangesState()
{ var b = new EndingMoveBrain<SpearMove>(EndingMoveRule.Spear, SpearMove.BurnStrike);
  b.Advance(0); var (turn, current) = (b.Turn, b.Current); _ = b.Current; _ = b.Turn;
  Assert.Equal((turn, current), (b.Turn, b.Current)); }
[Fact] public void Brain_ReplaysIdenticallyFromTheSameRolls()
{ int[] rolls = [0, 1, 1, 0, 1, 0, 0, 1];
  var a = new EndingMoveBrain<ShieldMove>(EndingMoveRule.Shield, ShieldMove.Bash);
  var c = new EndingMoveBrain<ShieldMove>(EndingMoveRule.Shield, ShieldMove.Bash);
  Assert.Equal(rolls.Select(a.Advance), rolls.Select(c.Advance)); }
```

`initialPrevious` is the "previous move" that turn 0 sees. Pass a value that turn 0 ignores: Heart turn 0 is always Debilitate, Spear turn 0 is guarded, and Shield turn 0 is a 50 % roll. Check the Shield turn-0 rule in `mechanics.json`. If turn 0 can be Bash and a Bash that follows a "previous Bash" behaves differently, pass `Fortify` instead and say so in a comment.

- [ ] **Step 2: Run them to see them fail**

Run: `nice -n 19 make test`
Expected: FAIL. The build reports that `EndingNumbers`, `HeartBuffRule`, `EndingMoveBrain` and `BashDebuffFor` do not exist.

- [ ] **Step 3: Implement the three files and `BashDebuffFor`** with the signatures above. Each value is a `ascension >= N ? a : b` expression; add no other logic.

- [ ] **Step 4: Run the tests**

Run: `nice -n 19 make build && nice -n 19 make test`
Expected: 0 warnings / 0 errors; all pass.

- [ ] **Step 5: Commit** (no checklist items; the fights in Tasks 4 and 5 cover these)

```bash
git add src/Core/EndingNumbers.cs src/Core/HeartBuffRule.cs src/Core/EndingMoveBrain.cs src/Core/EndingMoveRule.cs tests/EndingNumbersTests.cs tests/HeartBuffRuleTests.cs tests/EndingMoveBrainTests.cs tests/EndingMoveRuleTests.cs
git commit -m "feat(act4): Ending numbers by ascension, Heart buff rule, move brain"
```

---

### Task 3: Static Ending sprites from the StS1 setup pose

**Files:**
- Create: `tools/sts1/ending_art.py`, `tools/sts1/tests/test_ending_art.py`, `tools/sts1/tests/fixtures/ending_art/` (a hand-made two-bone skeleton: one region attachment, one weighted mesh, a 3.4-format atlas, a 16×16 PNG)
- Create (generated, committed): `godot/SlaySpoon/images/ending/{heart,shield,spear}.png`, `godot/SlaySpoon/images/ending/heart_boss.png`, `godot/SlaySpoon/images/ending/heart_boss_outline.png`
- Modify: `Makefile` (target `sts1-ending-art`, added to `.PHONY`)

**Interfaces:**
- Consumes: `data/sts1/act4/assets/images/npcs/heart/skeleton.{json,atlas,png}`, `.../monsters/theEnding/shield/skeleton.json` with `shield.png` and the atlas, the same for spear, and `.../ui/map/boss/heart.png` and `.../ui/map/bossOutline/heart.png`.
- Produces:
  - `render(skeleton: Path, atlas: Path, out: Path, scale: float = 1.0) -> tuple[int, int, int, int]`: writes a trimmed RGBA PNG and returns the setup-pose origin's pixel position and the image size (x, y, w, h);
  - CLI `--assets <dir> --out <dir>`: renders the three monsters, copies the two map icons to `heart_boss.png` and `heart_boss_outline.png`, and prints one JSON line per monster `{name, origin_x, origin_y, width, height}`. Task 4 copies those numbers into `EndingVisuals`.

Spine 3.4 setup pose, with the math decided (Pillow only; no numpy in the image):
- Bones: compose the local `x, y, rotation, scaleX, scaleY` (and `shearX`/`shearY` if present) with the parent world transform. Honor `inheritRotation: false` and `inheritScale: false` as Spine 3.4 does. Y is up; flip once when writing pixels.
- Draw order: the `slots` array order. Each slot uses its setup `attachment` from skin `default`. Skip `path` and `boundingbox` attachments.
- `region`: the four corners from the attachment's own `x, y, rotation, scaleX, scaleY, width, height` in bone space.
- `mesh`: if `len(vertices) == len(uvs)`, the mesh is unweighted (bone space of the slot's bone). Otherwise it is weighted: for each vertex, `boneCount` followed by `(boneIndex, x, y, weight)` per bone, summed in world space. UVs are relative to the atlas region; handle `rotate: true` regions.
- Raster: for each triangle (mesh) or each two triangles (region), use `Image.transform(..., Image.AFFINE, ...)` from atlas space to output space, masked by the triangle polygon, and alpha-composite it. `blend: additive` slots are added with `ImageChops.add` on premultiplied RGB. Slot `color` alpha multiplies.
- Trim to the alpha bounding box plus a 4 px margin.

- [ ] **Step 1: Write the failing tests**

```python
def test_child_bone_inherits_parent_rotation(): world = bone_world(skel_with_parent_rot_90); assert approx(world["child"].x, 0) and approx(world["child"].y, 10)
def test_child_bone_ignores_parent_rotation_when_inherit_rotation_false(): ...; assert approx(world["child"].rotation, 0)
def test_weighted_mesh_vertex_is_weight_sum(): assert approx(mesh_world_vertex(two_bones, [2, 0, 1, 0, .5, 1, 1, 0, .5]), (expected_x, expected_y))
def test_rotated_atlas_region_uvs(): assert region_uv(atlas_rotated, (0, 0)) == (u_right, v_top)
def test_render_is_deterministic(tmp_path): render(...a); render(...b); assert a.read_bytes() == b.read_bytes()
def test_render_trims_to_alpha(tmp_path): x, y, w, h = render(fixture...); img = Image.open(out); assert img.size == (w, h) and img.getbbox() == (4, 4, w - 4, h - 4)
def test_path_attachments_draw_nothing(tmp_path): with pytest.raises(ValueError, match="nothing to draw"): render(only_path_fixture...)
```

Write the exact expected numbers from the fixture you build: a parent rotated 90° with a child at local (10, 0) lands at world (0, 10).

- [ ] **Step 2: Run them to see them fail**

Run: `podman run --rm -v "$PWD":/repo:Z localhost/slayspoon-tools pytest -q tools/sts1/tests/test_ending_art.py` (or `nice -n 19 make test`)
Expected: FAIL, because `ending_art` does not exist yet.

- [ ] **Step 3: Implement `tools/sts1/ending_art.py`** with the math above. Then add the `Makefile` target:

```make
sts1-ending-art:
	$(TOOLS) python tools/sts1/ending_art.py --assets data/sts1/act4/assets --out godot/SlaySpoon/images/ending
```

- [ ] **Step 4: Run the tests, then render**

Run: `nice -n 19 make test`, then `nice -n 19 make sts1-ending-art`
Expected: all pytest pass. Five PNGs and three JSON lines. Open the three monster PNGs (Read tool) and compare them with StS1's look. Shield and Spear should face left (they stand on the right in StS1); the Heart is centered. If a sprite is clearly broken (parts scattered), the bone or mesh math is wrong: fix it before you commit. If the setup pose itself looks odd (for example the Heart's tendrils folded), that is the builder call below. Note it in the commit body for the owner.

- [ ] **Step 5: Build** (the PCK must import the new PNGs)

Run: `nice -n 19 make build`
Expected: 0 warnings / 0 errors, and `godot/SlaySpoon/images/ending/*.png.import` files created by the Godot export. Commit them if the existing `images/keys` imports are committed; follow whatever is already done there.

- [ ] **Step 6: Commit** (checklist items come with Tasks 4 and 5)

```bash
git add tools/sts1/ending_art.py tools/sts1/tests/test_ending_art.py tools/sts1/tests/fixtures/ending_art Makefile godot/SlaySpoon/images/ending
git commit -m "feat(act4): render static Ending sprites from the StS1 setup pose"
```

---

### Task 4: Spire Shield and Spire Spear (elite)

**Files:**
- Create: `src/Act4/EndingVisuals.cs`, `src/Act4/Sts1SpireShield.cs`, `src/Act4/Sts1SpireSpear.cs`, `src/Act4/Sts1ShieldAndSpearElite.cs`, `src/Act4/EndingEncounterScenePatch.cs`, `src/Act4/EndingDrawPile.cs`
- Create: `godot/SlaySpoon/localization/{eng,fra}/monsters.json`, `.../encounters.json`
- Test: `tests/EndingTextTests.cs`

**Interfaces:**
- Consumes: `EndingMoveBrain<TMove>`, `EndingMoveRule.Shield/Spear/BashDebuffFor`, `EndingNumbers.*` (Task 2), `EndingBranchState` (Task 1), the PNGs and JSON lines (Task 3).
- Produces:
  - `static class EndingVisuals`: `NCreatureVisuals Create(string texturePath, Vector2 origin, Vector2 size)`, and a Harmony prefix on `MonsterModel.CreateVisuals` that, for any `IEndingMonster`, returns the code-built visuals and skips vanilla.
  - `interface IEndingMonster { string TexturePath { get; } Vector2 Origin { get; } Vector2 Size { get; } }`.
  - `static class EndingDrawPile`: `Task ShuffleIn(Player player, IEnumerable<CardModel> cards, Rng rng)`.
  - `Sts1SpireShield`, `Sts1SpireSpear` (ids `STS1_SPIRE_SHIELD`, `STS1_SPIRE_SPEAR`), `Sts1ShieldAndSpearElite` (`RoomType.Elite`, `Slots { "shield", "spear" }`, `HasScene => true`, `FullyCenterPlayers => true`).
  - Text keys: `STS1_SPIRE_SHIELD.name`, `STS1_SPIRE_SHIELD.moves.{BASH,FORTIFY,SMASH}.title`, `STS1_SPIRE_SPEAR.name`, `STS1_SPIRE_SPEAR.moves.{BURN_STRIKE,PIERCER,SKEWER}.title`, and the `STS1_SHIELD_AND_SPEAR_ELITE.*` keys a vanilla encounter has. Grep `SlayTheSpire2.pck` for `"KAISER_CRAB_BOSS.` and `"CRUSHER.` to get the exact key set.

Decisions:
- **Visuals.** `MonsterModel.CreateVisuals` is not virtual and loads `VisualsPath` from the PCK. Override `VisualsPath` to return the vanilla fallback creature scene path (`SceneHelper.GetScenePath("creature_visuals/fallback")`), so asset preloading finds a real scene. The prefix builds the node tree in code instead: an `NCreatureVisuals` root with a `Sprite2D` named `Visuals`, a `Control` named `Bounds` sized to the sprite, and `Marker2D`s named `IntentPos` (above the sprite), `CenterPos` and `TalkPos`. Each child has `UniqueNameInOwner = true` and `Owner = root`, because `NCreatureVisuals._Ready` resolves `%Visuals`, `%Bounds`, `%IntentPos` and `%CenterPos`. The body is not a SpineSprite, so `HasSpineAnimation` is false. Verify in `MonsterModel`/`NCreature` that a creature without Spine skips the animator (the fallback scene path does the same) and that hurt/death fall back to a tween or nothing. If anything requires a Spine body, stop and report it.
- **Encounter scene.** `EncounterModel`'s scene path is private and derived from the id. Patch the two members that use it: the method that instantiates the scene (`Control`) and the asset-path enumeration that adds it when `HasScene`. For `Sts1ShieldAndSpearElite`, return a code-built `Control` laid out like vanilla `kaiser_crab_boss.tscn`: one slot node per name, Shield to the left of the players and Spear to the right. Read that scene's node structure from the game's PCK with Godot headless (`--headless --script` that loads and prints the tree), and do not commit the vanilla scene or its dump. Remove the asset-path entry rather than leaving a missing path.
- **Own RNG.** Each monster builds `new Rng(runState.Rng.Seed, "SlaySpoon.Ending." + Id.Entry)` once, before its first move is chosen, and rolls only through it (`NextInt(2)`). Never use `MonsterModel.Rng` (Shield and Spear get the same seed from the map column), `RunRng.MonsterAi` or `Rng.Shuffle`.
- **Move machine.** Return a `MonsterMoveStateMachine` with one `MoveState` per move, each followed by one shared `EndingBranchState` whose closure calls `brain.Advance(rng.NextInt(2))` and returns that move's state id. The initial state is the move from the first `Advance`. Log each transition once: `SpoonLog.Info($"ending: {Id.Entry} turn {brain.Turn} -> {move}")`. The owner compares host and client logs in co-op.
- **Shield.** On `AfterAddedToRoom`: `ArtifactPower` = `ShieldArtifact`, `BackAttackLeftPower` 1 on itself.
  - Bash: attack `ShieldBash`, then one debuff per player hit: Strength −1 (`StrengthPower` −1), or Focus −1 when `BashDebuffFor(hasOrbSlots, rng.NextInt(2))` says so. `hasOrbSlots` = the player has at least one orb slot. Intents: single attack + debuff.
  - Fortify: `FortifyBlock` Block to every living enemy (`ValueProp.Move`). Intent: defend.
  - Smash: attack `ShieldSmash`, then gain Block equal to `ShieldSmashFixedBlock` if set, else the highest `TotalDamage` among the attack's `Results`. Intents: single attack + defend.
- **Spear.** On `AfterAddedToRoom`: `ArtifactPower` = `SpearArtifact`, `BackAttackRightPower` 1 on itself, and `SurroundedPower` 1 on every opponent. This mirrors vanilla Rocket, the right-hand Kaiser Crab half.
  - Burn Strike: attack `SpearBurnStrike` × 2, then 2 `Burn` per player hit: to the discard pile, or shuffled into the draw pile with `EndingDrawPile.ShuffleIn` when `SpearBurnsToDrawPile`. Intents: multi-attack + the vanilla status-card intent (find its class next to `DebuffIntent`).
  - Piercer: +2 `StrengthPower` to every living enemy. Intent: buff.
  - Skewer: attack `SpearSkewer` × `SpearSkewerHits`. Intent: multi-attack.
- **Surrounded and Back Attack when one dies.** Vanilla `SurroundedPower.AfterDeath` handles the facing. Check whether it also removes the survivor's Back Attack power, as the Kaiser Crab needs. If it does not, remove the survivor's `BackAttackLeftPower`/`BackAttackRightPower` in the dying monster's death hook.
- **`EndingDrawPile.ShuffleIn`.** `CardPilePosition.Random` uses the vanilla `Rng.Shuffle`, so do not use it. Add each card with `CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Draw, creator: null, CardPilePosition.Top)`. The null creator keeps them "enemy status" for `MonsterDamageRule`. Then move each card to index `rng.NextInt(count + 1)` of the draw pile through `CardPile`'s remove and `AddInternal(card, index, silent: true)`.
- **Attacks** use `DamageCmd.Attack(..).FromMonster(this)`, which hits every player and goes through Monster Damage once. HP comes from `EndingNumbers`; `EnemyHpPatch` and `Act4MultiplayerScalingPatch` do the rest.
- **Rewards.** Vanilla elite rewards (gold, potion roll, card, relic), as in StS1's elite room.
- **Text.** EN/FR names from `data/sts1/act4/localization/{eng,fra}.json`: Spire Shield / Bouclier de la Tour, Spire Spear / Lance de la Tour. StS1 has no move names, so the builder chose these:

  | Key | EN | FR |
  |---|---|---|
  | BASH | Bash | Heurt |
  | FORTIFY | Fortify | Fortifier |
  | SMASH | Smash | Fracas |
  | BURN_STRIKE | Burn Strike | Frappe ardente |
  | PIERCER | Piercer | Perceur |
  | SKEWER | Skewer | Embrocher |

  The encounter title is "Spire Shield and Spear" / "Bouclier et Lance de la Tour".

- [ ] **Step 1: Write the failing test**

```csharp
// tests/EndingTextTests.cs: reads both languages' monsters.json and encounters.json (pattern: LocalizationTablesTests.FindRepo)
[Theory]
[InlineData("monsters", "STS1_SPIRE_SHIELD.name")] [InlineData("monsters", "STS1_SPIRE_SHIELD.moves.BASH.title")]
[InlineData("monsters", "STS1_SPIRE_SHIELD.moves.FORTIFY.title")] [InlineData("monsters", "STS1_SPIRE_SHIELD.moves.SMASH.title")]
[InlineData("monsters", "STS1_SPIRE_SPEAR.name")] [InlineData("monsters", "STS1_SPIRE_SPEAR.moves.BURN_STRIKE.title")]
[InlineData("monsters", "STS1_SPIRE_SPEAR.moves.PIERCER.title")] [InlineData("monsters", "STS1_SPIRE_SPEAR.moves.SKEWER.title")]
[InlineData("encounters", "STS1_SHIELD_AND_SPEAR_ELITE.title")]
public void EndingTextExistsInBothLanguages(string table, string key)
{ foreach (var lang in new[] { "eng", "fra" }) Assert.False(string.IsNullOrWhiteSpace(Read(lang, table)[key])); }
```

Add one `InlineData` for every other key the vanilla encounter has (from the PCK grep).

- [ ] **Step 2: Run it to see it fail**

Run: `nice -n 19 make test`
Expected: FAIL (the file or key is missing).

- [ ] **Step 3: Implement the files above**, then add the text.

- [ ] **Step 4: Build and test**

Run: `nice -n 19 make build && nice -n 19 make test`
Expected: 0 warnings / 0 errors; all pass, `LocalizationTablesTests` included.

- [ ] **Step 5: Checklist** (heading `## Spire Shield and Spear (WP4-6 Task 4)`; reachable through the vanilla console `fight STS1_SHIELD_AND_SPEAR_ELITE` if it accepts mod encounters, else through the act 4 map after Task 8):
  - Shield stands behind the players on the left, Spear in front on the right; players show Surrounded; Shield shows Back Attack.
  - Shield's opening is Fortify or Bash; turn 3 is always Smash. Smash's Block equals the damage it dealt (99 at A10).
  - Bash removes 1 Strength (Defect sometimes loses 1 Focus instead).
  - Spear opens with Burn Strike, never twice in a row; turn 2 is Skewer; Burns go to the discard pile (shuffled into the draw pile at A10).
  - Killing either one removes Surrounded and the survivor's Back Attack; the players face the survivor.
  - In co-op, attacks hit every player, and the host and client logs show the same `ending:` lines.
  - Monster HP 200 % doubles both monsters' HP once; Monster Damage 50 % halves Bash.
  - Victory gives elite rewards (gold, card, relic, sometimes a potion).

- [ ] **Step 6: Commit**

```bash
git add src/Act4/EndingVisuals.cs src/Act4/Sts1SpireShield.cs src/Act4/Sts1SpireSpear.cs src/Act4/Sts1ShieldAndSpearElite.cs \
  src/Act4/EndingEncounterScenePatch.cs src/Act4/EndingDrawPile.cs godot/SlaySpoon/localization tests/EndingTextTests.cs docs/testing/step-6-checklist.md
git commit -m "feat(act4): Spire Shield and Spire Spear elite"
```

---

### Task 5: Corrupt Heart (boss)

**Files:**
- Create: `src/Act4/Sts1CorruptHeart.cs`, `src/Act4/Sts1CorruptHeartBoss.cs`
- Modify: `godot/SlaySpoon/localization/{eng,fra}/{monsters,encounters}.json`, `tests/EndingTextTests.cs`, `tests/HeartInvincibleRuleTests.cs`

**Interfaces:**
- Consumes: `EndingMoveBrain<HeartMove>`, `EndingMoveRule.Heart`, `HeartBuffRule`, `EndingNumbers` (Task 2); `Sts1InvinciblePower`, `Sts1BeatOfDeathPower`, `HeartInvincibleRule.InitialCap` (Task 1); `EndingVisuals`, `IEndingMonster`, `EndingDrawPile` (Task 4); `heart.png`, `heart_boss.png`, `heart_boss_outline.png` (Task 3).
- Produces:
  - `Sts1CorruptHeart` (id `STS1_CORRUPT_HEART`, moves `DEBILITATE`, `BLOOD_SHOTS`, `ECHO`, `BUFF`);
  - `Sts1CorruptHeartBoss` (`RoomType.Boss`, one monster, no scene, `BossNodePath => "res://SlaySpoon/images/ending/heart_boss"`, `BossNodeSpineResource => null`, no custom background or BGM). `NBossMapPoint` then loads `heart_boss.png` and `heart_boss_outline.png`; check that it appends `.png` and `_outline.png` exactly as KaiserCrabBoss's placeholder does.

Decisions:
- **On `AfterAddedToRoom`:** `Sts1InvinciblePower` with `HeartInvincibleRule.InitialCap(ascension, players)` and `Sts1BeatOfDeathPower` with `HeartBeatOfDeath(ascension)`. `ascension` is the run's ascension level; `players` is the run's player count. The budget ignores the Monster HP percent (builder call).
- **Debilitate:** for every player, Vulnerable, Weak and Frail at `DebilitateAmount`. Then `EndingDrawPile.ShuffleIn` one each of `Dazed`, `Slimed`, `Wound`, `Burn` and `Void` into that player's draw pile. Intents: debuff + status.
- **Blood Shots:** `HeartBloodShot` × `HeartBloodShotHits`. **Echo:** `HeartEcho` single attack.
- **Buff:**
  - if the Heart's Strength is negative, raise it to 0;
  - add `HeartBuffRule.BaseStrength`;
  - apply `HeartBuffRule.Extra(buffsDone)`: Artifact → `ArtifactPower`, BeatOfDeath → +1 `Sts1BeatOfDeathPower`, PainfulStabs → `PainfulStabsPower` (check that vanilla's amount means one Wound per unblocked hit, as in StS1; if it means something else, report it), Strength → `StrengthPower`;
  - `buffsDone++`, an unsaved field (combat restarts on reload). Intent: buff.
- **Text:** "Corrupt Heart" / "Cœur corrompu". The builder chose these move names:

  | Key | EN | FR |
  |---|---|---|
  | DEBILITATE | Debilitate | Affaiblir |
  | BLOOD_SHOTS | Blood Shots | Tirs sanglants |
  | ECHO | Echo | Écho |
  | BUFF | Buff | Renforcement |

  The encounter title is "Corrupt Heart" / "Cœur corrompu".

- [ ] **Step 1: Write the failing tests**

```csharp
// EndingTextTests: add InlineData for STS1_CORRUPT_HEART.name, .moves.{DEBILITATE,BLOOD_SHOTS,ECHO,BUFF}.title, STS1_CORRUPT_HEART_BOSS.title (+ the vanilla encounter key set)
// HeartInvincibleRuleTests
[Fact] public void InvincibleBudgetIgnoresMonsterHpPercent()
{ // the cap depends only on ascension and players: the signature carries no HP percent, and these hold at any modifier
  Assert.Equal(780, HeartInvincibleRule.InitialCap(9, 2)); Assert.Equal(520, HeartInvincibleRule.InitialCap(10, 2)); }
```

- [ ] **Step 2: Run them to see them fail**

Run: `nice -n 19 make test`
Expected: FAIL on the missing text keys.

- [ ] **Step 3: Implement the two classes and the text.**

- [ ] **Step 4: Build and test**

Run: `nice -n 19 make build && nice -n 19 make test`
Expected: 0 warnings / 0 errors; all pass.

- [ ] **Step 5: Checklist** (`## Corrupt Heart (WP4-6 Task 5)`):
  - Turn 1 is always Debilitate: Vulnerable 2, Weak 2, Frail 2, and five statuses shuffled into the draw pile.
  - The next turns cycle: Blood Shots or Echo, then the other one (never two Echoes in a row), then Buff.
  - Buffs in order: Artifact 2, Beat of Death +1, Painful Stabs, Strength +10, then Strength +50 each time, plus Strength +2 each time.
  - HP 750 (800 at A8 or higher); Blood Shots 2×12 (×15 at A9); Echo 40 (45 at A9).
  - The map shows the Heart icon on the boss node, with its outline when hovered.
  - Two players: HP 750 × 2 × 1.3 = 1950; the Invincible budget is 780 and both players' hits share it.

- [ ] **Step 6: Commit**

```bash
git add src/Act4/Sts1CorruptHeart.cs src/Act4/Sts1CorruptHeartBoss.cs godot/SlaySpoon/localization tests/EndingTextTests.cs tests/HeartInvincibleRuleTests.cs docs/testing/step-6-checklist.md
git commit -m "feat(act4): Corrupt Heart boss"
```

---

### Task 6: The Ending act model

**Files:**
- Create: `src/Act4/Sts1EndingAct.cs`, `src/Act4/EndingActAssetsPatch.cs`, `src/Act4/EndingRoomsPatch.cs`
- Create: `godot/SlaySpoon/localization/{eng,fra}/acts.json`
- Modify: `tests/EndingTextTests.cs`, `src/ModEntry.cs` (self-check line)

**Interfaces:**
- Consumes: `Sts1ShieldAndSpearElite` (Task 4), `Sts1CorruptHeartBoss` (Task 5).
- Produces:
  - `sealed class Sts1EndingAct : ActModel`, id `STS1_ENDING_ACT`;
  - `static ActModel Sts1EndingAct.CreateForRun(IRunState run)`: `ModelDb.Act<Sts1EndingAct>().ToMutable()`, with its rooms filled from `new Rng(run.Rng.Seed, "SlaySpoon.EndingRooms")`;
  - text keys `STS1_ENDING_ACT.title` (plus any other `acts` key a vanilla act has; grep the PCK for `"GLORY.`).

Decisions:
- **Abstract members:**
  - `Index => -1`, so the act is kept out of `ActsByIndex`. `ModelDb.Acts` is a hardcoded list, so the act never enters random act choice. `IsDefault => false`.
  - `BaseNumberOfRooms => 2` and `NumberOfWeakEncounters => 0`.
  - `AllEvents`, `AllAncients`, `GetUnlockedAncients`, `BossDiscoveryOrder`: empty. `GenerateAllEncounters`: the two encounters. `IsUnlocked`: true.
  - Music, ambience, colors and chest: return `ModelDb.Act<Glory>()`'s values (read through the canonical model; do not copy literals). `GetMapPointTypes`, `ApplyActDiscoveryOrderModifications`: the minimal no-op Glory allows. Check their contracts.
- **Assets.** The rest site background, the three map backgrounds, the combat background scene, `GenerateBackgroundAssets` and `GetFullLayerPath` all derive from the protected, non-virtual `FilePathIdentifier`. One Harmony patch class redirects them for `Sts1EndingAct`: patch `FilePathIdentifier`'s getter to return Glory's identifier when `__instance is Sts1EndingAct`. If that tiny getter is inlined (test in game: the act 4 rest site shows Glory's background), patch each public consumer instead.
- **Rooms.** `ActModel.GenerateRooms` is not virtual and assumes non-empty weak/normal/ancient lists. A prefix returns false for `Sts1EndingAct` and sets the act's room set directly: no events, no normal encounters, the Shield and Spear elite as the only elite, the Heart as boss, no ancient, no second boss. Read the private room-set member names in the decompiled `ActModel` and set them through `AccessTools`.
- **Self-check.** In `ModEntry`, log whether `ModelDb.GetById<ActModel>` finds `STS1_ENDING_ACT`, which `ActModel.FromSave` needs on reload.
- **Text.** "The Ending" / "La fin" (StS1's own act names).

- [ ] **Step 1: Write the failing test** (`EndingTextTests`: `[InlineData("acts", "STS1_ENDING_ACT.title")]` plus the vanilla act key set)
- [ ] **Step 2: Run it to see it fail.** `nice -n 19 make test`: FAIL.
- [ ] **Step 3: Implement the three files, the text and the self-check line.**
- [ ] **Step 4: Build and test.** `nice -n 19 make build && nice -n 19 make test`: 0 warnings / 0 errors; all pass.
- [ ] **Step 5: Checklist** (`## The Ending act (WP4-6 Task 6)`):
  - The log at startup shows the `STS1_ENDING_ACT` self-check as found.
  - In act 4, the act banner reads "The Ending" / "La fin"; the music, map, rest site and combat backgrounds are Glory's.
- [ ] **Step 6: Commit**

```bash
git add src/Act4/Sts1EndingAct.cs src/Act4/EndingActAssetsPatch.cs src/Act4/EndingRoomsPatch.cs src/ModEntry.cs godot/SlaySpoon/localization tests/EndingTextTests.cs docs/testing/step-6-checklist.md
git commit -m "feat(act4): The Ending act model on Glory's assets"
```

---

### Task 7: The Ending map

**Files:**
- Create: `src/Core/EndingMapLayout.cs`, `src/Act4/EndingActMap.cs`
- Modify: `src/Config/Act4Modifier.cs` (`SpoonAct4.ModifyGeneratedMapLate`)
- Test: `tests/EndingMapLayoutTests.cs`

**Interfaces:**
- Consumes: `Sts1EndingAct` (Task 6).
- Produces:
  - `enum EndingNode { RestSite, Shop, Elite, Boss }`;
  - `EndingMapLayout.Column = 3`, `EndingMapLayout.Nodes -> IReadOnlyList<(int Row, EndingNode Node)>` = `[(0, RestSite), (1, Shop), (2, Elite), (3, Boss)]`, `EndingMapLayout.Rows = 4`;
  - `sealed class EndingActMap(IRunState run) : ActMap`, on the `GoldenPathActMap` pattern: `StartingMapPoint` at (3, 0), typed RestSite; the Shop and Elite chain with `AddChildPoint`; `BossMapPoint` at (3, 3); `startMapPoints` holds (3, 0). The grid is 7 columns wide.

Decisions:
- `SpoonAct4.ModifyGeneratedMapLate(IRunState run, ActMap map, int actIndex)` returns `new EndingActMap(run)` when `run.Acts[actIndex] is Sts1EndingAct`; otherwise it returns `map`. This hook also runs for saved maps, so a reload rebuilds the same map, and it runs after Golden Compass's map change, so the Ending map always wins.
- Check that `NMapScreen` makes a non-Ancient `StartingMapPoint` travelable when nothing is visited. If it does not, put the rest site at row 1 and an empty start point at row 0, and adjust the layout and its test together.
- Emerald never marks act index 3 (`EmeraldRule.LastAct = 2`); no change needed. The Ruby option is hidden once the Ruby key is held (`RubyRestSitePatch`), so the act 4 rest site shows only vanilla options.

- [ ] **Step 1: Write the failing test**

```csharp
[Fact] public void EndingMapIsRestShopEliteBossInOneColumn()
{ Assert.Equal([(0, EndingNode.RestSite), (1, EndingNode.Shop), (2, EndingNode.Elite), (3, EndingNode.Boss)], EndingMapLayout.Nodes);
  Assert.Equal(3, EndingMapLayout.Column); Assert.Equal(4, EndingMapLayout.Rows); }
```

- [ ] **Step 2: Run it to see it fail.** `nice -n 19 make test`: FAIL.
- [ ] **Step 3: Implement the layout, the map and the override.** The map reads its rows and types from `EndingMapLayout`.
- [ ] **Step 4: Build and test.** 0 warnings / 0 errors; all pass.
- [ ] **Step 5: Checklist** (`## The Ending map (WP4-6 Task 7)`):
  - The act 4 map is one straight path: rest site → merchant → elite → Heart.
  - The rest site has no Ruby option.
  - Buying Golden Compass at the act 4 merchant does not change the map.
  - Save and quit at the merchant, then resume: the same map, with the rest site visited.
- [ ] **Step 6: Commit**

```bash
git add src/Core/EndingMapLayout.cs src/Act4/EndingActMap.cs src/Config/Act4Modifier.cs tests/EndingMapLayoutTests.cs docs/testing/step-6-checklist.md
git commit -m "feat(act4): fixed rest, merchant, elite, Heart map"
```

---

### Task 8: Route the run into The Ending

**Files:**
- Create: `src/Core/EndingRouteRule.cs`, `src/Act4/Act4Routing.cs`
- Modify: `src/ModEntry.cs` (register the `Act4Keys.Changed` handler once)
- Test: `tests/EndingRouteRuleTests.cs`

**Interfaces:**
- Consumes: `SpoonAct4.Of(IRunState)`, `Act4Keys.HasAll`, `Act4Keys.Changed` (static event), `Sts1EndingAct.CreateForRun` (Task 6); the private `RunManager.State` getter pattern from `Act4UnlockPatch`.
- Produces: `EndingRouteRule.ShouldAppend(bool act4Run, bool hasAllKeys, int actCount) -> bool` (true only for `act4Run && hasAllKeys && actCount == 3`); `Act4Routing.EnsureEndingAct(RunState state) -> bool` (idempotent; true when it appended).

Decisions:
- **Append when the keys complete, not when leaving act 3.** The act 3 boss then stops being the final boss, so vanilla gives its rewards (gold, potion roll, card), as StS1 does before act 4. It also does not record the win time, and its reward screen leads to `EnterNextAct` → act 4. The Heart becomes the final boss: no rewards, win time recorded, then `EnterNextAct` opens the `TheArchitect` victory room and `WinRun`. The victory contract stays vanilla.
- **DoubleBoss (A10)** stays on act 3: vanilla sets the second boss at run start, when act 3 is the last act. Appending later never gives the Heart a second boss.
- **Callers:**
  1. An `Act4Keys.Changed` handler registered once in `ModEntry`. It reads the current run through `RunManager.State` and calls `EnsureEndingAct`. Every peer takes keys through synced actions, so every peer appends at the same point.
  2. A postfix on `RunManager.InitializeSavedRun`: a no-op when the save already has four acts, and it repairs a save taken between the key and the append.
  3. A prefix on `RunManager.EnterNextAct` as the last guard.
  Replace `RunState.Acts` with a new list (the setter is private: `AccessTools.PropertySetter`), the same way the vanilla debug act switch does.
- Log `SpoonLog.Info("act4: The Ending appended")` once per run.
- A run without `SpoonAct4` (Daily, or Standard before the unlock) never reaches this code's effect: `ShouldAppend` is false.

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact] public void ShouldAppend_RequiresAct4RunAndAllKeys()
{ Assert.True(EndingRouteRule.ShouldAppend(true, true, 3));
  Assert.False(EndingRouteRule.ShouldAppend(true, false, 3));
  Assert.False(EndingRouteRule.ShouldAppend(false, true, 3)); }
[Fact] public void ShouldAppend_NeverAddsASecondEndingAct() => Assert.False(EndingRouteRule.ShouldAppend(true, true, 4));
[Fact] public void ShouldAppend_IgnoresRunsWithOtherActCounts() => Assert.False(EndingRouteRule.ShouldAppend(true, true, 2));
```

- [ ] **Step 2: Run them to see them fail.** `nice -n 19 make test`: FAIL.
- [ ] **Step 3: Implement the rule and `Act4Routing` with the three callers.**
- [ ] **Step 4: Build and test.** 0 warnings / 0 errors; all pass.
- [ ] **Step 5: Checklist** (`## Route into The Ending (WP4-6 Task 8)`; needs `Act4Preview = true` in `SlaySpoon.cfg` until Task 10):
  - Without all three keys, the act 3 boss ends the run as in vanilla (victory room, credits).
  - With all three keys, the act 3 boss gives rewards, then act 4 starts.
  - A10: both act 3 bosses, then act 4 with a single Heart.
  - Collect the last key, save and quit before the act 3 boss, then resume: act 4 still follows. The log shows "The Ending appended" once.
  - Co-op: the client collects the last key; both players go to act 4 together. Resuming the co-op run keeps act 4.
  - Daily run: no keys, no act 4.
- [ ] **Step 6: Commit**

```bash
git add src/Core/EndingRouteRule.cs src/Act4/Act4Routing.cs src/ModEntry.cs tests/EndingRouteRuleTests.cs docs/testing/step-6-checklist.md
git commit -m "feat(act4): continue into The Ending once the team holds all keys"
```

---

### Task 9: Victory after the Heart and the StS1 stinger

**Files:**
- Create: `src/Act4/EndingStingerPatch.cs`, `godot/SlaySpoon/audio/ending_stinger.ogg` (copy of `data/sts1/act4/assets/audio/music/STS_EndingStinger_v1.ogg`)

**Interfaces:**
- Consumes: `Sts1EndingAct` (Task 6), routing (Task 8).
- Produces: nothing other tasks use.

Decisions:
- No code change to the victory path: after the Heart, vanilla `EnterNextAct` enters the `TheArchitect` event room and `WinRun` ends the run. Check the decompiled `TheArchitect` once to confirm it does not assume three acts (dialogue lookup by act index, for example). If it does, patch only that lookup, mapping act index 3 to 2.
- Stinger (nice-to-have; drop this part if it fights the game's FMOD music): a postfix on `RunManager.EnterNextAct` (or on the victory room's entry). When the room entered is the victory room and the run's last act is `Sts1EndingAct`, play `res://SlaySpoon/audio/ending_stinger.ogg` once through a Godot `AudioStreamPlayer` added to the scene tree on the `Master` bus. Free it when it finishes. The StS1 ending scene images are not used (builder call).

- [ ] **Step 1: Build.** `nice -n 19 make build`: 0 warnings / 0 errors, and the PCK contains `SlaySpoon/audio/ending_stinger.ogg` (or its `.oggvorbisstr` import).
- [ ] **Step 2: Test.** `nice -n 19 make test`: all pass. No new test, because there is no new pure logic.
- [ ] **Step 3: Checklist** (`## Victory after the Heart (WP4-6 Task 9)`):
  - Killing the Heart leads to the Architect and a victory screen; run history shows 4 acts and a win.
  - The StS1 ending stinger plays once at the Architect.
  - Dying to the Heart, or to the Shield and Spear, is a normal defeat ("killed by" shows the localized name).
- [ ] **Step 4: Commit**

```bash
git add src/Act4/EndingStingerPatch.cs godot/SlaySpoon/audio docs/testing/step-6-checklist.md
git commit -m "feat(act4): StS1 ending stinger at the victory room"
```

---

### Task 10: Release: Custom Run option, Standard access, version 0.9.0

**Files:**
- Modify: `src/CustomRunUi/ModifierListPatch.cs`, `src/Core/Act4UnlockRule.cs` (`Act4Access.Released = true`), `tests/Act4UnlockRuleTests.cs`, `mod/SlaySpoon.json` (`"version": "0.9.0"`), `src/ModEntry.cs` (`Version = "0.9.0"`), `CLAUDE.md` (Step 6 status), `README.md` (player view: Act 4), `docs/testing/step-6-checklist.md`
- Test: `tests/ModifierListTests.cs` (create it if no test covers `ModifierListPatch` yet; if its list is not reachable from `src/Core/`, move the list of offered Custom Run modifier types to `src/Core/CustomRunOptions.cs` as type names, and have the patch read it)

**Interfaces:**
- Consumes: everything above.
- Produces: `SpoonAct4` listed in Custom Run; `Act4Access.Released == true`.

Decisions:
- The Custom Run option *is* `SpoonAct4`, ticked in the list. It carries the keys and bypasses the Standard unlock prerequisite. Its existing `SPOON_ACT4.title/description` text is used. Neow keeps ignoring it (`NeowRule.VisibleModifierCount`), so ticking Act 4 does not change Neow's offer.
- Daily is untouched: `ModifierListPatch` runs for the Custom Run list only, and `StandardRunModifiersPatch` adds the flag only in Standard mode.
- Update `InStandardRun_NeedsUnlockAndReleaseOrPreview`: with `Released` true, `InStandardRun(true, false)` is true and `InStandardRun(false, true)` stays false. Rename it to `InStandardRun_NeedsTheUnlockOnceReleased`.

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact] public void InStandardRun_NeedsTheUnlockOnceReleased()
{ Assert.True(Act4Access.InStandardRun(true, false)); Assert.False(Act4Access.InStandardRun(false, false)); Assert.False(Act4Access.InStandardRun(false, true)); }
[Fact] public void CustomRunOption_ListsAct4() => Assert.Contains("SpoonAct4", CustomRunOptions.TypeNames);
[Fact] public void CustomRunOption_IsNotOfferedInDaily() => Assert.False(CustomRunOptions.OfferedIn(gameMode: "Daily"));
```

`CustomRunOptions.OfferedIn(string gameMode) -> bool` is true only for `"Custom"`. The patch checks it with the game's `GameMode` name, so the Daily guarantee is pinned in pure code.

- [ ] **Step 2: Run them to see them fail.** `nice -n 19 make test`: FAIL.
- [ ] **Step 3: Implement.** Flip `Released`; add `SpoonAct4` to the Custom Run list through `CustomRunOptions`; bump both versions to 0.9.0.
  - `CLAUDE.md` Step 6 status: WP4-6 implemented (0.9.0); act routing on key completion; static sprites; Released; Custom Run option; build/test counts; in-game checks pending in `docs/testing/step-6-checklist.md`.
  - `README.md`: a short player paragraph on Act 4: the unlock, the keys, the Custom Run option, and that Daily is unchanged.
- [ ] **Step 4: Build and test.** `nice -n 19 make build && nice -n 19 make test`: 0 warnings / 0 errors; all pass. Write the final counts into `CLAUDE.md`.
- [ ] **Step 5: Checklist** (`## Release (WP4-6 Task 10)`), the spec's owner play-test list:
  - Solo and co-op; no keys and all keys; resume in act 3 and act 4; both act 3 endings (vanilla victory without keys, act 4 with keys).
  - Monster HP and Monster Damage modifiers in act 4; victory and defeat.
  - Standard: the unlock persists after a restart, and once unlocked, Act 4 is offered at every ascension including 0 (with `Act4Preview` off).
  - Custom Run: ticking "Act 4" gives keys and act 4 without the unlock. Daily runs show no keys and end after act 3.
- [ ] **Step 6: Commit**

```bash
git add src/CustomRunUi/ModifierListPatch.cs src/Core/CustomRunOptions.cs src/Core/Act4UnlockRule.cs tests/Act4UnlockRuleTests.cs tests/ModifierListTests.cs \
  mod/SlaySpoon.json src/ModEntry.cs CLAUDE.md README.md docs/testing/step-6-checklist.md
git commit -m "feat(act4): release Act 4 (Custom Run option, Standard access) as 0.9.0"
```

No push and no Workshop upload; the owner decides both.

---

## Builder calls (the owner can change them)

1. **Static sprites, not animations.** The three monsters are PNGs rendered from the StS1 Spine 3.4 setup pose (option b). Converting 3.4 skeletons to StS2's Spine 4.2 runtime is not cheap or certain: the animation timeline formats changed in 4.0, and the Heart has 189 bones and mesh/path attachments. This matches the Watcher's "static visuals first". If a setup pose looks wrong, the fallback is to render frame 0 of the `idle` animation, or to borrow a vanilla monster's visual (option c).
2. **Code-built visuals and encounter layout.** The nodes are built in C#, not as `.tscn` files, so the mod's Godot project never references the game's C# scripts.
3. **The act is appended when the keys complete,** not when act 3 ends. With the keys, the act 3 boss then gives normal boss rewards, as StS1 does before act 4.
4. **Glory's music, map, rest site and combat backgrounds** for The Ending; the StS1 ending scene images are not used.
5. **Act 4 map:** rest site → merchant → elite → Heart, with no ancient and no event.
6. **The Spear (right) applies Surrounded,** mirroring vanilla Rocket. The Shield (left) has Back Attack Left, the Spear Back Attack Right.
7. **Co-op targeting:** Debilitate, Bash's debuff and Burn Strike's Burns hit every player; Beat of Death hits only the player who played the card; Smash gains Block equal to its highest damage dealt to one player.
8. **Monster HP % does not change the Invincible budget** (only ascension and player count, per owner decision 2).
9. **Move names** (StS1 has none): Bash/Heurt, Fortify/Fortifier, Smash/Fracas, Burn Strike/Frappe ardente, Piercer/Perceur, Skewer/Embrocher, Debilitate/Affaiblir, Blood Shots/Tirs sanglants, Echo/Écho, Buff/Renforcement.
10. **Act 4 elite rewards** are vanilla elite rewards (gold, potion roll, card, relic).
11. **Statuses shuffled into the draw pile** take their position from the monster's own RNG, not vanilla `CardPilePosition.Random` (which uses the shared shuffle RNG).
12. **The StS1 ending stinger** plays once at the victory room (optional, dropped if it clashes with FMOD music).
13. **Neow ignores the Act 4 option** in Custom Runs, as it already does in Standard runs.

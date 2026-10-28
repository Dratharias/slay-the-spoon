# Step 1 — Mod skeleton, Custom Run config, monster scaling — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A Slay the Spire 2 mod that loads, adds two Slay the Spoon modifiers (Monster HP %, Monster damage %) to the Custom Run screen with a −/+ value control, syncs them to multiplayer clients, persists them in saves, and applies them in combat.

**Architecture:** One C# assembly `SlaySpoon.dll` plus `SlaySpoon.pck` (localization). Configuration rides on StS2's own `ModifierModel` + `[SavedProperty]` machinery, which already syncs through the lobby and saves with the run. Harmony patches add our modifiers to the Custom Run list only (never to `ModelDb.GoodModifiers`, which Daily runs also use), add the −/+ control, and scale enemy HP at creation. Damage uses the `ModifyDamageMultiplicative` hook. Pure arithmetic lives in game-independent files so it can be unit-tested without the game.

**Tech Stack:** C# / .NET 9 (game runtime 9.0.7), Godot 4.5.1 (GodotSharp 4.5.1), Harmony (`0Harmony.dll` shipped with the game), xUnit, Podman (`mcr.microsoft.com/dotnet/sdk:9.0`), Godot 4.5.1 headless for the PCK.

**Spec:** `docs/superpowers/specs/2026-10-28-slay-the-spoon-design.md` (§3, §4 multipliers, §5 recon, §7 step 1).

## Global Constraints

- Target StS2 v0.107.1; manifest `min_game_version` = `"0.107.1"`.
- Mod id `SlaySpoon` (manifest `id`, DLL/PCK basename, `res://SlaySpoon/...` root). Display name `Slay the Spoon`. C# namespace root `SlaySpoon`.
- Our model classes are prefixed `Spoon` so their ModelDb ids are `SPOON_*` (id = class name in UPPER_SNAKE, e.g. `SealedDeck` → `SEALED_DECK`).
- Every option lives in the Custom Run screen only. Standard and Daily runs must stay vanilla: never patch `ModelDb.GoodModifiers` / `BadModifiers`.
- Multipliers: 25% to 500% in 25% steps, default 100%, separate HP and damage. Applied to every enemy (normal, elite, boss, summoned). HP rounding: round half away from zero, minimum 1.
- All docs and code comments in English. README states: unofficial, not affiliated with Mega Crit; game assets belong to Mega Crit; code is vibe coded (written by an AI, Claude, directed by the author); no paid content. License MIT.
- Decompiled game code (`~/Projects/sts-decomp/`) never enters the repo.
- The dev machine is Bazzite (immutable): build .NET inside Podman, do not install SDKs on the host.
- Repository Makefile exposes `up` / `down` / `restart` (owner convention) plus `build` and `test`.

## Review Focus

1. **Client joins after the host changed a value** → the client's list shows the host's percent, not 100%. (Task 4 checklist item.)
2. **Save & quit mid-run, then continue** (solo and co-op) → the percent survives and still applies in the next combat. (Task 5 checklist item.)
3. **Saved-property net id space** → injecting our property name must not overflow `SavedPropertiesTypeCache.NetIdBitSize`; the mod recomputes it and logs `count`/`bits`. (Task 3 log assertion.)
4. **Summoned minions and multi-creature fights** → every enemy is scaled exactly once, including ones spawned mid-combat. (Task 5 checklist item.)
5. **Mod installed, modifiers unticked, or a Standard/Daily run** → monster HP and damage are exactly vanilla. (Task 2 tests for 100%, Task 5 checklist item.)

---

### Task 1: Repository scaffold and build pipeline

**Files:**
- Create: `.gitignore` (ignore `build/`, `bin/`, `obj/`, `*.user`, `.godot/`)
- Create: `LICENSE` (MIT, copyright 2026 Dratharias)
- Create: `README.md`
- Create: `Makefile`
- Create: `src/SlaySpoon.csproj`
- Create: `src/ModEntry.cs`
- Create: `mod/SlaySpoon.json`
- Create: `godot/project.godot`, `godot/export_presets.cfg`
- Create: `godot/SlaySpoon/localization/eng/modifiers.json`, `godot/SlaySpoon/localization/fra/modifiers.json` (empty object `{}` for now)
- Create: `tools/build-pck.sh`

**Interfaces:**
- Produces: `make build` → `build/mods/SlaySpoon/{SlaySpoon.json,SlaySpoon.dll,SlaySpoon.pck}`; `make up` → build + copy that folder to `$(STS2_DIR)/mods/SlaySpoon/`; `make down` → remove it; `make restart` → `down` then `up`; `make test` → runs xUnit tests in the container.
- Produces: `public static class SlaySpoon.ModEntry` with `public static void Initialize()`, marked `[ModInitializer(nameof(Initialize))]`; it creates `new Harmony("slayspoon")` and calls `PatchAll(typeof(ModEntry).Assembly)`. Later tasks add registration calls inside `Initialize()`.
- Produces: `public static class SlaySpoon.SpoonLog` with `public static void Info(string message)` prefixing `[SlaySpoon] ` and writing through the game's `Log.Info`.

- [ ] **Step 1: Write `src/SlaySpoon.csproj`**
  `TargetFramework` `net9.0`, `Nullable` enable, `LangVersion` latest, `AssemblyName` `SlaySpoon`. Property `Sts2DataDir` (default `$(HOME)/.local/share/Steam/steamapps/common/Slay the Spire 2/data_sts2_linuxbsd_x86_64`). References `sts2.dll`, `0Harmony.dll`, `GodotSharp.dll` from `$(Sts2DataDir)` with `Private=false`. Exclude `../tests/**`.

- [ ] **Step 2: Write `mod/SlaySpoon.json`**
  ```json
  {
    "id": "SlaySpoon",
    "name": "Slay the Spoon",
    "author": "Dratharias",
    "description": "Slay the Spire 1 cards and run options for Slay the Spire 2 custom runs. Unofficial.",
    "version": "0.1.0",
    "has_pck": true,
    "has_dll": true,
    "dependencies": [],
    "affects_gameplay": true,
    "min_game_version": "0.107.1"
  }
  ```

- [ ] **Step 3: Write `ModEntry` and `SpoonLog`** (interfaces above). `Initialize()` logs `Slay the Spoon 0.1.0 loaded`.

- [ ] **Step 4: Write `tools/build-pck.sh`**
  Downloads `Godot_v4.5.1-stable_linux.x86_64.zip` from the official GitHub release into `~/.cache/slay-spoon/godot/` if missing, then runs `godot --headless --path godot --export-pack "SlaySpoon" <out>.pck`. The export preset in `godot/export_presets.cfg` is a pack-only preset named `SlaySpoon` that includes `SlaySpoon/**`. If Godot refuses without export templates, install only the templates matching 4.5.1 into the same cache dir.

- [ ] **Step 5: Write `Makefile`**
  `STS2_DIR ?= $(HOME)/.local/share/Steam/steamapps/common/Slay the Spire 2` (quote it everywhere, it contains spaces). `build`: `podman run --rm` the .NET 9 SDK image with the repo mounted rw and `$(STS2_DIR)/data_sts2_linuxbsd_x86_64` mounted ro at `/sts2`, `dotnet build src -c Release -p:Sts2DataDir=/sts2`; then `tools/build-pck.sh`; then stage the three files in `build/mods/SlaySpoon/`. `test`: same container, `dotnet test tests`.

- [ ] **Step 6: Write `README.md`**
  Sections: what it is (one paragraph, fr+en not required, English), features (step 1 list), install (Workshop later; manual: copy `build/mods/SlaySpoon` into `<StS2>/mods/`), build (`make build`, needs Podman), disclaimer (the Global Constraints README sentences, verbatim intent), license.

- [ ] **Step 7: Build and install**
  Run: `make up`
  Expected: `build/mods/SlaySpoon/` contains `SlaySpoon.json`, `SlaySpoon.dll`, `SlaySpoon.pck`, and the same three files exist in `$(STS2_DIR)/mods/SlaySpoon/`.

- [ ] **Step 8: Verify the game loads the mod**
  Launch StS2 (accept the mod-loading prompt the first time), reach the main menu, quit. Find the log with `find ~/.local/share -name godot.log -path '*lay*pire*2*'`.
  Expected in the log: `Loading assembly DLL ...SlaySpoon.dll`, `Loading Godot PCK ...SlaySpoon.pck`, `[SlaySpoon] Slay the Spoon 0.1.0 loaded`, no `Failed` state for `SlaySpoon`.

- [ ] **Step 9: Commit**
  ```bash
  git add .gitignore LICENSE README.md Makefile src mod godot tools
  git commit -m "feat: mod skeleton and build pipeline"
  ```

### Task 2: Scaling arithmetic (pure, unit-tested)

**Files:**
- Create: `src/Core/ScalingMath.cs` (no `MegaCrit` / `Godot` usings)
- Create: `tests/SlaySpoon.Tests.csproj` (xUnit, `net9.0`, compiles `../src/Core/**/*.cs` as linked files, no game references)
- Create: `tests/ScalingMathTests.cs`

**Interfaces:**
- Produces in `SlaySpoon.Core.ScalingMath` (static class):
  - `public const int MinPercent = 25, MaxPercent = 500, StepPercent = 25, DefaultPercent = 100;`
  - `public static int Step(int percent, int direction)` — `direction` is `+1` or `-1`; returns the next multiple of 25 clamped to [25, 500].
  - `public static int ScaleHp(int baseHp, int percent)` — round half away from zero, minimum 1.
  - `public static decimal DamageMultiplier(int percent)` — `percent / 100m`.

- [ ] **Step 1: Write the failing tests**
  ```csharp
  [Theory]
  [InlineData(100, 1, 125)] [InlineData(100, -1, 75)]
  [InlineData(25, -1, 25)]  [InlineData(500, 1, 500)]
  [InlineData(110, 1, 125)] [InlineData(110, -1, 100)]   // off-grid values snap to the grid
  public void Step_moves_one_notch_and_clamps(int p, int dir, int expected) =>
      Assert.Equal(expected, ScalingMath.Step(p, dir));

  [Theory]
  [InlineData(40, 100, 40)] [InlineData(40, 150, 60)] [InlineData(41, 150, 62)] // 61.5 → 62
  [InlineData(1, 25, 1)]    [InlineData(3, 25, 1)]    [InlineData(500, 500, 2500)]
  public void ScaleHp_rounds_half_up_with_minimum_one(int hp, int p, int expected) =>
      Assert.Equal(expected, ScalingMath.ScaleHp(hp, p));

  [Fact] public void DamageMultiplier_default_is_exactly_one() =>
      Assert.Equal(1m, ScalingMath.DamageMultiplier(ScalingMath.DefaultPercent));
  [Fact] public void DamageMultiplier_quarter() =>
      Assert.Equal(0.25m, ScalingMath.DamageMultiplier(25));
  ```

- [ ] **Step 2: Run tests to verify they fail**
  Run: `make test`
  Expected: build error, `ScalingMath` does not exist.

- [ ] **Step 3: Implement `ScalingMath`** (interface above).

- [ ] **Step 4: Run tests to verify they pass**
  Run: `make test`
  Expected: all tests pass.

- [ ] **Step 5: Commit**
  ```bash
  git add src/Core tests
  git commit -m "feat: monster scaling arithmetic with tests"
  ```

### Task 3: The two modifiers and saved-property registration

**Files:**
- Create: `src/Config/SpoonPercentModifier.cs`
- Create: `src/Config/SpoonMonsterHp.cs`
- Create: `src/Config/SpoonMonsterDamage.cs`
- Create: `src/Config/SavedPropertyRegistration.cs`
- Modify: `src/ModEntry.cs` (call `SavedPropertyRegistration.Register()` first in `Initialize()`)
- Modify: `godot/SlaySpoon/localization/{eng,fra}/modifiers.json`

**Interfaces:**
- Consumes: `ScalingMath` (Task 2).
- Produces: `public abstract class SlaySpoon.Config.SpoonPercentModifier : ModifierModel` with
  - `[SavedProperty] public int Percent { get; set; }` defaulting to `ScalingMath.DefaultPercent`; setter calls `AssertMutable()`.
  - `public override LocString Description` → base description with variable `Percent` added.
- Produces: `public sealed class SpoonMonsterHp : SpoonPercentModifier` and `public sealed class SpoonMonsterDamage : SpoonPercentModifier` (ids `SPOON_MONSTER_HP`, `SPOON_MONSTER_DAMAGE`). Damage behaviour is added in Task 5.
- Produces: `public static class SavedPropertyRegistration` with `public static void Register()`.

Why registration is needed: `SavedPropertiesTypeCache` only scans the game's own `AbstractModelSubtypes`, so mod properties are invisible to save/sync unless injected, and it sizes network property ids with `NetIdBitSize = ceil(log2(nameCount))` once in its static constructor.

- [ ] **Step 1: Implement `SavedPropertyRegistration.Register()`**
  Call `SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(SpoonMonsterHp))` and `(typeof(SpoonMonsterDamage))`, then recompute `NetIdBitSize` from the private `_netIdToPropertyNameMap.Count` with the same formula (`Mathf.CeilToInt(Math.Log2(count))`) and set it through Harmony `AccessTools` (private setter). Log `[SlaySpoon] saved properties: count=<n> bits=<b>`. Every player runs the same mod version (the game enforces matching mods), so the map and bit size come out identical on all peers.

- [ ] **Step 2: Implement the three modifier classes** (interfaces above).

- [ ] **Step 3: Fill localization**
  `eng/modifiers.json`:
  ```json
  {
    "SPOON_MONSTER_HP.title": "Monster HP",
    "SPOON_MONSTER_HP.description": "Enemies have {Percent}% of their normal HP.",
    "SPOON_MONSTER_DAMAGE.title": "Monster Damage",
    "SPOON_MONSTER_DAMAGE.description": "Enemies deal {Percent}% of their normal damage."
  }
  ```
  `fra/modifiers.json`: `"PV des monstres"`, `"Les ennemis ont {Percent} % de leurs PV normaux."`, `"Dégâts des monstres"`, `"Les ennemis infligent {Percent} % de leurs dégâts normaux."`

- [ ] **Step 4: Build, install, verify in the log**
  Run: `make restart`, launch StS2 to the main menu, quit.
  Expected in `godot.log`: `[SlaySpoon] saved properties: count=<n> bits=<b>` with `2^b >= n`; `Found loc table from mod: eng modifiers`; no exception mentioning `SlaySpoon`.

- [ ] **Step 5: Commit**
  ```bash
  git add src godot
  git commit -m "feat: monster HP and damage modifiers with saved percent"
  ```

### Task 4: Custom Run screen — list entries, −/+ control, client sync

**Files:**
- Create: `src/CustomRunUi/ModifierListPatch.cs`
- Create: `src/CustomRunUi/PercentStepper.cs`
- Create: `src/CustomRunUi/SyncModifierListPatch.cs`
- Create: `docs/testing/step-1-checklist.md`

**Interfaces:**
- Consumes: `SpoonPercentModifier`, `SpoonMonsterHp`, `SpoonMonsterDamage` (Task 3); `ScalingMath.Step` (Task 2).
- Produces: `public static class PercentStepper` with
  - `public static void Attach(NRunModifierTickbox tickbox)` — adds the −/+ buttons to a tickbox whose `Modifier is SpoonPercentModifier`.
  - `public static void RefreshLabel(NRunModifierTickbox tickbox)` — rebuilds the label text.

Game facts the patches rely on (all in the decompiled `NCustomRunModifiersList.cs`, `NRunModifierTickbox.cs`, `NCustomRunScreen.cs`):
- `NCustomRunModifiersList.GetAllModifiers()` (private, `IEnumerable<ModifierModel>`) feeds `_Ready`, which creates one `NRunModifierTickbox` per model.
- Ticked tickboxes' `Modifier` instances are what `GetModifiersTickedOn()` returns and what the host sends with `Lobby.SetModifiers(...)` when the list emits `ModifiersChanged`. Mutating `Percent` on that instance and emitting `ModifiersChanged` re-sends it.
- Clients receive changes through `SyncModifierList(IReadOnlyList<ModifierModel>)`, which only sets `IsTicked` (matching by `IsEquivalent`, i.e. by type).
- The tickbox label is the private `_label` (`MegaRichTextLabel`), built from `new LocString("main_menu_ui", "CUSTOM_RUN_SCREEN.MODIFIER_LABEL")` with variables `color`, `modifier_title`, `modifier_description`. Modifiers in neither Good nor Bad lists get `color` = `blue`.
- In client and load modes, `Initialize(MultiplayerUiMode)` disables every tickbox.

- [ ] **Step 1: `ModifierListPatch`** — Harmony postfix on `NCustomRunModifiersList.GetAllModifiers` that appends `ModelDb.Modifier<SpoonMonsterHp>().ToMutable()` and `ModelDb.Modifier<SpoonMonsterDamage>().ToMutable()` to `__result`.

- [ ] **Step 2: `PercentStepper`** — Harmony postfix on `NRunModifierTickbox._Ready` calling `Attach`. `Attach` adds two plain Godot `Button` nodes (`−`, `+`; no custom node subclasses, so no Godot script registration is needed) styled to sit at the right end of the row. Pressing one: `Percent = ScalingMath.Step(Percent, ∓1)`, `RefreshLabel`, then emit `NCustomRunModifiersList.SignalName.ModifiersChanged` on the owning list (walk up the parent chain). The buttons are disabled whenever the tickbox is disabled (client and load modes).

- [ ] **Step 3: `SyncModifierListPatch`** — postfix on `NCustomRunModifiersList.SyncModifierList`: for each of our tickboxes, copy `Percent` from the synced modifier of the same type (if present) into `tickbox.Modifier`, then `RefreshLabel`.

- [ ] **Step 4: Write `docs/testing/step-1-checklist.md`** with the in-game checks of this task and Task 5. This task's items:
  - Solo Custom Run: both Slay the Spoon entries are listed; − / + step by 25 between 25% and 500%; the label shows the value.
  - Standard run and Daily run screens: no Slay the Spoon entries.
  - Co-op (2 players), host Custom Run: the client sees the host's ticks **and** values live; the client cannot change them.
  - Host sets 200% **before** the client joins; after joining, the client shows 200% (Review Focus 1).

- [ ] **Step 5: Build, install, run the checklist items above**
  Run: `make restart`, then the Task 4 checklist items. The co-op items need the owner and a friend.
  Expected: every item behaves as written.

- [ ] **Step 6: Commit**
  ```bash
  git add src/CustomRunUi docs/testing
  git commit -m "feat: Slay the Spoon entries and percent control in Custom Run"
  ```

### Task 5: Apply the multipliers in combat

**Files:**
- Create: `src/Combat/EnemyHpPatch.cs`
- Modify: `src/Config/SpoonMonsterDamage.cs`
- Modify: `docs/testing/step-1-checklist.md`

**Interfaces:**
- Consumes: `ScalingMath.ScaleHp`, `ScalingMath.DamageMultiplier` (Task 2); `SpoonMonsterHp`, `SpoonMonsterDamage` (Task 3).

Game facts: every creature in combat is created by `CombatState.CreateCreature(MonsterModel, CombatSide, string?)`, which for enemies rolls HP (`SetUniqueMonsterHpValue`) and applies co-op scaling (`ScaleMonsterHpForMultiplayer`) before returning. `Creature.SetMaxHpInternal(decimal)` / `SetCurrentHpInternal(decimal)` set HP without VFX. `CombatState.RunState.Modifiers` lists the run's modifiers. Modifiers receive combat hooks; `ModifyDamageMultiplicative` returns a factor (see `WeakPower.cs`).

- [ ] **Step 1: `EnemyHpPatch`** — Harmony postfix on `CombatState.CreateCreature`: if `__result.Side == CombatSide.Enemy` and the run has a `SpoonMonsterHp`, set max HP to `ScalingMath.ScaleHp(__result.MaxHp, percent)` and current HP to the new max. This runs once per creature at creation, after co-op scaling, so mid-combat summons are covered and nothing is scaled twice.

- [ ] **Step 2: Damage** — `SpoonMonsterDamage` overrides `ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)`: returns `ScalingMath.DamageMultiplier(Percent)` when `dealer?.Side == CombatSide.Enemy`, else `1m`. The game's own damage pipeline does the rounding (as for Weak), so no extra minimum is applied to damage.

- [ ] **Step 3: Add this task's checklist items**
  - Solo, HP 200%: first enemy's HP is double the vanilla range; with HP 25%, a 3-HP enemy has 1 HP.
  - Damage 50%: intent numbers and HP actually lost are halved compared to the same fight without the modifier.
  - A fight with summons (e.g. an enemy that spawns minions): spawned minions are scaled too, once.
  - Co-op: host and client see identical enemy HP.
  - Save & quit during a run with HP 200% + damage 50%, continue: next combat still scaled; the Custom Run summary/resume keeps 200%/50% (Review Focus 2).
  - Mod installed but entries unticked, and a Standard run: enemy HP and damage are vanilla (Review Focus 5).

- [ ] **Step 4: Build, install, run the full checklist**
  Run: `make restart`, then every item of `docs/testing/step-1-checklist.md`.
  Expected: every item behaves as written.

- [ ] **Step 5: Commit**
  ```bash
  git add src docs/testing
  git commit -m "feat: apply monster HP and damage multipliers in combat"
  ```

# Step 6 WP3 — Act 4 Unlock, Team Keys and Run State Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Record the permanent Act 4 unlock, carry an "Act 4 run" flag and the team-wide Ruby, Emerald and Sapphire keys
through a run (saved and synced), and let players collect each key, without adding the act itself (WP4).

**Architecture:** A hidden modifier, `SpoonAct4`, marks an Act 4 run and owns the key state through one saved property,
as `SpoonMerchantLoans` owns the debt ledger. Every key change runs on every peer in the same order (rest site
choice, combat victory, or a queued `GameAction`), so the state never needs its own network message. Pure rules live
in `src/Core/` with xUnit tests; Harmony patches and Godot nodes only wire them into the game.

**Tech Stack:** C# .NET 9, Harmony, Godot 4.5.1, xUnit, pytest; `make build`, `make test` (Podman).

**Spec:** `docs/superpowers/plans/2026-10-29-step-6-act4.md` (work package 3 and the approved unlock rule) and
`docs/review/act4-mechanics.md` (section "Keys").

## Global Constraints

- The mod must never write the regular (unmodded) save. The unlock goes to `mod_configs/SlaySpoon.cfg`.
- Unlock: the first time a player reaches Act 3 (act index 2) in a **Standard** run at **Ascension 10**, Act 4 is
  unlocked for good; victory is not needed and a defeat never revokes it. Once unlocked, it applies at every ascension, 0 included.
- Daily runs stay vanilla. Custom Runs get Act 4 only through their own option (WP6), never through the unlock.
- Keys are team-wide: a key any player collects counts for all.
- Ruby: an extra rest site option; its taker gives up their own rest action; it disappears for the team once taken.
- Sapphire: once a chest's relics are handed out, every player who received one may give it back for the key; the
  first to accept takes the key for the team; the swap undoes what the relic did on pickup (upgrades included).
- Emerald: one elite per act (acts 1-3) is marked and buffed; winning that fight gives the key to the team, at no individual cost.
- Seeds stay vanilla: every random choice the mod adds uses its own `Rng(run.Rng.Seed, "<SlaySpoon name>")`, never a vanilla RNG.
- Saved properties reuse vanilla property names (the loans ledger uses `FurCoatCoordCols`).
- Every new text exists in `godot/SlaySpoon/localization/eng/` and `.../fra/`. Repo text in English.
- No lobby option is exposed in this package. Standard access stays off until WP6 (`Act4Access.Released = false`);
  the owner previews it with the config key `Act4Preview` (Task 4).
- Never commit decompiled game code. Commit on `step-6-act4` with the trailer
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Review Focus

1. **Two players pick the Ruby at the same moment in co-op.** Each peer may process the two picks in a different order;
   both must reach the same state: the key is held, both players lose their rest action. (Task 1 tests
   `Take_FirstTakeChangesSecondDoesNot` and `Take_IsOrderIndependent`; Task 6: `OnSelect` always returns true.)
2. **Two players accept the Sapphire swap at the same moment.** The queued actions run in one order everywhere: the
   first takes the key and loses their relic, the second keeps their relic. (Task 1 test `Take_FirstTakeChangesSecondDoesNot`;
   Task 8: the action reverts nothing when `Take` returns false.)
3. **A swapped relic whose pickup changed the deck in a way that can't be undone** (removed or transformed cards):
   no swap button for that relic. (Task 8 test `Plan_RemovedCardIsIrreversible`.)
4. **A run saved and resumed** (solo and `make mp`) after a key was taken, and on the act whose Emerald node is
   stored: keys and marked node come back identical, the map keeps the same marked node. (Task 1 test `Encode_RoundTripsKeysAndNodes`,
   Task 7 test `MarkAct_KeepsAnAlreadyStoredNode`.)
5. **A Standard run reaching Act 3 at A10 that is loaded rather than entered** (resume of a save made in Act 3): the
   unlock is still recorded. (Task 3 patch covers both `SetActInternal` and saved-run load; checklist item.)

---

## File map

| File | Responsibility |
|------|----------------|
| `src/Core/Act4Keys.cs` | Pure team key state + Emerald nodes, encode/decode for the saved property |
| `src/Core/Act4UnlockRule.cs` | Pure unlock and access rules |
| `src/Core/SpoonConfig.cs` | Config JSON: merge keys instead of overwriting; unlock per profile |
| `src/Core/EmeraldRule.cs` | Pure Emerald node pick and buff values |
| `src/Core/SapphireRevert.cs` | Pure pickup-diff rule for the Sapphire swap |
| `src/Config/Act4Modifier.cs` | `SpoonAct4` hidden modifier: saved keys, rest option, Emerald hooks |
| `src/Act4/` (new folder) | Patches: run start injection, unlock recording, Ruby UI, Emerald map marker, Sapphire swap action and UI |
| `src/Act4/Sts1RegenerateMonsterPower.cs` | Non-decaying monster Regenerate |
| `src/TopBar/TopBarPercentPatch.cs` | Key display for `SpoonAct4` |
| `godot/SlaySpoon/images/keys/` | Six StS1 key PNGs copied from `data/sts1/act4/assets/images/` |

---

### Task 1: Team key state

**Files:**
- Create: `src/Core/Act4Keys.cs`
- Test: `tests/Act4KeysTests.cs`

**Interfaces:**
- Produces:
  - `public enum Act4Key { Ruby = 1, Emerald = 2, Sapphire = 4 }`
  - `public sealed class Act4Keys`:
    - `bool Has(Act4Key key)`, `bool HasAll`;
    - `bool Take(Act4Key key)` → true only when the key was missing (idempotent);
    - `(int Col, int Row)? EmeraldNode(int actIndex)`, `void MarkEmerald(int actIndex, int col, int row)`;
    - `int[] Encode()`, `static Act4Keys Decode(int[] data)` (throws `ArgumentException` on bad data);
    - `static event Action? Changed` raised by `Take` when it changes something and by `Decode` callers through `NotifyChanged()`.
- Encoding: `[1 (version), flags bitmask, n, then n × (actIndex, col, row)]`; an empty array decodes to no key, no node.

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact] public void Take_FirstTakeChangesSecondDoesNot() { var k = new Act4Keys(); Assert.True(k.Take(Act4Key.Ruby)); Assert.False(k.Take(Act4Key.Ruby)); Assert.True(k.Has(Act4Key.Ruby)); }
[Fact] public void HasAll_NeedsTheThreeKeys() { var k = new Act4Keys(); k.Take(Act4Key.Ruby); k.Take(Act4Key.Emerald); Assert.False(k.HasAll); k.Take(Act4Key.Sapphire); Assert.True(k.HasAll); }
[Fact] public void Encode_RoundTripsKeysAndNodes() { var k = new Act4Keys(); k.Take(Act4Key.Sapphire); k.MarkEmerald(0, 3, 5); k.MarkEmerald(1, 2, 9);
    var back = Act4Keys.Decode(k.Encode()); Assert.True(back.Has(Act4Key.Sapphire)); Assert.False(back.Has(Act4Key.Ruby));
    Assert.Equal((3, 5), back.EmeraldNode(0)); Assert.Equal((2, 9), back.EmeraldNode(1)); Assert.Null(back.EmeraldNode(2)); }
[Fact] public void Take_IsOrderIndependent() { var a = new Act4Keys(); a.Take(Act4Key.Ruby); a.Take(Act4Key.Sapphire); a.Take(Act4Key.Ruby);
    var b = new Act4Keys(); b.Take(Act4Key.Sapphire); b.Take(Act4Key.Ruby); Assert.Equal(a.Encode(), b.Encode()); }
[Fact] public void Decode_EmptyIsAFreshState() { Assert.False(Act4Keys.Decode(Array.Empty<int>()).Has(Act4Key.Ruby)); }
[Theory] [InlineData(new[] { 2, 0, 0 })] [InlineData(new[] { 1, 8, 0 })] [InlineData(new[] { 1, 0, 1, 0, 3 })]
public void Decode_RejectsBadData(int[] data) { Assert.Throws<ArgumentException>(() => Act4Keys.Decode(data)); }
```

- [ ] **Step 2: Run** `make test` — Expected: build error, `Act4Keys` not defined.
- [ ] **Step 3: Implement `Act4Keys`** following `LoanLedger`'s encode/decode style (flags above 7 and wrong lengths throw).
- [ ] **Step 4: Run** `make test` — Expected: PASS, every previous test still passes.
- [ ] **Step 5: Commit** `feat(act4): team key state`.

### Task 2: Unlock rule and config merge

**Files:**
- Create: `src/Core/Act4UnlockRule.cs`
- Modify: `src/Core/SpoonConfig.cs`, `src/Saves/SpoonConfigFile.cs`, `tests/SpoonConfigTests.cs`
- Test: `tests/Act4UnlockRuleTests.cs`

**Interfaces:**
- Produces:
  - `Act4UnlockRule.ShouldUnlock(bool standard, int ascension, int actIndex) → bool` (standard && ascension ≥ 10 && actIndex ≥ 2);
  - `Act4Access.Released` (`public const bool Released = false;` flipped in WP6) and
    `Act4Access.InStandardRun(bool unlocked, bool preview) → bool` = `(Released || preview) && unlocked`;
  - `SpoonConfig.Read(string? json) → Dictionary<string, string>` (empty on null or bad JSON);
  - `SpoonConfig.With(string? json, string key, string value) → string` (keeps every other key);
  - `SpoonConfig.Act4UnlockKey(int profileId) → "Act4Unlocked.profile{id}"`, `SpoonConfig.PreviewKey = "Act4Preview"`;
  - `SpoonConfig.ReadFlag(string? json, string key) → bool` ("True" only);
  - `SpoonConfig.WriteSyncChoice(string? json, bool choice) → string` now takes the current file content (today: `WriteSyncChoice(bool)`);
  - `SpoonConfigFile.Act4Unlocked(int profileId) → bool`, `SpoonConfigFile.SaveAct4Unlocked(int profileId)`, `SpoonConfigFile.Act4Preview → bool`.

- [ ] **Step 1: Write the failing tests**

```csharp
[Theory]
[InlineData(true, 10, 2, true)]   // reaches Act 3 at A10
[InlineData(true, 10, 1, false)]  // only Act 2
[InlineData(true, 9, 2, false)]   // below A10
[InlineData(false, 10, 2, false)] // Custom or Daily
[InlineData(true, 10, 3, true)]   // later acts too
public void ShouldUnlock(bool standard, int ascension, int actIndex, bool expected) => Assert.Equal(expected, Act4UnlockRule.ShouldUnlock(standard, ascension, actIndex));
[Fact] public void InStandardRun_NeedsUnlockAndReleaseOrPreview() { Assert.False(Act4Access.InStandardRun(true, false)); Assert.True(Act4Access.InStandardRun(true, true)); Assert.False(Act4Access.InStandardRun(false, true)); }
// SpoonConfigTests
[Fact] public void WriteSyncChoice_KeepsTheUnlock() { string json = SpoonConfig.With(null, SpoonConfig.Act4UnlockKey(1), "True");
    json = SpoonConfig.WriteSyncChoice(json, false); Assert.True(SpoonConfig.ReadFlag(json, SpoonConfig.Act4UnlockKey(1))); Assert.False(SpoonConfig.ReadSyncChoice(json)); }
[Fact] public void Unlock_IsPerProfile() { string json = SpoonConfig.With(null, SpoonConfig.Act4UnlockKey(1), "True"); Assert.False(SpoonConfig.ReadFlag(json, SpoonConfig.Act4UnlockKey(2))); }
[Fact] public void Read_BadJsonIsEmpty() { Assert.Empty(SpoonConfig.Read("{nope")); }
```

- [ ] **Step 2: Run** `make test` — Expected: build errors on the missing members.
- [ ] **Step 3: Implement** the rule, `Act4Access`, and the config helpers; update `SaveSyncChoice` to pass the current file
  content. Nothing ever removes the unlock key (a defeat cannot revoke it).
- [ ] **Step 4: Run** `make test` — Expected: PASS, the existing `SpoonConfigTests` adapted to the new signature.
- [ ] **Step 5: Commit** `feat(act4): unlock rule and config key merge`.

### Task 3: Record the unlock in game

**Files:**
- Create: `src/Act4/Act4UnlockPatch.cs`

**Interfaces:**
- Consumes: `Act4UnlockRule.ShouldUnlock`, `SpoonConfigFile.Act4Unlocked/SaveAct4Unlocked` (Task 2).
- Produces: `Act4UnlockPatch.CurrentProfileId() → int` (the `_profileIdProvider` read already used by `ProgressSyncPatch`; move it to a shared helper rather than copy it).

- [ ] **Step 1: Postfix on `RunManager.SetActInternal(int actIndex)`** (async: wrap `__result` in a continuation, as other async
  postfixes in the mod do) and on `RunManager.InitializeSavedRun` (after the state exists): when
  `ShouldUnlock(State.GameMode == GameMode.Standard, State.AscensionLevel, State.CurrentActIndex)` and the profile is not
  yet unlocked, save it and log `[SlaySpoon] act4: unlocked for profile N`. Grep `InitializeSavedRun` first: if it already
  goes through `SetActInternal`, patch only that one.
- [ ] **Step 2: Run** `make build` — Expected: 0 warnings, 0 errors.
- [ ] **Step 3: Add checklist items** to `docs/testing/step-6-checklist.md` (create it, same layout as `step-5-checklist.md`):
  Standard A10 → reach Act 3 → cfg holds `Act4Unlocked.profile1`; A9 Act 3 and A10 Act 2 don't; defeat afterwards keeps it;
  resuming an Act 3 save at A10 records it; the regular save folder is untouched.
- [ ] **Step 4: Commit** `feat(act4): record the permanent unlock`.

### Task 4: The `SpoonAct4` run flag

**Files:**
- Create: `src/Config/Act4Modifier.cs`, `src/Act4/StandardRunModifiersPatch.cs`
- Modify: `src/Core/NeowRule.cs`, `src/Neow/NeowBlessingPatch.cs`, `tests/NeowRuleTests.cs`,
  `godot/SlaySpoon/localization/{eng,fra}/modifiers.json`

**Interfaces:**
- Consumes: `Act4Keys` (Task 1), `Act4Access`, `SpoonConfigFile.Act4Preview/Act4Unlocked` (Task 2).
- Produces:
  - `public sealed class SpoonAct4 : ModifierModel` with `public Act4Keys Keys`, `[SavedProperty] public int[] FurCoatCoordCols`
    (get `Keys.Encode()`; set decodes, logs and falls back to a fresh state on `ArgumentException`, then `Act4Keys.NotifyChanged()`);
  - `static SpoonAct4? SpoonAct4.Of(IRunState? run)` → the run's instance or null.
  - `NeowRule.VisibleModifierCount(int count, int hidden, bool blessingTicked, bool anyNeowOption, bool forceBlessings)`.

- [ ] **Step 1: Write the failing Neow test**

```csharp
[Fact] public void HiddenModifierAloneLeavesTheBlessings() => Assert.Equal(0, NeowRule.VisibleModifierCount(1, hidden: 1, blessingTicked: false, anyNeowOption: false, forceBlessings: false));
[Fact] public void HiddenModifierIsNotCountedNextToOthers() => Assert.Equal(1, NeowRule.VisibleModifierCount(2, hidden: 1, blessingTicked: false, anyNeowOption: true, forceBlessings: false));
```

- [ ] **Step 2: Run** `make test` — Expected: build error on the new parameter.
- [ ] **Step 3: Implement** `count - hidden` as the starting count; `NeowBlessingPatch.VisibleCount` passes
  `modifiers.Count(m => m is SpoonAct4)` as `hidden`. Update existing NeowRule tests to pass `hidden: 0`.
- [ ] **Step 4: Run** `make test` — Expected: PASS.
- [ ] **Step 5: Add `SpoonAct4`** and its text: EN title "Act 4", FR "Acte 4"; EN description "The way to the Heart is open:
  find the Ruby, Emerald and Sapphire keys.", FR "Le chemin du Cœur est ouvert : trouvez les clés de rubis, d'émeraude et de saphir."
  Do not add it to `ModifierListPatch` (WP6 does).
- [ ] **Step 6: Inject it into Standard runs** (`StandardRunModifiersPatch`):
  - prefix on `StartRunLobby.BeginRunForAllPlayers(string seed, List<ModifierModel> modifiers)` (host and solo): when
    `GameMode == Standard`, `Act4Access.InStandardRun(unlocked for the host's profile, Act4Preview)` and no `SpoonAct4` in the
    list, replace `modifiers` with a copy plus a mutable `SpoonAct4` (created the way the Custom list creates modifiers, through `ModelDb`);
  - prefix on `NCharacterSelectScreen.BeginRun(seed, acts, modifiers)`: stash the list in a static `Pending`, then pass an
    empty list to vanilla (no "ignoring" error);
  - prefixes on `NGame.StartNewSingleplayerRun` (when `gameMode == Standard`) and `NGame.StartNewMultiplayerRun` (when
    `lobby.GameMode == Standard`): replace `modifiers` with `Pending` when it is non-empty, then clear `Pending`.
  Builder call: in co-op the host's unlock decides for the team; clients receive the flag in `LobbyBeginRunMessage`.
- [ ] **Step 7: Run** `make build` and `make test` — Expected: 0 warnings / 0 errors; all tests pass.
- [ ] **Step 8: Checklist items:** with `"Act4Preview": "True"` and an unlocked profile, a Standard run's log line lists
  `SPOON_ACT4`, Neow offers its blessings, the run resumes with the flag; without the unlock or the preview, no flag;
  Daily and Custom runs never get it; a co-op client gets the host's flag.
- [ ] **Step 9: Commit** `feat(act4): hidden run flag for Standard runs`.

### Task 5: Key sprites and top bar

**Files:**
- Create: `godot/SlaySpoon/images/keys/{key_red,key_green,key_blue,ruby_key,emerald_key,sapphire_key}.png`
- Modify: `src/TopBar/TopBarPercentPatch.cs`, `tools/sts1/tests/test_act4.py`

**Interfaces:**
- Consumes: `SpoonAct4.Keys`, `Act4Keys.Changed` (Tasks 1, 4).
- Produces: `KeySprites.Path(Act4Key key, bool large) → "res://SlaySpoon/images/keys/<file>.png"` in `src/Act4/KeySprites.cs`.

- [ ] **Step 1: Write the failing pytest** `test_key_sprites_match_the_import`: each of the six files under
  `godot/SlaySpoon/images/keys/` is byte-identical to its source under `data/sts1/act4/assets/images/` (`ui/topPanel/` for
  the small ones, `largeRelics/` for the large ones).
- [ ] **Step 2: Run** `make test` — Expected: FAIL, files missing.
- [ ] **Step 3: Copy the six PNGs**; run `make test` — Expected: PASS.
- [ ] **Step 4: Top bar:** a `case SpoonAct4 act4:` that replaces the icon with three small key textures (red, green, blue
  order, as StS1), each at full opacity when held and at 0.25 when missing; refresh on `Act4Keys.Changed`, unsubscribe on
  `TreeExiting` (same shape as `ShowDebts`). The hover tip stays the modifier's.
- [ ] **Step 5: Run** `make build` — Expected: 0 warnings / 0 errors. Checklist: three dim keys at run start; a key brightens
  for every player when one is taken.
- [ ] **Step 6: Commit** `feat(act4): key sprites and top bar display`.

### Task 6: Ruby rest site option

**Files:**
- Create: `src/Act4/Sts1RubyRestSiteOption.cs`, `src/Act4/RubyRestSitePatch.cs`
- Modify: `src/Config/Act4Modifier.cs`, `godot/SlaySpoon/localization/{eng,fra}/rest_site_ui.json` (new table file)

**Interfaces:**
- Consumes: `SpoonAct4.Of`, `Act4Keys.Take`, `KeySprites.Path`.
- Produces: `Sts1RubyRestSiteOption : RestSiteOption`, `OptionId = "SPOON_RUBY"`.

No new pure logic: the key rule is Task 1's `Take`, already tested for repeated and reordered takes.

- [ ] **Step 1: Implement the option:**
  - `SpoonAct4.TryModifyRestSiteOptions(player, options)` adds one `Sts1RubyRestSiteOption(player)` when the team lacks the Ruby;
  - `IsEnabled => !keys.Has(Ruby)`; `OnSelect` calls `Take(Ruby)` and **always returns true**, so peers that see two picks in a
    different order still agree (both takers lose their action);
  - icon: `RestSiteOption.IconPath` is private, so a postfix on its getter returns `KeySprites.Path(Ruby, large: true)` for
    this option; override `AssetPaths` to the same path;
  - `RubyRestSitePatch`: postfix on `NRestSiteRoom.OnAfterPlayerSelectedRestSiteOption` that disables the local player's
    Ruby button once the key is held (keep the button so option indexes stay stable). Solo needs nothing: the taker's
    options close.
- [ ] **Step 2: Text** (`rest_site_ui`): `OPTION_SPOON_RUBY.name` EN "Recall", FR "Rappel";
  `.description` EN "Take the [gold]Ruby Key[/gold] for the team. You give up resting or smithing here.",
  FR "Prenez la [gold]clé de rubis[/gold] pour l'équipe. Vous renoncez à vous reposer ou à forger ici."
  Match the markup the vanilla `rest_site_ui` entries use (grep the PCK).
- [ ] **Step 3: Run** `make build` and `make test` — Expected: 0/0, all pass.
- [ ] **Step 4: Checklist:** solo: Recall takes the Ruby, no heal/smith afterwards; next rest site has no Recall. Co-op
  (`make mp`): one player recalls, the other still rests; the other's Recall button greys out; both clicking together
  leaves both without a rest action and one key; save/reload keeps the key.
- [ ] **Step 5: Commit** `feat(act4): Ruby key rest site option`.

### Task 7: Emerald elite

**Files:**
- Create: `src/Core/EmeraldRule.cs`, `src/Act4/Sts1RegenerateMonsterPower.cs`, `src/Act4/EmeraldMapMarkerPatch.cs`
- Modify: `src/Config/Act4Modifier.cs`, `godot/SlaySpoon/localization/{eng,fra}/powers.json`
- Test: `tests/EmeraldRuleTests.cs`

**Interfaces:**
- Consumes: `Act4Keys.MarkEmerald/EmeraldNode/Take`, `SpoonAct4.Of`, `KeySprites.Path`, `Sts1MetallicizePower`.
- Produces:
  - `EmeraldRule.LastAct = 2`;
  - `EmeraldRule.Pick(IReadOnlyList<(int Col, int Row)> elites, int roll) → (int Col, int Row)?` (sorts by row, then col, takes `roll % count`; null when empty);
  - `enum EmeraldBuffKind { Strength, MaxHp, Metallicize, Regenerate }`;
  - `EmeraldRule.Buff(int roll, int actIndex) → (EmeraldBuffKind Kind, int Amount)` (roll 0-3; Amount for MaxHp is the percent, 25);
  - `EmeraldRule.MaxHpGain(int maxHp) → int` (25 %, rounded half up);
  - `EmeraldRule.MarkAct(Act4Keys keys, int actIndex, IReadOnlyList<(int, int)> elites, int roll) → bool` (stores a node
    unless the key is held, the act is past `LastAct`, or a node is already stored).

- [ ] **Step 1: Write the failing tests**

```csharp
[Theory] [InlineData(0, 0, EmeraldBuffKind.Strength, 2)] [InlineData(2, 0, EmeraldBuffKind.Metallicize, 4)] [InlineData(3, 0, EmeraldBuffKind.Regenerate, 3)]
[InlineData(0, 2, EmeraldBuffKind.Strength, 4)] [InlineData(2, 2, EmeraldBuffKind.Metallicize, 8)] [InlineData(3, 2, EmeraldBuffKind.Regenerate, 7)] [InlineData(1, 1, EmeraldBuffKind.MaxHp, 25)]
public void Buff_MatchesStS1(int roll, int act, EmeraldBuffKind kind, int amount) => Assert.Equal((kind, amount), EmeraldRule.Buff(roll, act));
[Theory] [InlineData(50, 13)] [InlineData(44, 11)] [InlineData(150, 38)] public void MaxHpGain_RoundsHalfUp(int hp, int gain) => Assert.Equal(gain, EmeraldRule.MaxHpGain(hp));
[Fact] public void Pick_IgnoresInputOrder() { var a = new[] { (4, 9), (1, 6), (2, 6) }; var b = new[] { (2, 6), (4, 9), (1, 6) }; Assert.Equal(EmeraldRule.Pick(a, 1), EmeraldRule.Pick(b, 1)); Assert.Equal((2, 6), EmeraldRule.Pick(a, 1)); }
[Fact] public void Pick_NoEliteNoNode() => Assert.Null(EmeraldRule.Pick(Array.Empty<(int, int)>(), 0));
[Fact] public void MarkAct_KeepsAnAlreadyStoredNode() { var k = new Act4Keys(); k.MarkEmerald(1, 3, 4); Assert.False(EmeraldRule.MarkAct(k, 1, new[] { (5, 5) }, 0)); Assert.Equal((3, 4), k.EmeraldNode(1)); }
[Fact] public void MarkAct_NothingOnceHeldOrInAct4() { var k = new Act4Keys(); Assert.False(EmeraldRule.MarkAct(k, 3, new[] { (1, 1) }, 0)); k.Take(Act4Key.Emerald); Assert.False(EmeraldRule.MarkAct(k, 0, new[] { (1, 1) }, 0)); }
```

- [ ] **Step 2: Run** `make test` — Expected: build error, `EmeraldRule` not defined.
- [ ] **Step 3: Implement `EmeraldRule`**; run `make test` — Expected: PASS.
- [ ] **Step 4: Game wiring on `SpoonAct4`:**
  - `AfterMapGenerated(map, actIndex)`: collect `map.GetAllMapPoints()` with `PointType == MapPointType.Elite` as
    `(coord.col, coord.row)`, roll with `new Rng(run.Rng.Seed, $"SlaySpoonEmerald{actIndex}").NextInt(4096)`, `MarkAct`, then `NotifyChanged()`;
  - `BeforeCombatStart()`: when the run's `CurrentMapCoord` equals `EmeraldNode(CurrentActIndex)` and the key is missing,
    roll `new Rng(run.Rng.Seed, $"SlaySpoonEmeraldBuff{actIndex}").NextInt(4)` and apply `Buff` to every enemy of the
    combat: `StrengthPower`, `CreatureCmd.GainMaxHp(enemy, MaxHpGain(enemy.MaxHp))` (it heals the gain),
    `Sts1MetallicizePower`, `Sts1RegenerateMonsterPower`. Use the combat state accessor the vanilla `BeforeCombatStart`
    relics use (grep `DelicateFrond`). HP comes after Monster HP and multiplayer scaling, so +25 % is of the scaled HP.
  - `AfterCombatVictory(room)`: same node test, then `Take(Emerald)`.
  - Builder call: the key goes to the team on victory, with no reward-screen item; the top bar shows it.
- [ ] **Step 5: `Sts1RegenerateMonsterPower`** (`IBorrowedIcon` → `RegenPower`): at the end of its owner's side turn, heal
  `Amount` unless the owner is dead or dying; never decrements. Text EN "Regenerate" / FR "Régénération", description
  "At the end of its turn, heals {Amount} HP." / "À la fin de son tour, récupère {Amount} PV." (copy the placeholder syntax
  of an existing StS1 power entry).
- [ ] **Step 6: Map marker** (`EmeraldMapMarkerPatch`): postfix on `NNormalMapPoint._Ready` that adds a small
  `key_green` `TextureRect` (mouse ignored) over the point whose `Point.coord` is the current act's Emerald node, removed
  when `Act4Keys.Changed` reports the key held.
- [ ] **Step 7: Run** `make build` and `make test` — Expected: 0/0, all pass.
- [ ] **Step 8: Checklist:** one elite per act carries the green key on the map, the same one on both co-op clients and
  after reload; the fight starts with one of the four buffs on every enemy (log line names it); winning lights the green
  key; skipping it marks a new elite next act; no mark in an act once the key is held; seeds otherwise unchanged (same
  map and first rewards as an unmodded run with the same seed).
- [ ] **Step 9: Commit** `feat(act4): Emerald key from a marked elite`.

### Task 8: Sapphire swap

**Files:**
- Create: `src/Core/SapphireRevert.cs`, `src/Act4/SapphireSwapAction.cs`, `src/Act4/SapphireChestPatch.cs`
- Modify: `godot/SlaySpoon/localization/{eng,fra}/` (a new key in the table the mod's other buttons use, e.g. `merchant_room.json`'s neighbour; create `treasure_room.json` if none fits)
- Test: `tests/SapphireRevertTests.cs`

**Interfaces:**
- Consumes: `Act4Keys.Take`, `SpoonAct4.Of`, the `NetLoanAction` / `LoanAction` pattern (`src/Loans/LoanAction.cs`).
- Produces:
  - `sealed record PickupSnapshot(IReadOnlyDictionary<int, string> Deck, int Gold, int MaxHp, int PotionCount)` — deck
    cards keyed by a handle the game layer assigns, valued by their serialized form;
  - `sealed record PickupRevertPlan(bool Reversible, IReadOnlyList<int> Restore, IReadOnlyList<int> Remove, int GoldToLose, int MaxHpToLose, int PotionsToDiscard)`;
  - `SapphireRevert.Plan(PickupSnapshot before, PickupSnapshot after) → PickupRevertPlan`;
  - `struct NetSapphireSwap : INetAction, IPacketSerializable { int RelicIndex; }` and `SapphireSwapAction : GameAction`.

- [ ] **Step 1: Write the failing tests**

```csharp
static PickupSnapshot S(int gold = 100, int hp = 70, int potions = 0, params (int, string)[] deck) => new(deck.ToDictionary(c => c.Item1, c => c.Item2), gold, hp, potions);
[Fact] public void Plan_UpgradedCardIsRestored() { var p = SapphireRevert.Plan(S(deck: new[] { (1, "Strike+0"), (2, "Bash+0") }), S(deck: new[] { (1, "Strike+1"), (2, "Bash+0") })); Assert.True(p.Reversible); Assert.Equal(new[] { 1 }, p.Restore); }
[Fact] public void Plan_AddedCardIsRemoved() { var p = SapphireRevert.Plan(S(deck: new[] { (1, "Strike+0") }), S(deck: new[] { (1, "Strike+0"), (7, "Feed+0") })); Assert.Equal(new[] { 7 }, p.Remove); }
[Fact] public void Plan_RemovedCardIsIrreversible() { Assert.False(SapphireRevert.Plan(S(deck: new[] { (1, "Strike+0") }), S()).Reversible); }
[Fact] public void Plan_GoldAndMaxHpGainsAreTakenBack() { var p = SapphireRevert.Plan(S(gold: 100, hp: 70), S(gold: 400, hp: 80)); Assert.Equal(300, p.GoldToLose); Assert.Equal(10, p.MaxHpToLose); }
[Fact] public void Plan_NothingChangedIsAPlainSwap() { var p = SapphireRevert.Plan(S(), S()); Assert.True(p.Reversible); Assert.Empty(p.Restore); Assert.Equal(0, p.GoldToLose); }
[Fact] public void Plan_NewPotionsAreDiscarded() { Assert.Equal(2, SapphireRevert.Plan(S(potions: 1), S(potions: 3)).PotionsToDiscard); }
```

- [ ] **Step 2: Run** `make test` — Expected: build error, `SapphireRevert` not defined.
- [ ] **Step 3: Implement `SapphireRevert.Plan`** (losses never go negative: gold or max HP that went down count as 0); run
  `make test` — Expected: PASS.
- [ ] **Step 4: Snapshot around chest pickups** (`SapphireChestPatch`):
  - while the treasure room awards relics (`TreasureRoomRelicSynchronizer.RelicsAwarded` fired, until the room is exited)
    and the run has `SpoonAct4` without the Sapphire, a prefix/postfix pair on `RelicCmd.Obtain(RelicModel, Player, int)`
    takes a `PickupSnapshot` of that player before and after (wrap the returned `Task`), using each deck card's save form
    (`ToSerializable()`, compared as JSON) and a per-card handle; keep `(player, relic, before, after, card objects)` per player;
  - this runs on every peer, since `AnimateRelicAwards` obtains the relics everywhere.
- [ ] **Step 5: The offer:** postfix on the treasure room node's `_Ready` (grep `NTreasureRoom`) adds a button, shown to the
  local player once their relic is obtained, when the team lacks the Sapphire and the plan is `Reversible`. Text EN
  "Swap for the Sapphire Key", FR "Échanger contre la clé de saphir"; tooltip EN "Give back {Relic} and undo what it did
  on pickup. The key counts for the whole team.", FR "Rendez {Relic} et annulez ce qu'elle a fait à sa prise. La clé
  compte pour toute l'équipe." It hides on `Act4Keys.Changed` when the key is held, and with the room.
- [ ] **Step 6: The action:** clicking enqueues `SapphireSwapAction(me, new NetSapphireSwap { RelicIndex = index of the relic in me.Relics })`
  through `RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue`. `ExecuteAction` on every peer: if `Take(Sapphire)`
  returns false, do nothing (the second taker keeps their relic); otherwise apply the stored plan for that player:
  replace each `Restore` card by `CardModel.FromSerializable(before)` at its deck position (find the deck-replace call
  `CardCmd.Transform` uses), remove each `Remove` card from the deck, `PlayerCmd.LoseGold(GoldToLose, player)`,
  `CreatureCmd.LoseMaxHp(..., MaxHpToLose)` (grep the exact name), discard `PotionsToDiscard` of the newest potions, then
  `RelicCmd.Remove(relic)`. Log the plan.
- [ ] **Step 7: Run** `make build` and `make test` — Expected: 0/0, all pass.
- [ ] **Step 8: Checklist:** solo chest: take a relic, the swap button appears; swapping removes it and lights the blue
  key; War Paint / Whetstone swapped: their upgraded cards come back unupgraded; Old Coin: the gold goes back; Pear /
  Strawberry: max HP goes back; a relic that removed or transformed cards shows no button. Co-op: both players see the
  button; both clicking together gives one key and only the first loses their relic; save after leaving the room keeps the key.
- [ ] **Step 9: Commit** `feat(act4): Sapphire key swap after a chest`.

### Task 9: Status and version

**Files:**
- Modify: `CLAUDE.md`, `docs/superpowers/plans/2026-10-29-step-6-act4.md`, `mod/SlaySpoon.json` (version), `docs/testing/step-6-checklist.md`

- [ ] **Step 1:** Bump `mod/SlaySpoon.json` from 0.7.0 to 0.8.0.
- [ ] **Step 2:** Run `make build` and `make test`; record the counts.
- [ ] **Step 3:** Update the Step 6 status in `CLAUDE.md` (WP3 implemented, preview key, counts, checklist pending) and mark
  WP3 implemented in the step 6 plan.
- [ ] **Step 4: Commit** `docs: step 6 WP3 status`.

## Builder calls (the owner can change them)

- In co-op, the host's permanent unlock decides whether a Standard run is an Act 4 run; each player earns their own unlock.
- The run flag is fixed at run start: an unlock earned mid-run applies from the next run.
- Until WP6, Standard access needs the `Act4Preview` config key; the unlock itself is recorded from now on.
- Two players taking the Ruby at the same time both spend their rest action.
- The Emerald key goes to the team on victory with no reward-screen item.
- The Sapphire swap is offered only while the players are still in the treasure room; reloading drops the offer.
- A relic whose pickup removed or transformed cards cannot be swapped (that can't be undone); everything else it did on
  pickup that the plan does not list (relic counters, enchantments) stays.

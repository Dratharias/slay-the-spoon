# Step 7 — The Watcher as a playable character — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** StS1's Watcher ("La Gardienne") is a sixth character, playable solo and in co-op in a Custom Run whose lobby ticks a StS1 card set modifier, with her whole StS1 kit: stances, Mantra, Scry, her cards, the cards they create, her 9 relics and 3 potions.

**Architecture:** `Sts1Watcher : CharacterModel` lives in `ModelDb` (reflection discovery) but never in the hardcoded `ModelDb.AllCharacters`, so Standard, Daily, random-character picks and vanilla progress counts stay vanilla; a postfix on `NCustomRunScreen.InitCharacterButtons` adds her button, shown only while StS1-only or Spire Souvenir is ticked. The StS1 mechanics StS2 lacks (stances, Mantra, Scry, last-card tracking) are player powers plus one command class, with every number in pure `src/Core/` code under xUnit. Visuals are static images cut from the StS1 jar by `art.py`; Spine animation comes later with the Act 4 animation work.

**Tech Stack:** C# / .NET 9, Harmony, Godot 4.5.1 (PCK), xUnit; Python 3.12 + pytest in the `slayspoon-tools` Podman image.

**Spec:** `docs/superpowers/specs/2026-10-28-slay-the-spoon-design.md` (§3.3, §6), amended by the owner request in `docs/backlog.md` (2026-10-29: the Watcher is playable when a StS1 card set mode is ticked in a Custom Run) and the owner answers below.

## Owner decisions (2026-10-29)

1. **Availability:** Custom Run only, when StS1-only or Spire Souvenir is ticked. Standard and Daily never show her.
2. **Visuals:** a static image first (combat body, corpse at death, a static stance eye). Animation later, with the same
   Spine tool as the Act 4 Heart.
3. **Content:** everything. Her cards, the 8 generated cards (Miracle, Insight, Smite, Safety, Through Violence,
   Expunger, Beta, Omega), the 9 relics (Pure Water as starter, Holy Water, Damaru, Teardrop Locket, Violet Lotus,
   Melange, Golden Eye, Cloak Clasp, Duality) and the 3 potions (Bottled Miracle, Stance Potion, Ambrosia), in her own pools.

## Recon

- **Card count:** `cards/purple` holds 77 classes, but StS1's `CardLibrary` registers 75 of them. **Discipline** and
  **Unraveling** are cut cards: they have text but never appear in StS1. They are left out, as the Defect's Impulse was
  (open owner question, see the end). The 75 include the four starter cards.
- **StS1 starter:** 4 Strike, 4 Defend, Eruption, Vigilance; Pure Water; 72 HP, 99 gold, 3 energy.
- **`ModelDb.AllCharacters` is a fixed array of five.** It drives the Daily character roll (`NDailyRunScreen`), random
  character resolution (`StartRunLobby.BeginRunLocally`), the save count and the Daily/Orobas epoch checks in
  `ProgressSaveManager.UpdateEpochsPostRun` (`CharacterStats.Count >= AllCharacters.Count()`). Never extend it.
- **Progress in a Custom Run:** `UpdateWithRunData` creates a `CharacterStats` entry for the run's character and adds
  playtime, but grants no ascension or win in Custom, and `TryObtainEpochPostRun` returns early because
  `GameMode.AreAchievementsAndEpochsLocked()` is true for Custom. The entry lands in the modded profile only.
  It still counts in `CharacterStats.Count` for later Standard runs, so the epoch count needs a guard.
- **Solo ascension clamp:** `StartRunLobby.BeginRunLocally` caps a singleplayer run's ascension to the character's
  `CharacterStats.MaxAscension`, whatever the mode. With no Standard wins, the Watcher's is always 0.
- **`NGameOverScreen.SaveBadgesToProgress`** indexes `CharacterStats[character.Id]`. The entry exists by then because
  `UpdateWithRunData` ran first; Task 1 verifies the order in game.
- **Character assets** are found by `Id.Entry.ToLowerInvariant()` at fixed `res://` paths (creature visuals, top panel
  icon, character select art, energy counter, merchant, rest site, multiplayer hands, map marker, transition material,
  card trail, FMOD events). A unique id (`sts1_watcher`) means our PCK can supply them at those paths without collisions.
  FMOD events cannot come from our PCK: they need patched getters or silence.
- **`NCreatureVisuals`** works without Spine (`HasSpineAnimation => SpineBody != null`). A static body needs `%Visuals`,
  `%Bounds`, `%IntentPos`, `%CenterPos`, and optionally `%TalkPos`.
- **StS2 has no stances, Mantra or Scry.** It has extra turns (`ShouldTakeExtraTurn` / `AfterTakingExtraTurn`, used by
  Pael's Eye), `VigorPower`, `BlurPower`, `EnergyNextTurnPower`, `DrawCardsNextTurnPower`, `PlatedArmorPower` and
  Retain, plus a hidden `TokenCardPool` and `TokenPotionPool` pattern for generated items.
- **StS1 jar assets:** `images/characters/watcher/` (Spine 3.4 idle atlas, `eye_anim/` with Calm/Wrath/Divinity,
  `corpse.png`, `shoulder.png`, `shoulder2.png`); `images/ui/charSelect/watcherButton.png` and `watcherPortrait.jpg`;
  `images/ui/topPanel/purple/` (energy orb layers); `images/relics/` and `images/largeRelics/` (relic icons);
  `localization/<lang>/stances.json`. StS1 potions are drawn from shared layers at runtime: no ready-made icon.
- **Local decompilation:** the CFR output in `~/Projects/sts-decomp/sts1/` holds the cards (`purple/`, `tempCards/`).
  Stances, the 9 relics, the 3 potions and `optionCards/` (Wish's and Stance Potion's choices) must be decompiled there
  first (CFR, local only, never committed).

## Global Constraints

- Target StS2 v0.107.1; mod id `SlaySpoon`; docs and comments in English; every new text in `eng` and `fra`.
- Classes prefixed `Sts1` (ModelDb id `STS1_*`); the character is `Sts1Watcher` (id `STS1_WATCHER`). Never rename after release.
- Decompiled code (Java or C#) never enters the repo, not even as comments.
- The mod never writes the regular (unmodded) save. Standard and Daily stay vanilla: the Watcher never appears there.
- With no StS1 card set modifier ticked, the Custom Run character list is exactly vanilla.
- Every random choice uses the game's synced RNGs (co-op peers must agree); candidate lists in deterministic order.
- Podman for every tool, heavy jobs under `nice -n 19`. No `make up` while `pgrep -x SlayTheSpire2` finds the game.
- Branch `step-7-watcher`, cut from the tip of `step-6-act4`. Commit per task. No push, no Workshop upload.

## Review Focus

1. **Unticking the modifier with the Watcher selected:** the selection falls back to the previous vanilla character on
   every peer, and a co-op client never starts a run as a character the host cannot see (Task 2, `WatcherAvailability` tests).
2. **Stance changes in a row** (Calm → Wrath → Calm, entering the stance already held, Divinity from Mantra during
   Wrath): energy from leaving Calm counts once per exit, re-entering the same stance triggers nothing (Task 4 tests).
3. **Scry with fewer cards than asked**, or an empty draw pile: shuffles nothing in, as in StS1, and still fires Nirvana and
   Weave (Task 5, `ScryMath.CardsToShow`).
4. **Save and reload mid-combat in a stance, with Mantra, and mid-run with growing cards** (Windmill Strike, Perseverance,
   Sands of Time, Lesson Learned): stance and Mantra are combat state; card growth that StS1 keeps on the combat copy
   stays there (checklist lines, Task 9).
5. **Solo ascension:** a solo Watcher run can start at the highest ascension the player has on any vanilla character
   (Task 3, `WatcherAscension.Cap`).

---

### Task 1: Character shell spike (static visuals, solo, Custom Run)

The riskiest unknown is whether StS2 accepts a sixth character outside `AllCharacters` from char select to game over.
This task proves it with the starter deck only and static art.

**Files:** create `src/Characters/Sts1Watcher.cs`, `src/Characters/Pools/Sts1WatcherCardPool.cs`,
`Sts1WatcherRelicPool.cs`, `Sts1WatcherPotionPool.cs`, `src/Characters/WatcherAssetPatch.cs` (FMOD fallbacks, any
asset path our PCK cannot serve); `godot/SlaySpoon/` scenes and images at the vanilla paths for `sts1_watcher`;
`tools/sts1/art.py` (`--watcher`: body from the idle atlas, corpse, eye, char select, energy layers, rest site);
`tools/sts1/tests/test_art.py`; `localization/{eng,fra}/characters.json`.

**Interfaces:**
- Produces: `Sts1Watcher` (HP 72, gold 99, `MaxEnergy` 3, starter deck 4 `Sts1StrikeWatcher`, 4 `Sts1DefendWatcher`,
  `Sts1Eruption`, `Sts1Vigilance`, relic `Sts1PureWater` as a placeholder relic with no effect until Task 6), the three pools.
- The four starter cards are written by hand here, in `src/Cards/Sts1/Watcher/` with portraits from `art.py`
  (codegen never overwrites a ported card). Eruption and Vigilance only deal damage / gain Block until Task 4 adds
  their stance.
- Strike and Defend carry `CardTag.Strike` / `CardTag.Defend` (Amalgamator, Perfected Strike-likes count them).
- Name: EN "The Watcher", FR from StS1's own French character text.

- [ ] **Step 1:** pytest `test_watcher_art_outputs` (art.py `--watcher` writes every listed file, PNG sizes match the
  vanilla Ironclad counterparts). Run: FAIL.
- [ ] **Step 2:** Implement the art cut: render the idle atlas's setup pose once to a PNG (pure Python compositing of
  the atlas regions at their slot transforms; no Spine runtime), corpse and eye frames as-is. Run: PASS.
- [ ] **Step 3:** Add the character, the pools and the scenes; a temporary console command `sts1watcher` swaps the solo
  player to her for the spike. `make build`: 0 warnings.
- [ ] **Step 4:** Spike in game with the owner (owner clicks): combat, map, rest site, merchant, death and game over,
  victory screen. Record in `docs/review/watcher-spike.md` every crash or missing asset and the fix chosen.
- [ ] **Step 5:** Commit `feat: Watcher character shell with static visuals`.

### Task 2: Custom Run registration and co-op

**Files:** create `src/Core/WatcherAvailability.cs`, `src/CustomRunUi/WatcherButtonPatch.cs`; test
`tests/WatcherAvailabilityTests.cs`.

**Interfaces:**
- Produces: `WatcherAvailability.IsAvailable(IEnumerable<string> modifierIds) -> bool` (true when
  `SpoonCardSetSts1Only` or `SpoonSpireSouvenirModifier` is among them);
  `WatcherAvailability.Fallback(string selected, string previous, bool available) -> string`.

- [ ] **Step 1:** Tests: `IsAvailable_WithSts1Only`, `IsAvailable_WithSouvenir`, `IsAvailable_FalseWithMonsterHpOnly`,
  `Fallback_ReturnsPreviousWhenUnavailable`, `Fallback_KeepsWatcherWhenAvailable`. Run: FAIL.
- [ ] **Step 2:** Implement. Run: PASS.
- [ ] **Step 3:** Postfix `NCustomRunScreen.InitCharacterButtons`: instantiate the vanilla char select button for
  `Sts1Watcher`, toggle its visibility on every modifier change (host and client), apply `Fallback` through the lobby's
  own character change so peers sync. Remove the Task 1 console command.
- [ ] **Step 4:** `make mp` check with the owner: host ticks StS1-only, client picks the Watcher, both reach a combat;
  host unticks, client falls back. Commit `feat: Watcher selectable in Custom Run with a StS1 card set`.

### Task 3: Progress guards

**Files:** create `src/Core/WatcherProgress.cs`, `src/Saves/WatcherProgressPatch.cs`; test `tests/WatcherProgressTests.cs`.

**Interfaces:**
- Produces: `WatcherProgress.VanillaStatsCount(IEnumerable<string> statIds, IEnumerable<string> vanillaIds) -> int`;
  `WatcherAscension.Cap(IEnumerable<int> vanillaMaxAscensions) -> int` (highest, 0 when empty).

- [ ] **Step 1:** Tests: `VanillaStatsCount_IgnoresWatcher` (5 vanilla + `STS1_WATCHER` → 5; 4 + Watcher → 4),
  `Cap_IsHighestVanilla`, `Cap_ZeroWhenNoStats`. Run: FAIL.
- [ ] **Step 2:** Implement. Run: PASS.
- [ ] **Step 3:** Transpiler on `ProgressSaveManager.UpdateEpochsPostRun` replaces the two `CharacterStats.Count` reads
  with `VanillaStatsCount`; postfix on `StartRunLobby.BeginRunLocally`'s singleplayer clamp (or a prefix that raises the
  Watcher's `MaxAscension` view) uses `WatcherAscension.Cap`. Never write the Watcher's stats into the regular profile.
- [ ] **Step 4:** `make build` 0 warnings; commit `fix: Watcher stats never count toward vanilla epochs`.

### Task 4: Stances and Mantra core

**Files:** create `src/Core/StanceMath.cs`, `src/Watcher/Stance.cs` (enum `Stance { Neutral, Calm, Wrath, Divinity }`),
`src/Watcher/StanceCmd.cs`, `src/Watcher/Powers/Sts1CalmStancePower.cs`, `Sts1WrathStancePower.cs`,
`Sts1DivinityStancePower.cs`, `Sts1MantraPower.cs`, `src/Watcher/IStanceListener.cs`, `src/Watcher/StanceEyePatch.cs`;
text in `powers.json` (stance names and tips from StS1's `stances.json`); test `tests/StanceMathTests.cs`.

**Interfaces:**
- Produces: `StanceMath.DealtMultiplier(Stance) -> decimal` (Wrath 2, Divinity 3, else 1),
  `StanceMath.ReceivedMultiplier(Stance) -> decimal` (Wrath 2, else 1), `StanceMath.EnergyOnExit(Stance from, bool violetLotus) -> int`
  (Calm 2, Calm + Violet Lotus 3), `StanceMath.EnergyOnEnter(Stance to) -> int` (Divinity 3),
  `StanceMath.AddMantra(int current, int gain) -> (int mantra, bool enterDivinity)` (threshold 10, remainder kept).
- `StanceCmd.Enter(Player, Stance, CardModel? source) -> Task`: no-op when already in that stance; removes the old stance
  power, adds the new one, grants energy, then calls `IStanceListener.AfterStanceChanged(Player, Stance from, Stance to)`
  on the player's powers, relics and cards in hand. `StanceCmd.Current(Player) -> Stance`.
- `StanceCmd.GainMantra(Player, int)`.
- Divinity leaves at the start of the holder's next turn. Wrath and Divinity multiply attack damage only (card attacks,
  not Thorns or power damage), through `ModifyDamageMultiplicative`, so intents show it. Received ×2 in Wrath applies
  to enemy attacks.

- [ ] **Step 1:** Tests: `Wrath_DoublesDealtAndReceived`, `Divinity_TriplesDealtOnly`, `CalmExit_GivesTwo`,
  `CalmExit_WithVioletLotus_GivesThree`, `NeutralExit_GivesNothing`, `Mantra_TenEntersDivinity`,
  `Mantra_TwelveKeepsTwo`, `Mantra_NineStays`. Run: FAIL.
- [ ] **Step 2:** Implement `StanceMath`. Run: PASS.
- [ ] **Step 3:** Implement the powers, `StanceCmd` and the static eye overlay on the Watcher's visuals. Console command
  `sts1stance <calm|wrath|divinity|neutral>` for testing.
- [ ] **Step 4:** `make build` 0 warnings; commit `feat: Watcher stances and Mantra`.

### Task 5: Scry, Retain helpers, last card played

**Files:** create `src/Core/ScryMath.cs`, `src/Watcher/ScryCmd.cs`, `src/Watcher/IScryListener.cs`,
`src/Watcher/LastCardPlayed.cs`; test `tests/ScryMathTests.cs`.

**Interfaces:**
- Produces: `ScryMath.CardsToShow(int amount, int drawPileCount, bool goldenEye) -> int` (amount + 2 with Golden Eye,
  capped by the pile, never a reshuffle); `ScryCmd.Scry(Player, int amount) -> Task` (selection of any number of the
  top cards to discard, then `IScryListener.AfterScry(Player, int discarded)` even when 0 were shown);
  `LastCardPlayed.Type(Player) -> CardType?` for this combat, read from the combat history.

- [ ] **Step 1:** Tests: `CardsToShow_CappedByPile`, `CardsToShow_EmptyPileIsZero`, `CardsToShow_GoldenEyeAddsTwo`. Run: FAIL.
- [ ] **Step 2:** Implement. Run: PASS.
- [ ] **Step 3:** Implement `ScryCmd` on StS2's card selection screen (prompt text in both languages) and
  `LastCardPlayed`. `make build` 0 warnings; commit `feat: Scry and last-card tracking`.

### Task 6: Data, generated cards, relics and potions

**Files:** `Makefile` (`sts1-data` += `purple:Watcher`; purple skips `match.py`, since the Watcher has no StS2 pool and
every purple card is her own); `tools/sts1/extract.py` (`--only` allowlist from StS1's `CardLibrary` for purple, and the
generated cards from `tempCards/` and `optionCards/`); `tools/sts1/art.py --relics` (the 9 icons from `images/relics/`);
`tools/sts1/tests/`; `src/Characters/Pools/Sts1WatcherTokenCardPool.cs`; `src/Relics/Watcher/` (9 relics);
`src/Potions/Watcher/` (3 potions, icons borrowed from vanilla potions through `IBorrowedIcon`); text in `relics.json`, `potions.json`.

Relic values from the StS1 classes (recorded in the commit message, never as code): Pure Water (Miracle at combat start),
Holy Water (3 Miracles, replaces Pure Water as boss relic), Damaru (1 Mantra per turn), Teardrop Locket (start combat in
Calm), Violet Lotus (+1 on leaving Calm), Melange (Scry 3 on shuffle), Golden Eye (Scry +2), Cloak Clasp (1 Block per card
in hand at turn end), Duality (Yang: +1 temporary Dexterity per attack played).

- [ ] **Step 1:** pytest `test_purple_follows_card_library` (75 cards, no Discipline, no Unraveling) and
  `test_watcher_generated_cards` (the 8 tokens plus Wish's and Stance Potion's options). Run: FAIL.
- [ ] **Step 2:** Implement and run `make sts1-data`. Expected: Ironclad, Silent, Defect and Colorless outputs
  byte-identical (`git status`); new `data/sts1/watcher.json`, portraits and skeletons.
- [ ] **Step 3:** Port the relics, potions and the 8 generated cards (hidden token pool, outside every reward source).
  Holy Water replaces Pure Water on pickup, as in StS1. `make build` 0 warnings.
- [ ] **Step 4:** Commit `feat: Watcher card data, generated cards, relics and potions`.

### Tasks 7–9: Port the Watcher's cards

As in 3a–3c: read the StS1 class, copy the closest StS2 pattern, typed vars named in both `cards.json`, hover tips for
stances, Mantra, Scry and Retain; new powers in `src/Cards/Sts1/Powers/` with a borrowed icon. Cards that grow while
retained (Perseverance, Windmill Strike, Sands of Time) keep the growth on the combat copy. Vault uses
`ShouldTakeExtraTurn` (in co-op only the player who played it gets the extra turn, as Pael's Eye does). Foreign Influence
draws from the attack pools of the current card set mode. `make build` 0 warnings; one checklist line per card in
`docs/testing/step-7-checklist.md`; commit `feat: port StS1 Watcher <batch>`.

- **Task 7 — attacks.**
- **Task 8 — skills.**
- **Task 9 — powers, then the gate:** `NoUnportedCards` passes over the Watcher folder; `make test` green.

### Task 10: Wrap-up

- [ ] **Step 1:** Checklist header and mode lines (StS1-only Watcher run; Spire Souvenir Watcher run; co-op Watcher +
  vanilla character; unticking falls back; game over and victory screens; solo ascension cap; save/reload in a stance).
  README, CLAUDE.md (status and code map), `docs/backlog.md` (request done), spec amendment noting the Watcher is in scope.
  Version: next minor in `mod/SlaySpoon.json`.
- [ ] **Step 2:** `make test` and `make build` green. Commit `docs: step 7 wrap-up`.

## Builder calls (for owner review)

- She never enters `AllCharacters`: no Standard, Daily or random-character Watcher.
- Spire Souvenir: she has no StS2 cards, so "Hybrid" for her is her StS1 pool plus Hybrid colorless.
- Solo ascension: capped by the highest ascension the player has on any vanilla character.
- Stats: her playtime goes into the modded profile; she never counts toward vanilla epochs.
- Merchant, multiplayer hands and map marker borrow vanilla images until animation work; her FMOD sounds are silent or borrowed.
- Potion icons are borrowed vanilla icons (StS1 potions have no single image to cut).
- Vault in co-op: an extra turn for the player who played it only.

## Open owner question

- Discipline and Unraveling exist in StS1's files but never in its game. Port them, or leave them out (current plan)?

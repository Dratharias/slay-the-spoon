# Slay the Spoon — notes for Claude

Slay the Spire 2 mod (StS2 Early Access v0.107.1, Godot 4.5.1 + C# .NET 9, Harmony) that brings
vanilla Slay the Spire 1 cards into StS2 and adds Custom Run options set by the lobby host.
Read `README.md` for the player view and the spec for every decision.

## Where we are (2026-10-30)

- **Step 1 is done and merged into `main`:**
  - mod skeleton;
  - Monster HP and Monster Damage modifiers (25–500 %) in Custom Run, synced in co-op and saved;
  - opt-in one-way progress sync into the modded profile;
  - the top bar shows each modifier's percent (red above 100 %, blue below).
- **Not yet tested in game** (the owner tests these when they feel like it): the edge cases left
  unticked in `docs/testing/step-1-checklist.md`.
- **Deferred minors:** listed in `docs/backlog.md`.
- **Step 2a is done and merged into `main`** (was branch `step-2`; plan `docs/superpowers/plans/2026-10-28-step-2a-*.md`):
  - StS1 tooling: extraction, duplicate table, art, codegen (`tools/sts1/`, `make sts1-data`);
  - card set modes (StS1-only, Hybrid) as Custom Run modifiers; Hybrid is now the "Spire Souvenir" modifier, which hands out the relic;
  - the 31 Ironclad cards StS2 dropped, ported by hand in `src/Cards/Sts1/Ironclad/`.
  - Not tested in game: everything in `docs/testing/step-2a-checklist.md`.
  - The Ironclad StS1 starter deck equals StS2's, so no starter change was needed, and Archaic
    Tooth / Amalgamator work as in vanilla.
- **Step 2b is done and merged into `main`** (plan `docs/superpowers/plans/2026-10-28-step-2b-*.md`):
  - Juggernaut and Rampage (StS1 version kept) and Rupture (duplicate) ported;
  - Hybrid: a duplicate is one slot; created cards come out as a random version; card rewards
    have a synced swap button (hidden reward alternatives, `src/CardSets/RewardSwapPatch.cs`).
  - Not tested in game: `docs/testing/step-2b-checklist.md`.
  - The owner still has to review `docs/review/ironclad-duplicates.md`.
- **Step 3a is done and merged into `main`** (was branch `step-3`; plan `docs/superpowers/plans/2026-10-29-step-3a-silent.md`, version 0.3.0):
  - tooling handles several characters (`make sts1-data` writes per-character data and review pages);
  - the Silent StS1 pool and the Spire Souvenir in the Silent relic pool;
  - 37 Silent cards in `src/Cards/Sts1/Silent/` (23 unique, 9 StS1 namesakes kept, 5 duplicates),
    plus Clash, Dual Wield, Entrench and Shockwave for the Ironclad (their StS2 versions are outside its pool).
  - Namesakes take StS2's French title (StS1's own French names collide: its Outmaneuver is "Tacticien").
  - Not tested in game: `docs/testing/step-3a-checklist.md`. The owner still has to review `docs/review/silent-duplicates.md`.
- **Step 3b is implemented and merged into `main`** (plan `docs/superpowers/plans/2026-10-29-step-3b-defect.md`, version 0.4.0):
  - 38 Defect cards (24 unique, 10 StS1 namesakes kept, 4 duplicates), the Defect pool and Spire Souvenir eligibility;
  - 12 attacks, 20 skills and 6 Powers, with EN/FR text and StS1 portraits;
  - Genetic Algorithm persists growth through `Sts1SavedProperties`; saved property names reuse vanilla IDs;
  - Electrodynamics changes Lightning targeting only for its holder; Lock-On scopes its marker per target;
  - build: 0 warnings / 0 errors; tests: 61 xUnit and 32 pytest pass, including `NoUnportedCards`;
  - not installed or tested in game: `docs/testing/step-3b-checklist.md` remains unchecked, especially save/reload and co-op;
  - the owner still has to review `docs/review/defect-duplicates.md` (classifications stay draft).
- **Step 3c is implemented and merged into `main`** (plan `docs/superpowers/plans/2026-10-29-step-3c-colorless.md`, version 0.5.0):
  - 39 StS1 colorless cards cataloged; 31 custom classes in `src/Cards/Sts1/Colorless/` (6 attacks, 22 skills,
    3 Powers) and 8 native cards reused; all 22 namesakes still draft for owner review (`docs/review/colorless-duplicates.md`);
  - the colorless pool follows the mode for all five characters; the Souvenir holder gets Hybrid colorless;
  - seven event-only classes (Apotheosis, Apparition, Bite, Enlightenment, JAX, Metamorphosis, Ritual Dagger)
    live in a hidden event pool, outside ordinary pools (32 ordinary cards); vanilla event grants are patched;
    Bite, JAX and Ritual Dagger have console access only;
  - Massive Scroll falls back to ordinary allowed cards when its co-op-only filter leaves none (builder default pending feedback);
  - Ritual Dagger reuses The Scythe's saved property names; Sadistic Nature reuses vanilla `SleightOfFleshPower`;
  - build: 0 warnings / 0 errors; tests: 83 xUnit and 35 pytest pass, including `NoUnportedCards`;
  - not installed or tested in game: `docs/testing/step-3c-checklist.md` remains unchecked.
- **Step 3d is implemented and merged into `main`** (plan `docs/superpowers/plans/2026-10-29-step-3d-sts1-events.md`, version 0.6.0):
  - StS1 events Vampires, Augmenter and The Nest (`src/Events/`) give Bite, J.A.X. and Ritual Dagger, plus the
    Mutagenic Strength relic (Augmenter only, borrowed Symbiotic Virus icon, `TemporaryStrengthPower`);
  - a `Hook.ModifyNextEvent` postfix replaces an act 2 event with an eligible unseen one 1 time in 3, on its own
    `Rng` (seeds stay vanilla); the rule lives in `src/Core/Sts1EventRule.cs`: StS1-only run or a Souvenir holder,
    Vampires needs every player to hold it, and players without it see the card option locked;
  - `sts1event <id>` console command; event portraits in `godot/SlaySpoon/images/events/` (`art.py --events`);
  - build: 0 warnings / 0 errors; tests: 100 xUnit and 36 pytest pass;
  - not installed or tested in game: `docs/testing/step-3d-checklist.md` remains unchecked.
- Steps 2 to 7 are merged into `main`. The 0.10.1 fix wave is also merged incrementally (owner, 2026-10-30).
  After step 3, look at a Steam Workshop "alpha" with the owner (no upload without their go-ahead).
  The Hybrid version switch also covers the merchant (local, price kept), choose-a-card screens (the synced
  index carries the version, `src/Core/VersionChoiceCode.cs`) and reward grids such as Sealed Deck (a synced
  "choose a version" step per duplicate kept); not tested in game (`docs/testing/step-2b-checklist.md`).
- **Steam Workshop:** `make workshop` stages `build/workshop/` (content, preview, `item.vdf`; visibility friends only);
  `make workshop-upload` runs SteamCMD in Podman and needs the owner's Steam login, so only the owner runs it, in a terminal.
  The first upload stores the item id in `workshop/published-file-id` (commit it). A local `mods/SlaySpoon` disables the Workshop copy.
- **No git remote yet.** The public repo `Dratharias/slay-the-spoon` on GitHub (MIT) gets created and
  pushed only when the owner says so. The same goes for the Steam Workshop upload.

## Step 5 status (2026-10-29)

- Merchant Loans is implemented and merged into `main` (0.7.0): ledger, synchronized requests/offers,
  reserved offer gold, automatic repayment, top-bar debt display, merchant ask popup and loan panel.
- Owner correction: debts survive death and co-op resurrection. No death hook clears them.
- English/French text includes the rule and the need to retry the purchase after accepting.
- Build: 0 warnings / 0 errors; tests: 154 xUnit and 42 pytest pass.
- In-game validation remains pending in `docs/testing/step-5-checklist.md`.
- Built artifacts are staged in `build/mods/SlaySpoon`; this update has not been installed.

## Step 6 status (2026-10-29)

- Act 4 source import started (merged into `main`); plan: `docs/superpowers/plans/2026-10-29-step-6-act4.md`.
- `make sts1-act4-data` stages 23 assets and selected EN/FR monster/power text in `data/sts1/act4/`.
- SHA-256 manifest and deterministic import tests; no Java/decompiled code committed.
- Mechanics audit recorded: `data/sts1/act4/mechanics.json` (hand-written, pinned to the jar's class SHA-256s,
  checked by `tools/sts1/tests/test_act4_mechanics.py`) and `docs/review/act4-mechanics.md`. The StS1 Act 4
  classes are decompiled in `~/Projects/sts-decomp/sts1/` (never commit them).
- Owner decisions: StS1 HP raises → StS2 A8, damage raises → A9, StS1 A18/A19 changes → A10; the Heart's
  Invincible cap scales with the same multiplayer factor as HP.
- StS2's `MultiplayerScalingModel.GetMultiplayerScaling` throws for act index 3: patch it before any act 4 co-op fight.
  Owner decision: act 4 HP keeps StS2's formula (HP × player count × factor) with act 3's factors (1.2 elite, 1.3 boss).
- Approved Act 4 unlock: first reach Act 3 in a Standard run at Ascension 10; victory is not required.
  The unlock is permanent and makes Act 4 available at every ascension level, including 0.
  A Custom Run option bypasses this prerequisite. The unlock and the keys are implemented in WP3 (see below).
- Co-op keys are team-wide (owner decision): one set for the team; a key any player collects counts for all.
  Ruby: one player gives up their rest-site action. Sapphire (0.12.0, replaces the 0.8.0 give-back swap): the
  rock-paper-scissors losers of a chest rematch for the relics left until the last player chooses between the last relic
  and the key (solo: relic or key); with the key held, chests are vanilla.
- WP3 is implemented (0.8.0): team key state, permanent unlock, hidden Standard run flag, key sprites/top bar,
  Ruby Recall rest option, Emerald elite marker/buffs, and Sapphire chest-relic swap (EN/FR).
  Build: 0 warnings / 0 errors; tests: 223 xUnit and 52 pytest pass. In-game checks remain pending.
- WP4-6 implemented (0.9.0, merged into `main`): Spire Shield and Spear elite, Corrupt Heart boss (static sprites rendered from
  the StS1 setup pose, `make sts1-ending-art`), The Ending act (StS1 backgrounds/music since 0.10.1; Glory map), fixed map rest → merchant → elite → Heart,
  the act is appended when the team holds all three keys (the act 3 boss then gives rewards), victory through the Architect,
  StS1 ending stinger.
- Released: Standard access after the unlock; Custom Run option "Act 4" (`SpoonAct4`) bypasses the unlock; Daily unchanged.
  Build: 0 warnings / 0 errors; tests: 285 xUnit and 64 pytest pass. In-game checks pending in `docs/testing/step-6-checklist.md`;
  design calls and minors in `docs/backlog.md`. Plan: `docs/superpowers/plans/2026-10-29-step-6-act4-wp4-6-ending.md`
  (WP3 plan: `docs/superpowers/plans/2026-10-29-step-6-act4-wp3-keys-unlock.md`).

## Step 7 status (2026-10-30)

- The Watcher is implemented and merged into `main` (0.10.0; plan `docs/superpowers/plans/2026-10-29-step-7-watcher.md`):
  a sixth character, playable in Custom Run only while a StS1 card set modifier (StS1-only or Spire Souvenir) is ticked.
  She never enters `ModelDb.AllCharacters`, so Standard, Daily, random picks and vanilla epoch counts stay vanilla.
- Done: static visuals (combat body, corpse, rest site, merchant, stance eye), stances (Wrath, Calm, Divinity) and Mantra,
  Scry, last-card tracking, 71 cards plus her 4 starter cards (25 attacks, 34 skills, 12 powers; Discipline and Unraveling are not
  ported), the 8 generated cards, her relics (Pure Water starter; Holy Water and Violet Lotus available through the Orobas fallback since 0.10.1; native Cloak Clasp reused), her 3 potions, EN/FR text and the progress guards.
- Owner decisions: static visuals first, animations and in-game validation deep backlog; she may obtain Archaic Tooth
  through the vanilla rule (no guard).
- Build: 0 warnings / 0 errors; tests: 366 xUnit and 74 pytest pass. In-game checks pending in
  `docs/testing/step-7-checklist.md`; design calls and minors in `docs/backlog.md`.

## Fix wave (2026-10-30)

- Version remains **0.10.1** in both `mod/SlaySpoon.json` and `src/ModEntry.cs`.
- Tasks 1–4 are committed and merged into `main`: keys/config fixes; Ending rewards, Smash, clock repair,
  French defeat text, StS1 backgrounds/music and menu/volume handling; Watcher fidelity fixes; Orobas fallback.
- StS1 combat and campfire backgrounds and Act 4/Heart music are shipped. Glory remains for the map;
  the shared StS2 merchant scene remains. No new cutscene or credits.
- At A10 only the second act 3 boss rewards when act 4 follows. Holy Water/Violet Lotus occupy the unavailable
  Tooth slot for the Watcher only, selected with the owner's event RNG; other sources stay excluded.
- Final review: source-level comparison with local StS1/StS2; 0 build warnings/errors; **408 xUnit + 74 pytest pass**.
  `make workshop` succeeds; PCK resource references, EN/FR tables, version and staged DLL/PCK/JSON were verified.
- Workshop package: `../slay-the-spoon-fixes/build/workshop/`, item `3829630403`. **Staged only, not uploaded or installed.**
  Publish from that worktree while the main working copy contains the separate uncommitted character-relic work;
  rebuilding in the main working copy would include its untracked C# files through the SDK glob.
- In-game validation remains deferred in `docs/testing/fix-wave-checklist.md`; no unchecked item is marked passed.
  Review notes: `docs/review/ending-fix-wave.md`, `watcher-fix-wave.md`, `orobas-fix-wave.md` and `fix-wave-final.md`.
- Remaining decisions, preview gaps and co-op exceptions stay in `docs/backlog.md`. Character relics are the next
  separate tranche (Champion Belt, Magic Flower, The Specimen); boss relics use a future Custom Run option and
  special-event relics remain imported without implementation.
- Owner instruction: merge each validated, Workshop-packageable task into `main` incrementally. No push/upload.

## 0.10.2 (2026-10-30)

- Owner-request quick fixes: StS1 power icons no longer show the game's "NOPE" missing image; bigger top-bar keys;
  loan panel on the right, − buttons, interest 0–900 %. Co-op Custom Run: allies' cards trigger Strangle/Choke. Build 0 warnings; 412 xUnit + 74 pytest pass. Not tested in game.

## 0.11.0 (2026-10-30)

- Emerald elite: StS1 map flames (green, `images/map/flame_*.png`, from `make sts1-act4-data`) plus the gem.
- Sapphire key shown in the opened chest (`SapphireKeyView`); replaced by the chest rematch in 0.12.0.
- Loan panel on `src/Ui/SpoonWidgets.cs` (controller rows/buttons); gold hover lists what is left to repay or collect.
- Character color (`src/Tint/`, pause menu button, `shaders/character_tint.gdshader`): overlay or dominant-hue mode,
  optional map lines, saved in the cfg (`CharacterTint`), shared in co-op by `TintAction`. Applied like StS2's monster
  hue variants (`SetNormalMaterial`).
- Build 0 warnings; 421 xUnit + 74 pytest pass. Not tested in game (gamepad, co-op sync and tint visuals included).

## 0.12.0 (2026-10-31)

- Sapphire chest rematch (owner request): `src/Core/ChestRematchRule.cs` (rounds, tested), `src/Act4/ChestRematchPatch.cs`
  (synchronizer prefixes; the key is the vote index past the last relic, sent as the vanilla `PickRelicAction`), and
  `src/Act4/ChestRematchView.cs` (round animations, rematch line, key beside the relics left; the final award is the
  vanilla animation queued after the rounds). The give-back swap (`SapphireChestPatch`, `SapphireRevert`,
  `PickupLedger`) is removed. Build 0 warnings; 398 xUnit + 74 pytest pass. Not tested in game:
  `docs/testing/sapphire-rematch-checklist.md`.

## Documents

| Path | What |
|------|------|
| `docs/superpowers/specs/2026-10-28-slay-the-spoon-design.md` | Approved design: owner decisions (§3), builder calls (§4), game recon (§5), architecture (§6), delivery order (§7) |
| `docs/superpowers/plans/2026-10-28-step-1-*.md` | Step 1 plan (done) |
| `docs/superpowers/plans/2026-10-28-step-2a-*.md`, `...-step-2b-*.md` | Step 2 plans (done) |
| `docs/superpowers/plans/2026-10-29-step-3c-colorless.md` | Step 3c plan (done) |
| `docs/testing/step-*-checklist.md` | In-game checklists the owner ticks |
| `docs/review/*-duplicates.md` | Owner review pages for Ironclad, Silent, Defect and Colorless (generated from `data/duplicates/*.json`) |
| `docs/backlog.md` | Known gaps, fixed when the owner feels like it |

## Owner rules

- The owner doesn't write C#. They make the architecture calls and play-test.
  - Ask them about design choices only.
  - Report in French. Everything written to the repo is in English.
- The mod ships in **English and French only**. Every text the mod adds goes in both
  `godot/SlaySpoon/localization/eng/` and `.../fra/`.
- The mod must **never write the regular (unmodded) save**. Progress sync only reads it.
- Standard and Daily modes remain available; never lock the modes themselves.
  Daily stays vanilla. Standard exceptions are Spire Souvenir and the approved Act 4 unlock
  documented above. Lobby activation options live in Custom Run.
- Bug reports are not support requests (see README). Don't promise fixes.
- Never commit decompiled game code.
- Commit on a step branch. Push and publish only on request.

## Build and run

`make build | test | up | down | restart | mp | clean | workshop | workshop-upload`. .NET runs in Podman
(`mcr.microsoft.com/dotnet/sdk:9.0`, `--security-opt label=disable` because of SELinux).
`tools/build-pck.sh` downloads Godot 4.5.1 into `~/.cache/slay-spoon` and exports the PCK headless; `tools/strip_pck.py` then drops the `project.binary` and script class cache Godot adds.

- **Game install:** `~/.local/share/Steam/steamapps/common/Slay the Spire 2`. The mod lives in
  `mods/SlaySpoon/` (`.json` + `.dll` + `.pck`).
- **Before `make up`:** close the game (`pgrep -x SlayTheSpire2`; kill it with `pkill -x`, never `pkill -f`).
- **After a session:** run `make down` so the owner's game runs unmodded.
- **Log:** `~/.local/share/SlayTheSpire2/logs/godot.log` (older runs: `godot<timestamp>.log`). Mod
  lines start with `[SlaySpoon]`. Each run logs a line
  `Embarking on a CUSTOM ... Modifiers: ...` listing its modifiers.
- **Modded profile:** with mods active the game uses `modded/profile1`, separate from the regular
  profile. The opt-in answer is stored in `~/.local/share/SlayTheSpire2/mod_configs/SlaySpoon.cfg`.
- **`make mp`:** two local instances, `--fastmp host_custom` and `--fastmp join --clientId 1001`.
  - Both write to the same `godot.log`.
  - A run started this way has dev player IDs 1/1001, so it resumes only under `make mp`. A normal
    Steam launch flags it corrupt and renames it.
- **No automated clicks:** synthetic xdotool clicks don't reach the game. Screenshots work
  (`import -window <id>`), but the owner does every click.
- **Tests:** they are xUnit, and they only cover the game-independent code in `src/Core/`, which
  `tests/SlaySpoon.Tests.csproj` links in. Put pure logic there so it can be tested. They also read
  repo files: every StS1 card has its text in both languages, a portrait, no `NotImplementedException`,
  and text placeholders that match its vars. `make test` also runs the pytest suites in `tools/sts1/tests` and `tools/tests`.
- **StS1 sources (local only):** the jar at `~/.local/share/Steam/steamapps/common/SlayTheSpire/desktop-1.0.jar`,
  CFR output in `~/Projects/sts-decomp/sts1/` (with `localization/{eng,fra}/cards.json`), and the
  StS2 card text dump in `~/Projects/sts-decomp/sts2-loc/` (`make sts2-loc`).

## Code map

| Folder | What |
|--------|------|
| `src/ModEntry.cs` | `[ModInitializer]`: self-check, Linux Harmony fix, `PatchAll` |
| `src/LinuxHarmonyFix.cs` | `dlopen("libgcc_s.so.1", RTLD_GLOBAL)`, without which every Harmony patch fails on Linux (`mm-exhelper.so: undefined symbol _Unwind_*`) |
| `src/Core/` | Pure logic: scaling maths, save paths, config parsing |
| `src/Config/` | `ModifierModel`s. One sealed class per percent (`SpoonMonsterHp150`); the class name *is* the value, so the game's own id sync and save carry it |
| `src/CustomRunUi/` | Extra Custom Run rows with − / + steppers, co-op sync, lobby init fixes |
| `src/Combat/` | Enemy HP scaling (`Creature.ScaleHpForMultiplayer` postfix + solo `CombatState.CreateCreature`); `EnemyEffectDamage` flags dealer-less enemy damage for Monster Damage |
| `src/Saves/` | Progress sync prefix on `ProgressSaveManager.LoadProgress`, first-launch popup, cfg file |
| `src/TopBar/` | Percent label instead of the missing modifier icon |
| `src/Config/CardSetModifiers.cs` | Card set modifiers: `SpoonCardSetSts1Only`, and `SpoonSpireSouvenirModifier`, which gives the Spire Souvenir at run start to every character with StS1 cards (Hybrid comes from the relic; mutually exclusive) |
| `src/CardSets/` | One postfix on `CardPoolModel.GetUnlockedCards` makes every card source (rewards, shop, transform, random generation) follow the mode; `Sts1Matches.g.cs` is generated. Hybrid duplicates: `VersionSwap`, `RandomVersionPatch` (CardFactory postfixes), `RewardSwapPatch`, `ChooseACardSwapPatch`, `GridVersionPatch`, `MerchantSwapPatch` (shared `SwapButton`, `SyncedChoice`). `CharacterCardsMerchantPatch` lets vanilla "<Character> Cards" add to a merchant offer holding StS1 cards |
| `src/Cards/` | `Sts1Card` / `Sts1IroncladCard` / `Sts1SilentCard` / `Sts1DefectCard` bases, the hidden per-character StS1 pools, one class per StS1 card (`Sts1HeavyBlade`), StS1 powers in `Sts1/Powers/` (icons borrowed from vanilla powers through `IBorrowedIcon`) |
| `src/Neow/` | Neow's Blessings: a transpiler makes Neow read a zero modifier count; after vanilla modifier options, the blessings replace the "done" page |
| `src/Relics/` | Spire Souvenir relic (Ironclad, Silent and Defect pool postfix, kept out of the run-start grab bag shuffle so seeds stay vanilla, borrowed Dusty Tome icon); its effect lives in `CardSetPools.ModeFor`. Mutagenic Strength, appended to the event relic pool |
| `src/Events/` | StS1 events (`Sts1Event` base, portrait path patch), `EventInjectionPatch` (act 2 replacement); text in `localization/<lang>/events.json` |
| `src/DevConsole/` | `sts1card` and `sts1event` console commands (the vanilla `card` and `event` commands can't see the hidden StS1 pools or events); `sts1stance`, `sts1scry` and `sts1watcheritem` for the Watcher (stance change, Scry, hidden relics and potions) |
| `src/Watcher/` | The Watcher's mechanics: stance powers and `StanceCmd` (Wrath, Calm, Divinity, energy on exit), Mantra, `ScryCmd`, last-card tracking, the static stance eye, her powers in `Powers/`; the numbers live in `src/Core/` (`StanceMath`, `ScryMath`) |
| `src/Cards/Sts1/Watcher/`, `WatcherTokens/` | One class per ported Watcher card, and the hidden generated cards (Miracle, Insight, Smite, Safety, Through Violence, Expunger, Beta, Omega) outside every reward source |
| `src/CustomRunUi/WatcherButtonPatch.cs` | Her Custom Run character button (shown only while a StS1 card set is ticked, fallback to the last vanilla pick when unticked, guarded against init failures) |
| `src/Saves/WatcherProgressPatch.cs` | Progress guards: she never counts toward vanilla epochs, solo ascension cap from the best vanilla character |
| `tools/sts1/` | Python (Podman image `slayspoon-tools`): `extract.py` → `data/sts1/`, `match.py` → `data/duplicates/` + review page, `art.py` → portraits, `codegen.py` → card skeletons + text. Codegen never overwrites a ported card |
| `godot/SlaySpoon/` | Packed into the PCK. Localization tables merge into the game's (`res://SlaySpoon/localization/<lang>/<table>.json`) |

## Game internals

- **Decompiled source:** `~/Projects/sts-decomp/sts2/` (ilspycmd output, local only). Grep it
  before guessing an API.
- **Model discovery:** `ModelDb` and the network message types find mod subclasses by reflection;
  abstract types are skipped.
  - Don't instantiate models outside `ModelDb`: it throws `DuplicateModelException`.
  - The lobby rejects players whose mods differ.
- **Damage:** `ModifyDamageMultiplicative` scales attacks and intents together.
- **Damage with no enemy dealer:** `MonsterDamageRule` also scales Status cards no player created (a `creator` on
  `AddGeneratedCardsToCombat` marks player-made ones) and Constrict/Galvanic (an `AsyncLocal` victim set around
  their async hook). Curses and the players' own powers stay vanilla (owner call, 2026-10-29).
- **Private members:** read them with `AccessTools.FieldRefAccess` / `MethodDelegate`.
  - `RunManager.State` is private.
  - The Custom Run screen is cached and reused between openings.
- **Startup order:** mod init runs before `ProgressSaveManager.LoadProgress`. Saves go through
  `ISaveStore` (Steam cloud).
- **Game text:** vanilla texts and keys can be grepped straight from `SlayTheSpire2.pck`
  (`grep -a -o '"KEY": *"[^"]*"'`). Use this to match the game's wording in English and French.

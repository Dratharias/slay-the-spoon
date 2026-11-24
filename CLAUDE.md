# Slay the Spoon — notes for Claude

Slay the Spire 2 mod (StS2 Early Access v0.111.1, Godot 4.5.1 + C# .NET 9, Harmony, BaseLib dependency) that brings
Slay the Spire 1 content into StS2 and adds Custom Run modes set by the lobby host. Version **1.11.6** (in both
`mod/SlaySpoon.json` and `src/ModEntry.cs`). `README.md` is the player view; `docs/superpowers/specs/` holds the
approved design of every feature.

## State of the project

- Every planned feature is implemented and on `main`; the build has 0 warnings; 1049 xUnit + 112 pytest pass.
- **Almost nothing is tested in game.** The owner ticks the checklists in `docs/testing/` when they feel like it; never
  mark an unchecked item as passed.
- Co-op is exercised by the bot tester on the `bot-tester` branch (never merged into `main`, never shipped); its
  scenario queue and results live in `docs/testing/bot-backlog.md` on that branch.
- `docs/backlog.md`: every known gap, minor and design call, each with a disposition. Open owner items: the Spoon art
  (placeholder Pixabay photos, drawing list in the backlog) and two design calls (King Midas revival at 1 gold,
  Deck of 10's 1-in-11 Neow odds).
- Public GitHub repo `Dratharias/slay-the-spoon` (MIT, remote `origin` only). Steam Workshop item `3829630403`, public.

## Owner rules

- The owner doesn't write C#. They make the design calls and play-test. Ask them about design choices only.
- Report in French. Everything written to the repo is in English.
- The mod ships in **English and French only**: every text goes in both `godot/SlaySpoon/localization/eng/` and `fra/`.
  Menu and Custom Run texts stay short; players discover the rest.
- Never write (or copy) the regular, unmodded save. The game copies it into the modded profile once by itself.
- Standard and Daily stay available; never lock a mode. Daily stays vanilla. Standard exceptions (owner-approved): the
  Spire Souvenir, the Act 4 unlock, the Watcher (solo and co-op), the Folie à deux and Slay the Spoon mode cards, and
  Neow's Deck of 10 option. Everything else is a Custom Run option.
- Bug reports are not support requests (see README). Don't promise fixes.
- Never commit decompiled game code.
- Bug fixes go to `main` as found: bump the patch version, build, test, `make workshop`, write a Steam changenote in
  `workshop/changelog-<version>.txt`. Features go on their own branch first. Push only when the owner asks; the Steam
  upload is always the owner's.

## Build and run

`make build | test | up | down | restart | mp | clean | workshop | workshop-upload`. .NET runs in Podman
(`mcr.microsoft.com/dotnet/sdk:9.0`, `--security-opt label=disable` because of SELinux). `tools/build-pck.sh` downloads
Godot 4.5.1 into `~/.cache/slay-spoon` and exports the PCK headless; `tools/strip_pck.py` drops the `project.binary` and
script class cache Godot adds.

- **Game install:** `~/.local/share/Steam/steamapps/common/Slay the Spire 2`; the mod goes in `mods/SlaySpoon/`
  (`.json` + `.dll` + `.pck`). A local `mods/SlaySpoon` disables the Workshop copy unless the Workshop version is greater.
- **Before `make up`:** close the game (`pgrep -x SlayTheSpire2`; kill with `pkill -x`, never `pkill -f`).
  **After a session:** `make down`, so the owner's game runs unmodded.
- **Log:** `~/.local/share/SlayTheSpire2/logs/godot.log` (older: `godot<timestamp>.log`). Mod lines start with
  `[SlaySpoon]`; each run logs `Embarking on a CUSTOM ... Modifiers: ...`.
- **Modded profile:** `modded/profile1`. The mod's cfg (`~/.local/share/SlayTheSpire2/mod_configs/SlaySpoon.cfg`) holds the
  Act 4 unlock and the character color.
- **`make mp`:** two local instances (`--fastmp host_custom`, `--fastmp join --clientId 1001`), one shared `godot.log`.
  Such runs have dev player IDs 1/1001 and resume only under `make mp`.
- **No automated clicks:** xdotool clicks don't reach the game. Screenshots work (`import -window <id>`).
- **Tests:** xUnit covers only the game-independent code in `src/Core/` (linked into `tests/SlaySpoon.Tests.csproj`): put
  pure logic there. Tests also read repo files (every StS1 card has EN/FR text, a portrait, no `NotImplementedException`,
  placeholders matching its vars). `make test` also runs pytest in `tools/sts1/tests` and `tools/tests`.
- **Local sources (never commit):** StS2 decompile in `~/Projects/sts-decomp/sts2-0.111.1/` (grep it before guessing an
  API); StS1 jar at `~/.local/share/Steam/steamapps/common/SlayTheSpire/desktop-1.0.jar`, its CFR output and EN/FR text
  in `~/Projects/sts-decomp/sts1/`; StS2 card text dump in `~/Projects/sts-decomp/sts2-loc/` (`make sts2-loc`). Vanilla
  texts can be grepped from `SlayTheSpire2.pck` (`grep -a -o '"KEY": *"[^"]*"'`) to match the game's wording.

## Steam Workshop

- `make workshop` stages `build/workshop/` (content, preview, `item.vdf`). The staged DLL and PCK must hold no bot code:
  `grep -a -c -i -E "AutoSlay|BotRun|SpoonBot"` prints 0 for each.
- `make workshop-upload` runs SteamCMD in Podman with the owner's Steam login: only the owner runs it, in a terminal.
  Each new version waits in Valve's moderation queue before subscribers see it (usually under a day; nothing to request).
- After an upload, `tools/workshop_dependency.py` (Steam client running, logged in as the owner) sets BaseLib
  (Workshop 3737335127, ≥ 3.4.7) as a required item, the tags (`workshop/tags`) and the French title and description
  (`workshop/description-fr.txt`; `description.txt` is English). Each description is at most 8000 UTF-8 bytes.
- Images: `tools/workshop_banners.py` → `workshop/images/` (banners, card and relic showcases, `preview.png`), shown
  through raw.githubusercontent.com: push them before the upload. Renders come from `src/DevConsole/CardRenderer.cs`
  (game launched with `SLAYSPOON_RENDER_DIR`, command in the script's docstring): cards (`Name+9`; a forged curse with
  defects: `BadLuck+9~Spiteful~Echo`), relics (`relic:Name`), the Forge climb GIF (`gif:Name`, in its own launch), elite
  portraits, the Conductor screen.

## Features

**StS1 content.** Card set modifiers: StS1-only, or Spire Souvenir (Hybrid: StS1 and StS2 cards; a duplicate pair is one
slot with a synced version switch on rewards, merchant, choose-a-card screens and grids; namesakes take StS2's French
title). Ported cards for the Ironclad, Silent, Defect and colorless pools; StS1 curses (Cursed Tome books,
Necronomicurse, Parasite, Pride); StS1 events injected in act 2 (`Sts1EventRule`); StS1 relics (character, boss, event);
the Ancient Potion. Boss relics come through the "StS1 Boss Relics" option (the act 2 and 3 Ancients offer a boss chest).
Duplicate tables in `data/duplicates/*.json`, owner pages in `docs/review/`.

**The Watcher.** A sixth character, an ordinary pick in every lobby but Daily; she always uses her StS1 pool. Stances,
Mantra, Scry, her cards, relics and potions, Spine animations, progress guards (she never counts toward vanilla epochs).

**Act 4 / The Ending.** Unlocked permanently once Ascension 5 is unlocked (or through the Custom Run option "Act 4");
team-wide keys (Ruby: a rest action; Emerald: a burning elite; Sapphire: a chest rock-paper-scissors rematch). Spire
Shield and Spear, Corrupt Heart, StS1 backgrounds and music, victory through the Architect. Act 4 HP uses StS2's formula
with act 3's factors.

**Custom Run modes** (rules in `src/Core/<Name>Rule.cs`):

| Mode | Code | In short |
|------|------|----------|
| Monster HP / Damage 25–500 % | `src/Config/`, `src/Combat/` | the class name carries the percent |
| Merchant Loans | `src/Loans/` | synced requests and offers; debts survive death |
| Campfire Gifts, campfire sales | `src/Campfire/` | give or sell a card at the fire |
| Starting Chest | `src/Ui/ChestOpeningOverlay.cs` | a Neow option: pick 1 relic of 10 |
| Folie à deux | `src/Folie/` | every card reward is bid on in a chest; also a Standard mode card |
| Matriochka | `src/Matriochka/` | Hoarder, a forced card every floor, 999-card cap, Dolly's Mirror |
| King Midas | `src/Midas/` | 11 gold relics; gold melts on travel; rising prices; relic sale; co-op revival |
| Spoon | `src/Spoon/` | max HP is the currency; the Spoon act and its boss; also a Standard mode card |
| The Forge | `src/Forge/` | fuse copies up to +9 (`data/forge/curves.json`), Duplicate, forged looks, level ladder |
| Mutation, Forced Trade | `src/Config/` | cards transform each act; co-op card trade |
| Deck of 10, Blood, Trophies, Conductor, Useless Relics | `src/DeckOfTen/`, `src/Blood/`, `src/Trophies/`, `src/Conductor/`, `src/UselessRelics/` | small challenge modes |
| Cursed Deck, Overcursed | `src/Curses/` | curses playable through the Unholy Grimoire, cursed relics; Overcursed triples curses and adds the Witching Hour |
| Forged curses | `src/Curses/Forged/` | Forge + Cursed Deck: curses fuse to +9, defects at +3/+6/+9 |
| Achilles' Heel | `src/Achilles/` | Block relics; any unblocked attack hit kills |
| Glass Cannon | `src/GlassCannon/` | Frail, Frenzy stacks and a Strength streak |

Mutually exclusive: King Midas × Matriochka, Deck of 10 × Hoarder, StS1-only × Spire Souvenir. The vanilla modifiers ×
mod modes table is in `docs/backlog.md`.

## Code map

| Path | What |
|------|------|
| `src/ModEntry.cs` | `[ModInitializer]`: self-check, Linux Harmony fix, `PatchEach` (class by class; logs every broken target before refusing to load) |
| `src/LinuxHarmonyFix.cs` | `dlopen("libgcc_s.so.1", RTLD_GLOBAL)`; without it every Harmony patch fails on Linux |
| `src/Core/` | Pure, tested logic for every feature |
| `src/Config/` | `ModifierModel`s, card set modifiers |
| `src/CustomRunUi/` | Extra Custom Run rows (− / + steppers), co-op sync, lobby fixes, the Watcher's button |
| `src/CardSets/` | One `CardPoolModel.GetUnlockedCards` postfix makes every card source follow the mode; Hybrid swaps (`SwapButton`, `SyncedChoice`); `OfferWeightPatch` |
| `src/Cards/` | `Sts1Card` bases, hidden StS1 pools, one class per card, StS1 powers (`IBorrowedIcon`), can't-play lines |
| `src/Relics/`, `src/Potions/`, `src/Events/`, `src/Neow/` | StS1 relics, potions and events, Spire Souvenir, boss chest, Neow's Blessings |
| `src/Act4/` | Keys, unlock, The Ending act and its monsters (`Sts1EndingMonster`), `IAppendedAct` (shared with the Spoon act), map sizing |
| `src/Watcher/`, `src/Characters/` | Watcher mechanics, stance effects, Architect dialogue, scene and trail aliases |
| `src/Spine/` | `Sts1Spine`: StS1 skeletons converted by `make sts1-spine`; every entry point returns null so callers keep a static sprite |
| `src/Saves/` | cfg file, progress guards, `DetourGuard`, `SerializationPatchGuard`, `ExtendedSaveNormalizer` |
| `src/Ui/`, `src/TopBar/`, `src/Tint/` | Mode cards, selection counter, shared widgets; top-bar labels and keys; character color |
| `src/DevConsole/` | `sts1card`, `sts1event`, `sts1stance`, `sts1scry`, `sts1watcheritem`; `CardRenderer` |
| `tools/` | `sts1/` (extraction, duplicates, art, codegen, Act 4 and Spine assets; codegen never overwrites a ported card), `forge_curves.py`, `curse_table.py`, `trophy_art.py`, Workshop scripts |
| `godot/SlaySpoon/` | Packed into the PCK; localization tables merge into the game's (`res://SlaySpoon/localization/<lang>/<table>.json`) |

## Game internals and pitfalls

- **Model discovery:** `ModelDb` and network message types find mod subclasses by reflection (abstract types skipped).
  Never instantiate a model outside `ModelDb` (`DuplicateModelException`). Mod ids are the class name in
  SCREAMING_SNAKE (`Sts1PureWater` → `STS1_PURE_WATER`). The lobby rejects players whose mods differ. The game registers
  mod `[SavedProperty]` names itself.
- **Harmony:** any method named `Prefix`, `Postfix`, `Transpiler`, `Finalizer` or `Prepare` in a patch class is applied
  as a patch: helpers need other names.
- **Lost detours:** the runtime recompiles hot methods (tiered compilation) without their Harmony detour, so a patch can
  stop running on one co-op peer only (desync). `DetourGuard` re-applies lost detours every frame, before every game
  action and before each checksum; inlined copies stay invisible to it, so `SerializationPatchGuard` checks key results.
  Prefer model hooks (combat hook listeners) over patches on small, inlinable methods. For a borrowed scene or icon path
  whose getter can revert, alias the vanilla path in Godot's resource cache (`WatcherTrailAlias`, `PowerIconAlias`).
- **Co-op sync:** every player choice goes through a synced action or `SyncedChoice`; randomness comes from the mod's
  own seeded `Rng` streams, never from vanilla's, so seeds and vanilla draws are unchanged.
- **Never open a player choice from `AfterRoomEntered`:** it runs while the screen is faded out (black screen). Use
  `AfterActEntered` or later, and nothing before `RunManager.Launch` sets `LocalContext.NetId`.
- **Start relics:** `RunManager.FinalizeStartingRelics` calls `AfterObtained` on every held relic on every new-run path;
  never apply pickup effects by hand as well.
- **No dead players outside combat:** vanilla waits forever for their vote.
- **Cards:** `NCard.UpdateVisuals` does nothing before the node is in the tree ("Broken Card"). Never read
  `CardModel.AncientTextBg` on a non-Ancient card: it throws inside the redraw and crashes the game.
- **Room counts:** `ActModel.GetNumberOfRooms` subtracts 1 in multiplayer; appended acts override it
  (`AppendedRoomCountPatch`). `MultiplayerScalingModel.GetMultiplayerScaling` throws for act index 3 (patched).
- **Damage:** `ModifyDamageMultiplicative` scales attacks and intents together; `MonsterDamageRule` also scales
  dealer-less enemy damage (unowned Status cards, Constrict, Galvanic). Curses and players' own powers stay vanilla.
- **Private members:** `AccessTools.FieldRefAccess` / `MethodDelegate` (`RunManager.State` is private; the Custom Run
  screen is cached and reused).
- **Startup:** mod init runs before `ProgressSaveManager.LoadProgress`; saves go through `ISaveStore` (Steam cloud).

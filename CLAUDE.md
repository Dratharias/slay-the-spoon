# Slay the Spoon — notes for Claude

Slay the Spire 2 mod (StS2 Early Access v0.107.1, Godot 4.5.1 + C# .NET 9, Harmony) that brings
vanilla Slay the Spire 1 cards into StS2 and adds Custom Run options set by the lobby host.
Read `README.md` for the player view and the spec for every decision.

## Where we are (2026-10-28)

- **Step 1 is done and merged into `main`:**
  - mod skeleton;
  - Monster HP and Monster Damage modifiers (25–500 %) in Custom Run, synced in co-op and saved;
  - opt-in one-way progress sync into the modded profile;
  - the top bar shows each modifier's percent (red above 100 %, blue below).
- **Not yet tested in game** (the owner tests these when they feel like it): the edge cases left
  unticked in `docs/testing/step-1-checklist.md`.
- **Deferred minors:** listed in `docs/backlog.md`.
- **Step 2a is done on branch `step-2`** (not merged; plan `docs/superpowers/plans/2026-10-28-step-2a-*.md`):
  - StS1 tooling: extraction, duplicate table, art, codegen (`tools/sts1/`, `make sts1-data`);
  - card set modes (StS1-only, Hybrid) as Custom Run modifiers;
  - the 31 Ironclad cards StS2 dropped, ported by hand in `src/Cards/Sts1/Ironclad/`.
  - Not tested in game: everything in `docs/testing/step-2a-checklist.md`.
  - The Ironclad StS1 starter deck equals StS2's, so no starter change was needed, and Archaic
    Tooth / Amalgamator work as in vanilla.
- **Step 2b is done on the same branch** (plan `docs/superpowers/plans/2026-10-28-step-2b-*.md`):
  - Juggernaut and Rampage (StS1 version kept) and Rupture (duplicate) ported;
  - Hybrid: a duplicate is one slot; created cards come out as a random version; card rewards
    have a synced swap button (hidden reward alternatives, `src/CardSets/RewardSwapPatch.cs`).
  - Not tested in game: `docs/testing/step-2b-checklist.md`.
  - The owner still has to review `docs/review/ironclad-duplicates.md`.
- **Next:** in-game tests by the owner, then merge `step-2` into `main` when they say so; then step 3
  (Silent, Defect, colorless; the swap at the merchant and on choose-a-card screens is in the backlog).
- **No git remote yet.** The public repo `Dratharias/slay-the-spoon` on GitHub (MIT) gets created and
  pushed only when the owner says so. The same goes for the Steam Workshop upload.

## Documents

| Path | What |
|------|------|
| `docs/superpowers/specs/2026-10-28-slay-the-spoon-design.md` | Approved design: owner decisions (§3), builder calls (§4), game recon (§5), architecture (§6), delivery order (§7) |
| `docs/superpowers/plans/2026-10-28-step-1-*.md` | Step 1 plan (done) |
| `docs/superpowers/plans/2026-10-28-step-2a-*.md`, `...-step-2b-*.md` | Step 2 plans (done) |
| `docs/testing/step-*-checklist.md` | In-game checklists the owner ticks |
| `docs/review/ironclad-duplicates.md` | Owner review page: every StS1 Ironclad card against StS2 (generated from `data/duplicates/ironclad.json`) |
| `docs/backlog.md` | Known gaps, fixed when the owner feels like it |

## Owner rules

- The owner doesn't write C#. They make the architecture calls and play-test.
  - Ask them about design choices only.
  - Report in French. Everything written to the repo is in English.
- The mod ships in **English and French only**. Every text the mod adds goes in both
  `godot/SlaySpoon/localization/eng/` and `.../fra/`.
- The mod must **never write the regular (unmodded) save**. Progress sync only reads it.
- Standard and Daily runs stay available and vanilla. A lock on them was tried and reverted.
  The mod's options live only in Custom Run.
- Bug reports are not support requests (see README). Don't promise fixes.
- Never commit decompiled game code.
- Commit on a step branch. Push and publish only on request.

## Build and run

`make build | test | up | down | restart | mp | clean`. .NET runs in Podman
(`mcr.microsoft.com/dotnet/sdk:9.0`, `--security-opt label=disable` because of SELinux).
`tools/build-pck.sh` downloads Godot 4.5.1 into `~/.cache/slay-spoon` and exports the PCK headless.

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
  and text placeholders that match its vars. `make test` also runs the `tools/sts1` pytest suite.
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
| `src/Combat/` | Enemy HP scaling (`Creature.ScaleHpForMultiplayer` postfix + solo `CombatState.CreateCreature`) |
| `src/Saves/` | Progress sync prefix on `ProgressSaveManager.LoadProgress`, first-launch popup, cfg file |
| `src/TopBar/` | Percent label instead of the missing modifier icon |
| `src/Config/CardSetModifiers.cs` | Card set modes: `SpoonCardSetSts1Only`, `SpoonCardSetHybrid` (mutually exclusive) |
| `src/CardSets/` | One postfix on `CardPoolModel.GetUnlockedCards` makes every card source (rewards, shop, transform, random generation) follow the mode; `Sts1Matches.g.cs` is generated. Hybrid duplicates: `VersionSwap`, `RandomVersionPatch` (CardFactory postfixes), `RewardSwapPatch` |
| `src/Cards/` | `Sts1Card` / `Sts1IroncladCard` bases, the hidden `Sts1IroncladCardPool`, one class per StS1 card (`Sts1HeavyBlade`), StS1 powers in `Sts1/Powers/` (icons borrowed from vanilla powers through `IBorrowedIcon`) |
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
- **Damage sources not scaled:** damage with no enemy dealer (Constrict, Burn…). It is in the backlog.
- **Private members:** read them with `AccessTools.FieldRefAccess` / `MethodDelegate`.
  - `RunManager.State` is private.
  - The Custom Run screen is cached and reused between openings.
- **Startup order:** mod init runs before `ProgressSaveManager.LoadProgress`. Saves go through
  `ISaveStore` (Steam cloud).
- **Game text:** vanilla texts and keys can be grepped straight from `SlayTheSpire2.pck`
  (`grep -a -o '"KEY": *"[^"]*"'`). Use this to match the game's wording in English and French.

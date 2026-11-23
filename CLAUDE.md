# Slay the Spoon — notes for Claude

Slay the Spire 2 mod (StS2 Early Access v0.111.1 since mod 1.3.8, Godot 4.5.1 + C# .NET 9, Harmony) that brings
vanilla Slay the Spire 1 cards into StS2 and adds Custom Run options set by the lobby host.
Read `README.md` for the player view and the spec for every decision.

## Where we are (2026-10-30)

- **Step 1 is done and merged into `main`:**
  - mod skeleton;
  - Monster HP and Monster Damage modifiers (25–500 %) in Custom Run, synced in co-op and saved;
  - opt-in one-way progress sync into the modded profile (removed in 1.3.8: the game copies unmodded saves itself);
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
  - `docs/review/ironclad-duplicates.md` approved by the owner (2026-11-05).
- **Step 3a is done and merged into `main`** (was branch `step-3`; plan `docs/superpowers/plans/2026-10-29-step-3a-silent.md`, version 0.3.0):
  - tooling handles several characters (`make sts1-data` writes per-character data and review pages);
  - the Silent StS1 pool and the Spire Souvenir in the Silent relic pool;
  - 37 Silent cards in `src/Cards/Sts1/Silent/` (23 unique, 9 StS1 namesakes kept, 5 duplicates),
    plus Clash, Dual Wield, Entrench and Shockwave for the Ironclad (their StS2 versions are outside its pool).
  - Namesakes take StS2's French title (StS1's own French names collide: its Outmaneuver is "Tacticien").
  - Not tested in game: `docs/testing/step-3a-checklist.md`. `docs/review/silent-duplicates.md` approved (2026-11-05).
- **Step 3b is implemented and merged into `main`** (plan `docs/superpowers/plans/2026-10-29-step-3b-defect.md`, version 0.4.0):
  - 38 Defect cards (24 unique, 10 StS1 namesakes kept, 4 duplicates), the Defect pool and Spire Souvenir eligibility;
  - 12 attacks, 20 skills and 6 Powers, with EN/FR text and StS1 portraits;
  - Genetic Algorithm persists growth through `Sts1SavedProperties`; saved property names reuse vanilla IDs;
  - Electrodynamics changes Lightning targeting only for its holder; Lock-On scopes its marker per target;
  - build: 0 warnings / 0 errors; tests: 61 xUnit and 32 pytest pass, including `NoUnportedCards`;
  - not installed or tested in game: `docs/testing/step-3b-checklist.md` remains unchecked, especially save/reload and co-op;
  - `docs/review/defect-duplicates.md` approved by the owner (2026-11-05);
- **Step 3c is implemented and merged into `main`** (plan `docs/superpowers/plans/2026-10-29-step-3c-colorless.md`, version 0.5.0):
  - 39 StS1 colorless cards cataloged; 31 custom classes in `src/Cards/Sts1/Colorless/` (6 attacks, 22 skills,
    3 Powers) and 8 native cards reused; namesakes approved 2026-11-05 (`docs/review/colorless-duplicates.md`): Discovery, Jack of All Trades,
    Secret Technique and Secret Weapon are `Retired` (the StS2 card in every mode; classes kept for old saves);
  - the colorless pool follows the mode for all five characters; the Souvenir holder gets Hybrid colorless;
  - seven event-only classes (Apotheosis, Apparition, Bite, Enlightenment, JAX, Metamorphosis, Ritual Dagger)
    live in a hidden event pool, outside ordinary pools (32 ordinary cards); vanilla event grants are patched;
    Bite, JAX and Ritual Dagger have console access only;
  - Massive Scroll falls back to ordinary allowed cards when its co-op-only filter leaves none (owner-approved 2026-11-05);
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
- **Steam Workshop:** `make workshop` stages `build/workshop/` (content, preview, `item.vdf`; visibility public since 1.7.0, owner 2026-11-18);
  `make workshop-upload` runs SteamCMD in Podman and needs the owner's Steam login, so only the owner runs it, in a terminal.
  The first upload stores the item id in `workshop/published-file-id` (commit it). A local `mods/SlaySpoon` disables the Workshop copy unless the Workshop version is greater (game 0.111.1).
  BaseLib (Workshop 3737335127, ≥ 3.4.7) is a manifest dependency (`mod/SlaySpoon.json`) and a Steam required item:
  after an upload, `tools/workshop_dependency.py` maps `workshop/required-items` through the game's `libsteam_api.so`
  (Steam client running, logged in as the owner; fallback: "Add/Remove Required Items" on the item page).
  The same script sets the Workshop tags (`workshop/tags`: categories plus English and French) and the French title and
  description (`workshop/description-fr.txt`, Steam's per-language text; `description.txt` is English only, 8000 characters
  max each). Images: `tools/workshop_banners.py` → `workshop/images/` (slim banners per party mode and section, card and relic showcases, the 512×512
  `preview.png` thumbnail), shown through raw.githubusercontent.com, so push them before the upload. Showcases and preview
  need card renders: `src/DevConsole/CardRenderer.cs` renders cards (`Name+9`: Forge level) and relics (`relic:Name`: icon and tooltip) to PNG in EN/FR at the main menu, then quits, when the
  game is launched with `SLAYSPOON_RENDER_DIR` (command in the script's docstring); `--cards DIR` reads them.
- **1.0.4 (2026-11-12)** is on `main`: merchant trade windows, campfire card sales, Act 4 by default once
  Ascension 5 is unlocked (`docs/testing/release-1.0.4.md`).
- **1.0.5 (2026-11-12)** is on `main`: code-review fixes for loans and campfire sales (Escape only closes, Withdraw
  cancels a request; Accept rejected only when a picked offer changed; sales always end and explain failures).
  Not tested in game: "Review fixes" in `docs/testing/release-1.0.4.md`. **Public GitHub repo** `Dratharias/slay-the-spoon` (MIT, remote `origin`, created 2026-11-18). Push only when the
  owner asks; the Steam Workshop upload is the owner's too.

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
  rock-paper-scissors losers of a chest rematch for the relics left; since 0.12.8 the key is one more choice from the
  first round (key voters fight for it too; solo: relic or key); with the key held, chests are vanilla.
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
- Workshop item `3829630403`. All step/fix worktrees and branches were merged into `main` and removed (2026-11-12).
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

## 0.12.1 (2026-10-31)

- Sapphire Key shown from the chest's opening as one more slot (N players: N relics + the key; scene slots from
  `treasure_room.tscn`, a fifth slot for 4 relics), dimmed until it is offered to the last player; solo: a choice from
  the start. Build 0 warnings; 398 xUnit + 74 pytest pass. Not tested in game.

## Backlog wave 0.12.6–0.12.38 (2026-11-05, branch `backlog-wave`, worktree `../slay-the-spoon-wave`)

- Block Return icon; Ending fight backgrounds and heal; Sapphire Key selectable from the first chest round (several key
  voters fight for it); rematch losers point again; StS1 Ancient Potion (`src/Potions/Sts1/`, StS1-only runs, every
  character); Watcher head icon, stance sounds (`audio/watcher/`), screen-edge flash and stance aura
  (`src/Watcher/StanceFx.cs`), generic Stance tip; small fixes listed in `docs/backlog.md`.
- Cleanups: unused usings, the `Act4Preview` key and `CustomRunOptions` removed; Invincible clamp in
  `HeartInvincibleRule`; Ending rooms seeded from the run seed on both paths; `ending_art.py` honors trimmed regions.
- 0.12.25: Watcher game-over stats guard, Spirit Shield in-combat Block preview, 1 s death length for Ending monsters.
- 0.12.26: French act 4 title "La Fin"; four colorless namesakes retired (StS2 card in every mode). Remaining design calls settled with the owner (2026-11-05 Q&A), listed in
  `docs/backlog.md` under "Design calls settled"; combo pairs in `docs/review/hybrid-combos.md`.
- 0.12.27: "Selected: X / Y" counter on every multi-pick card grid (`src/Ui/SelectionCounterPatch.cs`).
- 0.12.28: the Architect talks to the Watcher (`src/Watcher/ArchitectDialoguePatch.cs`, `ancients.json`; owner proofreads).
- 0.12.29: weighted card draws (`src/Core/OfferWeights.cs`, `src/CardSets/OfferWeightPatch.cs`): Souvenir lean 60/40 on the
  seed and the combo pairs ×0.5; one draw on the same RNG, vanilla draw when weights are equal.
- 0.12.30: StS1 events Golden Idol, Forgotten Altar and Moai Head (act 2 injection, `Sts1EventRule`), with the Golden
  Idol and Bloody Idol relics (`src/Relics/Events/`, event relic pool postfix); art from `tools/sts1/event_art.py`.
- 0.12.31: StS1 Cursed Tome with its three books (Necronomicon, Enchiridion, Nilry's Codex) and the Necronomicurse
  (`src/Cards/Sts1/Curses/`, hidden `Sts1CurseCardPool`).
- 0.12.32: StS1 Ominous Forge (Warped Tongs, Pain curse) and Bonfire Spirits (Spirit Poop; reward by offered card rarity).
- 0.12.33: StS1 Mind Bloom (shared event: act 1 boss fight, 999 gold + 2 Normality, or upgrade all + Mark of the Bloom).
- 0.12.34: Campfire Gifts Custom Run modifier (`SpoonCampfireGift`, `src/Campfire/`): co-op rest option "Gift" spends the
  giver's action, moves a deck card (no curses) to an ally picked with Mend's targeting, the ally pays 50 gold; a trade
  is two gifts at the same fire. Not tested in game, co-op sync included.
- 0.12.35: Starting Chest Custom Run modifier (`SpoonStartingChest`): a Neow option (like Draft's) plays the act's
  Spine chest opening (`src/Ui/ChestOpeningOverlay.cs`), then the vanilla synced "Choose a Relic" screen with 10 relics
  read from the player's grab bag (only the kept one leaves it). Not tested in game (overlay placement included).
- 0.12.36: the owner's character-relic and Watcher-selection work (committed as found, branch `character-relics`) is
  finished: Champion Belt, Magic Flower (Ironclad) and The Specimen (Silent) are Rare pool relics in Custom Runs with a
  StS1 card set; 19 StS1 relics ported in `src/Relics/Sts1/` (3 character, 16 boss: the 8 character boss relics plus
  Busted Crown, Coffee Dripper, Cursed Key, Fusion Hammer, Runic Dome, Sacred Bark, Slaver's Collar, Tiny House), art in
  `images/relics/sts1/`, EN/FR text, `tests/Sts1RelicTests.cs`. Sacred Bark scales the holder's potion vars in the
  `PotionModel.DynamicVars` getter; Runic Dome hides intents on the holder's screen only. The Watcher is selectable in
  solo Standard and every Custom Run (`WatcherRunRule`: a solo Watcher run uses StS1 cards), an owner-approved
  Standard exception.
- 0.12.37: Custom Run option "StS1 Boss Relics" (`SpoonBossRelics`): the act 2 and act 3 Ancients offer a boss chest of
  3 (`src/Relics/BossChest.cs`, rule `src/Core/BossChestRule.cs`): Darv's 11 natives with Darv's rules, the 8 shared
  ports and the takers' character boss relics (only when every taker plays that character). Co-op takers share the
  chest once every player has made their first choice; ties go to the vanilla rock-paper-scissors fight, losers get the
  unvoted relics. `Sts1InactiveBossRelic` is now `Sts1BossRelic`.
- Build 0 warnings; 553 xUnit + 78 pytest pass. Not tested in game.
- 0.12.38 (backlog close): StS1 can't-play lines (`src/Cards/CantPlayMessagePatch.cs`: the card `NCardPlay.CancelPlayCard` refused, `<ID>.cantPlay` in `cards.json`,
  opt-in through `Sts1Card.HasCantPlayMessage`); Lesson Learned's end-of-combat upgrade preview; Watcher orb energy icon
  (`OrbEnergyPrefixPatch`); Strangle/Choke co-op tooltip (`StrangleCoopTextPatch`). Every other backlog minor now carries
  a disposition (done, checked, kept with a reason, or owner item). Build 0 warnings; 553 xUnit + 78 pytest pass.
  Not tested in game: `docs/testing/backlog-close-checklist.md`.
- **1.0.0 (2026-11-05, owner instruction "backlog empty → 1.0.0"):** version bump only, same code as 0.12.38.
  `make workshop` staged `build/workshop/`; not uploaded, not installed. What remains is owner work: the in-game
  checklists in `docs/testing/`, the Spine animations, the client log for the co-op crash report, text proofreading.
- **1.0.1 (2026-11-05):** BaseLib is a manifest and Workshop dependency; Spine animations for the Ending monsters and the
  Watcher (`src/Spine/`, `make sts1-spine`); load fix: Harmony applies any method named `Prefix`, `Postfix`,
  `Transpiler`, `Finalizer` or `Prepare` in a patch class as a patch, so helpers need other names; the Watcher's Random
  transpiler also accepts BaseLib's `GetRandomEligibleCharacters` in place of `ModelDb.AllCharacters`. Tint at the rest
  site, merchant and map ping; Heart status shuffles on their own RNG; Shield and Spear out of the 15-elite epoch;
  `InvincibleBudget` and `WatcherAscension.LimitFor` in Core with tests. The 1.0.0 Workshop staging had the load bug:
  never upload it. Build 0 warnings; 560 xUnit + 91 pytest pass. In-game checks: `docs/testing/spine-checklist.md`.

## 1.1.0 — two modes (2026-11-14, branch `two-modes`)

- **Folie à deux** (`SpoonFolieADeux`, spec `docs/superpowers/specs/2026-11-14-folie-a-deux-design.md`): a fourth
  multiplayer host card (Madness art) starts a Standard run with the hidden flag (new Standard exception); Custom Run
  checkbox. Every card reward is bid on in a chest (`src/Folie/`): `FolieChest` (bids as the mod's own
  `FolieBidMessage`, keyed by a chest name the same on every client; fights through vanilla
  `RelicPickingResult.GenerateRelicFight` on a key-seeded `Rng`), `FolieChestOverlay` (view), `FolieRewards` (combat,
  treasure, Matriochka floors), `FolieCustomRewards` (`RewardsCmd.OfferCustom`), `FolieNeow` + `FolieRendezvous` (Draft
  rounds and Sealed Deck with consolation), `FolieCardPicks` (out-of-combat picks from events and relics: the owner picks,
  then a one-head chest; `FromSimpleGridForRewards` / `FromChooseACardScreen` postfixes at `Priority.Last`). Rules in `src/Core/FolieRule.cs`.
- **Matriochka** (`SpoonMatriochka`, spec `docs/superpowers/specs/2026-11-14-matriochka-design.md`): Hoarder,
  unskippable card reward every floor, Dolly's Mirror on the act 3 Reflections row (1.8.2), Doll Room (Bing Bong), act 3 Reflections
  row (Shatter); rerolls per act from the row's −/+ (0–10, default 3). Rules in `src/Core/MatriochkaRule.cs`.
- Build 0 warnings; 594 xUnit + 91 pytest pass. Not tested in game: `docs/testing/two-modes-checklist.md`.

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
- The mod must **never write the regular (unmodded) save**. Since 1.3.8 the mod does not copy it either; the game does that once.
- Standard and Daily modes remain available; never lock the modes themselves.
  Daily stays vanilla. Standard exceptions are Spire Souvenir, the approved Act 4 unlock, the Watcher in Standard (solo since 0.12.36, co-op since 1.8.2)
  the Folie à deux / Slay the Spoon mode cards and the Deck of 10 Neow option (1.5.1), documented above. Lobby activation options live in Custom Run.
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
  profile; the game copies the regular saves into it on the first modded launch. The mod's cfg (`~/.local/share/SlayTheSpire2/mod_configs/SlaySpoon.cfg`) holds the Act 4 unlock and the character color.
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
| `src/Saves/` | cfg file (Act 4 unlock, character color), Watcher progress guards |
| `src/TopBar/` | Percent label instead of the missing modifier icon |
| `src/Config/CardSetModifiers.cs` | Card set modifiers: `SpoonCardSetSts1Only`, and `SpoonSpireSouvenirModifier`, which gives the Spire Souvenir at run start to every character with StS1 cards (Hybrid comes from the relic; mutually exclusive) |
| `src/CardSets/` | One postfix on `CardPoolModel.GetUnlockedCards` makes every card source (rewards, shop, transform, random generation) follow the mode; `Sts1Matches.g.cs` is generated. Hybrid duplicates: `VersionSwap`, `RandomVersionPatch` (CardFactory postfixes), `RewardSwapPatch`, `ChooseACardSwapPatch`, `GridVersionPatch`, `MerchantSwapPatch` (shared `SwapButton`, `SyncedChoice`). `CharacterCardsMerchantPatch` lets vanilla "<Character> Cards" add to a merchant offer holding StS1 cards |
| `src/Cards/` | `Sts1Card` / `Sts1IroncladCard` / `Sts1SilentCard` / `Sts1DefectCard` bases, the hidden per-character StS1 pools, one class per StS1 card (`Sts1HeavyBlade`), StS1 powers in `Sts1/Powers/` (icons borrowed from vanilla powers through `IBorrowedIcon`) |
| `src/Neow/` | Neow's Blessings: a transpiler makes Neow read a zero modifier count; after vanilla modifier options, the blessings replace the "done" page |
| `src/Relics/` | Spire Souvenir relic (Ironclad, Silent and Defect pool postfix, kept out of the run-start grab bag shuffle so seeds stay vanilla, borrowed Dusty Tome icon); its effect lives in `CardSetPools.ModeFor`. Mutagenic Strength, appended to the event relic pool |
| `src/Events/` | StS1 events (`Sts1Event` base, portrait path patch), `EventInjectionPatch` (act 2 replacement); text in `localization/<lang>/events.json` |
| `src/DevConsole/` | `sts1card` and `sts1event` console commands (the vanilla `card` and `event` commands can't see the hidden StS1 pools or events); `sts1stance`, `sts1scry` and `sts1watcheritem` for the Watcher (stance change, Scry, hidden relics and potions) |
| `src/Spine/` | `Sts1Spine`: loads the StS1 skeletons that `make sts1-spine` converts to Spine 4.2 (`tools/sts1/spine34to42.py`, `spine_assets.py` → `godot/SlaySpoon/spine/<key>/`) through ClassDB, builds a `CreatureAnimator` limited to the animations each skeleton has, reads bone positions; every entry point returns null so callers keep their static sprite |
| `src/Watcher/` | The Watcher's mechanics: stance powers and `StanceCmd` (Wrath, Calm, Divinity, energy on exit), Mantra, `ScryCmd`, last-card tracking, the static stance eye, her powers in `Powers/`; the numbers live in `src/Core/` (`StanceMath`, `ScryMath`) |
| `src/Cards/Sts1/Watcher/`, `WatcherTokens/` | One class per ported Watcher card, and the hidden generated cards (Miracle, Insight, Smite, Safety, Through Violence, Expunger, Beta, Omega) outside every reward source |
| `src/CustomRunUi/WatcherButtonPatch.cs` | Her Custom Run character button (shown only while a StS1 card set is ticked, fallback to the last vanilla pick when unticked, guarded against init failures) |
| `src/Saves/WatcherVanillaGuardsPatch.cs` | Progress guards: she never counts toward vanilla epochs, solo ascension cap from the best vanilla character |
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

## 1.2.0 — King Midas (2026-11-14, branch `two-modes`)

- **King Midas** (`SpoonRoiMidas`, Custom Run only, spec `docs/superpowers/specs/2026-11-14-roi-midas-design.md`):
  11 gold relics at run start (pickup gold and Greed applied by hand in `AfterRunCreated`, removed from every grab bag);
  card rewards removed in `TryModifyRewardsLate` and out-of-combat picks blocked (`src/Midas/MidasCardPicks.cs`,
  `Priority.First` prefixes; Neow kept); "?" events drawn from the gold list on a seed+floor `Rng`
  (`src/Midas/MidasEvents.cs`), non-gold options dropped by a `SetEventState` prefix; rest site options cost
  50 + 50 per paid action at the same fire and come back after each one (`src/Midas/MidasRestSite.cs`, a
  `RestSiteSynchronizer.ChooseOption` prefix/postfix), Maw Bank "Reopen" while the bank is broken. Rules in
  `src/Core/MidasRule.cs`. Mutually exclusive with Matriochka (every reroll value).
- Build 0 warnings; 603 xUnit + 91 pytest pass. Not tested in game: `docs/testing/roi-midas-checklist.md`.

## 1.3.0 — Spoon (2026-11-16, branch `two-modes`)

- **Spoon** (`SpoonSpoon`, Custom Run only, spec `docs/superpowers/specs/2026-11-14-spoon-design.md`): max HP as the
  run's currency. Every player starts with a Spoonful (`src/Spoon/Spoonful.cs`: a Feed copy, Retain + Eternal (Innate dropped 2026-11-16),
  no Exhaust; hidden `SpoonCardPool`); the 12 max HP relics at run start since 1.3.7 (`SpoonRule.StartRelics`,
  pickup max HP applied by hand, removed from every grab bag); "Feast" rest option (+5 max HP, `SpoonFeastRestSiteOption`); Fruit Juice replaces one merchant potion
  slot (`SpoonMerchantPatch`).
- **The Spoon act** (`src/Spoon/SpoonAct.cs`, `SpoonRouting`, `SpoonActMap`; `IAppendedAct` shared with the Ending):
  inserted from `AfterCombatVictory` after the last act 3 boss when a player has 300+ max HP, in front of The Ending
  (`EndingRouteRule.ShouldAppend` takes `spoonPresent`). One rest site then the boss; `SpoonMapTravelPatch` keeps free
  travel from softlocking the empty grid.
- **The fight** (`SpoonMonster`, `SpoonFight`, `SpoonFeed`, `SpoonPowerBlockPatch`; rules in `src/Core/SpoonRule.cs`):
  fighters = players at 300+ max HP, the rest are supporters. Mouthful (30, eats 10 max HP and heals) and Ladle Smash (50),
  +3 Strength per move, damage scaled by the team's total max HP, 0 on supporters; supporter damage to the Spoon is
  nullified (hits, listed damaging powers, Haunt, Judgement) and the Spoon only targets fighters (`GetPossibleTargets`
  postfix); supporters get Spoon-Feed (5 max HP to a fighter); defeat when every fighter is dead.
- Art is placeholder (Pixabay photos, sources in `docs/backlog.md`, which also lists the drawing list, the open design
  call about Ruby Recall at the Spoon rest site and the deferred minors).
- Review fix wave: Spoon-Feed gives min(5, max HP − 1), so down to 1 max HP, never lethal, and the fighter gains what
  was lost (`SpoonRule.GiftAmount`, owner decision 2026-11-16); a supporter's Pressure Points
  skips the Spoon; the Juice price comes from the mod's stream (`SpoonJuiceCostPatch`, `PlayerRng.Shops` untouched).
- Main-menu "Test: The Spoon" (`src/Spoon/SpoonTestStart.cs`, `SpoonTestButtonPatch`): only for a mod loaded from the game's
  `mods/` folder (`SpoonTestStartRule.IsLocalInstall`), a solo Custom Run on the Spoon act (Ironclad, 350 max HP, `SpoonTestDeck`),
  hidden `SpoonTestRun` flag skips the modded profile's progress (`SpoonTestProgressGuards`).
- Build 0 warnings; 675 xUnit + 91 pytest pass; `make workshop` stages 1.3.0. Not tested in game:
  `docs/testing/spoon-checklist.md`.
- **Cutlery redesign** (branch `spoon-test-start`, spec `docs/superpowers/specs/2026-11-16-spoon-cutlery-design.md`): the
  Spoon cycles Sets the Table (Frail, deals each player a cutlery kit by deck size), Mouthful (Block from unblocked
  damage), Banquet (Vulnerable, eats 10 max HP unless Skimmed, heals 60 + 10 per extra player) and Ladle Smash; Strength =
  (max HP − start) / 5, applied by delta (`SyncStrength`). Cards in `src/Spoon/SpoonCutlery.cs` (Teaspoon, Wooden Spoon,
  Skimmer, Melon Baller; Pixabay portraits), powers Sated/Skimmed, tweens `SetTableAsync`/`BanquetAsync`. Build 0
  warnings; 702 xUnit + 91 pytest pass. Not tested in game.
- **1.3.1:** the Spoon and Ending map screens sized to their rows (`src/Core/AppendedMapHeight.cs`, `src/Act4/AppendedMapHeightPatch.cs`):
  vanilla spreads any map over 2325 px, so a 3-row act was as tall as a 15-room one. A `NMapScreen.SetMap` postfix gives
  the rows Glory's spacing (2325 / 13), keeps Glory's boss gap, redraws the paths, sets `_distY`; scroll top follows the
  boss. View only. 706 xUnit + 91 pytest pass. Not tested in game.
- **1.3.2 (balance, owner 2026-11-16):** growth from hits no longer gives Strength (`StrengthFor(maxHp, start, grown)`; a
  play test had 200+ hits from the growth → Strength loop); Appetite 5 %/stack; Banquet also hits for 12; the kit is
  one of each of five spoons every cycle (new Stolen Spoonful: steals 3 Appetite, which the player keeps at +5 % damage
  until the Spoon takes 1 back per gain) plus a random extra per 5 cards from 25 (seeded `Rng`); Wooden Spoon deals 10 %
  of max HP. 717 xUnit + 91 pytest pass. Not tested in game.
- **1.3.7 (owner, 2026-11-16):** no more supporters: every player fights the Spoon, Spoon-Feed is removed (the fighter,
  supporter and gift rules above are history). The act opens when the team's summed max HP reaches 300 / 550 / 750 / 900
  for 1–4 players (`SpoonRule.TeamThreshold`). 1200 HP before vanilla multiplayer scaling. Workshop preview is the
  Spoonful portrait under a title band; description and README rewritten. 722 xUnit + 91 pytest pass. Not tested in game.
- 1.3.7 also (owner, 2026-11-16): Appetite in every Spoon-run fight. `SpoonSpoon.AfterAttack` moves 1 Appetite per hit
  from the player to an ordinary attacker (modifiers are combat hook listeners); the Spoonful removes Appetite from any
  monster. 726 xUnit + 91 pytest pass. Not tested in game.
- 1.3.7 also (owner, 2026-11-16): mode cards (`src/Ui/ModeCardPatch.cs`, replaces `FolieHostButtonPatch`): a
  "Slay the Spoon" card on the solo and host screens starts a Standard run with `SpoonSpoon` (new Standard exception;
  Neow hides it like Folie à deux, so Custom Spoon runs get Neow's usual options too). The Spire Souvenir is in every
  vanilla relic pool and the modifier gives it to every player (Necrobinder/Regent: Hybrid colorless). Custom Run
  descriptions rewritten short. Not tested in game.

## 1.3.8 — game 0.111.1 (2026-11-18, branch `spoon-test-start`)

- Port to StS2 v0.111.1 (`min_game_version` 0.111.1). The API changes it absorbed: damage hooks and
  `CreatureCmd.Damage` / `AttackCommand.FromCard` take a `CardPlay?`; `ModifyCardPlayResultPileTypeAndPosition` became
  `ModifyCardPlayResultLocation` (`CardLocation`); `CharacterModel.GenerateAnimator` takes the creature; `LoseBlock` takes a
  context and remover; `Rng.Seed` is gone (seeds are `ulong`, `RunRngSet.Seed`); `CardCreationOptions.WithCustomPool` is gone
  (filter the pools); `NControllerManager.IsUsingController` → `IsUsingDirectionalNavigation`; `PotionFactory.GetPotionOptions`
  lost its blacklist (callers apply it). The game now registers mod models' `[SavedProperty]` names itself
  (`ModelIdSerializationCache`), so `Sts1SavedProperties` is removed.
- `ModEntry.PatchEach` patches class by class and logs every broken target before refusing to load (a game update
  shows all breakages in one launch). New decompile: `~/Projects/sts-decomp/sts2-0.111.1/` (the old one stays in `sts2/`).
- Digest rest option in Spoon runs (`SpoonDigestRestSiteOption`, owner 2026-11-18): −15 max HP, heal to full; disabled at
  15 max HP or less (`SpoonRule.CanDigest`). Ruby Recall stays at the Spoon act's rest site (owner decision).
- The mod's progress copy and first-launch popup are removed (owner, 2026-11-18): 0.111.1 copies the unmodded saves
  into the modded folder itself on the first modded launch (once, with its own popup).
- Build 0 warnings; 721 xUnit + 91 pytest pass; loads at the main menu on 0.111.1 with BaseLib 3.4.8. Not tested in a run.

## 1.4.0 — The Forge (2026-11-18, branch `forge`)

- **The Forge** (`SpoonForge`, Custom Run only, spec `docs/superpowers/specs/2026-11-18-forge-design.md`, plan
  `docs/superpowers/plans/2026-11-18-forge.md`): rest option Fuse (`src/Forge/ForgeFuseRestSiteOption.cs`, Smith icon) merges
  two copies (same id, or a StS1 card and its `Sts2Counterpart` namesake; the player picks the version on a preview);
  levels add up plus one, clamped to the card's cap. `ForgeLevels` patches `MaxUpgradeLevel` (cap in Forge runs; 9 inside an
  open scope: fusion and loading a saved card above +1), gates `IsUpgradable` so ordinary upgrades stop at +1, and replaces
  `UpgradeInternal` past +1: replay `OnUpgrade`'s var deltas (or the table's `perLevel`), keep cost, stars and local keywords;
  cost-only cards keep lowering their cost. Caps: `data/forge/curves.json` (embedded resource), generated by
  `tools/forge_curves.py` from `data/forge/cards.json`, a hand review of all 792 cards (what +1 does, what the card does,
  cap, `perLevel`, `beyond`, `costPerLevel`, `powerBonus`, `play`; energy/extra plays 3, draw 5; Limit Break gains 1
  Strength per level past +1 before doubling; Tracking +25 % per level). `powerBonus` (`ModifyPowerAmountGivenAdditive`
  on `SpoonForge`, by the card source) and `play` (`AfterCardPlayed`: Block, Damage, Draw, Energy, Strength, Dexterity,
  Vigor, Poison, Weak, Vulnerable) add per level past +1 up to the cap (`ForgeBonuses`); the card text gets an orange
  "Forged: ..." line (`ForgeBonusTextPatch`). Owner page: `docs/review/forge-curves.md`. Without a row: grows to +9 when the +1 changes a
  number, else not at all. Every fusable card fuses to +9 (owner, 2026-11-18: absurd is fine, the goal is fun): past its cap
  only the row's `beyond` vars (every non-risky var) keep growing, and the ladder gives Retain at +6, cost −1 at +7,
  Innate at +9 (`ForgeRule.BonusAt`). Merchant Duplicate button (`src/Forge/ForgeDuplicate.cs`, queued `NetForgeDuplicate`): +0 copy once per
  visit, price `ForgeRule.DuplicatePrice`, purchases saved in `SpoonForgeDuplicates`.
- The fusion preview sets `ForgeFusion.Previewing`, which the Hybrid swap, King Midas and Folie à deux respect.
- Build 0 warnings; 748 xUnit + 99 pytest pass. Not tested in game: `docs/testing/forge-checklist.md`.

## 1.5.0 — backlog modes (2026-11-18, branch `backlog-modes`)

- Seven Custom Run modes from the owner's brainstorm (spec `docs/superpowers/specs/2026-11-18-backlog-modes-design.md`,
  builder calls there; compatibility table at its end): **Mutation** (`SpoonMutation`, each new act transforms unupgraded
  cards), **Deck of 10** (`SpoonDeckOfTen`, excess dropped for +1 max HP each), **Blood** (`SpoonBlood`, gold read as
  (HP − 1) × 5 and paid in HP, `src/Blood/`, Blood Fang relic), **Trophies** (`SpoonTrophies`, elite attack cards,
  `src/Trophies/SpoonTrophyCard.cs`), co-op **Forced Trade** (`SpoonForcedTrade`), co-op **Conductor** (`SpoonConductor`,
  `src/Conductor/`), **Useless Relics** (`SpoonUselessRelics`, `src/UselessRelics/`: a once-per-fight relic that flashed
  can be thrown for 10 damage through a synced `UselessRelicThrowAction`; thrown relics are filtered out of both
  `IterateHookListeners` until combat ends). Pure rules in `src/Core/*Rule.cs` with tests.
- Build 0 warnings; 795 xUnit + 99 pytest pass. Not tested in game: `docs/testing/backlog-modes-checklist.md`.
- **1.5.1 (owner, 2026-11-18):** Deck of 10 is a relic (`src/DeckOfTen/SpoonDeckOfTenRelic.cs`): the mode gives it at
  the start; the first trim is the Sealed Deck keep grid, later trims the removal screen. New Standard exception: Neow's
  cursed option is Deck of 10 one time in 11 (`DeckOfTenNeowPatch`, the mod's own seed stream, vanilla draws untouched).
  801 xUnit + 99 pytest pass. Not tested in game.

## 1.6.0 — Cursed Deck (2026-11-18, branch `backlog-modes`)

- **Cursed Deck** (`SpoonCursedDeck`, Custom Run only, spec `docs/superpowers/specs/2026-11-18-cursed-deck-design.md`, plan
  `docs/superpowers/plans/2026-11-18-cursed-deck.md`): every player starts with the Unholy Grimoire and a random curse; the
  Grimoire makes curses playable (cost 1, 0 upgraded, one enemy target, Exhaust), then the curse's effect hits the chosen
  enemy. Curses upgrade to +1 in the mode (Black Censer too), with nerfs for Normality+/Guilty+. Every non-Ancient "?" event
  gets an "Embrace the curse" option (random curse for a cursed relic, 75 gold or a 25 % heal). Relic draws are cursed 2 in 3
  from a pool of 11 (7 vanilla + Blood Pact, Pin Doll, Black Censer, Reliquary) until all are held; seeded own streams, vanilla
  RNG untouched.
- Files: `src/Curses/` (mode, playable and upgrade patches, event option, relic draw, relics), `src/Cards/Sts1/Curses/`
  (Parasite, Pride, transform patch), `src/Core/CurseRule.cs`, `data/curses/curses.json` (hand table, embedded),
  `tools/curse_table.py` (checks every curse has a row, writes `docs/review/cursed-curses.md`).
- Build 0 warnings; 819 xUnit + 106 pytest pass. Not tested in game: `docs/testing/cursed-deck-checklist.md`.

## 1.7.0 — Achilles' Heel (2026-11-18, branch `backlog-modes`)

- **Achilles' Heel** (`SpoonAchillesHeelMode`, Custom Run only, spec `docs/superpowers/specs/2026-11-18-achilles-heel-design.md`):
  every player starts with the Achilles' Heel relic (`SpoonAchillesHeel`, `src/Achilles/`, Event rarity) and the three common Block
  relics (`AchillesRule.StartRelics`: Anchor, Oddly Smooth Stone, Gorget), taken out of every grab bag. The heel (`SpoonHeelPower`,
  never removed, survives the owner's death in co-op) kills its holder on any unblocked attack hit (not Poison). Dexterity per act is
  the 0-based act index, uncapped, so appended acts count (the Ending after the Spoon act gives 4). Attacks deal +1 per 5 Block
  (`AchillesBlock`); card offers lean x2 toward Block cards. Numbers in `src/Core/AchillesRule.cs`. All modes combine.
- Art: a gladiator sandal image the owner provided (2026-11-19), relic and Workshop images.
- Build 0 warnings; 839 xUnit + 106 pytest pass. Not tested in game: `docs/testing/achilles-heel-checklist.md`.

## 1.8.0 — Glass Cannon (2026-11-19, branch `glass-cannon`)

- **Glass Cannon** (`SpoonGlassCannon`, Custom Run only, spec `docs/superpowers/specs/2026-11-19-glass-cannon-design.md`):
  every player starts with the Glass Heart (`SpoonGlassHeart`, `src/GlassCannon/`, Event rarity), Vajra and Girya
  (`GlassCannonRule.StartRelics`, out of every grab bag). Each combat the heart applies 99 Frail and Frenzy
  (`SpoonFrenzyPower`, Buff, Enrage icon): +10 % powered attack damage per card played, the holder's and their Osty's;
  Strength equal to the streak at the end of each turn without a Block Skill (1, 2, 3...). A Block Skill
  (`GlassCannonBlock`: the Achilles test, neither Attack nor Power) resets stacks and streak. The Heart is unchanged.
  With Achilles' Heel too, offers lean ×2 toward Block cards and Attacks (once, `OfferWeightPatch.Weights`).
  Numbers in `src/Core/GlassCannonRule.cs`. All modes combine.
- Build 0 warnings; 857 xUnit + 106 pytest pass. Not tested in game: `docs/testing/glass-cannon-checklist.md`.

## 1.8.1 — forged looks (2026-11-19, `main`)

- Forged cards wear their level (owner request): from +2 the title follows `ForgeTitleColors` (yellow-green → cyan,
  lighter fill and near-black outline where it would clash with the Rare or Uncommon banner) and the frame crossfades to the
  Ancient one (`AncientBlend`, border, glass, navy text background; the banner swaps to the Ancient one at +6, `AncientBanner`, since two translucent banners showed a bar); a gloss shader sweeps the Ancient border
  (`shaders/forge_gloss.gdshader`), the +9 title shimmers (`shaders/forge_diamond.gdshader`). `src/Forge/ForgeCardLook.cs`
  patches `NCard.UpdateTitleLabel` and `UpdatePortrait`. Never read `CardModel.AncientTextBg` on a non-Ancient card: it
  throws, and inside the redraw that crashes the game; the path is built by hand.
- `MaxPatch`: forged cards keep their "+N" title outside a Forge run.
- `CardRenderer` `gif:Name`: the card's climb from +0 to +9 (1.5 s per level, upgrade pop and particles, 12 fps frames);
  `workshop_banners.py` turns it into `workshop/images/forge-climb-en.gif` (320 px, 128 colors), shown in both Workshop descriptions (English only).
  The GIF takes over a minute per language, so render it in its own launch.
- Build 0 warnings; 867 xUnit + 110 pytest pass. Not tested in game: "Forged look" in `docs/testing/forge-checklist.md`.


## 1.8.2 — fixes (2026-11-19, `main`)

- Forge: the fused card keeps an enchantment from either copy (`ForgeRule.FusedEnchantment`: the kept card's, else the
  sacrificed copy's), on both fusion paths (`ForgeFusion.CarryEnchantment`). Owner bug report.
- Matriochka: Dolly's Mirror is given on entering the act 3 Reflections room, before Shatter (owner, 2026-11-19), not at the
  first merchant; players who already hold it get none.
- In-game screenshots on the Workshop page and README (`workshop/images/screenshots/`): a 972-card Matriochka deck and a
  Bludgeon+9 dealing 344 for the Forge, language-free (the test accepts it without a `-en`/`-fr` suffix).
- Forged look: a grey copy of the ordinary frame (`ForgeCardLook.Backing`) sits under the Ancient border (a translucent
  glass meant for a full-art portrait; over the bare card back the top turned black with a hard edge). The real frame
  is drawn above the Ancient layers in the card scene, so it fades out; kept opaque, it hid them all up to +9.
- The Watcher is an ordinary character (owner, 2026-11-19): Standard co-op too; her Standard button and Random follow
  `WatcherRunRule.OfferedInLobby` (every lobby but Daily, solo or co-op). Her pool has no StS2 pair, so she keeps
  her StS1 cards whatever the shared card set; co-op ascension is the shared multiplayer one.
- 868 xUnit + 110 pytest pass. Not tested in game.

## 1.8.3 — Deck of 10 black screen (2026-11-20, `main`)

- Deck of 10 with a starting deck over 10 (Silent, or a King Midas curse) froze on a black screen at Neow: its trim
  opened the selection inside `AfterRoomEntered`, which runs while the screen is faded out, so the fade never lifted.
  The relic no longer trims on room entry; the start-of-run trim runs in `AfterActEntered` (after the fade-in), and no
  trim runs before `RunManager.Launch` sets `LocalContext.NetId` (`FinalizeStartingRelics`). Rule for every mod hook:
  never open a player choice from `AfterRoomEntered`.
- 870 xUnit + 110 pytest pass. Not tested in game.

## 1.8.4 — King Midas balance (2026-11-20, `main`)

- Owner: the ~4 000 gold start was too much. Gold melts on every map travel (10 %, at least 25; `MidasRule.Melt`), run in a
  prefix on the synced `MoveToMapCoordAction.ExecuteAction` before the fade (`src/Midas/MidasGold.cs`); a player the melt
  empties dies (forced kill). Merchant prices rise 10 % per purchase at that merchant (`ModifyMerchantPrice` on
  `SpoonRoiMidas`); the merchant and the rest site refuse what would leave 25 or less (`MidasRule.CanSpend`).
- 885 xUnit + 110 pytest pass. Not tested in game: "Balance (1.8.4)" in `docs/testing/roi-midas-checklist.md`.

## 1.8.5 — King Midas melt floor, rest site crash (2026-11-20, `main`)

- The melt's fixed part follows the run's income: the player's average gold gained per floor so far, at least 25
  (`MidasRule.MeltFloor`, `MidasMelt.FloorFor` from `MapPointHistory`); merchant and rest site refusals use the same floor.
- Rest site crash: vanilla completed the player's rest site after a paid action, so the returned options threw on use and
  the room exit completed it twice. King Midas now keeps the options (`ShouldDisableRemainingRestSiteOptions` false) and
  the pay-and-repeat postfix resets a completed source.
- Trophies: move titles drop the game's "_MOVE" suffix (`TrophyRule.MoveTitleKeys`); the card portrait is the elite's
  bestiary pose (`images/cards/trophies/`), rendered by `CardRenderer`'s `elites` mode and framed by `tools/trophy_art.py`.
- 896 xUnit + 110 pytest pass. Not tested in game: "Balance and rest site fix (1.8.5)" in `docs/testing/roi-midas-checklist.md`.

## 1.8.6 — co-op serializer fix (2026-11-20, `main`)

- Co-op desync found by the bot tester (`bot-tester` branch): the .NET runtime recompiles hot methods (tiered
  compilation) and the game's MonoMod does not carry Harmony detours over, so BaseLib's extended-data postfix on
  `SerializableCard.Serialize` stopped running on one peer while the other still read it: every synced message after the
  first card was shifted (state divergence, "ModelId entry ID ... is out of range"). `src/Saves/SerializationPatchGuard.cs`
  warms the card/relic/potion serializers 2 s after load (valid samples only: a blank card froze the game), re-applies every
  `Serialize`/`Deserialize` patch through Harmony's `PatchFunctions.UpdateWrapper`, then checks a test card every 5 s.
  Any other patch on a hot method can be lost the same way.
- King Midas texts (Custom Run, Workshop) mention the melt and the merchant's rising prices. Ruined players are logged.

## 1.8.7 — doubled start relic pickups (2026-11-20, `main`)

- King Midas and Spoon applied their start relics' pickup effects by hand in `AfterRunCreated`, but every new-run path
  (`NGame.StartRun`, character select, `NMultiplayerTest`) then runs `RunManager.FinalizeStartingRelics`, which calls
  `AfterObtained` on every held relic: King Midas started with double gold (about 3 500 instead of about 1 770) and two
  Greeds, Spoon with +103 max HP twice. Found by the bot tester. The hand-applied effects are removed; vanilla's run once.
  The King Midas melt (1.8.4/1.8.5) was tuned on the doubled start. 897 xUnit + 110 pytest pass. Not tested in game.

## 1.8.8 — co-op desync fixes from the bot tester (2026-11-20, `main`)

- `src/Saves/DetourGuard.cs`: in game the runtime sometimes installs new native code for a patched method without
  its Harmony detour (seen on dozens of methods: serializers, `MutableClone`, `CardEnergyCost.GetWithModifiers`,
  `MessageTypes.TryGetMessageType`, power icon paths...), so a patch stops running on one peer only. Each frame the guard
  reads a slice of every patched method's native code through MonoMod's `PlatformTriple.GetNativeMethodBody`; a method
  that no longer starts with a jump gets its patches re-applied (`PatchFunctions.UpdateWrapper`) and is logged
  ("detour guard: X lost its detour"). x64 only; inlined copies are invisible to it (functional checks stay in
  `SerializationPatchGuard`: card serializer, Forge `MaxUpgradeLevel`). Needs `AllowUnsafeBlocks`.
- Cursed Deck: a playable curse cost 1 on one peer and stayed unplayable on the other (divergence on turn 1): the cost
  postfix on `GetWithModifiers` had lost its detour. Covered by the guard.
- Blood: each peer charged the other player's whole HP at combat start (`Player.SyncWithSerializedPlayer` wrote a stale
  0 gold). The raw gold field now mirrors the HP-derived gold (`BloodGold.Mirror`) and the sync write is ignored.
- Spoon act: the boss room failed to load on one peer ("could not find res://scenes/backgrounds/spoon_act/layers"): the
  `GetBackgroundAssets` prefix was skipped (inlined). A prefix on the `BackgroundAssets` constructor maps the appended
  acts' titles to Glory's.
- Forge: a fusion re-applies the `CardModel` patches first (`SerializationPatchGuard.ReapplyOn`); the frame check
  re-applies them when the Forge cap stops answering +9.
- `src/Saves/ExtendedSaveNormalizer.cs`: makes a lost BaseLib `ToSerializable` postfix harmless.
- 897 xUnit + 110 pytest pass. Validated by co-op bot runs on `bot-tester` (see its `docs/testing/bot-backlog.md`).

## 1.8.9 — King Midas + Blood ruin (2026-11-20, `main`)

- King Midas with Blood killed both players on the map after a shop: Blood pays in whole HP (a 33 gold card costs 7 HP,
  35 gold), but the King Midas spend check (`MidasMelt.CanSpend`, merchant and rest site) used the listed price, so a
  purchase it allowed left exactly the melt floor and the next travel ruined the buyer. The check now uses
  `BloodRule.PaidGold` (price rounded up to the 5 gold rate) when Blood is active. Found by the co-op bot tester.
- 902 xUnit + 110 pytest pass. Not tested in game.

## 1.8.10 — Watcher death and victory (2026-11-20, `main`)

- The Watcher's creature visuals (built by `EndingVisuals.Create`, like the Ending monsters) had no `%FormVfx` holder.
  Every player death calls `NCreatureVisuals.RemoveFormVfx`, which throws without it: a dying Watcher broke
  `CreatureCmd.Kill`, and at the Architect `RunManager.WinRun` stopped before the game over screen (co-op bot run,
  Watcher host). `EndingVisuals.Create` now adds the holder (form powers such as Demon Form also need it).
- Serialization guard: a patch Harmony cannot rebuild (BaseLib's `NRewardButton._Ready`, "invalid program") is tried
  once instead of logging an error on every sweep.
- Found by the co-op bot tester. Not tested in game.

## 1.8.11 — Brilliance hit effect (2026-11-20, `main`)

- Brilliance referenced `vfx_attack_burn`, a scene absent from StS2; the bot run logged a failed resource load.
  It now uses the existing `vfx_attack_lightning` effect. In-game validation pending.

## 1.8.12 — Watcher rest site animation (2026-11-20, `main`)

- The Watcher's Spine body already plays `idle_loop`, but the vanilla rest site also tried its act-specific loops,
  absent from her skeleton. Exclude only her body from that vanilla animation pass; keep the rest site scene and UI.

## 1.8.13 — Spoon Digest rest site icon (2026-11-20, `main`)

- Digest reused Feast's preloaded heal icon, but the icon getter patch only handled Feast. The rest site repeatedly
  tried to load the missing `option_spoon_digest.png`. Both icon path and texture patches now cover Digest too.

## 1.8.14 — Watcher rest site scene (2026-11-20, `main`)

- A long co-op Watcher run crashed on both peers at an act 3 rest site: the hot `RestSiteAnimPath` getter reverted to
  the nonexistent Watcher scene after runtime recompilation. The `NRestSiteCharacter.Create` call now routes the
  Watcher's scene lookup directly to the Silent donor, preserving the Watcher body swap and rest site controls.

## 1.8.15 — Forge Duplicate and King Midas (2026-11-20, `main`)

- Forge's Duplicate button checked only the card price against gold. In a King Midas run it could spend past the
  survival threshold enforced at merchants and rest sites. The button, selection filter, and final payment now use
  `MidasMelt.CanSpend` when Midas is active (including Blood's whole-HP payment rule).

## 1.8.16 — Watcher rest site animation follow-up (2026-11-20, `main`)

- The 1.8.12 postfix filtering `GetChildSpineNodes` was bypassed in a co-op bot run: the Watcher's skeleton still
  received the donor's missing `overgrowth_loop`. The `_Ready` call site now routes the Watcher to no donor Spine
  animations, leaving her separate `idle_loop` running. The patch logs if the expected call was not rewritten.

## 1.8.17 — Matriochka final card (2026-11-20, `main`)

- Owner: no mandatory card after the run's last boss. The Architect's victory room is an `EventRoom` on the boss map point
  (not an Ancient), so Matriochka's per-floor offer fired there; `MatriochkaRule.OffersFloorCard` now skips
  `AbstractRoom.IsVictoryRoom`. 907 xUnit + 110 pytest pass. Not tested in game.

## 1.8.18 — Folie à deux chest view (2026-11-20, `main`)

- Owner report: the chest's Continue button was below the screen and cards showed "Broken Card". `NCard.UpdateVisuals`
  does nothing before the node is ready; the column was built detached, so only pooled (already ready) cards rendered.
  The view now updates its cards once in the tree and `Fit()` scales the content to the viewport (also after bids and
  the Continue button). 907 xUnit + 110 pytest pass. Not tested in game.

## 1.8.19 — King Midas rest site softlock (2026-11-20, `main`)

- Co-op bot run (King Midas, Regent + Silent, act 2 rest site): both players could pay for no option, every option was
  greyed, and vanilla enables Proceed only after a choice, so the room could not be left. `MidasRestLeavePatch`
  (`NRestSiteRoom.UpdateRestSiteOptions` postfix) enables Proceed when `MidasRule.LeaveOnly`. 908 xUnit + 110 pytest
  pass. Not tested in game: "Rest site with no affordable option" in `docs/testing/roi-midas-checklist.md`.

## 1.8.20 — stale card-play cancel marker (2026-11-20, `main`)

- Vanilla co-op race found by the bot tester (Deck of 10 + Mutation, Defect + Regent): a card paused for a choice with
  `CancelPlayCardActions` (Photon Cut) when the combat ended in the same frame; the end-of-combat cancel removed it, then
  the pause set `ActionQueue.actionCancellingPlayCardActions` on the removed action, so the marker never cleared and the
  client dropped the next fight's host-confirmed plays (state divergence at the act 3 boss). `src/Combat/StaleCardCancelPatch.cs`
  (`ActionQueueSet.EnqueueWithoutSynchronizing` prefix) clears a marker whose action is no longer queued, and logs it.
  908 xUnit + 110 pytest pass. Not tested in game.

## 1.8.21 — co-op boss chest wait (2026-11-20, `main`)

- Co-op bot run (StS1 Boss Relics, act 3 Ancient): a player who took the boss chest before the others got the "waiting"
  page with no options, which vanilla treats as a finished event, so the room showed Proceed. The bot (a human could too)
  left the Ancient; the choose-a-relic screen then opened over the map and the map vote stalled; the final
  `SetEventFinished` would also have thrown. The waiting page now holds one locked option. 908 xUnit + 110 pytest pass.
  Not tested in game.

## 1.8.22 — Watcher card trail (2026-11-20, `main`)

- Co-op bot run (Blood + Cursed Deck, Silent + Watcher, act 1): the client crashed (SIGSEGV) right after failing to load
  `res://scenes/vfx/card_trail_sts1_watcher.tscn` during a shuffle: the hot `TrailPath` getter had lost its Harmony detour
  and returned the vanilla path. `src/Characters/WatcherTrailAlias.cs` makes a copy of Necrobinder's trail scene take
  over that path in Godot's resource cache at startup (logs "load check ok"), so the vanilla path loads with no patch.
  The same alias trick fits any borrowed scene path whose getter can revert.

## 1.8.23 — Watcher at Darv (2026-11-20, `main`)

- Co-op bot run (King Midas + Forge, Watcher + Silent): both peers died by SIGSEGV entering Darv. Darv's Dusty Tome
  (half of the visits) draws an Ancient card from the owner's pool; the Watcher's pool has none, so `NextItem` returned
  null and `SetupForPlayer` read `.Id` on it (the older `DustyTomeBypass` only covered StS1 card set modes). The bypass
  prefix now skips the setup when the pool has no Ancient card, and `DarvTomeFallbackPatch` (`Darv.GenerateInitialOptions`
  postfix) replaces that Tome with one more Darv relic the owner may take, on the event's Rng (`DarvTomeRule`).
  911 xUnit + 110 pytest pass. Not tested in game.

## 1.8.24 — Matriochka with vanilla Vintage (2026-11-23, `main`)

- Vintage turns normal fights' card rewards into relic rewards, which left Matriochka floors without their card.
  `src/Matriochka/MatriochkaVintagePatch.cs` (prefix/postfix on `Vintage.TryModifyRewardsLate`, independent of listener
  order) puts each replaced card reward back, unskippable, next to the relic (`MatriochkaRule.CardsAfterVintage`).
- `docs/backlog.md` "Vanilla Custom Run modifiers × mod modes" lists every vanilla modifier against the mod's modes.
- 915 xUnit + 110 pytest pass. Not tested in game.

## 1.8.25 — Matriochka with vanilla Hoarder (2026-11-23, `main`)

- Matriochka already applies Hoarder's rule, so both ticked gave 5 copies per card. Owner: keep them compatible and
  ignore Hoarder. `src/Matriochka/MatriochkaHoarderPatch.cs` skips `Hoarder.AfterCardChangedPiles` while Matriochka is
  active. Not tested in game.

## 1.8.26 — Conductor screen, King Midas with vanilla Midas (2026-11-23, `main`)

- Conductor (owner: the text popups were ugly): `src/Conductor/ConductorOverlay.cs` shows every movable reward of the
  fight on one screen (cards as `NCard`, relics and potions with icon, name and text), a row of character portraits per
  reward (owner preselected) and one Confirm. The picks travel as one synced index list (`SyncedChoice.ChooseMany`,
  `PlayerChoiceResult.FromIndexes`); `ConductorRule.Recipients` falls back to the owner for a bad pick. `ConductorPopup`
  is removed.
- King Midas + the base game's Midas (gold rewards ×2): `MidasRule.Scale` doubles the melt percent and minimum, the
  merchant price rise and the rest site prices (`SpoonRoiMidas.Scale`).
- 918 xUnit + 110 pytest pass. Not tested in game: the Conductor lines of `docs/testing/backlog-modes-checklist.md`,
  "With the base game's Midas" in `docs/testing/roi-midas-checklist.md`.

## 1.8.27 — Conductor layout (2026-11-23, `main`)

- Owner: four players looked cramped. From three players each reward's portraits are a 2 × 2 square
  (`ConductorRule.HeadColumns`); the rewards take 1–3 columns, whichever shrinks the page least on the viewport
  (`ConductorRule.Columns`, under 2 % better keeps fewer), redone on `SizeChanged`. `ConductorOverlay` builds from
  `Head`/`Entry` records so `CardRenderer`'s `conductor` mode can show it with sample rewards (2–4 players, 16:9, 16:10, 21:9
  SubViewports at the game's 1080 height). 924 xUnit + 110 pytest pass. Rendered, not tested in a run.

## 1.8.28 — Decimillipede Trophy title (2026-11-23, `main`)

- Bot run (Trophies + Useless Relics): a Trophy from the Decimillipede elite looked up `DECIMILLIPEDE_SEGMENT_BACK.name`,
  which does not exist (the segments share `DECIMILLIPEDE_SEGMENT.name` through an overridden `Title`). `SpoonTrophyCard`
  now reads the monster model's `Title` (cached `ModelDb.Monsters` by id). Not tested in game.


## 1.8.29 — Matriochka: Shatter forbidden, 999-card deck (2026-11-23, `main`)

- Owner, after a co-op bot run reached 3 127 cards: Reflections offers only Touch a Mirror in Matriochka
  (`ReflectionsTouchPatch`, was the Shatter-only patch); the deck stops at `MatriochkaRule.DeckLimit` (999):
  `CopiesFor` trims Hoarder copies, a full deck gets no floor card (solo and Folie à deux) and no combat card reward
  (Vintage's put-back included). Other card sources are not capped. 932 xUnit + 110 pytest pass. Not tested in game.

## 1.8.30 — Conductor: give a chosen card (2026-11-23, `main`)

- Owner request: on the Conductor screen a click on a card of a card reward rings it in gold (`ConductorOverlay.RewardView`,
  a click layer above each `NCard`); that card goes straight into the recipient's deck (`CardPileCmd.Add`, the reward
  removed); a second click clears it. One synced int per reward carries both (`ConductorRule.Encode`/`Decode`:
  slot + players × (card + 1)); `CardPicks` drops bad values. A hint line sits under the title. 938 xUnit + 110 pytest
  pass. Not tested in game.

## 1.8.31 — full detour sweep before actions and checksums (2026-11-23, `main`)

- Bot run Q5 line 22 (Conductor + Cursed Deck, Necrobinder + Regent): divergence at the turn 2 start checksum, only the
  piles differed, a curse in each hand. About 30 detours (`CardEnergyCost.GetWithModifiers` among them) were lost on one
  peer between two checksums; the frame guard (48 methods per frame, ~13 frames per sweep) put them back after the game
  had hashed the curse's cost (−1 on one peer, 1 on the other). `DetourGuard.SweepAll` now checks every patched method
  before each game action (`ActionExecutor.BeforeActionExecuted`, an event: no detour to lose) and before each checksum
  (`ChecksumSweepPatch`, prefix on `ChecksumTracker.GenerateChecksum`). The first sweep logs its duration. Not tested in game.

## 1.8.32–1.8.33 — King Midas: ruined players come back (2026-11-23, `main`)

- Owner: the melt still kills a player it empties; in co-op they come straight back (same travel, before the next room)
  with 1 HP (`CreatureCmd.Heal`, like vanilla's end-of-fight co-op revive) and 1 gold (`MidasRule.RevivalGift`), unless
  every player was ruined (game over). A dead player must never enter a room: vanilla has no dead players outside
  combat, and a shared event waited forever for the dead player's vote (bot run on 1.8.32, which left them dead for one room; fixed in 1.8.33). A player somehow
  dead on traveling gets the same revival instead of paying (`MidasRule.OnTravel`). Wacky-mode suggestion invite in
  the Workshop descriptions and README. Not tested in game: "Ruined players come back (1.8.33)" in
  `docs/testing/roi-midas-checklist.md`.

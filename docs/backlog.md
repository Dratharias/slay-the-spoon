# Backlog

Remaining design decisions and deferred work after the 0.10.1 fix wave (2026-10-30).
Completed work is recorded in CLAUDE.md; in-game checks remain in docs/testing/fix-wave-checklist.md.

**1.0.0 (2026-11-05):** every entry below is done, checked, kept with a reason, or an owner item (in-game checks,
the co-op crash report's client log; Spine animations are done in 1.0.1). New bugs and requests go at the end.

- ~~StS1 character-specific relics (Ironclad, Silent, Defect, Watcher pools) that StS2 dropped, offered when a StS1 card
  set mode is active (owner request, 2026-10-29).~~ Done in 0.12.36: Champion Belt and Magic Flower (Ironclad) and The
  Specimen (Silent) join their pools as Rare relics in Custom Runs with a StS1 card set (`src/Relics/Sts1/`), with StS1
  art (`tools/sts1/event_art.py`, `images/relics/sts1/`) and EN/FR text. Inventory: `docs/review/character-relics.md`
  and `data/duplicates/character-relics.json`. The boss relics are ported too; their Custom Run option (0.12.37) is the entry
  under "Design calls settled" below. Special event relics stay imported without implementation (owner, 2026-10-30).
- Act 4 keys (step 6 WP3), minors left after review:
  - ~~A co-op Sapphire swap waits up to 15 s for its pickup.~~ 0.12.0: the swap is gone (chest rematch instead).
  - ~~The Sapphire button position is hard-coded (650, 770).~~ 0.12.1: the key takes a free slot of the chest scene
    (left with 2 relics, right with 3); with 4 relics it uses a hand-picked fifth slot at (220, −180) from the chest
    center, which may sit close to the right-hand relic or a hand. Solo: 120 px right of the relic.
  - ~~The Ruby icon comes from a postfix on the private `RestSiteOption.IconPath` getter, which the JIT may inline.~~
    0.12.20: the public `Icon` getter is patched too.
  - ~~`StandardCharacterSelectPatch` sets `Pending` for any game mode~~: `NCharacterSelectScreen` only builds Standard
    lobbies (checked 2026-11-05); ~~`Act4UnlockPatch.Record` logs failures at info~~ (error with the stack since 0.12.17).
- Act 4 keys (step 6 WP3), design calls made without the owner (change any of them):
  - Sapphire chest rematch (0.12.0): only while the run has Act 4 and the team lacks the Sapphire; other chests stay
    vanilla (random consolation prize). The key is offered only to the last player left, so when every player wins a
    relic in the first round (no shared vote) nobody can take the key from that chest. Rematch rounds reuse the
    vanilla vote action; winners of earlier rounds keep their relic in hand, their fights are not replayed in the final
    award. In fake multiplayer the other players' rematch votes are random among the relics left and the key.
  - Ruby, Emerald and Sapphire text, key sprites at 25 % opacity in the top bar while missing.
  - Emerald elite buff: Strength act+2, +25 max HP, Metallicize 2·act+4, Regenerate 2·act+3 (one of them, rolled).
  - The cfg holding the permanent unlock is never overwritten when unreadable; bool and number values are accepted.
- Act 4 The Ending (step 6 WP4-6), design calls made without the owner (change any of them):
  - ~~Static sprites rendered from the StS1 setup pose, not animations.~~ 1.0.1: the Spear, the Shield and the Heart
    play their StS1 Spine skeletons (converted to 4.2 by `make sts1-spine`); the static sprites stay as the fallback.
  - Monster visuals and the elite slot layout are built in code, not as scenes.
  - The Ending is added to the run when the team takes its last key; only the second act 3 boss at A10 gives rewards
    (the single boss below A10 still rewards), per owner decision.
  - Map: rest site → merchant → elite → Heart, no ancient, no event.
  - The Spear applies Surrounded (Shield Back Attack Left, Spear Back Attack Right).
  - Co-op: Debilitate, Bash's debuff and Burn Strike's Burns hit every player; Beat of Death hits the card's player;
    Smash's Block equals its highest pre-Block hit on one player (99 at A10).
  - The Monster HP % does not change the Heart's Invincible budget.
  - Move names (StS1 has none): Bash/Heurt, Fortify/Fortifier, Smash/Fracas, Burn Strike/Frappe ardente,
    Piercer/Perceur, Skewer/Embrocher, Debilitate/Affaiblir, Blood Shots/Tirs sanglants, Echo/Écho, Buff/Buff (vanilla's French move title, not
    the planned "Renforcement").
  - Act 4 elite rewards are vanilla elite rewards.
  - Statuses shuffled into the draw pile take their position from the monster's own RNG.
  - Neow ignores the Act 4 Custom Run option.
  - Co-op: Shield and Spear get vanilla's multiplayer Artifact bonus (+1 per extra player), as every StS2 enemy does.
  - The Heart's sprite is the setup pose without IK/path constraints: its three top vents look folded.
  - The Heart's Painful Stabs buff applies 1 stack (StS1's data gives no amount).
  - The fixed map is built when the act's map is generated and forced over Golden Compass's map.
  - The Ending is appended as soon as the team holds all three keys, even in act 1 or 2 (harmless: only the last act
    checks the act count).
  - The Heart's intents and health bar use StS1's hitbox (476 × 410), not the full sprite; its vent tops may run ~90 px
    past the top of the screen, as in StS1 (no camera zoom added). Intents sit 60 px above the hitbox (estimate).
  - Blood Shots and Echo use vanilla's blunt hit effect; StS1's Blood Shot and Vice Crush effects and sounds are not ported.
  - The Heart's English/French loss lines are the builder's own wording.
  - ~~The act's French title is StS1's "La fin"~~: "La Fin", StS2's style (owner call 2026-11-05, 0.12.26).
- Act 4 The Ending (step 6 WP4-6), minors left after review:
  - ~~The Invincible clamp sits in `Sts1InvinciblePower`, not in the tested `HeartInvincibleRule`; it flashes inside the modify
    hook~~ 0.12.23: `HeartInvincibleRule.ClampHpLoss` (tested); the flash moved to `AfterModifyingHpLostAfterOsty`, as Intangible.
  - ~~`PoisonTicksDrainTheSameBudget` only exercises unblockable damage; `BlockedHitsAndExhaustedBudget` is vaguely named.~~
    `BlockedHitsDrainOnlyTheirHpLoss` added, the theory renamed `BudgetNeverGrowsNorGoesNegative` (2026-11-05).
  - ~~`Sts1InvinciblePower.AfterApplied` takes the turn cap from the current amount: a mid-turn re-apply would use the
    depleted value.~~ 0.12.15: the cap never drops on a re-apply.
  - ~~`EndingMoveBrain` tests are weak: they compare two brains or re-read getters instead of asserting an expected move
    sequence. `EndingNumbers` tests miss the low side of some thresholds~~ (expected StS1 cycles for the three enemies and
    both sides of every threshold tested, 2026-11-05); ~~`HeartBuffRule.Extra` accepts a negative count~~ (throws since 0.12.15).
  - ~~`ending_art.py` ignores atlas `offset`/`orig`~~ (region attachments honor them since 2026-11-05; meshes keep the
    packed area, as the Spine 3.4 runtime does; the three StS1 sprites render byte-identical); its additive
    blend is untested and not Spine-exact; triangle edges are not antialiased; no grandparent or shear bone test.
  - ~~Code-built monsters have no death animation length: their fade may be cut short~~ (1 s `DeathAnimLengthOverride`
    since 0.12.25; the length is a guess, unchecked in game).
  - ~~Mod elite wins count toward the vanilla 15-elite epoch check (modded profile only).~~ Fixed in 1.0.1:
    `EndingEliteEpochPatch` removes Shield and Spear from `GetEliteEncounters`, as the boss epoch already skips the Heart.
  - ~~Debilitate skips dead players: a player revived later in the fight never gets those debuffs or statuses. In co-op
    the status intent shows 5, the per-player count.~~ Owner call (2026-11-05): a revived player
    catches up on nothing, as in vanilla StS2; multi-target intents show the per-player amount in vanilla too.
  - ~~The Heart's status shuffles share its RNG with its move roll, so Blood Shots vs Echo on turn 2 depends on the
    number of players and their draw piles (deterministic on every peer, seeds stay vanilla).~~ Fixed in 1.0.1: the
    shuffles use their own `ShuffleRng` stream (`Sts1EndingMonster`), so the move roll no longer depends on the players.
  - ~~The Ending act's rooms are seeded from the caller's Rng seed in `EndingRoomsPatch` but from the run seed in
    `Sts1EndingAct.CreateForRun`~~: both use the run seed since 0.12.22 (the caller's seed only outside a run).
    `FillRooms` does not reset the ancient or visit counters (only called on a fresh act). No automated test covers the
    act model or its patches (game-bound; the checklist covers them).
  - ~~Visiting the act may add `STS1_ENDING_ACT` to the modded profile's discovered acts (not checked).~~ Checked
    2026-11-05: `ProgressState.MarkActAsSeen` adds it, and with the mod removed the id is kept in
    `_unknownDiscoveredActs` and written back, so the modded profile still loads. `CheckForDefeatedAllEnemiesAchievement`
    is empty in v0.107.1; a later game build that fills it in needs a re-check (watch item, nothing to fix today).
  - Kept as in-game checks (2026-11-05; `docs/testing/step-6-checklist.md`): the Ending map: with only three grid rows its four nodes sit far apart (check in game); after the start rest site
    the map marker is not drawn on it (vanilla skips the start row); Golden Compass bought in act 4 leaves
    `GoldenPathAct = 3` set (harmless). The `ModifyGeneratedMap` postfix discards late-listener edits on a fresh act 4
    map (none in vanilla; another mod's quest marks would be lost). If `ActModel.CreateMap` is inlined the prefix is dead
    and the postfix logs "replaced by the Ending map". Only the layout constants are unit-tested.
  - ~~Run history stores the Ending act's id: with the mod removed, history shows a placeholder act; run metrics
    (`ActWinMetric`) would carry the mod's act id if the game uploads metrics for modded runs (not checked).~~ Kept
    (2026-11-05): the history must name the act the run visited; a placeholder after removing the mod is the game's own
    handling of any unknown id, and metrics are the game's business.
  - ~~The `Act4Preview` cfg key is a no-op since the release~~: key, parameter and `Released` removed in 0.12.16.
    ~~`ModifierListPatch`'s `OfferedIn(Custom)` guard is always true at runtime (the list is Custom-only) and reads the
    pure list by string name.~~ 0.12.21: the guard and `CustomRunOptions` are gone; the Act 4 row is listed like the others.
  - ~~The Act 4 row in Custom Run has no icon (check how it renders).~~ Checked 2026-11-05: `NRunModifierTickbox` draws
    no icon for any modifier, vanilla included; only the top bar and the Daily screen read `ModifierModel.Icon`, and the
    top bar shows the keys for `SpoonAct4` (`TopBarPercentPatch`).
  - ~~The Invincible budget test only checks `InitialCap`'s signature; nothing tests that the applied power is not scaled
    in co-op (the power is game-bound).~~ Fixed in 1.0.1: the power's budget lives in
    `HeartInvincibleRule.InvincibleBudget`, tested for co-op (cap not scaled twice) and for a mid-turn re-apply.
  - Killing the Heart logs two vanilla errors from `ProgressSaveManager.ObtainCharUnlockEpoch` (`Act 4 is not yet
    implemented.`, `EpochModel was not found :(`): it switches on the act index for the character epochs and has no act 4
    case. Harmless (no epoch, no write); left alone because routing it to act 3 would hand out an epoch. Kept (2026-11-05).
- The Watcher (step 7), design calls made without the owner (change any of them):
  - She never enters `AllCharacters`: no Daily or co-op Standard Watcher. Since 0.12.36 (owner-approved character-relic
    work) she is selectable in solo Standard and in every Custom Run, and a solo Watcher run without a card set modifier
    uses StS1 cards (`WatcherRunRule`); random picks include her in solo Standard only. Checks: `docs/testing/watcher-selection.md`.
  - Spire Souvenir: she has no StS2 cards, so "Hybrid" for her is her StS1 pool plus Hybrid colorless.
  - Solo ascension is capped by the highest ascension the player has on any vanilla character.
  - Her playtime goes into the modded profile; she never counts toward vanilla epochs.
  - Merchant, multiplayer hands and map marker borrow vanilla images until animation work; her FMOD sounds are silent
    or borrowed. Potion icons are borrowed vanilla icons (StS1 potions have no single image to cut).
  - ~~The Architect has no dialogue for her: no attack scene at the victory, and `GetArchitectAttackVfx` is unused for now.~~
    0.12.28: owner-approved dialogue (see "Design calls settled" below).
  - Discipline and Unraveling are not ported (StS1 never registers them). Cloak Clasp is not ported: she finds the
    native shared Cloak Clasp.
  - She may obtain Archaic Tooth through the vanilla rule (Prismatic Gem or a transform can put Bash, Neutralize, Unleash,
    Falling Star or Dualcast in her deck); no guard.
  - A host that unticks the modifier and readies in the same moment can start a run with her and no StS1 set (race);
    the Spire Souvenir may skip her (`HasSts1Cards`), which is harmless since her pool has no StS2 versions.
  - Vault in co-op: an extra turn for the player who played it only.
  - Lesson Learned uses the combat card selection RNG and upgrades
    its card directly when the kill ends the combat (with vanilla's upgrade preview since 0.12.38).
  - Foreign Influence draws from the attack pools of the current card set (character pools only, 55/30/15 rarity roll on
    the combat card generation RNG), no colorless attacks. Owner-approved StS2 rule for cross-character generation.
- The Watcher (step 7), minors left after review:
  - ~~Signature Move's playability and glow match StS1, but the game displays its generic unplayable message~~: 0.12.38
    `CantPlayMessagePatch` shows StS1's line (`<ID>.cantPlay`) for Signature Move, Clash, Grand Finale, Secret Technique
    and Secret Weapon when their own condition blocks them.
  - ~~Duality's power has no `powers.json` text of its own~~: none needed, `TemporaryDexterityPower` takes its title from
    the relic and its description from vanilla `TEMPORARY_DEXTERITY_POWER` (checked 2026-11-05).
  - Character shell: ~~`OrbModel` takes the energy prefix from the pool title `sts1_watcher`~~ (0.12.38: `OrbEnergyPrefixPatch`
    gives her orbs the Necrobinder energy icon); ~~`WatcherAssetPatch`'s Layer log
    message is misleading when the node exists with another type~~ (0.12.19); ~~`NGameOverScreen` reads `CharacterStats[her id]`
    unguarded~~ (a prefix creates her stats entry first since 0.12.25; the checklist still covers first death and victory).
  - ~~Custom Run button: the static `_previous` relies on vanilla selecting child 0 on open~~: 0.12.36 rewrote
    `WatcherButtonPatch` (always visible, no `_previous` fallback); its selection checks are in `docs/testing/watcher-selection.md`.
  - Progress guards: ~~two classes are named `WatcherProgressPatch`~~ (the Saves one is `WatcherVanillaGuardsPatch`
    since 0.12.13); ~~the epoch-count transpiler drops the labels of the replaced `get_Count`; `VanillaIds()` is
    rebuilt per call~~ (both fixed in 0.12.14); ~~no log when the vanilla ascension cap applies~~ (logged once per value since 0.12.21); ~~`VanillaCount` and `MaxAscensionFor` are untested~~
    (1.0.1: both delegate to `WatcherProgress.VanillaStatsCount` and `WatcherAscension.LimitFor`, tested).
  - ~~Mark and Pressure Points HP loss has no dealer (matches StS1); "on kill by player" effects see no killer.~~ Kept
    (2026-11-05, fidelity rule): StS1 deals it as HP loss with no source.
  - ~~Wave of the Hand applies Weak through a throwing choice context (`AfterBlockGained` gives none); harmless unless a
    hook on Weak ever opens a choice.~~ Kept (2026-11-05): vanilla `JuggernautPower` applies from this hook the same way, and no hook on Weak opens a
    choice (comment in the power).
  - ~~Mental Fortress, Empty Fist and Flurry of Blows show all three stance tips~~: they show StS1's generic Stance
    tip since 0.12.13, as in StS1.
  - ~~Spirit Shield shows no live Block~~ (in-combat Block preview since 0.12.25); Mark and the end-turn death effect share the Doom icon.
  - ~~Lesson Learned's combat-ending upgrade shows no card-upgrade preview effect.~~ 0.12.38: it plays vanilla's
    `NCardUpgradeVfx` for the local player, as `CardCmd.Upgrade` does.
  - ~~`LastCardPlayed` reset between combats (Sanctity) is unverified.~~ Checked 2026-11-05: it reads
    `CombatManager.History`, which `CombatManager` clears at combat start and end.
  - ~~Flurry/Weave and Sands of Time/Windmill Strike share small logic blocks that could be helpers on `Sts1WatcherCard`;
    Consecrate lacks a `<summary>`; mixed `ThrowIfNull` vs `Target!`.~~ 0.12.16 and 0.12.20: `ReturnFromDiscard` and
    `RetainedBy` on `Sts1WatcherCard`; every Watcher card checks its target with `ThrowIfNull`, as the other characters do.
  - ~~Unused `using` blocks in card and power files, empty `OnUpgrade` bodies, a stray blank line in Deus Ex Machina.~~
    0.12.16: every IDE0005 hit in `src/` removed (one lint build with IDE0005 as a warning), the 8 empty overrides dropped.
  - Weak tests: pure-logic coverage of ~~`MantraTracker` keying~~ (`MantraTallies`, tested since 0.12.24), retain gating and
    Lesson Learned selection (both read game state: deck order and the synced run RNG); power tests are
    mostly arithmetic and `WatcherPowerTextTests` greps source; ~~stale pytest name `test_pending_ports_never_enter_runtime`~~ (now `test_every_watcher_card_is_ported`); Task 9's tests were written after the code (no red run). Kept (2026-11-05): what is left reads game state and
    stays in `docs/testing/step-7-checklist.md`.
- ~~The Watcher (step 7), deep backlog: Spine animations for her (combat, rest site, merchant, multiplayer hands, map marker)
  and in-game validation of everything in `docs/testing/step-7-checklist.md`. A "Deep backlog priority" section, if one
  exists on `main`, keeps its own wording; Watcher animations stay at the bottom of it.
  Owner item, kept deferred (2026-11-05): StS2 ships the Spine 4.2 runtime and the StS1 skeletons are Spine 3.4.02
  (`docs/review/act4-mechanics.md`), so animations need the skeletons converted or re-exported in the Spine editor,
  then checked in game; the owner placed this at the bottom of the deep backlog and only they can play-test it.~~
  Done in 1.0.1: `tools/sts1/spine34to42.py` converts the 3.4 JSON to 4.2 (unsupported input is refused),
  `tools/sts1/spine_assets.py` exports her body and staff eye (and the three Ending monsters) into
  `godot/SlaySpoon/spine/`, and `src/Spine/Sts1Spine.cs` loads them at runtime. Combat, rest site and merchant use her
  skeleton (Idle at 0.7, Hit); the eye skeleton follows the body's `eye_anchor` bone and plays None/Calm/Wrath/Divinity;
  her corpse sprite replaces the body on death. Multiplayer hands and the map marker have no StS1 skeleton and keep
  their images. In-game checks: `docs/testing/spine-checklist.md`.

- Fidelity rule (owner, 2026-10-30): StS1 character cards and StS1 content follow StS1 behavior wherever StS2's engine
  allows; StS2 conventions apply to colorless cards and cross-character cards. Design calls above that chose StS2
  behavior for character cards are to be brought back to StS1.

## Owner requests (2026-10-30)

- ~~New game mode: pick the starting relic among 10 random choices.~~ Done in 0.12.35 as the Custom Run modifier
  Starting Chest (`SpoonStartingChest`, see its entry below).
- Act 4 keys, visuals:
  - ~~The Emerald key marker is barely visible on the map.~~ ~~Import the StS1 burning elite map icon.~~ Done in 0.11.0:
    StS1's six map flame frames, tinted green, burn behind the node, with the emerald gem beside it.
  - ~~The keys are too small in the top bar.~~ Done in 0.10.2 (cropped large sprites, 36 px).
  - ~~The Sapphire swap button looks bad.~~ Done in 0.11.0: the key gem itself is clickable, with the game's reward
    alternative button ("Take the Sapphire Key") under it for the controller.
- Sapphire in the chest: show the Sapphire key in the chest as a clickable choice next to the relics. In co-op, a player
  who loses the rock-paper-scissors while the key is still available chooses between the key and the relic instead of
  receiving the relic automatically.
  - 0.11.0, partial: the key shows in the opened chest, with a give-back swap after pickup.
  - Done in 0.12.0 (owner request 2026-10-31, replaces the swap): the rock-paper-scissors losers rematch for another relic
    until the last player chooses between the last relic and the Sapphire Key (0.12.8: any player may take the key from
    the first round); once the key is held, the last relic goes
    to them automatically as in vanilla. Solo: relic or key. Checklist: `docs/testing/sapphire-rematch-checklist.md`.
    0.12.1 (owner request): the key shows from the start as one more slot (N players: N relics + the key), dimmed
    for everyone until it is offered to the last player; in solo it is a choice from the start.
- ~~Strangle in co-op: also trigger on cards played by allies, not only its owner's.~~ Done in 0.10.2 for Strangle and Choke
  (same power), Custom Run co-op only so Standard/Daily stay vanilla (`StrangleRule`). ~~Minor: the power's vanilla text
  still says "you play a card".~~ 0.12.38: "Whenever any player plays a card" there (`StrangleCoopTextPatch`).
- ~~StS1 powers show "NOPE" as their floating text when applied (seen on Metallicize; every StS1 buff does it).~~ Done in
  0.10.2: "NOPE" is the game's missing-power image; the borrowed big-icon path is now seeded into the getter's cache field.
- Merchant Loans:
  - ~~The interest control also needs − buttons to lower the rate; interest ranges from 0 % (minimum) to 900 %.~~ Done in
    0.10.2 (amount −10/−5/−1/+1/+5/+10, rate −100/−10/−1/+1/+10/+100).
  - ~~Rebuild the loan panel from standard widgets, usable with a gamepad.~~ Done in 0.11.0 (`src/Ui/SpoonWidgets.cs`):
    focusable rows step with left/right like the Custom Run rows, buttons answer the game's confirm. Gamepad not tested
    in game; the rows are Godot controls styled like the game, not the game's own scene.
  - ~~Move the loan panel to the right side of the screen instead of the left.~~ Done in 0.10.2.
  - ~~Hovering the gold shows what is left to repay or collect.~~ Done in 0.11.0 (`GoldHoverPatch`).
- Character hue option: tint each character at 20 % opacity, player-chosen hue. Optionally tint the player's map
  drawings too. Refinement: compute the character's dominant color and tint only pixels near it within a threshold
  (e.g. turn the Silent's green toward turquoise at 70 % transparency); hue, threshold and opacity are player settings.
  - Done in 0.11.0 (`src/Tint/`): pause menu "Character Color" (Off / Color overlay / Dominant color only; hue, strength,
    threshold, map drawings), saved in the mod cfg, shared in co-op through the action queue. Applied like StS2's own
    monster hue variants: a shader material on the combat body. The dominant color is the character's map drawing
    color. Minors: ~~the map ping, rest site and merchant bodies are not tinted~~ (tinted since 1.0.1: `TintRestSitePatch`,
    `TintMerchantPatch`, `TintMapPingPatch`, which follows the map-lines option); lines already drawn keep their color;
    the tint replaces any normal material the character body had; co-op sync and the static Watcher body not tested.
    The rest are in-game checks (`docs/testing/fix-wave-checklist.md`).

## Deep backlog priority (owner, 2026-10-30)

- Character/Act 4 animations and in-game validation are deferred behind the remaining content work.
  Existing testing checklists remain pending; this priority change does not mark them passed. The content work is done
  (1.0.0); both stay with the owner: in-game checks need their clicks, the Spine conversion is done (1.0.1).
- Watcher relic plan: reuse native shared Cloak Clasp rather than duplicate it; Holy Water and Violet Lotus
  replace Archaic Tooth at Orobas when the Tooth's conditions fail. See `docs/review/character-relics.md`.
- Bugs and in-game test results: the owner logs them here later (none open beyond the crash report below).

- **Dusty Tome in StS1-only (0.12.4, owner call 2026-11-01):** Darv's Dusty Tome crashed the game (null Ancient pick: the
  StS1-only pool has no Ancient card). The Tome now reads the vanilla pool and gives a vanilla StS2 Ancient card. Not tested in game.

- **Act 4 saves unloadable (0.12.5):** the save writer drops empty id lists and `RoomSet.FromSave` then reads null; the
  Ending has no events or normal fights, so every co-op save taken after the third key was renamed `.corrupt` on reload
  (2026-10-31 14:40 and 15:24). `EndingRoomSetLoadPatch` reads missing lists as empty. Not tested in game.

- **Block Return icon (0.12.6):** Talk to the Hand's debuff showed the borrowed Thorns icon; it now shows StS1's
  `talk_to_hand` icon (`art.py --powers`, `Sts1PowerIcons`). Other StS1 powers still borrow vanilla icons. Not tested in game.

- **Ending combat background and heal (0.12.7):** the StS1 combat background was a full-rect child of the combat room's
  zero-size `BgContainer`, so the elite and Heart fights showed none; it is now a fixed box centred on that origin
  (size guessed from vanilla layer offsets, check edges and screen shake in game). Entering The Ending now heals like an
  act's Ancient (missing HP, 80 % of it under Weary Traveler; `EndingHealRule`). Not tested in game.

- **Sapphire Key from the first round (0.12.8):** the key is no longer dimmed until the last player: every player still
  choosing may vote for it while a relic is left. Several key voters play rock-paper-scissors for it on the chest's RNG
  (no fight animation for the key yet); the losers vote again among the relics left. The gem glows while it is the local
  vote and disappears once someone takes it. Not tested in game.

- **Rematch hands (0.12.9):** after a lost chest fight, the loser's hand kept its last rock, paper or scissors texture
  while voting again (vanilla never reopens a chest after a fight); it now points again. Not tested in game.

- **Watcher stance aura (0.12.18):** while in Calm, Wrath or Divinity a soft halo pulses behind her body and particles
  rise around her (calm: slow and sparse; wrath: fast and dense), in StS1's stance colors; gone in Neutral and on death.
  An approximation of StS1's `StanceAuraEffect` and `*ParticleEffect`. Not tested in game.

- **Watcher stance feedback (0.12.12):** entering Calm, Wrath or Divinity plays StS1's enter sound and holds its loop
  (Oggs copied from the jar by `make sts1-watcher-art` into `audio/watcher/`), plus StS1's colored screen-edge flash
  (sky, scarlet, pink). Godot players on her creature node, at the game's squared Master x SFX volume, updated when
  the sliders move; the loop stops on Neutral, death and combat end. In co-op every client hears every Watcher's
  stance (StS1 was solo-only). Not tested in game.

- **Watcher head icon (0.12.11):** the top-bar and stats icon (and its outline) is now her head, cut from a 2x render
  of the StS1 idle skeleton (`tools/sts1/art.py`, `HEAD_BOX` around the ground point) instead of the shrunken
  character-select button, which still feeds the map marker. Not tested in game.

- **Ancient Potion (0.12.10):** StS1's Ancient Potion (uncommon, gain 1 Artifact, may target an ally like StS2's
  Dexterity Potion) joins every random potion source (`PotionFactory.GetPotionOptions`: rewards, merchant, in-combat
  generation) in StS1-only runs, for every character. Hidden `Sts1PotionPool`, outside Standard, Daily, Hybrid and the
  potion lab; console: `sts1watcheritem potion STS1_ANCIENT_POTION`. Image borrowed from Liquid Bronze (StS1 potions are
  drawn from tinted shapes, not one image). Events that read the pools directly (The Legends Were True, Endless
  Conveyor) do not offer it. Not tested in game.

### Owner list, 2026-11-05 (triage; done items are marked with their version)

- Bugs: ~~Sapphire Key greyed out until the last player (owner wants it selectable at once, screenshot
  `~/Pictures/Debug Slay the spoon/Saphire key.png`)~~ (0.12.8); ~~rock-paper-scissors fingers wrong after a loss~~ (0.12.9); ~~Talk to the Hand's
  Block Return debuff icon not imported~~ (0.12.6); ~~Act 4 elite and Heart fights have no background; Act 4 second boss should heal 80 %~~ (0.12.7).
- 2026-10-31 15:23 "crash on color change": the log shows a state divergence on leaving Infested Automaton (host Watcher,
  client Ironclad with Souvenir, 0.12.3, no TintAction queued), then the client was dropped. Cause not found: needs the
  client's state dump (the host log only has its own). Owner item (2026-11-05): no client log or dump exists on this
  machine (only the host's renamed `.corrupt` saves); the next desync needs the client's `godot.log` from the friend.
  Re-analysis (1.0.1, 2026-11-05): the host's logs from that day are rotated out (oldest kept: 2026-11-01, a later
  session with no divergence). The 15:23 `.corrupt` save stops before the fight: act 2, modifiers Sealed Deck, Spire
  Souvenir and Act 4; host Watcher (8 HP after the Decimillipede elite, Wave of the Hand and Deus Ex Machina from a
  transform), client Ironclad; last rooms elite, treasure, rest site (both healed). The divergent combat is not in any
  file on this machine, so no cause can be named; nothing in the save points at the tint (no tint state is saved).
  Dropped by the owner (2026-11-05): the day's game patch changes the ground anyway; a new report needs both peers'
  `godot.log` from the same session.
- 2026-11-01 Corpse Explosion "-90 AOE": its power deals target max HP × stacks to every other enemy when the target dies
  (StS1 rule); three Knights died together. Looks intended unless players took damage. Settled: intended (owner, 2026-11-05, below).
- Watcher: ~~head icon (owner call: crop it from the static sprite)~~ done in 0.12.11; ~~stance-change sound~~ done in 0.12.12 (with a screen-edge flash); ~~StS1's stance
  particles and aura~~ approximated in 0.12.18 (pulsing halo behind her plus rising Godot particles in the stance color,
  not StS1's exact effects); ~~Spine animations are still missing~~ (done in 1.0.1, above).
- ~~Artifact potion back in StS1 mode (owner call: StS1-only mode, every character). StS2 has none: port StS1's
  Ancient Potion (uncommon, gain 1 Artifact).~~ Done in 0.12.10.
- Design calls settled with the owner (2026-11-05 Q&A; all implemented by 0.12.37):
  - ~~Souvenir balance: a per-run lean rolled on the seed, 60/40 toward StS1 or StS2 cards; rarity odds unchanged;
    duplicates keep the version button.~~ Done in 0.12.29 (`OfferWeights`, `OfferWeightPatch`): the favored game's unique
    cards hold 60 % of the two games' unique share within the rolled rarity; duplicate slots keep their count share and
    their 50/50 version roll. Discovery-like distinct draws (`TakeRandom`) stay uniform.
  - Campfire Sales replaces the fixed 50-gold gift: the seller sets 0–999 gold and 0–900% interest;
    the buyer accepts or declines. Available gold goes to the seller, and only the shortfall incurs interest.
    Debts share the merchant repayment ledger (or the campfire modifier's saved ledger when loans are disabled).
    Curses and unremovable cards stay; upgrades and enchantments transfer. Only a completed sale spends the
    seller's rest action. Native popups replace the custom loan/sale panels. In-game validation remains pending.
  - ~~Selected-card counter ("X / Y selected", EN/FR) on every multi-pick grid, vanilla modes included (display only).~~
    Done in 0.12.27 (`src/Ui/SelectionCounterPatch.cs`: "Selected: X / Y" under the prompt of the six grid screens).
  - ~~Cross-game combos: a hand-picked pair list, weight ×0.5 when a card would complete a pair the team holds. Pairs
    kept (team-wide): Terror + Dominate, Terror + Bully, Choke + Flurry of Blows; see `docs/review/hybrid-combos.md`.~~
    Done in 0.12.29 (same patch; rewards, merchants, Calamity-like generation and transforms).
  - ~~Starting relic "treasure chest" mode: a Custom Run modifier; at run start each player opens their own chest of 10
    relics (common, uncommon and rare from their pools, rolled on the seed) with the vanilla chest scene and opening
    animation, and keeps one; no rock-paper-scissors; the starter relic stays and Neow is unchanged.~~ Done in 0.12.35
    ("Starting Chest", `src/Config/StartingChestModifier.cs`). Owner call (2026-11-05): the vanilla "Choose a Relic"
    screen after the act's Spine chest opening, not the treasure room scene. Builder calls, for owner review: the chest
    is a Neow option like Draft's, so Neow offers its blessings too only with Neow's Blessings ticked; rarities roll
    as vanilla chests (50/33/17 %), on a per-player stream named after the modifier; the 9 relics not kept stay in the
    bag; the screen's Skip keeps nothing; relics Neow refuses (`IsAllowedAtNeow`) are left out.
  - StS1 special event relics come from their own StS1 events, ported like Vampires and Augmenter (act 2 injection).
    Order: ~~Golden Idol + Forgotten Altar + Moai Head~~ (0.12.30: `src/Events/Sts1GoldenIdolEvent.cs`,
    `Sts1ForgottenAltar.cs`, `Sts1MoaiHead.cs`; relics in `src/Relics/Events/`; StS1 base values, no A15 raises;
    Forgotten Altar's Humanity option is left out), then ~~Cursed Tome~~ (0.12.31: `src/Events/Sts1CursedTome.cs`;
    Necronomicon, Enchiridion, Nilry's Codex; Necronomicurse in a hidden curse pool; the book is obtained directly
    instead of StS1's reward screen; the Codex asks at the end-of-turn flush, like Well-Laid Plans), then ~~Accursed Blacksmith + Bonfire~~ (0.12.32: Warped Tongs, Spirit Poop, Pain in the
    hidden curse pool, not Eternal; builder calls: Bonfire counts Ancient as Rare, Status as Common, Event/Token/Quest as
    Special (heal 5); Leave shows only when no card can be offered), then ~~Mind Bloom~~ (0.12.33: `src/Events/Sts1MindBloom.cs`, Mark of
    the Bloom blocks every `CreatureCmd.Heal` except a co-op resurrection; builder calls: the event is shared (StS2
    enters event combats for the whole team), so the team votes and a Souvenir run needs every player to hold the relic;
    I am War fights a random StS2 act 1 boss with StS2's boss rewards plus a Rare relic, instead of StS1's 50 gold +
    Rare relic; I am Healthy is left out, since StS1 only offers it past floor 40).
  - ~~StS1 boss relics (Custom Run option): the act 2 and act 3 Ancients gain an option "Choose a boss relic among 3"
    that replaces the Ancient's own pick. In co-op the players who take it share a chest of 3 with rock-paper-scissors,
    otherwise like a regular StS2 chest. Neow does not offer it.~~ Done in 0.12.37: modifier `SpoonBossRelics`,
    `src/Relics/BossChest.cs`, rule `src/Core/BossChestRule.cs`. Pool: the 11 Darv natives (Darv's act and Pandora's
    Box rules), the 8 shared ports, and the character boss relics. Builder calls: the chest opens once every player has
    made their first choice at the Ancient; character boss relics enter a shared chest only when every taker plays that
    character; relics a taker already owns are left out; the result is told on the Ancient's last page, without the
    treasure room's rock-paper-scissors animation. Checklist: `docs/testing/boss-relics-checklist.md`.
  - Massive Scroll keeps its fallback to ordinary allowed cards when the co-op filter leaves none.
  - Corpse Explosion keeps StS1's rule (target max HP × stacks to every other enemy, chains included): the "−90 AOE"
    report is intended.
  - ~~The Architect gets a short EN/FR dialogue for the Watcher, in the tone of "you do not really exist" (a nod to
    Slay the Spoon being a mod), reusing a vanilla character's attack effect~~. Done in 0.12.28
    (`src/Watcher/ArchitectDialoguePatch.cs`, two repeating dialogues in `localization/{eng,fra}/ancients.json`).
    The owner still proofreads the text.
  - Duplicate tables (`docs/review/*-duplicates.md`) approved; doubtful cases settled: Discovery, Jack of All Trades,
    Secret Technique and Secret Weapon use the StS2 card in every mode (`Sts1Relation.Retired` since 0.12.26, classes
    kept for old saves); keyword-only duplicates keep the version choice; each version keeps its own game's rarity;
    the 13 StS1 cards whose StS2 namesake is an event or colorless card stay in their character pool in every mode.
  - Every "design call made without the owner" listed above is validated as is (owner, 2026-11-05); revisit one only
    when an in-game test shows a problem. The fidelity rule still wins over them.
- ~~2026-11-05 screenshot `~/Pictures/Debug Slay the spoon/unknown relic.png`: The Specimen shows raw
  `relics.STS1_THE_SPECIMEN.title/description` keys at a merchant.~~ 0.12.36: every StS1 relic has EN/FR text and art,
  checked by `tests/Sts1RelicTests.cs`.

- **Code review 1.0.4 → 1.0.5 (2026-11-12):** every finding fixed in 1.0.5 except one cleanup, kept: `SpoonCampfireGift`
  copies `SpoonMerchantLoans`' ledger field, `DeepCloneFields` override and saved-property decode/fallback almost line
  for line (`src/Config/CampfireGiftModifier.cs`). A shared base (e.g. `LedgerModifier`) would keep the two in sync;
  no player-visible effect, so it waits for the next change to either ledger.

- **Folie à deux (owner request, 2026-11-14):** new multiplayer mode where every card reward is bid on at a chest and
  ties are fought with the vanilla rock-paper-scissors; fourth host card (Standard + hidden flag, a new Standard
  exception) and a Custom Run checkbox. Design agreed: `docs/superpowers/specs/2026-11-14-folie-a-deux-design.md`.
  Implemented in 1.1.0 (branch `two-modes`); not tested in game: `docs/testing/two-modes-checklist.md`. Known gaps:
  - Card picks outside combat that are not card rewards (Brain Leech, Room Full of Cheese, Massive Scroll, Hefty
    Tablet, Lead Paperweight, Sea Glass) are bid on after the owner picks (`src/Folie/FolieCardPicks.cs`). The source's
    own "add to deck" then puts the cards in the winner's deck, so its preview plays on the owner's side; combat picks
    (potions, Discovery, Toolbox) stay vanilla.
  - Bids travel as the mod's own `FolieBidMessage` keyed by the chest, not through the vanilla treasure-room
    synchronizer; a chest interrupted by a reload is replayed from scratch.
  - A chest over another player's event pops over whatever screen the bidder is on; a winner of another player's
    custom reward gets a separate reward screen. Both need in-game checking.
  - The chest view is built from plain controls (heads, names, scaled card visuals), not the vanilla chest scene.

- **Matriochka (owner request, 2026-11-14):** Custom Run modifier: Hoarder, an unskippable card reward on every floor,
  Dolly's Mirror from the first merchant, forced Doll Room with Bing Bong, forced Reflections "Shatter" in act 3.
  Design: `docs/superpowers/specs/2026-11-14-matriochka-design.md`. Implemented in 1.1.0 (branch `two-modes`), with card
  reward rerolls per act set with the row's −/+ (0–10, default 3, per player); not tested in game:
  `docs/testing/two-modes-checklist.md`. Builder calls: non-combat rooms give their reward when the room is entered (the spec says when it ends);
  rerolls are counted per player.

- **King Midas (owner request, 2026-11-14):** Custom Run modifier `SpoonRoiMidas`: the gold relics at start, no card
  rewards, gold events only (gold options only), paid and repeatable rest site actions (50 + 50 per action at the same
  fire), Maw Bank "Reopen" at rest sites. Design: `docs/superpowers/specs/2026-11-14-roi-midas-design.md`. Implemented
  in 1.2.0 (branch `two-modes`); not tested in game: `docs/testing/roi-midas-checklist.md`.
  - Minor: the per-fire paid count is not saved; a reload mid rest site resets the price to 50.
  - Builder call: Endless Conveyor and Trial are left out (their gold is random); Matriochka is mutually exclusive.
  - To check: vanilla events drawn again after a visit (repeats allowed) may carry one-time assumptions.

- **Spoon (owner request, 2026-11-14):** Custom Run modifier `SpoonSpoon`: the Spoonful card (a Feed copy that stays),
  max HP relics in the normal draws, the Feast rest site option, Fruit Juice at every merchant, and a one-room Spoon act
  after the last act 3 boss when a player has 300+ max HP (before The Ending when it follows). Design:
  `docs/superpowers/specs/2026-11-14-spoon-design.md`. Implemented in 1.3.0 (branch `two-modes`); not tested in game:
  `docs/testing/spoon-checklist.md`.
  - **Drawing list (owner, later; every drawing is a placeholder today):**
    1. The Spoon combat sprite: PNG, transparent, about 400x600 px (600x900 overflowed the top bar in game, 2026-11-16), facing left (`images/spoon/spoon.png`). Open.
    2. The Spoon map boss icon: PNG 512x512 plus the same shape as an outline (`spoon_boss.png`,
       `spoon_boss_outline.png`). Open.
    3. Spoon act combat background: JPG 1920x1136 (`images/spoon/combat_bg.jpg`; optional). Open.
    4. ~~Spoon-Feed card portrait~~: the card is gone (1.3.7).
    5. Spoonful card portrait: same (`images/cards/spoon/spoon_spoonful.png`; optional). Open.
    Animations: light tweens on the static sprite (`src/Spoon/SpoonMotion.cs`, owner request 2026-11-16: rotations,
    swoop and rise); a drawn replacement sprite keeps them if its ground point stays at the handle tip.
  - **Restaurant ambience (owner request, 2026-11-16, open):** in the Spoon act, a light restaurant ambience (chatter,
    cutlery, glasses) under the music, with the boss music kept as the main track. Its level is set like a sound
    effect (follows the SFX volume, kept low in decibels so it never covers the music). Needs a free ambience recording
    and an `AmbientSfx`-style loop for `SpoonAct` (today Glory's).
  - **Test start balance (owner, 2026-11-16):** the bare test deck lost to the Spoon ("soit mon deck est nul ou la
    cuillère est OP"; the owner blames the deck). The test deck is now upgraded with seven combat relics; the Spoon's
    numbers stay a starting point until a real run reaches it. Second test (same day, upgraded deck): +3 Strength
    per turn reached 24 by turn 9 (Ladle at 74) with 1221/1500 HP left; replaced by the growth rule (spec, "Boss
    numbers"), 2000 HP per fighter, Bouchée 40, Ladle 70. Open: nothing on the Spoon explains the growth (no power
    tooltip); add one if players miss it. Third test (same day): the owner found three stacked scalings (team max HP ratio,
    Strength from growth, heal from stolen max HP) too much; the ratio is gone and Appetite (Spoonful removes it) added.
    The Appetite tooltip uses "{Amount}0 %" for the total; fine while the step stays 10 %.
  - **Art sources:** the placeholder art comes from two Pixabay photos under the Pixabay Content License (free use, no
    attribution required): "teaspoon" https://pixabay.com/photos/teaspoon-554065/ (spoon sprite, boss icon and outline,
    card portraits) and "exclusive banquet" https://pixabay.com/photos/exclusive-banquet-1812772/ (combat background,
    card portrait backgrounds). They are placeholders to replace with drawings.
    Cutlery portraits (2026-11-16, same license): Teaspoon https://pixabay.com/photos/spoon-blueberries-teaspoon-1567331/,
    Wooden Spoon https://pixabay.com/photos/wooden-spoon-wood-cook-spoon-4647800/, Skimmer (cropped from)
    https://pixabay.com/photos/kitchen-utensils-help-cook-cutlery-5142006/, Melon Baller
    https://pixabay.com/photos/melon-aqueous-red-fruit-marbles-4230629/.
  - **Decision log:** the act is appended like The Ending, not a second combat in the boss room or a map node;
    Spoon-Feed numbers (5 max HP) and the 300 threshold accepted by the owner; nothing eaten or given is returned after
    a win; losing to the Spoon is a defeat; boss numbers (about 1500 HP per fighter, Bouchée 30 + 10 max HP eaten,
    Coup de louche 50, +3 Strength per move) are a starting point, tuned after play; Spoon damage is scaled by the team's
    total max HP (`base x 300 x players / team max HP`); a run where nobody reaches 300 has no Spoon.
    Owner decision 2026-11-16 (replaces the review wave's "unplayable at 5 max HP or less"): Spoon-Feed is playable
    while the supporter has more than 1 max HP and gives min(5, max HP - 1) (`SpoonRule.GiftAmount`); the supporter
    never drops below 1 max HP, so vanilla `LoseMaxHp`'s damage step (unblockable, unpowered) leaves at least 1 HP, and
    the fighter gains exactly what was lost.
    Owner decision 2026-11-16 (1.3.7, replaces fighters, supporters and Spoon-Feed): everyone fights; the act needs the
    team's summed max HP at 300 / 550 / 750 / 900 for 1–4 players (`SpoonRule.TeamThreshold`). Spec amendment in
    `2026-11-14-spoon-design.md`.
  - ~~**Owner design call:** in an Act 4 run missing only the Ruby key, the Spoon act's rest site offers Ruby Recall, a 4th
    chance at the Ruby key, which then appends The Ending after the Spoon.~~ Settled 2026-11-18: kept.
  - **Deferred minors (all open; the final review may settle some):**
    - ~~T2: no top-bar icon or short text for `SpoonSpoon` (the default modifier icon shows, like King Midas).~~ Done in
      the 1.3.0 review fix wave: `SPOON_SPOON.short` ("Spoon" / "Cuill.") shows in the top bar, and King Midas shows its
      short text too.
    - ~~T3: relics added to the grab bags are not re-added when a refreshing bag re-reads its relics by rarity.~~ Gone in
      1.3.7: the 12 relics are given at the start and leave the grab bags.
    - ~~T3: `SpoonRelics.AddTo` skips a missing relic id or an unknown deque name silently (no log line).~~ Gone in 1.3.7.
    - 1.3.7 builder call: with the 12 relics from the start, Neow (Nutritious Oyster, Stone Humidifier) and vanilla
      events (Hungry for Mushrooms, Room Full of Cheese, Trash Heap, Fake Merchant, Nonupeipe) can still give a second
      copy; kept, since more max HP fits the mode. A missing relic id is skipped silently at the start grant.
    - T4: the floor counter skips a number in the Spoon act (the empty grid row; cosmetic).
    - T4: `SpoonAct.BaseNumberOfRooms` is 1, which multiplayer reads as 0 (continue-run info only).
    - T4: `EndingGeneratedMapPatch` builds a throwaway fixed map just to compare map types, and its log line says
      "act4:" for the Spoon act too.
    - T4: free travel (Winged Boots, Flight) on the Spoon map is handled by `SpoonMapTravelPatch`; check it in game.
    - ~~T5: Pressure Points played by a supporter still fires the marks a fighter applied (dealer-less).~~ Done in the
      1.3.0 review fix wave: a supporter's Pressure Points skips the Spoon's mark; other enemies trigger as usual.
    - ~~T5: Spoon-Feed can create max HP: a supporter under 6 max HP loses less than 5 (`LoseMaxHp` floors at 1) while the
      fighter gains 5.~~ Done in the 1.3.0 review fix wave: Spoon-Feed is unplayable at 5 max HP or less
      (`SpoonRule.CanGift`), and the fighter gains what the supporter actually lost (`SpoonRule.GiftTransferred`).
      Superseded by the owner decision of 2026-11-16 (decision log above).
    - ~~Final review: the Fruit Juice merchant entry drew `PlayerRng.Shops` once more (`CalcCost`).~~ Done in the 1.3.0
      review fix wave: `SpoonJuiceCostPatch` prices the Juice from the mod's `SlaySpoon.Spoon.Juice_*` stream.
    - ~~T5: skipping `HauntPower.AfterCardPlayed` for a supporter also skips its `Rng.CombatTargets` draw (deterministic,
      but the vanilla stream use differs).~~ Gone with the supporters (1.3.7).
    - T5: the Spoon's move-roll stream is named `SlaySpoon.Ending.SPOON_MONSTER` (naming only).
    - ~~T5: the Spoon-Feed arrow can rest on a supporter or a summon; the play is refused silently (UX).~~ Gone with the supporters (1.3.7).
    - T5: `SpoonMonster`'s hooks are not gated on `SpoonSpoon.IsActive` (matters for a
      console-spawned Spoon only).
    - ~~T5: the `GetPossibleTargets` postfix relies on the private method not being inlined; accepted, because the
      multiplicative hook zeroes a stray hit anyway.~~ Gone with the supporters (1.3.7).
    - Final review: `SpoonMonster`'s hooks gate on the monster, not on `SpoonSpoon.IsActive` (see T5 above; console
      spawn only).
    - Final review: fighter pets (Osty) keep vanilla damage; the team-max-HP formula scales damage to players only.
    - Final review: `SpoonRule.DamageFactor` and the Monster Damage % modifier each truncate, so a hit can land ±1 HP
      off the exact product when both apply.
    - Final review: the Feast text hardcodes 5 instead of reading `SpoonRule.FeastMaxHp` (Spoon-Feed is gone, 1.3.7).
    - Final review: the Spoon act has no entry heal like The Ending's; its row-0 rest site covers it.
    - 1.3.2: Stolen Spoonful's portrait is a copy of Spoonful's (drawing list); its Appetite texts hardcode 5 %, 2, 1
      and 3 like the other Spoon texts.
    - Balance play test (2026-11-16, seed GPHXW6YEPE, won in 18 turns): the test deck's blocks (Barricade, Entrench,
      Body Slam, Impervious) let the Ladle pierce once; Appetite went −4 per cycle (Teaspoon, Stolen 3, Spoonful) and
      Melon Baller took Strength from +3 to −6. Owner: keep the counter deck as the test deck, no Baller change; retest
      after the Stolen Spoonful merge (Appetite now −1 per cycle).

## Game 0.111.1 follow-ups (2026-11-18)

- Design call (owner): the new Custom Run "Randomize" button draws 2 good + 1 bad vanilla modifiers
  (`ModifierModel.Pick2Good1Bad`, from `ModelDb.GoodModifiers`) and unticks every other row, ours included.
  Kept as is for now; our modes could join the draw through a postfix if wanted.
- ~~Double first-launch popup (ours and the game's save copy)~~: done in 1.3.8, the mod's progress copy and popup are
  removed; the game copies unmodded saves once (owner, 2026-11-18).
- Minor: 0.111.1 lets most potions target another player. Ours stay `Self` (Watcher stance potions need the
  Watcher); Bottled Miracle could become `AnyPlayer`.
- Testing: a Workshop copy with a greater version now wins over a local `mods/SlaySpoon`; keep the local
  version at least equal to the uploaded one when testing with `make up`.

## Custom Run modes picked by the owner (2026-11-18)

All seven are implemented in 1.5.0 (spec `docs/superpowers/specs/2026-11-18-backlog-modes-design.md`, checklist
`docs/testing/backlog-modes-checklist.md`). The owner's original wording, kept for reference:

- **Blood:** gold no longer exists. Everything is paid in current HP: at the merchant, at events and at the campfire. A
  vampirism relic is given at the start.
- **Deck of 10:** the deck is locked at 10 cards. Taking a card forces you to discard one, and each discarded card gives
  +1 max HP.
- **Mutation:** at the end of each act, every card that is not upgraded is transformed at random.
- **Trophies:** each elite killed gives a card drawn from its own attacks, for example "Gremlin Nob's Strike".
- Co-op **Forced Trade:** after each combat won, each player gives one card from their deck to a random player.
- Co-op **Conductor:** a random player, different each combat, hands out the end-of-combat rewards. The players' rewards
  are merged.
- **Useless Relics (owner, 2026-11-18):** in combat, a relic that has already triggered and cannot trigger again this
  fight (for example Akabeko after its first attack) can be thrown at an enemy for a fixed 10 damage. Thrown relics come
  back to the inventory at the end of combat; until then the relic is greyed out like a melted wax relic (checked in 0.111.1: a melted wax relic stays in the inventory, `RelicCmd.Melt` sets `Status = Disabled` and `UpdateTexture` tints it dark red; a thrown relic can reuse the disabled status and the tint without being wax). Design calls: which relics count as "spent" (a per-relic list or a
  generic signal), how to throw (click on the relic, then a target), and co-op sync.
- **Compatibility (owner, 2026-11-18):** each new mode must be checked against every existing one before it ships, for
  example Forced Trade with Folie à deux (both move cards between players after a fight). Each design pass lists the
  pairs that clash and decides: combine, order the effects, or make them mutually exclusive.

## Backlog modes (1.5.0, 2026-11-18)

- Deck of 10 relic (1.5.1): Neow's cursed slot, 1 in 11, is a builder call (the owner chose "a Neow option"); also the
  keep grid only for the first trim. Both in the spec; the owner may change the odds or make it a fourth option.
- Minor: Conductor assigns rewards through one panel per reward, then each player's own reward screen opens; the spec's
  merged screen was not built (see the spec's builder calls). Gold is not split.
- Minor: Trophies read the elites' moves at combat start; a move whose damage depends on state later in the fight shows
  its start-of-fight value.
- Minor: Useless Relics uses a hand list (`UselessRelicRule.OncePerCombat`); mod relics and future vanilla relics are not
  on it. A thrown relic's hover tip doesn't say it was thrown (only the grey tint shows it).
- Minor: Useless Relics throws are not saved; the game restarts a fight on reload, so nothing is lost.
- Minor: Blood shows the spendable gold equivalent in the top bar; no HP-cost text on merchant prices.

## The Forge (1.4.0, 2026-11-18)

- Minor: in a Forge run every fusable card's title shows its level as a number ("Strike+1"), because the game names
  cards that way once their max level is above 1. Kept: it shows the level the fusion uses.
- Minor: the preview cards of a cancelled fusion stay registered in the run's card scope until the run ends, as the
  Hybrid version swap does. Harmless (not in any pile, not saved).
- Minor: the other-version result goes through `CardCmd.Transform`, so relic hooks on cards added to the deck
  (`ModifyCardBeingAddedToDeck`, e.g. eggs) can apply to it. Kept: a fusion that changes the card is a new card.
- Check in game: the Duplicate service runs its synced deck choice inside a queued action (vanilla removal uses a
  one-off message). If the merchant freezes in co-op, move it to a message like `MerchantCardRemovalMessage`.
- The curve table is generated (`tools/forge_curves.py`, run with the 0.111.1 decompile mounted as `/sts2src`);
  every card's hand call lives in `data/forge/cards.json` (792 cards). Owner review: `docs/review/forge-curves.md`.
- Check in game: `powerBonus` with a negative amount (Malaise's Strength loss), Wish's option cards counting as the
  card source, Time's Up's fractional `ExtraDamage` step (0.25) and Friendship's Strength loss crossing 0 at +2.

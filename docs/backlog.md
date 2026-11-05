# Backlog

Remaining design decisions and deferred work after the 0.10.1 fix wave (2026-10-30).
Completed work is recorded in CLAUDE.md; in-game checks remain in docs/testing/fix-wave-checklist.md.

- StS1 character-specific relics (Ironclad, Silent, Defect, Watcher pools) that StS2 dropped, offered when a StS1 card
  set mode is active (owner request, 2026-10-29). Inventory completed 2026-10-30:
  `docs/review/character-relics.md` and `data/duplicates/character-relics.json`.
  First slice in progress, uncommitted in the main working copy: Champion Belt, Magic Flower, The Specimen.
  This work is outside the fix wave and is not included in its Workshop package.
  Owner decisions (2026-10-30): the boss relics are activated by a Custom Run option; special event relics are imported
  but not implemented, and how to obtain them is decided later.
- Act 4 keys (step 6 WP3), minors left after review:
  - ~~A co-op Sapphire swap waits up to 15 s for its pickup.~~ 0.12.0: the swap is gone (chest rematch instead).
  - ~~The Sapphire button position is hard-coded (650, 770).~~ 0.12.1: the key takes a free slot of the chest scene
    (left with 2 relics, right with 3); with 4 relics it uses a hand-picked fifth slot at (220, −180) from the chest
    center, which may sit close to the right-hand relic or a hand. Solo: 120 px right of the relic.
  - The Ruby icon comes from a postfix on the private `RestSiteOption.IconPath` getter, which the JIT may inline.
  - `StandardCharacterSelectPatch` sets `Pending` for any game mode; `Act4UnlockPatch.Record` logs failures at info.
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
  - Static sprites rendered from the StS1 setup pose, not animations.
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
  - The act's French title is StS1's "La fin"; StS2's French act titles capitalize the noun ("La Gloire"), so "La Fin"
    would match the game's style.
- Act 4 The Ending (step 6 WP4-6), minors left after review:
  - The Invincible clamp sits in `Sts1InvinciblePower`, not in the tested `HeartInvincibleRule`; it flashes inside the modify
    hook (vanilla flashes after modifying, and the overkill path can call the hook twice).
  - `PoisonTicksDrainTheSameBudget` only exercises unblockable damage; `BlockedHitsAndExhaustedBudget` is vaguely named.
  - `Sts1InvinciblePower.AfterApplied` takes the turn cap from the current amount: a mid-turn re-apply would use the
    depleted value.
  - `EndingMoveBrain` tests are weak: they compare two brains or re-read getters instead of asserting an expected move
    sequence. `EndingNumbers` tests miss the low side of some thresholds; `HeartBuffRule.Extra` accepts a negative count.
  - `ending_art.py` ignores atlas `offset`/`orig` (fine for the three StS1 atlases, wrong for a trimmed one); its additive
    blend is untested and not Spine-exact; triangle edges are not antialiased; no grandparent or shear bone test.
  - Code-built monsters have no death animation length: their fade may be cut short.
  - Mod elite wins count toward the vanilla 15-elite epoch check (modded profile only).
  - Debilitate skips dead players: a player revived later in the fight never gets those debuffs or statuses. In co-op
    the status intent shows 5, the per-player count.
  - The Heart's status shuffles share its RNG with its move roll, so Blood Shots vs Echo on turn 2 depends on the
    number of players and their draw piles (deterministic on every peer, seeds stay vanilla).
  - The Ending act's rooms are seeded from the caller's Rng seed in `EndingRoomsPatch` but from the run seed in
    `Sts1EndingAct.CreateForRun`: the same result today (one elite, one boss), divergent if a list ever grows.
    `FillRooms` does not reset the ancient or visit counters (only called on a fresh act). No automated test covers the
    act model or its patches (game-bound; the checklist covers them).
  - Visiting the act may add `STS1_ENDING_ACT` to the modded profile's discovered acts (not checked). If a later game
    build fills in `CheckForDefeatedAllEnemiesAchievement`, its achievement lookup would throw for this act.
  - The Ending map: with only three grid rows its four nodes sit far apart (check in game); after the start rest site
    the map marker is not drawn on it (vanilla skips the start row); Golden Compass bought in act 4 leaves
    `GoldenPathAct = 3` set (harmless). The `ModifyGeneratedMap` postfix discards late-listener edits on a fresh act 4
    map (none in vanilla; another mod's quest marks would be lost). If `ActModel.CreateMap` is inlined the prefix is dead
    and the postfix logs "replaced by the Ending map". Only the layout constants are unit-tested.
  - Run history stores the Ending act's id: with the mod removed, history shows a placeholder act; run metrics
    (`ActWinMetric`) would carry the mod's act id if the game uploads metrics for modded runs (not checked).
  - The `Act4Preview` cfg key is a no-op since the release (`Released || preview`); the key and the parameter can go.
    `ModifierListPatch`'s `OfferedIn(Custom)` guard is always true at runtime (the list is Custom-only) and reads the
    pure list by string name.
  - The Act 4 row in Custom Run has no icon (check how it renders).
  - The Invincible budget test only checks `InitialCap`'s signature; nothing tests that the applied power is not scaled
    in co-op (the power is game-bound).
  - Killing the Heart logs two vanilla errors from `ProgressSaveManager.ObtainCharUnlockEpoch` (`Act 4 is not yet
    implemented.`, `EpochModel was not found :(`): it switches on the act index for the character epochs and has no act 4
    case. Harmless (no epoch, no write); left alone because routing it to act 3 would hand out an epoch.
- The Watcher (step 7), design calls made without the owner (change any of them):
  - She never enters `AllCharacters`: no Standard, Daily or random-character Watcher.
  - Spire Souvenir: she has no StS2 cards, so "Hybrid" for her is her StS1 pool plus Hybrid colorless.
  - Solo ascension is capped by the highest ascension the player has on any vanilla character.
  - Her playtime goes into the modded profile; she never counts toward vanilla epochs.
  - Merchant, multiplayer hands and map marker borrow vanilla images until animation work; her FMOD sounds are silent
    or borrowed. Potion icons are borrowed vanilla icons (StS1 potions have no single image to cut).
  - The Architect has no dialogue for her: no attack scene at the victory, and `GetArchitectAttackVfx` is unused for now.
  - Discipline and Unraveling are not ported (StS1 never registers them). Cloak Clasp is not ported: she finds the
    native shared Cloak Clasp.
  - She may obtain Archaic Tooth through the vanilla rule (Prismatic Gem or a transform can put Bash, Neutralize, Unleash,
    Falling Star or Dualcast in her deck); no guard.
  - A host that unticks the modifier and readies in the same moment can start a run with her and no StS1 set (race);
    the Spire Souvenir may skip her (`HasSts1Cards`), which is harmless since her pool has no StS2 versions.
  - Vault in co-op: an extra turn for the player who played it only.
  - Lesson Learned uses the combat card selection RNG and upgrades
    its card directly when the kill ends the combat (no preview effect).
  - Foreign Influence draws from the attack pools of the current card set (character pools only, 55/30/15 rarity roll on
    the combat card generation RNG), no colorless attacks. Owner-approved StS2 rule for cross-character generation.
- The Watcher (step 7), minors left after review:
  - Signature Move's playability and glow match StS1, but the game displays its generic unplayable message:
    CardModel exposes only IsPlayable, with no per-card message override. A dialogue patch is deferred.
  - Duality's power has no `powers.json` text of its own (it extends vanilla `TemporaryDexterityPower`).
  - Character shell: `OrbModel` takes the energy prefix from the pool title `sts1_watcher`; `WatcherAssetPatch`'s Layer log
    message is misleading when the node exists with another type; `NGameOverScreen` reads `CharacterStats[her id]`
    unguarded (the entry is created in memory, the checklist covers first death and victory).
  - Custom Run button: the static `_previous` relies on vanilla selecting child 0 on open; selecting the fallback while
    readied is unverified; tests hard-code modifier id strings.
  - Progress guards: two classes are named `WatcherProgressPatch` (Characters and Saves; rename the Saves one, e.g.
    `WatcherVanillaGuardsPatch`); the epoch-count transpiler drops the labels of the replaced `get_Count`; `VanillaIds()` is
    rebuilt per call; no log when the vanilla ascension cap applies; `VanillaCount` and `MaxAscensionFor` are untested
    (game-bound).
  - Mark and Pressure Points HP loss has no dealer (matches StS1); "on kill by player" effects see no killer.
  - Wave of the Hand applies Weak through a throwing choice context (`AfterBlockGained` gives none); harmless unless a
    hook on Weak ever opens a choice.
  - Mental Fortress, Empty Fist and Flurry of Blows show all three stance tips (Calm, Wrath, Divinity), not only the
    stances they name.
  - Spirit Shield shows no live Block; Mark and the end-turn death effect share the Doom icon.
  - Lesson Learned's combat-ending upgrade shows no card-upgrade preview effect.
  - `LastCardPlayed` reset between combats (Sanctity) is unverified.
  - Flurry/Weave and Sands of Time/Windmill Strike share small logic blocks that could be helpers on `Sts1WatcherCard`;
    Consecrate lacks a `<summary>`; mixed `ThrowIfNull` vs `Target!`.
  - Unused `using` blocks in card and power files, empty `OnUpgrade` bodies, a stray blank line in Deus Ex Machina.
  - Weak tests: pure-logic coverage of `MantraTracker` keying, retain gating and Lesson Learned selection; power tests are
    mostly arithmetic and `WatcherPowerTextTests` greps source; stale pytest name `test_pending_ports_never_enter_runtime`; Task 9's tests were written after the code (no red run).
- The Watcher (step 7), deep backlog: Spine animations for her (combat, rest site, merchant, multiplayer hands, map marker)
  and in-game validation of everything in `docs/testing/step-7-checklist.md`. A "Deep backlog priority" section, if one
  exists on `main`, keeps its own wording; Watcher animations stay at the bottom of it.

- Fidelity rule (owner, 2026-10-30): StS1 character cards and StS1 content follow StS1 behavior wherever StS2's engine
  allows; StS2 conventions apply to colorless cards and cross-character cards. Design calls above that chose StS2
  behavior for character cards are to be brought back to StS1.

## Owner requests (2026-10-30)

- New game mode: pick the starting relic among 10 random choices.
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
  (same power), Custom Run co-op only so Standard/Daily stay vanilla (`StrangleRule`). Minor: the power's vanilla text
  still says "you play a card".
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
    color. Minors: the map ping, rest site and merchant bodies are not tinted; lines already drawn keep their color;
    the tint replaces any normal material the character body had; co-op sync and the static Watcher body not tested.

## Deep backlog priority (owner, 2026-10-30)

- Character/Act 4 animations and in-game validation are deferred behind the remaining content work.
  Existing testing checklists remain pending; this priority change does not mark them passed.
- Watcher relic plan: reuse native shared Cloak Clasp rather than duplicate it; Holy Water and Violet Lotus
  replace Archaic Tooth at Orobas when the Tooth's conditions fail. See `docs/review/character-relics.md`.
- Bugs and in-game test results: the owner logs them here later.

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
  client's state dump (the host log only has its own).
- 2026-11-01 Corpse Explosion "-90 AOE": its power deals target max HP × stacks to every other enemy when the target dies
  (StS1 rule); three Knights died together. Looks intended unless players took damage.
- Watcher: head icon (owner call: crop it from the static sprite); stance-change animations and sound missing.
- ~~Artifact potion back in StS1 mode (owner call: StS1-only mode, every character). StS2 has none: port StS1's
  Ancient Potion (uncommon, gain 1 Artifact).~~ Done in 0.12.10.
- Design, to discuss with the owner (do not implement yet): Souvenir reward balance (fewer commons; per-run lean
  toward one game's cards); campfire card trade/gift option for 50 gold per taker (−50 each on a trade);
  selected-card counter on Sealed Deck-style grids; Hybrid/Souvenir cross-character combos (2026-11-05, e.g. the
  Silent's Terror, +99 Vulnerable, with the Ironclad's Dominate, Strength per Vulnerable): not forbidden, but rarer
  as a pair.
- 2026-11-05 screenshot `~/Pictures/Debug Slay the spoon/unknown relic.png`: The Specimen shows raw
  `relics.STS1_THE_SPECIMEN.title/description` keys at a merchant. It comes from the uncommitted character-relic work
  (never in a packaged build from `main`): its text must land in `relics.json` (EN/FR) before that tranche ships.

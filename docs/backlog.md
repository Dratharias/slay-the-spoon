# Backlog

Known gaps deferred from step 1's final review. Fixed when the author feels like it.

- ~~The Watcher (StS1 character) as a playable character when a StS1 card set mode is ticked in a
  Custom Run (owner request, 2026-10-29).~~ Done in 0.10.0 (step 7): playable in Custom Run with a StS1
  card set, static visuals. In-game validation and animations are deep backlog.
- Act 4 with the StS1 keys (Ruby, Emerald, Sapphire): permanently unlock by first reaching Act 3
  in a Standard run at Ascension 10, whether that run is won or lost. Once unlocked, Act 4 is
  available at every ascension level, including 0. A Custom Run option bypasses the prerequisite
  (owner clarification, 2026-10-29). Daily runs remain unchanged.
  Act 4 shipped in 0.9.0; the source import plan is `docs/superpowers/plans/2026-10-29-step-6-act4.md`.
- StS1 character-specific relics (Ironclad, Silent, Defect, Watcher pools) that StS2 dropped, offered when a StS1 card
  set mode is active (owner request, 2026-10-29). Inventory completed 2026-10-30:
  `docs/review/character-relics.md` and `data/duplicates/character-relics.json`.
  First proposed slice: Champion Belt, Magic Flower, The Specimen. The eight missing boss relics are approved
  for porting as inactive content: no acquisition or random generation for now (owner, 2026-10-30).
  Owner decisions (2026-10-30): the boss relics are activated by a Custom Run option; special event relics are imported
  but not implemented, and how to obtain them is decided later.
- Act 4 keys (step 6 WP3), minors left after review:
  - A co-op Sapphire swap waits up to 15 s for its pickup: a peer whose pickup never arrives refuses after the wait while
    the others apply the swap (logged as a warning).
  - The Sapphire button position is hard-coded (650, 770); anchor it to the chest's relic row.
  - The Ruby icon comes from a postfix on the private `RestSiteOption.IconPath` getter, which the JIT may inline.
  - The Sapphire heal revert sets HP directly: no HP-lost entry in run history, no sound.
  - `StandardCharacterSelectPatch` sets `Pending` for any game mode; `Act4UnlockPatch.Record` logs failures at info.
- Act 4 keys (step 6 WP3), design calls made without the owner (change any of them):
  - Sapphire give-back undoes the pickup heal (never below 1 HP) and added potion slots; a relic whose effect has no
    safe undo gets no Sapphire button.
  - Ruby, Emerald and Sapphire text, key sprites at 25 % opacity in the top bar while missing.
  - Emerald elite buff: Strength act+2, +25 max HP, Metallicize 2·act+4, Regenerate 2·act+3 (one of them, rolled).
  - The cfg holding the permanent unlock is never overwritten when unreadable; bool and number values are accepted.
- Act 4 The Ending (step 6 WP4-6), design calls made without the owner (change any of them):
  - Static sprites rendered from the StS1 setup pose, not animations.
  - Monster visuals and the elite slot layout are built in code, not as scenes.
  - The Ending is added to the run when the team takes its last key, so the act 3 boss gives normal boss rewards.
  - The Ending uses Glory's music, map, rest site and combat backgrounds; StS1's ending scene images are unused.
    Owner decision (2026-10-30): no ending cutscene (the Necrobinder and the Regent have none) and no end credits; the
    rest of StS1's Ending content replaces Glory's where StS1 has it (fix wave Task 2).
  - Map: rest site → merchant → elite → Heart, no ancient, no event.
  - The Spear applies Surrounded (Shield Back Attack Left, Spear Back Attack Right).
  - Co-op: Debilitate, Bash's debuff and Burn Strike's Burns hit every player; Beat of Death hits the card's player;
    Smash's Block equals its highest damage to one player.
  - The Monster HP % does not change the Heart's Invincible budget.
  - Move names (StS1 has none): Bash/Heurt, Fortify/Fortifier, Smash/Fracas, Burn Strike/Frappe ardente,
    Piercer/Perceur, Skewer/Embrocher, Debilitate/Affaiblir, Blood Shots/Tirs sanglants, Echo/Écho, Buff/Buff (vanilla's French move title, not
    the planned "Renforcement").
  - Act 4 elite rewards are vanilla elite rewards.
  - Statuses shuffled into the draw pile take their position from the monster's own RNG.
  - The StS1 ending stinger plays once at the victory room (dropped if it clashes with the game's music).
  - Neow ignores the Act 4 Custom Run option.
  - Co-op: Shield and Spear get vanilla's multiplayer Artifact bonus (+1 per extra player), as every StS2 enemy does.
  - The Heart's sprite is the setup pose without IK/path constraints: its three top vents look folded.
  - The Heart's Painful Stabs buff applies 1 stack (StS1's data gives no amount).
  - The fixed map is built when the act's map is generated and forced over Golden Compass's map.
  - A10 with all three keys: the first act 3 boss also gives boss rewards once act 4 is appended (vanilla gives none to
    either act 3 boss when act 3 is last). Owner decision (2026-10-30): only the second act 3 boss rewards (not
    implemented).
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
  - Smash's Block counts the damage actually dealt (blocked + unblocked), not overkill; StS1 used the pre-block damage.
  - The French loss line "pris en tenaille par Bouclier et Lance de la Tour" has no article.
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
  - Routing's WinTime repair misses one sequence that needs both live callers to fail first: act 3 boss counted as final,
    save on the reward screen, resume: the load postfix appends act 4 but does not reset `WinTime`, so the run clock stays
    frozen at the act 3 time. No unit test for `EnsureEndingAct` idempotence (game-bound).
  - The ending stinger ignores the game's volume sliders (Godot player, not FMOD), keeps playing if the player quits to
    the menu during it, and its `EnterNextAct` prefix has no try/catch (the postfix side does).
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
  - Holy Water and Violet Lotus are ported with their effects (Holy Water replaces Pure Water with three Miracles) but have
    no acquisition source yet. Owner decision (2026-10-30): they replace Archaic Tooth in Orobas's offer when the Tooth's
    conditions are not met: one of the two at random (event RNG), for the Watcher only (not implemented).
  - She may obtain Archaic Tooth through the vanilla rule (Prismatic Gem or a transform can put Bash, Neutralize, Unleash,
    Falling Star or Dualcast in her deck); no guard.
  - A host that unticks the modifier and readies in the same moment can start a run with her and no StS1 set (race);
    the Spire Souvenir may skip her (`HasSts1Cards`), which is harmless since her pool has no StS2 versions.
  - Vault in co-op: an extra turn for the player who played it only.
  - Brilliance shows a calculated damage preview text; Ragnarok hits random enemies (`RandomEnemy`); Signature Move gives
    no can't-play message; Fear No Evil enters Calm on a kill only.
  - Block Return gives Block on any powered player attack; Lesson Learned uses the combat card selection RNG and upgrades
    its card directly when the kill ends the combat (no preview effect).
  - Halt gives its two Block gains separately (as Juggernaut triggers twice); Wave of the Hand expires at the end of the
    side's turn, StS1 at the end of the round.
  - Foreign Influence draws from the attack pools of the current card set (character pools only, 55/30/15 rarity roll on
    the combat card generation RNG), no colorless attacks.
  - Omniscience's copies Exhaust (StS1 purges them), so on-exhaust triggers and Exhume see them.
  - Battle Hymn stops adding Smites once no enemy is alive (StS1: "basically dead" check).
  - Master Reality is a custom power that upgrades cards the owner creates in combat; other players' cards are untouched.
  - Rushdown's English upgrade text has no DEPRECATED branch; Duality's power has no `powers.json` text of its own
    (it extends vanilla `TemporaryDexterityPower`).
  - Perseverance's French text keeps a duplicate "Retenue." keyword line.
- The Watcher (step 7), minors left after review:
  - Character shell: `OrbModel` takes the energy prefix from the pool title `sts1_watcher`; `WatcherAssetPatch`'s Layer log
    message is misleading when the node exists with another type; `NGameOverScreen` reads `CharacterStats[her id]`
    unguarded (the entry is created in memory, the checklist covers first death and victory).
  - Custom Run button: the static `_previous` relies on vanilla selecting child 0 on open; selecting the fallback while
    readied is unverified; tests hard-code modifier id strings.
  - Progress guards: two classes are named `WatcherProgressPatch` (Characters and Saves; rename the Saves one, e.g.
    `WatcherVanillaGuardsPatch`); the epoch-count transpiler drops the labels of the replaced `get_Count`; `VanillaIds()` is
    rebuilt per call; no log when the vanilla ascension cap applies; `VanillaCount` and `MaxAscensionFor` are untested
    (game-bound).
  - Block Return gives no Block on the killing hit (StS1 does) and ignores pet (Osty) attacks.
  - Master Reality upgrades Foreign Influence's pick and has no Status/Curse filter (StS1 upgrades only temp-card, Discovery
    and colorless-choice cards).
  - Establishment discounts cards kept by Runic Pyramid or Ringing Triangle (StS1 only Retain); its clamp reads this-turn
    cost modifiers, so a retained card that is free this turn gets no combat discount.
  - Deva Form applies 1 instead of its Magic var.
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

## Deep backlog priority (owner, 2026-10-30)

- Character/Act 4 animations and in-game validation are deferred behind the remaining content work.
  Existing testing checklists remain pending; this priority change does not mark them passed.
- Watcher relic plan: reuse native shared Cloak Clasp rather than duplicate it; Holy Water and Violet Lotus
  replace Archaic Tooth at Orobas when the Tooth's conditions fail. See `docs/review/character-relics.md`.
- Bugs and in-game test results: the owner logs them here later.

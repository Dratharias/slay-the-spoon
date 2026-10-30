# Watcher spike (Step 7, Task 1)

Can StS2 run a sixth character that is not in `ModelDb.AllCharacters`? This page lists what Task 1 built, what is
proven only by reading the decompiled game and building, and what the owner must check in game. Nothing here has been
run in game yet; the matching checkboxes are in `docs/testing/step-7-checklist.md`.

## How to start a Watcher run

`make up`, launch the game and open Custom Run. Tick "Slay the Spire 1 Cards" or "Spire Souvenir": her button appears
at the end of the character row (Task 2, `src/CustomRunUi/WatcherButtonPatch.cs`). The temporary `sts1watcher` console
command Task 1 used is gone.

Until Task 7 ports her cards, card rewards, shops and random card generation draw from the Ironclad's pool
(`WatcherSpikeRewardsPatch`, since deleted): her own pool holds only the four Basic starter cards. Pure Water has its text but no
effect until Task 6.

## Asset paths and the fallback chosen for each

The game derives most character assets from the id (`sts1_watcher`) in non-virtual getters. A mod PCK can add images
but not Spine skeletons or FMOD events, so every Spine scene and sound comes from a donor character.

| Asset | Where it shows | Source | Risk |
|-------|----------------|--------|------|
| Combat body | combat, game over | `CreateVisuals` replaced by a static `Sprite2D` scene built in code (`EndingVisuals`, as for the Act 4 monsters) | Low: same path as the Act 4 enemies, but a player creature with no Spine is new |
| Corpse | death in combat | texture swap in a `StartDeathAnim` postfix; revive swaps back | Medium: corpse placement (origin 256,512) is a guess |
| Death sound | death in combat | Silent's death event, played by the same postfix (NCreature only plays it with a Spine animator) | Low |
| Attack / cast / death SFX | combat | Silent's FMOD events | Low |
| Select / transition SFX | run start | Silent's (`silent_select`, `wipe_silent`) | Low |
| Top bar icon | top bar, run history, stats | `Icon` builds a full-rect `TextureRect` with our PNG; the Silent's icon scene is only preloaded | Medium: sizing in each container |
| Icon outline | top bar hover/selection | our PNG | Medium: the StS1 source is opaque, so the outline is a full white square |
| Char select icon / locked icon | character select only | our PNGs | Unused until she is listed |
| Map marker | map | our PNG (virtual `MapMarkerPath`) | Low |
| Energy counter | combat | Ironclad's scene; its five `Layer1..5` get our orb textures | Medium: if the scene has no such node names, the log says `Watcher energy counter: no LayerN` and the Ironclad orb stays |
| Card trail | card play | Necrobinder's (purple) | Low |
| Rest site | rest site | Silent's scene; its Spine nodes are removed before `_Ready` and our sprite added (scale 1.6, feet at -32,250); `FlipX` mirrors it | High: scale and position are guesses |
| Merchant | shop | Silent's scene; its Spine body (child 0) is hidden and our sprite added (scale 1.6) | High: scale, position and camera angle are guesses |
| Co-op hands | co-op only | Silent's arm textures | Not reachable in solo |
| Char select background / transition | run start fade | Silent's | Low |
| Card frame / energy icon | cards, tooltips | Necrobinder's energy colour, `card_frame_pink` | Low |
| Pure Water icon | relic bar | vanilla `ripple_basin` atlas entry | Low |
| Victory | Architect scene | four vanilla hit effects (`GetArchitectAttackVfx`) | Medium: never run |

## Crashes found by reading the game, fixed before the spike

- `RelicModel.Pool` / `PotionModel.Pool` call `First()` over the vanilla pools, which leave out hers: they would throw
  for Pure Water (energy icons in tooltips). `WatcherPoolLookupPatch` answers for her pools.
- After an elite or boss win, `ProgressSaveManager` looks up her epochs: `CheckFifteenElitesDefeatedEpoch` and
  `CheckFifteenBossesDefeatedEpoch` throw for an unknown character and `ObtainCharUnlockEpoch` asks for
  `STS1_WATCHER2_EPOCH`, which does not exist. `WatcherProgressPatch` skips all three for her.
- Rewards and shops would find no card in her pool (Basic only): `WatcherSpikeRewardsPatch` (temporary, since deleted).

## Known gaps, not fixed in Task 1

- **Epoch count:** her Custom run adds a `CharacterStats` entry to the modded profile. `UpdateEpochsPostRun` compares
  `CharacterStats.Count` with `AllCharacters.Count()` (5), so a modded profile with four vanilla characters played and
  one Watcher run could grant the Daily/Orobas epochs early. Task 2 (Step 3) adds the transpiler guard; until then,
  test on a modded profile where all five vanilla characters have been played, or accept the early unlock.
- **Merchant game over:** dying in a shop (a debt event, for example) plays the hidden Spine body's `die` animation;
  our sprite stays standing.
- **Co-op combat reload:** a dead Watcher's corpse is not restored when a co-op combat is reloaded (solo is unaffected).
- **Pure Water** says it adds a Miracle; it does nothing until Task 6.

## Owner results

To fill in after the spike: one line per checklist entry that failed, with the log lines and the fix chosen.

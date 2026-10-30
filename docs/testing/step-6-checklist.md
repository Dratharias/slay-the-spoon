# Step 6 — Act 4 permanent unlock in-game checklist

Build and automated tests pass. All checks below require the owner in game; none is claimed complete.
Look in `~/.local/share/SlayTheSpire2/mod_configs/SlaySpoon.cfg` for `Act4Unlocked.profile1` and in
`godot.log` for `[SlaySpoon] act4: unlocked for profile N`. Start from a cfg without the key.

- [ ] Standard run at Ascension 10: reaching Act 3 writes `Act4Unlocked.profile1` and logs the unlock once.
- [ ] Standard run at Ascension 9 reaching Act 3: nothing recorded.
- [ ] Standard run at Ascension 10 reaching only Act 2: nothing recorded.
- [ ] Daily and Custom runs at Ascension 10 reaching Act 3: nothing recorded.
- [ ] Die in Act 3 (or later) after the unlock: the key stays in the cfg.
- [ ] Resume a saved Standard A10 run that is already in Act 3 (key removed beforehand): the resume records it.
- [ ] The regular (unmodded) save folder is untouched: no modified files under `~/.local/share/SlayTheSpire2/profile1`.

## Act 4 run flag (Task 4)

Act 4 is released: a Standard run gets the flag once the profile holds the unlock (see above); a Custom Run gets it
from the "Act 4" row, without the unlock. No `Act4Preview` key is needed. The flag is `SPOON_ACT4`.

- [ ] Profile unlocked: a Standard solo run logs `[SlaySpoon] act4: flag added to the Standard run` once.
- [ ] Neow still offers its usual blessings in that run (the flag is not counted as a modifier).
- [ ] Save and quit, then resume: the run still has the flag and no `act4: saved key state dropped` error.
- [ ] Standard without the unlock: no flag.
- [ ] Daily runs never get the flag; Custom runs get it only with the "Act 4" row ticked.
- [ ] Co-op Standard run: the host's unlock decides; a client without the unlock still gets the flag from the host.
- [ ] A new Standard run after a run that took a key starts with no keys.
- [ ] A new run after a run with a loan starts with no debt (same session).

## Act 4 keys in the top bar (Task 5)

- [ ] A run with the Act 4 flag shows three dim keys (red, green, blue) in the top bar at run start; the hover tip is unchanged.
- [ ] When one player takes a key, that key brightens for every player.
- [ ] Save and resume: held keys stay bright.

## Ruby rest site option (Task 6)

- [ ] With the Act 4 flag, Recall / Rappel shows the Ruby icon and localized text.
- [ ] Solo: Recall takes the Ruby for the team and spends the rest action; no healing or smithing afterwards.
- [ ] The next rest site no longer offers Recall; saving and resuming keeps the Ruby.
- [ ] Co-op (`make mp`): one player recalls, the other can still rest or smith; their Recall icon greys out and cannot be selected.
- [ ] Co-op simultaneous Recall choices: both players spend their action and the team holds one Ruby, consistently on both peers.
- [ ] Without the Act 4 flag, rest sites have no Recall option.

## Emerald elite (Task 7)

- [ ] Before taking the Emerald, one elite per act (1–3) shows a green key; both co-op clients see the same node.
- [ ] Save/reload preserves the marked node. Skipping it marks an elite in the next act.
- [ ] Marked combat logs its buff and applies it to every enemy: Strength, +25% scaled max HP, Metallicize or non-decaying Regenerate.
- [ ] Regenerate heals at the end of the enemy turn without losing stacks; dead enemies stay dead.
- [ ] Winning grants the team the Emerald, brightens the top-bar key and removes the map marker. Later acts have no marker.
- [ ] With the same seed, map layout and ordinary rewards match a run without Act 4; no vanilla RNG is consumed by the marker or buff.

## Sapphire chest swap (Task 8)

- [ ] Obtain a chest relic: an EN/FR swap button appears after pickup effects finish. Swapping removes the relic and grants the team the Sapphire.
- [ ] War Paint / Whetstone: the affected cards return to their previous upgrades and deck positions; save/reload preserves those cards.
- [ ] Old Coin: pickup gold is removed. Pear / Strawberry: gained max HP is removed. Added potions are discarded.
- [ ] A relic that removed or transformed a card has no swap offer.
- [ ] Mango / Strawberry while damaged, then Sapphire: max HP and current HP both return to their values before the pickup.
- [ ] Potion Belt, then Sapphire: potion slots return to the count before the pickup; potions held before stay.
- [ ] Co-op: both players accept at the same time: one key, one relic lost (the first accepted), both screens agree.
- [ ] Co-op: both recipients see an offer. Simultaneous acceptance consumes only the first player's relic, on every peer.
- [ ] Co-op with different animation timing: accept as soon as the button appears; both screens agree (relic removed, key granted, same deck).
- [ ] Leaving or reloading the treasure room drops the offer; a queued stale offer cannot affect a later chest.
- [ ] Saving after the swap preserves the team key and the reverted inventory.

## Heart powers (WP4-6 Task 1)

- [ ] Invincible: the Heart's HP loss stops at 300 per turn solo, 200 at A10, and 780 with two players. Block on the Heart is not counted. Poison counts.
- [ ] Beat of Death: each card you play deals 1 damage to you (2 at A10). Strength does not change it. Monster Damage 200 % doubles it once.

## Spire Shield and Spear (WP4-6 Task 4)

Reach the fight with the vanilla console: `fight STS1_SHIELD_AND_SPEAR_ELITE` (it looks encounters up in ModelDb, mod ones included).

- [ ] Shield stands behind the players on the left, Spear in front on the right; players show Surrounded; Shield shows Back Attack.
- [ ] Both monsters show their static StS1 sprite, intents above it, and a hitbox matching the sprite. Hits shake them; death fades them out.
- [ ] Shield's opening is Fortify or Bash; turn 3 is always Smash. Smash's Block equals the damage it dealt (99 at A10).
- [ ] Bash removes 1 Strength (Defect sometimes loses 1 Focus instead).
- [ ] Spear opens with Burn Strike, never twice in a row; turn 2 is Skewer; Burns go to the discard pile (put on top of the draw pile at A10).
- [ ] Killing either one removes Surrounded and the survivor's Back Attack; the players face the survivor.
- [ ] In co-op, attacks hit every player, and the host and client logs show the same `ending:` lines.
- [ ] Monster HP 200 % doubles both monsters' HP once; Monster Damage 50 % halves Bash.
- [ ] Victory gives elite rewards (gold, card, relic, sometimes a potion).

## Corrupt Heart (WP4-6 Task 5)

Reach the fight with the vanilla console: `fight STS1_CORRUPT_HEART_BOSS`.

- [ ] The Heart shows its static StS1 sprite; its intents and health bar sit on its body (StS1 hitbox), not above the vents. The vent tops may run past the top of the screen, as in StS1.
- [ ] It starts with Invincible (300, 200 at A10) and Beat of Death (1, 2 at A10).
- [ ] Turn 1 is always Debilitate: Vulnerable 2, Weak 2, Frail 2, and five statuses (Dazed, Slimed, Wound, Burn, Void) shuffled into the draw pile at random spots.
- [ ] The next turns cycle: Blood Shots or Echo, then the other one (never two Echoes in a row), then Buff.
- [ ] Buffs in order: Artifact 2, Beat of Death +1, Painful Stabs, Strength +10, then Strength +50 each time, plus Strength +2 each time. With negative Strength (for example after Disarm), Buff first brings it back to 0.
- [ ] Painful Stabs: each unblocked Blood Shots hit adds one Wound to your discard pile; Beat of Death adds none.
- [ ] HP 750 (800 at A8 or higher); Blood Shots 2×12 (×15 at A9); Echo 40 (45 at A9).
- [ ] The map shows the Heart icon on the boss node, with its outline when hovered.
- [ ] Two players: HP 750 × 2 × 1.3 = 1950; the Invincible budget is 780 and both players' hits share it. Debilitate hits both players, each with their own five statuses. Artifact shows 3 (vanilla co-op Artifact scaling).
- [ ] Monster HP 200 % doubles the Heart's HP once but leaves the Invincible budget unchanged.
- [ ] In co-op, the host and client logs show the same `ending:` lines, including the `buff N` lines.

## The Ending act (WP4-6 Task 6)

- [ ] The log at startup shows `Ending act id: STS1_ENDING_ACT` and `Ending act self-check: ModelDb.GetById<ActModel>(STS1_ENDING_ACT) found`.
- [ ] In act 4, the act banner reads "The Ending" (French: "La fin").
- [ ] Act 4's music, ambience, map (backgrounds and dot colors), rest site background, combat backgrounds and chest are Glory's.

## The Ending map (WP4-6 Task 7)

- [ ] The act 4 map is one straight path: rest site → merchant → elite → Heart. The rest site is the travelable starting point, drawn as a normal rest site icon (no ancient).
- [ ] The rest site has no Ruby option.
- [ ] Buying Golden Compass at the act 4 merchant does not change the map; the log shows `act4: GoldenPathActMap replaced by the Ending map`.
- [ ] Save and quit at the merchant, then resume: the same map, with the rest site visited and the elite travelable.
- [ ] After the elite, only the Heart is travelable. The four nodes are spread over the whole map screen; the paths between them are drawn.
- [ ] The act 4 rest site shows the characters animated (glory loop) and hoverable, solo and in co-op (`make mp`); the Necrobinder's fire flickers. The log has no `Unexpected act` error.
- [ ] The Heart icon (and its outline) shows in the top bar boss icon during act 4, in the top bar room icon during the Heart fight, and on the Heart's floor in run history.
- [ ] Save and quit, then resume, on each act 4 node (rest site, merchant, elite, Heart): the run resumes in act 4 ("The Ending") at the same node.

## Route into The Ending (WP4-6 Task 8)

Start a Custom Run with the "Act 4" row ticked, or a Standard run once the profile holds the unlock.

- [ ] Without all three keys, the act 3 boss ends the run as in vanilla (victory room, credits).
- [ ] With all three keys, the act 3 boss gives rewards, then act 4 starts. The log shows `act4: The Ending appended` when the last key is taken.
- [ ] A10: both act 3 bosses (each gives rewards once act 4 follows), then act 4 with a single Heart.
- [ ] Killing the Heart opens the victory room, then the run is won; the run time stops at the Heart, not at the act 3 boss.
- [ ] Collect the last key, save and quit before the act 3 boss, then resume: act 4 still follows. The log shows "The Ending appended" once.
- [ ] Co-op: the client collects the last key; both players go to act 4 together. Resuming the co-op run keeps act 4.
- [ ] Daily run: no keys, no act 4.
- [ ] The log never shows `act4: The Ending was appended only when leaving an act`.

## Victory after the Heart (WP4-6 Task 9)

- [ ] Killing the Heart leads to the Architect and a victory screen; run history shows 4 acts and a win.
- [ ] The StS1 ending stinger plays once at the Architect (log: `act4: ending stinger played`). It does not replay after save and quit inside the Architect room, and it does not follow the game's music volume slider.
- [ ] Dying to the Heart, or to the Shield and Spear, is a normal defeat ("killed by" shows the localized name).

## Release (WP4-6 Task 10)

- [ ] Solo and co-op; no keys and all keys; resume in act 3 and act 4; both act 3 endings (vanilla victory without keys, act 4 with keys).
- [ ] Monster HP and Monster Damage modifiers apply in act 4; test both victory and defeat.
- [ ] Standard: the unlock persists after a restart, and once unlocked Act 4 is offered at every ascension including 0 (no `Act4Preview` key in the cfg).
- [ ] Standard, not unlocked: no keys and no act 4.
- [ ] Custom Run: the "Act 4" row is listed; ticking it gives keys and act 4 without the unlock, and Neow's offer is unchanged.
- [ ] Daily runs show no "Act 4" row, no keys, and end after act 3.
- [ ] The mod list shows version 0.9.0.

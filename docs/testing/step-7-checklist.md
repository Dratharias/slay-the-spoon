# Step 7 — The Watcher in-game checklist

Build and automated tests pass. All checks below require the owner in game; none is claimed complete.
Mod lines in `~/.local/share/SlayTheSpire2/logs/godot.log` start with `[SlaySpoon]`; also look there for
`ERROR` lines that mention `sts1_watcher`. Details and fallbacks: `docs/review/watcher-spike.md`.

## Character shell spike (Task 1)

Start each run from the main menu with the console command `sts1watcher` (temporary, solo only).

- [ ] The run starts: transition fade and sound, Neow, then the map with her marker. No error in the log.
- [ ] Top bar: her icon, 72/72 HP, 99 gold, Pure Water in the relic bar with its EN/FR text and the basin icon.
- [ ] Deck view: 4 Strike, 4 Defend, Eruption, Vigilance, with StS1 portraits and EN/FR text.
- [ ] Combat: her body stands at the usual place, hover box and intents look right, 3 energy with the purple orb
      (log has no `Watcher energy counter: no LayerN`), Strike/Eruption deal 6/9, Defend/Vigilance give 5/8 Block.
- [ ] Combat: attack and skill sounds play; the card trail is purple.
- [ ] Card reward after combat: offers cards (Ironclad's for now) and no crash.
- [ ] Elite win and boss win: no crash in the progress save (epoch checks skipped).
- [ ] Rest site: her sprite stands at the fire, sized like a vanilla character, no Silent visible; Rest and Smith work.
- [ ] Merchant: her sprite stands beside the merchant, no Silent visible; buying works.
- [ ] Death in combat: corpse sprite and death sound, then the game over screen shows her and the run history entry.
- [ ] Victory: the Architect scene plays her four hits and the victory screen shows.
- [ ] Save and quit mid-run, then continue: the run resumes as the Watcher.
- [ ] Standard and Daily runs never offer or show her; the regular (unmodded) save is untouched.

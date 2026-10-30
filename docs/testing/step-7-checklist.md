# Step 7 — The Watcher in-game checklist

Build and automated tests pass. All checks below require the owner in game; none is claimed complete.
Mod lines in `~/.local/share/SlayTheSpire2/logs/godot.log` start with `[SlaySpoon]`; also look there for
`ERROR` lines that mention `sts1_watcher`. Details and fallbacks: `docs/review/watcher-spike.md`.

## Character shell spike (Task 1)

Start each run from Custom Run with "Slay the Spire 1 Cards" or "Spire Souvenir" ticked, then pick the Watcher.

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
- [ ] Victory: the Architect victory event proceeds without crashing and the victory screen shows. Her attack scene
      is not expected: the Architect has no dialogue for an unknown character (known gap).
- [ ] Save and quit mid-run, then continue: the run resumes as the Watcher.
- [ ] Standard and Daily runs never offer or show her; the regular (unmodded) save is untouched.

## Progress guards (Task 3)

- [ ] After a Watcher Custom run (any outcome) with fewer than five vanilla characters played, the Daily and Orobas
      epochs are not granted early (timeline unchanged); log has no `watcher: rewrote` or `watcher: no CharacterStats` error.
- [ ] Solo Custom run with the Watcher: the ascension selector goes up to the best vanilla character's max ascension
      (0 when no vanilla ascension is unlocked), and the run starts at the chosen level instead of being reset to 0.
- [ ] Vanilla characters keep their own max ascension in solo runs; Standard and Daily are unchanged.

## Custom Run lobby (Task 2)

- [ ] Solo Custom run: her button is hidden with no modifier or only Monster HP/Damage; ticking "Slay the Spire 1 Cards"
      shows it at the end of the row; she can be picked and the run starts as her. Unticking while she is picked hides
      it and selects the last vanilla character again (Ironclad if none was picked).
- [ ] Same with "Spire Souvenir" as the enabling modifier: she can be picked and the run starts as her.
- [ ] Closing and reopening Custom Run shows a single Watcher button, never two.
- [ ] `make mp`: the host ticks "Slay the Spire 1 Cards"; the client sees her button, picks her, and the host sees the
      client on the Watcher (remote outline and lobby icon); both ready and reach a combat as Ironclad + Watcher.
- [ ] `make mp`: with the client on the Watcher, the host unticks the modifier: the button hides on both sides and the
      client falls back to its last vanilla pick, which the host also sees. Re-ticking shows her again.
- [ ] `make mp`: the host itself picks the Watcher, then unticks: the host falls back and the client sees the change.
- [ ] Controller/keyboard focus: moving right from the last vanilla button reaches her only while she is shown.
- [ ] Orobas may offer her Archaic Tooth only when her deck holds Bash, Neutralize, Unleash, Falling Star or Dualcast
      (vanilla rule, e.g. after Prismatic Gem or a transform); otherwise its third option follows vanilla.
- [ ] Standard and Daily character select never show her button.

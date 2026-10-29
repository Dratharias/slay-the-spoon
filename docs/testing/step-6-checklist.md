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

Set `"Act4Preview": "True"` in `mod_configs/SlaySpoon.cfg` and unlock Act 4 first (see above). The flag is `SPOON_ACT4`.

- [ ] Preview on and profile unlocked: a Standard solo run logs `[SlaySpoon] act4: flag added to the Standard run` once.
- [ ] Neow still offers its usual blessings in that run (the flag is not counted as a modifier).
- [ ] Save and quit, then resume: the run still has the flag and no `act4: saved key state dropped` error.
- [ ] Without the unlock, or without the preview key: no flag.
- [ ] Daily and Custom runs never get the flag.
- [ ] Co-op Standard run: the host's unlock decides; a client without the unlock still gets the flag from the host.
- [ ] A new Standard run after a run that took a key starts with no keys.
- [ ] A new run after a run with a loan starts with no debt (same session).

## Act 4 keys in the top bar (Task 5)

- [ ] A run with the Act 4 flag shows three dim keys (red, green, blue) in the top bar at run start; the hover tip is unchanged.
- [ ] When one player takes a key, that key brightens for every player.
- [ ] Save and resume: held keys stay bright.

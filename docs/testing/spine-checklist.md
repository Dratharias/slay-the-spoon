# Spine animations checklist (1.0.1)

StS1 skeletons converted to Spine 4.2 (`make sts1-spine`), loaded by `src/Spine/Sts1Spine.cs`. Nothing below is
tested in game yet. Every line also checks `godot.log`: no `[SlaySpoon] spine:` error and no spine-godot warning.

## The Ending (act 4)

- [ ] Spire Spear and Spire Shield idle animated (Spear slower, as in StS1), placed where the static sprites were.
- [ ] Spear: Burn Strike plays the attack on each hit; Skewer plays it on each of its hits; hits play the hurt animation.
- [ ] Shield: Bash plays Attack, Smash plays the older swing (`attack_smash`); hits play the hurt animation.
- [ ] Corrupt Heart beats (idle at 1.5); its hit box and intent stay where they were.
- [ ] Deaths: each plays its hurt once, then fades within 1 s; the fight's rewards are unchanged.
- [ ] Character Color tint (pause menu) has no effect on monsters.

## The Watcher

- [ ] Combat body idles (0.7 speed) and plays Hit when damaged; her hit box, intents and health bar are in place.
- [ ] Staff eye: closed in Neutral, open with the Calm, Wrath and Divinity irises, blending over 0.2 s; it follows the
      staff while she idles.
- [ ] Death: the body is replaced by her corpse sprite; a co-op revive brings back the animated body and the eye.
- [ ] Rest site and merchant: animated body, facing the same way as the static one did.
- [ ] Character Color tint covers her body, the eye and the corpse; turning it off restores them all.
- [ ] Co-op (`make mp`): both screens show the same animations; no desync.

## Other 1.0.1 changes

- Load: a fresh launch with BaseLib 3.4.7 logs "Slay the Spoon 1.0.1 loaded" and the game's banner shows no mod error
  (1.0.0's banner said "2 mods loaded WITH ERRORS": a helper named `Prefix` and BaseLib's random-character transpiler).
  The fixed DLL was seen loading cleanly in the log on 2026-11-05; the boxes below stay for the owner.
- [ ] Character select: Random still picks among the vanilla characters (and the Watcher in solo Standard), with BaseLib loaded.
- [ ] Character Color tint at the rest site and the merchant (Spine characters and the Watcher), on/off restores them.
- [ ] Map ping (co-op) takes the pinging player's map color when "map lines" is on, the vanilla color otherwise.
- [ ] Corrupt Heart: Blood Shots or Echo on turn 2 is the same for one and for two players on the same seed.
- [ ] Defeating Shield and Spear does not raise the "Elites Defeated" count in `godot.log`.

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

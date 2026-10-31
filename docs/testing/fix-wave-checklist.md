# Fix wave (0.10.1) — in-game checklist

## Act 4 keys

- [ ] Golden Compass picked up mid-act (Act 4 run, Emerald not yet held): the Emerald marker moves to an elite node of the regenerated map (or disappears if it has no elite); fighting that elite grants the Emerald key.
- [ ] Resuming a run saved mid-act keeps the Emerald marker where it was (a saved map is not re-picked).
- [ ] Co-op Sapphire: one player presses the swap button the instant it appears; the other peer applies the same swap (same relic given back, same key on both screens) even if its chest animation was still running.
- [ ] A Sapphire swap for a relic that finished its pickup long ago in the same chest still works as before.
- [ ] Metallicize held by an enemy (Emerald elite buff) reads "At the end of its turn, gains N Block" (FR: "À la fin de son tour, gagne N d'Armure"); on the player it still reads "your turn ... gain" / "votre tour ... gagnez".
- [ ] Regenerate on an enemy: the power's tooltip reads fine in EN and FR and the heal amount matches the stack count.
- [ ] First launch after the update: the permanent Act 4 unlock and the progress-sync answer in `mod_configs/SlaySpoon.cfg` are still there; no `SlaySpoon.cfg.tmp` is left behind.

## The Ending

- [ ] A10 with all keys: the first act 3 boss gives no rewards; the second gives the usual boss rewards and proceeds to act 4. Below A10 the single boss still rewards. Runs without act 4 keep vanilla behavior.
- [ ] Smash below A10: Block equals damage before player Block, overkill or HP-loss prevention; co-op uses the highest individual hit. At A10 it is always 99.
- [ ] Shield/Spear defeat text in French includes "le Bouclier et la Lance de la Tour".
- [ ] Resume a three-act save with all keys and a stale WinTime on the boss reward screen: act 4 is appended once and the clock resumes. Repeated routing calls do not append twice or reset a genuine Heart victory time.
- [ ] Act 4 combat and rest site show StS1 backgrounds; map remains Glory. Character positions and rest controls still work in solo/co-op.
- [ ] Act 4 music loops; Heart switches to its StS1 boss music; victory plays the stinger once, without cutscene or credits. Resume each room and check music too.
- [ ] Master/Music sliders affect the StS1 music and stinger, including zero; returning to menu during playback or transition stops it and it does not restart there.

## Watcher fidelity

- [ ] Deva Form stacks its Magic amount; later copies raise next turn's gain and subsequent growth. Verify the first three turns with two copies.
- [ ] Talk to the Hand grants Block for each hit, including lethal hits and fully blocked hits; Osty attacks grant Block to Osty's player. HP loss/Thorns do not trigger it.
- [ ] Master Reality upgrades holder-created combat cards, excluding Status, Curse and Foreign Influence's choice; other players' generated cards are unchanged. Foreign Influence+ still costs zero this turn.
- [ ] Establishment discounts Retain and temporary Retain; cards kept solely by Runic Pyramid/Ringing Triangle are unchanged. A Retained card made free this turn receives the lasting discount; repeated discounts stop at zero.
- [ ] Wave of the Hand still triggers from Block during the enemy turn, then expires at round end.
- [ ] Omniscience exhausts its selected original, purges its copies without on-exhaust effects, including an unplayable selection. Copies cannot be recovered by Exhume. Double-play effects still apply.
- [ ] Halt: outside Wrath 3/4 Block; in Wrath 12/18 before modifiers. With +2 Dexterity, base Halt in Wrath gives 16 in one gain (one Juggernaut/Wave of the Hand trigger). Check Frail too.
- [ ] Battle Hymn adds Smites only during live combat, with no surviving enemies or combat-ending state excluded.
- [ ] Fear No Evil enters Calm against an attacking enemy whether it survives or dies. Signature Move requires no other Attack in hand and glows accordingly.
- [ ] Ragnarok rolls each hit among surviving enemies; Brilliance includes Mantra already consumed by Divinity and resets next combat.
- [ ] Perseverance has one Retain line in French. Rushdown/Rushdown+ show their normal English description, never DEPRECATED; the upgrade changes only cost from 1 to 0.

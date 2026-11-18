# Achilles' Heel — in-game checklist (1.7.0)

Spec: `docs/superpowers/specs/2026-11-18-achilles-heel-design.md`. Tick only what was seen in game.

## Custom Run and start
- [ ] An "Achilles' Heel" / "Talon d'Achille" row with EN/FR text; the top bar shows its short name.
- [ ] Every player starts with Achilles' Heel, Anchor, Oddly Smooth Stone and Gorget, each once.
- [ ] After saving and reloading mid-run, the start relics are not given again.
- [ ] None of the four shows up later in a relic reward or the shop (they left the grab bags).
- [ ] With King Midas as well, the gold relics and the Block relics are all given.

## The heel
- [ ] An unblocked attack hit kills the holder; a fully blocked hit does not.
- [ ] Poison, Doom and other non-attack losses do not kill.
- [ ] Artifact does not block the heel.
- [ ] Dexterity is 0 in act 1 and 1 in act 2 (2 in act 3).
- [ ] Dexterity is 3 in a lone Ending and 4 in an Ending after the Spoon act.
- [ ] Attack damage gets +1 per 5 Block held; the bonus shows on the attack intent and on the card preview.
- [ ] Unpowered damage (orbs, relics) gets no bonus.

## Rewards
- [ ] Card offers lean toward Block cards (count over several rewards); a same-rarity draw stays seeded and identical on every client.

## Co-op
- [ ] A killed player is dead for the rest of that combat and comes back after it.
- [ ] The heel stays after a co-op revive in the same combat.
- [ ] A player without the mode (or without the relic) sees no change.

## Compatibility
| Mode | Decision |
|------|----------|
| Spoon | Combine. Max HP still grows; the heel ignores HP. |
| Blood | Combine. HP is the price and the heel ignores HP. |
| Cursed Deck | Combine. Curse HP losses are not attacks. |
| King Midas | Combine. Midas gold relics and the 3 Block relics are all given. |
| Deck of 10, Matriochka, Folie à deux, The Forge | Combine. |
| Monster Damage modifier | Combine. Only Block matters, so damage scaling makes the heel harder to protect. |
| Every other mode | Combine. |

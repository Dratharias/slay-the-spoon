# Glass Cannon — in-game checklist (1.8.0)

Spec: `docs/superpowers/specs/2026-11-19-glass-cannon-design.md`. Tick only what was seen in game.

## Custom Run and start
- [ ] A "Glass Cannon" / "Canon de verre" row with EN/FR text; the top bar shows its short name.
- [ ] Every player starts with Glass Heart, Vajra and Girya, each once.
- [ ] After saving and reloading mid-run, the start relics are not given again.
- [ ] None of the three shows up later in a relic reward or the shop.
- [ ] Girya's Lift option shows at rest sites.

## Each combat
- [ ] Combat starts with Frail 99 and Frenzy; Artifact blocks the Frail (and only the Frail).
- [ ] Frenzy shows 0, then +10 per card played (Attacks, Skills, Powers, playable curses alike).
- [ ] Attack damage grows by 10 % per stack (intent preview and card preview); orbs, Poison and Thorns don't.
- [ ] A Skill that gives Block (Defend, Survivor) resets Frenzy to 0 and the streak.
- [ ] Iron Wave (Attack with Block) and a Power with Block (Pillar of Creation, Shroud) do not reset it.
- [ ] Block from relics and powers (Anchor, Metallicize, Feel No Pain) does not reset it.
- [ ] Strength +1, +2, +3 at the end of three clean turns (+6 total); a Block Skill turn gives none and restarts at +1.
- [ ] Frenzy starts back at 0 in the next combat.
- [ ] Save and reload mid-combat: note what Frenzy shows after the reload (the state is not saved; expected: the
      combat restarts from its saved start).

## Necrobinder
- [ ] Osty's attacks get the Frenzy bonus; the streak Strength goes to the Necrobinder only.

## Co-op
- [ ] Each player's stacks and streak follow their own cards only.
- [ ] Another player's attacks get no bonus from my Frenzy.
- [ ] A revived player keeps their Frenzy.

## Rewards
- [ ] Glass Cannon alone: card offers unchanged.
- [ ] With Achilles' Heel too: offers lean toward Block cards and Attacks; a same-rarity draw stays seeded and identical
      on every client.

## The Heart
- [ ] The Corrupt Heart can be killed in 3 turns up to A9 (Invincible unchanged).

## Compatibility
| Mode | Decision |
|------|----------|
| Achilles' Heel | Combine. Offers lean toward Block cards and Attacks; Frail and the heel stack up. |
| Spoon, Blood | Combine. |
| Cursed Deck | Combine. Playable curses give stacks. |
| King Midas | Combine. Gold relics, Vajra and Girya are all given; Midas fires still charge for Lift. |
| The Forge | Combine. A forged Block Skill still resets Frenzy. |
| Every other mode | Combine. |

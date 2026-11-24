# 1.11.0 in-game checks

## Forge ladder (spec `docs/superpowers/specs/2026-11-23-forge-ladder-banner-design.md`)

- [ ] Forge run, deck view, inspect a +0 card, tick "View Upgrade": a panel on the left lists +1 to +9.
- [ ] Hovering a level shows it on the big card with its hover tips; the hovered small card is lit.
- [ ] A +4 card lists +5 to +9; a +9 card shows no panel; a card that does not fuse (no growth) shows vanilla only.
- [ ] Untick, the arrows to another card and closing the screen remove or rebuild the panel; clicks on it never close the screen.
- [ ] Outside a Forge run the screen is vanilla.
- [ ] Forged curses (Forge + Cursed Deck) list their levels and defects.

## Overcursed (spec `docs/superpowers/specs/2026-11-23-overcursed-design.md`)

- [ ] A curse from an event, a relic or Embrace comes with two copies; a normal card comes alone.
- [ ] With Cursed Deck the start curse comes in three.
- [ ] With Hoarder or Matriochka a curse comes in three, not five.
- [ ] Every "?" event offers Embrace; with King Midas it always gives 150 gold.
- [ ] The merchant can still remove a curse.
- [ ] Co-op: both players see the same deck counts.

## Witching Hour (1.11.1)
- [ ] Overcursed at an ascension that adds Ascender's Bane: the deck holds one Witching Hour instead (floor 1), not three.
- [ ] It starts in hand (Innate), cannot be removed or transformed (Eternal).
- [ ] Odd turns: your other cards cost 1 more while it is in hand; it cannot be played, even with the Unholy Grimoire ("Not at this hour.").
- [ ] Turns 3, 5...: from the draw pile it comes to hand about 80 % of the time; the same in both co-op clients.
- [ ] Cursed Deck + Overcursed, even turn: the Grimoire plays it on an enemy, which skips its next turn (stun intent).
- [ ] Portrait, EN/FR title, text and hover tip read well.
- [ ] Cursed Deck: Neow's Bones never comes from a chest, elite or merchant.

## Overcursed cursed relics (1.11.2)

- [ ] Overcursed alone: a merchant's relic slots are mostly cursed relics (5 in 6 per slot), priced like Uncommons for the mod's four.
- [ ] Overcursed alone: an event giving a random relic (Round Tea Party, Unrest Site, Luminous Choir...) usually gives a cursed relic.
- [ ] Overcursed alone: chest and elite relics stay vanilla.
- [ ] Overcursed alone: Embrace the curse gives a cursed relic about 2 times in 3, gold or heal otherwise.
- [ ] Overcursed + Cursed Deck: chests and elites cursed about 2 in 3, merchants about 5 in 6.
- [ ] Once all ten cursed relics are held, every pull is vanilla again.
- [ ] Co-op: both players see the same merchant and event relics (no divergence).
- [ ] Ancients (Neow, Darv, Orobas...) are unchanged.

## Spoon act restaurant ambience (1.11.3)

- [ ] Spoon act rest site and boss fight: a quiet restaurant ambience (chatter, cutlery) plays under the music, fading in over 2 s.
- [ ] The SFX slider (and Master) changes its level live; SFX at 0 silences it; the music stays the main track.
- [ ] It stops when The Ending follows, at the game over / victory return to the menu, and never plays outside the Spoon act.
- [ ] The loop point is not audible (73 s loop).

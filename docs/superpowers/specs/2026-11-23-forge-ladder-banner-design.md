# Forge ladder banner (design)

## Intent

Owner request (2026-11-23): in a Forge run, "a +1 → +9 upgrade preview banner for forged cards, shown when upgrades are
displayed, in place of the vanilla one". Vanilla's "View upgrades" tickbox on the card inspect screen only shows the
next level, and nothing once a card is at +1, so a player cannot see what fusing a card further would give.

## Design (builder calls, owner validated the idea)

- Where: the card inspect screen (deck view, card grids, hand inspect), when "View upgrades" is ticked, in a Forge run,
  for a card fusion can take to +9 that is below +9. Otherwise the screen is vanilla.
- What: a panel on the left of the big card lists every level from the next one to +9 as small cards (3 columns,
  `ForgeLadderRule.Levels`). The big card shows the next level (as vanilla does for +1); hovering a small card shows
  that level on the big card, with its hover tips. The hovered small card is lit, the others dimmed.
- The levels are built like a reload: a clone of the card raised through `ForgeLevels.RaiseTo`, so Forge bonuses and
  the ladder (Retain +6, cost −1 +7, Innate +9) appear exactly as they would after fusion.
- View only: no state, no sync, clicks on the panel are swallowed so they never close the screen.
- Approaches: (1) side panel next to the vanilla card (picked: the vanilla layout, arrows and tickbox stay where they
  are); (2) shrink the big card and put a strip below it (rejected: it fights the screen's open and arrow tweens, and the
  tickbox sits at the bottom).

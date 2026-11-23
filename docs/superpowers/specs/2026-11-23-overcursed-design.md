# Overcursed (design)

## Intent

Owner idea (2026-11-23, "overcursed"): Matriochka for curses only. Every curse a player receives comes in three, so a
deck drowns in curses; with Cursed Deck (playable curses) and The Forge (fused curses) it becomes a curse engine.

## Rules (builder calls, the owner gave the idea only)

- Custom Run checkbox `SpoonOvercursed` (EN "Overcursed", FR "Surmaudit"). Combines with every mode.
- Every curse that enters a deck from outside (rewards, events, relics, Forge duplicate, Embrace) brings two copies
  (`OvercursedRule.CopiesFor`), as vanilla Hoarder does for every card. Other cards come one at a time.
- With vanilla Hoarder or Matriochka ticked, they already copy every card: Overcursed adds none (no five of a kind).
  Matriochka's 999-card limit also caps Overcursed's copies.
- Merchant card removal stays allowed (unlike Hoarder): removing a curse is the counterplay, one copy at a time.
- With Cursed Deck, the start curse comes in three.
- Every "?" event gets Cursed Deck's "Embrace the curse" option (a random curse, so three copies, then a cursed relic,
  75 gold or a 25 % heal). With King Midas the reward is always gold, doubled (150); with Blood gold becomes the heal.
  The Midas rule also applies to Cursed Deck alone.
- Rejected: a separate set of curse events (too much text for a mode whose rule is one line); tripling Cursed Deck's
  cursed relic draws (they are relics, not curses).

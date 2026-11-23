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

## Cursed relics (owner, 2026-11-23; 1.11.2)

Owner request: in Overcursed, the regular Cursed Deck's cursed relics are available at merchants and in events, with higher odds.

Builder calls:
- Sources: Overcursed curses merchant relic slots and random relic pulls inside an ordinary "?" event room (Ancients and the
  Architect excluded). Chests and elites stay Cursed Deck only, so Overcursed alone leaves them vanilla.
- Odds: 5 pulls in 6 at merchants and events (Cursed Deck's 2 in 3 elsewhere); Embrace the curse gives a cursed relic
  2 times in 3 while one is left (Cursed Deck: 1 in 3).
- Same pool of ten, same seeded streams and reservation per floor as Cursed Deck; once all are held, pulls are vanilla.

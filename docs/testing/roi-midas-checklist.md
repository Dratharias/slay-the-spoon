# King Midas (1.2.0) — in-game checklist

Nothing here has been tested in game. Log lines: `[SlaySpoon] King Midas: <event> replaces <event>`.

## Setup

- [ ] Custom Run shows the "King Midas" checkbox with its short text (EN and FR).
- [ ] Ticking King Midas unticks Matriochka, at any reroll value, and the other way round.
- [ ] Run start: every player has the 11 relics (Bowler Hat, Amethyst Aubergine, Lucky Fysh, Maw Bank, Golden Idol,
      Old Coin, Golden Pearl, Signet Ring, Cursed Pearl, Membership Card, The Courier), Greed in the deck, and the
      gold of Old Coin, Golden Pearl, Signet Ring and Cursed Pearl added.
- [ ] None of these relics shows up later in a chest, an elite reward, a shop or an event.
- [ ] Save and reload after floor 1: relics, gold and Greed are kept, nothing is granted twice.

## No card rewards

- [ ] Hallway, elite and boss combats give no card reward (gold, potions, relics stay).
- [ ] Treasure rooms and custom rewards (events, relics) give no card reward.
- [ ] Massive Scroll, Lead Paperweight, Sea Glass, Brain Leech, Room Full of Cheese: no pick screen, no card.
- [ ] Neow is vanilla: Draft, Sealed Deck and card options still work.
- [ ] Cards generated in combat (Discovery, potions) still work.
- [ ] The merchant sells cards as usual; The Courier refills it.

## Gold events

- [ ] Every "?" room that becomes an event is one of: Lost Wisp, The Lantern Key, Dense Vegetation, Colossal Flower,
      Jungle Maze Adventure, Sunken Statue, This or That, Sunken Treasury, Trash Heap, Mind Bloom, The Nest, Moai Head.
- [ ] Repeats happen; a repeated vanilla event plays correctly.
- [ ] Only the gold options are shown (see the spec's table); Colossal Flower lets you dig deeper but not take the core.
- [ ] Moai Head only appears while every player still holds the Golden Idol.
- [ ] Co-op (`make mp`): both clients get the same event; shared events (Lantern Key, Dense Vegetation, Jungle Maze)
      stay shared.

## Rest sites

- [ ] Every option shows its price on focus (50 for the first action).
- [ ] After paying, the options come back; the next action costs 100, then 150. The next fire starts at 50 again.
- [ ] An option you cannot afford is greyed out and cannot be clicked.
- [ ] Proceed works at any time (the fire stays lit, since the options keep coming back).
- [ ] Maw Bank: after buying something (bank broken), "Reopen" appears with the Maw Bank icon; it costs the same price,
      the bank works again (gold on room entry), and Reopen disappears.
- [ ] Co-op: each player's price and options are their own; the other player's choices show on their character.

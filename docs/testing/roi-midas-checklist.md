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
- [ ] Maw Bank: the rest site no longer offers "Reopen" (moved to the merchant in 1.9.0, see below).
- [ ] Co-op: each player's price and options are their own; the other player's choices show on their character.

## Balance (1.8.4)

- [ ] Each map travel costs 10 % of the gold, at least 25 (3 900 → 3 510; 100 → 75). No loss at the start of an act.
- [ ] Traveling with 25 gold or less kills the player (solo: game over screen over the map).
- [ ] At the merchant, each purchase raises the remaining prices by 10 %; the next merchant starts at normal prices.
- [ ] A purchase that would leave 25 gold or less shows a red price and is refused; same for rest site actions.
- [ ] Co-op (`make mp`): both clients show the same gold after travel; a ruined player dies on both.

## Balance and rest site fix (1.8.5)

- [ ] The melt is 10 % of the gold or the average gold gained per floor so far, whichever is larger (never under 25):
      after a rich event (Mind Bloom 999) the melt jumps for the following floors.
- [ ] Merchant and rest site refuse what would leave gold at or under that amount.
- [ ] Rest site: after a paid action (Reopen included), the options come back and every one of them works; proceeding
      and traveling does not crash.

## Rest site with no affordable option (1.8.19)

- [ ] Reach a rest site with too little gold for any option (every option greyed): Proceed is enabled at once and the map opens.
- [ ] With enough gold for one option, Proceed stays disabled until an option is chosen (vanilla).

## With the base game's Midas (1.8.26)
- [ ] King Midas + Midas: gold rewards doubled; the melt takes 20 % (at least 50, or the average gain per floor);
  merchant prices rise 20 % per purchase; rest site actions cost 100, 200, 300.
- [ ] King Midas alone: unchanged (10 %, 25, +10 %, 50/100/150).

## Ruined players come back (1.8.33)

- [ ] Co-op (`make mp`): a player the melt empties dies on the map, then is alive with 1 HP and 1 gold when the next room
      opens (log: "King Midas: player N revived for the room"); a shared event, chest or rest site works for them.
- [ ] Leaving that room with too little gold kills and revives them again; gold earned in the room keeps them above the melt.
- [ ] Both players ruined on the same travel: game over. Solo: a ruin is still a game over.
- [ ] Both clients show the same gold and HP after the revival travel.

## "Broke" caption (1.8.34)

- [ ] A ruined player sees "Broke" (French: "Fauché") over a greyed screen for about 3 s; the ally does not.
- [ ] Input is not blocked; the caption is readable at 16:9 and 21:9, and also shows over a solo game over.

## Relic sale and bank reopening (1.9.0)

- [ ] Merchant: a "Sell a relic" button sits at the top right (below Duplicate in a Forge run); greyed when you hold
      nothing sellable (the 11 King Midas relics, your starter relic, Ancient and Event relics are never offered).
- [ ] Sell opens the "Choose a Relic" screen with your sellable relics; skipping it sells nothing and changes no gold.
- [ ] Selling pays 75 % of the vanilla price (Common 131, Uncommon 168, Rare 206, Shop 150), removes the relic (its
      removal effect runs, e.g. lost max HP), and logs "King Midas: player N sold X for Y gold".
- [ ] The sold relic appears in a new slot below the buttons, dearer than the sale: never under sale + 25 %
      (164 / 210 / 258 / 188), Membership Card included; buying it back gives it back, the slot empties, nothing restocks.
- [ ] Several sales in one visit add several slots side by side; leaving the merchant loses the unsold ones.
- [ ] Co-op (`make mp`): the ally sees the seller lose the relic and gain the gold; the resale slot shows only in the
      seller's shop; buying it back updates the ally's view; no desync.
- [ ] Maw Bank: "Reopen the bank (50 gold)" shows only while the bank is broken; the price follows the merchant rise
      (55 after one purchase, 60 after two; ×2 rise with the base game's Midas); greyed when paying would ruin you.
- [ ] Reopening takes the gold, the bank pays again on the next room, the button hides; it does not raise prices; a
      later purchase breaks the bank again and the button comes back.
- [ ] Run seed unchanged: the merchant's other prices and the next merchants' stock are the same with or without a sale.


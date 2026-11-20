# King Midas (Roi Midas) — design

Approved by the owner on 2026-11-14 ("oui, écris-le et code").

## Intent

A gold run. Players start with every relic that makes or saves gold, never receive cards for free, and every
"?" event they meet is a gold event in which gold is the only way out. Cards come from the merchant only.

## Approach

A Custom Run modifier `SpoonRoiMidas` ("King Midas" / "Roi Midas"), a checkbox like the mod's other modifiers.
No multiplayer host card. Pure rules live in `src/Core/MidasRule.cs` with xUnit tests.
Alternatives dropped: a host card like Folie à deux (the owner wants it in Custom Run only); a full custom event
(the owner wants vanilla gold events, repeated).

## Rules

1. **Starting relics.** At run start every player gets Bowler Hat, Amethyst Aubergine, Lucky Fysh, Maw Bank, Golden
   Idol (the mod's StS1 port), Old Coin, Golden Pearl, Signet Ring, Cursed Pearl (with its Greed), Membership Card
   and The Courier. Their pickup effects apply (Old Coin, Golden Pearl, Signet Ring and Cursed Pearl gold, Cursed
   Pearl's Greed). They leave every grab bag. Excluded on purpose: Ectoplasm, Lord's Parasol, Seal of Gold, Silken
   Tress, Dragon Fruit.
2. **No card rewards.** Card rewards are removed everywhere: combat (elites and bosses included), treasure, events,
   relic and potion custom rewards. Out-of-combat card pick screens (Massive Scroll, Hefty Tablet, Lead Paperweight,
   Sea Glass, Brain Leech, Room Full of Cheese) give nothing. Kept: the merchant, cards generated in combat,
   the starter deck, Neow.
3. **Gold events only.** Every "?" room that resolves to an event draws a gold event, from any act, repeats allowed,
   on the mod's own `Rng` (run seed + floor: vanilla streams do not move, co-op clients agree). In each event only
   the options that lead to gold remain.
4. **Rest site "Reopen".** A rest option that makes a broken Maw Bank work again. Shown only while the player's bank
   is broken.
5. **Paid, repeatable rest sites** (owner addition, 2026-11-14: "il faut payer pour upgrade/sleep/etc. mais on peut
   répéter l'action"). Every rest site option, Reopen included, costs gold: 50 for the player's first action at a fire,
   50 more for each further one at the same fire (50, 100, 150…), back to 50 at the next fire, counted per player.
   After each paid action the options come back, so a player acts again while they can pay. An option the player
   cannot afford is greyed out; the focused option shows its price.
6. **Balance patch** (owner, 2026-11-20: "il faut forcer le joueur à flamber son argent"; the start is close to 4 000
   gold with discounted prices):
   - **Melt.** Each travel to a map room costs every living player 10 % of their gold, at least 25
     (`MidasRule.Melt`). A player the melt leaves with no gold dies (a forced kill: Lizard Tail does not save them).
     It runs in the synced travel action, on the map before the fade. Act starts are not travels: no melt at the act's
     Ancient.
   - **Rising prices.** Each purchase at a merchant raises the price of everything left there by 10 % of its price
     (cards, relics, potions, removal), per player, back to normal at the next merchant.
   - **No fatal spending.** The merchant refuses a purchase that would leave the buyer ruined by the next melt (25 gold
     or less); the price shows red. Builder call: rest site actions follow the same rule.

The map, the "?" room odds and merchants stay vanilla (The Courier refills the shop anyway).

## Gold events and the options kept

| Event | Kept |
|-------|------|
| Lost Wisp | Search |
| The Lantern Key (shared) | Return the Key |
| Dense Vegetation (shared) | Trudge On |
| Colossal Flower | Extract / Reach Deeper on every page, Extract Instead (Pollinous Core removed) |
| Jungle Maze Adventure (shared) | both (both give gold) |
| Sunken Statue | Dive into the Water |
| This or That | Plain |
| Sunken Treasury | both chests |
| Trash Heap | Grab |
| Mind Bloom (StS1) | I am Rich |
| The Nest (StS1) | Smash and Grab |
| Moai Head (StS1) | Offer the Golden Idol; drawn only while every player still holds the idol |

An event's own `IsAllowed` still applies to vanilla events. The StS1 events skip their card-set eligibility (their
gold options need no StS1 card). If filtering would leave a page with no enabled option, the page stays vanilla.

## Builder calls

- Endless Conveyor and Trial are left out: their gold depends on a random dish or a random trial page (2 in 3 Trial
  pages give no gold), so "gold is the only option" could not hold.
- Multi-page events keep the path to gold, including Colossal Flower's "reach deeper".
- Shared events stay shared.
- Matriochka is mutually exclusive with King Midas (it hands out a card reward on every floor).
- Folie à deux stays compatible (only Neow picks are left to bid on).
- Neow's Draft and Sealed Deck still work (Neow is kept).
- Standard and Daily stay vanilla.
- The per-fire count is not saved: reloading in the middle of a rest site starts the price at 50 again.

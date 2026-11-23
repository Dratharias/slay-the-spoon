# King Midas: selling relics to the merchant (design)

## Intent

In King Midas, gold only melts and gets spent. Bot runs show late-run income falling below the melt floor (Regent
464 → 9 gold over act 3 with no purchase). The owner wants a fun way for players to refill their purse: sell relics to
the merchant, choosing what to give up.

## Owner decisions (2026-11-23)

- A **Sell** button at the merchant, below the Forge **Duplicate** button. It opens a pick of the player's relics.
- Relics of the starting pack cannot be sold: the 11 King Midas gold relics and the character's starter relic.
- **Ancient** and **Event** relics cannot be sold.
- Sell price: the relic's merchant buy price × 0.75 (`RelicModel.MerchantCost`: Common 175 → 131, Uncommon 225 → 168,
  Rare 275 → 206, Shop 200 → 150; rounded down).
- The merchant puts the sold relic back on sale, dearer than the sale price.

## Builder calls (owner may correct)

- King Midas only. Unlimited sales per visit.
- Resale price: the vanilla merchant price for its rarity (`MerchantCost`), then the King Midas +10 % per purchase at
  this merchant, like every other item. Always at least the sale price + 25 %, so selling and buying back costs gold.
- The resold relic sits in an extra relic slot of the seller's own merchant inventory (StS2 inventories are per
  player, so an ally does not see it). Leaving the merchant loses it: it does not return to any pool.
- The base game's Midas modifier does not double the sale price (it would let a sale beat the buy-back).
- Under Blood, the sale is an ordinary gold gain (`PlayerCmd.GainGold`), handled by Blood's existing rule.
- Selling a relic removes it the vanilla way (`RelicCmd.Remove`), so its `AfterRemoved` runs (lost max HP, etc.).
- Co-op: the sale is a queued synced action, like `ForgeDuplicateAction`.

## Approaches considered

1. **Button under Duplicate (picked).** Reuses `ForgeDuplicateButtonPatch`'s placement, `NativeTradePopup.Button`, the
   queued-action pattern; works with a controller; players see what they can sell.
2. Right-click a relic in the top bar. Needs a hook into the relic bar's input, nothing tells the player it exists, and
   controller users have no right click.
3. A sale counter at the rest site. Off the merchant, where the owner wants it.

## Left to the builder

- Where the extra resale slot sits (clone of an `NMerchantRelic`, next to the Sell button).
- The pick screen (a relic choice like the vanilla "Choose a Relic" screen, or the sale panel style).
- Pure rules in `src/Core/MidasRule.cs` (who can sell what, prices) with xUnit tests; EN/FR text.

## Addendum: Maw Bank reopened at the merchant (owner, 2026-11-23)

- The rest site option that reopens a broken Maw Bank (`MawBankRestSiteOption`) moves to the merchant: a **Reopen the
  bank** button, shown while the bank is broken, below the Sell button. The rest site option is removed.
- The Maw Bank itself stays vanilla: a purchase made after reopening breaks it again (owner: "too bad if the player
  shops"), so players reopen it last.
- Builder calls: price 50 gold, times King Midas's merchant rise at this merchant (`MidasRule.PriceFactor`, vanilla
  Midas scale included); refused when `MidasMelt.CanSpend` refuses; not a `MerchantEntry`, so paying does not break the
  bank or count as a purchase for the rise. Synced queued action like the sale.

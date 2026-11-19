# The Forge — in-game checklist (1.4.0)

Spec: `docs/superpowers/specs/2026-11-18-forge-design.md`. Tick only what was seen in game.

## Custom Run
- [ ] "The Forge" row in Custom Run (EN/FR title and description); top bar shows "Forge".

## Fuse (rest site)
- [ ] The Fuse option uses the Smith icon; it is disabled with the explanation when no pair exists.
- [ ] Pick the kept card, then the copy (only valid copies offered); Escape at either step cancels and keeps the action.
- [ ] Preview shows the result level; skipping it cancels without spending the rest action.
- [ ] Strike + Strike = Strike+1; Strike+1 + Strike+1 = Strike+3; the result never passes +9.
- [ ] Damage/Block repeat the +1 delta per level; cost and keywords stay as at +1.
- [ ] A cost-only card (e.g. Barricade-like) lowers its cost each level and stops at 0.
- [ ] A card capped at 3 (e.g. Adrenaline) still fuses past +3, but its energy and draw stop growing; Battle Trance's
      draw stops at +5 while a capped card's damage or Block keeps growing.
- [ ] Ladder: Retain at +6, cost 1 lower at +7 (never below 0), Innate at +9.
- [ ] Tracking+3: Weak enemies take +100 % attack damage (50 + 25 × 2); the card shows an orange "Forged: Tracking +50." line.
- [ ] A `play` bonus card (e.g. Havoc+3: 8 extra damage to a random enemy) applies its bonus on every play, and the text
      lists it in English and French.
- [ ] A capped card's `play`/`powerBonus` stops growing past its cap (Double Energy+5 gives the same extra Energy as +3).
- [ ] Limit Break+9: gains 8 Strength, then doubles.
- [ ] StS1 card + its StS2 namesake: the preview offers both versions; the chosen one replaces the kept card in place.
- [ ] The kept card's enchantment carries over to the other version when it can.
- [ ] 1.8.2: fusing an unenchanted card with an enchanted copy (either order, same card or other version) keeps the
      enchantment on the result; with two enchanted copies the kept card's stays.
- [ ] Smith, Armaments, Apotheosis and other ordinary upgrades never take a card past +1.
- [ ] Limit Break+2 to +5: text shows "Gain N Strength, then double"; the Strength is gained before doubling.
- [ ] Save and reload a run with +2 to +9 cards: levels, numbers and cost unchanged.
- [ ] Co-op: a fusion on one client shows the same deck on the other; no desync at the next fight.

## Forged look (1.8.1)
- [ ] +2 to +8: the title takes yellow-green, orange, red, magenta, violet, blue, cyan in turn; readable on every banner
      (Rare +2/+3 and Uncommon +7/+8 get the lighter fill and dark outline).
- [ ] The frame fades toward the Ancient frame level by level; the card body darkens to navy; the text stays readable.
- [ ] A gloss sweeps the Ancient border; at +9 the title shimmers (light band, faint rainbow).
- [ ] The character color of the ordinary frame is kept below +9.
- [ ] A card node reused for an unforged card (hand, grid, reward) shows the vanilla frame again.
- [ ] Real Ancient cards and +0/+1 cards look vanilla.

## Duplicate (merchant)
- [ ] "Duplicate" button at the merchant (top right, under the loan button spot); disabled when no card is affordable.
- [ ] Prompt lists the three prices; Common 25, Uncommon 50, Rare 75; +25 at Ascension 6+; +25 per earlier purchase.
- [ ] The copy is +0 and joins the deck; gold drops by the price; the button turns to "Duplicated" until the next merchant.
- [ ] Curses and Statuses are not offered. Cancel spends nothing.
- [ ] Co-op: the purchase and the new card show on both clients; the purchase count survives save/reload.

## Other modes
- [ ] With Spire Souvenir (Hybrid): the swap button does not appear on the fusion preview.
- [ ] With King Midas and Folie à deux: the fusion preview is not blocked or turned into a chest.

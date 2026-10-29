# Step 2b — in-game checklist

Tick when checked in game. "Base / +" means the card before and after upgrade.
Quickest way: Custom Run, Ironclad, "Spire Souvenir" (Hybrid: the Ironclad starts with the relic) or "Slay the Spire 1 Cards" ticked.
Fastest: in combat, open the dev console (` key, or ' / * / ^; any modded run has it) and type `sts1card STS1_HEAVY_BLADE` (Tab completes the id; add `Deck` to put it in the deck). Vanilla helpers: `energy 9`, `godmode`, `upgrade 0` (leftmost card in hand).

## Namesakes

- [ ] StS1-only and Hybrid, Ironclad: Juggernaut and Rampage are offered only in their StS1 version (StS1 art); the StS2 versions never show up.
- [ ] Juggernaut (2 energy): whenever you gain Block, 5 / 7 damage to a random enemy.
- [x] Rampage: 8 damage, then +5 / +8 damage each play this combat; back to 8 next combat.
- [ ] StS1-only: Rupture is offered only in its StS1 version.
- [x] StS1 Rupture: losing HP to your own cards (Bloodletting, Offering, Hemokinesis…), Brutality or Combust gives 1 / 2 Strength at once, on any turn. Damage from enemies doesn't.

## Hybrid: duplicates

- [ ] Hybrid, Ironclad: Rupture shows up about as often as any other uncommon (one slot, not two), sometimes as the StS1 version, sometimes as the StS2 one.
- [ ] The same holds at the merchant, in transforms and in random combat generation (e.g. a Discovery-style effect).
- [ ] An upgraded Rupture in a reward stays upgraded whichever version it comes out as.
- [ ] Co-op Hybrid: no desync after rewards, a merchant visit or a transform that produced a Rupture.
- [ ] Standard and StS1-only runs: never a random version (StS1-only always has the StS1 Rupture).

## Hybrid: swap button on card rewards

- [ ] A card reward offering Rupture shows a button "Rupture: StS1 version" (or "StS2 version") next to Skip; other cards get no button. French: "Rupture : version StS1".
- [ ] Pressing it turns the card into the other version in place (art, text); pressing again turns it back. The upgrade (+) is kept.
- [ ] Taking the card after a swap adds the version shown.
- [ ] Skip and Reroll still work; after a reroll that brings a Rupture, its button works too.
- [ ] Co-op: swap then take the card; the other player's view of your deck shows the same version, and nothing desyncs.
- [ ] Standard and StS1-only rewards: no swap button, Skip/Reroll unchanged.
- [ ] Controller: the swap button can be focused and pressed; its hotkey (draw pile button) works.

## Neow's Blessings

- [ ] Custom Run shows "Neow's Blessings" / "Bénédictions de Neow" with its description in both languages.
- [ ] Ticked with Monster HP 150 % (or a card set mode): Neow offers its usual choice (two blessings and a curse option), as in a Standard run.
- [ ] Not ticked, same options: Neow offers nothing, as before.
- [ ] Ticked with Draft (or Sealed Deck): Draft's picks come first, then Neow's blessings; picking one finishes the event.
- [ ] The top bar shows "Neow" for it.
- [ ] Co-op: the host ticks it; both players get Neow's blessings, no desync.

## Spire Souvenir

- [ ] Custom Run with "Spire Souvenir" ticked, Ironclad: the run starts with the relic next to Burning Blood; its tooltip shows the Dusty Tome icon and the text in English and French.
- [ ] Same run with Defect, Regent or Necrobinder: no relic, vanilla cards. (Since step 3a the Silent gets the relic too; see `step-3a-checklist.md`.)
- [ ] Relic held: card rewards mix StS1 and StS2 cards, and a duplicate (Rupture) shows the swap button.
- [ ] Relic held: the merchant, a transform, and Infernal Blade or an Attack Potion offer StS1 cards too.
- [ ] "Spire Souvenir" run: the relic never drops as a reward (the Ironclad already has it).
- [ ] Standard run, Ironclad: the relic can drop as a Common relic (or `relic add SPOON_SPIRE_SOUVENIR` in the console); held, it mixes the cards as above.
- [ ] Custom Run with "Slay the Spire 1 Cards" and the relic (console): StS2 cards come back.
- [ ] Daily run: the relic never drops.
- [ ] `relic remove SPOON_SPIRE_SOUVENIR`: the next reward is back to the run's cards.
- [ ] Save, quit and resume with the relic: the effect persists.
- [ ] Co-op (`make mp`), "Spire Souvenir" ticked, host Ironclad and client another character: only the host gets the relic and mixed rewards; no desync.

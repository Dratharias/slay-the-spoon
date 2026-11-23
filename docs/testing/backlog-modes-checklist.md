# Backlog modes — in-game checklist (1.5.0)

Spec: `docs/superpowers/specs/2026-11-18-backlog-modes-design.md`. Tick only what was seen in game.

## Custom Run
- [ ] Seven new rows (Mutation, Deck of 10, Blood, Trophies, Forced Trade, Conductor, Useless Relics) with EN/FR text;
      the top bar shows each short name.
- [ ] Mutually exclusive: Blood with Merchant Loans and King Midas; Conductor with Folie à deux and every Matriochka value.

## Mutation
- [ ] Entering act 2: every unupgraded, transformable card is transformed (preview shown); upgraded and Eternal cards stay.
- [ ] Save and reload at the start of act 2: no second mutation. Same seed twice gives the same cards.

## Deck of 10
- [ ] Every player starts with the Deck of 10 relic (Precarious Shears icon, EN/FR text); it never shows in a relic reward.
- [ ] The Silent's 12-card start opens the Sealed Deck grid on the first room: pick 10 to keep; each dropped card gives +1 max HP.
- [ ] A card reward on a full deck opens the unskippable removal screen for exactly the excess; the new card can be dropped.
- [ ] A card added in combat is trimmed when the combat ends. Eternal cards can't be picked.
- [ ] Standard run: Neow's cursed option is sometimes Deck of 10 (1 in 11; restart runs to see it). Taking it opens the
      keep grid at once; the other two options and the rest of the run are the same as without the mod for that seed.
- [ ] Co-op: the keep grid of one player doesn't block or desync the other.

## Blood
- [ ] Gold starts at 0; the top bar shows (HP − 1) × 5; no gold reward after combat.
- [ ] Buying at the merchant costs ceil(price / 5) HP and never kills; card removal and events with gold costs work.
- [ ] Blood Fang: an attack's unblocked damage heals 20 % (rounded down). It never shows in a relic reward.
- [ ] Save and reload: HP, gold display and Blood Fang survive.

## Trophies
- [ ] Winning an elite fight adds one Trophy card per player, titled "<Monster>: <Move>" in EN and FR.
- [ ] Its damage and hit count match the move; cost 1/2/3 by total damage; the upgrade adds 25 % rounded up.
- [ ] A Decimillipede Trophy is titled "Decimillipede: <Move>" (EN) / "Dix-Mille-Pattes: <Move>" (FR), not a raw key (1.8.28).
- [ ] An elite without attacks gives "<Monster>: Trophy" (10 damage, cost 1). Trophy cards survive save and reload.

## Forced Trade (co-op)
- [ ] After each victory every player picks one card (synced, not skippable) and it lands in a random ally's deck,
      upgrade and enchantment kept. Solo: nothing happens.

## Conductor (co-op)
- [ ] After each victory one player (never the same twice in a row) gets one screen with every movable reward: cards
  shown as cards, relics and potions with their icon, name and text; one row of character portraits per reward, the
  owner selected first; one Confirm button (1.8.26). Mouse, keyboard and controller can pick a portrait.
- [ ] From three players each reward's portraits form a 2 × 2 square (two players: one line) (1.8.27).
- [ ] The rewards take 1 to 3 columns, whichever shrinks the page least for the screen shape (16:9, 16:10, 21:9);
  resizing the window or changing the resolution redoes it; twelve rewards (four players) fit and Confirm is visible.
- [ ] Other players wait, then each player's reward screen shows what they received; gold unchanged.
- [ ] A relic given to a player who already holds it stays with its owner.
- [ ] 1.8.30: clicking a card rings it in gold (click again clears it); after Confirm that card is in the chosen player's
      deck with no card choice for them, in both games; an unclicked card reward still lets the recipient choose.

## Useless Relics
- [ ] Akabeko, after its Vigor at combat start: clicking it starts targeting; the enemy takes 10 damage through Block.
- [ ] The thrown relic turns grey and stops working (e.g. a thrown Centennial Puzzle no longer draws); Escape cancels.
- [ ] A relic not on the list (or one that hasn't flashed yet) opens the inspect screen as usual.
- [ ] After combat the relic is back to normal and works in the next fight.
- [ ] Co-op: a throw by one player shows the damage on both clients; controller targeting works.

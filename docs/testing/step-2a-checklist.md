# Step 2a — in-game checklist

Tick when checked in game. "Base / +" means the card before and after upgrade.
Quickest way to get a card: Custom Run with "Slay the Spire 1 Cards" ticked, Ironclad, then card rewards and shops.
Fastest: in combat, open the dev console (` key, or ' / * / ^; any modded run has it) and type `sts1card STS1_HEAVY_BLADE` (Tab completes the id; add `Deck` to put it in the deck). Vanilla helpers: `energy 9`, `godmode`, `upgrade 0` (leftmost card in hand).

## Card set modes

- [ ] Custom Run shows "Slay the Spire 1 Cards" and "Spire Souvenir" (formerly "Cards From Both Games"); ticking one unticks the other. Their names and descriptions read right in English and French.
- [ ] In co-op, the client sees the host's tick.
- [ ] The top bar shows the ticked mode's short label instead of a missing icon.
- [ ] StS1-only, Ironclad: card rewards offer only `Sts1*` cards and StS2 cards that share a StS1 name (Bash, Inflame, Shrug It Off…). No StS2-only card (e.g. Ashen Strike).
- [ ] "Spire Souvenir", Ironclad: rewards mix both games.
- [ ] StS1-only with Silent, Defect, Regent or Necrobinder: their rewards stay vanilla.
- [ ] Shop, card transform, and vanilla generators (Infernal Blade, Distraction-style random cards) follow the mode.
- [ ] StS1-only or Spire Souvenir, Ironclad, with the vanilla "Silent Cards" modifier also ticked: the merchant's
      character cards include Silent cards (StS1 ones in StS1-only), like the card rewards do.
- [ ] Card library, from the main menu and from inside a run: vanilla pools only, no `Sts1*` cards.
- [ ] Save, quit and resume with StS1 cards in the deck (upgraded ones too): the cards come back as they were.
- [ ] Standard and Daily runs: rewards are vanilla even after a StS1-only Custom Run.
- [ ] Co-op StS1-only: both players are offered the same kinds of cards; no desync when rewards roll.

## StS1 Ironclad cards

- [ ] Every StS1 card shows its StS1 art, a red Ironclad frame, and its text in English and in French (no raw `STS1_…` keys).
- [ ] Cleave: 8 / 11 damage to ALL enemies.
- [ ] Clothesline (2 energy): 12 / 14 damage, 2 / 3 Weak.
- [x] Heavy Blade (2 energy): 14 damage + Strength ×3 / ×5. With 2 Strength the card reads 20 / 24. Other attacks still add Strength once.
- [ ] Pummel: 2 damage 4 / 5 times, Exhausts.
- [x] Sever Soul (2 energy): Exhausts every non-Attack in hand, then 16 / 22 damage.
- [ ] Wild Strike: 12 / 17 damage, a Wound is shuffled into the draw pile. Counts for Perfected Strike.
- [ ] Reckless Charge (0 energy): 7 / 10 damage, a Dazed is shuffled into the draw pile.
- [ ] Carnage (2 energy): Ethereal, 20 / 28 damage.
- [ ] Immolate (2 energy): 21 / 28 damage to ALL enemies, a Burn goes to the discard pile.
- [ ] Dropkick: 5 / 8 damage; if the target was Vulnerable before the hit, +1 energy and draw 1. Glows when an enemy is Vulnerable.
- [x] Blood for Blood: costs 4 / 3; each HP loss this combat (in hand, draw or discard pile) lowers it by 1. A copy made mid-combat starts already reduced. 18 / 22 damage.
- [ ] Reaper (2 energy, Exhausts): 4 / 5 damage to ALL enemies, heals the unblocked total.
- [x] Searing Blow (2 energy): 12 damage, then 16, 21, 27… The campfire offers it again after each upgrade; the title reads "Searing Blow+2", "+3"… Save, quit and resume keeps the level.
- [ ] Disarm (Exhausts): the target loses 2 / 3 Strength.
- [x] Flex (0 energy): +2 / +4 Strength, lost at the end of the turn. The buff shows the Flex Potion icon and names Flex.
- [ ] Ghostly Armor: Ethereal, 10 / 13 Block.
- [ ] Intimidate (0 energy, Exhausts): 1 / 2 Weak to ALL enemies.
- [x] Limit Break: doubles Strength (with -2 Strength it goes to -4). Exhausts; the upgrade doesn't.
- [ ] Power Through: 2 Wounds into the hand, 15 / 20 Block. With a full hand the Wounds go where vanilla sends overflow.
- [ ] Seeing Red (1 / 0 energy, Exhausts): +2 energy.
- [x] Sentinel: 5 / 8 Block. Exhausted by another card (e.g. Sever Soul, True Grit) or by Ethereal: +2 / +3 energy.
- [x] Spot Weakness: +3 / +4 Strength only if the target intends to attack. Glows when an enemy intends to attack.
- [ ] Warcry (0 energy, Exhausts): draw 1 / 2, then pick a card to put on top of the draw pile (prompt in English and French).
- [ ] Exhume (1 / 0 energy, Exhausts): pick a card from the Exhaust pile into the hand; other Exhumes aren't offered. With an empty Exhaust pile nothing happens.
- [x] Double Tap: the next 1 / 2 Attacks this turn are played twice. The buff shows the Burst icon and reads "Attack" in its tooltip; it's gone at the end of the turn.
- [ ] Berserk (0 energy): gain 2 / 1 Vulnerable; from next turn on, +1 energy at the start of each turn (before the draw). Two Berserks give +2.
- [ ] Brutality (0 energy; Innate when upgraded): after the hand is drawn each turn, draw 1 card and lose 1 HP.
- [x] Combust: at the end of the turn, lose 1 HP and deal 5 / 7 damage to ALL enemies. A second Combust makes it 2 HP and 10 (or 12/14) damage. Does nothing once every enemy is dead.
- [ ] Evolve: drawing a Status (Wound, Dazed, Burn…) draws 1 / 2 more cards, also during the start-of-turn draw.
- [ ] Fire Breathing: drawing a Status or a Curse deals 6 / 10 damage to ALL enemies (Strength doesn't add).
- [ ] Metallicize: at the end of each turn, gain 3 / 4 Block. It never decays (unlike Plating).
- [ ] Every StS1 power shows a vanilla icon (Berserk: Energy Next Turn, Brutality: Draw Cards Next Turn, Combust: Inferno, Evolve: Adaptable, Fire Breathing: Pyre, Metallicize: Plating, Double Tap: Burst, Flex: Flex Potion) and its tooltip in English and French.

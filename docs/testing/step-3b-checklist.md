# Step 3b — in-game checklist

Tick when checked in game. "Base / +" means the card before and after upgrade.
Quickest way: Custom Run, Defect, "Spire Souvenir" (Hybrid: the Defect starts with the relic) or "Slay the Spire 1 Cards" ticked.
Fastest: in combat, open the dev console (` key) and type `sts1card STS1_BARRAGE` (Tab completes the id; add `Deck` to put it in the deck). Vanilla helpers: `energy 9`, `godmode`, `upgrade 0` (leftmost card in hand).

## Defect attacks

- [ ] Blizzard (1): 2 / 3 damage to ALL enemies per Frost you Channeled this combat; in combat the text shows the total.
- [ ] Core Surge (1, Exhaust): 11 / 15 damage, gain 1 Artifact.
- [ ] Doom and Gloom (2): 10 / 14 damage to ALL enemies, then Channel 1 Dark.
- [ ] Bullseye (1): 8 / 11 damage and 2 / 3 Lock-On (crosshair icon from vanilla Tracking). Lightning (passive and Evoke) and Dark Evoke deal 50% more to that enemy; Frost and Plasma don't change; card attacks don't change. Lock-On drops by 1 at the end of each enemy turn.
- [ ] Melter (1): removes all the enemy's Block, then 10 / 14 damage.
- [ ] Streamline (2): 15 / 20 damage; each play makes that copy cost 1 less for the rest of the combat (2 → 1 → 0, never below 0); back to 2 next combat.
- [ ] Thunder Strike (3): one 7 / 9 damage hit on a random enemy per Lightning you Channeled this combat; in combat the text shows the hit count; no Lightning yet: no hit.
- [ ] StS1-only and Hybrid, Defect: Barrage, Claw and Hyperbeam show up only in their StS1 version (StS1 art).
- [ ] Barrage (1): 4 / 6 damage per Orb you hold; with no Orbs, no hit.
- [ ] Claw (0): 3 / 5 damage; every StS1 Claw (hand, piles) deals 2 more for the rest of the combat.
- [ ] Hyperbeam (2): 26 / 34 damage to ALL enemies with the StS2 beam, lose 3 Focus.
- [ ] StS1-only and Hybrid, Defect: Rebound and Rip and Tear show up in their StS1 version (StS2's are Event cards).
- [ ] Rebound (1): 9 / 12 damage; the next card you play this turn goes on top of your draw pile.
- [ ] Rip and Tear (1): 7 / 9 damage to a random enemy twice.

## Defect skills

- [ ] Aggregate (1): gain 1 Energy per 4 / 3 cards in the draw pile, rounded down; an empty pile gives none.
- [ ] Amplify (1): the next 1 / 2 Powers this turn play twice; stacks, ignores Skills and Attacks, expires at turn end, and never repeats a teammate's Power. Signal Boost icon and EN/FR power text appear.
- [ ] Auto-Shields (1): gain 11 / 15 Block only with zero current Block.
- [ ] Charge Battery (1): gain 7 / 10 Block and 1 Energy next turn.
- [ ] Chill (0, Exhaust): Channel 1 Frost per living enemy; upgrade adds Innate and keeps Exhaust.
- [ ] Consume (2): gain 2 / 3 Focus and lose 1 Orb slot, removing the last Orb without evoking if needed; at zero slots still gain Focus, never negative capacity.
- [ ] Darkness (1): Channel 1 Dark; only the upgrade triggers all Dark passives once, including the new Orb.
- [ ] Equilibrium (2): gain 13 / 16 Block and Retain the hand this turn.
- [ ] Fission (0, Exhaust): remove all Orbs without evoking / Evoke all Orbs; gain 1 Energy and draw 1 card per Orb. Empty slots give nothing; Orb capacity stays unchanged; no stale Orb sprites remain.
- [ ] Force Field (4): gain 12 / 16 Block; costs 1 less per Power played this combat, including Powers played before this card was generated; counts repeated plays, ignores teammate Powers, never costs below 0. A cloned copy keeps its discount without applying it twice.
- [ ] Fusion (2 / 1): Channel 1 Plasma; does not Exhaust.
- [ ] Genetic Algorithm (1, Exhaust): starts at 1 Block, gains 2 / 3 permanently after each play. Check next combat, upgrade/downgrade, save/reload, and both co-op clients; generated cards without a deck version do not alter another copy.
- [ ] Recursion (1 / 0): Evoke the first Orb then Channel it again at the back; Dark retains accumulated damage, channel/evoke triggers fire; no Orbs is a no-op.
- [ ] Recycle (1 / 0): choose and Exhaust a hand card, gain its current Energy cost; X uses remaining Energy, unplayable/zero-cost cards give 0, empty hand is a no-op. Check temporary cost changes and co-op selection.
- [ ] Reinforced Body (X): gain 7 / 9 Block X times; Dexterity applies per repetition; check 0 Energy, Chemical X and repeated plays.
- [ ] Reprogram (1): lose 1 / 2 Focus, gain 1 / 2 Strength and Dexterity; Artifact can block the Focus loss.
- [ ] Seek (0, Exhaust): choose 1 / 2 cards from the draw pile and put them in hand (not a draw); empty/short pile and full hand work; selection text appears in EN/FR and choices sync in co-op.
- [ ] Stack (1): Block equals discard pile size, +3 upgraded; shows computed Block in combat and applies Dexterity once.
- [ ] Steam Barrier (0): gain 6 / 8 Block; that copy loses 1 base Block each play this combat, down to 0; upgrades/downgrades keep the reduction, other copies and the next combat reset normally.
- [ ] Tempest (X, Exhaust): Channel X / X+1 Lightning; check 0 Energy, Chemical X, repeated plays and zero Orb slots.
- [ ] All skill descriptions render in English and French, including upgrade previews, Energy icons and Orb/Power hover tips.

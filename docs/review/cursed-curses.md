# Cursed Deck: every curse

Hand-reviewed table `data/curses/curses.json`; `tools/curse_table.py` writes this page. With the Unholy Grimoire
a curse costs 1 (0 upgraded) and targets one enemy: it is exhausted (Grimoire: 5 damage to ALL enemies),
then **Played** hits the chosen enemy. Left in hand, it does **In hand** to you, as in vanilla. Upgraded (+), its
number goes up on both sides; a curse with no played effect gets the Grimoire at 12 instead.

| Curse | Keywords | In hand (you) | Played (enemy) | + Played | + Grimoire | + Cost | Why |
|-------|----------|---------------|----------------|----------|------------|--------|-----|
| BAD_LUCK | Eternal, Unplayable | End of turn in hand: lose 13 HP | loses 13 HP | loses 20 HP | 5 | 0 | Owner's example: -13 HP flat to the monster, 20 upgraded, on both sides |
| DECAY | Unplayable | End of turn in hand: take 2 damage | takes 2 damage | takes 4 damage | 5 | 0 | Unpowered damage, Block stops it, like the vanilla side; doubled upgraded since 2 -> 3 is invisible |
| REGRET | Unplayable | End of turn in hand: lose 1 HP per card in hand | loses 1 HP per card in your hand | loses 2 HP per card in your hand | 5 | 0 | Counts your hand after Regret left it; no var, the upgraded player side is a patch on its turn-end damage |
| DOUBT | Unplayable | End of turn in hand: gain 1 Weak | gains 1 Weak | gains 2 Weak | 5 | 0 | Mirror effect |
| SHAME | Unplayable | End of turn in hand: gain 1 Frail | gains 1 Vulnerable | gains 2 Vulnerable | 5 | 0 | Frail barely matters on monsters (few gain Block); Vulnerable is the attacker's version of the same malus |
| DEBT | Unplayable | End of turn in hand: lose 10 gold | you gain 10 gold | you gain 15 gold | 5 | 0 | Monsters have no gold: the monster pays you. In Blood runs gold gains are 0, so it does nothing there |
| STS1_PAIN | Unplayable (StS1, Ominous Forge) | While in hand: lose 1 HP whenever you play another card | loses 1 HP per card you played this turn | loses 2 HP per card you played this turn | 5 | 0 | Enemy loses 1 HP per card you played this turn, Pain included; the mod owns the class, its +1 raises the HP lost |
| NORMALITY | Unplayable | While in hand: you can't play more than 3 cards this turn | - | - | 12 | 0 | No monster version of a card limit: Grimoire only, 12 upgraded. Its limit stays 3 (an upgrade lowering it to 2 would read as a nerf of the Grimoire) |
| ENTHRALLED | Eternal; playable for 2 in vanilla | While in hand: must be played before other cards | - | - | 12 | 0 | Already playable: keeps its cost 2, 0 upgraded; with the Grimoire it is exhausted on play (5 / 12) |
| SPORE_MIND | Exhaust; playable for 1 in vanilla | No effect (dead card) | - | - | 12 | 0 | Already exhausts when played, so it already fed the Grimoire; 0 cost and 12 upgraded |
| CLUMSY | Ethereal, Unplayable | No effect; Ethereal | - | - | 12 | 0 | Owner's example of a curse with no effect: Grimoire only (5, 12 upgraded) |
| INJURY | Unplayable | No effect | - | - | 12 | 0 | Grimoire only |
| WRITHE | Innate, Unplayable | No effect; Innate | - | - | 12 | 0 | Grimoire only; Innate makes it a reliable opening hit |
| GUILTY | Unplayable | Leaves the deck after 5 combats | - | - | 12 | 0 | Grimoire only; the countdown counts combats it spends in the deck, playing it doesn't change that |
| FOLLY | Eternal, Innate, Ethereal, Unplayable | No effect (Preserved Fog) | - | - | 12 | 0 | Grimoire only |
| GREED | Eternal, Unplayable | No effect (Cursed Pearl, Sunken Treasury) | - | - | 12 | 0 | Grimoire only |
| CURSE_OF_THE_BELL | Eternal, Unplayable | No effect (Calling Bell) | - | - | 12 | 0 | Grimoire only |
| POOR_SLEEP | Retain, Unplayable | No effect; Retain (Unrest Site) | - | - | 12 | 0 | Grimoire only; Retain lets you hold it for the right turn |
| ASCENDERS_BANE | Eternal, Ethereal, Unplayable | No effect (ascension curse) | - | - | 12 | 0 | Grimoire only; it can be upgraded like the others |
| STS1_NECRONOMICURSE | Eternal, Unplayable (StS1, Necronomicon) | When exhausted, comes back to your hand | - | - | 12 | 1 | Exhausted on play then back in hand: replayable all turn. Upgraded it keeps cost 1 (at 0 it would loop forever); Grimoire 12 per energy |

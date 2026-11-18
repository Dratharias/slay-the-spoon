# The Forge: every card

Hand-reviewed table `data/forge/cards.json` (StS2 and the mod's StS1 cards); `tools/forge_curves.py` turns it
into the runtime table `data/forge/curves.json`. Each level past +1 replays the card's +1 numbers (cost and
keywords stay as at +1) up to **Cap**; past the cap only the **Past the cap** vars keep growing. Cards whose +1
only lowers the cost keep lowering it until 0. **Power bonus** and **Play** are added per level past +1
(so +3 gives twice the amount). Every card also gets Retain at +6, cost -1 at +7 and Innate at +9.

| Card | +1 | Effect | Cap | Per level | Past the cap | Power bonus | Play | Why |
|------|----|--------|-----|-----------|--------------|-------------|------|-----|
| ABRASIVE | ThornsPower +2 (4->6) | Gain 1 Dexterity and 4 Thorns | 9 | ThornsPower +2, DexterityPower +1 | - | - | - | Thorns replay is safe; Dexterity also grows by 1 per level to fit the defensive power |
| ABUNDANCE | cost 1 -> 0 | Exhaust: choose 1 of 3 upgraded Powers, free this turn | 5 | - | - | - | Draw +1 | Cost already 0 at +1; a tiny Draw per level to find the Power's synergies, Exhaust keeps it safe, capped at 5 |
| ACCELERANT | Accelerant +1 (1->2) | Poison triggers N additional times | 5 | - | - | - | - | Extra Poison triggers are a multiplier: cap 5 |
| ACCURACY | AccuracyPower +2 (4->6) | Shivs deal N additional damage | 9 | - | - | - | - | Shiv damage bonus is safe to replay to +9 |
| ACROBATICS | Cards +1 (3->4) | Draw 3, discard 1 | 5 | - | - | - | - | Draw: cap 5 |
| ADAPTIVE_STRIKE | Damage +5 (18->23) | Deal 18 damage, add a 0-cost copy to discard | 9 | - | - | - | - | Plain damage replay |
| ADRENALINE | Energy +1 (1->2) | Gain 1 energy, draw 2, Exhaust | 3 | - | - | - | - | Energy: cap 3 |
| AFTERIMAGE | Innate | Whenever you play a card, gain 1 Block (AfterimagePower 1) | 9 | AfterimagePower +1 | - | - | - | Block per card grows by 1 per level; Innate is already given |
| AFTERLIFE | Summon +3 (6->9) | Summon 6, Exhaust | 5 | - | - | - | - | Summons: cap 5 |
| AGGRESSION | Innate | Each turn start, put a random Attack from discard into hand and Upgrade it (AggressionPower 1, hardcoded) | 5 | - | - | AggressionPower +1 | - | One more Attack recovered per turn per level (amount = attacks taken); card generation: cap 5 |
| ALCHEMIZE | cost 1 -> 0 | Exhaust: procure a random potion | 9 | - | - | - | Poison +2 | Cost hits 0 at +1; potions stay one per play, so add a small alchemical Poison per level |
| ALIGNMENT | Energy +1 (2->3) | 2 stars: gain 2 energy | 3 | - | - | - | - | Energy: cap 3 |
| ALL_FOR_ONE | Damage +4 (10->14) | Deal 10, put all 0-cost cards from discard into hand | 9 | - | - | - | - | Plain damage replay |
| ANGER | Damage +2 (6->8) | Deal 6, add a copy to discard | 9 | - | - | - | - | Plain damage replay |
| ANOINTED | Retain | Exhaust: put every Rare from draw pile into hand | 5 | - | - | - | Draw +1 | No number; it is about fetching cards, so a small Draw per level (Exhaust, cap 5) |
| ANTICIPATE | DexterityPower +2 (2->4) | Gain 2 temporary Dexterity this turn | 9 | - | - | - | - | Temporary Dexterity replay is safe |
| APOTHEOSIS | cost 2 -> 1 | Exhaust, Innate: upgrade every other card in combat | 9 | - | - | - | Strength +1, Dexterity +1 | Cost reaches 0 at +2; ascension theme: gain 1 Strength and 1 Dexterity per level (Exhaust, once per combat) |
| APPARITION | remove Ethereal | Exhaust: gain 1 Intangible | 3 | IntangiblePower +1 | - | - | - | Intangible grows by 1 per level: cap 3 |
| ARMAMENTS | IsUpgraded: upgrade every card in hand instead of one | Gain 5 Block, upgrade a card in hand | 9 | Block +3 | - | - | - | The +1 changes no number; the Block var grows by 3 per level |
| ARSENAL | Innate | Whenever you create a card, gain 1 Strength (ArsenalPower 1) | 9 | ArsenalPower +1 | - | - | - | Strength per created card grows by 1 per level; Strength is safe |
| ASHEN_STRIKE | ExtraDamage +1 (3->4) | Deal 6 + 3 per exhausted card | 9 | - | CalculationBase | - | - | Damage per exhausted card is safe to replay; base damage also keeps up |
| ASSASSINATE | Damage +3, Vulnerable +1 | 0 cost, Innate, Exhaust: deal 10, apply 1 Vulnerable | 9 | - | - | - | - | Damage and Vulnerable replay are safe |
| ASTRAL_PULSE | Damage +2 (6->8) | 3 stars: deal 6 to all enemies twice | 9 | - | - | - | - | Plain damage replay |
| AUTOMATION | cost 1 -> 0 | Every 10 cards drawn, gain 1 energy (AutomationPower = energy) | 3 | Energy +1 | - | - | - | Cost hits 0 at +1; the Energy var grows instead, energy: cap 3 (cost lowering, if any, still applies) |
| BACKFLIP | Block +3 (5->8) | Gain 5 Block, draw 2 | 9 | - | - | - | - | Block replay is safe; draw stays 2 |
| BACKSTAB | Damage +4 (11->15) | 0 cost, Innate, Exhaust: deal 11 | 9 | - | - | - | - | Plain damage replay |
| BALL_LIGHTNING | Damage +3 (7->10) | Deal 7, channel 1 Lightning | 9 | - | - | - | - | Plain damage replay |
| BANSHEES_CRY | cost 9 -> 7 | Deal 33 to all enemies; costs 2 less per Ethereal card played | 9 | Damage +5 | - | - | - | Cost-only: keeps dropping by 2 to 0 at +5; damage also grows by 5 per level so it keeps scaling |
| BARRAGE | Damage +2 (5->7) | Deal 5 once per channeled orb | 9 | - | - | - | - | Plain damage replay |
| BARRICADE | cost 3 -> 2 | Block is not removed at turn start (BarricadePower, single) | 9 | - | - | - | Block +5 | Cost-only drops to 0 at +3; the power cannot stack, so a Block bonus per level fits the theme |
| BASH | Damage +2, Vulnerable +1 | Deal 8, apply 2 Vulnerable | 9 | - | - | - | - | Damage and Vulnerable replay are safe |
| BATTLE_TRANCE | Cards +1 (3->4) | 0 cost: draw 3, no more draw this turn | 5 | - | - | - | - | Draw: cap 5 |
| BEACON_OF_HOPE | Innate | Whenever you gain Block on your turn, allies gain half of it (single stack, 50% hardcoded) | 9 | - | - | - | Block +4 | Single-stack power with a hardcoded 50%; Block on play feeds the share to allies |
| BEAM_CELL | Damage +1, Vulnerable +1 | 0 cost: deal 3, apply 1 Vulnerable | 9 | - | - | - | - | Damage and Vulnerable replay are safe |
| BEAT_DOWN | Cards +1 (3->4) | Autoplay 3 random Attacks from discard | 3 | - | - | - | - | Cards here are free extra plays of Attacks, not draw: cap 3 |
| BEAT_INTO_SHAPE | Damage +2, CalculationBase +2, CalculationExtra +2 | Deal 5, Forge 5 + 5 per time the target was hit this turn | 5 | - | CalculationBase, Damage | - | - | Forge per hit is a per-hit multiplier: cap 5; base and damage keep growing |
| BEGONE | IsUpgraded: the Minion Strike is upgraded | Transform a card in hand into a Minion Strike | 9 | - | - | - | Damage +4 | No var; a small Damage per level to the target or a random enemy fits the banishing theme |
| BELIEVE_IN_YOU | Energy +1 (2->3) | 0 cost: an ally gains 2 energy | 3 | - | - | - | - | Energy: cap 3 |
| BIASED_COGNITION | FocusPower +1 (5->6) | Gain 5 Focus, lose 1 Focus each turn | 5 | - | - | - | - | Focus: cap 5 |
| BIG_BANG | Innate | 0 cost, Exhaust: draw 1, gain 1 star, 1 energy, Forge 5 | 3 | Cards +1, Energy +1, Stars +1, Forge +5 | Forge | - | - | The +1 grows no var; everything grows by 1 (Forge by 5) per level, energy/stars: cap 3, Forge keeps growing |
| BLACK_HOLE | BlackHolePower +1 (3->4) | Whenever you gain or spend stars, deal 3 to all enemies | 9 | - | - | - | - | Damage per star event is safe to replay |
| BLADE_DANCE | Cards +1 (3->4) | Exhaust: add 3 Shivs to hand | 5 | - | - | - | - | Card generation: cap 5 |
| BLADE_OF_INK | Cards +1 (2->3) | Add 2 Inky Shivs to hand | 5 | - | - | - | - | Card generation: cap 5 |
| BLADE_SYMPHONY | cost 2 -> 1 | Multiplayer: each teammate adds 2 Shivs to hand | 5 | Cards +1 | - | - | - | Cost-only drops to 0 at +2; then more Shivs per teammate, card generation: cap 5 |
| BLAZE | StrengthPower +2 (5->7) | Multiplayer: an ally gains 5 Strength | 9 | - | - | - | - | Strength replay is safe |
| BLIGHT_STRIKE | Damage +2 (8->10) | Deal 8, apply Doom equal to damage dealt | 9 | - | - | - | - | Damage replay also grows the Doom |
| BLOODLETTING | Energy +1 (2->3) | 0 cost: lose 3 HP, gain 2 energy | 3 | - | - | - | - | Energy: cap 3; HP cost limits it |
| BLOOD_WALL | Block +4 (16->20) | Lose 2 HP, gain 16 Block | 9 | - | - | - | - | Block replay is safe |
| BLUDGEON | Damage +10 (32->42) | Deal 32 | 9 | - | - | - | - | Plain damage replay |
| BLUR | Block +3 (5->8) | Gain 5 Block, Block is kept next turn (Blur 1) | 9 | - | - | - | - | Block replay is safe |
| BODYGUARD | Summon +2 (5->7) | Summon 5 | 5 | - | - | - | - | Summons: cap 5 |
| BODY_SLAM | cost 1 -> 0 | Deal damage equal to your Block (ExtraDamage 1 x Block) | 3 | ExtraDamage +1, CalculationBase +3 | CalculationBase | - | - | Cost hits 0 at +1; Block multiplier grows by 1 to x3 at cap 3, flat base damage keeps growing |
| BOLAS | Damage +1 (3->4) | 0 cost: deal 3, returns to hand next turn | 9 | - | - | - | - | Plain damage replay |
| BOMBARDMENT | Damage +6 (18->24) | Exhaust: deal 18, replays itself from the exhaust pile each turn | 9 | - | - | - | - | Plain damage replay |
| BONE_SHARDS | OstyDamage +3, Block +3 | Osty deals 9 to all, gain 9 Block, Osty dies | 9 | - | - | - | - | Damage and Block replay are safe |
| BOOST_AWAY | Block +3 (6->9) | 0 cost: gain 6 Block, add a Dazed to discard | 9 | - | - | - | - | Block replay is safe |
| BOOT_SEQUENCE | Block +3 (10->13) | 0 cost, Innate, Exhaust: gain 10 Block | 9 | - | - | - | - | Block replay is safe |
| BORROWED_TIME | Energy +2 (4->6) | Gain 4 energy, your cards cost 1 more this turn | 3 | Energy +1 | - | - | - | Energy: cap 3, and only +1 per level past +1 (the +2 delta would reach 10) |
| BOUNCING_FLASK | Repeat +1 (3->4) | Apply 3 Poison to a random enemy 3 times | 5 | Repeat +1, PoisonPower +1 | PoisonPower | - | - | Bounces are Poison hits, not card plays: cap 5 on bounces, Poison per bounce keeps growing |
| BRAND | StrengthPower +1 (1->2) | 0 cost: lose 1 HP, exhaust a card, gain 1 Strength | 9 | - | - | - | - | Strength replay is safe |
| BREAK | Damage +10, Vulnerable +2 | Deal 20, apply 5 Vulnerable | 9 | - | - | - | - | Damage and Vulnerable replay are safe |
| BREAKTHROUGH | Damage +4 (9->13) | Lose 1 HP, deal 9 to all enemies | 9 | - | - | - | - | Plain damage replay |
| BRIGHTEST_FLAME | Energy +1, Cards +1 | 0 cost: gain 2 energy, draw 2, lose 2 max HP | 3 | - | - | - | - | Energy and draw: cap 3; max HP loss limits it |
| BUBBLE_BUBBLE | PoisonPower +3 (9->12) | Apply 9 Poison to a poisoned enemy | 9 | - | - | - | - | Poison replay is safe |
| BUFFER | BufferPower +1 (1->2) | Prevent the next HP loss (Buffer 1) | 3 | - | - | - | - | Buffer: cap 3 |
| BULK_UP | Strength +1, Dexterity +1 | Lose 1 orb slot, gain 2 Strength and 2 Dexterity | 9 | - | - | - | - | Strength/Dexterity replay is safe |
| BULLET_TIME | cost 3 -> 2 | Cards in hand are free this turn, no more draw | 9 | - | - | - | - | Cost-only: drops to 0 at +3; no draw after, so no loop |
| BULLY | ExtraDamage +1 (2->3) | 0 cost: deal 4 + 2 per Vulnerable on the target | 9 | - | - | - | - | Damage per Vulnerable is safe to replay |
| BULWARK | Block +3, Forge +3 | Gain 12 Block, Forge 10 | 9 | - | - | - | - | Block and Forge replay are safe |
| BUNDLE_OF_JOY | Cards +1 (3->4) | Exhaust: add 3 random colorless cards to hand | 5 | - | - | - | - | Card generation: cap 5 |
| BURNING_PACT | Cards +1 (2->3) | Exhaust a card, draw 2 | 5 | - | - | - | - | Draw: cap 5 |
| BURST | Skills +1 (1->2) | This turn, your next Skill is played an extra time (BurstPower) | 3 | - | - | - | - | Skills here are extra plays, not draw: cap 3 |
| BURY | Damage +11 (52->63) | Deal 52 | 9 | - | - | - | - | Plain damage replay |
| BYRDONIS_EGG | none (MaxUpgradeLevel 0) | Unplayable Quest card: adds the Hatch rest site option | no fusion | - | - | - | - | Quest card with MaxUpgradeLevel 0 and no effect to scale; fusing two eggs would only destroy one |
| BYRD_SWOOP | Damage +4 (14->18) | 0 cost: Byrdpip deals 14 | 9 | - | - | - | - | Plain damage replay |
| CACOPHONY | Damage +33 (66->99) | Every 33 cards drawn, deal 66 to a random enemy | 9 | - | - | - | - | Unpowered damage replay is safe |
| CALAMITY | cost 3 -> 2 | Whenever you play an Attack, add 1 random Attack to hand (CalamityPower 1, hardcoded) | 5 | - | - | CalamityPower +1 | - | Cost-only drops to 0 at +3; then one more Attack generated per level, card generation: cap 5 |
| CALCIFY | CalcifyPower +2 (4->6) | Osty's attacks deal 4 more damage | 9 | - | - | - | - | Osty damage bonus replay is safe |
| CALCULATED_GAMBLE | Retain | 0 cost, Exhaust: discard your hand, draw that many | 5 | - | - | - | Draw +1 | No number; one extra card drawn per level fits the reroll, Exhaust: cap 5 |
| CALL_OF_THE_VOID | Innate | Each turn, add 1 random Ethereal card to hand (Cards 1) | 5 | Cards +1 | - | - | - | The +1 grows no var; one more Ethereal card per turn per level, card generation: cap 5 |
| CALTROPS | ThornsPower +2 (3->5) | Gain 3 Thorns | 9 | - | - | - | - | Thorns replay is safe |
| CAPACITOR | Repeat +1 (2->3) | Gain 2 orb slots | 5 | - | - | - | - | Repeat is orb slots here, not plays: orbs cap 5 |
| CAPTURE_SPIRIT | Damage +1, Cards +1 | Deal 3 unblockable, shuffle 3 Souls into draw pile | 5 | - | Damage | - | - | Card generation: cap 5; damage keeps growing |
| CASCADE | IsUpgraded: X+1 cards autoplayed | X cost: autoplay the top X cards of your draw pile | 3 | - | - | - | Draw +1 | X-cost with no var; a small Draw per level refills the pile it plays from, cap 3 as it chains plays |
| CATASTROPHE | Cards +1 (2->3) | Autoplay 2 random cards from your draw pile | 3 | - | - | - | - | Cards here are free extra plays, not draw: cap 3 |
| CELESTIAL_MIGHT | Repeat +1 (3->4) | Deal 6 three times | 3 | Repeat +1, Damage +2 | Damage | - | - | Extra hits: cap 3; damage per hit also grows and keeps growing past it |
| CHAOS | Repeat +1 (1->2) | Channel 1 random orb | 5 | - | - | - | - | Repeat is orbs channeled: orbs cap 5 |
| CHARGE | IsUpgraded: the Minion Dive Bombs are upgraded | Transform 2 cards from your draw pile into Minion Dive Bombs | 5 | Cards +1 | - | - | - | The +1 grows no var; one more card transformed per level, card generation: cap 5 |
| CHARGE_BATTERY | Block +3 (7->10) | Gain 7 Block, gain 1 energy next turn | 9 | - | - | - | - | Block replay is safe; energy stays 1 |
| CHILD_OF_THE_STARS | BlockForStars +1 (2->3) | Whenever you spend stars, gain 2 Block per star | 9 | - | - | - | - | Block per star replay is safe |
| CHILL | remove Exhaust | 0 cost, Exhaust: channel 1 Frost | 9 | - | - | - | Block +2 | No var; Frost is the defensive orb, so a small Block per level fits |
| CINDER | Damage +6 (18->24) | Deal 18, exhaust a random card in hand | 9 | - | - | - | - | Plain damage replay |
| CLASH | Damage +4 (14->18) | 0 cost, only playable if hand is all Attacks: deal 14 | 9 | - | - | - | - | Plain damage replay |
| CLAW | Damage +1, Increase +1 | 0 cost: deal 3, every Claw gains 2 damage this combat | 5 | - | Damage | - | - | Growth per play is scaling: cap 5; base damage keeps growing |
| CLEANSE | Summon +2 (3->5) | Summon 3, exhaust a card from draw pile | 5 | - | - | - | - | Summons: cap 5 |
| CLOAK_AND_DAGGER | Cards +1 (1->2) | Gain 6 Block, add 1 Shiv to hand | 5 | Cards +1, Block +2 | Block | - | - | Shiv generation: cap 5; Block also grows and keeps growing past it |
| CLOAK_OF_STARS | Block +3 (7->10) | 1 star: gain 7 Block | 9 | - | - | - | - | Block replay is safe |
| COLD_SNAP | Damage +3 (6->9) | Deal 6, channel 1 Frost | 9 | - | - | - | - | Plain damage replay |
| COLLISION_COURSE | Damage +4 (10->14) | 0 cost: deal 10, add a Debris to hand | 9 | - | - | - | - | Plain damage replay |
| COLOSSUS | Block +3 (4->7) | Gain 4 Block; for 1 turn, Vulnerable enemies deal you half damage | 9 | - | - | - | - | Block replay is safe |
| COMET | Damage +11 (33->44) | 5 stars: deal 33, apply 3 Weak and 3 Vulnerable | 9 | - | - | - | - | Plain damage replay |
| COMPACT | Block +1 (6->7); IsUpgraded: the Fuel cards are upgraded | Gain 6 Block, transform Statuses in hand into Fuel | 9 | Block +3 | - | - | - | A +1 Block delta is too small per level; Block grows by 3 instead |
| COMPILE_DRIVER | Damage +3 (7->10) | Deal 7, draw 1 per distinct orb type | 9 | - | - | - | - | Damage replay; the draw stays tied to orbs |
| CONCOCT | ConcoctPower +1 (3->4) | Multiplayer: this turn an ally's unblocked attacks apply 3 Poison | 9 | - | - | - | - | Poison per hit replay is safe |
| CONFLAGRATION | Repeat +1 (4->5) | Deal 2 to all enemies 4 times | 3 | Repeat +1, Damage +1 | Damage | - | - | Extra hits: cap 3; damage per hit also grows and keeps growing past it |
| CONQUEROR | Forge +2 (3->5) | Forge 3; the target takes double damage from Sovereign Blade for 1 turn (ConquerorPower 1, hardcoded) | 5 | - | Forge | ConquerorPower +1 | - | Forge keeps growing; the double-damage window lasts 1 more turn per level, a multiplier: cap 5 |
| CONSTELLATION | Block +3 (9->12) | Multiplayer, 2 stars: an ally draws 1, gains 1 energy and 9 Block | 9 | - | - | - | - | Block replay is safe; draw and energy stay 1 |
| CONSUMING_SHADOW | Repeat +1 (2->3) | Channel 2 Dark orbs; at turn end evoke your last orb (ConsumingShadowPower 1) | 5 | - | - | - | - | Repeat is Dark orbs channeled: orbs cap 5 |
| CONVERGENCE | Stars +1 (1->2) | Retain your hand; next turn gain 1 energy and 1 star | 3 | - | - | - | - | Stars: cap 3 |
| COOLANT | CoolantPower +1 (2->3) | Each turn, gain 2 Block per distinct orb type | 9 | - | - | - | - | Block replay is safe |
| COOLHEADED | Cards +1 (1->2) | Channel 1 Frost, draw 1 | 5 | - | - | - | - | Draw: cap 5 |
| COORDINATE | StrengthPower +3 (5->8) | Multiplayer: an ally gains 5 temporary Strength | 9 | - | - | - | - | Temporary Strength replay is safe |
| CORROSIVE_WAVE | CorrosiveWave +1 (2->3) | This turn, whenever you draw a card, apply 2 Poison to all enemies | 9 | - | - | - | - | Poison per draw replay is safe |
| CORRUPTION | cost 3 -> 2 | Skills cost 0 and Exhaust when played (CorruptionPower, single stack) | 9 | - | - | - | Strength +1 | Cost-only drops to 0 at +3; the power cannot stack, so a dark Strength gain per level |
| COSMIC_INDIFFERENCE | Block +3 (6->9) | Gain 6 Block, put a card from discard on top of draw pile | 9 | - | - | - | - | Block replay is safe |
| COUNTDOWN | CountdownPower +3 (6->9) | Each turn, apply 6 Doom to a random enemy | 9 | - | - | - | - | Doom replay is safe |
| CRASH_LANDING | Damage +5 (21->26) | Deal 21 to all enemies, fill your hand with Debris | 9 | - | - | - | - | Plain damage replay |
| CREATIVE_AI | cost 3 -> 2 | Each turn, add 1 random Power to hand (CreativeAi 1) | 5 | CreativeAi +1 | - | - | - | Cost-only drops to 0 at +3; then one more Power per turn per level, card generation: cap 5 |
| CRESCENT_SPEAR | ExtraDamage +1 (2->3) | 1 star: deal 8 + 2 per star-cost card you own | 9 | - | - | - | - | Damage per star card replay is safe |
| CRIMSON_MANTLE | CrimsonMantlePower +3 (7->10) | Each turn, take self damage (1 per Mantle played) and gain 7 Block | 9 | - | - | - | - | Block replay is safe |
| CRUELTY | CrueltyPower +25 (25->50) | Vulnerable enemies take 25% more attack damage | 5 | - | - | - | - | A damage multiplier: cap 5 (+150%) |
| CRUSH_UNDER | Damage +1, StrengthLoss +1 | Deal 8 to all enemies, they lose 1 Strength this turn | 9 | - | - | - | - | Damage and temporary Strength loss replay are safe |
| DAGGER_SPRAY | Damage +2 (4->6) | Deal 4 to all enemies twice | 9 | - | - | - | - | Plain damage replay |
| DAGGER_THROW | Damage +3 (9->12) | Deal 9, draw 1, discard 1 | 9 | - | - | - | - | Plain damage replay |
| DANSE_MACABRE | DanseMacabrePower +2 (4->6) | Whenever you play a card costing 2+, gain 4 Block | 9 | - | - | - | - | Block replay is safe |
| DARKNESS | IsUpgraded: Dark orbs trigger their passive twice | Channel 1 Dark orb, trigger every Dark orb's passive | 9 | - | - | - | Damage +3 | No var; a small dark Damage per level to a random enemy fits the orb's theme |
| DARK_EMBRACE | cost 2 -> 1 | Whenever a card is exhausted, draw 1 (DarkEmbracePower 1, hardcoded) | 3 | - | - | DarkEmbracePower +1 | - | Cost-only drops to 0 at +2; then one more draw per exhaust per level, a per-trigger draw: cap 3 |
| DARK_SHACKLES | StrengthLoss +6 (9->15) | 0 cost, Exhaust: the enemy loses 9 Strength this turn | 9 | - | - | - | - | Temporary Strength loss replay is safe |
| DASH | Damage +3, Block +3 | Gain 10 Block, deal 10 | 9 | - | - | - | - | Damage and Block replay are safe |
| DEADLY_POISON | PoisonPower +2 (5->7) | Apply 5 Poison | 9 | - | - | - | - | Poison replay is safe |
| DEATHBRINGER | Doom +5 (21->26) | Apply 21 Doom and 1 Weak to all enemies | 9 | - | - | - | - | Doom replay is safe |
| DEATHS_DOOR | Block +1 (6->7) | Gain 6 Block; twice more if you applied Doom this turn | 9 | Block +2 | - | - | - | A +1 Block delta is too small per level; Block grows by 2 instead (it is gained up to 3 times) |
| DEATH_MARCH | CalculationBase +1, ExtraDamage +2 | 8 dmg + 4 per card drawn this turn outside hand draw | 9 | - | - | - | - | damage only; full replay is safe |
| DEBILITATE | Damage +2, DebilitatePower +1 turn | 10 dmg; Vulnerable/Weak doubled on target for 2 turns | 9 | - | - | - | - | damage and debuff turns are safe |
| DECISIONS_DECISIONS | Cards +2 | Draw 3, then autoplay a chosen Skill 3 times (6 stars, Exhaust) | 3 | - | - | - | - | draw grows by 2 per level; hand is full by +3 |
| DEFEND_DEFECT | Block +3 | Gain 5 Block | 9 | - | - | - | - | block only; full replay |
| DEFEND_IRONCLAD | Block +3 | Gain 5 Block | 9 | - | - | - | - | block only; full replay |
| DEFEND_NECROBINDER | Block +3 | Gain 5 Block | 9 | - | - | - | - | block only; full replay |
| DEFEND_REGENT | Block +3 | Gain 5 Block | 9 | - | - | - | - | block only; full replay |
| DEFEND_SILENT | Block +3 | Gain 5 Block | 9 | - | - | - | - | block only; full replay |
| DEFILE | Damage +4 | 13 dmg, Ethereal | 9 | - | - | - | - | damage only |
| DEFLECT | Block +3 | 0 cost: gain 4 Block | 9 | - | - | - | - | block only |
| DEFRAGMENT | FocusPower +1 | Gain 1 Focus | 5 | - | - | - | - | Focus: cap 5 |
| DEFY | Block +3 | Gain 6 Block, apply 1 Weak (Ethereal) | 9 | - | - | - | - | block grows; Weak stays 1 |
| DELAY | Block +2, Energy +1 | Gain 11 Block, 1 energy next turn | 3 | - | Block | - | - | next-turn energy capped at 3 |
| DEMESNE | cost 3 -> 2 | Power: +1 max energy and +1 hand draw per turn (Ethereal) | 9 | - | - | - | - | cost-only reaches 0 at +3; no powerBonus since amount is energy+draw per turn |
| DEMONIC_SHIELD | removes Exhaust | Lose 1 HP, give an ally Block equal to your Block (co-op only) | 9 | CalculationBase +2 | - | - | - | flat Block on top of the copied Block, the shield gets thicker |
| DEMON_FORM | StrengthPower +1 | Power: gain 3 Strength each turn | 9 | - | - | - | - | Strength is safe; full replay to +9 |
| DEVASTATE | Damage +10 | 35 dmg (4 stars) | 9 | - | - | - | - | damage only |
| DEVOUR_LIFE | DevourLifePower +1 | Power: playing a Soul summons Osty for N | 5 | - | - | - | - | summons: cap 5 |
| DIRGE | Summon +1; IsUpgraded upgrades the X Souls | X cost: summon 3 X times, add X Souls to draw pile (Exhaust) | 5 | - | - | - | - | summons: cap 5; Souls stay at +1 |
| DISCOVERY | removes Exhaust | Choose 1 of 3 class cards, free this turn | 9 | - | - | - | Block +2 | no number; small Block (Draw/Energy would loop once the ladder makes it 0 cost) |
| DISMANTLE | Damage +2 | 8 dmg, hits twice on Vulnerable | 9 | - | - | - | - | damage only |
| DISTRACTION | cost 1 -> 0 | Add a random Skill to hand, free this turn (Exhaust) | 9 | - | - | - | Weak +1 | cost already 0 at +1; Weak fits a distraction |
| DODGE_AND_ROLL | Block +2 | Gain 4 Block, same Block next turn | 9 | - | - | - | - | block only |
| DOMINATE | VulnerablePower +1 | Apply 1 Vulnerable, gain Strength equal to target's Vulnerable (Exhaust) | 9 | - | - | - | - | Vulnerable and Strength grow together; safe |
| DOUBLE_ENERGY | cost 1 -> 0 | Double your energy (Exhaust) | 3 | - | - | - | Energy +1 | 0 at +1; tiny extra energy, Exhaust bounds it |
| DOWSING | none (MaxUpgradeLevel 0) | Quest: after 5 unknown rooms transforms into Abundance | no fusion | - | - | - | - | quest card cannot be upgraded at all; fusing it is meaningless |
| DRAIN_POWER | Damage +2, Cards +1 | 10 dmg, upgrade 2 random cards in discard | 5 | - | Damage | - | - | upgrade count: cap 5; damage keeps growing |
| DRAMATIC_ENTRANCE | Damage +4 | 0 cost, 11 dmg to all, Innate, Exhaust | 9 | - | - | - | - | damage only |
| DREDGE | adds Retain | Put 3 cards from discard into hand (Exhaust) | 5 | Cards +1 | - | - | - | Retain changes no number; grow the cards fetched |
| DRUM_OF_BATTLE | Energy +1 | Draw 2; when exhausted gain 2 energy | 3 | - | - | - | - | energy: cap 3 |
| DUALCAST | cost 1 -> 0 | Evoke front orb twice | 9 | - | - | - | Damage +3 | 0 at +1; a small hit like a third evoke |
| DUAL_WIELD | Cards +1 | Add 1 copy of an Attack/Power in hand | 5 | - | - | - | - | card generation: cap 5 |
| DYING_STAR | Damage +2, StrengthLoss +2 | 9 dmg to all, temporary Strength loss 9 (Ethereal, 3 stars) | 9 | - | - | - | - | damage and temp Strength loss are safe |
| ECHOING_SLASH | Damage +3 | 10 dmg to all, repeats per kill | 9 | - | - | - | - | damage only |
| ECHO_FORM | removes Ethereal | Power: first card each turn is played twice | 3 | EchoForm +1 | - | - | - | extra plays: +1 echoed card per level, cap 3 |
| EIDOLON | cost 2 -> 1 | Autoplay every Ethereal card in exhaust pile (Exhaust) | 9 | - | - | - | - | cost-only reaches 0 at +2; fine |
| END_OF_DAYS | Doom +8 | Apply 29 Doom to all, kill doomed | 9 | - | - | - | - | Doom is a damage stand-in; safe |
| ENERGY_SURGE | Energy +1 | All players gain 2 energy (co-op, Exhaust) | 3 | - | - | - | - | energy: cap 3 |
| ENFEEBLING_TOUCH | StrengthLoss +3 | Enemy temporarily loses 8 Strength (Ethereal) | 9 | - | - | - | - | debuff only; safe |
| ENLIGHTENMENT | IsUpgraded: cost 1 for the whole combat instead of this turn | Hand cards cost at most 1 (Exhaust, 0 cost) | 5 | - | - | - | Draw +1 | no number; drawing more cards to discount fits; Exhaust bounds it |
| ENTRENCH | cost 2 -> 1 | Double your Block | 9 | - | - | - | - | cost-only reaches 0 at +2 |
| ENTROPY | adds Innate | Power: each turn transform 1 card in hand | 3 | Cards +1 | - | - | - | Innate gives no number; more chaos per turn, kept small (exact select count) |
| ENVENOM | EnvenomPower +1 | Power: unblocked attack hits apply 1 Poison | 9 | - | - | - | - | poison is safe; full replay |
| EQUILIBRIUM | Block +3 | Gain 13 Block, retain hand this turn | 9 | - | - | - | - | block only |
| ERADICATE | Damage +3 | X cost: 11 dmg X times (Retain) | 9 | - | - | - | - | damage only |
| ESCAPE_PLAN | Block +2 | 0 cost: draw 1, gain 3 Block if Skill | 9 | - | - | - | - | block only |
| ETERNAL_ARMOR | PlatingPower +3 | Gain 9 Plating | 9 | - | - | - | - | block-like; safe |
| EVIL_EYE | Block +3 | Gain 8 Block, twice if a card was exhausted this turn | 9 | - | - | - | - | block only |
| EXPECT_A_FIGHT | CalculationBase +1, CalculationExtra +3 | Block 15 + 5 per Strength | 5 | - | CalculationBase | - | - | per-Strength multiplier: cap 5; base keeps growing |
| EXPERTISE | Cards +1 | Draw 2, they Retain this turn | 5 | - | - | - | - | draw: cap 5 |
| EXPOSE | Power (Vulnerable) +1 | Remove Block and Artifact, apply 2 Vulnerable (Exhaust) | 9 | - | - | - | - | Vulnerable is safe |
| EXTERMINATE | Damage +1 | 3 dmg x4 to all | 9 | - | - | - | - | damage only |
| FADE | Dexterity +3 | Ally gains 6 temporary Dexterity (co-op, Retain) | 9 | - | - | - | - | temporary Dexterity is safe |
| FALLING_STAR | Damage +4 | 8 dmg, 1 Weak, 1 Vulnerable (2 stars) | 9 | - | - | - | - | damage only |
| FAN_OF_KNIVES | Shivs +1 | Power: Shivs hit all enemies; add 4 Shivs | 5 | - | - | - | - | card generation: cap 5 |
| FASTEN | ExtraBlock +2 | Power: Defend cards gain +4 Block | 9 | - | - | - | - | block only |
| FEAR | Damage +1, Vulnerable +1 | 7 dmg, 1 Vulnerable (Ethereal) | 9 | - | - | - | - | safe vars |
| FEED | Damage +2, MaxHp +1 | 10 dmg; on kill gain 3 max HP (Exhaust) | 3 | - | Damage | - | - | max HP: cap 3; damage keeps growing |
| FEEDING_FRENZY | Strength +2 | Gain 5 temporary Strength | 9 | - | - | - | - | temporary Strength is safe |
| FEEL_NO_PAIN | Power +1 | Power: gain 3 Block whenever a card is exhausted | 9 | - | - | - | - | block only |
| FERAL | cost 2 -> 1 | Power: first 0-cost Attack each turn returns to hand | 9 | - | - | - | - | cost-only reaches 0 at +2; extra returns would be extra plays, left alone |
| FETCH | OstyDamage +3 | Osty deals 3; draw 1 first time each turn | 9 | - | - | - | - | damage only |
| FIEND_FIRE | Damage +3 | Exhaust hand, 7 dmg per card | 9 | - | - | - | - | damage only |
| FIGHT_ME | Damage +1, Strength +1 | 5 dmg x2, gain 3 Strength, enemy gains 1 | 9 | - | - | - | - | Strength and damage are safe |
| FIGHT_THROUGH | Block +4 | Gain 13 Block, 2 Wounds to discard | 9 | - | - | - | - | block only |
| FINESSE | Block +3 | 0 cost: 4 Block, draw 1 | 9 | - | - | - | - | block only; draw stays 1 |
| FINISHER | Damage +2 | 6 dmg per Attack played this turn | 9 | - | - | - | - | damage only |
| FISTICUFFS | Damage +2 | 7 dmg, gain Block equal to damage | 9 | - | - | - | - | damage/block only |
| FLAK_CANNON | Damage +3 | Exhaust statuses, 8 dmg per status | 9 | - | - | - | - | damage only |
| FLAME_BARRIER | Block +4, DamageBack +2 | 12 Block, thorns 4 this turn | 9 | - | - | - | - | safe vars |
| FLANKING | cost 2 -> 1 | Target takes x2 attack damage from other players this turn (co-op) | 5 | - | - | FlankingPower +1 | - | cost hits 0 at +2; the hardcoded x2 multiplier grows instead (multiplier: cap 5) |
| FLASH_OF_STEEL | Damage +3 | 0 cost: 5 dmg, draw 1 | 9 | - | - | - | - | damage only |
| FLATTEN | OstyDamage +4 | Osty deals 12; free after Osty attacked | 9 | - | - | - | - | damage only |
| FLECHETTES | Damage +2 | 5 dmg per Skill in hand | 9 | - | - | - | - | damage only |
| FLICK_FLACK | Damage +2 | 7 dmg to all (Sly) | 9 | - | - | - | - | damage only |
| FOCUSED_STRIKE | Damage +2, FocusPower +1 | 9 dmg, temporary Focus 1 | 5 | - | Damage | - | - | Focus: cap 5; damage keeps growing |
| FOOTWORK | Dexterity +1 | Gain 2 Dexterity | 9 | - | - | - | - | Dexterity is safe |
| FORBIDDEN_GRIMOIRE | cost 2 -> 1 | Power (Eternal): a card removal reward after each combat | 3 | - | - | ForbiddenGrimoirePower +1 | - | cost reaches 0 at +2; one more removal reward per level (economy: cap 3) |
| FOREGONE_CONCLUSION | Cards +1 | Next turn, put 2 chosen draw-pile cards in hand | 5 | - | - | - | - | card selection count: cap 5 |
| FORGOTTEN_RITUAL | Energy +1 | Gain 3 energy (Exhaust) | 3 | - | - | - | - | energy: cap 3 |
| FRIENDSHIP | StrengthPower -1 (lose 1 Strength instead of 2) | Lose 2 Strength, +1 max energy per turn | 9 | - | - | - | - | replayed -1 turns the Strength loss into a Strength gain from +3: fun, Strength is safe; energy stays 1 |
| FTL | Damage +1, PlayMax +1 | 0 cost: 5 dmg, draw 1 if fewer than 3 cards played | 3 | - | Damage | - | - | draw condition: cap 3; damage keeps growing |
| FUEL | Energy +1 | 0 cost: gain 1 energy (Exhaust) | 3 | - | - | - | - | energy: cap 3 |
| FURNACE | Forge +2 | Power: Forge 5 each turn | 9 | - | - | - | - | Forge is damage scaling; safe |
| FUSION | removes Exhaust | Channel 1 Plasma | 9 | - | - | - | Block +2 | no number; small Block, Plasma energy left alone (0-cost via ladder) |
| GAMMA_BLAST | Damage +5 | 13 dmg, 2 Weak, 2 Vulnerable (3 stars) | 9 | - | - | - | - | damage only |
| GANG_UP | ExtraDamage +2 | 5 dmg + 5 per ally hit on target this turn (co-op) | 9 | - | - | - | - | damage only |
| GATHER_LIGHT | Block +3 | 8 Block, gain 1 star | 9 | - | - | - | - | block grows; star stays 1 |
| GENESIS | StarsPerTurn +1 | Power: gain 2 stars each turn | 3 | - | - | - | - | stars: cap 3 |
| GENETIC_ALGORITHM | Increase +1 | Gain Block; Block grows 3 permanently per play (Exhaust) | 5 | - | - | - | - | scaling per play: cap 5 |
| GIANT_ROCK | Damage +4 | 20 dmg | 9 | - | - | - | - | damage only |
| GLACIER | Block +3 | 6 Block, channel 2 Frost | 9 | - | - | - | - | block only |
| GLASSWORK | Block +3 | 5 Block, channel 1 Glass | 9 | - | - | - | - | block only |
| GLIMMER | Cards +1 | Draw 3, put 1 back on top | 5 | - | - | - | - | draw: cap 5 |
| GLIMPSE_BEYOND | Cards +1 | Every player adds 3 Souls to draw pile (co-op, Exhaust) | 5 | - | - | - | - | card generation: cap 5 |
| GLITTERSTREAM | Block +2, BlockNextTurn +2 | 11 Block now, 5 next turn | 9 | - | - | - | - | block only |
| GLOW | Stars +1 | Gain 1 star, draw 1 now and 1 next turn | 3 | - | - | - | - | stars: cap 3 |
| GOLD_AXE | adds Retain | Deal 1 dmg per card played this combat | 9 | ExtraDamage +1 | - | - | - | Retain gives no number; grow the per-card damage (damage is safe) |
| GO_FOR_THE_EYES | Damage +1, Weak +1 | 0 cost: 3 dmg, 1 Weak if target attacks | 9 | - | - | - | - | safe vars |
| GRAND_FINALE | Damage +15 | 0 cost: 60 dmg to all when draw pile is empty | 9 | - | - | - | - | damage only |
| GRAVEBLAST | Damage +2, removes Exhaust | 4 dmg, return a discard card to hand | 9 | - | - | - | - | damage grows; full replay |
| GRAVE_WARDEN | Block +3 | 8 Block, add 1 Soul to draw pile | 9 | - | - | - | - | block only |
| GUARDS | IsUpgraded: the Minion Sacrifices are upgraded | Transform any hand cards into Minion Sacrifice (Exhaust) | 9 | - | - | - | Block +3 | no number; Block fits the guard theme |
| GUIDING_STAR | Damage +1, Cards +1 | 12 dmg, draw 2 next turn (1 star) | 5 | - | Damage | - | - | draw: cap 5; damage keeps growing |
| GUNK_UP | Damage +1 | 4 dmg x3, add Slimed to discard | 9 | - | - | - | - | damage only |
| HAILSTORM | HailstormPower +2 | Power: end of turn with Frost, 6 dmg to all | 9 | - | - | - | - | damage only |
| HAMMER_TIME | cost 2 -> 1 | Power: your Forges also forge for allies (co-op) | 9 | - | - | - | - | cost reaches 0 at +2; single-stack power, nothing to scale |
| HAND_OF_GREED | Damage +5, Gold +5 | 20 dmg; on kill gain 20 gold | 3 | - | Damage | - | - | gold: cap 3; damage keeps growing |
| HAND_TRICK | Block +3 | 7 Block, a Skill in hand gains Sly this turn | 9 | - | - | - | - | block only |
| HANG | Damage +3 | 10 dmg, Hang power doubles | 9 | - | - | - | - | damage only |
| HAUNT | HpLoss +2 | Power: playing a Soul deals 7 to a random enemy | 9 | - | - | - | - | damage only |
| HAVOC | cost 1 -> 0 | Autoplay top card of draw pile and exhaust it | 9 | - | - | - | Damage +4 | 0 at +1; a chaotic random hit |
| HAZE | Poison +2, Weak +1 | 4 Poison and 1 Weak to all | 9 | - | - | - | - | safe vars |
| HEADBUTT | Damage +3 | 9 dmg, put a discard card on top of draw pile | 9 | - | - | - | - | damage only |
| HEAVENLY_DRILL | Damage +2 | X cost: 8 dmg X times, doubled at 4+ energy | 9 | - | - | - | - | damage only |
| HEGEMONY | Damage +3, Energy +1 | 15 dmg, 2 energy next turn | 3 | - | Damage | - | - | energy: cap 3; damage keeps growing |
| HEIRLOOM_HAMMER | Damage +5 | 20 dmg, add a copy of a colorless card in hand | 9 | - | - | - | - | damage grows; copy count stays 1 |
| HELIX_DRILL | Damage +2 | 0 cost: 3 dmg per energy spent this turn | 9 | - | - | - | - | damage only |
| HELLO_WORLD | adds Innate | Power: add 1 random Common card to hand each turn | 5 | - | - | HelloWorldPower +1 | - | Innate gives no number; one more Common per turn per level (card generation: cap 5) |
| HELLRAISER | cost 2 -> 1 | Power: drawn Strikes are autoplayed | 9 | - | - | - | - | cost reaches 0 at +2; single-stack power |
| HEMOKINESIS | Damage +5 | Lose 2 HP, 15 dmg | 9 | - | - | - | - | damage only |
| HIBERNATE | Repeat +1 | Hibernate power, channel 2 Frost (co-op) | 5 | - | - | - | - | Repeat is Frost orbs channeled: orbs cap 5 |
| HIDDEN_CACHE | StarNextTurnPower +1 | Gain 1 star, 3 stars next turn | 3 | - | - | - | - | stars: cap 3 |
| HIDDEN_DAGGERS | IsUpgraded: Shivs are upgraded | 0 cost: discard 2, add 2 Shivs | 5 | Shivs +1 | - | - | - | no number grows; one more Shiv per level (card generation: cap 5) |
| HIDDEN_GEM | Replay +1 | A random draw-pile card gains Replay 2 | 3 | - | - | - | - | replays: cap 3 |
| HIGH_FIVE | OstyDamage +2, Vulnerable +1 | Osty deals 11 to all, 2 Vulnerable to all | 9 | - | - | - | - | safe vars |
| HOLOGRAM | Block +2, removes Exhaust | 3 Block, return a discard card to hand | 9 | - | - | - | - | block grows; full replay |
| HOTFIX | removes Exhaust | 0 cost: gain 2 temporary Focus | 5 | FocusPower +1 | - | - | - | no var grows on +1; more temporary Focus per level (Focus: cap 5) |
| HOWL_FROM_BEYOND | Damage +6 | 18 dmg to all, replays from exhaust | 9 | - | - | - | - | damage only |
| HUDDLE_UP | Cards +1 | All players draw 2 (co-op, Exhaust) | 5 | - | - | - | - | draw: cap 5 |
| HYPERBEAM | Damage +6 | 24 dmg to all, lose 3 Focus next turn | 9 | - | - | - | - | damage grows; Focus loss stays |
| ICE_LANCE | Damage +5 | 19 dmg, channel 3 Frost | 9 | - | - | - | - | damage only; Frost count stays |
| IGNITION | removes Exhaust | an ally Channels Plasma (MP only) | 9 | - | - | - | Vigor +2 | no number to grow; Vigor fits 'ignition' and stays a self buff, unlike energy |
| IMITATION_LEARNING | ImitationLearningPower 2 -> 3 | copies the ally's next N Power plays for you (autoplay) | 3 | - | - | - | - | each stack is a free replay of a Power: replay cap |
| IMPATIENCE | Cards 2 -> 3 | 0-cost: if no Attacks in hand, draw N | 5 | - | - | - | - | draw |
| IMPERVIOUS | Block 30 -> 40 | gain Block, Exhaust | 9 | - | - | - | - | Block grows freely |
| INFERNAL_BLADE | cost 1 -> 0 | add a random Attack to hand, free this turn; Exhaust | 9 | - | - | - | Vigor +3 | cost-only card already at 0 after +1; Vigor powers up the free Attack it hands you |
| INFERNO | InfernoPower 6 -> 9 | lose 1 HP each turn; whenever you lose HP on your turn, damage ALL enemies | 9 | - | - | - | - | AoE damage per trigger, safe to grow |
| INFINITE_BLADES | gains Innate | InfiniteBladesPower 1 (hardcoded): add N Shivs to hand each turn | 5 | - | - | InfiniteBladesPower +1 | - | one more Shiv per turn per level past +1 (+9: 9 Shivs a turn); Shivs still cost plays, not energy loops Cap 5 (lead review: card generation). |
| INFLAME | StrengthPower 2 -> 3 | gain Strength | 9 | - | - | - | - | Strength is safe |
| INTERCEPT | Block 9 -> 13 | gain Block, redirect attacks on an ally to you (MP only) | 9 | - | - | - | - | Block grows freely |
| INVOKE | Summon 2 -> 3, Energy 2 -> 3 | next turn Summon N and gain energy | 3 | - | - | - | - | energy |
| IRON_WAVE | Damage 5 -> 7, Block 5 -> 7 | deal damage and gain Block | 9 | - | - | - | - | safe vars |
| ITERATION | IterationPower 2 -> 3 | first Status drawn each turn draws N cards | 5 | - | - | - | - | draw |
| I_AM_INVINCIBLE | Block 10 -> 13 | gain Block; auto-plays itself if on top of draw pile at end of turn | 9 | - | - | - | - | Block grows freely |
| JACKPOT | Damage 25 -> 30; IsUpgraded: the 3 generated 0-cost cards are upgraded | deal damage, add 3 random 0-cost cards to hand | 9 | - | - | - | - | Damage replay is enough; the generated count stays 3 so it never floods hand |
| JACK_OF_ALL_TRADES | Cards 1 -> 2 | add N random Colorless cards to hand; Exhaust | 5 | - | - | - | - | card generation |
| JUGGERNAUT | JuggernautPower 6 -> 8 | whenever you gain Block, deal N damage to a random enemy | 9 | - | - | - | - | damage per Block gain, safe |
| JUGGLING | gains Innate | JugglingPower 1 (hardcoded): 3rd Attack each turn adds N copies of it to hand | 5 | - | - | JugglingPower +1 | - | one more copy per level past +1; copies still cost energy, once per turn Cap 5 (lead review: card copies). |
| KINGLY_KICK | Damage 27 -> 35 | 4-cost attack, costs 1 less each time it's drawn | 9 | - | - | - | - | Damage grows freely |
| KINGLY_PUNCH | Damage 8 -> 10, Increase 4 -> 6 | attack; gains Increase damage each time it's drawn | 5 | - | Damage | - | - | growth per draw capped, base Damage keeps growing |
| KNIFE_TRAP | IsUpgraded: upgrades each Shiv before replaying it | replay every Shiv in your exhaust pile at the target | 9 | - | - | - | Damage +5 | no number; extra unpowered damage to the trap's target fits the knife theme |
| KNOCKDOWN | Damage 10 -> 14, KnockdownPower 2 -> 3 | attack; target takes xN attack damage from other players this turn (MP) | 5 | - | Damage | - | - | KnockdownPower is a damage multiplier: cap it, keep raw Damage growing |
| KNOCKOUT_BLOW | Damage 30 -> 38 | attack; gain 5 Stars if it kills | 9 | - | - | - | - | Damage grows; Stars bonus stays hardcoded at 5 |
| KNOW_THY_PLACE | removes Exhaust | 0-cost: apply 1 Weak and 1 Vulnerable | 9 | WeakPower +1, VulnerablePower +1 | - | - | - | no var grows by default; +1 Weak and Vulnerable per level fits the card |
| LANTERN_KEY | none (MaxUpgradeLevel 0) | quest card: Unplayable; forces act 3 '?' rooms into the War Historian event | no fusion | - | - | - | - | quest item with MaxUpgradeLevel 0; fusing it makes no sense |
| LARGESSE | IsUpgraded: the generated Colorless card is upgraded | 0-cost: give an ally a random Colorless card (MP) | 9 | - | - | - | Block +3 | no number; a small self Block per level keeps the generous 0-cost card a non-loop |
| LEADING_STRIKE | Damage 3 -> 6 | attack, then add 2 Shivs to hand | 9 | - | - | - | - | Damage replay; Shiv count stays 2 |
| LEAP | Block 9 -> 12 | gain Block | 9 | - | - | - | - | safe |
| LEGION_OF_BONE | Summon 6 -> 8 | every living player Summons N (MP) | 5 | - | - | - | - | summons |
| LEG_SWEEP | Block 11 -> 14, Weak 2 -> 3 | gain Block, apply Weak | 9 | - | - | - | - | safe vars |
| LETHALITY | LethalityPower 50 -> 75 | first Attack each turn deals +N% damage | 9 | - | - | - | - | percent damage on one attack per turn: safe to grow (+9: +275%) |
| LIFT | Block 11 -> 16 | give an ally Block (MP) | 9 | - | - | - | - | safe |
| LIGHTNING_ROD | Block 4 -> 7 | gain Block; Channel 1 Lightning at the start of the next N turns (LightningRodPower 2) | 5 | Block +3, LightningRodPower +1 | Block | - | - | the +1 never grows the orb turns; add 1 turn per level up to the orb cap, Block keeps growing |
| LOOP | Loop 1 -> 2 | LoopPower: trigger first orb's passive N extra times each turn | 3 | - | - | - | - | extra triggers |
| LUMINESCE | Energy 2 -> 3 | 0-cost token: gain energy; Retain, Exhaust | 3 | - | - | - | - | energy |
| LUNAR_BLAST | Damage 4 -> 5 | 0-cost: hit once per Skill played this turn | 9 | - | - | - | - | per-hit damage, safe |
| MACHINE_LEARNING | gains Innate | MachineLearningPower: draw N (Cards 1) extra cards each turn | 3 | Cards +1 | - | - | - | the +1 never grows Cards; +1 draw per turn per level, low cap since it is permanent per-turn draw |
| MAD_SCIENCE | gains Innate | Tinker Time card: Attack (12 dmg) / Skill (8 Block) / Power, with a rider (Weak/Vuln, Strangle, energy, draw, Str/Dex, ...) | 9 | Damage +3, Block +3, SappingWeak +1, SappingVulnerable +1, ChokingDamage +2, ExpertiseStrength +1, ExpertiseDexterity +1 | - | - | - | no var grows by default; grow the safe numbers (damage, block, debuffs, Strangle, Str/Dex), leave energy/draw/hits/cost riders alone |
| MAKE_IT_SO | Damage 6 -> 9 | 0-cost attack; returns to hand every 3rd Skill played in a turn | 9 | - | - | - | - | Damage only; the return threshold stays 3 |
| MALAISE | IsUpgraded: X+1 | X cost: enemy loses X Strength and gains X Weak; Exhaust | 9 | - | - | StrengthPower -1, WeakPower +1 | - | X+1 never grows; each level past +1 adds 1 more Strength loss and Weak (negative additive on StrengthPower) |
| MANGLE | Damage 20 -> 26, StrengthLoss 10 -> 15 | attack; target loses N Strength this turn (ManglePower) | 9 | - | - | - | - | temporary Strength loss and damage, safe |
| MANIFEST_AUTHORITY | Block 7 -> 8; IsUpgraded: the generated Colorless card is upgraded | gain Block, add a random Colorless card to hand | 9 | Block +3 | - | - | - | a +1 Block replay is meagre; 3 Block per level instead |
| MASTER_OF_STRATEGY | Cards 3 -> 4 | 0-cost: draw N; Exhaust | 5 | - | - | - | - | draw |
| MASTER_PLANNER | cost 2 -> 1 | Power: Skills you play gain Sly | 9 | - | - | - | Dexterity +1 | cost-only (0 at +2), then nothing; Dexterity per level fits a Skill-heavy Silent plan |
| MAUL | Damage 5 -> 6, Increase 2 -> 3 | hit twice; every Maul gains Increase damage when played | 5 | - | Damage | - | - | growth per play capped, base Damage keeps growing |
| MAYHEM | cost 2 -> 1 | MayhemPower 1 (hardcoded): auto-play top card of draw pile each turn | 3 | - | - | MayhemPower +1 | - | cost-only to 0; one more autoplay per turn per level past +1 (+3: 3 a turn); autoplays are bounded per turn, not a loop Cap 3 (lead review: autoplays per turn). |
| MELANCHOLY | Block 13 -> 17 | gain Block; costs 1 less whenever a creature dies | 9 | - | - | - | - | Block grows; the cost cut per death stays 1 |
| MEMENTO_MORI | CalculationBase 9 -> 11, ExtraDamage 4 -> 5 | attack: base + N per card discarded this turn | 9 | - | - | - | - | damage only, safe |
| METAMORPHOSIS | Cards 3 -> 5 | add N random Attacks, free this combat, to draw pile; Exhaust | 5 | - | - | - | - | card generation |
| METEOR_SHOWER | Damage 14 -> 21 | 0 energy, 2 Stars: AoE damage, 2 Weak and 2 Vulnerable to all | 9 | - | - | - | - | Damage replay; star cost kept |
| METEOR_STRIKE | Damage 24 -> 30 | 5-cost attack, then Channel 3 Plasma | 9 | - | - | - | - | Damage grows; Plasma count stays 3 |
| MIDNIGHT | Damage 60 -> 72 | 12-cost attack, costs 1 less per card exhausted this combat (MP) | 9 | - | - | - | - | Damage grows freely |
| MIMIC | removes Exhaust | gain Block equal to the target ally's Block (MP) | 9 | CalculationBase +3 | - | - | - | no var grows by default; flat Block on top via CalculationBase |
| MIND_BLAST | cost 1 -> 0 | Innate; damage = cards in draw pile x1 | 9 | CalculationBase +4 | - | - | - | cost-only already at 0 after +1; flat damage on top via CalculationBase (a perLevel keeps the 0 cost) |
| MINION_DIVE_BOMB | Damage 13 -> 16 | 0-cost token attack; Exhaust | 9 | - | - | - | - | safe |
| MINION_SACRIFICE | Block 7 -> 10 | 0-cost token Block; Exhaust | 9 | - | - | - | - | safe |
| MINION_STRIKE | Damage 6 -> 9 | 0-cost token attack, draw 1; Exhaust | 9 | - | - | - | - | Damage only; draw stays 1 |
| MIRAGE | removes Exhaust | gain Block equal to total Poison on enemies | 9 | CalculationBase +3 | - | - | - | no var grows by default; flat Block on top via CalculationBase |
| MISERY | Damage 7 -> 9, gains Retain | 0-cost attack; copy the target's debuffs onto all other enemies | 9 | - | - | - | - | Damage replay; Retain kept |
| MODDED | Cards 1 -> 2 | 0-cost: gain 1 orb slot, draw N; costs 1 more each play this combat | 5 | - | - | - | - | draw |
| MOLTEN_FIST | Damage 10 -> 14 | attack, double target's Vulnerable; Exhaust | 9 | - | - | - | - | safe |
| MOMENTUM_STRIKE | Damage 11 -> 15 | attack; becomes 0-cost for the combat | 9 | - | - | - | - | safe |
| MONARCHS_GAZE | cost 2 -> 1 | MonarchsGazePower: your attacks make the target lose N (StrengthLoss 1) Strength | 5 | StrengthLoss +1, cost -1 | - | - | - | the +1 never grows StrengthLoss; +1 per level (cost still drops to 0 via costPerLevel), capped as per-hit scaling |
| MONOLOGUE | gains Retain | 0-cost: this turn gain N (Power 1) temporary Strength per card played | 5 | Power +1 | - | - | - | the +1 never grows Power; +1 Strength per card per level, capped as scaling-per-play |
| MULTI_CAST | IsUpgraded: X+1 evokes | X cost: evoke your next orb X times | 9 | - | - | - | Damage +4 | X+1 never grows and extra evokes are replays; a small unpowered hit per level instead |
| MURDER | cost 3 -> 2 | damage = 1 + 1 per card drawn this combat | 9 | CalculationBase +5, cost -1 | - | - | - | keep the cost falling to 0 by +3 and add flat damage so it keeps growing after |
| NECRO_MASTERY | Summon 5 -> 8 | Summon N; Osty's HP loss is dealt to all enemies | 5 | - | - | - | - | summons |
| NEGATIVE_PULSE | Block 5 -> 6, Doom 7 -> 11 | gain Block, apply Doom to all enemies | 9 | - | - | - | - | Doom acts like damage, safe |
| NEOWS_FURY | Damage 10 -> 14, Cards 2 -> 3 | attack, return up to N cards from discard to hand; Exhaust | 5 | - | Damage | - | - | card recursion capped, Damage keeps growing |
| NEUROSURGE | Energy 3 -> 4 | 0-cost Power: gain energy, draw 2, gain 3 Doom each turn (NeurosurgePower) | 3 | - | - | - | - | energy |
| NEUTRALIZE | Damage 3 -> 4, Weak 1 -> 2 | 0-cost attack, apply Weak | 9 | - | - | - | - | safe |
| NEUTRON_AEGIS | PlatingPower 8 -> 11 | 1 energy + 5 Stars: gain Plating | 9 | - | - | - | - | Plating is Block per turn, safe |
| NIGHTMARE | cost 3 -> 2 | choose a card in hand, add 3 copies (NightmarePower, hardcoded) next turn; Exhaust | 5 | - | - | NightmarePower +1 | - | cost-only to 0 by +4; one more copy per level past +1 fits the card (hand limit caps it) Cap 5 (lead review: card copies). |
| NOSTALGIA | cost 1 -> 0 | NostalgiaPower 1: first N Attacks/Skills each turn go on top of the draw pile | 5 | - | - | NostalgiaPower +1 | - | cost-only already at 0; one more card per turn per level, bounded by energy Cap 5 (lead review: draw-like). |
| NOT_YET | Heal 10 -> 13 | heal; Exhaust | 5 | - | - | - | - | healing |
| NOXIOUS_FUMES | PoisonPerTurn 2 -> 3 | Poison all enemies each turn | 9 | - | - | - | - | Poison is safe |
| NO_ESCAPE | CalculationBase 10 -> 15 | apply Doom: base + 5 per 10 Doom already on target | 9 | - | - | - | - | Doom, safe |
| NULL | Damage 10 -> 13, Weak 1 -> 2 | attack, Weak, Channel 1 Dark | 9 | - | - | - | - | safe vars |
| OBLIVION | Doom 3 -> 4 | 0-cost: this turn, each card you play applies N Doom to the target | 9 | - | - | - | - | Doom per card is damage-like; full replay is fine |
| OFFERING | Cards 3 -> 5 | 0-cost: lose 6 HP, gain 2 energy, draw N; Exhaust | 5 | - | - | - | - | draw; energy stays 2 |
| OMNISLICE | Damage 8 -> 11 | 0-cost attack; damage dealt (with overkill) splashes to other enemies | 9 | - | - | - | - | safe |
| ONE_FOR_ALL | OneForAllPower 3 -> 4 | all players' 0-energy attacks deal +N damage (MP) | 9 | - | - | - | - | flat damage, safe |
| ONE_TWO_PUNCH | Attacks 1 -> 2 | this turn your next N Attacks are played twice | 3 | - | - | - | - | extra plays, not draw: tighter cap |
| ORBIT | cost 2 -> 1 | OrbitPower: gain N energy (1) per 4 energy spent | 9 | - | - | - | - | cost-only to 0 by +2; the power grants energy so it gets nothing more |
| OUTBREAK | Poison 9 -> 12 | apply Poison to all enemies, then trigger every enemy's Poison | 9 | - | - | - | - | Poison is safe |
| OUTMANEUVER | Energy 2 -> 3 | gain N energy next turn | 3 | - | - | - | - | energy |
| OUTRAGE | Damage 9 -> 13 | 0-cost attack; every ally gets a copy in their discard (MP) | 9 | - | - | - | - | safe |
| OVERCLOCK | Cards 2 -> 3 | 0-cost: draw N, add a Burn to discard | 5 | - | - | - | - | draw |
| PACTS_END | Damage 18 -> 24 | 0-cost AoE attack, only if 3+ cards in exhaust pile | 9 | - | - | - | - | Damage only; the exhaust threshold stays 3 |
| PAGESTORM | cost 1 -> 0 | PagestormPower: whenever you draw an Ethereal card, draw N (Cards 1) | 5 | - | - | PagestormPower +1 | - | cost-only already at 0; one more draw per Ethereal card per level, bounded by Ethereal cards in the deck Cap 5 (lead review: draw). |
| PALE_BLUE_DOT | Cards 1 -> 2 | after 5 cards in a turn, draw N next turn | 5 | - | - | - | - | draw |
| PANACHE | PanacheDamage 10 -> 14 | every 5 cards in a turn, deal N to all enemies | 9 | - | - | - | - | damage, safe |
| PANIC_BUTTON | Block 30 -> 40 | 0-cost: gain Block, no Block from cards for 2 turns; Exhaust | 9 | - | - | - | - | Block grows; the no-Block turns stay 2 |
| PARRY | ParryPower 10 -> 14 | Sovereign Blade grants N Block when played (read by the card) | 9 | - | - | - | - | Block, safe |
| PARSE | Cards 3 -> 4 | draw N; Ethereal | 5 | - | - | - | - | draw |
| PARTICLE_WALL | Block 9 -> 12 | 0 energy, 2 Stars: gain Block, return to hand | 9 | - | - | - | - | Block, safe; star cost kept |
| PATTER | Block 8 -> 10, VigorPower 2 -> 3 | gain Block and Vigor | 9 | - | - | - | - | safe vars |
| PECK | Repeat 3 -> 4 | hit 2 damage N times | 3 | - | - | - | - | hit count |
| PERFECTED_STRIKE | ExtraDamage 2 -> 3 | damage = 6 + N per Strike card you own | 9 | - | - | - | - | per-Strike damage; damage only, safe to keep growing |
| PHANTOM_BLADES | PhantomBladesPower 9 -> 12 | Shivs gain Retain; first Shiv each turn deals +N | 9 | - | - | - | - | flat damage, safe |
| PHOTON_CUT | Damage 10 -> 13, Cards 1 -> 2 | attack, draw N, put 1 card from hand on top of draw pile | 5 | - | Damage | - | - | draw capped, Damage keeps growing |
| PIERCING_WAIL | StrengthLoss 6 -> 8 | all enemies lose N Strength this turn; Exhaust | 9 | - | - | - | - | temporary Strength loss, safe |
| PILLAGE | Damage 6 -> 9 | attack, then draw until a non-Attack is drawn | 9 | - | - | - | - | Damage only |
| PILLAR_OF_CREATION | Block 2 -> 3 | gain N Block whenever you generate a card | 9 | - | - | - | - | Block per trigger, safe |
| PINPOINT | Damage 15 -> 19 | 3-cost attack, costs 1 less per Skill played this turn | 9 | - | - | - | - | safe |
| PLOT | Cards 2 -> 3 | all allies draw N next turn (MP) | 5 | - | - | - | - | draw |
| POISONED_STAB | Damage 6 -> 8, Poison 3 -> 4 | attack, apply Poison | 9 | - | - | - | - | safe vars |
| POKE | OstyDamage 6 -> 9 | 0-cost: Osty attacks | 9 | - | - | - | - | safe |
| POMMEL_STRIKE | Damage 9 -> 10, Cards 1 -> 2 | attack, draw N | 5 | - | Damage | - | - | draw capped, Damage keeps growing |
| POUNCE | Damage 14 -> 20 | attack; your next Skill is free (FreeSkillPower 1) | 9 | - | - | - | - | Damage only; the free Skill is energy, kept at 1 |
| PRECISE_CUT | CalculationBase 13 -> 16 | 0-cost attack: base - 2 per other card in hand | 9 | - | - | - | - | damage, safe |
| PREDATOR | Damage 15 -> 20 | attack, draw 2 next turn | 9 | - | - | - | - | Damage only; the 2 draw stays hardcoded |
| PREPARED | Cards 1 -> 2 | 0-cost: draw N, then discard N | 5 | - | - | - | - | draw |
| PREP_TIME | PrepTimePower 4 -> 6 | gain N Vigor each turn | 9 | - | - | - | - | Vigor, safe |
| PRIMAL_FORCE | IsUpgraded: Giant Rocks are upgraded | 0-cost: transform every Attack in hand into Giant Rock | 9 | - | - | - | Vigor +3 | no number; Vigor per level fuels the first Giant Rock without turning a repeatable 0-cost card into Strength stacking |
| PRODUCTION | Energy 2 -> 3 | 0-cost: gain energy; Exhaust | 3 | - | - | - | - | energy |
| PROLONG | removes Exhaust | 0-cost: gain your current Block again next turn | 9 | - | - | - | Block +3 | no number; a small Block per level (gained after the copy) fits |
| PROPHESIZE | Cards 6 -> 9 | draw N | 5 | - | - | - | - | draw |
| PROTECTOR | cost 1 -> 0, CalculationBase 10 -> 15 | Osty attack: base + Osty's max HP | 9 | - | - | - | - | the replay grows CalculationBase only (vars change, cost stays 0): fine |
| PROWESS | Strength 1 -> 2, Dexterity 1 -> 2 | gain Strength and Dexterity | 9 | - | - | - | - | safe vars |
| PULL_AGGRO | Summon 4 -> 5, Block 7 -> 9 | Summon N, gain Block | 5 | - | Block | - | - | summons capped, Block keeps growing |
| PULL_FROM_BELOW | Damage 5 -> 7 | hit once per Ethereal card played this combat | 9 | - | - | - | - | per-hit damage, safe |
| PURITY | Cards 3 -> 5 | 0-cost: exhaust up to N cards from hand; Retain, Exhaust | 5 | - | - | - | - | selection count, harmless; cap 5 is plenty with a 10-card hand |
| PUTREFY | Power 2 -> 3 | apply N Weak and N Vulnerable; Exhaust | 9 | - | - | - | - | debuffs, safe |
| PYRE | Energy 1 -> 2 | PyrePower: +N energy each turn | 3 | - | - | - | - | energy per turn |
| QUADCAST | cost 1 -> 0 | evoke your front orb 4 (Repeat) times | 3 | Repeat +1 | - | - | - | cost-only already at 0; one more evoke per level is a replay, so low cap (perLevel keeps the 0 cost) |
| QUASAR | IsUpgraded: the 3 offered Colorless cards are upgraded | 0 energy, 2 Stars: choose 1 of 3 Colorless cards into hand | 9 | - | - | - | Damage +5 | no number; a small radiant hit per level, star cost kept |
| RADIATE | Damage 3 -> 4 | 0-cost AoE: one hit per Star gained this turn | 9 | - | - | - | - | per-hit damage, safe |
| RAGE | Power 3 -> 5 | this turn, gain N Block whenever you play an Attack | 9 | - | - | - | - | Block per trigger, safe |
| RAINBOW | removes Exhaust | Channel Lightning, Frost and Dark | 9 | - | - | - | Block +2, Damage +2 | no number and extra orbs would be snowbally; a bit of Frost-like Block and Lightning-like damage per level |
| RALLY | Block 12 -> 17 | all players gain Block (MP) | 9 | - | - | - | - | safe |
| RAMPAGE | Increase 5 -> 10 | attack; gains Increase damage each play this combat | 5 | - | - | - | - | growth per play |
| RATTLE | OstyDamage 7 -> 9 | Osty attacks once + once per Osty attack this turn | 9 | - | - | - | - | per-hit damage, safe |
| REANIMATE | Summon 20 -> 25 | Summon 20 (Osty max HP), Exhaust | 5 | - | - | - | - | summon growth; cap 5 per summon policy |
| REAP | Damage 27 -> 33 | Deal 27 damage, Retain | 9 | - | - | - | - | plain damage, full replay |
| REAPER_FORM | adds Retain | Attacks apply Doom equal to damage x ReaperFormPower amount (1, hardcoded) | 9 | - | - | ReaperFormPower +1 | - | amount multiplies Doom per damage; +1 multiplier per level is absurd but end-of-turn Doom only |
| REAVE | Damage 10 -> 13; Soul added is upgraded (IsUpgraded) | Deal 10, shuffle Cards (1) Soul into draw pile | 5 | Damage +3, Cards +1 | Damage | - | - | one more Soul per level (card generation, cap 5); damage keeps growing |
| REBOOT | Cards 4 -> 6 | Shuffle hand into draw pile, draw Cards, Exhaust | 5 | - | - | - | - | draw; cap 5 |
| REBOUND | Damage 9 -> 12 | Deal 9, next card played goes on top of draw pile | 9 | - | - | - | - | plain damage, full replay |
| REFINE_BLADE | Forge 8 -> 12 | Forge 8, gain 1 energy next turn | 9 | - | - | - | - | Forge is Sovereign Blade damage, safe to grow; energy not touched |
| REFLECT | Block 15 -> 20 | Gain 15 Block, reflect blocked attack damage this turn | 9 | - | - | - | - | block, full replay |
| REFLEX | Cards 2 -> 3 | Sly: draw 2 | 5 | - | - | - | - | draw; cap 5 |
| REFRACT | Damage 10 -> 13 | Deal 10 twice, channel Repeat (2) Glass | 9 | - | - | - | - | damage only; Glass count not grown |
| RELAX | Block +2, Cards +1, Energy +1 | Gain Block, next turn draw Cards and gain Energy, Exhaust | 3 | - | Block | - | - | energy next turn; cap 3, block keeps growing |
| REND | CalculationBase 10 -> 12, ExtraDamage 5 -> 8 | Deal 10 + 5 per non-temporary debuff on target | 9 | - | - | - | - | damage, full replay |
| RESONANCE | StrengthPower 1 -> 2 | Gain Strength, all enemies lose 1 Strength (hardcoded) | 9 | - | - | - | - | self Strength, safe; powerBonus avoided since it would also cancel the enemy -1 |
| RESTLESSNESS | Cards 2 -> 3, Energy 2 -> 3 | If only card in hand: draw Cards, gain Energy; Retain | 3 | - | - | - | - | energy + draw; cap 3 |
| RICOCHET | Repeat 4 -> 5 | Sly: deal 3 damage Repeat times to random enemies | 9 | - | - | - | - | Repeat is hit count, not plays; extra 3-damage hits are safe, full replay |
| RIGHT_HAND_HAND | OstyDamage 4 -> 6 | Osty deals 4; returns to hand after you play a 2+ energy card | 9 | - | - | - | - | damage, full replay |
| RIP_AND_TEAR | Damage 7 -> 9 | Deal 7 twice to random enemies | 9 | - | - | - | - | plain damage |
| ROCKET_PUNCH | Damage +1, Cards +1 | Deal 13, draw Cards; cost -1 when you create a Status | 5 | - | Damage | - | - | draw; cap 5, damage keeps growing |
| ROLLING_BOULDER | RollingBoulderPower 5 -> 10 | Power: rolling boulder damage that grows by 5 each turn | 9 | - | - | - | - | damage power, safe |
| ROYALTIES | Gold 30 -> 40 | Power: gain Gold after combat | 3 | - | - | - | - | gold; cap 3 |
| ROYAL_GAMBLE | adds Retain | Costs 5 stars: gain 9 stars, Exhaust | 3 | Stars +1 | - | - | - | +1 star net per level; stars capped at 3 |
| RUPTURE | Strength 1 -> 2 | Power: gain Strength when you lose HP from a card | 9 | - | - | - | - | Strength scaling, safe |
| SACRIFICE | cost 1 -> 0 | Kill Osty, gain Block = 3x Osty max HP; Retain | 9 | CalculationBase +5 | - | - | - | cost already 0 at +1; flat +5 Block per level on top of the sacrifice |
| SALVO | Damage 12 -> 16 | Deal 12, retain your hand this turn | 9 | - | - | - | - | plain damage |
| SCAVENGE | Energy 2 -> 3 | Exhaust a card, gain Energy next turn | 3 | - | - | - | - | energy; cap 3 |
| SCOURGE | Doom 13 -> 16, Cards +1 | Apply Doom, draw Cards | 5 | - | Doom | - | - | draw; cap 5, Doom keeps growing |
| SCRAPE | Damage +3, Cards +1 | Deal 7, draw Cards, discard drawn non-0-cost cards | 5 | - | Damage | - | - | draw; cap 5, damage keeps growing |
| SCRAWL | adds Retain | Draw until hand is full, Exhaust (cost 1) | 3 | cost -1 | - | - | - | no number; cost reaches 0 at +2, a free full-hand refill once per combat |
| SCULPTING_STRIKE | Damage 9 -> 12 | Deal 9, make a card in hand Ethereal | 9 | - | - | - | - | plain damage |
| SEANCE | cost 1 -> 0 | Transform Cards (1) from draw pile into Soul, Ethereal | 5 | Cards +1 | - | - | - | cost already 0; one more card turned into a Soul per level (card generation, cap 5) |
| SECOND_WIND | Block 5 -> 7 | Exhaust non-Attacks in hand, gain Block per card | 9 | - | - | - | - | block, full replay |
| SECRET_TECHNIQUE | removes Exhaust | Put a Skill from draw pile into hand (cost 0) | 9 | - | - | - | Block +3 | tutor with no number; small Block per level for the Skill it fetches |
| SECRET_WEAPON | removes Exhaust | Put an Attack from draw pile into hand (cost 0) | 9 | - | - | - | Vigor +2 | tutor with no number; Vigor primes the fetched Attack |
| SEEKER_STRIKE | Damage 9 -> 12 | Deal 9, choose 1 of Cards (3) random draw pile cards into hand | 9 | - | - | - | - | plain damage |
| SEEKING_EDGE | Forge 7 -> 11 | Power: Sovereign Blade hits all enemies; Forge 7 | 9 | - | - | - | - | Forge is damage, safe |
| SENTRY_MODE | cost 2 -> 1 | Power: add SentryModePower (1) Sweeping Gaze to hand each turn | 5 | SentryModePower +1, cost -1 | - | - | - | cost reaches 0 at +2; one more Sweeping Gaze per turn per level (card generation, cap 5) |
| SERPENT_FORM | SerpentFormPower 4 -> 6 | Power: deal damage whenever you play a card | 9 | - | - | - | - | damage power, safe |
| SETUP_STRIKE | Damage +2, Strength 3 -> 4 | Deal 7, gain temporary Strength (SetupStrikePower) | 9 | - | - | - | - | damage + temporary Strength, safe |
| SEVEN_STARS | cost 2 -> 1 | Costs 7 stars: deal 7 damage 7 times to all enemies | 9 | Damage +1, cost -1 | - | - | - | cost reaches 0 at +2; +1 damage per hit per level (x7 hits) |
| SEVERANCE | Damage 13 -> 18 | Deal 13, add a Soul to draw, discard and hand | 9 | - | - | - | - | plain damage |
| SHADOWMELD | cost 1 -> 0 | This turn Block gains are doubled (2^Power, Power 1) | 3 | Power +1 | - | - | - | exponential multiplier: x4 at +2, x8 at +3, then stops |
| SHADOW_SHIELD | Block 11 -> 15 | Gain 11 Block, channel Dark | 9 | - | - | - | - | block, full replay |
| SHADOW_STEP | cost 1 -> 0 | Discard hand; next turn gain DoubleDamagePower (turns = ShadowStepPower, 1 hardcoded) | 3 | - | - | ShadowStepPower +1 | - | power amount becomes turns of Double Damage; +1 turn per level, cap 3 |
| SHARED_FATE | EnemyStrengthLoss 2 -> 3 | You lose 2 Strength, enemy loses EnemyStrengthLoss Strength, Exhaust | 9 | - | - | - | - | enemy Strength loss, safe |
| SHATTER | Damage 7 -> 11 | Deal 7 to all, evoke every orb twice, Exhaust | 9 | - | - | - | - | plain damage |
| SHINING_STRIKE | Damage 8 -> 11 | Deal 8, gain 2 stars, goes on top of draw pile | 9 | - | - | - | - | damage only; stars not grown |
| SHIV | Damage 4 -> 6 | Deal 4, Exhaust (cost 0 token) | 9 | - | - | - | - | plain damage |
| SHOCKWAVE | Power 3 -> 5 | Apply Power Weak and Vulnerable to all enemies, Exhaust | 9 | - | - | - | - | debuff durations, safe |
| SHROUD | Block 3 -> 4 | Power: gain Block whenever you apply Doom | 9 | - | - | - | - | block power, safe |
| SHRUG_IT_OFF | Block 8 -> 11 | Gain 8 Block, draw 1 | 9 | - | - | - | - | block only; draw not grown |
| SIC_EM | OstyDamage +1, SicEmPower 3 -> 4 | Osty deals 5; Osty hits on target summon SicEmPower this turn | 9 | - | - | - | - | small damage and summon growth, safe |
| SIDESTEP | Energy 1 -> 2 | Gain Energy next turn (cost 0) | 3 | - | - | - | - | energy; cap 3 |
| SIGNAL_BOOST | cost 1 -> 0 | Next SignalBoostPower (1) Power cards are played twice, Exhaust | 3 | SignalBoostPower +1 | - | - | - | extra plays; one more doubled Power per level, cap 3 |
| SKEWER | Damage 8 -> 11 | X cost: deal 8 X times | 9 | - | - | - | - | plain damage |
| SKIM | Cards 3 -> 4 | Draw Cards | 5 | - | - | - | - | draw; cap 5 |
| SLEIGHT_OF_FLESH | SleightOfFleshPower 9 -> 13 | Power: deal damage whenever you apply a debuff to an enemy | 9 | - | - | - | - | damage power, safe |
| SLICE | Damage 6 -> 9 | Deal 6 (cost 0) | 9 | - | - | - | - | plain damage |
| SMOKESTACK | SmokestackPower 5 -> 7 | Power: deal damage to all enemies whenever you create a Status | 9 | - | - | - | - | damage power, safe |
| SNAKEBITE | Poison 7 -> 10 | Apply Poison, Retain | 9 | - | - | - | - | poison, safe |
| SNAP | OstyDamage 7 -> 10 | Osty deals 7, give a card in hand Retain | 9 | - | - | - | - | damage, full replay |
| SNEAKY | SneakyPower 1 -> 2 | Power: gain Block whenever an ally plays an Attack | 9 | - | - | - | - | block power, safe |
| SOLAR_STRIKE | Damage +1, Stars +1 | Deal 9, gain Stars | 3 | - | Damage | - | - | stars; cap 3, damage keeps growing |
| SOUL | Cards 2 -> 3 | Draw Cards, Exhaust (cost 0 token) | 5 | - | - | - | - | draw; cap 5 |
| SOULBOUND | adds Innate | Ally power: whenever you create a Soul, ally gets SoulboundPower (1) Souls | 5 | - | - | SoulboundPower +1 | - | one more Soul for the ally per level (card generation, cap 5); no loop thanks to IsAddingSoul |
| SOUL_STORM | ExtraDamage 4 -> 6 | Deal 9 + ExtraDamage per exhausted Soul | 9 | - | - | - | - | damage scaling, safe |
| SOVEREIGN_BLADE | cost 2 -> 1 | Forged token: deal Damage Repeat times, Retain | 9 | cost -1 | - | - | Damage +5 | cost reaches 0 at +2; Damage/Repeat are Forge-managed state, so growth goes through an unpowered play bonus |
| SOW | Damage 8 -> 11 | Deal 8 to all, Retain | 9 | - | - | - | - | plain damage |
| SPECTRUM_SHIFT | cost 2 -> 1 | Power: add Cards (1) random Colorless cards to hand each turn | 5 | Cards +1, cost -1 | - | - | - | cost reaches 0 at +2; one more Colorless card per turn per level (card generation, cap 5) |
| SPEEDSTER | adds Innate | Power: deal SpeedsterPower (2) to all enemies when you draw outside hand draw | 9 | SpeedsterPower +1 | - | - | - | damage power with no +1 delta; +1 damage per level |
| SPINNER | channels a Glass orb on play (IsUpgraded) | Power: channel SpinnerPower (1) Glass each turn | 5 | SpinnerPower +1 | - | - | - | one more Glass per turn per level (orbs, cap 5) |
| SPIRIT_OF_ASH | BlockOnExhaust 4 -> 5 | Power: gain Block whenever you exhaust a card | 9 | - | - | - | - | block power, safe |
| SPITE | Repeat 2 -> 3 | Deal 5 (Repeat times if you lost HP this turn), cost 0 | 9 | - | - | - | - | Repeat is hit count, not plays; extra 5-damage hits are safe, full replay |
| SPLASH | generated Attack is upgraded (IsUpgraded) | Choose 1 of 3 other-character Attacks, free this turn | 9 | - | - | - | Vigor +2 | no number; Vigor powers the borrowed Attack |
| SPOILS_MAP | none (MaxUpgradeLevel 0) | Unplayable quest: 600 gold at the act's treasure | no fusion | - | - | - | - | quest card that can never upgrade; fusing it would only destroy a quest |
| SPOILS_OF_BATTLE | Forge 6 -> 9 | Forge 6, draw 2 | 9 | - | - | - | - | Forge is damage, safe; draw not grown |
| SPUR | Summon +2, Heal +2 | Summon, heal Osty, Retain | 5 | - | - | - | - | summon + healing; cap 5 |
| SQUASH | Damage +2, Vulnerable 2 -> 3 | Deal 10, apply Vulnerable | 9 | - | - | - | - | damage + debuff duration, safe |
| SQUEEZE | CalculationBase 25 -> 30, ExtraDamage 5 -> 6 | Osty deals 25 + ExtraDamage per other Osty attack card | 9 | - | - | - | - | damage, full replay |
| STACK | CalculationBase 0 -> 3 | Gain Block equal to discard pile size (+ base) | 9 | - | - | - | - | block, full replay |
| STAMPEDE | cost 2 -> 1 | Power: at end of turn auto-play Power (1) random Attacks from hand | 3 | Power +1, cost -1 | - | - | - | cost reaches 0 at +2; one more autoplay per level (extra plays, cap 3) |
| STARDUST | Damage 5 -> 7 | Star X: deal 5 X times to random enemies | 9 | - | - | - | - | plain damage |
| STOKE | generated cards are upgraded (IsUpgraded) | Exhaust hand, add as many random class cards | 3 | - | - | - | Energy +1 | no number; 1 energy per level to play the new hand, cap 3 (2 energy at +3) |
| STOMP | Damage 12 -> 15 | Deal 12 to all; costs 1 less per Attack played this turn | 9 | - | - | - | - | plain damage |
| STONE_ARMOR | PlatingPower 4 -> 6 | Power: gain Plating | 9 | - | - | - | - | block-like, safe |
| STORM | StormPower 1 -> 2 | Power: channel Lightning per Power played | 5 | - | - | - | - | orbs per trigger; cap 5 |
| STORM_OF_STEEL | Shivs are upgraded (IsUpgraded) | Discard hand, add a Shiv per card discarded | 9 | - | - | - | Strength +1 | no number; Strength boosts every Shiv hit |
| STRANGLE | Damage +2, StranglePower 2 -> 3 | Deal 8, target takes StranglePower per card played this turn | 9 | - | - | - | - | damage, safe |
| STRATAGEM | cost 1 -> 0 | Power: on shuffle, pick StratagemPower (1, hardcoded) cards from draw pile into hand | 5 | - | - | StratagemPower +1 | - | one more pick per shuffle per level (draw-like, cap 5) |
| STRIKE_DEFECT | Damage 6 -> 9 | Deal 6 | 9 | - | - | - | - | plain damage |
| STRIKE_IRONCLAD | Damage 6 -> 9 | Deal 6 | 9 | - | - | - | - | plain damage |
| STRIKE_NECROBINDER | Damage 6 -> 9 | Deal 6 | 9 | - | - | - | - | plain damage |
| STRIKE_REGENT | Damage 6 -> 9 | Deal 6 | 9 | - | - | - | - | plain damage |
| STRIKE_SILENT | Damage 6 -> 9 | Deal 6 | 9 | - | - | - | - | plain damage |
| STS1_AGGREGATE | CardsPerEnergy 4 -> 3 | Gain 1 energy per CardsPerEnergy cards in draw pile | 2 | - | - | - | - | divisor: -1 per level hits 0 (division by zero) at +4; stop at 2 cards per energy |
| STS1_ALCHEMIZE | cost 1 -> 0 | Obtain a random potion, Exhaust | 9 | - | - | - | Poison +2 | cost already 0, no var; a whiff of brewed poison per level |
| STS1_ALL_OUT_ATTACK | Damage 10 -> 14 | Deal 10 to all, discard a random card | 9 | - | - | - | - | plain damage |
| STS1_ALPHA | adds Innate | Shuffle Beta into draw pile, Exhaust | 3 | - | - | - | Draw +1 | no number; a draw per level helps reach Beta, cap 3 |
| STS1_AMPLIFY | Powers 1 -> 2 | This turn the next Powers Power cards are played twice | 3 | - | - | - | - | extra plays; cap 3 |
| STS1_APOTHEOSIS | cost 2 -> 1 | Upgrade every card in combat, Exhaust | 9 | cost -1 | - | - | Strength +1, Dexterity +1 | cost 0 at +2; ascension theme: Strength and Dexterity per level, once per combat |
| STS1_APPARITION | removes Ethereal | Gain IntangiblePower (1), Exhaust | 3 | IntangiblePower +1 | - | - | - | Intangible turns; cap 3 |
| STS1_AUTO_SHIELDS | Block 11 -> 15 | Gain Block if you have none | 9 | - | - | - | - | block, full replay |
| STS1_A_THOUSAND_CUTS | Sts1AThousandCutsPower 1 -> 2 | Power: deal damage to all enemies per card played | 9 | Sts1AThousandCutsPower +1 | - | - | - | damage power, safe; explicit perLevel because the var key is a nameof constant |
| STS1_BANDAGE_UP | Heal 4 -> 6 | Heal 4, Exhaust (cost 0) | 5 | - | - | - | - | healing; cap 5 |
| STS1_BANE | Damage 7 -> 10 | Deal 7 (twice if target is poisoned) | 9 | - | - | - | - | plain damage |
| STS1_BARRAGE | Damage 4 -> 6 | Deal 4 once per orb | 9 | - | - | - | - | plain damage |
| STS1_BATTLE_HYMN | adds Innate | Power: add Magic (1) Smite to hand each turn | 5 | Magic +1 | - | - | - | one more Smite per turn per level (card generation, cap 5) |
| STS1_BECOME_ALMIGHTY | Magic 3 -> 4 | Unplayable choice token: gain Magic Strength | 9 | - | - | - | - | Strength, safe |
| STS1_BERSERK | VulnerablePower 2 -> 1 | Power: gain 1 Vulnerable, gain Energy (1) each turn | 2 | VulnerablePower -1 | - | - | - | drawback reaches 0 at +2 (0 amount is a no-op); never negative, energy per turn not grown |
| STS1_BETA | cost 2 -> 1 | Shuffle Omega into draw pile, Exhaust | 3 | cost -1 | - | - | Draw +1 | cost 0 at +2; a draw per level helps reach Omega, cap 3 |
| STS1_BITE | Damage +1, Magic (heal) 2 -> 3 | Deal 7, heal Magic | 5 | - | Damage | - | - | Magic is healing; cap 5, damage keeps growing |
| STS1_BLADE_DANCE | Cards 3 -> 4 | Add Cards Shivs to hand | 5 | - | - | - | - | card generation; cap 5 |
| STS1_BLASPHEMY | adds Retain | Enter Divinity, die at end of turn, Exhaust | 9 | - | - | - | Vigor +3 | no number; Vigor is tripled by Divinity, fits the all-in turn |
| STS1_BLIND | targets all enemies (IsUpgraded) | Apply WeakPower (2) Weak | 9 | WeakPower +1 | - | - | - | +1 Weak duration per level, safe |
| STS1_BLIZZARD | ExtraDamage 2 -> 3 | Deal ExtraDamage per Frost channeled this combat to all | 9 | - | - | - | - | damage scaling, safe |
| STS1_BLOOD_FOR_BLOOD | Damage 18 -> 22, cost 4 -> 3 | Deal 18; costs 1 less per HP loss this combat | 9 | - | - | - | - | damage replay; cost already falls with HP loss |
| STS1_BOWLING_BASH | Damage 7 -> 10 | Deal 7 once per enemy | 9 | - | - | - | - | plain damage |
| STS1_BRILLIANCE | CalculationBase 12 -> 16 | Deal 12 + Mantra gained this combat | 9 | - | - | - | - | damage, full replay |
| STS1_BRUTALITY | adds Innate | Power: each turn draw Sts1BrutalityPower (1, hardcoded) and lose that much HP | 5 | - | - | Sts1BrutalityPower +1 | - | draw + HP loss per turn; +1 per level, cap 5 |
| STS1_CALCULATED_GAMBLE | removes Exhaust | Discard hand, draw that many (cost 0) | 3 | - | - | - | Draw +1 | no number; one extra card per level, cap 3 |
| STS1_CALTROPS | ThornsPower 3 -> 5 | Power: gain Thorns | 9 | - | - | - | - | Thorns damage, safe |
| STS1_CARNAGE | Damage 20 -> 28 | Ethereal: deal 20 | 9 | - | - | - | - | plain damage |
| STS1_CARVE_REALITY | Damage 6 -> 10 | Deal 6, add a Smite to hand | 9 | - | - | - | - | plain damage |
| STS1_CATALYST | triples instead of doubles Poison (IsUpgraded) | Add the target's Poison again (x2 or x3 total), Exhaust | 9 | - | - | PoisonPower +3 | - | no var; +3 Poison per level on top of the multiplied amount |
| STS1_CHILL | adds Innate | Channel a Frost per enemy, Exhaust (cost 0) | 9 | - | - | - | Block +3 | no number; Frost theme is Block |
| STS1_CHOKE | StranglePower 3 -> 5 | Deal 12, target takes StranglePower per card played this turn | 9 | - | - | - | - | damage, safe |
| STS1_CHRYSALIS | Cards 3 -> 5 | Shuffle Cards random Skills (free this combat) into draw pile, Exhaust | 5 | - | - | - | - | card generation; cap 5 |
| STS1_CLASH | Damage 14 -> 18 | Deal 14, only playable if hand is all Attacks (cost 0) | 9 | - | - | - | - | plain damage |
| STS1_CLAW | Damage 3 -> 5 | Deal 3, all Claws gain 2 damage | 9 | - | - | - | - | damage, full replay; Increase stays 2 |
| STS1_CLEAVE | Damage 8 -> 11 | Deal 8 to all | 9 | - | - | - | - | plain damage |
| STS1_CLOTHESLINE | Damage +2, Weak 2 -> 3 | Deal 12, apply Weak | 9 | - | - | - | - | damage + debuff, safe |
| STS1_COLLECT | +1 turn (IsUpgraded) | X cost: put a Miracle+ in hand for X turns, Exhaust | 3 | - | - | Sts1CollectPower +1 | - | one more Miracle+ turn per level; Miracle is energy, cap 3 |
| STS1_COMBUST | Sts1CombustPower 5 -> 7 | Power: end of turn lose 1 HP, deal damage to all | 9 | - | - | - | - | damage power, safe |
| STS1_CONCENTRATE | Cards (discard count) 3 -> 2 | discard 3 cards, gain 2 energy (0 cost) | 3 | - | - | - | - | the +1 lowers the discard; +3 reaches discard 0, past that it would go negative |
| STS1_CONCLUDE | Damage +4 | AoE damage, end your turn | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_CONJURE_BLADE | IsUpgraded: Expunger gets X+1 hits instead of X | X cost, exhaust: shuffle an Expunger hitting X(+1) times into the draw pile | 9 | - | - | - | Strength +1 | no var; Strength multiplies across every Expunger hit, once per combat (exhaust) |
| STS1_CONSECRATE | Damage +3 | 0-cost AoE damage | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_CONSERVE_BATTERY | Block +3 | block + 1 energy next turn (constant) | 9 | - | - | - | - | block grows, the energy stays 1 |
| STS1_CONSUME | FocusPower +1 | gain 2 Focus, lose an orb slot | 5 | - | - | - | - | Focus: cap 5 |
| STS1_CORE_SURGE | Damage +4 | damage + 1 Artifact (constant), exhaust | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_CORPSE_EXPLOSION | Poison +3 | poison + Corpse Explosion (damage to all on death) | 9 | - | - | - | - | poison is safe to grow |
| STS1_CRESCENDO | cost 1 -> 0 | enter Wrath; retain, exhaust | 9 | - | - | - | Vigor +3 | cost hits 0 at +1; Vigor feeds the next attack, doubled by the Wrath it enters |
| STS1_CRIPPLING_POISON | Poison +3 | poison + 2 Weak to all enemies, exhaust | 9 | - | - | - | - | poison is safe to grow; Weak stays 2 |
| STS1_CRUSH_JOINTS | Damage +2, Magic (Vulnerable) +1 | damage; Vulnerable if last card was a skill | 9 | - | - | - | - | damage and Vulnerable turns are safe |
| STS1_CUT_THROUGH_FATE | Damage +2, Magic (Scry) +1 | damage, Scry, draw 1 (constant) | 9 | - | - | - | - | damage and Scry are safe; draw stays 1 |
| STS1_DARKNESS | IsUpgraded: also trigger every Dark orb's passive | channel 1 Dark orb | 9 | - | - | - | Damage +3 | no var; an unpowered dark hit per level fits the orb's evoke |
| STS1_DECEIVE_REALITY | Block +3 | block + a Safety in hand | 9 | - | - | - | - | replay of the +1 block delta is the right growth |
| STS1_DEEP_BREATH | Cards +1 | shuffle discard into draw, draw | 5 | - | - | - | - | draw: cap 5 |
| STS1_DEFEND_WATCHER | Block +3 | block | 9 | - | - | - | - | replay of the +1 block delta is the right growth |
| STS1_DEUS_EX_MACHINA | Magic (Miracles) +1 | unplayable; on draw add Magic Miracles (1 energy each) to hand, exhaust | 5 | - | - | - | - | Miracles are energy; once per combat, so cap 5 (7 Miracles) and nothing beyond |
| STS1_DEVA_FORM | removes Ethereal | power: gain energy each turn, the gain rises by Magic each turn | 3 | Magic +1 | - | - | - | no number at +1; Magic is the energy ramp per turn, energy cap 3 |
| STS1_DEVOTION | Magic (Mantra/turn) +1 | power: gain Mantra each turn | 5 | - | - | - | - | Mantra feeds Divinity (energy + triple damage); cap 5, nothing beyond |
| STS1_DIE_DIE_DIE | Damage +4 | AoE damage, exhaust | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_DISARM | StrengthLoss +1 | enemy loses Strength, exhaust | 9 | - | - | - | - | enemy Strength loss is safe to grow |
| STS1_DISCOVERY | removes Exhaust | choose 1 of 3 class cards, free this turn (retired) | 9 | - | - | - | Block +3 | no number; non-exhaust repeatable card, so a safe Block bonus instead of draw/energy |
| STS1_DISTRACTION | cost 1 -> 0 | exhaust: random skill to hand, free this turn | 9 | - | - | - | Weak +1 | cost hits 0 at +1; distracted enemies get Weak |
| STS1_DOOM_AND_GLOOM | Damage +4 | AoE damage, channel Dark | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_DOPPELGANGER | IsUpgraded: X+1 instead of X | X cost, exhaust: next turn draw X and gain X energy | 3 | - | - | EnergyNextTurnPower +1, DrawCardsNextTurnPower +1 | - | no var; +1 draw and energy next turn per level, once per combat (exhaust) |
| STS1_DOUBLE_TAP | Attacks +1 | next N attacks are played twice | 3 | - | - | - | - | extra plays: cap 3 |
| STS1_DRAMATIC_ENTRANCE | Damage +4 | innate AoE damage, exhaust | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_DROPKICK | Damage +3 | damage; vs Vulnerable gain 1 energy and draw 1 (constant) | 9 | - | - | - | - | damage grows, energy/draw stay 1 |
| STS1_DUAL_WIELD | Cards (copies) +1 | copy an attack/power in hand N times | 5 | - | - | - | - | card generation: cap 5 |
| STS1_ELECTRODYNAMICS | Orbs +1 | Lightning hits all; channel N Lightning | 5 | - | - | - | - | orbs: cap 5 |
| STS1_EMPTY_BODY | Block +3 | block, exit stance | 9 | - | - | - | - | replay of the +1 block delta is the right growth |
| STS1_EMPTY_FIST | Damage +5 | damage, exit stance | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_EMPTY_MIND | Magic (draw) +1 | draw, exit stance | 5 | - | - | - | - | Magic is draw: cap 5 |
| STS1_ENDLESS_AGONY | Damage +2 | 0-cost damage, copies itself when drawn, exhaust | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_ENLIGHTENMENT | IsUpgraded: hand costs 1 for the combat instead of this turn | 0 cost: reduce hand costs to 1 | 9 | - | - | - | Block +3 | no number; 0-cost repeatable, so a safe Block bonus, no draw/energy |
| STS1_ENTRENCH | cost 2 -> 1 | double your Block | 9 | - | - | - | Block +3 | cost-only reaches 0 at +2; extra Block per level feeds the doubling |
| STS1_ENVENOM | cost 2 -> 1 | power: unblocked attack damage applies Poison | 9 | - | - | EnvenomPower +1 | - | +1 Poison per hit per level past +1 (cost still drops to 0 at +2) |
| STS1_EQUILIBRIUM | Block +3 | block, retain hand this turn | 9 | - | - | - | - | replay of the +1 block delta is the right growth |
| STS1_ERUPTION | cost 2 -> 1 | damage, enter Wrath | 9 | Damage +3, cost -1 | - | - | - | cost-only would stop at 0 (+2); grow damage and keep lowering the cost |
| STS1_ESTABLISHMENT | adds Innate | power: retained cards cost Magic less | 3 | Magic +1 | - | - | - | no number at +1; Magic is cost reduction per retain (energy-like), cap 3 |
| STS1_EVALUATE | Block +4 | block, Insight into draw pile | 9 | - | - | - | - | replay of the +1 block delta is the right growth |
| STS1_EVISCERATE | Damage +2 | 3 hits, costs 1 less per discard this turn | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_EVOLVE | Sts1EvolvePower +1 | power: draw when you draw a Status | 5 | - | - | - | - | draw: cap 5 |
| STS1_EXHUME | cost 1 -> 0 | exhaust: return an exhausted card to hand | 5 | - | - | - | Draw +1 | cost hits 0 at +1; exhaust, so a once-per-combat draw is safe Cap 5 (lead review: draw). |
| STS1_EXPUNGER | Damage +6 | token: damage X times | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_FAME_AND_FORTUNE | Magic (gold) +5 | Wish choice: gain gold | 3 | - | - | - | - | gold: cap 3 |
| STS1_FASTING | Magic +1 | power: gain Strength and Dexterity, 1 less energy per turn | 5 | - | Magic | - | - | Strength/Dexterity are safe; kept as decided |
| STS1_FEAR_NO_EVIL | Damage +3 | damage; Calm if the enemy intends to attack | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_FINESSE | Block +2 | 0 cost: block, draw 1 (constant) | 9 | - | - | - | - | block grows, draw stays 1 |
| STS1_FIRE_BREATHING | Sts1FireBreathingPower +4 | power: AoE damage when drawing Status/Curse | 9 | - | - | - | - | damage is safe to grow |
| STS1_FISSION | IsUpgraded: evoke orbs instead of removing them | exhaust: remove all orbs, gain that much energy and draw | 3 | - | - | - | Energy +1 | no number; energy fits, once per combat (exhaust) |
| STS1_FLASH_OF_STEEL | Damage +3 | 0 cost: damage, draw 1 (constant) | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_FLECHETTES | Damage +2 | damage once per skill in hand | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_FLEX | StrengthPower +2 | temporary Strength this turn | 9 | - | - | - | - | temporary Strength is safe |
| STS1_FLURRY_OF_BLOWS | Damage +2 | 0 cost damage, returns on stance change | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_FLYING_KNEE | Damage +3 | damage, 1 energy next turn (constant) | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_FLYING_SLEEVES | Damage +2 | retain, 2 hits | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_FOLLOW_UP | Damage +4 | damage; 1 energy if last card was an attack (constant) | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_FORCE_FIELD | Block +4 | block, costs 1 less per power played | 9 | - | - | - | - | replay of the +1 block delta is the right growth |
| STS1_FOREIGN_INFLUENCE | IsUpgraded: chosen attack is free this turn | 0 cost exhaust: choose 1 of 3 attacks from any class | 9 | - | - | - | Vigor +3 | no var; Vigor powers the attack it hands you |
| STS1_FORESIGHT | Magic (Scry) +1 | power: Scry at turn start | 9 | - | - | - | - | Scry is not draw; safe to grow |
| STS1_FORETHOUGHT | IsUpgraded: any number of cards instead of 1 | 0 cost: put hand cards at the bottom, free until played | 9 | - | - | - | Block +3 | no number; 0-cost repeatable cost cheat, so Block rather than draw/energy |
| STS1_FUSION | cost 2 -> 1 | channel Plasma | 9 | - | - | - | - | cost-only reaches 0 at +2; Plasma is energy, no more growth wanted |
| STS1_GENETIC_ALGORITHM | Increase +1 | exhaust: block, permanently grows its block | 9 | Increase +1 | - | - | - | permanent block growth is safe; perLevel because the scan missed the IntVar delta |
| STS1_GHOSTLY_ARMOR | Block +3 | ethereal block | 9 | - | - | - | - | replay of the +1 block delta is the right growth |
| STS1_GLASS_KNIFE | Damage +4 | 2 hits, loses 2 damage per play | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_GOOD_INSTINCTS | Block +3 | 0 cost block | 9 | - | - | - | - | replay of the +1 block delta is the right growth |
| STS1_GRAND_FINALE | Damage +10 | AoE damage, only with an empty draw pile | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_HALT | Block +1, Magic (Wrath block) +5 | 0 cost block, more in Wrath | 9 | - | - | - | - | block values are safe |
| STS1_HEATSINKS | Cards +1 | power: draw when you play a power | 5 | - | - | - | - | draw: cap 5 |
| STS1_HEAVY_BLADE | Multiplier +2 | damage, Strength counts Multiplier times | 5 | - | - | - | - | Strength multiplier: cap 5 |
| STS1_HEEL_HOOK | Damage +3 | damage; vs Weak gain 1 energy, draw 1 (constant) | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_HELLO_WORLD | adds Innate | power: add a random Common card to hand each turn | 5 | - | - | HelloWorldPower +1 | - | no number; one more card per turn per level Cap 5 (lead review: card generation). |
| STS1_HYPERBEAM | Damage +8 | AoE damage, lose 3 Focus (constant) | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_IMMOLATE | Damage +7 | AoE damage, Burn into discard | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_INDIGNATION | Magic (Vulnerable) +2 | Wrath, or Vulnerable to all if already in Wrath | 9 | - | - | - | - | Vulnerable turns are safe |
| STS1_INNER_PEACE | Magic (draw) +1 | Calm, or draw if already in Calm | 5 | - | - | - | - | Magic is draw: cap 5 |
| STS1_INSIGHT | Cards +1 | token: retain, draw, exhaust | 5 | - | - | - | - | draw: cap 5 |
| STS1_INTIMIDATE | WeakPower +1 | 0 cost exhaust: Weak to all | 9 | - | - | - | - | Weak is safe |
| STS1_JACK_OF_ALL_TRADES | Cards +1 | exhaust: add random colorless cards to hand | 5 | - | - | - | - | card generation: cap 5 |
| STS1_JAX | Strength +1 | lose 3 HP, gain Strength | 9 | - | - | - | - | Strength is safe; HP loss stays 3 |
| STS1_JUDGEMENT | Magic (threshold) +10 | kill an enemy at or under Magic HP | 9 | - | - | - | - | threshold growth is fun and bounded by its 1 cost |
| STS1_JUGGERNAUT | JuggernautPower +2 | power: gaining block damages a random enemy | 9 | - | - | - | - | damage is safe |
| STS1_JUST_LUCKY | Damage +1, Block +1, Magic (Scry) +1 | 0 cost: scry, block, damage | 9 | - | - | - | - | all safe values |
| STS1_LESSON_LEARNED | Damage +3 | damage; on a kill upgrade a deck card, exhaust | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_LIKE_WATER | Magic +2 | power: block at end of turn in Calm | 9 | - | - | - | - | block is safe |
| STS1_LIMIT_BREAK | removes Exhaust | double your Strength | 5 | StrengthPower +1 | StrengthPower | - | - | gains 1 Strength per level past +1 before doubling; kept as decided |
| STS1_LIVE_FOREVER | Magic (Plated Armor) +2 | Wish choice: gain Plated Armor | 9 | - | - | - | - | Plated Armor is block, safe |
| STS1_LOCK_ON | Damage +3, Sts1LockOnPower +1 | damage, Lock-On for N turns | 9 | - | - | - | - | damage and Lock-On turns are safe |
| STS1_MADNESS | cost 1 -> 0 | exhaust: a random hand card costs 0 for the combat | 5 | - | - | - | Draw +1 | cost hits 0 at +1; exhaust, so a once-per-combat draw finds more to make free Cap 5 (lead review: draw). |
| STS1_MAGNETISM | cost 2 -> 1 | power: a random colorless card to hand each turn | 9 | - | - | Sts1MagnetismPower +1 | - | one more colorless card per turn per level (cost still drops to 0 at +2) |
| STS1_MASTERFUL_STAB | Damage +4 | 0 cost damage, +1 cost per HP loss | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_MASTER_REALITY | cost 1 -> 0 | power: created cards are upgraded (single stack) | 9 | - | - | - | Dexterity +1 | cost hits 0 at +1 and the power cannot stack; a small one-time stat per level |
| STS1_MEDITATE | Magic (cards returned) +1 | return cards from discard with Retain, Calm, end turn | 5 | - | - | - | - | card recursion like draw: cap 5 |
| STS1_MELTER | Damage +4 | remove enemy block, damage | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_MENTAL_FORTRESS | Magic +2 | power: block on stance change | 9 | - | - | - | - | block is safe |
| STS1_METALLICIZE | Sts1MetallicizePower +1 | power: block at end of turn | 9 | - | - | - | - | block is safe |
| STS1_METAMORPHOSIS | Cards +2 | exhaust: shuffle N random attacks, free for the combat | 5 | - | - | - | - | card generation: cap 5 |
| STS1_MIND_BLAST | cost 2 -> 1 | innate: damage equal to draw pile size | 5 | ExtraDamage +1, cost -1 | - | - | - | damage per draw-pile card grows (multiplier, cap 5) and the cost keeps dropping |
| STS1_MIRACLE | Energy +1 | token: retain, gain energy, exhaust | 3 | - | - | - | - | energy: cap 3 |
| STS1_NIRVANA | Magic +1 | power: block whenever you Scry | 9 | - | - | - | - | block is safe |
| STS1_OMEGA | Magic +10 | token power: AoE damage at end of turn | 9 | - | - | - | - | damage is safe |
| STS1_OMNISCIENCE | cost 4 -> 3 | exhaust: play a card from the draw pile twice | 9 | - | - | - | - | cost-only reaches 0 at +4; play count stays 2 (extra plays not grown) |
| STS1_OUTMANEUVER | Energy +1 | energy next turn | 3 | - | - | - | - | energy: cap 3 |
| STS1_PANACEA | ArtifactPower +1 | 0 cost exhaust: gain Artifact | 9 | - | - | - | - | Artifact is safe to grow on an exhaust card |
| STS1_PANACHE | Sts1PanachePower +4 | 0 cost power: every 5th card a turn deals AoE damage | 9 | Sts1PanachePower +4 | - | - | - | damage is safe; perLevel because the scan missed the const-keyed var |
| STS1_PERSEVERANCE | Block +2, Magic +1 | retain: block, grows when retained | 9 | - | - | - | - | block values are safe |
| STS1_PHANTASMAL_KILLER | cost 1 -> 0 | next turn attacks deal double damage | 5 | - | - | ShadowStepPower +1 | - | cost hits 0 at +1; one more turn of double damage per level Cap 5 (lead review: double-damage turns). |
| STS1_POWER_THROUGH | Block +5 | 2 Wounds to hand, block | 9 | - | - | - | - | replay of the +1 block delta is the right growth |
| STS1_PRAY | Magic (Mantra) +1 | Mantra, Insight into draw pile | 5 | - | - | - | - | Mantra leads to Divinity (energy): cap 5 |
| STS1_PRESSURE_POINTS | Magic (Mark) +3 | apply Mark, all Marked enemies lose HP | 9 | - | - | - | - | Mark is damage, safe |
| STS1_PROSTRATE | Magic (Mantra) +1 | 0 cost: Mantra, 4 block | 3 | - | - | - | - | 0-cost Mantra reaching 10 per play would chain Divinity energy; cap 3 |
| STS1_PROTECT | Block +4 | retain: block | 9 | - | - | - | - | replay of the +1 block delta is the right growth |
| STS1_PUMMEL | Repeat +1 | exhaust: 2 damage N times | 3 | Repeat +1, Damage +1 | Damage | - | - | hit count capped at 3 levels; damage per hit keeps growing so +9 still matters |
| STS1_PURITY | Cards +2 | 0 cost exhaust: exhaust up to N cards from hand | 4 | - | - | - | - | exhaust count, not draw; +4 already covers a full hand |
| STS1_QUICK_SLASH | Damage +4 | damage, draw 1 (constant) | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_RAGNAROK | Damage +1, Magic (hits) +1 | N hits of damage to random enemies | 3 | - | Damage, Magic | - | - | kept as decided: hits are damage, not plays |
| STS1_RAMPAGE | Increase +3 | damage, grows by Increase each play | 9 | Increase +3 | - | - | - | per-play damage growth is safe; perLevel because the scan missed the const-keyed var |
| STS1_REACH_HEAVEN | Damage +5 | damage, Through Violence into draw pile | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_REAPER | Damage +1 | exhaust: AoE damage, heal unblocked damage | 9 | - | - | - | - | tiny +1 delta, exhaust: healing stays small even at +9 |
| STS1_REBOUND | Damage +3 | damage, next card goes on top of the draw pile | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_RECKLESS_CHARGE | Damage +3 | 0 cost damage, Dazed into draw pile | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_RECURSION | cost 1 -> 0 | evoke your next orb and channel a copy | 9 | - | - | - | Damage +3 | cost hits 0 at +1; 0-cost repeatable, so an evoke-like hit rather than energy/draw |
| STS1_RECYCLE | cost 1 -> 0 | exhaust a hand card, gain its cost in energy | 9 | - | - | - | Block +3 | cost hits 0 at +1; 0-cost repeatable energy card, so Block, no energy |
| STS1_REFLEX | Cards +1 | unplayable: draw when discarded | 5 | - | - | - | - | draw: cap 5 |
| STS1_REINFORCED_BODY | Block +2 | X cost: block X times | 9 | - | - | - | - | replay of the +1 block delta is the right growth |
| STS1_REPROGRAM | FocusPower, StrengthPower, DexterityPower +1 | lose Focus, gain Strength and Dexterity | 5 | - | DexterityPower, StrengthPower | - | - | kept as decided: Focus loss stops at +5, Strength/Dexterity keep growing |
| STS1_RIDDLE_WITH_HOLES | Damage +1 | 5 hits | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_RIP_AND_TEAR | Damage +2 | 2 hits to random enemies | 9 | - | - | - | - | replay of the +1 damage delta is the right growth |
| STS1_RITUAL_DAGGER | Increase +2 | exhaust: damage, permanently grows on a kill | 9 | Increase +2 | - | - | - | permanent damage growth is safe; perLevel because the scan missed the IntVar delta |
| STS1_RUPTURE | StrengthPower +1 | power: Strength when you lose HP from a card | 9 | - | - | - | - | Strength is safe |
| STS1_RUSHDOWN | cost 1 -> 0 | power: draw 2 when entering Wrath | 5 | - | - | Sts1RushdownPower +1 | - | cost hits 0 at +1; +1 draw per Wrath entry per level (already a known infinite piece, kept at 1) Cap 5 (lead review: draw). |
| STS1_SADISTIC_NATURE | SleightOfFleshPower +2 | Power: whenever you apply a debuff to an enemy, it takes 5 damage | 9 | SleightOfFleshPower +2 | - | - | - | Damage per debuff is safe; perLevel pins the +2 since the scanner missed the keyed var |
| STS1_SAFETY | Block +4 | Token: 12 Block, Retain, Exhaust | 9 | - | - | - | - | Plain block, full replay |
| STS1_SANCTITY | Block +3 | 6 Block; draw 2 if the last card played was a Skill | 9 | - | - | - | - | Only Block grows; draw stays at 2 |
| STS1_SANDS_OF_TIME | Damage +6 | Retain; 20 damage; cost -1 each time it is retained | 9 | - | - | - | - | Plain damage |
| STS1_SASH_WHIP | Damage +2, Magic (Weak) +1 | 8 damage; apply 1 Weak if the last card was an Attack | 9 | - | - | - | - | Damage and Weak are safe |
| STS1_SCRAWL | cost 1 -> 0 | Exhaust; draw until your hand is full | 9 | - | - | - | Vigor +2 | Already free and draw is capped by hand size; Vigor feeds the next attack of the full hand |
| STS1_SEARING_BLOW | Damage per StS1 Searing Blow formula (grows each upgrade) | 2 cost, damage that grows with every upgrade, MaxUpgradeLevel 99 | 9 | - | - | - | - | Keeps its own MaxUpgradeLevel (ForgeLevels skips it); its formula already scales with level |
| STS1_SECRET_TECHNIQUE | loses Exhaust | Put a Skill from your draw pile into your hand (Retired: StS2 card used in every mode) | 9 | - | - | - | Block +3 | No number; a small Block per level fits a defensive tutor |
| STS1_SECRET_WEAPON | loses Exhaust | Put an Attack from your draw pile into your hand (Retired: StS2 card used in every mode) | 9 | - | - | - | Vigor +2 | No number; Vigor powers the fetched Attack |
| STS1_SEEING_RED | cost 1 -> 0 | Exhaust; gain 2 energy | 3 | Energy +1 | - | - | - | Cost already 0; grow the energy itself, capped at 3 (4 energy at +3, Exhaust) |
| STS1_SEEK | Cards +1 | Exhaust; put N chosen cards from the draw pile into hand | 5 | - | - | - | - | Tutor count, draw-like cap |
| STS1_SELF_REPAIR | Healing +3 | Power: heal 7 at the end of combat | 5 | - | - | - | - | Healing cap |
| STS1_SENTINEL | Block +3, Energy +1 | 5 Block; gain 2 energy when exhausted | 3 | - | Block | - | - | Energy capped at 3, Block keeps growing |
| STS1_SETUP | cost 1 -> 0 | Put a card from hand on top of the draw pile; it costs 0 until played | 9 | - | - | - | Block +3 | Not Exhaust and free, so no draw/energy; a small Block per level |
| STS1_SEVER_SOUL | Damage +6 | Exhaust all non-Attacks in hand; deal 16 damage | 9 | - | - | - | - | Plain damage |
| STS1_SHOCKWAVE | Power (Weak and Vulnerable amount) +2 | Exhaust; apply 3 Weak and 3 Vulnerable to all enemies | 9 | Power +1 | - | - | - | Debuff turns are safe; perLevel slows the keyed var (scanner missed it) to +1 |
| STS1_SIGNATURE_MOVE | Damage +10 | 30 damage; only playable if it is the only Attack in hand | 9 | - | - | - | - | Plain damage |
| STS1_SIMMERING_FURY | Magic (cards drawn next turn) +1 | Enter Wrath next turn and draw 2 cards next turn | 5 | - | - | - | - | Magic is draw: draw cap |
| STS1_SKEWER | Damage +3 | X-cost: deal 7 damage X times | 9 | - | - | - | - | Damage per hit is safe |
| STS1_SMITE | Damage +4 | Token: Retain, Exhaust; 12 damage | 9 | - | - | - | - | Plain damage |
| STS1_SNEAKY_STRIKE | Damage +4 | 12 damage; gain 2 energy if you discarded this turn | 9 | - | - | - | - | Only damage grows; energy stays 2 |
| STS1_SPIRIT_SHIELD | Magic (Block per card in hand) +1 | Gain 3 Block per other card in hand | 9 | - | - | - | - | Block only, safe even when large |
| STS1_SPOT_WEAKNESS | StrengthPower +1 | Gain 3 Strength if the target intends to attack | 9 | - | - | - | - | Strength is safe |
| STS1_STACK | CalculationBase +3 | Block equal to discard pile size (+3 when upgraded) | 9 | - | - | - | - | Flat Block bonus, safe |
| STS1_STATIC_DISCHARGE | Orbs +1 | Power: channel 1 Lightning whenever you take unblocked attack damage | 5 | - | - | - | - | Orb count cap |
| STS1_STEAM_BARRIER | Block +2 | 0 cost: 6 Block, loses 1 Block each play this combat | 9 | - | - | - | - | Plain block |
| STS1_STORM | Innate | Power: channel 1 Lightning whenever you play a Power | 5 | Orbs +1 | - | - | - | Innate-only upgrade; grow the Orbs var (lightning per Power) with the orb cap |
| STS1_STREAMLINE | Damage +5 | 15 damage; costs 1 less this combat each play | 9 | - | - | - | - | Plain damage |
| STS1_STRIKE_WATCHER | Damage +3 | 6 damage (Strike) | 9 | - | - | - | - | Plain damage |
| STS1_STUDY | cost 2 -> 1 | Power: at end of turn shuffle 1 Insight into the draw pile | 3 | Magic +1, cost -1 | - | - | - | Grow Insights per turn (card generation) to 3, keep lowering cost to 0 |
| STS1_SUCKER_PUNCH | Damage +2, Weak +1 | 7 damage, apply 1 Weak | 9 | - | - | - | - | Damage and Weak are safe |
| STS1_SWIFT_STRIKE | Damage +3 | 0 cost: 7 damage | 9 | - | - | - | - | Plain damage |
| STS1_SWIVEL | Block +3 | 8 Block; next Attack costs 0 (FreeAttackPower 1, hardcoded) | 9 | - | - | - | - | Block grows; the free attack stays one |
| STS1_TACTICIAN | Energy +1 | Unplayable; gain 1 energy when discarded | 3 | - | - | - | - | Energy cap |
| STS1_TALK_TO_THE_HAND | Damage +2, Magic (Block Return) +1 | Exhaust; 5 damage, target gives 2 Block to attackers per hit | 9 | - | - | - | - | Block per hit is safe |
| STS1_TANTRUM | Magic (hit count) +1 | 3 damage x3, enter Wrath, shuffles into draw pile | 5 | - | - | - | - | Hit count is a multiplier |
| STS1_TEMPEST | IsUpgraded: channel X+1 Lightning | X-cost, Exhaust: channel X Lightning | 9 | - | - | - | Damage +3 | No var; a Lightning-themed unpowered zap per level |
| STS1_TERROR | cost 1 -> 0 | Exhaust; apply 99 Vulnerable | 9 | - | - | - | Weak +1 | Vulnerable is maxed and cost is 0; terror also weakens |
| STS1_THIRD_EYE | Block +2, Magic (Scry) +2 | 7 Block, Scry 3 | 9 | - | - | - | - | Block and Scry are safe |
| STS1_THROUGH_VIOLENCE | Damage +10 | Token: Retain, Exhaust, 0 cost; 20 damage | 9 | - | - | - | - | Plain damage |
| STS1_THUNDER_STRIKE | Damage +2 | 7 damage to random enemies per Lightning channeled this combat | 9 | - | - | - | - | Damage per hit is safe |
| STS1_TRANQUILITY | cost 1 -> 0 | Retain, Exhaust; enter Calm | 9 | - | - | - | Block +3 | Calm is defensive: small Block per level |
| STS1_TRANSMUTATION | IsUpgraded: generated cards are upgraded | X-cost, Exhaust: add X random colorless cards to hand, free this turn | 9 | - | - | - | Block +3 | No var to grow; small Block per level (generation itself can't be scaled) |
| STS1_TRIP | IsUpgraded: targets all enemies | 0 cost: apply 2 Vulnerable | 9 | VulnerablePower +1 | - | - | - | Grow the Vulnerable amount, safe |
| STS1_UNLOAD | Damage +4 | 14 damage, discard all non-Attacks | 9 | - | - | - | - | Plain damage |
| STS1_VAULT | cost 3 -> 2 | Exhaust: take an extra turn, end your turn | 9 | - | - | - | - | Cost-only keeps lowering to 0 by +3; Exhaust and a Single power keep it to one extra turn |
| STS1_VIGILANCE | Block +4 | 8 Block, enter Calm | 9 | - | - | - | - | Plain block |
| STS1_VIOLENCE | Cards +1 | Exhaust, 0 cost: put 3 random Attacks from draw pile into hand | 5 | - | - | - | - | Card count cap |
| STS1_WALLOP | Damage +3 | 9 damage; gain Block equal to unblocked damage | 9 | - | - | - | - | Damage (and thus Block) is safe |
| STS1_WARCRY | Cards +1 | Exhaust, 0 cost: draw 1, put a card from hand on top | 5 | - | - | - | - | Draw cap |
| STS1_WAVE_OF_THE_HAND | Magic (Weak) +1 | This turn, whenever you gain Block apply 1 Weak to all enemies | 9 | - | - | - | - | Weak is safe |
| STS1_WEAVE | Damage +2 | 0 cost: 4 damage; returns to hand on Scry | 9 | - | - | - | - | Plain damage |
| STS1_WHEEL_KICK | Damage +5 | 15 damage, draw 2 | 9 | - | - | - | - | Only damage grows; draw stays 2 |
| STS1_WILD_STRIKE | Damage +5 | 12 damage, shuffle a Wound into the draw pile | 9 | - | - | - | - | Plain damage |
| STS1_WINDMILL_STRIKE | Damage +3, Magic (growth when retained) +1 | Retain; 7 damage, +4 damage each time retained | 9 | - | - | - | - | Damage growth only, safe |
| STS1_WISH | Strength +1, PlatedArmor +2, Gold +5 (display); IsUpgraded upgrades the three option cards once | Exhaust: choose 6 Plated Armor, 3 Strength or 25 Gold | 3 | - | - | StrengthPower +1, Sts1PlatedArmorPower +2 | - | Wish's own vars are display only: the option cards hold the real values, so replay changes nothing in play. powerBonus grows the Strength/Plated Armor actually applied (source = Wish). Gold has no hook and stays put. Cap 3 (the cap now limits powerBonus): 5 Strength or 10 Plated Armor at +3 |
| STS1_WORSHIP | Retain | Gain 5 Mantra | 3 | Magic +2 | - | - | - | Retain is already on the ladder; grow Mantra (Divinity = energy) with a low cap: 9 Mantra at +3 |
| STS1_WRAITH_FORM | IntangiblePower +1 | Power: gain 2 Intangible; lose 1 Dexterity each turn | 3 | IntangiblePower +1 | - | - | - | Intangible cap (scanner missed the keyed var) |
| STS1_WREATH_OF_FLAME | Magic (Vigor) +3 | Gain 5 Vigor | 9 | - | - | - | - | Vigor is safe |
| SUBROUTINE | cost 1 -> 0 | Power: gain 1 energy whenever you play a Power | 9 | - | - | - | Block +3 | Energy per Power can't be capped through powerBonus (uncapped to +9), so a small Block instead |
| SUCKER_PUNCH | Damage +2, Weak +1 | 8 damage, apply 1 Weak | 9 | - | - | - | - | Damage and Weak are safe |
| SUMMON_FORTH | Forge +3 | Put Sovereign Blades into hand, Forge 8 | 9 | - | - | - | - | Forge is damage, safe |
| SUNDER | Damage +8 | 26 damage; gain 3 energy if it kills | 9 | - | - | - | - | Only damage grows; energy stays 3 |
| SUPERCRITICAL | Energy +2 | Exhaust, 0 cost: gain 4 energy | 3 | Energy +1 | - | - | - | Energy cap; +1 per level instead of +2 (7 then 8) |
| SUPERMASSIVE | ExtraDamage +1 | 5 damage +3 per card you generated this combat | 9 | - | - | - | - | Damage scaling, safe |
| SUPPRESS | Damage +6, Weak +2 | Innate, 0 cost: 11 damage, 3 Weak | 9 | - | - | - | - | Damage and Weak are safe |
| SURVIVOR | Block +3 | 8 Block, discard 1 | 9 | - | - | - | - | Plain block |
| SWEEPING_BEAM | Damage +3 | 6 damage to all enemies, draw 1 | 9 | - | - | - | - | Only damage grows |
| SWEEPING_GAZE | OstyDamage +5 | Ethereal, Exhaust: Osty deals 10 damage to a random enemy | 9 | - | - | - | - | Plain damage |
| SWORD_BOOMERANG | Repeat +1 | 3 damage to random enemies 3 times | 5 | - | - | - | - | Hit count is a damage multiplier, not a replay |
| SWORD_SAGE | cost 2 -> 1 | Power: Sovereign Blade is replayed 1 extra time | 9 | - | - | - | Vigor +2 | Cost reaches 0 at +2; replays must not grow; Vigor powers the Blade |
| SYNCHRONIZE | CalculationExtra +1 | Gain temporary Focus per distinct orb type channeled | 5 | - | - | - | - | Focus multiplier cap |
| SYNTHESIS | Damage +6 | 14 damage; next Power costs 0 | 9 | - | - | - | - | Only damage grows |
| TACTICIAN | Energy +1 | Sly, cost 3: gain 1 energy | 3 | - | - | - | - | Energy cap |
| TAG_TEAM | Damage +4 | Multiplayer: 11 damage; the target's next ally attack plays twice | 9 | - | - | - | - | Damage only; extra plays stay 1 |
| TANK | cost 1 -> 0 | Multiplayer Power: take 50% more damage, allies Guarded | 9 | - | - | - | Block +4 | Single-stack power with nothing to raise; a tank gains Block |
| TAUNT | Block +1, Vulnerable +1 | 6 Block, apply 1 Vulnerable | 9 | - | - | - | - | Block and Vulnerable are safe |
| TEAR_ASUNDER | Damage +2 | 5 damage, +1 hit per time you lost HP this combat | 9 | - | - | - | - | Damage per hit is safe |
| TEMPEST | IsUpgraded: channel X+1 Lightning | X-cost: channel X Lightning | 9 | - | - | - | Damage +3 | No var; Lightning-themed unpowered damage per level |
| TERRAFORMING | VigorPower +3 | Gain 7 Vigor | 9 | - | - | - | - | Vigor is safe |
| TESLA_COIL | Damage +1; IsUpgraded: Lightning passives trigger twice | 3 damage, trigger each Lightning's passive on the target | 9 | Damage +2 | - | - | - | +1 damage is negligible; +2 per level gives the zap some weight |
| THE_BALL | Increase +5 | Multiplayer: 10 damage, +10 damage per play, passes to an ally | 5 | - | - | - | - | Per-play scaling cap |
| THE_BOMB | BombDamage +10 | In 3 turns deal 40 damage to all enemies | 9 | - | - | - | - | Damage is safe |
| THE_GAMBIT | Block +25 | 0 cost: 50 Block; die if you take unblocked damage | 9 | - | - | - | - | Block is safe |
| THE_HUNT | Damage +5 | Exhaust: 10 damage; fatal gives an extra card reward | 9 | - | - | - | - | Plain damage |
| THE_SCYTHE | Increase +2 | Exhaust: 13 damage, +5 permanently each play | 5 | - | - | - | - | Permanent per-play scaling cap |
| THE_SEALED_THRONE | cost 1 -> 0 | Power (3 stars): gain 1 star whenever you play a card | 9 | - | - | - | Block +3 | Stars per card must not grow; a small Block per level |
| THE_SMITH | Forge +10 | Star cost 4: Forge 30 | 9 | - | - | - | - | Forge is damage, safe |
| THINKING_AHEAD | loses Exhaust | 0 cost: draw 2, put 1 card from hand on top | 4 | Cards +1 | - | - | - | Free, non-Exhaust draw: grow Cards but cap at 4 (draw 5) to avoid easy loops |
| THRASH | Damage +2 | 4 damage x2, exhaust a random Attack in hand and gain its damage | 9 | - | - | - | - | Plain damage |
| THRUMMING_HATCHET | Damage +3 | 11 damage; returns to hand next turn | 9 | - | - | - | - | Plain damage |
| THUNDER | ThunderPower +3 | Power: evoking an orb deals 8 damage to its targets | 9 | - | - | - | - | Damage, safe |
| THUNDERCLAP | Damage +3 | 4 damage to all, 1 Vulnerable to all | 9 | - | - | - | - | Plain damage |
| TIMES_UP | Retain | Deal damage equal to the target's Doom | 9 | ExtraDamage +0.25 | - | - | - | Retain is on the ladder; grow damage per Doom slowly (x3 at +9) since Doom stacks are large |
| TOOLS_OF_THE_TRADE | cost 1 -> 0 | Power: draw 1 more and discard 1 each turn | 9 | - | - | - | Block +4 | Draw via powerBonus would be uncapped; a small Block per level instead |
| TORIC_TOUGHNESS | Block +2 | 5 Block now and for the next 2 turns | 9 | - | - | - | - | Plain block |
| TRACKING | cost 2 -> 1 | Power: Weak enemies take +50% attack damage (TrackingPower 50, hardcoded) | 9 | - | - | TrackingPower +25 | - | Brief example: +25% per level past +1 |
| TRANSFIGURE | loses Exhaust | A card in hand costs 1 more and gains a Replay | 9 | - | - | - | Block +3 | Replays must not grow; a small Block per level |
| TRASH_TO_TREASURE | cost 1 -> 0 | Power: channel a random orb whenever you generate a card | 9 | - | - | - | Block +3 | Orbs per generated card via powerBonus would be uncapped; a small Block per level |
| TREMBLE | Vulnerable +1 | Exhaust: apply 3 Vulnerable | 9 | - | - | - | - | Vulnerable is safe |
| TRUE_GRIT | Block +2; IsUpgraded: choose the card to exhaust | 7 Block, exhaust a random card (chosen when upgraded) | 9 | - | - | - | - | Plain block |
| TURBO | Energy +1 | 0 cost: gain 2 energy, add a Void to discard | 3 | - | - | - | - | Energy cap; the Void keeps the drawback |
| TUTOR | cost 1 -> 0 | Multiplayer: an ally puts a card from their draw pile into hand | 9 | - | - | - | Block +3 | Free already; a small Block per level |
| TWIN_STRIKE | Damage +2 | 5 damage x2 | 9 | - | - | - | - | Plain damage |
| TYRANNY | Innate | Power: draw 1 more and exhaust 1 each turn | 9 | - | - | - | Strength +1 | Innate is on the ladder; a tyrant's Strength on play (played once per combat) |
| ULTIMATE_DEFEND | Block +4 | 11 Block | 9 | - | - | - | - | Plain block |
| ULTIMATE_STRIKE | Damage +6 | 14 damage | 9 | - | - | - | - | Plain damage |
| UNDEATH | Block +2 | 0 cost: 7 Block, add a copy to discard | 9 | - | - | - | - | Plain block (the clone inherits the level) |
| UNDERWORLD | loses Exhaust | Multiplayer: this turn damage dealt also applies that much Doom | 9 | - | - | - | Vulnerable +1 | Doom multiplier via powerBonus would be uncapped; Vulnerable fits the setup |
| UNLEASH | CalculationBase +3 | Osty deals 6 + his Block damage | 9 | - | - | - | - | Flat damage, safe |
| UNMOVABLE | cost 2 -> 1 | Power: the first Block card each turn gains double Block | 9 | - | - | UnmovablePower +1 | - | One more doubled Block card per level; Block is safe |
| UNRELENTING | Damage +6 | 14 damage; next Attack costs 0 | 9 | - | - | - | - | Only damage grows |
| UNTOUCHABLE | Block +3 | Sly: 6 Block | 9 | - | - | - | - | Plain block |
| UPPERCUT | Power (Weak and Vulnerable) +1 | 13 damage, 1 Weak and 1 Vulnerable | 9 | - | - | - | - | Debuff turns are safe |
| UPROAR | Damage +2 | 6 damage x2, autoplay a random Attack from the draw pile | 9 | - | - | - | - | Only damage grows; the autoplay stays one |
| UP_MY_SLEEVE | Cards +1 | Add 3 Shivs to hand; costs 1 less this combat | 5 | - | - | - | - | Card generation cap |
| VEILPIERCER | Damage +3 | 10 damage; next Ethereal card costs 0 | 9 | - | - | - | - | Only damage grows |
| VENERATE | Stars +1 | Gain 2 stars | 3 | - | - | - | - | Stars cap |
| VICIOUS | Cards +1 | Power: draw 1 whenever you apply Vulnerable | 5 | - | - | - | - | Draw cap |
| VOID_FORM | loses Ethereal | Power: the first 2 cards each turn are free; end your turn | 3 | VoidFormPower +1 | - | - | - | Free cards per turn are energy: +1 per level to 4 |
| VOLLEY | Damage +4 | X-cost: 10 damage to random enemies X times | 9 | - | - | - | - | Plain damage |
| VOLTAIC | loses Exhaust | Cost 3: channel 1 Lightning per Lightning channeled this combat | 9 | - | - | - | Damage +3 | Orb count must not grow; Lightning-themed unpowered damage per level |
| WELL_LAID_PLANS | cost 2 -> 1 | Power (Single): retain your hand | 9 | - | - | - | Block +3 | Cost reaches 0 at +2; a small Block per level |
| WHIRLWIND | Damage +3 | X-cost: 5 damage to all X times | 9 | - | - | - | - | Plain damage |
| WHISTLE | Damage +11 | Exhaust: 33 damage, stun the target | 9 | - | - | - | - | Plain damage; stun stays |
| WHITE_NOISE | cost 1 -> 0 | Exhaust: add a random Power to hand, free this turn | 9 | - | - | - | Block +3 | Generation count can't grow; a small Block per level |
| WISH | Retain | Exhaust, 0 cost: put any card from your draw pile into hand | 5 | - | - | - | Draw +1 | Retain is on the ladder; one-shot Exhaust tutor, so a tiny draw per level, draw cap (draw 4 at +5) |
| WISP | Retain | Exhaust, 0 cost: gain 1 energy | 3 | Energy +1 | - | - | - | Energy var grows, capped (3 energy at +3, Exhaust) |
| WRAITH_FORM | IntangiblePower +1 | Power: gain 2 Intangible, lose Dexterity each turn | 3 | - | - | - | - | Intangible cap |
| WROUGHT_IN_WAR | Damage +2, Forge +2 | 7 damage, Forge 7 | 9 | - | - | - | - | Damage and Forge are safe |
| ZAP | cost 1 -> 0 | Channel 1 Lightning | 9 | - | - | - | Damage +3 | Free already; a Lightning-themed unpowered zap per level |

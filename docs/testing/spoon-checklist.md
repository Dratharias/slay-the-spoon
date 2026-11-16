# Spoon (1.3.0) — in-game checklist

Nothing here has been tested in game. Log lines: `[SlaySpoon] spoon: ...`. For a quick Spoon fight, the console
`sts1`-style spawning is not supported: reach the act 3 boss with 300+ max HP (give max HP with Fruit Juice and Feast).

## Setup and Spoonful

- [ ] Custom Run shows the "Spoon" checkbox with its short text (EN and FR). Standard and Daily stay vanilla.
- [ ] Run start: every player (all five characters) has one Spoonful in the deck: cost 1, Attack, 10 damage, Fatal +3 max HP.
- [ ] Spoonful has Retain and Innate, is not exhausted when played, and shows no card frame or portrait glitch.
- [ ] Upgrade: +2 damage and +1 max HP. It can be upgraded at the rest site and by events.
- [ ] It cannot be removed (merchant, events) or transformed. Vanilla Feed still appears in Ironclad rewards.
- [ ] Spoonful portrait (placeholder) shows.
- [ ] Compatible with King Midas, Matriochka, Folie a deux and the Act 4 option: no crash, Spoonful still given.

## Max HP economy

- [ ] The 12 relics (Big Mushroom, Darkstone Periapt, Fake Mango, Chosen Cheese, Dragon Fruit, Lee's Waffle, Looming
      Fruit, Nutritious Oyster, Stone Humidifier, Mango, Pear, Strawberry) can come out of chests, elites, the merchant,
      events and Ancients, and each can be taken.
- [ ] Every rest site has a "Feast" option: +5 max HP, spends the rest action like Rest or Smith. Co-op: each player's own.
- [ ] Ruby Recall and Feast both fit on the rest site list.
- [ ] Every merchant sells Fruit Juice (+5 max HP) at its usual price, in one potion slot; the layout is otherwise vanilla.
- [ ] Bought, the Fruit Juice is gone like any shop potion.
- [ ] Merchant prices in a Spoon run match the same seed without Spoon, apart from the Juice slot (cards, other
      potions and removal cost; relic offers may differ, since the max HP relics join the grab bags).
- [ ] Seeds stay vanilla: the same seed with the modifier off gives the same map and rewards (beyond the added relics).

## The Spoon act

- [ ] Act 3 boss dies with a player at 300+ max HP: the Spoon act follows (rest site, merchant halfway, then the boss). Log line "Spoon act inserted".
- [ ] A10: only the second act 3 boss leads to the Spoon; the first one behaves as before.
- [ ] Nobody at 300 (max HP 299): no Spoon, the run ends as vanilla (victory, or The Ending).
- [ ] Co-op: one 300+ player is enough; every client enters the Spoon act.
- [ ] Act 4 run with all three keys: act 3, then the Spoon (act 4), then The Ending (act 5).
- [ ] The Spoon is the final boss without The Ending: its death wins the run (Architect, then victory screen).
- [ ] The Spoon with The Ending behind it: ordinary boss rewards, then The Ending starts.
- [ ] The Ending already appended earlier: the Spoon still goes in front of it.
- [ ] Map: the Spoon map screen is laid out correctly (rest site, then the boss), no NaN/blank layout.
- [ ] Winged Boots and the Flight modifier on the Spoon map: no softlock, no charge spent.
- [ ] The floor counter in the act (known cosmetic skip) and the act title "The Spoon" / "La Cuillère" read well.
- [ ] Save and reload at the Spoon rest site and at the boss room: the act and fighter set are restored; reload at the room restarts the fight.
- [ ] Open design call: Act 4 run missing only the Ruby key: Ruby Recall at the Spoon rest site (see `docs/backlog.md`).

## The fight

- [ ] Fighters are the players with 300+ max HP when the fight starts; the others are supporters.
- [ ] Spoon HP is about 1500 per fighter (counts fighters only); Monster HP / Monster Damage modifiers apply.
- [ ] Mouthful: 30 base damage, eats 10 max HP per fighter hit, the Spoon heals by the max HP that dropped.
- [ ] Ladle Smash: 50 base damage. Strength grows by 3 after each move; the first hit is at base.
- [ ] Damage is scaled by the team's total max HP: solo 300 takes the base, solo 600 takes half; duo 600 in total takes the base.
- [ ] Intents show the scaled number per player; a supporter sees 0, a fighter sees the scaled hit.
- [ ] Mouthful's debuff-style intent reads well (it stands for the max HP loss).
- [ ] The Spoon never targets a supporter and a supporter takes no damage from it (no on-hit reactions either).
- [ ] Supporter damage to the Spoon is nullified: attacks, Poison, Doom, Demise, Constrict, Strangle, Disintegration,
      Magic Bomb, Corpse Explosion, Mark, Haunt, Judgement, orbs, summons (Osty), thorns, Combust, Fire Breathing,
      A Thousand Cuts, Panache.
- [ ] Supporter debuffs still apply (Uppercut's Weak and Vulnerable, Strength loss).
- [ ] A fighter's damage works normally, including Poison and Doom.
- [ ] Pressure Points by a supporter on a Spoon a fighter marked: the Spoon takes no damage (fixed in the review wave).
- [ ] Max HP eaten and given stay lost after a win; nothing is returned.
- [ ] Save and reload during the Spoon fight (solo and co-op): fighters, supporters, Spoon-Feed and the boss HP come back.
- [ ] Defeat: every fighter dead while supporters live ends the run (game-over screen), also in co-op with one fighter.
- [ ] Solo at 300+: the player is the fighter and there are no supporters; Spoon-Feed is not given.

## Spoon-Feed

- [ ] Each supporter gets one Spoon-Feed at combat start (cost 1, Skill, Retain, not exhausted); fighters get none.
- [ ] Targeting: the arrow can only land on a living fighter; with the arrow on a supporter the play is refused (known UX gap).
- [ ] Play: the supporter loses 5 max HP, the fighter gains 5; the team's total, and the damage, stay the same.
- [ ] A supporter with 2 to 5 max HP: Spoon-Feed gives all but 1 max HP (e.g. 3 max HP gives 2); the supporter stays
  alive at 1 max HP and 1 HP, and the fighter gains exactly what the supporter lost.
- [ ] A supporter with 1 max HP: Spoon-Feed is unplayable (refused like other unplayable cards).
- [ ] A supporter with 6+ max HP: the supporter loses 5 and the fighter gains 5.
- [ ] Text (EN/FR) says "up to 5" and "keeping at least 1".
- [ ] Not playable when no fighter is alive. The card cannot be upgraded and is not added to the deck after the fight.
- [ ] Spoon-Feed portrait (placeholder) shows.

## Art

- [ ] Spoon sprite (about 600x900 placeholder): stands on screen, not cropped, facing the players.
- [ ] Hitbox: clicking and targeting the Spoon works; intents sit above it and are not clipped (top at -700, intents at -760).
- [ ] Boss map icon and its outline show on the map and the fight start screen.
- [ ] Combat background shows in the Spoon fight; the rest site uses the Ending's campfire background.
- [ ] Card portraits for Spoonful and Spoon-Feed show in hand, deck view and card library without a missing-image mark.
- [ ] Text: every name (Spoonful / Cuillerée, Spoon-Feed / Becquée, The Spoon / La Cuillère, Bouchée, Coup de louche)
      reads right in EN and FR; the owner proofreads.

## Main-menu test start ("Test: The Spoon")

- [ ] With the mod in the game's `mods/SlaySpoon/` folder, the main menu shows "Test: The Spoon" (FR "Test : la
      Cuillère") under Multiplayer; the log has `spoon test: mod source ModsDirectory ... test button shown`.
- [ ] With only the Workshop copy (no local `mods/SlaySpoon`), the button is absent (log: `test button hidden`).
- [ ] With a run already saved, the button is hidden (like Singleplayer); after Abandon Run it comes back.
- [ ] Clicking it starts a solo Ironclad run directly on the Spoon act's map (rest site, merchant, then the Spoon); no Neow,
      no act 1 map. Top bar shows the "Spoon" and "Test" modifiers.
- [ ] Player: 350 max HP and 350 HP, vanilla starting gold, Burning Blood, and the Spoonful in hand at combat start.
- [ ] Deck (unupgraded): Body Slam ×2, Barricade, Entrench, Impervious, Shrug It Off ×2, Flame Barrier, Iron Wave ×2,
      Metallicize (StS1 port), Juggernaut, Defend ×4, Strike ×2, Bash, plus the Spoonful.
- [ ] Music and background are the Spoon act's; the map's boss icon is the Spoon.
- [ ] Save and quit at the rest site and in the fight, then Continue: the run reloads on the Spoon act with the same
      deck, HP and modifiers.
- [ ] Winning the fight wins the run (the Spoon is the last act); losing shows the game-over screen.
- [ ] Game over (win or loss): no unlock score bar progress, no new epoch; the modded profile's play time, floors,
      encounter stats and enemy kill stats do not change (log: `spoon test: run end progress skipped`, `unlock score
      skipped`). The run still appears in the run history as a Custom Run.

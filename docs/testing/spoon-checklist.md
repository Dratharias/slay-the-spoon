# Spoon (1.3.0) — in-game checklist

Nothing here has been tested in game. Log lines: `[SlaySpoon] spoon: ...`. For a quick Spoon fight, the console
`sts1`-style spawning is not supported: reach the act 3 boss with 300+ max HP (give max HP with Fruit Juice and Feast).

## Setup and Spoonful

- [ ] Custom Run shows the "Spoon" checkbox with its short text (EN and FR). Standard and Daily stay vanilla.
- [ ] Run start: every player (all five characters) has one Spoonful in the deck: cost 1, Attack, 10 damage, Fatal +3 max HP.
- [ ] Spoonful has Retain (no longer Innate, owner nerf 2026-11-16), is not exhausted when played, and shows no card frame or portrait glitch.
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
- [ ] 1.3.1: the Spoon and Ending map screens are as compact as act 3's rows (no long empty climb to the boss), the dotted paths join
      the moved points, traveled paths are colored, scrolling stops just above the boss, and selecting a point
      scrolls to the current row. Other acts' maps are unchanged.
- [ ] Winged Boots and the Flight modifier on the Spoon map: no softlock, no charge spent.
- [ ] The floor counter in the act (known cosmetic skip) and the act title "The Spoon" / "La Cuillère" read well.
- [ ] Save and reload at the Spoon rest site and at the boss room: the act and fighter set are restored; reload at the room restarts the fight.
- [ ] Open design call: Act 4 run missing only the Ruby key: Ruby Recall at the Spoon rest site (see `docs/backlog.md`).

## The fight

- [ ] Fighters are the players with 300+ max HP when the fight starts; the others are supporters.
- [ ] Spoon HP is about 2000 per fighter (counts fighters only); Monster HP / Monster Damage modifiers apply.
- [ ] The cycle (2026-11-16 cutlery spec) repeats every 4 turns: Sets the Table, Mouthful, Banquet, Ladle Smash.
- [ ] Sets the Table: Frail 2 on every living fighter; each living player (supporters too) gets their kit shuffled into
      the draw pile with the vanilla card preview (log: `spoon: set the table for a N-card deck`). Intents: debuff + status.
- [ ] Mouthful: 35 base damage; the Spoon gains Block equal to the HP its hit took past Block (none when fully blocked).
      Intents: attack + defend. No max HP eaten.
- [ ] Banquet (1.3.2): a light 12 damage hit (attack intent shown with the debuff and heal), then Vulnerable 2 on every fighter; each fighter loses 10 max HP (not a hit: no Block, no on-hit reactions)
      unless Skimmed; the Spoon heals 60 solo, 70 / 80 / 90 with 2 / 3 / 4 players (log: `spoon: Banquet ate ... healed`).
- [ ] Ladle Smash: 55 base damage. No Strength from turns passing. It steals max HP from each living player hit: a quarter
      of the damage past their Block (115 through → 28; all blocked → 0), never below 1 max HP; the Spoon gains them and
      1 Strength per 5 stolen in total (log: `spoon: Ladle stole`, then `Strength from max HP`).
- [ ] Growth: HP lost past Block raises the Spoon's max HP by as much (its HP does not rise).
- [ ] Strength follows max HP: (max HP − max HP grown from hits − start max HP) / 5, so stolen max HP counts, rounded toward zero (1.3.2:
      growth no longer gives Strength, no more 200+ hits); it goes negative when Melon Baller takes it below its start (log: `spoon: max HP ... Strength from max HP S`). A player's Strength debuff on the
      Spoon (Disarm, Malaise...) stays applied after the Spoon's own Strength changes.
- [ ] No max HP scaling: the Mouthful intent reads 35 + Strength (times Appetite) whatever the players' max HP.
- [ ] Appetite: gained only after Sets the Table, 2 per living player (supporters included: 2 solo, 8 at 4 players), +5 %
      damage per stack in the intent; it goes negative (−5 % per stack, never below 0 damage), for the Spoon and the
      players. A Spoonful played on the Spoon removes 1 (2 upgraded) even at 0 (→ −1); a supporter's Spoonful removes it too while dealing 0. EN/FR tooltip reads well.
- [ ] Animations (`SpoonMotion`, tweens on the static sprite, pivot at the handle tip): slow idle sway; Mouthful leans
      back and swoops at the players, the hit lands at the bottom, then it rises back up; Ladle Smash rises and slams
      with a squash; a short recoil when hit; it swells a little as it grows (at most 1.2×); on death it tips over and
      fades. Sets the Table hops and taps three times, tilting left and right; Banquet leans back and swells in three
      gulps, then shudders. No error line `spoon: animation failed` in the log.
- [ ] Intents show the scaled number per player; a supporter sees 0, a fighter sees the scaled hit.
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

## Cutlery (combat-only, Exhaust)

- [ ] Kit at each Sets the Table (1.3.2): one each of Teaspoon, Wooden Spoon, Skimmer, Melon Baller and Stolen Spoonful;
      from 25 cards, 1 extra random kit spoon, + 1 per further 5 cards (25: +1, 30: +2). Log:
      `spoon: set the table for a N-card deck: Teaspoon 1, Wooden 1, ...`; co-op clients deal the same extras.
- [ ] Every cycle adds a new kit: unplayed cutlery piles up in the draw/discard piles. Nothing reaches the deck after
      the fight; the cards never appear in rewards, the merchant or random generation.
- [ ] Teaspoon (0, targets the Spoon): removes 2 Appetite (3 upgraded), draws 1, even below 0.
- [ ] Wooden Spoon (1, attack, targets the Spoon): deals 10 % of the player's max HP (35 at 350), then Sated 1 (2 upgraded): the Spoon's
      Appetite adds no damage on its next turn (2 upgraded), the intent shows its normal damage, the stacks stay and it
      still gains Appetite after the move (log: `spoon: Sated turn spent`); Sated shows with its tooltip (EN/FR).
- [ ] Skimmer (1, self): 6 Block (9 upgraded) and Skimmed: the next Banquet eats none of this player's max HP; Skimmed
      ends after the Spoon's next move.
- [ ] Melon Baller (2, targets the Spoon): takes 12 max HP (15 upgraded) from the Spoon, never below 1, and the player
      gains them (with the heal of a vanilla max HP gain); the Spoon's Strength drops accordingly.
- [ ] Stolen Spoonful (owner merge, 2026-11-16): no longer in the kit (4 spoons). At the Spoon fight start every Spoonful
      in the combat piles becomes a Stolen Spoonful, upgrade kept (log: `Spoonful -> Stolen Spoonful`): 1, attack,
      Retain, no Exhaust, 10 damage (12 upgraded), steals 1 Appetite (2 upgraded) that the player keeps. After a move that
      pierced a player's Block the Spoon takes 1 from that player, even below 0. After the fight the deck still holds
      the Spoonful.
- [ ] A supporter's cutlery works too (they are targetable on the Spoon even though the supporter deals it no damage).
- [ ] Portraits (Pixabay placeholders) and EN/FR titles and descriptions read well, Banquet named in gold.
- [ ] Co-op: every client sees the same kits, the same Sated/Skimmed state and the same Spoon max HP.

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
- [ ] Deck (every card upgraded): Body Slam ×2, Barricade, Entrench, Impervious, Shrug It Off ×2, Flame Barrier, Iron Wave ×2,
      Metallicize (StS1 port), Juggernaut, Defend ×4, Strike ×2, Bash, plus the Spoonful.
- [ ] Relics besides Burning Blood: Anchor, Horn Cleat, Captain's Wheel, Orichalcum, Bronze Scales, Vajra, Lantern;
      each triggers in the fight.
- [ ] Music and background are the Spoon act's; the map's boss icon is the Spoon.
- [ ] Save and quit at the rest site and in the fight, then Continue: the run reloads on the Spoon act with the same
      deck, HP and modifiers.
- [ ] Winning the fight wins the run (the Spoon is the last act); losing shows the game-over screen.
- [ ] Game over (win or loss): no unlock score bar progress, no new epoch; the modded profile's play time, floors,
      encounter stats and enemy kill stats do not change (log: `spoon test: run end progress skipped`, `unlock score
      skipped`). The run still appears in the run history as a Custom Run.

# Spoon (1.3.0) — in-game checklist

Nothing here has been tested in game. Log lines: `[SlaySpoon] spoon: ...`. For a quick Spoon fight, the console
`sts1`-style spawning is not supported: reach the act 3 boss with the team's max HP at the threshold (300 solo, 550/750/900 for 2/3/4 players) (give max HP with Fruit Juice and Feast).

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

- [ ] Act 3 boss dies with the team's summed max HP at 300 / 550 / 750 / 900 (1–4 players): the Spoon act follows (rest site, merchant halfway, then the boss). Log line "Spoon act inserted".
- [ ] A10: only the second act 3 boss leads to the Spoon; the first one behaves as before.
- [ ] Team total one short (299 solo, 549 for two): no Spoon, the run ends as vanilla (victory, or The Ending).
- [ ] Co-op: the sum counts, however it is split (e.g. 500 + 50 for two); every client enters the Spoon act.
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

- [ ] Every player fights (1.3.7: no more supporters). Spoon HP is 1200 solo, 1200 × players × 1.3 in co-op (vanilla scaling, act 3 boss factor);
      Monster HP / Monster Damage modifiers apply. Log: `spoon: fight starts, N player(s), HP ...`.
- [ ] The cycle (2026-11-16 cutlery spec) repeats every 4 turns: Sets the Table, Mouthful, Banquet, Ladle Smash.
- [ ] Sets the Table: Frail 2 on every living player; each living player gets their kit shuffled into
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
- [ ] Appetite: gained only after Sets the Table, 2 per living player (2 solo, 8 at 4 players), +5 %
      damage per stack in the intent; it goes negative (−5 % per stack, never below 0 damage), for the Spoon and the
      players. A Spoonful played on the Spoon removes 1 (2 upgraded) even at 0 (→ −1). EN/FR tooltip reads well.
- [ ] Animations (`SpoonMotion`, tweens on the static sprite, pivot at the handle tip): slow idle sway; Mouthful leans
      back and swoops at the players, the hit lands at the bottom, then it rises back up; Ladle Smash rises and slams
      with a squash; a short recoil when hit; it swells a little as it grows (at most 1.2×); on death it tips over and
      fades. Sets the Table hops and taps three times, tilting left and right; Banquet leans back and swells in three
      gulps, then shudders. No error line `spoon: animation failed` in the log.
- [ ] Every player's damage works normally, including Poison, Doom, Mark (Pressure Points) and Judgement.
- [ ] Max HP eaten stays lost after a win; nothing is returned.
- [ ] Save and reload during the Spoon fight (solo and co-op): the boss HP and the Stolen Spoonfuls come back.
- [ ] Defeat: every player dead ends the run (vanilla game-over screen).

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
- [ ] Portraits (Pixabay placeholders) and EN/FR titles and descriptions read well, Banquet named in gold.
- [ ] Co-op: every client sees the same kits, the same Sated/Skimmed state and the same Spoon max HP.

## Art

- [ ] Spoon sprite (about 600x900 placeholder): stands on screen, not cropped, facing the players.
- [ ] Hitbox: clicking and targeting the Spoon works; intents sit above it and are not clipped (top at -700, intents at -760).
- [ ] Boss map icon and its outline show on the map and the fight start screen.
- [ ] Combat background shows in the Spoon fight; the rest site uses the Ending's campfire background.
- [ ] Card portraits for Spoonful and the cutlery show in hand, deck view and card library without a missing-image mark.
- [ ] Text: every name (Spoonful / Cuillerée, The Spoon / La Cuillère, Bouchée, Coup de louche)
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

## Merchant prices and short-map scrolling regression (2026-11-16)

Automated validation: build without warnings; 742 xUnit and 91 pytest tests pass.

- [ ] Spoon merchants price promoted event/Ancient relics from their grab-bag tier (175/225/275 base gold,
      normal variation and discounts), including restocks; other modes retain their original prices.
- [ ] Fresh Spoon and Ending maps have compact paths. Drag, wheel and controller scrolling stop at their bounds.
- [ ] Save at the merchant, quit and Continue: both short acts retain compact spacing and traveled-path colors.
      Closing/reopening the map at each node keeps its points visible rather than showing a long empty path.
- [ ] Starting a normal act after a short-act run restores ordinary map spacing and scrolling.

## Strength baseline regression (2026-11-16)

- [ ] With a 2000 max HP combat baseline, growth to 2010 grants +2 Strength; dropping back to 2000 removes it;
      dropping to 1985 gives -3 Strength. Growth from hits and Ladle theft both count.
- [ ] Independent Strength changes are preserved when the max HP contribution updates.
- [ ] Reloading combat establishes the baseline again at combat start.

## 1.3.3 balance and Banquet regression

- [ ] Solo Spoon starts at 1200 max HP. Mouthful/Ladle base damage is 25/35; Banquet heals 30.
- [ ] Unblocked hits still grow max HP by 100%; Strength stays 1 per 5 above/below combat-start max HP.
- [ ] With Block, Strength, Appetite, Vulnerable, Buffer or Intangible, an unprotected Banquet still takes exactly
      8 current HP before its separate 10 max HP loss (which can additionally clamp current HP).
- [ ] Skimmer played before an earlier move persists until Banquet, prevents both losses, then expires.
      Banquet still applies Vulnerable and heals. Melon Baller costs 1 energy.
- [ ] Banquet intent and tooltip show 8 unprotected and 0 protected, without attack scaling.
- [ ] Other attacks retain normal damage modifiers, prevention and pet redirection. Test co-op and save/reload.

## 1.3.4 short-map boss visibility

The boss height adjustment now preserves the parent anchor translation, like the other map points.
Previously it overwrote the laid-out position with a pre-parenting coordinate, leaving most of the icon above
view even at maximum scroll. Geometry tests cover the full placeholder including hover scale.

- [ ] On fresh and reloaded Spoon/Ending maps, scroll to the boss: the entire icon and hover outline are visible
      below the top bar and the boss can be selected. Check mouse wheel, drag and controller.

## 1.3.5 combat power icons

The combat power UI now resolves borrowed/own packed icons explicitly before loading the texture, avoiding
an inlined vanilla getter that looked up nonexistent mod IDs in the vanilla atlas.

- [ ] Metallicize shows the Plating icon; Skimmed shows Buffer, both below the player and in hover tips.
- [ ] Other borrowed powers (including Appetite/Sated) and the own-art Block Return retain their intended icons.
- [ ] Vanilla buffs, amounts, flashes and hover tips remain unchanged. No missing-sprite warnings for these powers.

## 1.3.6 publication

Owner approved the balance after play-testing. Attack hits now steal one Appetite even when fully blocked;
once per player per move. A Banquet canceled by Skimmed never hits.
The test menu/launcher/deck and install-detection helper were removed; legacy run flags and progress guards remain.

- [ ] Fully block Mouthful/Ladle: lose 1 Appetite, boss gains 1; max HP growth/theft still require HP damage.
- [ ] Sets the Table steals none; a Skimmed Banquet steals none from its protected player.
- [ ] Main menu has no test entry, including a local install. Existing test saves can still reload.

## 1.3.7 team threshold, no supporters (owner, 2026-11-16)

- [ ] Solo: 300 max HP opens the Spoon act, 299 does not.
- [ ] Two players: 550 summed opens it (e.g. 300 + 250), 549 does not. Three: 750. Four: 900.
- [ ] Every player is targeted by the Spoon's attacks, Frail, Banquet and Vulnerable; nobody gets Spoon-Feed (removed).
- [ ] Co-op Spoon HP is 1200 × players × 1.3 (vanilla scaling); the intent numbers match each player's own Appetite/Strength state.
- [ ] An old save in the Spoon fight with a pre-1.3.7 layout still loads (Spoon-Feed was never a deck card).

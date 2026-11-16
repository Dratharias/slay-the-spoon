# Spoon (Cuillère) — design

Owner requests of 2026-11-14 ("mode Spoon thématique : le joueur est spoonfed…"), approach picked the same day
("oui, option 1. quand le coeur est là, le coeur devient acte 5").

## Intent

A themed run about being spoon-fed: players grow their max HP all run long, and a huge max HP is the ticket to an
extra final boss, the Spoon. The fight is a brute-force check: it has a lot of HP and hits hard; "le scaling le bat"
(a run that scaled well beats it). The owner draws the Spoon art; placeholders until then.

**Goals:** max HP becomes the run's currency; one extra boss fight that only 300+ max HP players take on, the others
helping from the side. **Non-goals:** a complex boss (no minions, no phases), Standard/Daily changes, a new map.
**Anti-goal:** a Spoon that cannot be beaten by a well-scaled deck, or that a supporter can trivialise.

## Approaches

1. **An appended one-room act "Spoon" (picked).** Built like The Ending (`src/Act4/Act4Routing.cs`,
   `Sts1EndingAct`): saves, co-op sync, background, final-boss victory already work for an appended act.
2. A second combat chained in the act 3 boss room: more spectacular, but the game has no support for two combats in
   one room (rewards, combat end, victory would be fragile). Dropped.
3. An extra map node after the act 3 boss, like the A10 second boss: A10 already uses that slot, and adding a node
   mid-run depending on max HP means editing a live map. Dropped.

## Rules

1. **Activation.** A Custom Run modifier `SpoonSpoon` ("Spoon" / "Cuillère"), a checkbox like King Midas. No
   multiplayer host card. Standard and Daily stay vanilla. *(Builder proposal accepted by the owner.)*
2. **Spoonful, the Feed copy.** At run start every player gets one mod card copied from StS2's Ironclad `Feed`
   ("Appétit" in French): cost 1, Attack, 10 damage, if Fatal +3 max HP; upgraded +2 damage and +1 max HP.
   Differences: **Retain** and **Innate**, and **no Exhaust**. Every character gets it. It can be upgraded (rest
   site, events, effects), but it cannot be removed or transformed. Vanilla Feed stays in the Ironclad pool.
3. **Max HP relics in the normal draws.** These 12 relics can come out of the ordinary relic draws (chests, elites,
   merchant): Big Mushroom, Darkstone Periapt, Fake Mango, Chosen Cheese (Event), Dragon Fruit, Lee's Waffle (Shop),
   Looming Fruit, Nutritious Oyster, Stone Humidifier (Ancient), Mango (Rare), Pear (Uncommon), Strawberry (Common).
3a. **Rest site "Feast".** (Owner addition, 2026-11-14: "les feux de camps ont l'option de gagner 5pv max".) Every
   rest site has one more option: gain 5 max HP. It spends the player's rest action like Rest or Smith.
   Name proposal: "Feast" / "Festin".
3b. **Fruit Juice at every merchant.** (Owner addition, same day: "le jus de fruit est dispo a tout les marchands".)
   Every merchant sells StS2's Fruit Juice potion (+5 max HP, Rare) in one of its potion slots, at its usual price.
   Builder call: it replaces one random potion slot (drawn on the mod's own `Rng`) when the merchant has none, so
   the shop layout stays vanilla; bought, it is gone like any shop potion.
4. **The Spoon act.** When the last act 3 boss dies (the second one at A10), if at least one player has **300 max
   HP or more**, a one-room act "The Spoon" is inserted right after act 3. One 300+ player is enough in co-op.
   - With The Ending (Act 4 run, all three keys): act 3 → **Spoon (act 4)** → **The Ending (act 5)**. The Spoon
     goes in front of The Ending even when The Ending was appended earlier.
   - Map: a rest site, a merchant halfway, then the Spoon. (Owner addition, 2026-11-16: "le chemin est
     effroyablement long, j'ajouterais un marchand midway".)
   - Nobody at 300: no Spoon; the run ends as it does today (victory, or The Ending).
5. **The fight.** Fighters are the players with 300+ max HP when the Spoon fight starts; the others are supporters.
   *(Builder call: max HP cannot drop between the act 3 boss and the Spoon, so this keeps everyone who qualified, adds
   anyone the act 3 boss rewards push past 300, and needs no saved state: a reload restarts the fight from the room.)*
   - The Spoon never targets a supporter and a supporter cannot take damage from it.
   - A supporter's damage to the Spoon is nullified, but their debuffs apply (for example Uppercut's Weak and
     Vulnerable). Supporters play their deck normally otherwise. *(Builder proposal, not contradicted by the owner:)*
     every damage source a supporter owns is nullified, Poison and Doom they applied included, along with orbs,
     summons and thorns; only pure debuffs (Weak, Vulnerable, Strength loss and the like) get through.
   - **Bouchée** ("Mouthful") attacks remove max HP as well as HP, and every point of max HP eaten heals the Spoon
     by that much.
   - **Nothing is given back** (owner, 2026-11-14, replacing the earlier "rendus après le combat": "a la victoire
     contre la cuillère, les pv mangés et donnés restent perdus"). Max HP eaten by Bouchée and max HP given with
     Becquée stay lost after a win; a loss ends the run anyway. No max HP or HP bookkeeping is kept for the fight.
6. **The gift card.** Every supporter gets a combat-only card "Spoon-Feed" ("Becquée"): cost 1, choose a fighter
   (Mend's ally targeting); the supporter loses 5 max HP and the fighter gains 5. Retained, not exhausted.
   *(Numbers: builder proposal accepted by the owner.)* Owner decision 2026-11-16: playable while the supporter has
   more than 1 max HP; it moves min(5, max HP − 1), so the supporter keeps at least 1 max HP and never dies from it.
7. **After the Spoon.** If The Ending follows, the Spoon gives ordinary boss rewards and the run goes on; otherwise
   it is the final boss and its death wins the run.

> **2026-11-16:** the fight's moves, Strength and the player's answer to them are replaced by
> `2026-11-16-spoon-cutlery-design.md` (4-turn cycle, cutlery kit, Strength from max HP). The numbers below are history.

## Boss numbers (starting point, tuned after play)

Accepted as a proposal by the owner ("à ajuster une fois en jeu"):

- About 2000 HP per fighter (owner, 2026-11-16, was 1500), through StS2's multiplayer formula counting fighters only, with act 3 boss factors
  (as Act 4 does: `Act4MultiplayerScalingPatch`). Monster HP / Monster Damage modifiers apply as everywhere.
- **Damage (owner, 2026-11-16, replaces the scaling by the team's max HP, which with Strength and the heal stacked
  three ways):** each hit is base + Strength, times Appetite. Strength comes only from growth (below); the players' max
  HP no longer changes the damage. Intents show the result.
- **Appetite** (`SpoonAppetitePower`, owner idea 2026-11-16): +2 after each move, +10 % damage per stack; every
  Spoonful played on the Spoon removes 3, a supporter's included (their damage is nothing, the feeding counts). It
  pushes the players to play Spoonful. Numbers are builder proposals, tuned after play.
- Moves: **Bouchée**: 40 base damage and 10 max HP eaten; **Coup de louche** ("Ladle Smash"): 70 base damage
  (raised from 30 and 50 with the growth rule below; builder numbers, tuned after play).
- **Growth (owner, 2026-11-16, replaces "+3 Strength every turn", which reached 24 Strength by turn 9 in the first
  test and left no energy for anything but Block):** the Spoon gains as much max HP as the HP its hits take from players
  past their Block (no heal from it), and 1 Strength per 10 max HP gained over the fight (remainders carried over).
  It heals only by the max HP Bouchée eats. Fully blocked hits make it grow nothing. Strength adds to the base before
  the max HP scaling. Move order is a builder call (a simple
  fixed or alternating cycle; no minions).

## Art (owner, later)

Placeholders ship first; each drawing is a backlog item in `docs/backlog.md`. See "Drawing list" below.

| # | Drawing | Format | Needed? | Placeholder |
|---|---------|--------|---------|-------------|
| 1 | The Spoon, combat sprite | PNG, transparent background, about 600×900 px, facing left | yes | a recoloured vanilla boss sprite or a plain silhouette |
| 2 | The Spoon, map boss icon | PNG 512×512, transparent, plus the same shape as an outline (`heart_boss.png` / `heart_boss_outline.png`) | yes | drawing 1 scaled down |
| 3 | Spoon act combat background | JPG 1920×1136 | optional | act 3's background |
| 4 | "Becquée" card portrait | PNG, the mod's portrait format (250×190, same as `images/cards/`) | optional | a vanilla portrait (Feed or Mend) |
| 5 | "Spoonful" card portrait | same | optional | Feed's portrait |

Animations are out of scope (static sprite, as the Ending monsters started).

## Constraints

- English and French for every text; names above are the builder's proposals (`Spoonful` / `Cuillerée`,
  `Spoon-Feed` / `Becquée`, `The Spoon` / `La Cuillère`, `Bouchée`, `Coup de louche`) and the owner proofreads.
- Never write the regular save; the act list and fighter set must survive save/reload and be the same on every
  co-op client (decide at a synced point, as the Ending routing does).
- Vanilla RNG streams do not move: any random choice uses the mod's own named `Rng`.
- Pure rules (fighter selection, 300 threshold, act order, gift amounts, HP restore bookkeeping) go in
  `src/Core/SpoonRule.cs` with xUnit tests.

## Builder calls

- Insertion timing: the act must be inserted before vanilla treats the act 3 boss as the final boss (win time,
  victory room). The Ending already clears a stale `WinTime` (`EndingRouteRule.ShouldResetWinTime`); the Spoon needs
  the same care. `EndingRouteRule.ShouldAppend` checks `actCount == 3` and must accept the new shapes.
- `VanillaActIndex.For` already maps act indexes past 2 to act 3 for vanilla code that throws on them.
- **Losing to the Spoon is a defeat** (owner, 2026-11-14: "défaite si la cuillère gagne"), with or without The Ending
  behind it. The fight is lost when every fighter is dead, even if supporters are alive (they cannot finish it): the
  vanilla game-over screen runs. The act 3 boss is therefore no longer a sure win once someone reaches 300.
- How the 12 relics enter the draws (pool and rarity for the Event and Ancient ones; Shop ones already sell at the
  merchant) is the builder's call, without moving vanilla RNG.
- Compatible with every other mod modifier (King Midas included: Spoonful is a starting card, not a reward).

## Amendment 1.3.7: team threshold, no supporters (owner, 2026-11-16)

Supersedes rules 4–6 and every supporter line above. Everyone plays normally: there are no fighters or supporters, and
Spoon-Feed is removed. The Spoon act is inserted when the team's **summed** max HP reaches 300 (solo), 550 (2 players),
750 (3) or 900 (4) (`SpoonRule.TeamThreshold`). The Spoon has 1200 HP before the vanilla multiplayer scaling, which
multiplies it by the player count and the act 3 boss factor; its attacks, Frail, Banquet and Vulnerable reach every
living player. The fight is lost when every player is dead (vanilla).

Why these numbers (owner's, builder-checked): a Spoonful's max HP goes only to the player whose Spoonful lands the
killing blow, never to the team. In co-op the enemies are divided between the players (each one dies once), so each
player lands fewer killing blows than in solo; rest sites, relics, Fruit Juice and merchants stay per player. A solo run
gets roughly 50 max HP from Spoonful kills, so each extra player costs everyone about 10–15 of it. The per-player average
falls 300 → 275 → 250 → 225, slightly more than that loss: a little generous (builder's assessment).

## Amendment 1.3.7: Appetite in every fight (owner, 2026-11-16)

Appetite is no longer the Spoon fight's alone. In a Spoon run, each hit an ordinary monster lands on a player, even
fully blocked, takes 1 Appetite from that player (below 0 too) and gives it to the monster (`SpoonSpoon.AfterAttack`,
`SpoonRule.StolenOnHits`); the Spoon keeps its per-move rule. The Spoonful removes its Appetite (1, 2 upgraded) from any
monster it hits, not only the Spoon. Appetite lasts one combat (owner).

## Amendment 1.3.7: Slay the Spoon mode card (owner, 2026-11-16)

The mode is named "Slay the Spoon" in both languages. A "Slay the Spoon" card on the solo and multiplayer host screens
starts a Standard run carrying `SpoonSpoon` (a Standard exception, like Folie à deux); the Custom Run checkbox stays.
Menu text: "Dinner is served. The Spoon is waiting." / "À table, la Cuillère vous attend." Builder call: Neow treats
the flag as hidden, like Folie à deux, so both entry points get Neow's usual options.

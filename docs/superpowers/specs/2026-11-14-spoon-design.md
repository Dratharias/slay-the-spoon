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
   - Nobody at 300: no Spoon; the run ends as it does today (victory, or The Ending).
5. **The fight.** Fighters are the players with 300+ max HP when the act is inserted; the others are supporters.
   - The Spoon never targets a supporter and a supporter cannot take damage from it.
   - A supporter's damage to the Spoon is nullified, but their debuffs apply (for example Uppercut's Weak and
     Vulnerable). Supporters play their deck normally otherwise. *(Builder proposal, not contradicted by the owner:)*
     every damage source a supporter owns is nullified, Poison and Doom they applied included, along with orbs,
     summons and thorns; only pure debuffs (Weak, Vulnerable, Strength loss and the like) get through.
   - **Bouchée** ("Mouthful") attacks remove max HP as well as HP, and every point of max HP eaten heals the Spoon
     by that much.
   - Every max HP change during this fight is undone after it (losses and gifts alike); max HP gained from
     Spoonful kills in this fight is also undone, since only the Spoon could be killed. *(Builder call.)*
   - *(Builder proposal, not contradicted by the owner:)* current HP comes back with the max HP: a player whose
     max HP dropped by 100 (Bouchée) or who gave 15 (Becquée) gets that HP back too; ordinary damage stays. This
     matters when The Ending follows.
6. **The gift card.** Every supporter gets a combat-only card "Spoon-Feed" ("Becquée"): cost 1, choose a fighter
   (Mend's ally targeting); the supporter loses 5 max HP and the fighter gains 5. Retained, not exhausted.
   *(Numbers: builder proposal accepted by the owner.)*
7. **After the Spoon.** If The Ending follows, the Spoon gives ordinary boss rewards and the run goes on; otherwise
   it is the final boss and its death wins the run.

## Boss numbers (starting point, tuned after play)

Accepted as a proposal by the owner ("à ajuster une fois en jeu"):

- About 1500 HP per fighter, through StS2's multiplayer formula counting fighters only, with act 3 boss factors
  (as Act 4 does: `Act4MultiplayerScalingPatch`). Monster HP / Monster Damage modifiers apply as everywhere.
- **Damage relative to the team's max HP** (owner, 2026-11-14: "la cuillère fait des dégâts relatif aux max hp,
  plus on en as, moins elle est forte", then "cumulatif sur la team" / "on additionne toute l'équipe"). Each hit is
  scaled by the sum of every player's current max HP, supporters included:
  `damage = base × (300 × players) / teamMaxHp`, rounded down, at least 1. *(Builder call: the reference grows with the
  player count, so a solo run reads `base × 300 / maxHp` and co-op is not trivial.)* Solo: 300 max HP takes the base,
  600 takes half. Duo: 600 in total takes the base (for example a fighter at 320 and a supporter at 280). Bouchée eats
  max HP, so the Spoon gets stronger as the fight drags on; a Becquée gift moves max HP inside the team and leaves the
  total, and the damage, unchanged. Intents show the scaled number.
- Moves: **Bouchée**: 30 base damage and 10 max HP eaten; **Coup de louche** ("Ladle Smash"): 50 base damage; gains
  3 Strength every turn (Strength adds to the base before the max HP scaling). Move order is a builder call (a simple
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

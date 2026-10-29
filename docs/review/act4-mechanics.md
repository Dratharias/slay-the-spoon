# Act 4 mechanics audit (step 6, work package 2)

Sources: local CFR output of the StS1 jar (`~/Projects/sts-decomp/sts1/`, outside the repo) and the StS2 ilspy
output (`~/Projects/sts-decomp/sts2/`). Only behavior is recorded, never decompiled code. The data lives in
`data/sts1/act4/mechanics.json`; it pins the SHA-256 of every StS1 class it was read from, and
`tools/sts1/tests/test_act4_mechanics.py` fails if the installed jar changes.

Ascension numbers below are **StS1 levels** (1-20). StS2 has 10; the mapping is an owner question (see the end).

## The act

Fixed five-node map: rest site → merchant → elite (Shield and Spear) → boss (the Heart) → true victory.
No events, treasure or other encounters.

## Spire Shield (elite, left of the player)

- HP 110 (125 from A8). Starts with Artifact 1 (2 from A18). At combat start it gives the player Surrounded.
- **Bash:** 12 damage (14 from A3), then −1 Strength to the player; a player with orbs has a 50 % chance to lose
  1 Focus instead.
- **Fortify:** 30 Block to every enemy.
- **Smash:** 34 damage (38 from A3), then gains Block equal to the damage it dealt (after modifiers); from A18 it
  gains 99 Block instead.
- **Pattern**, by turn modulo 3: 1st, 50 % Fortify / 50 % Bash; 2nd, Bash unless it just used Bash, else
  Fortify; 3rd, Smash.

## Spire Spear (elite, right of the player)

- HP 160 (180 from A8). Starts with Artifact 1 (2 from A18).
- **Burn Strike:** 5 damage × 2 (6 × 2 from A3), then 2 Burns in the discard pile; from A18 the Burns are
  shuffled into the draw pile instead.
- **Piercer:** +2 Strength to every enemy.
- **Skewer:** 10 damage × 3 (× 4 from A3).
- **Pattern:** 1st, Burn Strike unless it just used it, else Piercer; 2nd, Skewer; 3rd, 50 % Piercer /
  50 % Burn Strike.

**Death of either one:** if the other still stands, the player turns to face it, loses Surrounded, and the
survivor loses Back Attack.

**Back Attack:** the player starts facing right, so the Shield begins behind them. The enemy behind the player
deals ×1.5 (after the player's damage-received modifiers and stance, rounded down). Playing a card that
targets a single enemy turns the player toward that enemy.

## Corrupt Heart (boss)

- HP 750 (800 from A9). At combat start: Invincible 300 (200 from A19), Beat of Death 1 (2 from A19).
- **Debilitate** (always its first move): Vulnerable 2, Weak 2, Frail 2, and one each of Dazed, Slimed, Wound,
  Burn and Void shuffled into the draw pile.
- **Blood Shots:** 2 damage × 12 (× 15 from A4).
- **Echo:** 40 damage (45 from A4).
- **Buff:** removes its negative Strength and gains 2 Strength, then, by how many times it has buffed:
  Artifact 2, Beat of Death +1, Painful Stabs, Strength +10, then Strength +50 every time after that.
- **Pattern** after Debilitate, by move modulo 3: 1st, 50 % Blood Shots / 50 % Echo; 2nd, Echo unless it just
  used Echo, else Blood Shots; 3rd, Buff.

## Powers and their StS2 port

| Power | StS1 behavior | StS2 port |
|-------|---------------|-----------|
| Invincible | Caps the Heart's HP loss, **after block**, to what remains; the remainder drops by each HP loss and refills at the start of the monsters' turn. Every damage type counts, HP loss and poison included. | New power. StS2's `ModifyDamageCap` caps the amount **before** block (`Hook.ModifyDamage`), and `HardToKillPower` caps each hit, so neither fits as is. Cap = remaining + the Heart's current Block; lower the remaining by `DamageResult.UnblockedDamage` in `AfterDamageReceived`; refill in `BeforeSideTurnStart` for the enemy side. Thorns dealt during the monsters' turn eat into the next player turn's pool, as in StS1. |
| Beat of Death | After the player plays a card, deals the amount to that player as Thorns. | New power on the `GalvanicPower` pattern: `AfterCardPlayed` damages the card's owner. As dealer-less or unpowered enemy damage, it must go through `MonsterDamageRule` once, like Galvanic. |
| Painful Stabs | Each of the Heart's hits that deals unblocked damage adds 1 Wound to the discard pile. | `PainfulStabsPower` exists. Reuse. |
| Surrounded | Player marker; facing follows the targeted enemy. | `SurroundedPower` exists (Kaiser Crab), facing per player, ×1.5 from the enemy behind. Reuse. |
| Back Attack | Applied to whichever enemy is behind. | StS2 puts a fixed `BackAttackLeftPower` / `BackAttackRightPower` on each enemy; `SurroundedPower` checks it against the facing. Shield gets Left, Spear gets Right (Kaiser Crab: `Crusher` / `Rocket`). Same result. |

## StS2 integration points

- **Monster model:** `MonsterModel` with `MinInitialHp`/`MaxInitialHp`, values through
  `AscensionHelper.GetValueIfAscension(...)`, `GenerateMoveStateMachine()` with `MoveState`s, intents and
  `FollowUpState` links. Combat-start powers go on in `AfterAddedToRoom`. The 50 % choices need the monster's own
  synced RNG, never a local `Random`.
- **Co-op:** StS2 enemy attacks hit every player (`DamageCmd.Attack(...).FromMonster(this)`), and debuffs take an
  opponent list. Debilitate, Bash and Burn Strike therefore apply to each player. Bash's Focus/Strength roll is
  made per player.
- **Two enemies with sides:** `KaiserCrabBoss` is the template for Shield + Spear.
- **Monster modifiers:** attacks go through `FromMonster`, so Monster Damage scales them once. Monster HP scales
  through the existing postfixes. Smash's Block equals the damage it dealt, so it follows Monster Damage too.
- **Visuals:** StS2 ships the Spine **4.2** runtime. The StS1 skeletons are Spine **3.4.02**: they must be
  converted to 4.2, or replaced by another visual (owner choice, work package 5).

- **Multiplayer HP scaling crashes past act 3.** `Creature.ScaleHpForMultiplayer` multiplies HP by
  `playerCount × MultiplayerScalingModel.GetMultiplayerScaling(encounter, actIndex)`, and that method throws
  `ArgumentOutOfRangeException` for any act index above 2. Act 4 (index 3) needs a patch there before any co-op
  fight. Owner decision (2026-10-29): act 4 HP follows StS2's own formula. Since the formula has no act 4 factor,
  act 4 reuses act 3's (1.2 elite, 1.3 boss): HP × player count × factor.

## Owner decisions (2026-10-29)

1. **Ascension mapping.** StS1 HP raises (A8/A9) go to StS2 **Tough Enemies (A8)**, damage raises (A3/A4) to
   **Deadly Enemies (A9)**, and the StS1 A18/A19 changes (Invincible 200, Beat of Death 2, Artifact 2, Smash
   99 Block, Burns in the draw pile) to **A10** (Double Boss, the highest level).
2. **Invincible in co-op.** The cap scales like the Heart's HP, with StS2's formula: 300 (or 200) × player
   count × 1.3, so a co-op fight takes about as many turns as a solo one. Every player's damage shares that one cap.
3. **Multiplayer HP.** Act 4 enemies follow StS2's formula (HP × player count × act factor), with act 3's factors.

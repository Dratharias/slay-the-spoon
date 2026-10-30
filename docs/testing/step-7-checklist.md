# Step 7 — The Watcher in-game checklist

Build and automated tests pass. All checks below require the owner in game; none is claimed complete.
Mod lines in `~/.local/share/SlayTheSpire2/logs/godot.log` start with `[SlaySpoon]`; also look there for
`ERROR` lines that mention `sts1_watcher`. Details and fallbacks: `docs/review/watcher-spike.md`.

## Character shell spike (Task 1)

Start each run from Custom Run with "Slay the Spire 1 Cards" or "Spire Souvenir" ticked, then pick the Watcher.

- [ ] The run starts: transition fade and sound, Neow, then the map with her marker. No error in the log.
- [ ] Top bar: her icon, 72/72 HP, 99 gold, Pure Water in the relic bar with its EN/FR text and its StS1 icon.
- [ ] Deck view: 4 Strike, 4 Defend, Eruption, Vigilance, with StS1 portraits and EN/FR text.
- [ ] Combat: her body stands at the usual place, hover box and intents look right, 3 energy with the purple orb
      (log has no `Watcher energy counter: no LayerN`), Strike/Eruption deal 6/9, Defend/Vigilance give 5/8 Block.
- [ ] Combat: attack and skill sounds play; the card trail is purple.
- [ ] Card reward after combat: offers cards (Ironclad's for now) and no crash.
- [ ] Elite win and boss win: no crash in the progress save (epoch checks skipped).
- [ ] Rest site: her sprite stands at the fire, sized like a vanilla character, no Silent visible; Rest and Smith work.
- [ ] Merchant: her sprite stands beside the merchant, no Silent visible; buying works.
- [ ] Death in combat: corpse sprite and death sound, then the game over screen shows her and the run history entry.
- [ ] Victory: the Architect victory event proceeds without crashing and the victory screen shows. Her attack scene
      is not expected: the Architect has no dialogue for an unknown character (known gap).
- [ ] Save and quit mid-run, then continue: the run resumes as the Watcher.
- [ ] Standard and Daily runs never offer or show her; the regular (unmodded) save is untouched.

## Progress guards (Task 3)

- [ ] After a Watcher Custom run (any outcome) with fewer than five vanilla characters played, the Daily and Orobas
      epochs are not granted early (timeline unchanged); log has no `watcher: rewrote` or `watcher: no CharacterStats` error.
- [ ] Solo Custom run with the Watcher: the ascension selector goes up to the best vanilla character's max ascension
      (0 when no vanilla ascension is unlocked), and the run starts at the chosen level instead of being reset to 0.
- [ ] Vanilla characters keep their own max ascension in solo runs; Standard and Daily are unchanged.

## Custom Run lobby (Task 2)

- [ ] Solo Custom run: her button is hidden with no modifier or only Monster HP/Damage; ticking "Slay the Spire 1 Cards"
      shows it at the end of the row; she can be picked and the run starts as her. Unticking while she is picked hides
      it and selects the last vanilla character again (Ironclad if none was picked).
- [ ] Same with "Spire Souvenir" as the enabling modifier: she can be picked and the run starts as her.
- [ ] Closing and reopening Custom Run shows a single Watcher button, never two.
- [ ] `make mp`: the host ticks "Slay the Spire 1 Cards"; the client sees her button, picks her, and the host sees the
      client on the Watcher (remote outline and lobby icon); both ready and reach a combat as Ironclad + Watcher.
- [ ] `make mp`: with the client on the Watcher, the host unticks the modifier: the button hides on both sides and the
      client falls back to its last vanilla pick, which the host also sees. Re-ticking shows her again.
- [ ] `make mp`: the host itself picks the Watcher, then unticks: the host falls back and the client sees the change.
- [ ] Controller/keyboard focus: moving right from the last vanilla button reaches her only while she is shown.
- [ ] Orobas may offer her Archaic Tooth only when her deck holds Bash, Neutralize, Unleash, Falling Star or Dualcast
      (vanilla rule, e.g. after Prismatic Gem or a transform); otherwise its third option follows vanilla.
- [ ] Standard and Daily character select never show her button.

## Task 4 — Stances and Mantra

Automated validation: 314 xUnit tests and 66 pytest tests pass. The following checks require the game.

- [ ] Eruption deals damage before entering Wrath; Vigilance gives Block before entering Calm.
- [ ] Leaving Calm gives 2 energy; entering Divinity gives 3; re-entering the same stance gives nothing.
- [ ] Wrath doubles outgoing card attacks and incoming enemy attacks, with matching intents; Thorns and power damage stay unchanged.
- [ ] Divinity triples card attacks and ends at the holder's next turn. Verify in solo and co-op.
- [ ] Gain 9, 10 and 12 Mantra through `StanceCmd.GainMantra`: threshold enters Divinity and keeps the remainder.
- [ ] `sts1stance calm|wrath|divinity|neutral` changes only the issuing player's stance on every peer.
- [ ] Save/reload mid-combat preserves stance and Mantra and restores the staff eye; death hides it.
- [ ] EN/FR stance and Mantra tooltips are readable.

## Task 2 — Button failure fallback

- [ ] In a temporary debug build, throw before scene creation and then after button insertion: Custom Run still opens,
  vanilla characters remain selectable, no partial Watcher button remains, and the exception is logged.
- [ ] Toggle modifiers and reopen the cached screen after the failure; the vanilla screen stays usable.

## Task 5 — Scry and last card played

Automated validation: 326 xUnit tests and 66 pytest tests pass; build has 0 warnings/errors.

- [ ] `sts1scry 3` reveals the top three cards, in draw order, with an EN/FR prompt. Select none, some and all.
- [ ] Draw afterward: cards not selected remain in the original order; selected cards enter discard in reveal order.
- [ ] A pile smaller than the requested amount shows only its contents; an empty pile never reshuffles or opens a selection.
- [ ] Co-op: each peer applies the issuing player's selection, while the other player's piles stay unchanged.
- [ ] Post-Scry listeners fire once even with zero cards shown or discarded; a card just moved to discard is included.
- [ ] Scry does not activate manual hand-discard effects (Tactician, Reflex, Tough Bandages or Sly).
- [ ] During a card's OnPlay, `LastCardPlayed.Type` returns the owner's previous completed play, including replay;
  teammates' plays do not change it. It returns null before the first completed play or outside combat.
- [ ] Once the relevant cards are ported: Retain and temporary Retain use vanilla expiration, and retain-growth effects
  react only to the owning player's `AfterFlush` retained-card list.

Golden Eye's +2 is covered by pure math tests; wiring the relic is part of Task 6.

## Task 6 — Generated cards, relics and potions

See `docs/review/watcher-task6.md` for values, hidden-pool decisions and test commands.

- [ ] Pure Water creates one Miracle before the first draw each combat; Retain works and playing it gains 1/2 Energy.
- [ ] Insight draws 2/3, Smite deals 12/16, Safety gives 12/16 Block, Through Violence deals 20/30; Retain and Exhaust work.
- [ ] Expunger deals 9/15 damage per configured hit, keeps its hit count when cloned, and does not Exhaust.
      Full creation through Conjure Blade awaits Task 8.
- [ ] Beta costs 2/1, Exhausts, and shuffles an unupgraded Omega into draw; both peers see the same insertion.
- [ ] Omega deals 50/60 at the holder's turn end, stacks, and is not amplified by Wrath/Divinity or Strength.
- [ ] Wish options apply 3/4 Strength, 6/8 Plated Armor or 25/30 Gold without firing card-play hooks.
      Full selection through Wish awaits Task 8.
- [ ] Plated Armor gives Block at the owner's turn end; each unblocked attack removes one stack. Blocked attacks,
      HP costs, Thorns and teammate turns do not remove stacks. Verify multiple hits and Buffer.
- [ ] Holy Water removes Pure Water on pickup and creates three Miracles, not four. Save/reload preserves replacement.
- [ ] Damaru gives one Mantra before each owner draw, including the first turn and extra turns; a teammate's extra
      turn grants none. At 9 Mantra, entering Divinity survives the turn-start stance expiry and grants 3 Energy.
- [ ] Teardrop Locket starts each combat in Calm; Violet Lotus gives 3 Energy on leaving Calm, never on re-entry.
- [ ] Melange asks the shuffling player to Scry 3; Golden Eye makes it 5. Empty and short piles never reshuffle again.
- [ ] Native Cloak Clasp remains a single shared relic. Duality grants temporary Dexterity per attack and clears it
      at the holder's turn end; teammate attacks have no effect.
- [ ] Bottled Miracle creates two Miracles; Stance Potion's unskippable choice enters the selected stance;
      Ambrosia enters Divinity. Verify all three on both peers and with potion-doubling effects.
- [ ] All relic art sizes and potion borrowed icons/outlines load; EN/FR cards, powers, relics and potions show no raw placeholders.
- [ ] Generated/choice cards never appear in rewards, shops, transforms or ordinary random generation.
- [ ] Holy Water and Violet Lotus never appear in ordinary rewards/shops/random generation; debug grants work.
- [ ] Vanilla characters retain their existing potion/relic pools; Standard and Daily are unchanged.

## Task 7 — Attacks

- [ ] Bowling Bash hits the target once per living enemy (7/10 each); a dead or escaped enemy is not counted.
- [ ] Brilliance deals 12/16 plus all Mantra its owner gained this combat (Mantra that became Divinity still counts);
      the preview updates in hand; a teammate's Mantra and a previous combat's Mantra never count.
- [ ] Carve Reality deals 6/10 and puts a Smite in hand (hand full: it goes to discard as usual).
- [ ] Conclude hits ALL enemies for 12/16, then ends the owner's turn (Retain and end-of-turn effects still run).
- [ ] Consecrate hits ALL enemies for 5/8 at 0 cost.
- [ ] Crush Joints deals 8/10 and applies 1/2 Vulnerable only if the owner's previous card was a Skill.
- [ ] Cut Through Fate deals 7/9, Scrys 2/3 (owner chooses), then draws 1.
- [ ] Empty Fist deals 9/14 and returns to Neutral (Calm exit energy; no effect when already Neutral).
- [ ] Fear No Evil deals 8/11 and enters Calm only if the target intended to attack when the card was played.
- [ ] Flurry of Blows deals 4/6 and returns from the discard pile to hand on each stance change of its owner,
      not on a teammate's change, and not from draw or exhaust.
- [ ] Flying Sleeves hits twice for 4/6 each and is retained at end of turn.
- [ ] Follow-Up deals 7/11 and gives 1 Energy only if the owner's previous card was an Attack.
- [ ] Just Lucky Scrys 1/2, gains 2/3 Block, then deals 3/4 damage.
- [ ] Lesson Learned deals 10/13, Exhausts, and a fatal hit upgrades one random upgradable card in the owner's deck
      (permanent; a teammate's deck is untouched; no effect without a kill or with Minion-style non-fatal deaths).
- [ ] Kill the last enemy with Lesson Learned: a deck card is upgraded (also saved after reload, same card on both peers).
- [ ] Ragnarok costs 3 and hits random enemies 5/6 times for 5/6; each hit picks again, both peers agree.
- [ ] Reach Heaven deals 10/15 and shuffles a Through Violence into the draw pile.
- [ ] Sands of Time (20/26, cost 4, Retain) loses 1 cost per retained turn this combat; the deck copy is unchanged next combat.
- [ ] Sash Whip deals 8/10 and applies 1/2 Weak only if the owner's previous card was an Attack.
- [ ] Signature Move (30/40) is unplayable, and not glowing, while another Attack is in hand; playable when alone.
- [ ] Talk to the Hand deals 5/7 and Exhausts; the enemy gets Block Return 2/3; every attack on it (card, relic-less
      player damage) gives the ATTACKING player that Block, in co-op too; Thorns and HP loss do not.
- [ ] Tantrum deals 3 damage 3/4 times, enters Wrath, and shuffles itself into a random spot of the draw pile.
- [ ] Wallop deals 9/12 and gives Block equal to the unblocked damage dealt (zero when fully blocked).
- [ ] Weave deals 4/6 and returns from the discard pile to hand whenever its owner Scrys (also from a Scry that
      discarded itself); a teammate's Scry does not.
- [ ] Wheel Kick deals 15/20 and draws 2.
- [ ] Windmill Strike (7/10, Retain) gains 4/5 damage per retained turn this combat (kept if downgraded in combat); the deck copy is unchanged next combat.
- [ ] All 25 cards show EN/FR text with no raw placeholders and the right hover tips (stances, Mantra, Smite, Through Violence, Fatal, Block Return).

## Task 8 — skills

- [ ] Alpha (cost 1, Exhaust; Innate upgraded) shuffles a Beta into a random spot of the draw pile.
- [ ] Blasphemy (cost 1, Exhaust; Retain upgraded) enters Divinity; the owner dies at the start of their next turn
      (Fairy in a Bottle still saves them); a teammate is unaffected.
- [ ] Collect is X cost, Exhausts; for the next X (X+1 upgraded) turns a Miracle+ arrives in hand at turn start; Chemical X adds 2;
      X = 0 unupgraded does nothing; the power counts down to nothing.
- [ ] Conjure Blade is X cost, Exhausts; it shuffles an Expunger that hits X (X+1 upgraded) times (Chemical X included); the
      Expunger hits that many times.
- [ ] Crescendo (cost 1, Exhaust, Retain; cost 0 upgraded) enters Wrath.
- [ ] Deceive Reality gives 4/7 Block and a Safety in hand.
- [ ] Deus Ex Machina is unplayable; when drawn (turn draw or card draw) it adds 2/3 Miracles to hand and exhausts; it also
      triggers when drawn into a full hand (check what happens to the Miracles).
- [ ] Empty Body gives 7/10 Block and exits the stance; Empty Mind draws 2/3 and exits the stance (Calm exit energy).
- [ ] Evaluate gives 6/10 Block and shuffles an unupgraded Insight into the draw pile (card text matches).
- [ ] Foreign Influence (cost 0, Exhaust) offers 3 distinct Attacks drawn from the card set mode's pools (StS1-only: Ironclad,
      Silent, Defect, Watcher StS1 attacks; Hybrid adds Necrobinder and Regent); the pick goes to hand; upgraded it costs 0 this turn.
- [ ] Halt (cost 0) gives 3/4 Block, plus 9/14 more while in Wrath (two Block gains).
- [ ] Indignation applies 3/5 Vulnerable to ALL enemies while in Wrath, otherwise enters Wrath.
- [ ] Inner Peace draws 3/4 while in Calm, otherwise enters Calm.
- [ ] Judgement (cost 1) kills an enemy at or below 30/40 HP, otherwise does nothing.
- [ ] Meditate puts 1/2 cards from the discard pile into hand (all of them when few), Retains them this turn, enters Calm and
      ends the turn.
- [ ] Omniscience (cost 4/3, Exhaust) lets the owner pick any draw pile card and plays it twice; the card ends in the exhaust pile
      (no duplicate left behind); an X card or a targeted Attack picks sensible targets/energy.
- [ ] Perseverance (5/7 Block, Retain) gains 2/3 Block for each turn it is retained this combat; the deck copy is unchanged next combat.
- [ ] Pray gains 3/4 Mantra and shuffles an Insight into the draw pile.
- [ ] Pressure Points applies 8/11 Mark, then every enemy loses HP equal to its Mark (ignores Block); a second play adds to the Mark and
      triggers again; in co-op the Mark is shared.
- [ ] Prostrate (cost 0) gains 2/3 Mantra and 4 Block; its upgraded text does not say DEPRECATED.
- [ ] Protect (cost 2, Retain) gives 12/16 Block.
- [ ] Sanctity gives 6/9 Block and draws 2 only if the owner's previous card was a Skill.
- [ ] Scrawl (cost 1/0, Exhaust) draws until the hand is full (10 cards).
- [ ] Simmering Fury: next turn the owner enters Wrath before drawing and draws 2/3 extra cards.
- [ ] Spirit Shield (cost 2) gives 3/4 Block for each other card in hand.
- [ ] Swivel gives 8/11 Block; the next Attack costs 0.
- [ ] Third Eye gives 7/9 Block and Scrys 3/5.
- [ ] Tranquility (cost 1/0, Exhaust, Retain) enters Calm.
- [ ] Vault (cost 3/2, Exhaust) ends the turn and the owner takes an extra turn; enemies do not act in between; in co-op only the
      owner gets the extra turn.
- [ ] Wave of the Hand: this turn each Block gain applies 1/2 Weak to ALL enemies; it wears off at end of turn.
- [ ] Wish (cost 3, Exhaust) offers Strength 3/4, Plated Armor 6/8 and Gold 25/30 through the synchronized choice screen; the
      upgraded options show upgraded values.
- [ ] Worship (cost 2) gains 5 Mantra; upgraded it Retains.
- [ ] Wreath of Flame gives 5/8 Vigor.
- [ ] All 34 cards show EN/FR text with no raw placeholders and the right hover tips.

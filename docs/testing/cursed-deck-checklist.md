# Cursed Deck — in-game checklist (1.6.0)

Spec: `docs/superpowers/specs/2026-11-18-cursed-deck-design.md`. Plan: `docs/superpowers/plans/2026-11-18-cursed-deck.md`.
Curse table (owner page): `docs/review/cursed-curses.md`. Tick only what was seen in game.

## Custom Run
- [ ] A "Cursed Deck" / "Deck maudit" row with EN/FR text; the top bar shows its short name.
- [ ] Without the mode (and in Standard and Daily) curses stay vanilla: Unplayable, +0, no target, no Embrace option,
      vanilla relic draws.
- [ ] A player holding the Grimoire outside the mode (console `relic add SPOON_UNHOLY_GRIMOIRE`) still cannot play curses.

## Start
- [ ] Every player starts with the Unholy Grimoire and one random curse (never Ascender's Bane or Curse of the Bell).
- [ ] The Grimoire is not also in a relic reward. Same seed gives the same start curse on every client.

## Playable curses
- [ ] The curse shows a cost of 1 (0 when upgraded) and can be played on one enemy; the Grimoire text shows 5 and 12.
- [ ] Playing it exhausts it: 5 damage to every enemy (12 for an upgraded curse with no effect), then its effect on the chosen enemy.
- [ ] Table check: Bad Luck -13 HP, Decay 2 damage, Regret 1 HP per card in hand, Doubt Weak, Shame Vulnerable, Debt +10 gold,
      Pain 1 HP per card played this turn, Parasite -3 max HP; upgraded values 20 / 4 / 2 per card / 2 / 2 / 15 / - / 5.
- [ ] A played curse no longer hurts you at end of turn; one left in hand does as in vanilla.
- [ ] Normality, Clumsy, Injury, Writhe, Guilty, Enthralled, Greed, Folly, Poor Sleep, Spore Mind play with the Grimoire damage only.
- [ ] Exhausted curses are back next combat. Necronomicurse+ still costs 1 and does not loop.
- [ ] Pride: Innate, Exhaust, puts a copy on the draw pile at end of turn. Playing it works.
- [ ] The targeting arrow shows on a playable curse (also on ones with no played effect: decide if it should).
- [ ] An auto-played curse (Mayhem-style effect) picks a random enemy target and does not crash.

## Upgraded curses
- [ ] Smith, Armaments, Apotheosis or the Black Censer upgrade a curse to +1 (title "+"), never beyond.
- [ ] Normality+ allows 4 cards and its red glow matches (glows only when the limit is reached). Guilty+ leaves after 3 combats.
- [ ] Regret+ costs 2 HP per card left in hand; Bad Luck+ costs 20 HP left in hand.
- [ ] Save and reload (Review Focus 4): an upgraded curse reloads upgraded, in the deck and in combat.
- [ ] Curses never fuse in a Forge run.

## Parasite and Pride
- [ ] Removing a Parasite from the deck costs its max HP once (3, or 5 upgraded); transforming it costs it once, not twice.
- [ ] Parasite and Pride have their portraits and EN/FR text; they appear in random curse draws; Necronomicurse does not.

## Embrace the curse (events)
- [ ] Every "?" event gets one extra option "Embrace the curse" before any Leave option; the event's own options are unchanged.
- [ ] It adds a random curse and one reward: a cursed relic not held, 75 gold or a heal of 25 % max HP.
      With every cursed relic held the relic share becomes gold.
- [ ] Ancients (Neow, act Ancients) never show it.
- [ ] A shared event (e.g. Punch Off) offers Embrace; a team vote for it skips the fight (design call, see backlog).
- [ ] Co-op: both players get the same curse and reward for the same player slot.
- [ ] Reload in the middle of an event: note whether Embrace comes back on a later page (known minor).

## Cursed relics
- [ ] Chest, elite and merchant relic draws are cursed about 2 in 3 (restart runs and count), from the pool of 10:
      Calling Bell, Distinguished Cape, Cursed Pearl, Blood-Soaked Rose, Preserved Fog, Darkstone Periapt,
      Blood Pact, Pin Doll, Black Censer, Reliquary.
- [ ] Pickup effects run (Calling Bell's curse and three relics, etc.). Never a relic the player already holds.
- [ ] Once a player holds every cursed relic the draws are vanilla. Seeds for the other third stay vanilla.
- [ ] Co-op: two players see the same cursed relic rolls, and a chest never shows the same cursed relic twice.
- [ ] Blood Pact: drawing a curse gives 4 Block. Pin Doll: +1 Strength per 3 curses at combat start.
      Black Censer: every curse that joins the deck is upgraded. Reliquary: 3 gold per curse at combat end.
- [ ] Relic art and EN/FR text for the four custom relics.
- [ ] A shop shows a cursed relic at a normal price (225 gold, give or take the usual variation) and it can be bought.
- [ ] A rest-site Dig or an event relic (Trial, Ranwid...) is never cursed; only chests, elites and shops roll cursed relics.
- [ ] The Architect (act 3 victory room) shows no Embrace option.

## Compatibility
- [ ] Deck of 10: curses take slots; dropping them in the keep grid works.
- [ ] King Midas: Midas events also get Embrace; Midas relics stay gold.
- [ ] Blood: Embrace's gold reward becomes the heal.
- [ ] Folie à deux: cursed relics replace relic draws before the chests; the bids still work.
- [ ] The Forge: curses never fuse.
- [ ] Any other mode ticked with it (Mutation, Trophies, Spoon, Matriochka...): nothing breaks.

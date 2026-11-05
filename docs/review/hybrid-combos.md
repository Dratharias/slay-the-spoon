# Hybrid and Souvenir combo pairs (owner-reviewed 2026-11-05)

Owner decision (2026-11-05): cross-game combos stay possible but are rarer as a pair. A card that would complete a
listed pair already held by the team (any player's deck, co-op included) has its offer weight multiplied by 0.5 in
card rewards, merchants and random generation. The list lives in a JSON file once validated; nothing is implemented yet.

Reviewed with the owner on 2026-11-05: ticked pairs are kept, the others dropped. Add new pairs at the bottom.

| Keep | Card A (holder) | Card B | Why it snowballs |
|------|-----------------|--------|------------------|
| [x] | Terror (Silent, StS1: 99 Vulnerable) | Dominate (Ironclad, StS2: Strength per Vulnerable) | 99 Vulnerable turns into 99 Strength (the owner's example) |
| [x] | Terror (Silent, StS1) | Bully (StS2: extra damage per Vulnerable) | Damage scales with 99 stacks |
| [-] | Terror (Silent, StS1) | Expose (StS2: Vulnerable after removing Artifact/Block) | Expose clears the Artifact that would block Terror |
| [-] | Catalyst (Silent, StS1: double/triple Poison) | Accelerant (StS2: Poison triggers again) | Multiplied Poison ticking several times per turn |
| [-] | Catalyst (Silent, StS1) | Noxious Fumes (StS2: Poison to all every turn) | Every enemy's Poison doubles or triples |
| [x] | Choke (Silent, StS1; allies' cards trigger it in Custom Run co-op) | Flurry of Blows (Watcher: 0-cost, returns on stance change) | Each ally card deals Choke damage, Flurry replays for free |

Answers (owner, 2026-11-05):
- A pair counts team-wide: the two cards may sit in different players' decks.
- The weight targets pairs that mixing creates: StS1 with StS2, or two characters that never share a deck in vanilla
  (Choke + Flurry of Blows is Silent + Watcher, both StS1, and is kept on purpose). No other same-game pairs.

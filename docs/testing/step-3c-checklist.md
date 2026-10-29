# Step 3c — Colorless cards

Target: StS2 v0.107.1. All in-game checks below remain pending. Every colorless card is
implemented (6 attacks, 22 skills, 3 Powers); none has been tested in game yet.

## Infrastructure audit (Task 3)

| Source | Mode handling |
|--------|---------------|
| Ordinary rewards, All Star, Brain Leech, Endless Conveyor, Lead Paperweight | CardCreationOptions reads pools using the receiving player's UnlockState |
| Merchant, including restocks | MerchantInventory reads the player's pool; colorless pricing and purchase history recognize the hidden StS1 pool |
| Transforms | Ordinary hidden pool is paired; event cards have runtime Event rarity and transform into ordinary colorless cards |
| Colorless Potion, Cosmic Concoction, Toolbox, Orange Dough, Spectrum Shift, colorless-generating cards | GetUnlockedCards uses the recipient's context; CardFactory handles Hybrid versions |
| Massive Scroll | Rebuild options for the owner, because vanilla uses run-wide unlocks; prefer co-op-exclusive cards, falling back to allowed ordinary cards if none exist |
| Spirit Grafter → Metamorphosis | Direct grant patched for effective mode |
| Zen Weaver → Enlightenment | Both direct grants patched; Hybrid rolls each copy independently |
| Jewelry Box → Apotheosis; Distinguished Cape → Apparition | Ancient-event relic grants patched for their owner |

Event previews show the selected mode's version, or both versions for a Hybrid duplicate, without
consuming RNG. Ordinary pool composition does not include EventCardPool. Seven custom cards live
in a separate hidden event pool: Apotheosis, Apparition, Bite, Enlightenment, JAX, Metamorphosis,
Ritual Dagger. Runtime Event rarity and CanBeGeneratedInCombat=false enforce their boundary;
the original source rarity remains unchanged in the catalog. Bite, JAX and Ritual Dagger have
console access only until an event source is implemented separately; no new events were added.

Massive Scroll's empty-pool fallback is a builder adaptation: StS1 has no co-op-exclusive cards.
It preserves the effective card set rather than injecting foreign cards. Vanilla-only behavior
is unchanged. This fallback was proposed to the owner and used as the default pending feedback.

## In-game checks after the card implementations

- [ ] StS2-only without Souvenir: ordinary colorless offers and event grants stay vanilla.
- [ ] StS1-only: all five characters get the ordinary StS1 colorless pool (32 cards before unlock filtering).
- [ ] Regent and Necrobinder keep their character pools; Souvenir eligibility remains Ironclad/Silent/Defect.
- [ ] Hybrid: same-effect cards use the selected weaker version, duplicates occupy one slot and rewards can swap versions.
- [ ] One-holder co-op: the recipient's Souvenir controls rewards, shop stock, transforms and generated cards; losing it ends Hybrid.
- [ ] No event-only card appears in ordinary rewards, shop stock, transforms or random combat generation.
- [ ] Event-only cards transformed away yield ordinary colorless cards, never another event-only card.
- [ ] Spirit Grafter, Zen Weaver, Jewelry Box and Distinguished Cape grant the correct versions in all modes.
- [ ] Event previews match the mode; repeatedly opening previews does not change subsequent RNG outcomes.
- [ ] Event grants and Hybrid version rolls agree across co-op clients and survive save/resume.
- [ ] Massive Scroll respects its owner's mode and gives a nonempty choice in StS1-only co-op.
- [ ] Merchant pricing includes the native colorless premium; purchase history records custom colorless cards.
- [ ] `sts1card` resolves all four card groups, including `STS1_JAX` and all seven event-only cards.
- [ ] Card portraits, colorless frames and energy icons render correctly in EN/FR.

## Automated checkpoint — 2026-10-29

- Build: zero warnings, zero errors; DLL and PCK produced, not installed.
- xUnit: 82 pass; `NoUnportedCards` fails on the 25 remaining skeletons. The test remains enabled.
- Python: 35 pass (run separately because `make test` stops on the xUnit failure).
- Local assembly inspection: all 11 Harmony injection points matched StS2 v0.107.1.
  This checks instruction matching, not an in-game execution of the patches.
- Asset, localization coverage and placeholder checks pass. Attack text is hand-reviewed in EN/FR;
  generated text for skills and Powers still needs editing alongside Tasks 5–6.
- Ritual Dagger: a local probe against the game assembly confirms unchanged saved-property network
  IDs/bit width, 15 initial damage, +3/+5 growth, JSON and packet round-trips, and growth retained
  after downgrade. This does not exercise combat, actual run saves or a co-op session.

## Colorless attacks (Task 4)

| Card | Base → upgraded | Checks |
|------|-----------------|--------|
| Bite | 1 energy, 7 → 8 damage, heal 2 → 3 HP | Healing follows the attack even when damage is fully blocked or kills the last enemy; event-only |
| Dramatic Entrance | 0 energy, 8 → 12 damage to all enemies | Innate and Exhaust remain on both versions; no friendly fire in co-op |
| Flash of Steel | 0 energy, 3 → 6 damage, draw 1 | Damage precedes draw; test empty draw pile and full hand |
| Mind Blast | 2 → 1 energy, damage equal to current draw pile size | Innate; preview and actual damage reflect draw pile changes and normal attack modifiers |
| Ritual Dagger | 1 energy, 15 initial damage, Fatal growth +3 → +5 | Exhaust; event-only; upgrade changes growth, not existing damage |
| Swift Strike | 0 energy, 7 → 10 damage | Strike tag works with Strike-specific effects |

- [ ] Verify all six attacks and their upgrades in EN/FR, including portraits and keyword tooltips.
- [ ] Mind Blast: test zero/nonzero draw piles, Strength, Weak and Vulnerable; no stale preview.
- [ ] Ritual Dagger: a surviving enemy, blocked hit or Minion kill grants no permanent growth;
  a valid Fatal grants growth once, including the final enemy of a combat.
- [ ] Ritual Dagger: deck and combat card gain the same amount; a temporary combat upgrade uses +5
  without permanently upgrading the deck card. An unrelated second dagger does not gain damage.
- [ ] Ritual Dagger: test repeated plays, combat copies, downgrade, save/resume and one-holder co-op.
  Deck growth follows StS2's DeckVersion linkage, as for The Scythe; a detached combat copy must
  not mutate an unrelated deck card.
- [ ] Bite and Ritual Dagger remain excluded from ordinary pools and random generation.

## Colorless skills (Task 5)

"Base / +" means the card before and after upgrade.

- [ ] Apotheosis (2 / 1, Exhaust, event-only): upgrades every card in the combat (hand, piles, exhausted); not Innate, unlike StS2's.
- [ ] Apparition (1, Ethereal, Exhaust, event-only): 1 Intangible; the upgrade loses Ethereal.
- [ ] Bandage Up (0, Exhaust): heal 4 / 6 HP.
- [ ] Blind (0): 2 Weak to one enemy; upgraded, to ALL enemies (no target arrow).
- [ ] Trip (0): 2 Vulnerable to one enemy; upgraded, to ALL enemies (no target arrow).
- [ ] Chrysalis (2, Exhaust): 3 / 5 random Skills of your character shuffled into the draw pile, free this combat (in StS1-only, StS1 Skills).
- [ ] Metamorphosis (2, Exhaust, event-only): same with 3 / 5 Attacks.
- [ ] Deep Breath (0): shuffles the discard pile into the draw pile, then draws 1 / 2; with an empty discard pile it only draws.
- [ ] Discovery (1, Exhaust; upgraded: no Exhaust): choose 1 of 3 character cards, free this turn; the choice can't be skipped.
- [ ] Enlightenment (0, no Exhaust, event-only): hand cards cost at most 1 this turn / this combat; cards already cheaper keep their cost.
- [ ] Finesse (0): 2 / 4 Block, draw 1. French title "Finesse".
- [ ] Forethought (0): base, choose exactly 1 hand card; upgraded, any number (0 allowed). They go under the draw pile and cost 0 until played; an X-cost card stays X. Empty hand: nothing happens. Prompts in EN/FR.
- [ ] Good Instincts (0): 6 / 9 Block. French title "Instinct" (the game's own translation).
- [ ] Jack of All Trades (0, Exhaust): 1 / 2 random colorless cards to hand (repeats possible; in StS1-only, StS1 colorless cards; never an event-only card).
- [ ] J.A.X. (0, event-only, console only): lose 3 HP (ignores Block), gain 2 / 3 Strength.
- [ ] Madness (1 / 0, Exhaust): a random hand card that currently costs energy costs 0 this combat; X-cost cards are never picked; nothing happens if no card qualifies.
- [ ] Panacea (0, Exhaust): 1 / 2 Artifact.
- [ ] Purity (0, Exhaust, no Retain): exhaust up to 3 / 5 hand cards; picking none is allowed. French title "Épuration".
- [ ] Secret Technique / Secret Weapon (0, Exhaust; upgraded: no Exhaust): unplayable without a Skill / an Attack in the draw pile; choose one to put in hand.
- [ ] Transmutation (X, Exhaust): X random colorless cards, upgraded when this is, free this turn; X = 0 does nothing; Chemical X adds 2.
- [ ] Violence (0, Exhaust): 3 / 4 random Attacks from the draw pile to hand; fewer if the pile holds fewer; a full hand sends the rest to the discard pile as vanilla.
- [ ] Generated cards (Chrysalis, Discovery, Jack, Transmutation) follow the effective mode and stay in sync in co-op.

## Colorless Powers (Task 6)

- [ ] Magnetism (2 / 1): from the next turn on, a random colorless card arrives in hand before the draw (Creative AI icon); two copies give 2 cards per turn.
- [ ] Panache (0): the Panache card itself doesn't count; every 5th card played in a turn deals 10 / 14 damage to ALL enemies (Strength doesn't add). The icon shows the cards left and resets to 5 at end of turn. A second Panache adds its damage to the same countdown (one icon, damage 20) and counts as a card played.
- [ ] Sadistic Nature (0): each debuff you apply to an enemy (Weak, Vulnerable, Poison…) deals 5 / 7 damage to it; Artifact blocking the debuff means no damage. It shows vanilla Sleight of Flesh's buff ("Tour de chair" in French).
- [ ] Co-op: each player's Powers trigger only on their own turn start and their own cards; Sadistic Nature doesn't fire on a teammate's debuffs.

# Step 3c — Colorless cards

Target: StS2 v0.107.1. All in-game checks below remain pending. Infrastructure is implemented,
but 25 custom cards are still skeletons; do not install this intermediate build for play.
The six attacks are implemented, with their in-game checks still pending.

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

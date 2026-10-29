# Step 3c — Colorless cards

Target: StS2 v0.107.1. All in-game checks below remain pending. Infrastructure is implemented,
but the 31 custom cards are still skeletons; do not install this intermediate build for play.

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
- xUnit: 82 pass; `NoUnportedCards` fails on the 31 planned skeletons. The test remains enabled.
- Python: 35 pass (run separately because `make test` stops on the xUnit failure).
- Local assembly inspection: all 11 Harmony injection points matched StS2 v0.107.1.
  This checks instruction matching, not an in-game execution of the patches.
- Asset, localization coverage and placeholder checks pass. Generated text still requires hand editing
  alongside each implementation in Tasks 4–6.

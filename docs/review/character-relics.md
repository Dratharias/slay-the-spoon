# Character relics — StS1 / StS2 inventory

Audit: 2026-10-30. Inventory and proposed scope only; no relics are enabled by this document.

Source: the installed StS1 jar, its `RelicLibrary` registrations and relic implementations, local English/French localization, and the local StS2 decompile (`MegaCrit.Sts2.Core.Models.Relics` and `.RelicPools`, target v0.107.1). Class/source hashes and exact IDs are recorded in [the inventory](../../data/duplicates/character-relics.json). Decompiled code stays outside the repository.

All 40 registered character relics are accounted for. Ironclad/Silent/Defect: 31 total, 20 existing native models and 11 missing (3 ordinary rares, 8 boss relics). Watcher: 9 total, 1 native shared relic and 8 missing. Native presence is not a claim of exact timing or multiplayer equivalence.

## Complete inventory

Native models are reuse candidates, not requests to duplicate them. `planned-port-inactive` means implementation is approved but acquisition remains disabled. `native-special` retains its existing native source. Watcher entries stay with step 7.

### Ironclad

| StS1 name (English / French) | StS1 tier | StS2 model / rarity / pool | Disposition |
|---|---|---|---|
| Black Blood / Sang noir | BOSS | BlackBlood / Starter / EventRelicPool | native-special |
| Brimstone / Soufre | SHOP | Brimstone / Shop / IroncladRelicPool | native |
| Burning Blood / Sang ardent | STARTER | BurningBlood / Starter / IroncladRelicPool | native |
| Champion Belt / Ceinture de champion | RARE | Absent | port-candidate |
| Charon's Ashes / Cendres de charon | RARE | CharonsAshes / Rare / IroncladRelicPool | native |
| Magic Flower / Fleur magique | RARE | Absent | port-candidate |
| Mark of Pain / Marque de douleur | BOSS | Absent | planned-port-inactive |
| Paper Phrog / Grenouye en papier | UNCOMMON | PaperPhrog / Uncommon / IroncladRelicPool | native |
| Red Skull / Crâne rouge | COMMON | RedSkull / Common / IroncladRelicPool | native |
| Runic Cube / Cube runique | BOSS | Absent | planned-port-inactive |
| Self-Forming Clay / Argile autoformante | UNCOMMON | SelfFormingClay / Uncommon / IroncladRelicPool | native |

### Silent

| StS1 name (English / French) | StS1 tier | StS2 model / rarity / pool | Disposition |
|---|---|---|---|
| Hovering Kite / Cerf-volant | BOSS | Absent | planned-port-inactive |
| Ninja Scroll / Parchemin de ninja | UNCOMMON | NinjaScroll / Shop / SilentRelicPool | native |
| Paper Krane / Grû en papier | UNCOMMON | PaperKrane / Rare / SilentRelicPool | native |
| Ring of the Serpent / Anneau du cobra | BOSS | Absent | planned-port-inactive |
| Ring of the Snake / Anneau du serpent | STARTER | RingOfTheSnake / Starter / SilentRelicPool | native |
| Snecko Skull / Crâne de geckobra | COMMON | SneckoSkull / Common / SilentRelicPool | native |
| The Specimen / Le spécimen | RARE | Absent | port-candidate |
| Tingsha / Tingsha | RARE | Tingsha / Uncommon / SilentRelicPool | native |
| Tough Bandages / Bandages rugueux | RARE | ToughBandages / Rare / SilentRelicPool | native |
| Twisted Funnel / Entonnoir torsadé | SHOP | TwistedFunnel / Uncommon / SilentRelicPool | native |
| Wrist Blade / Poignard | BOSS | Absent | planned-port-inactive |

### Defect

| StS1 name (English / French) | StS1 tier | StS2 model / rarity / pool | Disposition |
|---|---|---|---|
| Cracked Core / Noyau fissuré | STARTER | CrackedCore / Starter / DefectRelicPool | native |
| Data Disk / Disque dur | COMMON | DataDisk / Common / DefectRelicPool | native |
| Emotion Chip / Puce émotionnelle | RARE | EmotionChip / Rare / DefectRelicPool | native |
| Frozen Core / Noyau gelé | BOSS | Absent | planned-port-inactive |
| Gold-Plated Cables / Cable plaqué-or | UNCOMMON | GoldPlatedCables / Uncommon / DefectRelicPool | native |
| Inserter / Bras robotisé | BOSS | Absent | planned-port-inactive |
| Nuclear Battery / Batterie nucléaire | BOSS | Absent | planned-port-inactive |
| Runic Capacitor / Condensateur runique | SHOP | RunicCapacitor / Shop / DefectRelicPool | native |
| Symbiotic Virus / Virus symbiotique | UNCOMMON | SymbioticVirus / Uncommon / DefectRelicPool | native |

### Watcher

| StS1 name (English / French) | StS1 tier | StS2 model / rarity / pool | Disposition |
|---|---|---|---|
| Cloak Clasp / Attache de Cape | RARE | CloakClasp / Rare / SharedRelicPool | native-shared |
| Damaru / Damaru | COMMON | Absent | watcher-work |
| Golden Eye / Œil doré | RARE | Absent | watcher-work |
| Holy Water / Eau bénite | BOSS | Absent | watcher-work |
| Melange / Mélange | SHOP | Absent | watcher-work |
| Pure Water / Eau pure | STARTER | Absent | watcher-work |
| Violet Lotus / Lotus violet | BOSS | Absent | watcher-work |
| Teardrop Locket / Médaillon larme | UNCOMMON | Absent | watcher-work |
| Duality / Dualité | UNCOMMON | Absent | watcher-work |

## Ordinary port candidates

These three do not require restoring an event or inventing a boss-relic reward system. All are Rare in StS1.

| Character | Relic | Behavior to port | Implementation questions to resolve in code |
|---|---|---|---|
| Ironclad | Champion Belt (`ChampionsBelt`) | Applying Vulnerable also applies 1 Weak. | Inspect StS1's external trigger in the Vulnerable application path before implementing: Artifact, existing Vulnerable, and attribution must not produce extra triggers. Restrict to the holder's applications in co-op. |
| Ironclad | Magic Flower | Multiply combat healing by 1.5, rounded to the nearest integer (positive halves up). | Use the actual heal modification hook; establish whether victory healing still occurs in the combat phase. Avoid scaling max-HP changes or another player's healing. |
| Silent | The Specimen | On an enemy's death, transfer its remaining Poison to a random surviving enemy. | Capture Poison before removal; use synchronized target RNG; handle simultaneous deaths, no targets, Artifact and Snecko Skull. Shared Poison with multiple holders needs an explicit deterministic rule. |

Proposed first implementation slice: these three, using the existing character relic pools and native rarity system. Add art, EN/FR text, model registration, eligibility filtering and only the necessary pure-logic tests. Animation and in-game validation remain deep backlog at the owner's request; do not claim them complete.

## Existing models and differences

- `PaperFrog` is `PaperPhrog`; `PaperCrane` is `PaperKrane`; `SnakeRing` is `RingOfTheSnake`. These are name changes, not missing relics.
- Black Blood already heals 12 after victory. It is an event-pool Starter in StS2, obtained through `TouchOfOrobas` replacing Burning Blood, rather than a boss chest relic. Do not make a second Black Blood.
- Cloak Clasp already grants 1 Block per card remaining in hand at turn end. It is in `SharedRelicPool`, not a Watcher-exclusive pool. Step 7's nine-relic plan must account for this before creating a duplicate.
- Paper Krane changes Uncommon → Rare; Ninja Scroll changes Uncommon → Shop; Tingsha changes Rare → Uncommon; Twisted Funnel changes Shop → Uncommon. Preserve native rarity in the proposed reuse approach.
- Brimstone (2 personal Strength / 1 enemy Strength), Burning Blood (6 heal), Charon's Ashes (3 damage), Red Skull (3 Strength at <=50% HP), Self-Forming Clay (3 next-turn Block), Ring of the Snake (2 opening cards), Snecko Skull (+1 Poison), Tingsha (3 damage), Tough Bandages (3 Block), Twisted Funnel (4 Poison), Data Disk (1 Focus), Cracked Core (1 Lightning), Runic Capacitor (3 slots), and Symbiotic Virus (1 Dark) retain the main numeric effect. Reuse is a proposal, not an assertion that all engine hooks are identical.
- Native start-of-combat effects often run at first-turn hooks; Ninja Scroll uses native Shivs. Emotion Chip uses StS2 damage history. Gold-Plated Cables uses StS2 orb queue ordering. Preserve the native engine semantics, consistent with the existing Defect card port, unless a later decision requests exact StS1 behavior.
- Ring of the Drake is not Ring of the Serpent: Drake draws 2 extra cards for the first 3 turns; Serpent draws 1 extra every turn. Infused Core is not Frozen Core: it channels 3 Lightning initially and strengthens Lightning, rather than making Frost in empty slots.
- StS2-only character relics (Demon Tongue, Ruined Helmet, Helical Dart, Power Cell, Metronome) are not replacements for the missing StS1 effects. The proposed scope adds missing relics; it does not remove these native relics.

## Approved inactive ports — boss and starter upgrades

> **Update (0.12.37, owner decision 2026-11-05):** the source is now decided. With the Custom Run modifier "StS1 Boss
> Relics" (`SpoonBossRelics`), the act 2 and act 3 Ancients offer a chest of 3 boss relics drawn from the 11 Darv
> natives, the 8 shared ports and the taker's character boss relics above (Holy Water and Violet Lotus for the Watcher);
> see `src/Relics/BossChest.cs` and `docs/testing/boss-relics-checklist.md`. Without that modifier they stay out of every
> acquisition path, as below. The `planned-port-inactive` rows in the tables above keep their original status name.

**Owner decision (2026-10-30): port the eight missing boss relics, but leave them unused for now.** Implement their effects and assets without adding them to rewards, shops, events, Ancients, starting loadouts or random generation. Import approval does not approve an acquisition source. StS2's `RelicRarity` has no Boss member. There is no direct boss-tier registration equivalent. The missing boss relics are:

| Character | Relic | StS1 effect | Acquisition issue |
|---|---|---|---|
| Ironclad | Mark of Pain | +1 energy each turn; shuffle 2 Wounds into the draw pile at combat start. | Decide a source for a boss-strength tradeoff. |
| Ironclad | Runic Cube | Draw 1 card each time HP is lost in combat. | Decide a source; inspect out-of-turn draw semantics. |
| Silent | Hovering Kite | First manual discard each turn gives 1 energy. | Decide a source; end-turn hand cleanup must not trigger it. |
| Silent | Ring of the Serpent | Replace Ring of the Snake; draw 1 extra card each turn. | Starter replacement, mutually exclusive with native Orobas upgrade. |
| Silent | Wrist Blade | +4 attack damage when the card costs 0 this turn, including free-to-play non-X cards. | Decide a source; preserve the X-cost exclusion and modified costs. |
| Defect | Frozen Core | Replace Cracked Core; channel Frost at turn end if an orb slot is empty. | Starter replacement, mutually exclusive with Infused Core. |
| Defect | Inserter | Gain 1 orb slot every 2 turns. | Decide a source; inspect counter persistence between combats. |
| Defect | Nuclear Battery | Channel 1 Plasma at combat start. | Decide a source; native Plasma mechanics apply. |

The owner will decide how to make these relics available later, possibly through a Custom Run option. That option is an idea, not an approved feature to implement now. Acquisition decisions do not block the inactive ports. Black Blood's existing Orobas route should be preserved. Watcher's Holy Water and Violet Lotus have the same boss-source question; Holy Water is also a starter replacement. They stay in step 7, with this dependency recorded.

## Post-mission architecture decisions — event relics

None of the 40 character registrations is a StS1 SPECIAL-tier relic: event relics are a separate shared-content expansion. They are recorded here because the owner explicitly raised restoring events. The following is a dependency shortlist, not an approved port list or a full shared-relic audit.

The dependencies below were verified as relic-class references in the installed StS1 event bytecode; a reference can be a prerequisite, reward or exchange and does not by itself establish the entire event flow.

| StS1 event class(es) | Relics involved | Decision to retain |
|---|---|---|
| `GoldenIdolEvent`, `ForgottenAltar`, `MoaiHead` | Golden Idol; Bloody Idol at the altar | Restore the acquisition/exchange chain together, or define an alternative source. |
| `CursedTome` | Enchiridion, Necronomicon, Nilry's Codex | Whether to restore the event and its costs; Necronomicon also requires its curse interaction. |
| `FaceTrader` | Cultist Headpiece, Face of Cleric, Gremlin Visage, N'loth's Hungry Face, Ssserpent Head | Whether to restore the full positive/negative outcome set. |
| `MaskedBandits`, `TombRedMask` | Red Mask | Whether to restore the fight, tomb, and their linked ownership checks. |
| `Mushrooms` | Odd Mushroom | Whether to restore its encounter and reward branch. |
| `AccursedBlacksmith` | Warped Tongs | Whether to restore the event and its curse tradeoff. |
| `Nloth` | N'loth's Gift | Whether to restore the relic-for-relic exchange. |
| `MindBloom` | Mark of the Bloom | Whether to restore the upgrade/healing lock tradeoff and other event branches. |
| `Bonfire` | Spirit Poop | Whether to restore the card-sacrifice event. |
| `DrugDealer` (Augmenter) | Mutagenic Strength | Already ported in this repository; do not create a second relic/event. |

Before selecting any event, audit StS2's native equivalent and the existing mod event injection, then decide solo/co-op costs, participant eligibility and reward ownership. Do not add special relics to random rewards merely to make them obtainable. Neow's Lament, Circlet and Red Circlet are also SPECIAL in StS1 but are not ordinary event-port targets (start blessing / fallback / nonstandard content); no inclusion is implied.

## Proposed eligibility and follow-up order

1. Implement the three ordinary rares first. Scope activation to Custom Runs with StS1-only or Spire Souvenir selected, as requested in the backlog. Check per-player character pools; Regent and Necrobinder receive no character-specific StS1 additions.
2. Preserve native relics and their rarity. The card version-switch rule is not automatically a relic replacement rule. A naturally acquired Spire Souvenir in Standard does not automatically authorize new relic availability; decide any broader eligibility explicitly.
3. Port the eight missing boss relics as inactive content. Keep them out of all acquisition and random-generation paths; defer their activation to the owner’s later design. Resolve event-relic choices separately before implementing those events. The same inactive-acquisition principle applies to Holy Water and Violet Lotus when step 7 ports them.
4. Keep Watcher's eight missing relic implementations with the character work; reuse native Cloak Clasp. Animation and in-game validation remain deep backlog, rather than prerequisites for preparing the port.

No runtime files changed in this mission. This inventory does not assert that all shared StS1 relics have been audited or request their implementation.

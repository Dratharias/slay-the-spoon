# Step 3d — StS1 card events

Target: StS2 v0.107.1. Vampires, Augmenter and The Nest are implemented with the Mutagenic Strength
relic; nothing has been tested in game yet. Plan: `docs/superpowers/plans/2026-10-29-step-3d-sts1-events.md`.

## Testing tips

- `sts1event STS1_VAMPIRES`, `sts1event STS1_AUGMENTER`, `sts1event STS1_NEST` open an event directly,
  skipping the eligibility rule (the card lock still applies).
- An injected event logs `[SlaySpoon] StS1 event: <id> replaces <vanilla id>` in `godot.log`.

## Eligibility

- [ ] Standard run, no relic: act 2 events stay vanilla (no `[SlaySpoon] StS1 event` log line), and the
      same seed gives the same events with and without the mod installed.
- [ ] "Slay the Spire 1 Cards" run: all three events can appear in act 2, never in acts 1 or 3.
- [ ] Solo with the Spire Souvenir: all three can appear.
- [ ] Co-op, one player with the Souvenir: The Nest and Augmenter can appear, Vampires never does.
- [ ] Co-op, every player with the Souvenir: Vampires can appear too.
- [ ] Each event appears at most once per run; a room already replaced by Lantern Key stays Lantern Key's.
- [ ] Roughly one act 2 event room in three becomes a StS1 event while candidates remain.

## Events

- [ ] Portraits (StS1 art) fill the frame cleanly in all three events; title and text render in EN/FR.
- [ ] Vampires: Accept removes every basic Strike, adds 5 Bites, loses 30% max HP (rounded up).
- [ ] Vampires: the Blood Vial option appears only with Blood Vial, removes it, costs no max HP.
- [ ] Augmenter: J.A.X. adds the card; Transform asks for 2 cards and transforms them; it is locked
      with fewer than 2 transformable cards.
- [ ] Augmenter: Mutagenic Strength gives the relic (Symbiotic Virus icon), with tooltip in EN/FR.
- [ ] Mutagenic Strength: +3 Strength at each combat start, removed at the end of the first turn;
      the power icon (Flex) and its tooltip read correctly.
- [ ] The Nest: Smash and Grab gives 99 gold; Stay in Line costs 6 HP and adds Ritual Dagger.
- [ ] Dying in The Nest shows the event's death line on the game-over screen.
- [ ] Co-op, a player without the Souvenir: the card option shows "Locked" with the requirement;
      the other options work. The player with the relic takes the card normally.
- [ ] Save and quit in the middle of each event, resume: same event, same options.
- [ ] Co-op: both clients see the same event in the same room, choices stay in sync.

## Automated checkpoint — 2026-10-29

- Build: zero warnings, zero errors; DLL and PCK produced, not installed.
- xUnit: 100 pass, including the eligibility rule and the asset test (portrait and EN/FR text for every
  key each event uses).
- Python: 36 pass, including the event art export.

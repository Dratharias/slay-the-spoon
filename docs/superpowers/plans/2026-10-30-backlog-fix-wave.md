# Backlog fix wave (0.10.1)

**Goal:** fix the real gameplay bugs left in `docs/backlog.md` after steps 6 and 7, apply the owner's decisions of
2026-10-30, and bring Watcher card behavior back to StS1. Code-quality and weak-test minors stay in the backlog.

**Spec:** `docs/superpowers/specs/2026-10-28-slay-the-spoon-design.md`, amended by the owner decisions recorded in
`docs/backlog.md` on 2026-10-30:

- Fidelity rule: StS1 character cards and StS1 content follow StS1 behavior wherever StS2's engine allows; StS2
  conventions apply to colorless cards and cross-character cards.
- At A10 with Act 4 appended, only the second act 3 boss gives boss rewards.
- At Orobas, when Archaic Tooth's conditions fail, Holy Water or Violet Lotus (one at random, event RNG) takes its slot,
  for the Watcher only.

## Global Constraints

- Target StS2 v0.107.1; mod id `SlaySpoon`; docs and comments in English; every new or changed text in `eng` and `fra`.
- Decompiled code (Java or C#) never enters the repo, not even as comments. StS1 classes: `~/Projects/sts-decomp/sts1/`;
  StS2: `~/Projects/sts-decomp/sts2/`. Check StS1's actual behavior there before changing a card.
- The mod never writes the regular (unmodded) save. Standard and Daily stay vanilla. Seeds stay vanilla: new random
  choices use a synced RNG that vanilla does not draw from for the same purpose, or an existing one the vanilla path
  already consumes at that point (state which).
- Never rename a released class or ModelDb id; saved property names stay as they are.
- Pure logic goes in `src/Core/` with xUnit tests. `nice -n 19 make build` (0 warnings / 0 errors) and
  `nice -n 19 make test` green after every task.
- Branch `fix-wave-backlog` (worktree `../slay-the-spoon-fixes`), cut from `main`. Commit per task, explicit paths,
  trailer `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`. No push, no Workshop upload, no `make up`.
- Every fixed backlog line is removed from `docs/backlog.md` in the same task; anything found and not fixed is added there.
  Every in-game behavior changed gets a line in `docs/testing/fix-wave-checklist.md` (created in Task 1).

### Task 1: Act 4 keys bugs

Backlog section "Act 4 keys (step 6 WP3), minors left after review".

- Co-op Sapphire swap race: carry the swap plan (relic, chest, player) in the net message so a peer whose reward
  animation has not handed out the relic yet applies the same swap instead of refusing.
- `Act4Keys.Decode`: bound the saved count before `count * IntsPerNode`; a corrupt count decodes to "no stored node".
- `SpoonConfigFile`: write atomically (temp file + rename in the same directory); log an unreadable cfg once per
  session, not per query; keep non-scalar values on rewrite. Never touch the regular save.
- Golden Compass: after a mid-act map regeneration, re-pick the act's Emerald node from the new map with the same rule
  and RNG stream the original pick uses (state the stream); the marker follows.
- Text: Regenerate's text uses the right var; Metallicize's text no longer speaks to the player when an enemy holds it
  (EN and FR; match vanilla wording where vanilla has a third-person variant).
- Leave the Sapphire button position, the Ruby icon getter, the heal-revert history and `StandardCharacterSelectPatch`
  as backlog lines.

### Task 2: The Ending bugs and the A10 boss reward

Backlog sections "Act 4 The Ending (step 6 WP4-6)".

- A10 with Act 4 appended: only the second act 3 boss gives boss rewards; the first behaves as vanilla's first boss of
  a two-boss last act. Pure rule in `src/Core/` with tests; update the design-call line.
- Smash: its Block equals the pre-block damage of its hit (StS1), not the damage dealt.
- Debilitate's dead-player skip stays a backlog line (co-op only, StS1 has no reference behavior).
- French loss line: "pris en tenaille par le Bouclier et la Lance de la Tour" (check the monsters' French names).
- WinTime repair: cover the save-on-reward-screen sequence (act 3 boss counted final, save, resume: reset `WinTime`
  when the load postfix appends act 4). Make `EnsureEndingAct` idempotent and say how you checked it.
- Ending stinger: stop it when the player leaves to the menu; wrap the `EnterNextAct` prefix in try/catch like the
  postfix. Leave the volume-slider gap as a backlog line unless the game's music bus volume is readable from Godot
  (check, then state).

- StS1 ending visuals (owner, 2026-10-30): no ending cutscene (StS1 has none for the Necrobinder or the Regent) and no
  end credits; use the
  rest of StS1's Ending content in place of Glory's where StS1 has it (combat/room backgrounds, music if the jar holds
  it in a format Godot plays). Inventory the jar first; import through the existing `make sts1-act4-data` manifest
  (SHA-256, tests); keep Glory's asset for anything StS1 lacks (map, rest site if none). State what you used.

### Task 3: Watcher cards back to StS1

Backlog sections "The Watcher (step 7), design calls made without the owner" and "minors left after review".
For each item, read the StS1 class, then match its behavior unless StS2's engine prevents it (then say why in the
backlog line, which stays).

- Deva Form applies its Magic var, not 1.
- Block Return: Block on the killing hit too; pet (Osty) attacks follow StS1 (StS1 has no pets: decide by analogy,
  state it).
- Master Reality: upgrade only the cards StS1 upgrades (cards created in combat by the holder, minus Status and Curse),
  check Foreign Influence's pick against StS1.
- Establishment: discount only cards kept by Retain; a retained card free this turn still gets the combat discount.
- Wave of the Hand: lasts until the end of the round, as StS1.
- Omniscience: its copies are removed (purged), not exhausted.
- Halt: one Block gain, as StS1.
- Battle Hymn: StS1's dead check.
- Fear No Evil, Signature Move, Ragnarok, Brilliance: compare with StS1 and fix any behavior gap.
- Lesson Learned and Spirit Shield preview gaps stay in the backlog unless trivial.
- Text: Perseverance's French duplicate "Retenue." line; Rushdown's English upgrade text.
- Foreign Influence is a cross-character card generator: keep its current pools (StS2 rule) and say so in the backlog.
- Vault in co-op stays as is (design call, co-op only).

### Task 4: Orobas offers Holy Water or Violet Lotus

- At Orobas, when Archaic Tooth's `SetupForPlayer` fails for a Watcher player, add one of Holy Water / Violet Lotus,
  picked at random with the event's RNG, as that option-pool slot. Other characters keep vanilla's list (locked
  option when empty). Both relics stay out of every other source.
- Holy Water keeps its pickup effect (replaces Pure Water). If she no longer holds Pure Water, Holy Water still gives
  its effect as StS1 does (check StS1's `HolyWater`).
- Co-op: each player's Orobas options are built per owner, so peers agree.
- EN/FR text if any new option text is needed; checklist lines.

### Task 5: Wrap-up

- Version 0.10.1 in `mod/SlaySpoon.json` and `src/ModEntry.cs`.
- `docs/backlog.md`: fixed lines gone, remaining lines accurate; `CLAUDE.md` status gets a short "Fix wave (2026-10-30)"
  section with test counts; checklist header.

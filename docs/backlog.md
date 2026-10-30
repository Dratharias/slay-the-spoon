# Backlog

Known gaps deferred from step 1's final review. Fixed when the author feels like it.

- The Watcher (StS1 character) as a playable character when a StS1 card set mode is ticked in a
  Custom Run (owner request, 2026-10-29).
- Act 4 with the StS1 keys (Ruby, Emerald, Sapphire): permanently unlock by first reaching Act 3
  in a Standard run at Ascension 10, whether that run is won or lost. Once unlocked, Act 4 is
  available at every ascension level, including 0. A Custom Run option bypasses the prerequisite
  (owner clarification, 2026-10-29). Daily runs remain unchanged.
  Source import started on `step-6-act4`; see `docs/superpowers/plans/2026-10-29-step-6-act4.md`.
  Not playable yet.
- StS1 character-specific relics (Ironclad, Silent, Defect, Watcher pools) that StS2 dropped, offered when a StS1 card
  set mode is active (owner request, 2026-10-29). Not planned yet.
- Act 4 keys (step 6 WP3), minors left after review:
  - Golden Compass regenerates the map mid-act: the stored Emerald node may no longer be an elite, so that act loses its
    Emerald chance and the marker can sit on another point type (the grant already requires an elite room).
  - The Sapphire button position is hard-coded (650, 770); anchor it to the chest's relic row.
  - The Ruby icon comes from a postfix on the private `RestSiteOption.IconPath` getter, which the JIT may inline.
  - A co-op Sapphire swap accepted the instant the button appears can reach a peer before its reward animation has
    handed out the relic; that peer refuses while the others apply it. Fix by carrying the plan in the net message.
  - The Sapphire heal revert sets HP directly: no HP-lost entry in run history, no sound.
  - `Act4Keys.Decode`: bound the saved count before `count * IntsPerNode`.
  - `StandardCharacterSelectPatch` sets `Pending` for any game mode; `Act4UnlockPatch.Record` logs failures at info.
  - `SpoonConfigFile` writes are not atomic, an unreadable cfg logs on every query, non-scalar values are dropped on rewrite.
  - Regenerate's text uses `{Amount}`; Metallicize's text speaks to the player when an enemy has it.

# Fix wave final review — 0.10.1

Reviewed the complete wave against the approved plan, including the earlier keys commits and the resumed
Ending changes. The owner authorized incremental merges into main; no push, installation or Steam upload.

- Keys: bounded decode arithmetic; atomic same-directory config replacement; co-op swap payload carries its
  original chest/player/relic plan; Emerald regeneration uses the named mod RNG. The 15-second pickup timeout
  remains documented, as do UI/history and weak-test minors.
- Ending: first/second boss routing, stale WinTime load ordering and idempotent act append reviewed against
  RunManager/RewardsSet; Smash captures damage before Block/HP-loss substitutions. Imported resource SHA-256
  manifest covers the two music tracks added to the 23 existing resources. Audio stops on menu entry and reads
  the game's squared volume controls. Runtime rendering/audio remains an in-game checklist item.
- Watcher: all requested cards/actions/powers were compared locally. Final review moved Wave of the Hand's
  expiration to the enemy side's late end hook, preserving Block effects in ordinary end-of-turn hooks.
  Engine-only Signature Move dialogue customization remains explicitly deferred; its playability is correct.
- Orobas: fallback is scoped to actual per-owner option generation, not catalog inspection; no RNG is drawn by
  the catalog, no duplicate Tooth setup, and other characters keep vanilla results. Both relics remain excluded
  from normal pools/shops. Existing EN/FR relic text supplies the event option text.
- No released model ids or saved property names were renamed. New persistence code does not write regular saves.
  Foreign Influence pools and co-op Vault preserve owner decisions. Version remains 0.10.1.

Validation: build 0 warnings/errors, 408 xUnit + 74 pytest; Workshop staging for item 3829630403 succeeds.
The packaged DLL/PCK/JSON match the build outputs; 625 PCK entries include the new Ending imported payloads and
EN/FR tables. Packing-project overrides are absent. The uncommitted character-relic tranche is not packaged.
The main working copy's untracked relic files remain untouched; publish from the clean fix-wave worktree.

No new blocker found in this source/build/package review. In-game solo/co-op, save/reload, UI and audio checks
remain pending in docs/testing/fix-wave-checklist.md, consistent with the owner's deferred play-testing priority.

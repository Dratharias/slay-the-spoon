# Watcher fidelity review

Compared with local StS1 cards, actions and powers; no decompiled source is included here.

| Card or power | Result |
| --- | --- |
| Deva Form | Applies Magic; StS1 DevaPower's +1 next-gain on stacking is preserved. |
| Block Return | Uses AfterDamageGiven, dispatched on lethal hits before power removal. Pet attacks block their owning player by analogy with StS1's player-only recipient; co-op ordinary attacks block their attacker. |
| Master Reality | Holder-created temp cards only, excluding Status/Curse and Foreign Influence's direct insertion path. |
| Establishment | ShouldRetainThisTurn excludes no-flush relics; combat-only local costs prevent a temporary free card from losing its discount. Private cost access is isolated in one accessor. |
| Wave of the Hand | Expires after the enemy side, preserving enemy-turn Block triggers. |
| Omniscience | Original exhausts; marked copies purge, including failed autoplay. Does not use IsDupe, whose native duplication restrictions differ from StS1. |
| Halt | Two separately modified Block components, one gain. Magic is a BlockVar so its preview includes Dexterity/Frail too. |
| Battle Hymn | Checks the engine's ending/ended combat state plus surviving enemies. StS2 removes escaped creatures rather than exposing StS1 isEscaping. |
| Fear No Evil | Already checks attack intent before damage and enters Calm afterward, including kills; the old backlog description was stale. |
| Signature Move | Gameplay and glow already match. Engine only supplies a generic unplayable message; dialogue limitation remains in backlog. |
| Ragnarok | AttackCommand reselects a surviving random target and recalculates damage each hit; gameplay already matches. |
| Brilliance | CalculationBase + tracked combat Mantra already matches; consumed Mantra remains counted, PlayerCombatState lifetime resets it. |

Perseverance's duplicate French Retain line is removed. Rushdown's English runtime text was already correct:
StS1's unused UPGRADE_DESCRIPTION says DEPRECATED, but Rushdown.upgrade only changes cost. No deprecated
branch should be added. Both languages retain the same description on upgrade.

Foreign Influence's generation pools deliberately remain the approved StS2 cross-character rule.
Lesson Learned and Spirit Shield preview limitations and co-op Vault remain deferred.

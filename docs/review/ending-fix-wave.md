# Ending fix wave review

Local StS1 jar inventory: Ending atlas regions bg, campfire and event; STS_Act4_BGM_v2.ogg,
STS_Boss4_v6.ogg and STS_EndingStinger_v1.ogg. All 25 imported resources have SHA-256 entries.
Combat and rest use the first two regions; event is staged but this route has no ordinary event room.
The shared StS2 merchant room and Glory map remain. No cutscene or credits are added.

Audio uses Godot Ogg players with the same squared Master/Music values as NAudioManager's FMOD setters.
Menu entry invalidates pending stinger playback. In-game visual/audio validation remains in the checklist.

InitializeShared restores WinTime before InitializeSavedRun; the latter's postfix appends and clears it.
EnsureEndingAct's three-act gate returns before mutation on subsequent calls (four acts), preserving genuine
Heart victory times. Existing routing tests cover the count gate; new tests cover the stale-time decision.

Smash captures modified damage in BeforeDamageReceived, before Block and HP-loss substitutions. It does not
reconstruct damage from HP loss, which would undercount Buffer/pet absorption. A10 remains fixed at 99.
The first A10 boss reward guard returns before reward RNG draws; the second boss follows vanilla rewards.

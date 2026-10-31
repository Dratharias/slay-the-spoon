# Orobas fallback review

Vanilla OptionPool3 runs ArchaicTooth.SetupForPlayer exactly once. Its result determines whether the
Watcher gets one fallback slot. Touch of Orobas remains in the pool if eligible; the generic locked
placeholder is removed only when replaced. Other characters follow the unchanged vanilla getter.

The extra coin flip uses EventModel.Rng only during GenerateInitialOptions. That stream is initialized
from run seed, event id and owner player slot, so each player's independent options agree on peers and
reconstruction. AllPossibleOptions includes both relics for catalog/preloading, with no RNG draw.

The inherited RelicOption callback obtains the relic and finishes Orobas normally. EventOption.FromRelic
falls back to the existing localized relic title and DynamicEventDescription (EN/FR), so no option text is needed.
IsAllowed remains false and IsAllowedInShops false; neither relic enters another acquisition pool.

StS1 HolyWater.atBattleStartPreDraw gives three Miracles without testing Pure Water. Its canSpawn check
is an acquisition gate only. The existing port already removes Pure Water if present on pickup and always
provides its combat effect; both behaviors are preserved for this explicitly approved Orobas source.

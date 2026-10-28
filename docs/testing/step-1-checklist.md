# Step 1 — in-game checklist

Run `make up` first. For local co-op on one PC, `make mp` opens a host window (Custom Run lobby)
and a client window that joins it (Mega Crit's `--fastmp` dev flag, ENet on 127.0.0.1). The final
co-op check should also be done once over Steam with a friend.

## Custom Run screen (task 4)

- [x] Solo Custom Run: "Monster HP" and "Monster Damage" rows are listed at the bottom, with − / + buttons.
- [x] − / + step by 25 % between 25 % and 500 %, tick the row, and the row text shows the value. Nothing happens past the ends.
- [x] Starting the run: the log line "Embarking on a CUSTOM ... Modifiers:" lists MODIFIER.SPOON_MONSTER_HP… / SPOON_MONSTER_DAMAGE….
- [ ] Co-op (`make mp`): ticking a row or changing a value on the host shows up on the client.
- [ ] Co-op: the client's − / + buttons are greyed out and do nothing.
- [ ] Co-op: set 200 % on the host **before** the client joins; after joining, the client shows 200 %.
- [ ] Tick HP 200 %, press Back, reopen Custom Run and embark: the log's "Modifiers:" line still lists SPOON_MONSTER_HP200.

## Combat (task 5)

- [ ] Solo, Monster HP 200 %: the first enemy has double its usual HP range.
- [ ] Monster HP 25 %: a very low-HP enemy still has at least 1 HP.
- [ ] Monster Damage 50 %: enemy intents and the HP you actually lose are halved.
- [ ] A fight with summons (an enemy that spawns minions): the minions are scaled too, once.
- [ ] Test Subject boss: its second and third forms are scaled too. Ovicopter: hatched eggs are scaled.
- [x] Co-op: host and client see identical enemy HP.
- [ ] Save & quit during a run with HP 200 % + Damage 50 %, then continue: the next combat is still scaled, and the top bar still lists both modifiers.
- [ ] Custom run with both rows unticked: enemy HP and damage are vanilla.
- [x] Top bar during a run with HP 200 % + Damage 50 %: two entries read `200%` in red and `50%` in blue (no "missing" icon); hovering each names its modifier. The two entries don't overlap.

## Run modes

- [x] Solo menu: Standard and Daily are greyed out with "Disabled by Slay the Spoon…" (French: "Désactivé par Slay the Spoon…"); clicking them does nothing; Custom works.
- [x] Multiplayer > Host: same for Standard and Daily; Custom hosts a lobby.
- [ ] With a controller, opening either menu focuses Custom, not the greyed Standard button.

## Progress sync

- [ ] First launch with the mod (no `mod_configs/SlaySpoon.cfg`): a popup asks about progress sync; "Keep separate" leaves the modded profile alone, "Use my progress" copies and applies at once.
- [ ] Deleting `mod_configs/SlaySpoon.cfg` makes the popup come back at the next launch.

- [x] With the mod active, the log shows `[SlaySpoon] copied regular progress to modded/profile1/saves/progress.save`.
- [x] Characters and Neow unlocked in the regular profile are available in modded runs; a Sealed Deck run starts at Neow and offers the 30-card choice.
- [x] The regular profile's `progress.save` is byte-identical before and after a modded session.

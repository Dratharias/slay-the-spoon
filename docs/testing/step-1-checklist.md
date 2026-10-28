# Step 1 — in-game checklist

Run `make up` first. For local co-op on one PC, `make mp` opens a host window (Custom Run lobby)
and a client window that joins it (Mega Crit's `--fastmp` dev flag, ENet on 127.0.0.1). The final
co-op check should also be done once over Steam with a friend.

## Custom Run screen (task 4)

- [ ] Solo Custom Run: "Monster HP" and "Monster Damage" rows are listed at the bottom, with − / + buttons.
- [ ] − / + step by 25 % between 25 % and 500 %, tick the row, and the row text shows the value. Nothing happens past the ends.
- [ ] Starting the run: the log line "Embarking on a CUSTOM ... Modifiers:" lists MODIFIER.SPOON_MONSTER_HP… / SPOON_MONSTER_DAMAGE….
- [ ] Standard run and Daily run screens: no Slay the Spoon rows.
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
- [ ] Co-op: host and client see identical enemy HP.
- [ ] Save & quit during a run with HP 200 % + Damage 50 %, then continue: the next combat is still scaled, and the top bar still lists both modifiers.
- [ ] Mod installed but rows unticked, and a Standard run: enemy HP and damage are vanilla.

## Progress sync

- [ ] With the mod active, the log shows `[SlaySpoon] copied regular progress to modded/profile1/saves/progress.save`.
- [ ] Characters and Neow unlocked in the regular profile are available in modded runs; a Sealed Deck run starts at Neow and offers the 30-card choice.
- [ ] The regular profile's `progress.save` is byte-identical before and after a modded session.

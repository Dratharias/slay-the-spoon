# StS1 Boss Relics (0.12.37) — in-game checklist

Build, 553 xUnit tests and 78 Python tests pass. Nothing below has been tested in game yet.
Applies to Custom Runs with the "StS1 Boss Relics" option ticked.

## Solo

- [ ] Neow shows no boss chest option.
- [ ] The act 2 and act 3 Ancients show one more option, "Open the Boss Chest" (EN/FR), below their own gifts.
- [ ] Taking it plays the chest overlay, then a "Choose a Relic" screen with 3 boss relics; the picked relic is gained
      and the Ancient's last page names it. The Ancient's own gift is not gained.
- [ ] Act 2 chests can show Ectoplasm or Sozu but never Philosopher's Stone or Velvet Choker; act 3 the reverse.
- [ ] Character boss relics appear only for the matching character (Watcher: Holy Water, Violet Lotus).
- [ ] A relic already owned never shows up.
- [ ] Each granted StS1 boss relic works (Busted Crown card rewards, Coffee Dripper and Fusion Hammer rest sites,
      Cursed Key chests, Runic Dome intents, Sacred Bark potions, Slaver's Collar elites/bosses, Tiny House pickup,
      the energy relics' extra energy).
- [ ] The run history lists the boss chest choice for that Ancient.

## Co-op (two or more players)

- [ ] A player who takes the chest first sees "The chest stays shut until every player has made their choice." until
      the others choose; then the same 3 relics show on every taker's screen.
- [ ] Players who took the Ancient's gift get it as usual and see no chest.
- [ ] Different votes: each taker gets their relic. Same vote: one taker wins ("You win the rock-paper-scissors..."),
      the loser gets a relic nobody voted for, or nothing when none is left.
- [ ] Mixed characters among the takers: no character boss relic in the chest.
- [ ] Both clients stay in sync (no checksum error leaving the room); saving and reloading after the Ancient keeps the relics.

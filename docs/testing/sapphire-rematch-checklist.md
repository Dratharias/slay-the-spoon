# Sapphire chest rematch (0.12.0) — in-game checklist

Build, 398 xUnit tests and 74 Python tests pass. Nothing below has been tested in game yet.
Applies to runs with Act 4 (unlocked Standard run, or the Custom Run "Act 4" option) while the team lacks the Sapphire.

## Solo

- [ ] Opening a chest shows the Sapphire Key right of the relic (gem and "Take the Sapphire Key" button, EN/FR tooltip).
- [ ] Clicking the gem (or the button with a controller) takes the key: top-bar Sapphire lights up, no relic is gained.
- [ ] Taking the relic instead leaves the key; the next chest offers it again.
- [ ] Skipping with the proceed button still works; once the key is held, chests are vanilla (no key shown).

## Co-op (two or more players)

- [ ] Different relics in the first round: everyone gets their relic, no rematch, no key (design call, see backlog).
- [ ] Two players vote the same relic: rock-paper-scissors as usual; the winner keeps the relic in hand, the loser's
      hand points again and the line "Rematch: take the last relic or the Sapphire Key" shows (FR: "Revanche : …").
- [ ] The loser takes the key: the key is the team's on every screen; the relic left is not handed out.
- [ ] The loser takes the last relic instead: the final award shows every relic going to the right player, no fight
      replayed, and each player's relic bar and grab bag are correct.
- [ ] Three or four players: losers rematch among the relics left ("Rematch: choose another relic"); players already
      served see "Rematch: the others choose again" and cannot vote.
- [ ] Clicking a relic during a round animation does nothing; votes work again once the rematch line shows.
- [ ] Controller: after a round, focus lands on a relic left and left/right cycles only through the relics left.
- [ ] Key already held: the loser gets the relic left automatically (vanilla consolation prize, no rematch).
- [ ] Both peers agree on every award and on the key (`[SlaySpoon] act4: Sapphire chest round …` lines in `godot.log`).
- [ ] `make mp` (fake multiplayer): the other player's rematch votes come in on their own.

# Backlog close (0.12.38) — in-game checklist

Build, 553 xUnit tests and 78 Python tests pass. Nothing below has been tested in game yet.

## Can't-play lines

- [ ] Signature Move with another Attack in hand: the thought bubble says "This must be my only Attack!" (FR: "Ce doit
      être ma seule Attaque !"), with mouse and with a controller.
- [ ] Clash with a non-Attack in hand, Grand Finale with a non-empty draw pile: their StS1 lines show (EN/FR).
- [ ] The same cards blocked by something else (not enough energy, Entangled, a Normality-like hook) still show the
      vanilla line for that cause.
- [ ] Other cards' unplayable lines are unchanged.

## Watcher

- [ ] Lesson Learned killing the last enemy: the upgraded card's preview plays before the rewards, and the deck card is
      upgraded once.
- [ ] A Watcher holding an orb (e.g. through Prismatic Shard): the orb tooltip's energy icon is the purple one, not a
      missing image.

## Strangle in co-op

- [ ] Custom Run co-op: Strangle's and StS1 Choke's tooltip says "Whenever any player plays a card" (FR: "Lorsqu'un
      joueur joue une carte"), for the applier and for allies; Standard co-op and solo keep the vanilla text.

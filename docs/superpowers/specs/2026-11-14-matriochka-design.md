# Matriochka — design

Status: design agreed with the owner (2026-11-14). Tracked in `docs/backlog.md`.

## Intent

A Custom Run modifier about deck bloat taken to the absurd: the run is vanilla Hoarder ("Collectionniste" in French),
with a card reward the player cannot skip on every floor, a free Dolly's Mirror, a forced Bing Bong and a forced
"Shatter" at Reflections in act 3. The name is **"Matriochka" in both English and French** (dolls that multiply).

## Rules

1. **Hoarder included.** Every card added to the deck comes with 2 extra copies; the merchant no longer removes cards.
   Matriochka applies this itself (no need to tick Hoarder too); ticking both does not stack. *(builder call)*
2. **A mandatory card reward on every floor.** Combat rooms (normal, elite, boss): the usual card reward cannot be
   skipped. Other rooms (merchant, rest site, event, treasure): a card reward is added when the room ends, also
   unskippable. *(builder call on the non-combat rooms)*
3. **Dolly's Mirror from the first merchant.** Entering the run's first merchant room gives Dolly's Mirror for free
   (its pickup effect duplicates a card), on top of the normal shop.
4. **Doll Room guaranteed, Bing Bong forced.** The first event of act 2 is replaced by the Doll Room *(builder call)*;
   its only option gives Bing Bong.
5. **Reflections forced in act 3.** Every node on the map row just before the final rest-site row becomes the
   Reflections event *(builder call: the whole row, so every path meets it)*; "Shatter" (duplicate the deck, gain Bad
   Luck) is the only option. In co-op, every player shatters.
6. **Rerolls.** Each player may reroll card rewards a number of times per act (owner, 2026-11-14: 3 by default, set
   with the row's −/+ from 0 to 10). The vanilla reroll button (Driftwood's) shows the rerolls left; the count resets
   each act and is saved with the run.
7. **With Folie à deux** also ticked, the mandatory rewards are bid on; the winner must take a card.

Custom Run only, solo and co-op. Daily and Standard untouched.

## Approaches

One sensible way: a `SpoonMatriochka` Custom Run modifier like the mod's others, reusing vanilla Hoarder's hooks, the
mod's event injection (`Sts1EventRule` / `EventInjectionPatch`), pure rules in `src/Core/` with tests, EN/FR text. A
separate host-menu card (as for Folie à deux) was not wanted: Custom only.

## Constraints

- EN/FR text for everything added; never write the regular save; seeds stay vanilla (forced events and rows are
  deterministic from the map, no extra RNG draws on vanilla streams).

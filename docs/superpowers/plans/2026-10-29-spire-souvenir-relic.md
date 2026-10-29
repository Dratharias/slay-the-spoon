# Spire Souvenir Relic Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A Common Ironclad relic that puts its owner, and only its owner, in the Hybrid card set mode.

**Architecture:** The card set mode stops being run-wide and becomes per player: `effective mode = rule(run mode, player owns the relic)`.
The pool postfix (`UnlockedCardsPatch`) has no player argument, but nearly every vanilla caller passes `player.UnlockState`.
Each `Player` owns a distinct `UnlockState` instance (`Player.CreateForNewRun(..., UnlockState.FromSerializable(...))` per player, also on load).
So the postfix finds the player by reference. The run-wide `RunState.UnlockState` (a merged instance, used by Massive Scroll) matches no player and falls back to the run's mode.
The duplicate patches (`VersionSwap`, `RandomVersionPatch`, `RewardSwapPatch`) switch from "is the run Hybrid" to "is this player Hybrid".

**Tech Stack:** C# .NET 9, Harmony, StS2 v0.107.1 models; xUnit for `src/Core/`.

**Spec:** `docs/superpowers/specs/2026-10-29-spire-souvenir-relic-design.md`

## Global Constraints

- Branch `step-2`; commit after each task; never push. Commit messages end with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Every text the mod adds goes in both `godot/SlaySpoon/localization/eng/` and `.../fra/`. The repo is English-only otherwise.
- Never commit decompiled game code (reference: `~/Projects/sts-decomp/sts2/`).
- Build and test only through `make build` / `make test` (Podman). Close the game before `make up` (`pgrep -x SlayTheSpire2`).
- Rarity `RelicRarity.Common`, Ironclad relic pool only.
- Never offered when the run's mode is Hybrid or the run's `GameMode` is `GameMode.Daily`.
- Effect for its owner only, computed live from the owner's relics. Nothing extra is saved, nothing is cached per player.
- Standard runs without the relic must behave exactly as before. With no relic anywhere, the effective mode equals today's run mode on every path.

## Review Focus

1. **Co-op, relic on one player:** the other player's rewards, merchant and transforms stay in the run's mode. Check: in-game checklist line (Task 3); the per-player resolution is reference-based, so every client resolves the same player.
2. **Run-wide `UnlockState`** (Massive Scroll, any caller passing `RunState.UnlockState`): no player matches, so the run's mode applies and nothing throws. Check: `SouvenirRuleTests.NoOwner_KeepsRunMode` (Task 1) and the `null` player path in `CardSetPools.ModeFor` (Task 3).
3. **Relic lost mid-run** (an event removes or swaps it): the next offer uses the run's mode again. This holds because the mode is computed live. Check: checklist line (Task 3) with `relic remove SPOON_SPIRE_SOUVENIR`.
4. **Relic given in a Hybrid run** through the console: no change, no error. Check: `SouvenirRuleTests.HybridRun_StaysHybrid` (Task 1).
5. **Save and resume** with the relic: the effect returns with the relic. Check: checklist line (Task 3).

---

### Task 1: Per-player mode rule (pure logic)

**Files:**
- Create: `src/Core/SouvenirRule.cs`
- Test: `tests/SouvenirRuleTests.cs`

**Interfaces:**
- Produces:
  - `public static CardSetMode SouvenirRule.EffectiveMode(CardSetMode runMode, bool ownsSouvenir)`
  - `public static bool SouvenirRule.IsOffered(CardSetMode runMode, bool isDaily)`
  - Both live in namespace `SlaySpoon.Core`.

- [ ] **Step 1: Write the failing tests** in `tests/SouvenirRuleTests.cs` (class `SouvenirRuleTests`, same style as `NeowRuleTests`):

```csharp
[Fact] public void NoOwner_KeepsRunMode()
{
	Assert.Equal(CardSetMode.Sts2Only, SouvenirRule.EffectiveMode(CardSetMode.Sts2Only, ownsSouvenir: false));
	Assert.Equal(CardSetMode.Sts1Only, SouvenirRule.EffectiveMode(CardSetMode.Sts1Only, ownsSouvenir: false));
	Assert.Equal(CardSetMode.Hybrid, SouvenirRule.EffectiveMode(CardSetMode.Hybrid, ownsSouvenir: false));
}

[Fact] public void Owner_GoesHybrid()
{
	Assert.Equal(CardSetMode.Hybrid, SouvenirRule.EffectiveMode(CardSetMode.Sts2Only, ownsSouvenir: true));
	Assert.Equal(CardSetMode.Hybrid, SouvenirRule.EffectiveMode(CardSetMode.Sts1Only, ownsSouvenir: true));
}

[Fact] public void HybridRun_StaysHybrid()
{
	Assert.Equal(CardSetMode.Hybrid, SouvenirRule.EffectiveMode(CardSetMode.Hybrid, ownsSouvenir: true));
}

[Fact] public void Offered_OutsideHybridAndDaily()
{
	Assert.True(SouvenirRule.IsOffered(CardSetMode.Sts2Only, isDaily: false));
	Assert.True(SouvenirRule.IsOffered(CardSetMode.Sts1Only, isDaily: false));
	Assert.False(SouvenirRule.IsOffered(CardSetMode.Hybrid, isDaily: false));
	Assert.False(SouvenirRule.IsOffered(CardSetMode.Sts2Only, isDaily: true));
}
```

- [ ] **Step 2: Run `make test`.** Expected: the build fails because `SouvenirRule` does not exist.

- [ ] **Step 3: Implement `SouvenirRule`** in `src/Core/SouvenirRule.cs`: a static class with the two methods above and an XML summary citing spec §2.

- [ ] **Step 4: Run `make test`.** Expected: all tests pass, including the 4 new ones.

- [ ] **Step 5: Commit** with the message `feat: per-player card set rule for the Spire Souvenir`.

---

### Task 2: The relic itself

**Files:**
- Create: `src/Relics/SpoonSpireSouvenir.cs`, `src/Relics/SouvenirPoolPatch.cs`, `godot/SlaySpoon/localization/eng/relics.json`, `godot/SlaySpoon/localization/fra/relics.json`
- Modify: `tests/Sts1AssetsTests.cs` (one new fact), `CLAUDE.md` (code map row `src/Relics/`)

**Interfaces:**
- Consumes: `SouvenirRule.IsOffered` (Task 1); `CardSetPools.ModeOf(IRunState?)` (existing).
- Produces: `public sealed class SpoonSpireSouvenir : RelicModel` in namespace `SlaySpoon.Relics`. Its model id entry is `SPOON_SPIRE_SOUVENIR`.

- [ ] **Step 1: Write the failing test** `RelicTexts_AreComplete` in `tests/Sts1AssetsTests.cs`, reusing its `Table(lang, table)` helper:

```csharp
[Fact]
public void RelicTexts_AreComplete()
{
	foreach (string lang in new[] { "eng", "fra" })
	{
		Dictionary<string, string> relics = Table(lang, "relics");
		foreach (string key in new[] { "title", "description", "flavor" })
		{
			Assert.False(string.IsNullOrWhiteSpace(relics.GetValueOrDefault($"SPOON_SPIRE_SOUVENIR.{key}")), $"{lang} {key}");
		}
	}
}
```

- [ ] **Step 2: Run `make test`.** Expected: FAIL, because `relics.json` doesn't exist.

- [ ] **Step 3: Write the two `relics.json` files** with this exact text (keys sorted like the other tables):

| Key | eng | fra |
|-----|-----|-----|
| `SPOON_SPIRE_SOUVENIR.title` | `Spire Souvenir` | `Souvenir de la Tour` |
| `SPOON_SPIRE_SOUVENIR.description` | `You can now find cards from the other Slay the Spire.` | `Vous pouvez désormais trouver des cartes de l'autre Slay the Spire.` |
| `SPOON_SPIRE_SOUVENIR.flavor` | `A little piece of an older Spire.` | `Un petit morceau d'une Tour plus ancienne.` |

- [ ] **Step 4: Implement `SpoonSpireSouvenir`** with these members:
  - `Rarity => RelicRarity.Common`;
  - `protected override string IconBaseName => "dusty_tome"`. This borrows the vanilla Dusty Tome icon for the atlas icon, the outline and the big icon, which all derive from `IconBaseName`;
  - `IsAllowed(IRunState runState) => SouvenirRule.IsOffered(CardSetPools.ModeOf(runState), runState.GameMode == GameMode.Daily)`.

  The relic carries no hooks: its effect is read by Task 3. Its XML summary points at the spec.

- [ ] **Step 5: Implement `SouvenirPoolPatch`.** It is a Harmony postfix on `IroncladRelicPool`'s protected `GenerateAllRelics` (patch it by name, `"GenerateAllRelics"`). It appends `ModelDb.Relic<SpoonSpireSouvenir>()` to `__result`.
  - Mods load before `OneTimeInitialization.ExecuteEssential` → `ModelDb.Init`, and the pool caches its list on the first `AllRelics` read. So the postfix runs in time.
  - Each player's grab bag draws from their own character pool, which makes this a character relic.

- [ ] **Step 6: Add a `src/Relics/` row to the `CLAUDE.md` code map:** "Spire Souvenir relic (Ironclad pool postfix, borrowed Dusty Tome icon); its effect lives in `CardSetPools.ModeFor`".

- [ ] **Step 7: Run `make test` and `make build`.** Expected: all tests pass, and the build reports 0 warnings and 0 errors.

- [ ] **Step 8: Commit** with the message `feat: Spire Souvenir relic in the Ironclad pool`.

---

### Task 3: Per-player Hybrid on every card path

**Files:**
- Modify: `src/CardSets/CardSetPools.cs`, `src/CardSets/UnlockedCardsPatch.cs:33`, `src/CardSets/VersionSwap.cs:12-20`, `src/CardSets/RandomVersionPatch.cs:22,55`, `src/CardSets/RewardSwapPatch.cs:55`
- Modify: `README.md` (Features), `docs/testing/step-2b-checklist.md` (new section), the spec's `Status:` line (`approved 2026-10-29`)

**Interfaces:**
- Consumes: `SouvenirRule.EffectiveMode` (Task 1), `SpoonSpireSouvenir` (Task 2).
- Produces, in `CardSetPools`:
  - `public static CardSetMode ModeFor(Player? player, IRunState? fallbackRun = null)`: `ModeOf(fallbackRun)` when `player` is null, else `SouvenirRule.EffectiveMode(ModeOf(player.RunState), player.Relics.Any(r => r is SpoonSpireSouvenir))`.
  - `public static Player? PlayerOf(IRunState? run, UnlockState unlockState)`: the player whose `UnlockState` is the same reference, else null.
- Changes in `VersionSwap`: `IsHybrid(IRunState?)` becomes `IsHybrid(Player? player) => CardSetPools.ModeFor(player) == CardSetMode.Hybrid`, and `PartnerOf(card)` uses `card.Owner`.

- [ ] **Step 1: Add `ModeFor` and `PlayerOf`** to `CardSetPools`. `ModeOf` stays as the run-wide reader that `IsAllowed` uses.

- [ ] **Step 2: Rewire `UnlockedCardsPatch.Postfix`.** Replace the mode line with the code below, then keep the existing `Sts2Only` early return and the `Compose` call unchanged:

```csharp
RunState? run = RunState(RunManager.Instance);
CardSetMode mode = CardSetPools.ModeFor(CardSetPools.PlayerOf(run, unlockState), run);
```

  Update the class comment: the mode is now per player, and the player is found by its `UnlockState`.

- [ ] **Step 3: Switch the duplicate patches to the player.**
  - `VersionSwap.IsHybrid(Player?)` and `VersionSwap.PartnerOf(card)` use `card.Owner`.
  - `RandomVersionPatch.Reward` and `Combat` call `IsHybrid(player)`.
  - `RewardSwapPatch.AddSwapAlternatives` calls `IsHybrid(cardReward.Player)`.
  - Merchant and transform rolls already go through `PartnerOf(card)`, so they follow the card's owner with no further change.
  - Grep `src/` for any remaining `IsHybrid(` that takes an `IRunState`. Expected: none.

- [ ] **Step 4: Run `make test` and `make build`.** Expected: all tests pass, 0 warnings.

- [ ] **Step 5: Update the docs.**
  - `README.md` Features gets one bullet: "Spire Souvenir: a Common Ironclad relic, also in Standard runs, that lets its owner find cards from the other game (Hybrid for that player only; never offered in Hybrid or Daily runs)."
  - `docs/testing/step-2b-checklist.md` gets a section "Spire Souvenir" with unticked lines:
    - Standard run: `relic add SPOON_SPIRE_SOUVENIR`. The tooltip shows the Dusty Tome icon and the text in English and French.
    - Standard run, relic held: card rewards show StS1 cards, and a duplicate shows the swap button.
    - Merchant and a transform offer StS1 cards.
    - Custom Run with "Slay the Spire 1 Cards" and the relic: StS2 cards come back.
    - Custom Run with "Cards From Both Games": the relic never drops. Given by console, nothing changes.
    - `relic remove SPOON_SPIRE_SOUVENIR`: the next reward is back to the run's mode.
    - Save, quit and resume with the relic: the effect persists.
    - Co-op (`make mp`): relic on the host only. The client's rewards stay vanilla, and both screens agree on the host's reward.
  - Change the spec's `Status:` line to `approved by the owner 2026-10-29`.

- [ ] **Step 6: Commit** with the message `feat: Spire Souvenir puts its owner in Hybrid`.

- [ ] **Step 7: Install for the owner.** Check `pgrep -x SlayTheSpire2` (nothing must be running), then run `make up` and report the checklist section in French.

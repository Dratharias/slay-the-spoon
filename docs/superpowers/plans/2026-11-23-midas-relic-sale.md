# King Midas relic sale Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** In King Midas runs, a merchant Sell button lets a player sell a relic for 75 % of its merchant price; the
merchant offers it back, dearer, in an extra slot of the seller's shop. A broken Maw Bank is reopened at the merchant
for gold instead of at the rest site.

**Architecture:** Pure numbers and eligibility in `src/Core/MidasRule.cs` (tested). A queued synced action
(`MidasSaleAction`, the `ForgeDuplicateAction` pattern) runs the relic choice, `RelicCmd.Remove` and the gold gain on
every client. The resale is a local-only `MerchantRelicEntry` shown in a cloned `NMerchantRelic` node; vanilla's own
purchase path syncs its purchase (merchant purchases are local, synced by `RewardSynchronizer` events).

**Tech Stack:** C# .NET 9, Harmony, Godot 4.5.1, xUnit. Build and tests in Podman (`make build`, `make test`).

**Spec:** `docs/superpowers/specs/2026-11-23-midas-relic-sale-design.md`

## Global Constraints

- Work in `~/Projects/github/slay-the-spoon-main`, branch `main`. Version **1.9.0** in `mod/SlaySpoon.json` and `src/ModEntry.cs`.
- King Midas (`SpoonRoiMidas.IsActive`) only.
- Not sellable: the 11 relics of `MidasStart.Relics()`, and rarities Starter, Ancient, Event, None.
- Sale price = `RelicModel.MerchantCost` × 75 / 100, rounded down (175 → 131, 225 → 168, 275 → 206, 200 → 150). Not doubled by vanilla Midas.
- Resale base cost = `MerchantCost` (no random factor), then vanilla hooks (`Hook.ModifyMerchantPrice`: King Midas +10 % per purchase, Membership Card, ...), then never below ceil(sale × 1.25).
- Never draw from vanilla RNG streams (`PlayerRng.Shops`, relic grab bags): seeds stay vanilla.
- Every new text in `godot/SlaySpoon/localization/eng/` and `fra/`. Repo text in English.
- No bot code in the staged package (`grep -c DevBot` on the staged DLL is 0). No Steam upload.

## Review Focus

1. Membership Card (a King Midas start relic, −50 % merchant prices) would make the buy-back cheaper than the sale: an infinite gold loop. The resale floor applies after every price hook. Test: `ResaleFloor` values; in-game checklist line.
2. Creating a `MerchantRelicEntry` calls `CalcCost`, which draws `PlayerRng.Shops`; buying it calls `RestockAfterPurchase`, which pulls a relic from the grab bag. Both must be skipped for resale entries, or the run's seed drifts from vanilla and co-op peers diverge.
3. A player cancels the relic screen (skip button): no relic removed, no gold, no resale slot.
4. Selling a relic with a pickup effect (e.g. a max HP relic): `RelicCmd.Remove` runs its `AfterRemoved` like vanilla; the resale re-obtain runs `AfterObtained` again (a fresh canonical copy, counters reset).
5. Co-op: the sale is a queued action, so peers see the relic leave and the gold arrive; only the seller sees the resale slot.

---

### Task 1: Sale rules in Core

**Files:**
- Modify: `src/Core/MidasRule.cs`
- Test: `tests/MidasRuleTests.cs`

**Interfaces:**
- Produces:
  - `public enum MidasRelicRarity { Common, Uncommon, Rare, Shop, Starter, Ancient, Event, None }` (in `SlaySpoon.Core`)
  - `public const int SalePercent = 75;`
  - `public static bool CanSell(MidasRelicRarity rarity, bool startPack)`: false when `startPack` or rarity is Starter, Ancient, Event, None.
  - `public static int SalePrice(int merchantCost)`: `merchantCost * SalePercent / 100`.
  - `public static int ResaleFloor(int salePrice)`: ceil(salePrice × 1.25).

- [ ] **Step 1: Write the failing tests** in `tests/MidasRuleTests.cs`

```csharp
[Theory]
[InlineData(175, 131)] [InlineData(225, 168)] [InlineData(275, 206)] [InlineData(200, 150)]
public void SalePrice_IsThreeQuartersRoundedDown(int cost, int sale) => Assert.Equal(sale, MidasRule.SalePrice(cost));

[Theory]
[InlineData(131, 164)] [InlineData(168, 210)] [InlineData(206, 258)] [InlineData(150, 188)]
public void ResaleFloor_IsSalePlusAQuarterRoundedUp(int sale, int floor) => Assert.Equal(floor, MidasRule.ResaleFloor(sale));

[Theory]
[InlineData(MidasRelicRarity.Common, false, true)] [InlineData(MidasRelicRarity.Uncommon, false, true)]
[InlineData(MidasRelicRarity.Rare, false, true)] [InlineData(MidasRelicRarity.Shop, false, true)]
[InlineData(MidasRelicRarity.Starter, false, false)] [InlineData(MidasRelicRarity.Ancient, false, false)]
[InlineData(MidasRelicRarity.Event, false, false)] [InlineData(MidasRelicRarity.None, false, false)]
[InlineData(MidasRelicRarity.Rare, true, false)]
public void CanSell_ExcludesStartPackAndUnpricedRarities(MidasRelicRarity r, bool startPack, bool expected) =>
    Assert.Equal(expected, MidasRule.CanSell(r, startPack));

[Fact]
public void Resale_AlwaysCostsMoreThanTheSale()
{
    foreach (int cost in new[] { 175, 200, 225, 275 })
        Assert.True(MidasRule.ResaleFloor(MidasRule.SalePrice(cost)) > MidasRule.SalePrice(cost));
}
```

- [ ] **Step 2: Run** `make test` — Expected: build FAIL (`MidasRule.SalePrice` not defined).
- [ ] **Step 3: Implement** the Interfaces above in `src/Core/MidasRule.cs` (enum at namespace level in the same file).
- [ ] **Step 4: Run** `make test` — Expected: all xUnit and pytest pass.
- [ ] **Step 5: Commit** `git commit -m "King Midas: relic sale rules"` (with the Co-Authored-By line).

### Task 2: Sale action and resale entry

**Files:**
- Create: `src/Midas/MidasRelicSale.cs`

**Interfaces:**
- Consumes: Task 1; `MidasStart.Relics()` (`src/Config/RoiMidasModifier.cs`); `SpoonRoiMidas.IsActive(IRunState?)`.
- Produces:
  - `public static class MidasRelicSale`
    - `public static IReadOnlyList<RelicModel> Sellable(Player player)`: held relics where `CanSell(RarityOf(r), startPack: MidasStart ids contain r.Id)`.
    - `public static int PriceOf(RelicModel relic)` = `MidasRule.SalePrice(relic.MerchantCost)`.
    - `public static void Request()`: enqueue `MidasSaleAction` for the local player (copy `ForgeDuplicate.Request`).
    - `internal static async Task Run(Player player)`: if King Midas is inactive or `Sellable` is empty, return; `RelicSelectCmd.FromChooseARelicScreen(player, Sellable(player))`; null → return (cancel); `RelicCmd.Remove(relic)`; `PlayerCmd.GainGold(PriceOf(relic), player)`; if `LocalContext.IsMe(player)`, `Resale.Offer(relic.CanonicalInstance.ToMutable(), player, PriceOf(relic))`; log `King Midas: player {NetId} sold {Id.Entry} for {price}`; raise `Changed`.
    - `public static event Action? Changed;`
    - `public static event Action<MerchantRelicEntry>? ResaleOffered;` (Task 3 shows the slot).
  - `NetMidasSale` (`INetAction, IPacketSerializable`) and `MidasSaleAction : GameAction` (`NonCombat`), copies of the Forge pair.
  - `internal static class Resale`: a `HashSet<MerchantRelicEntry>` of resale entries plus their floors; `Offer` sets a `[ThreadStatic]` "creating" flag around `new MerchantRelicEntry(relic, player)` and raises `ResaleOffered`.
  - Harmony patches in the same file:
    - `MerchantRelicEntry.CalcCost` prefix: while creating a resale entry (or for a known resale entry), set `_cost` (`AccessTools.FieldRefAccess<MerchantEntry, int>("_cost")`) to `Model.MerchantCost` and skip the original.
    - `MerchantRelicEntry.RestockAfterPurchase` prefix: skip for resale entries (slot stays empty).
    - `MerchantEntry.Cost` getter postfix: for a resale entry, `__result = Math.Max(__result, floor)`.
- Map `RelicRarity` to `MidasRelicRarity` in a private `RarityOf`.

- [ ] **Step 1: Implement** the file as above.
- [ ] **Step 2: Run** `make build` — Expected: 0 warnings, 0 errors.
- [ ] **Step 3: Run** `make test` — Expected: all pass. Harmony treats any method named `Prefix`, `Postfix`, `Transpiler`, `Finalizer` or `Prepare` in a patch class as a patch: name helpers otherwise.
- [ ] **Step 4: Commit** `git commit -m "King Midas: relic sale action and resale entry"`.

### Task 3: Merchant button, resale slot, text

**Files:**
- Modify: `src/Midas/MidasRelicSale.cs` (button patch)
- Modify: `godot/SlaySpoon/localization/eng/merchant_room.json`, `fra/merchant_room.json`

**Interfaces:**
- Consumes: Task 2 (`Sellable`, `Request`, `Changed`, `ResaleOffered`); `NativeTradePopup.Button`, placement of `ForgeDuplicateButtonPatch` (`src/Forge/ForgeDuplicate.cs`).
- Produces: `MidasSellButtonPatch` (postfix on `NMerchantRoom._Ready`).

- [ ] **Step 1: Text.** `merchant_room.json`: `"SPOON_MIDAS_SELL": "Sell a relic"` / `"Vendre une relique"`.
- [ ] **Step 2: Button.** Postfix on `NMerchantRoom._Ready` when King Midas is active: `NativeTradePopup.Button(__instance.Inventory, text, MidasRelicSale.Request, disabled: Sellable(me).Count == 0)`, anchored top right like Duplicate, at `(-340, 130)` plus `(0, 70)` when `SpoonForge.Of(run)` is active. Refresh on `Changed`; unsubscribe on `TreeExiting`.
- [ ] **Step 3: Resale slot.** On `ResaleOffered`, duplicate the first `NMerchantRelic` under `%Relics` of `__instance.Inventory`, add it beside the Sell button (same parent as the button), `Initialize(__instance.Inventory)`, `FillSlot(entry)`. Unsubscribe on `TreeExiting`. One slot per sale (several sales: several slots in a row, 120 px apart).
- [ ] **Step 4: Run** `make build` and `make test` — Expected: 0 warnings, all pass (the loc test checks both languages).
- [ ] **Step 5: Commit** `git commit -m "King Midas: Sell button and resale slot at the merchant"`.

### Task 3b: Reopen the Maw Bank at the merchant

**Files:**
- Modify: `src/Core/MidasRule.cs`, `tests/MidasRuleTests.cs`
- Create: `src/Midas/MidasBankReopen.cs`
- Modify: `src/Midas/MidasRestSite.cs` (delete `MawBankRestSiteOption`), `src/Config/RoiMidasModifier.cs` (delete the
  `TryModifyRestSiteOptions` override), `src/Act4/RubyRestSitePatch.cs` (drop its two Maw Bank branches)
- Modify: `merchant_room.json` EN/FR; remove the unused `SPOON_MIDAS_BANK` rest site keys

**Interfaces:**
- Produces: `public const int BankReopenPrice = 50;` and
  `public static int BankReopen(int boughtHere, int scale = 1)` = round(`BankReopenPrice` × `PriceFactor(boughtHere, scale)`).
  `MidasBankReopen.Request()`, `NetMidasBankReopen` / `MidasBankReopenAction` (Forge pattern), `Run(player)`: bank broken
  and `MidasMelt.CanSpend(player, price)` → `PlayerCmd.LoseGold(price, player, GoldLossType.Spent)`, then
  `HasItemBeenBought = false`, `Status = RelicStatus.Normal` (the deleted option's body); raise `Changed`.
  `SpoonRoiMidas` exposes `public int BoughtHere(Player player)` for the price.

- [ ] **Step 1: Failing test:** `BankReopen(0) == 50`, `BankReopen(2) == 60`, `BankReopen(0, scale: 2) == ` the value
  `PriceFactor(0, 2)` gives × 50 (read `PriceFactor` first and pin the number).
- [ ] **Step 2:** `make test` fails; implement; `make test` passes.
- [ ] **Step 3: Button** in `MidasSellButtonPatch`: "Reopen the bank (N gold)" / "Rouvrir la tirelire (N or)", 70 px below
  Sell, visible only while the player's Maw Bank is broken, disabled when unaffordable; refresh on gold change and on
  `Changed` of both services (a purchase breaking the bank shows it).
- [ ] **Step 4:** `make build test` — 0 warnings, all pass. Commit `King Midas: reopen the Maw Bank at the merchant`.

### Task 4: Release 1.9.0

**Files:**
- Modify: `mod/SlaySpoon.json`, `src/ModEntry.cs` (1.9.0)
- Modify: `godot/SlaySpoon/localization/*/modifiers.json` (King Midas description: one short clause, "sell relics at the merchant" / "vendez vos reliques au marchand")
- Modify: `workshop/description.txt`, `workshop/description-fr.txt`, `README.md` (King Midas paragraph, same clause)
- Create: `workshop/changelog-1.9.0.txt`
- Modify: `docs/testing/roi-midas-checklist.md` (section "Relic sale (1.9.0)": bank reopen button appears after a purchase breaks the bank, reopens it, the rest site no longer offers it; sell, cancel, start pack and Ancient/Event absent, prices 131/168/206/150, resale above the floor with Membership Card, buy-back works, co-op peer sees the gold and the relic leave, Blood + King Midas sale, button under Duplicate with the Forge)
- Modify: `CLAUDE.md` (section "1.9.0 — King Midas relic sale")

- [ ] **Step 1:** Make the edits above.
- [ ] **Step 2: Run** `make build test workshop` — Expected: 0 warnings, all tests pass, `build/workshop/` staged with version 1.9.0.
- [ ] **Step 3: Check** `grep -ac "DevBot\|BotEnv" build/workshop/content/SlaySpoon/SlaySpoon.dll` — Expected: 0.
- [ ] **Step 4: Commit** `git commit -m "1.9.0: King Midas relic sale at the merchant"`. Push and Steam upload only on the owner's request.

# Step 5 — Merchant Loans Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** In co-op Custom Runs with "Merchant Loans" ticked, a player short of gold at the merchant can ask the
others for it; each other player can lend it (with interest up to 300%) or give it; the borrower picks the offers
they take; every gold the borrower gains afterwards repays their lenders in equal shares.

**Architecture:** Pure maths in `src/Core/` (`LoanLedger`, `LoanBoard`), tested with xUnit. A `SpoonMerchantLoans`
modifier owns the ledger, saves it in a vanilla-named `int[]` saved property, repays from `ModifyGoldGained` and
cancels debts in `AfterDeath`. Every request, offer and answer is one mod `INetAction` (`NetLoanAction` →
`LoanAction`) run on every client in the host's order, so the board, the transfers and the ledger stay identical.
A code-built panel on the merchant room shows the requests.

**Tech Stack:** C# .NET 9, Godot 4.5.1, Harmony, xUnit.

**Spec:** `docs/superpowers/specs/2026-10-28-slay-the-spoon-design.md` (§3 item 6, §4 "Loans", §6 "Loans", §8).

## Global Constraints

- Every added text in `godot/SlaySpoon/localization/eng/` and `.../fra/`; repo text in English.
- Standard and Daily runs untouched; without the modifier nothing changes (no patch acts, no RNG drawn).
- Never write the regular save; never commit decompiled code.
- Saved property names must be vanilla ones (`Sts1SavedProperties` refuses others): the ledger uses `FurCoatCoordCols`.
- Players are identified by their index in `runState.Players` (net ids are 64-bit; the save holds ints).
- Amount and interest steppers: +1 / +5 / +10 buttons (plus a reset to the minimum); interest 0–300%.
- Interest: owed = amount + (amount × rate + 50) / 100 (integer, half up), fixed at acceptance.
- Repayment: 100% of gold gained, oldest loan first, equal shares between that loan's lenders still owed,
  indivisible remainder to the earliest lenders; repayments reaching a lender are plain `Gold +=`, not gold gained.
- Gifts never enter the ledger. Debts are cancelled when the borrower dies. Solo runs: the modifier does nothing.
- Version 0.7.0. Commit on `step-5`. No push, no install without the owner.

## Review Focus

1. A gold gain applied in a different order than a loan action on two clients → ledgers diverge. Log the
   encoded ledger after every change (`[SlaySpoon] loans:`) so the owner can compare both clients' logs.
2. Save, quit and resume (and co-op resume) keep debts: `Decode(Encode(x))` round trip test; checklist item.
3. A lender spends gold after offering: acceptance clamps each offer to the lender's current gold (board test).
4. A lender who is also a borrower: repayment received is not taken for their own debt (ledger test via the
   modifier only crediting `Gold`; covered by `Repay` returning payments, not calling it recursively).
5. Rounding never creates or loses gold: kept + payments == gained, for 1 gold split between 3 lenders (test).

---

### Task 1: Loan ledger maths

**Files:** Create `src/Core/LoanLedger.cs`; Test `tests/LoanLedgerTests.cs`.

**Interfaces — Produces:**
- `public sealed record LoanEntry(int LoanId, int Borrower, int Lender, int Owed)`
- `public sealed record LoanPayment(int Lender, int Amount)`
- `public sealed class LoanLedger`:
  `IReadOnlyList<LoanEntry> Entries`; `static int Owed(int amount, int ratePercent)`;
  `int AddLoan(int borrower, IReadOnlyList<(int Lender, int Owed)> shares)` (returns the new loan id, max + 1,
  ignores shares with `Owed <= 0`, returns 0 and adds nothing when none is left);
  `(int Kept, IReadOnlyList<LoanPayment> Payments) Repay(int borrower, int gained)` (payments merged per lender,
  in first-paid order); `void Cancel(int borrower)`; `int DebtOf(int borrower)`; `int OwedTo(int lender)`;
  `int[] Encode()` (4 ints per entry, in order); `static LoanLedger Decode(int[]? data)` (null/empty → empty;
  length not a multiple of 4 → `ArgumentException`).

- [ ] **Step 1: Failing tests** — `Owed(100, 0) == 100`, `Owed(100, 300) == 400`, `Owed(5, 10) == 6` (0.5 rounds up),
  `Owed(4, 10) == 4`; repay with no debt keeps all; one lender owed 50, gain 30 → kept 0, pays 30, debt 20;
  gain 80 → kept 30, debt 0, entry removed; two lenders owed 20 and 40, gain 30 → 15/15; gain 30 with owed 10/40 →
  10 and 20 (the paid-off lender drops out, the rest goes to the other); three lenders, gain 1 → first lender 1,
  kept + payments sum == gained; two loans → the older one is paid first; Cancel removes only that borrower;
  Encode/Decode round trip; next loan id after decode is max + 1; Decode of length 5 throws.
- [ ] **Step 2:** `make test` → fails (type missing).
- [ ] **Step 3:** Implement. Equal shares per loan: while gold remains and entries remain, share = remaining / count;
  if 0, give 1 to the first `remaining` entries; else each gets min(share, owed); drop paid entries; repeat.
- [ ] **Step 4:** `make test` → all pass.
- [ ] **Step 5:** Commit `feat: loan ledger maths (equal shares, oldest loan first)`.

### Task 2: Loan board (requests and offers)

**Files:** Create `src/Core/LoanBoard.cs`; Test `tests/LoanBoardTests.cs`.

**Interfaces — Produces:**
- `public sealed record LoanOffer(int Lender, int Amount, int RatePercent, bool Gift)`
- `public sealed record LoanTransfer(int Lender, int Amount, int RatePercent, bool Gift)`
- `public sealed class LoanBoard`: `const int MaxRate = 300`; `void Request(int borrower, int missing)` (replaces the
  borrower's open request and its offers; `missing <= 0` cancels); `bool HasRequest(int)`; `int Missing(int)`;
  `IReadOnlyList<int> Borrowers` (request order); `IReadOnlyList<LoanOffer> OffersFor(int borrower)` (offer order);
  `void Offer(int borrower, LoanOffer offer)` (ignored without a request or from the borrower; rate clamped 0–300,
  ignored for gifts → 0; `Amount <= 0` withdraws; a lender's new offer replaces theirs in place);
  `IReadOnlyList<LoanTransfer> Accept(int borrower, IReadOnlyCollection<int> lenders, Func<int, int> goldOf)`
  (picked offers in offer order, each min(amount, need left, goldOf(lender)), skips zeros, closes the request);
  `void Cancel(int borrower)`; `void Clear()`; `event Action? Changed` raised by every mutation.

- [ ] **Step 1: Failing tests** — offer without request ignored; borrower can't offer to themselves; second offer
  from same lender replaces the first; amount 0 withdraws; rate 500 clamps to 300; gift rate forced to 0;
  accept picks only chosen lenders; accept stops at the missing amount (offers 30 + 30 for 40 → 30, 10);
  accept clamps to the lender's current gold; accept closes the request; new request clears old offers.
- [ ] **Step 2:** `make test` → fails.
- [ ] **Step 3:** Implement.
- [ ] **Step 4:** `make test` → all pass.
- [ ] **Step 5:** Commit `feat: loan board for merchant requests and offers`.

### Task 3: Modifier, synced actions, repayment

**Files:** Create `src/Config/MerchantLoansModifier.cs`, `src/Loans/Loans.cs`, `src/Loans/LoanAction.cs`;
Modify `src/CustomRunUi/ModifierListPatch.cs`, `src/TopBar/TopBarPercentPatch.cs`, `src/ModEntry.cs` (0.7.0),
`godot/SlaySpoon/localization/{eng,fra}/modifiers.json`.

**Interfaces — Produces:**
- `public sealed class SpoonMerchantLoans : ModifierModel` — `[SavedProperty] public int[] FurCoatCoordCols`
  (the encoded ledger); `LoanLedger Ledger`; `void Commit()` (re-encodes, logs, raises `Loans.Changed`);
  `ModifyGoldGained(player, amount)` → kept amount, lenders credited with `Gold +=`; `AfterDeath` → `Cancel`.
- `public static class Loans` — `LoanBoard Board`; `event Action? Changed`; `SpoonMerchantLoans? ModifierOf(IRunState?)`;
  `bool IsActive(IRunState?)` (modifier present and ≥ 2 players); `int IndexOf(Player)`; `Player PlayerAt(IRunState, int)`;
  `void Send(NetLoanAction action)` (local player, `ActionQueueSynchronizer.RequestEnqueue`);
  `void Apply(Player sender, NetLoanAction action)`; `string NameOf(Player)`.
- `public enum LoanActionKind { Request, Offer, Accept, Cancel }`; `public struct NetLoanAction : INetAction`
  (kind, borrower index, amount, rate, gift, lender mask for Accept); `public class LoanAction : GameAction`
  (`GameActionType.NonCombat`).
- Accept: transfers move gold directly (`lender.Gold -=`, `borrower.Gold +=`, not `GainGold`), non-gift transfers
  become one loan with `Owed(amount, rate)` per lender.
- Top bar: short label "Loans"/"Prêts"; while the local player owes, "-N" in red; while owed, "+N" in blue.

- [ ] **Step 1:** Implement; `make build` → 0 warnings, 0 errors; `make test` → all pass.
- [ ] **Step 2:** Commit `feat: Merchant Loans modifier with synced loan actions and repayment`.

### Task 4: Merchant UI

**Files:** Create `src/Loans/MerchantLoanPatch.cs`, `src/Loans/LoanPanel.cs`; localization in
`godot/SlaySpoon/localization/{eng,fra}/merchant_room.json`.

- A failed purchase for lack of gold (`NMerchantSlot.OnPurchaseFailed`, `PurchaseStatus.FailureGold`) with loans
  active and no open request of the local player opens an `NGenericPopup`: "You are {Missing} gold short…" → Ask.
- `LoanPanel` (added to `NMerchantRoom` in `_Ready`, board cleared on `_ExitTree`), visible while any request is open:
  - borrower's own request: missing, each offer as a toggle (picked by default), Accept enabled when the picked
    offers cover the missing amount, Cancel;
  - another player's request: amount (+1/+5/+10, reset), interest (+1/+5/+10 up to 300%, reset), Lend, Give,
    Withdraw (when an offer is up); the amount starts at min(need left, own gold) and never exceeds own gold.
- [ ] **Step 1:** Implement; `make build` → 0/0; `make test` → pass.
- [ ] **Step 2:** Commit `feat: merchant loan panel and ask popup`.

### Task 5: Docs

**Files:** Create `docs/testing/step-5-checklist.md`; Modify `CLAUDE.md`, `README.md`.
- [ ] Checklist: solo (no effect), 2-player ask/lend/give/withdraw/pick/accept, interest maths in game, repayment
  from combat gold and from gold rewards, save/quit/resume, borrower death, Standard run untouched, positions.
- [ ] Commit `docs: step 5 checklist, status and README`.

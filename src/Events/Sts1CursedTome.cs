using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;
using SlaySpoon.Relics.Events;

namespace SlaySpoon.Events;

/// <summary>
/// StS1's Cursed Tome: read three pages (1, 2 and 3 HP), then take a random book the reader lacks (10 HP; Necronomicon,
/// Enchiridion or Nilry's Codex, a Circlet once all three are held) or close it (3 HP). StS1 hands the book out as a
/// reward screen; here it is obtained directly, as the other events do.
/// </summary>
public sealed class Sts1CursedTome : Sts1Event
{
	private const string Key = "STS1_CURSED_TOME.pages.";

	public override Sts1EventKind Kind => Sts1EventKind.CursedTome;

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new DynamicVar("Page1", Sts1EventMath.TomePageHpLoss(1)),
		new DynamicVar("Page2", Sts1EventMath.TomePageHpLoss(2)),
		new DynamicVar("Page3", Sts1EventMath.TomePageHpLoss(3)),
		new DynamicVar("TakeHpLoss", Sts1EventMath.TomeTakeHpLoss),
		new DynamicVar("StopHpLoss", Sts1EventMath.TomeStopHpLoss),
	};

	protected override IReadOnlyList<EventOption> GenerateInitialOptions() => new EventOption[]
	{
		new(this, Read, Key + "INITIAL.options.READ"),
		new(this, Leave, Key + "INITIAL.options.LEAVE"),
	};

	private Task Read()
	{
		Page("PAGE_1", "Page1", () => Turn(1, "PAGE_2", "Page2", () => Turn(2, "PAGE_3", "Page3", () => Turn(3, "LAST", null, null))));
		return Task.CompletedTask;
	}

	/// <summary>Shows <paramref name="page"/> with one Continue option costing <paramref name="var"/>.</summary>
	private void Page(string page, string var, Func<Task> next)
	{
		SetEventState(L10NLookup(Key + page + ".description"), new EventOption[]
		{
			new EventOption(this, next, Key + page + ".options.CONTINUE").ThatDoesDamage(DynamicVars[var].BaseValue),
		});
	}

	private async Task Turn(int hpLoss, string page, string? var, Func<Task>? next)
	{
		await LoseHp(hpLoss);
		if (var != null && next != null)
		{
			Page(page, var, next);
			return;
		}
		SetEventState(L10NLookup(Key + "LAST.description"), new EventOption[]
		{
			new EventOption(this, Take, Key + "LAST.options.TAKE").ThatDoesDamage(DynamicVars["TakeHpLoss"].BaseValue),
			new EventOption(this, Stop, Key + "LAST.options.STOP").ThatDoesDamage(DynamicVars["StopHpLoss"].BaseValue),
		});
	}

	private async Task Take()
	{
		await LoseHp(Sts1EventMath.TomeTakeHpLoss);
		RelicModel[] books = new RelicModel[]
		{
			ModelDb.Relic<Sts1Necronomicon>(), ModelDb.Relic<Sts1Enchiridion>(), ModelDb.Relic<Sts1NilrysCodex>(),
		}.Where(book => Owner!.Relics.All(held => held.Id != book.Id)).ToArray();
		RelicModel relic = books.Length == 0 ? ModelDb.Relic<Circlet>() : books[Rng.NextInt(books.Length)];
		await RelicCmd.Obtain(relic.ToMutable(), Owner!);
		SetEventFinished(L10NLookup(Key + "TAKE.description"));
	}

	private async Task Stop()
	{
		await LoseHp(Sts1EventMath.TomeStopHpLoss);
		SetEventFinished(L10NLookup(Key + "STOP.description"));
	}

	private Task Leave()
	{
		SetEventFinished(L10NLookup(Key + "LEAVE.description"));
		return Task.CompletedTask;
	}

	private async Task LoseHp(int amount)
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, amount,
			ValueProp.Unblockable | ValueProp.Unpowered, null, null);
	}
}

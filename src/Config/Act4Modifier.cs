using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using SlaySpoon.Act4;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using SlaySpoon.Cards.Sts1.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Config;

/// <summary>
/// Run flag: the run has Act 4. It owns the team's key state (<see cref="Act4Keys"/>). Custom Runs offer it as the
/// "Act 4" row (<see cref="CustomRunUi.ModifierListPatch"/>), which needs no unlock. Standard runs get it, hidden, from
/// <see cref="Act4.StandardRunLobbyPatch"/> once the profile holds the permanent unlock (<see cref="Act4UnlockRule"/>,
/// <see cref="Act4Access"/>). Daily runs never carry it. Neow ignores it when counting modifiers.
/// </summary>
public sealed class SpoonAct4 : ModifierModel
{
	private Act4Keys _keys = new();

	public Act4Keys Keys => _keys;

	public override Task AfterMapGenerated(ActMap map, int actIndex)
	{
		var elites = map.GetAllMapPoints().Where(p => p.PointType == MapPointType.Elite)
			.Select(p => (p.coord.col, p.coord.row)).ToArray();
		// Draw directly over the candidate count: modulo of a fixed range would bias the choice.
		int roll = elites.Length == 0 ? 0 : new Rng(RunState.Rng.Seed, $"SlaySpoonEmerald{actIndex}").NextInt(elites.Length);
		if (EmeraldRule.MarkAct(Keys, actIndex, elites, roll)) Act4Keys.NotifyChanged();
		return Task.CompletedTask;
	}

	/// <summary>The marked node counts only while its room is an elite fight: Golden Compass can regenerate the map and leave the coordinate on a normal node.</summary>
	private bool AtEmeraldElite => !Keys.Has(Act4Key.Emerald)
		&& RunState.CurrentRoom is { RoomType: RoomType.Elite }
		&& RunState.CurrentMapCoord is { } coord
		&& Keys.EmeraldNode(RunState.CurrentActIndex) == (coord.col, coord.row);

	public override async Task BeforeCombatStart()
	{
		if (!AtEmeraldElite || RunState.CurrentRoom is not CombatRoom room) return;
		var (kind, amount) = EmeraldRule.Buff(
			new Rng(RunState.Rng.Seed, $"SlaySpoonEmeraldBuff{RunState.CurrentActIndex}").NextInt(4), RunState.CurrentActIndex);
		SpoonLog.Info($"act4: Emerald elite buff {kind} {amount}");
		var choice = new ThrowingPlayerChoiceContext();
		foreach (var enemy in room.Enemies.ToArray())
		{
			switch (kind)
			{
				case EmeraldBuffKind.Strength:
					await PowerCmd.Apply<StrengthPower>(choice, enemy, amount, null, null);
					break;
				case EmeraldBuffKind.MaxHp:
					await CreatureCmd.GainMaxHp(enemy, EmeraldRule.MaxHpGain(enemy.MaxHp));
					break;
				case EmeraldBuffKind.Metallicize:
					await PowerCmd.Apply<Sts1MetallicizePower>(choice, enemy, amount, null, null);
					break;
				case EmeraldBuffKind.Regenerate:
					await PowerCmd.Apply<Sts1RegenerateMonsterPower>(choice, enemy, amount, null, null);
					break;
			}
		}
	}

	public override Task AfterCombatVictory(CombatRoom room)
	{
		if (AtEmeraldElite) Keys.Take(Act4Key.Emerald);
		return Task.CompletedTask;
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		if (Keys.Has(Act4Key.Ruby)) return false;
		options.Add(new Sts1RubyRestSiteOption(player));
		return true;
	}

	/// <summary>MemberwiseClone would share the canonical instance's key state with every run; each run starts fresh.</summary>
	protected override void DeepCloneFields()
	{
		base.DeepCloneFields();
		_keys = new Act4Keys();
	}

	/// <summary>The run's flag, or null when the run has no Act 4.</summary>
	public static SpoonAct4? Of(IRunState? run)
	{
		return run?.Modifiers.OfType<SpoonAct4>().FirstOrDefault();
	}

	/// <summary>The encoded key state. Saved properties must reuse a vanilla name; Fur Coat's is an int array too.</summary>
	[SavedProperty]
	public int[] FurCoatCoordCols
	{
		get => _keys.Encode();
		set
		{
			try
			{
				_keys = Act4Keys.Decode(value);
			}
			catch (ArgumentException e)
			{
				SpoonLog.Error($"act4: saved key state dropped: {e.Message}");
				_keys = new Act4Keys();
			}
			Act4Keys.NotifyChanged();
		}
	}
}

using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using SlaySpoon.Act4;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using SlaySpoon.Core;

namespace SlaySpoon.Config;

/// <summary>
/// Hidden run flag: the run has Act 4. It owns the team's key state (<see cref="Act4Keys"/>). Standard runs get it from
/// <see cref="Act4.StandardRunLobbyPatch"/>; it is not in the Custom Run list. Neow ignores it when counting modifiers.
/// </summary>
public sealed class SpoonAct4 : ModifierModel
{
	private Act4Keys _keys = new();

	public Act4Keys Keys => _keys;

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

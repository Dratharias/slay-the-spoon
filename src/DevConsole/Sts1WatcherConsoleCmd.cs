using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using SlaySpoon.Characters;

namespace SlaySpoon.DevConsole;

/// <summary>
/// Temporary console command <c>sts1watcher</c> for the Step 7 spike: starts a solo Custom run as the Watcher, the way
/// the Custom Run screen starts one, before she has a place in the lobby. Removed by Task 2.
/// </summary>
public sealed class Sts1WatcherConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "sts1watcher";

	public override string Args => "";

	public override string Description => "Starts a solo Custom run as the Watcher (Step 7 spike; use it from the main menu).";

	public override bool IsNetworked => false;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (RunManager.Instance.IsInProgress || NGame.Instance == null)
		{
			return new CmdResult(success: false, "Use it from the main menu, with no run in progress.");
		}
		return new CmdResult(Start(), success: true, "Starting a Watcher run.");
	}

	private static async Task Start()
	{
		NGame game = NGame.Instance!;
		CharacterModel watcher = ModelDb.Character<Sts1Watcher>();
		string seed = SeedHelper.GetRandomSeed();
		var acts = ActModel.GetRandomList(new Rng((uint)StringHelper.GetDeterministicHashCode(seed), "act_selection"),
			SaveManager.Instance.GenerateUnlockStateFromProgress(), isMultiplayer: false).ToList();
		NAudioManager.Instance?.StopMusic();
		SfxCmd.Play(watcher.CharacterTransitionSfx);
		await game.Transition.FadeOut(0.8f, watcher.CharacterSelectTransitionPath);
		await game.StartNewSingleplayerRun(watcher, shouldSave: true, acts, Array.Empty<ModifierModel>(), seed,
			GameMode.Custom);
	}
}

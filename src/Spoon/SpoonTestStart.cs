using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using SlaySpoon.Act4;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Spoon;

/// <summary>
/// The main-menu "Test: The Spoon" start: a solo Custom Run (Spoon + the hidden <see cref="SpoonTestRun"/> flag) with an
/// Ironclad at 350 max HP and <see cref="SpoonTestDeck"/>, that opens straight on the Spoon act's map. Acts 1–3 are
/// drawn and filled as a Custom Run does it, then the Spoon act goes in at index 3 as <see cref="SpoonRouting"/> would.
/// </summary>
/// <remarks>
/// Mirrors <c>NGame.StartNewSingleplayerRun</c> and its private <c>StartRun</c>, with the deck and HP set before the run
/// state exists (so card creation hooks run as usual) and <c>EnterAct(3)</c> in place of <c>EnterAct(0)</c>: no Neow,
/// no act 1 map. The run saves to the modded profile like any run (save/reload can be tested).
/// </remarks>
public static class SpoonTestStart
{
	private static bool _starting;
	private static bool? _visible;

	/// <summary>True when this mod was loaded from the game's <c>mods/</c> folder; a Workshop copy never shows the button.</summary>
	public static bool ButtonAllowed
	{
		get
		{
			if (_visible is { } known) return known;
			try
			{
				var assembly = typeof(ModEntry).Assembly;
				Mod? mod = ModManager.Mods.FirstOrDefault(m => m.assembly == assembly)
				           ?? ModManager.Mods.FirstOrDefault(m => m.manifest?.id == "SlaySpoon" && m.state == ModLoadState.Loaded);
				_visible = SpoonTestStartRule.IsLocalInstall(mod?.modSource.ToString(), mod?.path, OS.GetExecutablePath());
				SpoonLog.Info($"spoon test: mod source {mod?.modSource} at {mod?.path}; test button {(_visible.Value ? "shown" : "hidden")}");
			}
			catch (Exception e)
			{
				SpoonLog.Error($"spoon test: install check failed, button hidden: {e.Message}");
				_visible = false;
			}
			return _visible.Value;
		}
	}

	public static async Task Start()
	{
		if (_starting || RunManager.Instance.IsInProgress) return;
		_starting = true;
		try
		{
			NGame game = NGame.Instance ?? throw new InvalidOperationException("no NGame");
			CharacterModel character = ModelDb.Character<Ironclad>();
			string seed = SeedHelper.GetRandomSeed();
			var unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
			// The Custom Run lobby's act draw (StartRunLobby): same named Rng over the seed.
			var acts = ActModel.GetRandomList(new Rng((uint)StringHelper.GetDeterministicHashCode(seed), "act_selection"),
				unlockState, false).ToList();
			List<ModifierModel> modifiers =
				[ModelDb.Modifier<SpoonSpoon>().ToMutable(), ModelDb.Modifier<SpoonTestRun>().ToMutable()];
			SpoonLog.Info($"spoon test: Embarking on a CUSTOM {character.Id.Entry} run. Seed: {seed} Modifiers: SPOON_SPOON, SPOON_TEST_RUN");

			SfxCmd.Play(character.CharacterTransitionSfx);
			await game.Transition.FadeOut(0.8f, character.CharacterSelectTransitionPath);

			Player player = Player.CreateForNewRun(character, unlockState, 1uL);
			player.Deck.Clear(silent: true);
			foreach (var (card, count) in SpoonTestDeck.Cards)
			{
				for (int i = 0; i < count; i++)
				{
					CardModel copy = card.ToMutable();
					copy.FloorAddedToDeck = 1;
					if (copy.IsUpgradable)
					{
						copy.UpgradeInternal();
						copy.FinalizeUpgradeInternal();
					}
					player.Deck.AddInternal(copy, -1, silent: true);
				}
			}
			foreach (RelicModel relic in SpoonTestDeck.Relics)
				player.AddRelicInternal(relic.ToMutable());
			player.Creature.SetMaxHpInternal(SpoonTestStartRule.MaxHp);
			player.Creature.SetCurrentHpInternal(SpoonTestStartRule.MaxHp);

			RunState state = RunState.CreateForNewRun([player], acts.Select(a => a.ToMutable()).ToList(), modifiers,
				GameMode.Custom, 0, seed);
			RunManager manager = RunManager.Instance;
			// Rooms for acts 1–3 and the modifiers' run start (Spoonful, grab bags) happen here, before the Spoon act exists.
			manager.SetUpNewSingleplayer(state, shouldSave: true);

			int index = SpoonRule.InsertIndex(state.Acts.Count, false);
			var withSpoon = new List<ActModel>(state.Acts);
			withSpoon.Insert(index, SpoonAct.CreateForRun(state));
			Act4Routing.SetActs(state, withSpoon);

			game.ReactionContainer.InitializeNetworking(new NetSingleplayerGameService());
			using (new NetLoadingHandle(manager.NetService))
			{
				await PreloadManager.LoadRunAssets(state.Players.Select(p => p.Character));
				await PreloadManager.LoadActAssets(state.Acts[index]);
				await manager.FinalizeStartingRelics();
				manager.Launch();
				game.RootSceneContainer.SetCurrentScene(NRun.Create(state));
				await manager.EnterAct(index, doTransition: false);
			}
			SpoonLog.Info($"spoon test: started on act {index} ({string.Join(", ", state.Acts.Select(a => a.Id.Entry))})");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"spoon test: start failed: {e}");
			if (NGame.Instance is { } game) await game.ReturnToMainMenuWithInternalError(e);
		}
		finally
		{
			_starting = false;
		}
	}
}

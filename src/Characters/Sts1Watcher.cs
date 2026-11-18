using MegaCrit.Sts2.Core.Entities.Creatures;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using SlaySpoon.Cards.Sts1.Watcher;
using SlaySpoon.Characters.Pools;
using SlaySpoon.Core;
using SlaySpoon.Relics.Watcher;
using SlaySpoon.Spine;

namespace SlaySpoon.Characters;

/// <summary>
/// The Watcher, StS1's fourth character, as an StS2 character with static visuals. ModelDb discovers this class by
/// reflection, but ModelDb.AllCharacters is a fixed list of the five vanilla characters, so dedicated patches add her to Custom and Standard solo selection without adding her to
/// Daily, random picks, unlocks or the compendium. Her own art replaces the paths the game
/// derives from the id where a virtual member allows it; <see cref="WatcherAssetPatch"/> covers the rest.
/// </summary>
public sealed class Sts1Watcher : CharacterModel
{
	public override Color NameColor => StsColors.purple;

	public override CharacterGender Gender => CharacterGender.Feminine;

	protected override CharacterModel? UnlocksAfterRunAs => null;

	public override int StartingHp => 72;

	public override int StartingGold => 99;

	public override CardPoolModel CardPool => ModelDb.CardPool<Sts1WatcherCardPool>();

	public override RelicPoolModel RelicPool => ModelDb.RelicPool<Sts1WatcherRelicPool>();

	public override PotionPoolModel PotionPool => ModelDb.PotionPool<Sts1WatcherPotionPool>();

	public override IEnumerable<CardModel> StartingDeck => new CardModel[]
	{
		ModelDb.Card<Sts1StrikeWatcher>(),
		ModelDb.Card<Sts1StrikeWatcher>(),
		ModelDb.Card<Sts1StrikeWatcher>(),
		ModelDb.Card<Sts1StrikeWatcher>(),
		ModelDb.Card<Sts1DefendWatcher>(),
		ModelDb.Card<Sts1DefendWatcher>(),
		ModelDb.Card<Sts1DefendWatcher>(),
		ModelDb.Card<Sts1DefendWatcher>(),
		ModelDb.Card<Sts1Eruption>(),
		ModelDb.Card<Sts1Vigilance>(),
	};

	public override IReadOnlyList<RelicModel> StartingRelics => new RelicModel[] { ModelDb.Relic<Sts1PureWater>() };

	public override float AttackAnimDelay => 0.15f;

	public override float CastAnimDelay => 0.25f;

	/// <summary>Her StS1 skeleton has Idle and Hit only: no attack or cast animation, as in StS1.</summary>
	public override CreatureAnimator GenerateAnimator(MegaSprite controller, Creature creature) => Sts1Spine.Animator(controller);

	// Necrobinder's purple UI colours.
	public override Color EnergyLabelOutlineColor => new Color("702D6FFF");

	public override Color DialogueColor => new Color("6B4658");

	public override VfxColor SpeechBubbleColor => VfxColor.Purple;

	public override Color MapDrawingColor => new Color("AC0486");

	public override Color RemoteTargetingLineColor => new Color("FD98C9FF");

	public override Color RemoteTargetingLineOutline => new Color("702D6FFF");

	/// <summary>The select and transition sounds are FMOD events, which a mod PCK can't add: the Silent's.</summary>
	public override string CharacterSelectSfx => "event:/sfx/characters/silent/silent_select";

	public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_silent";

	/// <summary>Only preloaded: <see cref="WatcherAssetPatch"/> builds the top bar icon from our texture.</summary>
	protected override string IconPath => SceneHelper.GetScenePath("ui/character_icons/silent_icon");

	protected override string CharacterSelectIconPath => WatcherAssets.CharacterSelectIcon;

	protected override string CharacterSelectLockedIconPath => WatcherAssets.CharacterSelectLockedIcon;

	protected override string MapMarkerPath => WatcherAssets.MapMarker;

	/// <summary>Textures the patched scenes use, loaded with the run so no lookup misses the cache.</summary>
	protected override IEnumerable<string> ExtraAssetPaths =>
		new[] { WatcherAssets.Body, WatcherAssets.Corpse, WatcherAssets.TopBarIconOutline }.Concat(WatcherAssets.OrbLayers);

	/// <summary>Her four hits on the Architect in the victory scene, with vanilla effects (4, like every character).</summary>
	public override List<string> GetArchitectAttackVfx() => new()
	{
		"vfx/vfx_attack_slash",
		"vfx/vfx_heavy_blunt",
		"vfx/vfx_flying_slash",
		"vfx/vfx_thrash",
	};
}

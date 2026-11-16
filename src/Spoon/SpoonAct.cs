using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using SlaySpoon.Act4;

namespace SlaySpoon.Spoon;

/// <summary>
/// Spoon mode's short act (docs/superpowers/specs/2026-11-14-spoon-design.md, rule 4): a rest site, a merchant, then the
/// Spoon. <see cref="SpoonRouting"/> inserts it after act 3, before The Ending when present. Glory supplies the map
/// colours, music and chest assets; the combat background comes from EndingBackgroundPatch.
/// </summary>
/// <remarks>
/// <see cref="Index"/> is -1, so the act stays out of <c>ModelDb.ActsByIndex</c> and random act choice, like
/// <see cref="Sts1EndingAct"/>. The appended-act patches (rooms, map, assets, rewards) cover it through
/// <see cref="IAppendedAct"/>.
/// </remarks>
public sealed class SpoonAct : ActModel, IAppendedAct
{
	/// <summary>Name of the act's own room Rng, so filling its rooms never draws from a vanilla Rng.</summary>
	public const string RoomsRngName = "SlaySpoon.Spoon.Rooms";

	string IAppendedAct.RoomsRngName => RoomsRngName;

	private static Glory Canon => ModelDb.Act<Glory>();

	public override int Index => -1;
	public override bool IsDefault => false;

	protected override int BaseNumberOfRooms => 1;
	protected override int NumberOfWeakEncounters => 0;

	public override Color MapTraveledColor => Canon.MapTraveledColor;
	public override Color MapUntraveledColor => Canon.MapUntraveledColor;
	public override Color MapBgColor => Canon.MapBgColor;

	public override string[] BgMusicOptions => Canon.BgMusicOptions;
	public override string[] MusicBankPaths => Canon.MusicBankPaths;
	public override string AmbientSfx => Canon.AmbientSfx;

	public override string ChestSpineResourcePath => Canon.ChestSpineResourcePath;
	public override string ChestSpineSkinNameNormal => Canon.ChestSpineSkinNameNormal;
	public override string ChestSpineSkinNameStroke => Canon.ChestSpineSkinNameStroke;
	public override string ChestOpenSfx => Canon.ChestOpenSfx;

	public override IEnumerable<EncounterModel> BossDiscoveryOrder => [];
	public override IEnumerable<AncientEventModel> AllAncients => [];
	public override IEnumerable<EventModel> AllEvents => [];

	public override IEnumerable<EncounterModel> GenerateAllEncounters() => [ModelDb.Encounter<SpoonBoss>()];

	public override bool IsUnlocked(UnlockState unlockState) => true;

	public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState state) => [];

	protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState)
	{
	}

	/// <summary>Same no-op as vanilla DeprecatedAct: the act's map is <see cref="SpoonActMap"/>, not built from counts.</summary>
	public override MapPointTypeCounts GetMapPointTypes(Rng mapRng) => new(0, 0);

	/// <summary>A mutable Spoon act for this run, its rooms filled from the act's own named Rng.</summary>
	public static ActModel CreateForRun(IRunState run)
	{
		var act = (SpoonAct)ModelDb.Act<SpoonAct>().ToMutable();
		act.FillRooms(new Rng(run.Rng.Seed, RoomsRngName));
		return act;
	}

	/// <summary>
	/// Sets the room set directly (vanilla GenerateRooms assumes non-empty weak/normal/ancient lists): the Spoon as the
	/// only room, no events, no normal or elite encounters, no ancient, no second boss. It draws nothing from the Rng.
	/// </summary>
	public void FillRooms(Rng rng)
	{
		AssertMutable();
		_rooms.events.Clear();
		_rooms.normalEncounters.Clear();
		_rooms.eliteEncounters.Clear();
		_rooms.Boss = ModelDb.Encounter<SpoonBoss>();
		_rooms.SecondBoss = null;
	}

	public ActMap CreateFixedMap() => new SpoonActMap();
}

/// <summary>
/// Mod initializers run before <c>ModelDb.Init</c>, so the id and the lookup that <c>ActModel.FromSave</c> does on
/// reload are checked once the model ids are assigned (as <see cref="EndingActLookupCheck"/> does for the Ending).
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.InitIds))]
internal static class SpoonActLookupCheck
{
	private static void Postfix()
	{
		try
		{
			ModelId id = ModelDb.GetId(typeof(SpoonAct));
			if (id.Entry != "SPOON_ACT")
				SpoonLog.Error($"SpoonAct: game id {id.Entry} differs from SPOON_ACT; its title won't load");
			if (ModelDb.GetByIdOrNull<ActModel>(id) is SpoonAct)
				SpoonLog.Info($"Spoon act self-check: ModelDb.GetById<ActModel>({id.Entry}) found");
			else
				SpoonLog.Error($"Spoon act self-check: ModelDb.GetById<ActModel>({id.Entry}) not found; Spoon act saves won't load");
		}
		catch (Exception e)
		{
			SpoonLog.Error($"Spoon act self-check failed: {e.Message}");
		}
	}
}

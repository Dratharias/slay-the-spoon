using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;

namespace SlaySpoon.Act4;

/// <summary>
/// StS1's fourth act, "The Ending": one elite (Spire Shield and Spire Spear) and the Corrupt Heart. It borrows
/// StS1 combat/rest backgrounds and Ogg music through EndingBackgroundPatch and EndingAudio. Glory supplies
/// the map and chest assets; the unused event background is imported for provenance.
/// </summary>
/// <remarks>
/// <see cref="Index"/> is -1, so the act stays out of <c>ModelDb.ActsByIndex</c> and random act choice
/// (<c>ModelDb.Acts</c> is a hardcoded list anyway). It is appended to a run only by the Act 4 routing.
/// </remarks>
public sealed class Sts1EndingAct : ActModel, IAppendedAct
{
	/// <summary>Name of the act's own room Rng, so filling its rooms never draws from a vanilla Rng.</summary>
	public const string RoomsRngName = "SlaySpoon.EndingRooms";

	string IAppendedAct.RoomsRngName => RoomsRngName;

	private static Glory Canon => ModelDb.Act<Glory>();

	public override int Index => -1;
	public override bool IsDefault => false;

	protected override int BaseNumberOfRooms => 2;
	protected override int NumberOfWeakEncounters => 0;

	public override Color MapTraveledColor => Canon.MapTraveledColor;
	public override Color MapUntraveledColor => Canon.MapUntraveledColor;
	public override Color MapBgColor => Canon.MapBgColor;

	public override string[] BgMusicOptions => [];
	public override string[] MusicBankPaths => [];
	public override string AmbientSfx => "";

	public override string ChestSpineResourcePath => Canon.ChestSpineResourcePath;
	public override string ChestSpineSkinNameNormal => Canon.ChestSpineSkinNameNormal;
	public override string ChestSpineSkinNameStroke => Canon.ChestSpineSkinNameStroke;
	public override string ChestOpenSfx => Canon.ChestOpenSfx;

	public override IEnumerable<EncounterModel> BossDiscoveryOrder => [];
	public override IEnumerable<AncientEventModel> AllAncients => [];
	public override IEnumerable<EventModel> AllEvents => [];

	public override IEnumerable<EncounterModel> GenerateAllEncounters() =>
		[ModelDb.Encounter<Sts1ShieldAndSpearElite>(), ModelDb.Encounter<Sts1CorruptHeartBoss>()];

	public override bool IsUnlocked(UnlockState unlockState) => true;

	public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState state) => [];

	protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState)
	{
	}

	/// <summary>Same no-op as vanilla DeprecatedAct: the act's map is built by the Act 4 map code, not from counts.</summary>
	public override MapPointTypeCounts GetMapPointTypes(Rng mapRng) => new(0, 0);

	/// <summary>A mutable Ending act for this run, its rooms filled from the act's own named Rng.</summary>
	public static ActModel CreateForRun(IRunState run)
	{
		var act = (Sts1EndingAct)ModelDb.Act<Sts1EndingAct>().ToMutable();
		act.FillRooms(new Rng(run.Rng.Seed, RoomsRngName));
		return act;
	}

	/// <summary>
	/// Sets the room set directly (vanilla GenerateRooms assumes non-empty weak/normal/ancient lists): no events,
	/// no normal encounters, the Shield and Spear as the only elite, the Heart as boss, no ancient, no second boss.
	/// </summary>
	public void FillRooms(Rng rng)
	{
		AssertMutable();
		_rooms.events.Clear();
		_rooms.normalEncounters.Clear();
		_rooms.eliteEncounters.Clear();
		_rooms.eliteEncounters.Add(rng.NextItem(AllEliteEncounters)!);
		_rooms.Boss = rng.NextItem(AllBossEncounters)!;
		_rooms.SecondBoss = null;
	}

	public ActMap CreateFixedMap() => new EndingActMap();

	/// <summary>Logs at startup whether the act's id is the one its text and saves use.</summary>
	public static void SelfCheck()
	{
		string id = ModelDb.GetId(typeof(Sts1EndingAct)).Entry;
		if (id == "STS1_ENDING_ACT")
		{
			SpoonLog.Info($"Ending act id: {id}");
		}
		else
		{
			SpoonLog.Error($"Sts1EndingAct: game id {id} differs from STS1_ENDING_ACT; its title won't load");
		}
	}
}

/// <summary>
/// Mod initializers run before <c>ModelDb.Init</c>, so the lookup that <c>ActModel.FromSave</c> does on reload
/// can only be checked once the model ids are assigned.
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.InitIds))]
internal static class EndingActLookupCheck
{
	private static void Postfix()
	{
		ModelId id = ModelDb.GetId(typeof(Sts1EndingAct));
		if (ModelDb.GetByIdOrNull<ActModel>(id) is Sts1EndingAct)
		{
			SpoonLog.Info($"Ending act self-check: ModelDb.GetById<ActModel>({id.Entry}) found");
		}
		else
		{
			SpoonLog.Error($"Ending act self-check: ModelDb.GetById<ActModel>({id.Entry}) not found; act 4 saves won't load");
		}
	}
}

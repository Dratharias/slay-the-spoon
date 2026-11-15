using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace SlaySpoon.Config;

/// <summary>
/// Run flag: Folie à deux. Every card reward a player would keep is bid on by the whole team in a chest
/// (<see cref="Folie.FolieChest"/>). Custom Runs offer it as a checkbox; the multiplayer host screen's
/// "Folie à deux" card starts a Standard run carrying it (<see cref="Folie.FolieLobbyPatch"/>). Solo runs ignore it.
/// Neow ignores it when counting modifiers, so a Standard run keeps its vanilla Neow.
/// </summary>
public sealed class SpoonFolieADeux : ModifierModel
{
	public static bool IsActive(IRunState? run) =>
		run != null && run.Players.Count >= 2 && run.Modifiers.Any(m => m is SpoonFolieADeux);
}

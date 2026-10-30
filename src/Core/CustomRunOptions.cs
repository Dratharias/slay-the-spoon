namespace SlaySpoon.Core;

/// <summary>Which Slay the Spoon options the Custom Run list offers, kept free of game types so tests can pin it.</summary>
public static class CustomRunOptions
{
	/// <summary>Modifier types (by class name) added to the Custom Run list on top of the percent, card set, Neow and loan rows.</summary>
	public static readonly IReadOnlyList<string> TypeNames = new[] { "SpoonAct4" };

	/// <summary>The list is Custom Run only; Daily draws from ModelDb.GoodModifiers and stays vanilla. Takes the game's GameMode name.</summary>
	public static bool OfferedIn(string gameMode) => gameMode == "Custom";
}

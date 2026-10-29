using SlaySpoon.Core;

namespace SlaySpoon.Act4;

/// <summary>Resource paths of the Act 4 key sprites (StS1: red = Ruby, green = Emerald, blue = Sapphire).</summary>
public static class KeySprites
{
	public static string Path(Act4Key key, bool large) => "res://SlaySpoon/images/keys/" + (large
		? key switch { Act4Key.Ruby => "ruby_key", Act4Key.Emerald => "emerald_key", _ => "sapphire_key" }
		: key switch { Act4Key.Ruby => "key_red", Act4Key.Emerald => "key_green", _ => "key_blue" }) + ".png";
}

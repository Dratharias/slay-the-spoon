using Godot;

namespace SlaySpoon.Characters;

internal static class WatcherSelectLayout
{
	public static Control AddRightGap(Control container, Control button)
	{
		const string name = "WatcherRightGap";
		if (container.GetNodeOrNull<Control>(name) is { } existing)
		{
			return existing;
		}
		var gap = new Control
		{
			Name = name,
			CustomMinimumSize = new Vector2(24, 0),
			MouseFilter = Control.MouseFilterEnum.Ignore,
			FocusMode = Control.FocusModeEnum.None,
		};
		container.AddChild(gap);
		container.MoveChild(gap, button.GetIndex() + 1);
		return gap;
	}
}

using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace SlaySpoon.Spoon;

/// <summary>
/// Light animations for the Spoon's static sprite (owner request, 2026-11-16): tweens on the body's rotation, position
/// and scale. The sprite's position is the ground point (the handle tip), so every rotation pivots there. Negative
/// rotation leans the bowl to the left, toward the players. Purely local visuals: nothing here touches combat state, so
/// co-op peers need no sync, and a missing node only skips the motion.
/// </summary>
internal sealed class SpoonMotion
{
	private readonly Sprite2D _body;
	private readonly Vector2 _restPosition;
	private Tween? _idle;
	private Tween? _action;
	private float _size = 1f;

	private SpoonMotion(Sprite2D body)
	{
		_body = body;
		_restPosition = body.Position;
	}

	/// <summary>The motion for the creature's sprite body, or null when it has no node (yet) or is not a sprite.</summary>
	public static SpoonMotion? For(Creature creature) =>
		creature.GetCreatureNode()?.Body is Sprite2D body && GodotObject.IsInstanceValid(body) ? new SpoonMotion(body) : null;

	private bool Valid => GodotObject.IsInstanceValid(_body) && _body.IsInsideTree();

	private Vector2 RestScale => new(_size, _size);

	/// <summary>A slow sway and breath while nothing else plays.</summary>
	public void Idle()
	{
		if (!Valid) return;
		_idle?.Kill();
		_idle = _body.CreateTween().SetLoops();
		_idle.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		_idle.TweenProperty(_body, "rotation", Mathf.DegToRad(-3f), 1.3);
		_idle.Parallel().TweenProperty(_body, "scale", new Vector2(_size * 0.99f, _size * 1.02f), 1.3);
		_idle.TweenProperty(_body, "rotation", Mathf.DegToRad(3f), 1.3);
		_idle.Parallel().TweenProperty(_body, "scale", RestScale, 1.3);
	}

	private Tween? StartAction()
	{
		if (!Valid) return null;
		_idle?.Kill();
		_action?.Kill();
		_action = _body.CreateTween();
		return _action;
	}

	/// <summary>Mouthful: leans back, swoops down at the players. Returns once the bowl lands (the hit goes there).</summary>
	public async Task SwoopAsync()
	{
		if (StartAction() is not { } t) return;
		t.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		t.TweenProperty(_body, "rotation", Mathf.DegToRad(14f), 0.25);
		t.Parallel().TweenProperty(_body, "position", _restPosition + new Vector2(25f, 0f), 0.25);
		t.TweenProperty(_body, "rotation", Mathf.DegToRad(-38f), 0.13).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.In);
		t.Parallel().TweenProperty(_body, "position", _restPosition + new Vector2(-70f, 0f), 0.13);
		await _body.ToSignal(t, Tween.SignalName.Finished);
	}

	/// <summary>Ladle Smash: rises up and back, slams down with a squash. Returns on impact.</summary>
	public async Task SmashAsync()
	{
		if (StartAction() is not { } t) return;
		t.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		t.TweenProperty(_body, "rotation", Mathf.DegToRad(22f), 0.32);
		t.Parallel().TweenProperty(_body, "position", _restPosition + new Vector2(30f, -90f), 0.32);
		t.TweenProperty(_body, "rotation", Mathf.DegToRad(-62f), 0.1).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.In);
		t.Parallel().TweenProperty(_body, "position", _restPosition + new Vector2(-90f, 0f), 0.1);
		t.TweenProperty(_body, "scale", new Vector2(_size * 1.12f, _size * 0.88f), 0.06);
		await _body.ToSignal(t, Tween.SignalName.Finished);
	}

	/// <summary>
	/// Sets the table: hops up and taps the bowl down three times, tilting left and right like cutlery set in place.
	/// Returns on the last tap (the kit is dealt then).
	/// </summary>
	public async Task SetTableAsync()
	{
		if (StartAction() is not { } t) return;
		t.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		foreach (float side in new[] { -1f, 1f, -1f })
		{
			t.TweenProperty(_body, "position", _restPosition + new Vector2(side * 20f, -45f), 0.14);
			t.Parallel().TweenProperty(_body, "rotation", Mathf.DegToRad(side * 12f), 0.14);
			t.TweenProperty(_body, "position", _restPosition + new Vector2(side * 30f, 0f), 0.08)
				.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
			t.Parallel().TweenProperty(_body, "scale", new Vector2(_size * 1.06f, _size * 0.94f), 0.08);
			t.TweenProperty(_body, "scale", RestScale, 0.06);
		}
		await _body.ToSignal(t, Tween.SignalName.Finished);
	}

	/// <summary>
	/// Banquet: leans back with the bowl up, swells in three gulps, then shudders. Returns after the last gulp (the max
	/// HP is eaten and the heal lands then).
	/// </summary>
	public async Task BanquetAsync()
	{
		if (StartAction() is not { } t) return;
		t.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		t.TweenProperty(_body, "rotation", Mathf.DegToRad(24f), 0.35);
		t.Parallel().TweenProperty(_body, "position", _restPosition + new Vector2(20f, -20f), 0.35);
		for (int gulp = 1; gulp <= 3; gulp++)
		{
			float swell = 1f + gulp * 0.05f;
			t.TweenProperty(_body, "scale", new Vector2(_size * swell, _size * (2f - swell)), 0.1)
				.SetTrans(Tween.TransitionType.Sine);
			t.TweenProperty(_body, "scale", new Vector2(_size * (2f - swell * 0.98f), _size * swell), 0.12)
				.SetTrans(Tween.TransitionType.Sine);
		}
		t.TweenProperty(_body, "rotation", Mathf.DegToRad(18f), 0.05);
		t.TweenProperty(_body, "rotation", Mathf.DegToRad(28f), 0.05);
		t.TweenProperty(_body, "rotation", Mathf.DegToRad(22f), 0.05);
		await _body.ToSignal(t, Tween.SignalName.Finished);
	}

	/// <summary>The "relevé": back upright with a small overshoot, then the idle again.</summary>
	public void Recover()
	{
		if (StartAction() is not { } t) return;
		t.TweenInterval(0.08);
		t.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		t.TweenProperty(_body, "rotation", 0f, 0.45);
		t.Parallel().TweenProperty(_body, "position", _restPosition, 0.45);
		t.Parallel().TweenProperty(_body, "scale", RestScale, 0.3);
		t.TweenCallback(Callable.From(Idle));
	}

	/// <summary>A short recoil when hit; skipped while an attack plays.</summary>
	public void Recoil()
	{
		if (_action != null && _action.IsValid() && _action.IsRunning()) return;
		if (StartAction() is not { } t) return;
		t.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		t.TweenProperty(_body, "rotation", Mathf.DegToRad(7f), 0.08);
		t.Parallel().TweenProperty(_body, "position", _restPosition + new Vector2(18f, 0f), 0.08);
		t.TweenProperty(_body, "rotation", 0f, 0.25).SetTrans(Tween.TransitionType.Sine);
		t.Parallel().TweenProperty(_body, "position", _restPosition, 0.25);
		t.TweenCallback(Callable.From(Idle));
	}

	/// <summary>
	/// The Spoon swells as it grows: a pulse, then a rest size of 1 + 3 % per 100 max HP grown, at most 1.2.
	/// Applied after the current action so its scale tweens keep their numbers.
	/// </summary>
	public void Grow(int grownTotal)
	{
		_size = Math.Min(1.2f, 1f + grownTotal * 0.0003f);
		if (!Valid) return;
		Tween pulse = _body.CreateTween();
		pulse.TweenInterval(0.5);
		pulse.TweenProperty(_body, "scale", RestScale * 1.08f, 0.12).SetTrans(Tween.TransitionType.Sine);
		pulse.TweenProperty(_body, "scale", RestScale, 0.25).SetTrans(Tween.TransitionType.Sine);
	}

	/// <summary>Death: tips over toward the players and fades, within the monster's 1 s death length.</summary>
	public void Topple()
	{
		if (StartAction() is not { } t) return;
		t.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		t.TweenProperty(_body, "rotation", Mathf.DegToRad(-95f), 0.7);
		t.Parallel().TweenProperty(_body, "modulate:a", 0f, 0.9).SetDelay(0.2);
	}
}

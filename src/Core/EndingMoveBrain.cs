namespace SlaySpoon.Core;

/// <summary>Move state for an Ending enemy; only <see cref="Advance"/> changes it.</summary>
public sealed class EndingMoveBrain<TMove>(Func<int, TMove, int, TMove> rule, TMove initialPrevious) where TMove : struct, Enum
{
	public int Turn { get; private set; }
	public TMove Current { get; private set; } = initialPrevious;

	public TMove Advance(int roll)
	{
		Current = rule(Turn, Current, roll);
		Turn++;
		return Current;
	}
}

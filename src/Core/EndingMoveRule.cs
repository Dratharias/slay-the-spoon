namespace SlaySpoon.Core;

public enum HeartMove { Debilitate, BloodShots, Echo, Buff }
public enum ShieldMove { Bash, Fortify, Smash }
public enum SpearMove { BurnStrike, Piercer, Skewer }
public enum BashDebuff { Strength, Focus }

/// <summary>StS1 Ending move cycles; turn is zero-based, roll is a fair 0/1 choice.</summary>
public static class EndingMoveRule
{
	public static HeartMove Heart(int turn, HeartMove previous, int roll) => turn == 0 ? HeartMove.Debilitate : ((turn - 1) % 3) switch
	{
		0 => roll == 0 ? HeartMove.BloodShots : HeartMove.Echo,
		1 => previous == HeartMove.Echo ? HeartMove.BloodShots : HeartMove.Echo,
		_ => HeartMove.Buff,
	};
	public static ShieldMove Shield(int turn, ShieldMove previous, int roll) => (turn % 3) switch
	{
		0 => roll == 0 ? ShieldMove.Fortify : ShieldMove.Bash,
		1 => previous == ShieldMove.Bash ? ShieldMove.Fortify : ShieldMove.Bash,
		_ => ShieldMove.Smash,
	};
	public static SpearMove Spear(int turn, SpearMove previous, int roll) => (turn % 3) switch
	{
		0 => turn > 0 && previous == SpearMove.BurnStrike ? SpearMove.Piercer : SpearMove.BurnStrike,
		1 => SpearMove.Skewer,
		_ => roll == 0 ? SpearMove.Piercer : SpearMove.BurnStrike,
	};
	/// <summary>Bash takes Focus only from a player with orbs, on a roll of 1.</summary>
	public static BashDebuff BashDebuffFor(bool hasOrbSlots, int roll) => hasOrbSlots && roll == 1 ? BashDebuff.Focus : BashDebuff.Strength;
}

using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;

namespace SlaySpoon.Act4;

/// <summary>Routes a fixed StS1 move cycle without drawing from the shared vanilla AI RNG.</summary>
internal sealed class EndingBranchState(Func<string> choose) : MonsterState
{
	public override string Id => "SPOON_NEXT_MOVE";
	public override bool ShouldAppearInLogs => false;
	public override string GetNextState(Creature owner, Rng rng) => choose();
	public override void RegisterStates(Dictionary<string, MonsterState> monsterStates) => monsterStates.Add(Id, this);
}

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace SlaySpoon.Cards.Sts1.Defect;

public sealed class Sts1SteamBarrier : Sts1DefectCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new BlockVar(6m, ValueProp.Move),
	};

	public override bool GainsBlock => true;

	private int _lostBlock;

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		DynamicVars.Block.BaseValue = Math.Max(0, DynamicVars.Block.BaseValue - _lostBlock);
	}

	public Sts1SteamBarrier()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		if (DynamicVars.Block.BaseValue > 0)
		{
			_lostBlock++;
			DynamicVars.Block.BaseValue = Math.Max(0, DynamicVars.Block.BaseValue - 1);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(2m);
	}
}

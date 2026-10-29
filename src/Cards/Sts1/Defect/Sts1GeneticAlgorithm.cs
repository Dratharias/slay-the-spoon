using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;

namespace SlaySpoon.Cards.Sts1.Defect;

/// <summary>
/// StS1's weaker Genetic Algorithm (grows by 2 / 3) replaces the StS2 card. The growth lives on the deck
/// card and is saved; the property names are vanilla Genetic Algorithm's, so the save already knows them
/// (see <see cref="Sts1SavedProperties"/>).
/// </summary>
public sealed class Sts1GeneticAlgorithm : Sts1DefectCard
{
	private const string IncreaseKey = "Increase";

	private int _currentBlock = 1;

	private int _increasedBlock;

	public override Sts1Relation Relation => Sts1Relation.SameKeepSts1;

	public override Type Sts2Counterpart => typeof(GeneticAlgorithm);

	public override bool GainsBlock => true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
	{
		new BlockVar(CurrentBlock, ValueProp.Move),
		new IntVar(IncreaseKey, 2m),
	};

	[SavedProperty]
	public int CurrentBlock
	{
		get => _currentBlock;
		set
		{
			AssertMutable();
			_currentBlock = value;
			DynamicVars.Block.BaseValue = _currentBlock;
		}
	}

	[SavedProperty]
	public int IncreasedBlock
	{
		get => _increasedBlock;
		set
		{
			AssertMutable();
			_increasedBlock = value;
		}
	}

	public Sts1GeneticAlgorithm()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		int increase = DynamicVars[IncreaseKey].IntValue;
		BuffFromPlay(increase);
		(DeckVersion as Sts1GeneticAlgorithm)?.BuffFromPlay(increase);
	}

	protected override void OnUpgrade()
	{
		DynamicVars[IncreaseKey].UpgradeValueBy(1m);
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		UpdateBlock();
	}

	private void BuffFromPlay(int extraBlock)
	{
		IncreasedBlock += extraBlock;
		UpdateBlock();
	}

	private void UpdateBlock()
	{
		CurrentBlock = 1 + IncreasedBlock;
	}
}

public sealed class FakeGuard : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[1] { CardKeyword.Ethereal };
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[1] { new BlockVar(7m, ValueProp.Move) };
	public FakeGuard()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}
	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(3m);
	}
}

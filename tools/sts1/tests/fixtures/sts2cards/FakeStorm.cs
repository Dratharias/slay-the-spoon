public sealed class FakeStorm : CardModel
{
	protected override bool HasEnergyCostX => true;
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[1] { new DamageVar(6m, ValueProp.Move) };
	public FakeStorm()
		: base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}
	public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[1] { CardKeyword.Innate };
	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(1m);
		AddKeyword(CardKeyword.Retain);
	}
}

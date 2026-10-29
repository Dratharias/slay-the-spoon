public sealed class FakeForge : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[1] { new DamageVar(10m, ValueProp.Move) };
	public FakeForge()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}
}

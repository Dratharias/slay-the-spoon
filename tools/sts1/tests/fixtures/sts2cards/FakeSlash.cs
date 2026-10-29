public sealed class FakeSlash : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[2]
	{
		new DamageVar(9m, ValueProp.Move),
		new PowerVar<VulnerablePower>(2m)
	};
	public FakeSlash()
		: base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}
	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(4m);
		base.DynamicVars["VulnerablePower"].UpgradeValueBy(1m);
		base.EnergyCost.UpgradeBy(-1);
	}
}

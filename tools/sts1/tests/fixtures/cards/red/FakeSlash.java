package fake;
public class FakeSlash extends AbstractCard {
    public static final String ID = "Fake Slash";
    public FakeSlash() {
        super(ID, NAME, "red/attack/fake_slash", 2, DESCRIPTION, AbstractCard.CardType.ATTACK, AbstractCard.CardColor.RED, AbstractCard.CardRarity.COMMON, AbstractCard.CardTarget.ENEMY);
        this.baseDamage = 9;
        this.magicNumber = this.baseMagicNumber = 2;
    }
    public void upgrade() {
        if (!this.upgraded) {
            this.upgradeName();
            this.upgradeDamage(4);
            this.upgradeMagicNumber(1);
            this.upgradeBaseCost(1);
        }
    }
}

package fake;
public class FakeStorm extends AbstractCard {
    public static final String ID = "Fake Storm";
    public FakeStorm() {
        super(ID, NAME, "red/attack/fake_storm", -1, DESCRIPTION, AbstractCard.CardType.ATTACK, AbstractCard.CardColor.RED, AbstractCard.CardRarity.RARE, AbstractCard.CardTarget.ALL_ENEMY);
        this.baseDamage = 5;
        this.isMultiDamage = true;
        this.isInnate = true;
    }
    public void upgrade() {
        if (!this.upgraded) {
            this.upgradeName();
            this.upgradeDamage(3);
            this.selfRetain = true;
        }
    }
}

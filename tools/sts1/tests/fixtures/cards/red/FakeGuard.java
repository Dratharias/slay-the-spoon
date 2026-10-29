package fake;
public class FakeGuard extends AbstractCard {
    public static final String ID = "Fake Guard";
    public FakeGuard() {
        super(ID, NAME, "red/skill/fake_guard", 1, DESCRIPTION, AbstractCard.CardType.SKILL, AbstractCard.CardColor.RED, AbstractCard.CardRarity.UNCOMMON, AbstractCard.CardTarget.SELF);
        this.baseBlock = 7;
        this.exhaust = true;
        this.isEthereal = true;
    }
    public void upgrade() {
        if (!this.upgraded) {
            this.upgradeName();
            this.upgradeBlock(3);
            this.exhaust = false;
        }
    }
}

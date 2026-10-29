package fake;
public class FakeCurse extends AbstractCard {
    public static final String ID = "Fake Curse";
    public FakeCurse() {
        super(ID, NAME, "red/skill/fake_curse", -2, DESCRIPTION, AbstractCard.CardType.SKILL, AbstractCard.CardColor.RED, AbstractCard.CardRarity.SPECIAL, AbstractCard.CardTarget.NONE);
        this.magicNumber = this.baseMagicNumber = 1;
    }
    public void upgrade() {
    }
}

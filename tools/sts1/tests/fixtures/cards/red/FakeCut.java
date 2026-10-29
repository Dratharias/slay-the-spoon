package fake;
public class FakeCut extends AbstractCard {
    public static final String ID = "Fake Cut";
    public FakeCut() {
        super(ID, NAME, "red/skill/fake_cut", 1, DESCRIPTION, AbstractCard.CardType.SKILL, AbstractCard.CardColor.RED, AbstractCard.CardRarity.UNCOMMON, AbstractCard.CardTarget.SELF);
    }
    public void upgrade() {
    }
}

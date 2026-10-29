package fake;
public class FakeForge extends AbstractCard {
    public static final String ID = "Fake Forge";
    public FakeForge() {
        this(0);
    }
    public FakeForge(int upgrades) {
        super(ID, NAME, "red/attack/fake_forge", 2, DESCRIPTION, AbstractCard.CardType.ATTACK, AbstractCard.CardColor.RED, AbstractCard.CardRarity.UNCOMMON, AbstractCard.CardTarget.ENEMY);
        this.baseDamage = 12;
        this.timesUpgraded = upgrades;
    }
    public void upgrade() {
        this.upgradeDamage(4 + this.timesUpgraded);
        ++this.timesUpgraded;
    }
}

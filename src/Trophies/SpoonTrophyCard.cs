using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using SlaySpoon.Core;
using SlaySpoon.Spoon;

namespace SlaySpoon.Trophies;

/// <summary>
/// Trophies: one elite attack move as a card. The monster, move, damage and hits are saved properties; the title is
/// "&lt;Monster&gt;: &lt;Move&gt;" from the game's own monster text, and the cost follows the damage (<see cref="TrophyRule"/>).
/// </summary>
public sealed class SpoonTrophyCard : SpoonCard
{
	private const string HitsKey = "Repeat";

	private string _monster = "";
	private string _move = "";
	private int _damage = TrophyRule.FallbackDamage;
	private int _hits = 1;

	public override bool CanBeGeneratedInCombat => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(_damage, ValueProp.Move), new RepeatVar(_hits)];

	public SpoonTrophyCard()
		: base(1, CardType.Attack, TargetType.AnyEnemy)
	{
	}

	public override string PortraitPath => ModelDb.Card<StrikeIronclad>().PortraitPath;

	[SavedProperty]
	public string TrophyMonster
	{
		get => _monster;
		set { AssertMutable(); _monster = value ?? ""; }
	}

	[SavedProperty]
	public string TrophyMove
	{
		get => _move;
		set { AssertMutable(); _move = value ?? ""; }
	}

	[SavedProperty]
	public int TrophyDamage
	{
		get => _damage;
		set
		{
			AssertMutable();
			_damage = Math.Max(value, 0);
			DynamicVars.Damage.BaseValue = _damage + (IsUpgraded ? TrophyRule.UpgradeBonus(_damage) : 0);
			Recost();
		}
	}

	[SavedProperty]
	public int TrophyHits
	{
		get => _hits;
		set
		{
			AssertMutable();
			_hits = Math.Max(value, 1);
			DynamicVars[HitsKey].BaseValue = _hits;
			Recost();
		}
	}

	private void Recost()
	{
		EnergyCost.SetCustomBaseCost(TrophyRule.Cost(_damage, _hits));
	}

	public override string Title
	{
		get
		{
			string name;
			try
			{
				string monster = new LocString("monsters", _monster + ".name").GetFormattedText();
				string move = _move.Length > 0
					? new LocString("monsters", $"{_monster}.moves.{_move}.title").GetFormattedText()
					: new LocString("cards", "SPOON_TROPHY_CARD.fallback").GetFormattedText();
				name = _monster.Length > 0 ? $"{monster}: {move}" : base.Title;
			}
			catch
			{
				name = base.Title;
			}
			return IsUpgraded && _monster.Length > 0 ? name + "+" : name;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars[HitsKey].IntValue).FromCard(this, cardPlay)
			.Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(TrophyRule.UpgradeBonus(_damage));
}

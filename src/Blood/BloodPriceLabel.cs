using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Entities.Players;
using SlaySpoon.Config;
using SlaySpoon.Core;

namespace SlaySpoon.Blood;

/// <summary>
/// Blood: a small red "N HP" line under every merchant price (cards, relics, potions, card removal), the HP the purchase
/// really costs (<see cref="BloodRule.HpPrice"/>). View only.
/// </summary>
[HarmonyPatch]
public static class BloodPriceLabel
{
	private const string LabelName = "SpoonBloodHp";

	private static readonly AccessTools.FieldRef<NMerchantSlot, Label> CostLabel =
		AccessTools.FieldRefAccess<NMerchantSlot, Label>("_costLabel");
	private static readonly Func<NMerchantSlot, Player?> SlotPlayer =
		AccessTools.MethodDelegate<Func<NMerchantSlot, Player?>>(AccessTools.PropertyGetter(typeof(NMerchantSlot), "Player"));

	public static IEnumerable<MethodBase> TargetMethods() =>
		new[] { typeof(NMerchantCard), typeof(NMerchantRelic), typeof(NMerchantPotion), typeof(NMerchantCardRemoval) }
			.Select(t => AccessTools.DeclaredMethod(t, "UpdateVisual"))
			.Where(m => m != null)!;

	[HarmonyPostfix]
	public static void AfterUpdateVisual(NMerchantSlot __instance)
	{
		try
		{
			Label? cost = CostLabel(__instance);
			if (cost == null || !GodotObject.IsInstanceValid(cost)) return;
			var hp = cost.GetNodeOrNull<Label>(LabelName);
			var player = SlotPlayer(__instance);
			bool show = player != null && SpoonBlood.IsActive(player.RunState) && __instance.Entry is { IsStocked: true } && cost.Visible;
			if (!show)
			{
				if (hp != null) hp.Visible = false;
				return;
			}
			if (hp == null)
			{
				hp = new Label { Name = LabelName, MouseFilter = Control.MouseFilterEnum.Ignore, HorizontalAlignment = HorizontalAlignment.Center };
				hp.AddThemeColorOverride("font_color", new Color(0.95f, 0.3f, 0.3f));
				hp.AddThemeColorOverride("font_outline_color", Colors.Black);
				hp.AddThemeConstantOverride("outline_size", 6);
				hp.AddThemeFontSizeOverride("font_size", 18);
				cost.AddChild(hp);
			}
			hp.Visible = true;
			var text = new LocString("modifiers", "SPOON_BLOOD.hpPrice");
			text.Add("Hp", (decimal)BloodRule.HpPrice(__instance.Entry.Cost));
			hp.Text = text.GetFormattedText();
			hp.Size = new Vector2(cost.Size.X, 24f);
			hp.Position = new Vector2(0f, cost.Size.Y - 2f);
		}
		catch (Exception e)
		{
			SpoonLog.Error($"blood: price label failed: {e.Message}");
		}
	}
}

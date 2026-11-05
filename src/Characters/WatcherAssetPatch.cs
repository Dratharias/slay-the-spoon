using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using SlaySpoon.Act4;
using SlaySpoon.Core;
using SlaySpoon.Spine;

namespace SlaySpoon.Characters;

/// <summary>
/// CharacterModel derives most asset paths from the character id in non-virtual (often private) getters, and the
/// vanilla scenes behind them hold Spine skeletons. A mod PCK can add images but not FMOD events or Spine data, so for
/// the Watcher: image getters point at our art, scene and sound getters at a donor character's (preloaded and
/// instantiated as vanilla), and the scenes that would show the donor's body get our static sprite instead.
/// </summary>
/// <remarks>
/// sts2.dll is plain IL and Harmony 2.4 disables inlining of a patched method (see EndingActAssetsPatch), so callers
/// such as AssetPaths read the patched getters.
/// </remarks>
[HarmonyPatch]
public static class WatcherAssetPatch
{
	/// <summary>Getter name and the character whose value the Watcher borrows.</summary>
	private static readonly (string Getter, Type Donor)[] Borrowed =
	{
		// Preloaded only: CreateVisuals is replaced below.
		("VisualsPath", typeof(Silent)),
		// Its five layers get our orb textures below; StS1's art was cut for this counter's layout.
		("EnergyCounterPath", typeof(Ironclad)),
		// Instantiated, then their Spine body is swapped for our sprite below.
		("MerchantAnimPath", typeof(Silent)),
		("RestSiteAnimPath", typeof(Silent)),
		// Purple, like her cards.
		("TrailPath", typeof(Necrobinder)),
		// Character select background and transition: only reached if she is ever listed there.
		("CharacterSelectBg", typeof(Silent)),
		("CharacterSelectTransitionPath", typeof(Silent)),
		// Co-op pointing hands.
		("ArmPointingTexturePath", typeof(Silent)),
		("ArmRockTexturePath", typeof(Silent)),
		("ArmPaperTexturePath", typeof(Silent)),
		("ArmScissorsTexturePath", typeof(Silent)),
		// FMOD events.
		("AttackSfx", typeof(Silent)),
		("CastSfx", typeof(Silent)),
		("DeathSfx", typeof(Silent)),
	};

	private static readonly Dictionary<string, string> Own = new()
	{
		["IconTexturePath"] = WatcherAssets.TopBarIcon,
		["IconOutlineTexturePath"] = WatcherAssets.TopBarIconOutline,
	};

	private static MethodInfo Getter(string name) =>
		AccessTools.PropertyGetter(typeof(CharacterModel), name)
		?? throw new MissingMemberException(nameof(CharacterModel), name);

	/// <summary>Getter delegates for the donors; the patched getter runs, and passes donors through untouched.</summary>
	private static readonly Dictionary<MethodBase, Func<CharacterModel, string>> DonorValue = new();

	private static readonly Dictionary<MethodBase, Type> DonorOf = new();

	private static readonly Dictionary<MethodBase, string> OwnValue = new();

	public static IEnumerable<MethodBase> TargetMethods()
	{
		foreach ((string name, Type donor) in Borrowed)
		{
			MethodInfo getter = Getter(name);
			DonorOf[getter] = donor;
			DonorValue[getter] = AccessTools.MethodDelegate<Func<CharacterModel, string>>(getter);
			yield return getter;
		}
		foreach ((string name, string path) in Own)
		{
			MethodInfo getter = Getter(name);
			OwnValue[getter] = path;
			yield return getter;
		}
	}

	public static void Postfix(CharacterModel __instance, MethodBase __originalMethod, ref string __result)
	{
		if (__instance is not Sts1Watcher)
		{
			return;
		}
		if (OwnValue.TryGetValue(__originalMethod, out string? own))
		{
			__result = own;
		}
		else if (DonorOf.TryGetValue(__originalMethod, out Type? donor))
		{
			__result = DonorValue[__originalMethod](ModelDb.GetById<CharacterModel>(ModelDb.GetId(donor)));
		}
	}

	internal static Vector2 BodyOrigin => new(WatcherAssets.BodyOriginX, WatcherAssets.BodyOriginY);

	/// <summary>
	/// Her skeleton playing its idle loop outside combat (no animator there), or a plain sprite of the idle pose if it
	/// fails to load; either way the ground point is at the node's origin.
	/// </summary>
	internal static Node2D Body(string name, float scale)
	{
		if (Sts1Spine.CreateSprite(Sts1Spine.Watcher, name) is { } spine)
		{
			spine.Scale = new Vector2(scale, scale);
			Sts1Spine.Play(spine, "idle_loop", loop: true);
			return spine;
		}
		return new Sprite2D
		{
			Name = name,
			Texture = PreloadManager.Cache.GetTexture2D(WatcherAssets.Body),
			Centered = false,
			Offset = -BodyOrigin,
			Scale = new Vector2(scale, scale),
		};
	}

	/// <summary>
	/// Combat, the game over screen and the fake merchant: her StS1 skeleton (Idle and Hit, animated by
	/// <see cref="Sts1Watcher.GenerateAnimator"/>), or the static idle pose if it fails to load.
	/// </summary>
	[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.CreateVisuals))]
	public static class CreateVisualsPatch
	{
		public static bool Prefix(CharacterModel __instance, ref NCreatureVisuals __result)
		{
			if (__instance is not Sts1Watcher)
			{
				return true;
			}
			__result = EndingVisuals.Create(WatcherAssets.Body, BodyOrigin,
				new Rect2(WatcherAssets.HitboxX, WatcherAssets.HitboxY, WatcherAssets.HitboxWidth, WatcherAssets.HitboxHeight),
				Sts1Spine.Watcher);
			return false;
		}
	}

	/// <summary>The top bar and stats icon: the vanilla icon scenes are one full-rect TextureRect; same, with our texture.</summary>
	[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.Icon), MethodType.Getter)]
	public static class IconPatch
	{
		public static bool Prefix(CharacterModel __instance, ref Control __result)
		{
			if (__instance is not Sts1Watcher)
			{
				return true;
			}
			var icon = new TextureRect
			{
				Name = "Sts1WatcherIcon",
				Texture = PreloadManager.Cache.GetTexture2D(WatcherAssets.TopBarIcon),
				MouseFilter = Control.MouseFilterEnum.Ignore,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			};
			icon.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			__result = icon;
			return false;
		}
	}

	/// <summary>The donor energy counter with StS1's purple orb on its five layers.</summary>
	[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter.Create))]
	public static class EnergyCounterPatch
	{
		public static void Postfix(Player player, NEnergyCounter? __result)
		{
			if (__result == null || player.Character is not Sts1Watcher)
			{
				return;
			}
			for (int i = 0; i < WatcherAssets.OrbLayers.Count; i++)
			{
				Node? found = __result.FindChild($"Layer{i + 1}", recursive: true, owned: false);
				if (found is TextureRect layer)
				{
					layer.Texture = PreloadManager.Cache.GetTexture2D(WatcherAssets.OrbLayers[i]);
				}
				else
				{
					SpoonLog.Error(found == null
						? $"Watcher energy counter: no Layer{i + 1} in the donor scene"
						: $"Watcher energy counter: Layer{i + 1} is a {found.GetType().Name}, not a TextureRect");
				}
			}
		}
	}

	/// <summary>
	/// The donor rest site scene without its Spine bodies (removed before _Ready, so the scene never animates or
	/// flips them) and with our sprite. Its hitbox, reticle and thought bubbles stay as they are.
	/// </summary>
	[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter.Create))]
	public static class RestSitePatch
	{
		internal const string BodyName = "Sts1WatcherBody";

		/// <summary>Feet at the bottom of the donor's hitbox, in the root's space; size close to the donor's.</summary>
		private static readonly Vector2 Feet = new(-32f, 250f);

		private const float Scale = 1.6f;

		public static void Postfix(Player player, NRestSiteCharacter __result)
		{
			if (player.Character is not Sts1Watcher)
			{
				return;
			}
			foreach (Node child in __result.GetChildren().Where(c => c.GetClass() == "SpineSprite").ToList())
			{
				__result.RemoveChild(child);
				child.QueueFree();
			}
			Node2D body = Body(BodyName, Scale);
			body.Position = Feet;
			__result.AddChild(body);
			__result.MoveChild(body, 0);
		}
	}

	/// <summary>Rest site characters on the right half face left: mirror our sprite the way FlipX mirrors Spine bodies.</summary>
	[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter.FlipX))]
	public static class RestSiteFlipPatch
	{
		public static void Postfix(NRestSiteCharacter __instance)
		{
			if (__instance.GetNodeOrNull<Node2D>(RestSitePatch.BodyName) is { } body)
			{
				body.Scale = new Vector2(-body.Scale.X, body.Scale.Y);
				body.Position = new Vector2(-body.Position.X, body.Position.Y);
			}
		}
	}

	/// <summary>
	/// NMerchantCharacter drives its Spine body as child 0 (_Ready, PlayAnimation), so the body stays, hidden, and our
	/// sprite is added after it.
	/// </summary>
	[HarmonyPatch(typeof(NMerchantRoom), "AfterRoomIsLoaded")]
	public static class MerchantPatch
	{
		private static readonly AccessTools.FieldRef<NMerchantRoom, List<Player>> Players =
			AccessTools.FieldRefAccess<NMerchantRoom, List<Player>>("_players");

		/// <summary>Donor Spine scale (0.47) over its combat scale (0.29): the same height ratio for our sprite.</summary>
		private const float Scale = 1.6f;

		public static void Postfix(NMerchantRoom __instance)
		{
			List<Player> players = Players(__instance);
			IReadOnlyList<NMerchantCharacter> visuals = __instance.PlayerVisuals;
			for (int i = 0; i < players.Count && i < visuals.Count; i++)
			{
				if (players[i].Character is not Sts1Watcher)
				{
					continue;
				}
				if (visuals[i].GetChild(0) is CanvasItem spine)
				{
					spine.Visible = false;
				}
				visuals[i].AddChild(Body("Sts1WatcherBody", Scale));
			}
		}
	}

	internal const string CorpseName = "Sts1WatcherCorpse";

	/// <summary>
	/// StS1 swaps her body for a corpse image when she dies. A Spine body is hidden and the corpse added beside it
	/// (vanilla already played the death sound, as for any Spine player); a static body takes the corpse texture and
	/// the death sound is played here, since NCreature plays none without an animator. A co-op revive brings her back.
	/// </summary>
	[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
	public static class DeathPatch
	{
		public static void Postfix(NCreature __instance)
		{
			Player? player = __instance.Entity.Player;
			if (player?.Character is not Sts1Watcher)
			{
				return;
			}
			if (__instance.Visuals.SpineBody is { } spine)
			{
				if (__instance.Visuals.GetNodeOrNull(CorpseName) != null) return;
				__instance.Body.Visible = false;
				var corpse = new Sprite2D
				{
					Name = CorpseName,
					Texture = PreloadManager.Cache.GetTexture2D(WatcherAssets.Corpse),
					Centered = false,
					Offset = -new Vector2(WatcherAssets.CorpseOriginX, WatcherAssets.CorpseOriginY),
					Material = spine.GetNormalMaterial(),
				};
				__instance.Visuals.AddChild(corpse);
				__instance.Visuals.MoveChild(corpse, __instance.Body.GetIndex() + 1);
				return;
			}
			if (__instance.Body is not Sprite2D body)
			{
				return;
			}
			if (body.Texture?.ResourcePath == WatcherAssets.Corpse)
			{
				return;
			}
			body.Texture = PreloadManager.Cache.GetTexture2D(WatcherAssets.Corpse);
			body.Offset = -new Vector2(WatcherAssets.CorpseOriginX, WatcherAssets.CorpseOriginY);
			SfxCmd.PlayDeath(player);
		}
	}

	[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartReviveAnim))]
	public static class RevivePatch
	{
		public static void Postfix(NCreature __instance)
		{
			if (__instance.Entity.Player?.Character is not Sts1Watcher)
			{
				return;
			}
			if (__instance.Visuals.SpineBody != null)
			{
				__instance.Visuals.GetNodeOrNull(CorpseName)?.QueueFree();
				__instance.Body.Visible = true;
				__instance.SetAnimationTrigger(CreatureAnimator.idleTrigger);
				return;
			}
			if (__instance.Body is not Sprite2D body)
			{
				return;
			}
			body.Texture = PreloadManager.Cache.GetTexture2D(WatcherAssets.Body);
			body.Offset = -BodyOrigin;
		}
	}
}

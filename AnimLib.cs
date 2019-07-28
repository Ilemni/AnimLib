using System.Collections.Generic;
using Terraria.ModLoader;

namespace AnimLib {
#pragma warning disable CS1591
	public class AnimLib : Mod {
    public AnimLib() {
      Properties = new ModProperties() {
        Autoload = true,
      };
      Instance = this;
    }
#pragma warning restore CS1591
		internal static AnimLib Instance;
		
		internal static List<AnimationSourcePlayer> PlayerSources { get; } = new List<AnimationSourcePlayer>();
		internal static Dictionary<int, List<AnimationSourceNpc>> NpcSources { get; } = new Dictionary<int, List<AnimationSourceNpc>>();
		internal static Dictionary<int, List<AnimationSourceProjectile>> ProjSources { get; } = new Dictionary<int, List<AnimationSourceProjectile>>();
		
		internal static void Debug(object msg) => Instance.Logger.Debug(msg);

    /// <summary> Get all AnimationPlayers of a ModPlayer instance that match your Mod.
		/// 
		/// These are returned as a list in the order that your mod added them in Mod.Call().
		/// </summary>
    /// <param name="modPlayer">The ModPlayer instance you want to get the AnimationPlayer of</param>
    /// <param name="preventDraw">Prevents AnimLib from drawing the Animation's PlayerLayer.
		///
		/// Set to true if you want to manage your PlayerLayers manually. 
		/// </param>
    /// <returns>List of all AnimationPlayers your mod has created.</returns>
    public static AnimationPlayer[] GetAnimationPlayers<T>(T modPlayer, bool preventDraw=false) where T : ModPlayer {
			AnimPlayer animPlayer = modPlayer.player.GetModPlayer<AnimPlayer>();
			var l = new List<AnimationPlayer>();
			foreach (var anim in animPlayer.Anims) {
				if (anim.Source.mod != modPlayer.mod) continue;
				anim.PreventDraw = preventDraw;
				l.Add(anim);
			}
			return l.ToArray();
		}
		
		/// <summary> Get all Animation Sources via Mod.Call() </summary>
		public override void PostSetupContent() {
			PlayerSources.Clear();
      foreach(var mod in ModLoader.Mods) {
        var obj = mod.Call("LoadAnimations", "Player");
        if (obj != null) {
					Debug($"Getting Player Animations from mod {mod.Name}");
          var obj2 = obj as List<AnimationSourcePlayer>;
					obj2.ForEach(o => {
						o.mod = mod;
						Debug($"Added Player Animation {o.GetType()}");
					});
					PlayerSources.AddRange(obj2);
        }
      }
			Debug($"Player Animations count now {PlayerSources.Count}");
    }
	}
}
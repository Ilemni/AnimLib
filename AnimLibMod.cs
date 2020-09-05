using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AnimLib.Animations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AnimLib {
  /// <summary>
  /// Central place for all <see cref="IAnimationSource"/>s across all other <see cref="Mod"/>s.
  /// </summary>
	public sealed class AnimLibMod : Mod {
    /// <summary>
    /// Creates a new instance of <see cref="AnimLibMod"/>. Only for use by tModloader.
    /// </summary>
    public AnimLibMod() {
      if (Instance is null) {
        Instance = this;
      }
      else {
        throw new InvalidOperationException($"{typeof(AnimLibMod)} can only be constructed by tModLoader.");
      }
      Properties = new ModProperties() {
        Autoload = Main.netMode != NetmodeID.Server,
      };
    }

    /// <summary>
    /// The active instance of <see cref="AnimLibMod"/>.
    /// </summary>
		public static AnimLibMod Instance { get; private set; }

    internal Dictionary<Mod, IAnimationSource[]> animationSources = new Dictionary<Mod, IAnimationSource[]>();
    internal Dictionary<Mod, Type> playerAnimationDataTypes = new Dictionary<Mod, Type>();

    /// <summary>
    /// Gets the <see cref="PlayerAnimationData"/> of the given type from the given <see cref="ModPlayer"/>.
    /// Use this if you want your code to use values such as the current track and frame.
    /// <para>This <strong>cannot</strong> be used during the <see cref="ModPlayer.Initialize"/> method.</para>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="PlayerAnimationData"/> to get.</typeparam>
    /// <param name="player">The <see cref="ModPlayer"/>.</param>
    /// <returns>A <see cref="PlayerAnimationData"/> of type <typeparamref name="T"/>.</returns>
    public static T GetPlayerAnimationData<T>(ModPlayer player) where T : PlayerAnimationData => GetPlayerAnimationData<T>(player.player);

    /// <summary>
    /// Gets the <see cref="PlayerAnimationData"/> of the given type from the given <see cref="Player"/>.
    /// Use this if you want your code to use values such as the current track and frame.
    /// <para>This <strong>cannot</strong> be used during the <see cref="ModPlayer.Initialize"/> method.</para>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="PlayerAnimationData"/> to get.</typeparam>
    /// <param name="player">The <see cref="Player"/>.</param>
    /// <returns>A <see cref="PlayerAnimationData"/> of type <typeparamref name="T"/>.</returns>
    public static T GetPlayerAnimationData<T>(Player player) where T : PlayerAnimationData {
      var animPlayer = player.GetModPlayer<AnimPlayer>();
      foreach (var playerData in animPlayer.animationDatas) {
        if (playerData is T t) {
          return t;
        }
      }
      return null;
    }

    /// <summary>
    /// Use this to null static reference types on unload.
    /// </summary>
    internal static event Action OnUnload;

    /// <summary>
    /// Collects and constructs all <see cref="IAnimationSource"/>s across all other <see cref="Mod"/>s.
    /// </summary>
    public override void PostSetupContent() {
      if (Main.netMode == NetmodeID.Server) {
        animationSources = null;
        playerAnimationDataTypes = null;
        return;
      }

      animationSources = new Dictionary<Mod, IAnimationSource[]>();
      playerAnimationDataTypes = new Dictionary<Mod, Type>();
      
      foreach (var mod in ModLoader.Mods) {
        if (mod is AnimLibMod || mod.Code is null) {
          continue;
        }
        var types = from type in mod.Code.GetTypes()
                    where type.IsSubclassOfRawGeneric(typeof(AnimationSource<>)) || type.IsSubclassOf(typeof(PlayerAnimationData))
                    select type;

        if (!types.Any()) continue;

        var list = new List<IAnimationSource>();
        foreach (var type in types) {
          if (type.IsSubclassOf(typeof(PlayerAnimationData))) {
            if (playerAnimationDataTypes.ContainsKey(mod)) {
              Logger.Error($"Error collecting PlayerAnimationData from [{mod.Name}]: More than one PlayerAnimationData found. Keeping {playerAnimationDataTypes[mod].GetType()}, skipping {type.FullName}");
            }
            else {
              playerAnimationDataTypes[mod] = type;
              Logger.Info($"From mod {mod.Name} collected PlayerAnimationData \"{type.Name}\"");
            }
          }
          else {
            IAnimationSource source = null;
            try {
              source = ConstructAnimationSource(type, mod);
              if (!(source is null)) {
                list.Add(source);
                Logger.Info($"From mod {mod.Name} collected AnimationSource \"{type.Name}\"");
              }
            }
            catch (Exception ex) {
              Logger.Error($"Exception thrown when constructing AnimationSource from [{mod.Name}:{type.FullName}]", ex);
            }
          }
        }
        animationSources.Add(mod, list.ToArray());
      }
      
      if (!animationSources.Any()) {
        animationSources = null;
        playerAnimationDataTypes = null;
        Logger.Warn("AnimLibMod loaded; no mods contained any AnimationSources. Currently there is no reason for this mod to be enabled.");
      }
    }

    private IAnimationSource ConstructAnimationSource(Type type, Mod mod) {
      var initMethod = type.GetMethodExt("Initialize", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null);
      var source = initMethod?.Invoke(null, null) as IAnimationSource;
      bool doAdd = true;
      if (source.tracks is null) {
        Logger.Error($"Error constructing AnimationSource from [{mod.Name}:{type.FullName}]: Tracks is null.");
        doAdd = false;
      }
      if (source.texture is null) {
        Logger.Error($"Error constructing AnimationSource from [{mod.Name}:{type.FullName}]: Texture is null.");
        doAdd = false;
      }
      if (source.spriteSize.X == 0 || source.spriteSize.Y == 0) {
        Logger.Error($"Error constructing AnimationSource from [{mod.Name}:{type.FullName}]: Sprite Size cannot be 0 width or 0 height.");
        doAdd = false;
      }
      if (doAdd) {
        (source as IWriteMod).mod = mod;
        return source;
      }
      return null;
    }

    /// <inheritdoc/>
    public override void Unload() {
      OnUnload?.Invoke();
      OnUnload = null;
      Instance = null;
    }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using AnimLib.Animations;
using Terraria.ModLoader;

namespace AnimLib {
  /// <summary>
  /// Manages the construction and distribution of all <see cref="AnimationSource"/>s and <see cref="AnimationController"/>s.
  /// <para><strong><see cref="AnimationSource"/></strong></para>
  /// <para>On <see cref="Mod.Load"/>, all <see cref="AnimationSource"/>s are constructed.</para>
  /// <para>On <see cref="Mod.PostSetupContent"/>, all <see cref="AnimationSource"/>s have their Textures assigned.</para>
  /// 
  /// <para><strong><see cref="AnimationController"/></strong></para>
  /// <para>On <see cref="Mod.Load"/>, all <see cref="Type"/>s of <see cref="AnimationController"/> are collected.</para>
  /// <para>On <see cref="ModPlayer.Initialize"/>, all <see cref="AnimationController"/>s are constructed and added to the <see cref="AnimPlayer"/>.</para>
  /// </summary>
  internal static class AnimLoader {
    static AnimLoader() => AnimLibMod.OnUnload += Unload;

    private static void Unload() {
      animationSources = null;
      animationControllerTypes = null;
    }

    /// <summary>
    /// Whether or not to use animations during this session. Returns <see langword="true"/> if this is not run on a server; otherwise, <see langword="false"/>.
    /// </summary>
    public static bool UseAnimations => Terraria.Main.netMode != Terraria.ID.NetmodeID.Server;

    /// <summary>
    /// Collection of all <see cref="animationSources"/>, constructed during <see cref="Mod.Load"/>.
    /// </summary>
    internal static Dictionary<Mod, AnimationSource[]> animationSources { get; private set; }

    /// <summary>
    /// Collection of all <see cref="Type"/>s of <see cref="AnimationController"/>, collected during <see cref="Mod.Load"/> and constructed during <see cref="ModPlayer.Initialize"/>.
    /// </summary>
    internal static Dictionary<Mod, Type> animationControllerTypes { get; private set; }

    /// <summary>
    /// Searches all mods for any and all classes extending <see cref="AnimationSource"/> and <see cref="AnimationController"/>.
    /// <para>For <see cref="AnimationSource"/>s, they will be constructed, check for loading, log errors and skip if applicible, and added to the dict.</para>
    /// </summary>
    internal static void Load() {
      animationSources = new Dictionary<Mod, AnimationSource[]>();
      animationControllerTypes = new Dictionary<Mod, Type>();

      foreach (var mod in ModLoader.Mods) {
        if (mod is AnimLibMod || mod.Code is null) {
          continue;
        }
        var types = from type in mod.Code.GetTypes()
                    where type.IsSubclassOf(typeof(AnimationSource)) || type.IsSubclassOf(typeof(AnimationController))
                    select type;

        if (!types.Any()) continue;

        var list = GetAnimationSourcesFromTypes(types, mod);
        if (list.Count > 0) {
          var type = GetAnimationControllerTypeFromTypes(types, mod);
          if (type != null) {
            animationSources[mod] = list.ToArray();
            animationControllerTypes[mod] = type;
          }
          else {
            AnimLibMod.Instance.Logger.Error($"{mod.Name} error: {mod.Name} contains {(list.Count > 1 ? "classes" : "a class")} extending {nameof(AnimationSource)}, but does not contain any classes extending {nameof(AnimationController)}s");
          }
        }
      }

      if (!animationSources.Any() && !animationControllerTypes.Any()) {
        AnimLibMod.Instance.Logger.Info($"AnimLibMod loaded; no mods contained any {nameof(AnimationSource)}s or {nameof(AnimationController)}s. Currently there is no reason for this mod to be enabled.");
      }
    }

    /// <summary>
    /// Searches all types from the given <see cref="Mod"/> for <see cref="AnimationSource"/>, and checks if they should be included.
    /// </summary>
    private static List<AnimationSource> GetAnimationSourcesFromTypes(IEnumerable<Type> types, Mod mod) {
      List<AnimationSource> sources = new List<AnimationSource>();

      foreach (var type in types) {
        if (!type.IsSubclassOf(typeof(AnimationSource))) {
          continue;
        }
        try {
          if (TryConstructAnimationSource(type, mod, out var source)) {
            sources.Add(source);
            AnimLibMod.Instance.Logger.Info($"From mod {mod.Name} collected {nameof(AnimationSource)} \"{type.SafeTypeName(nameof(AnimationSource))}\"");
          }
        }
        catch (Exception ex) {
          AnimLibMod.Instance.Logger.Error($"Exception thrown when constructing {nameof(AnimationSource)} from [{mod.Name}:{type.FullName}]", ex);
        }
      }

      return sources;
    }

    /// <summary>
    /// Attempts to construct the animation source, and rejects any that have bad inputs.
    /// </summary>
    private static bool TryConstructAnimationSource(Type type, Mod mod, out AnimationSource source) {
      source = Activator.CreateInstance(type, true) as AnimationSource;

      string texturePath = source.GetType().FullName.Replace('.', '/');
      if (!source.Load(ref texturePath)) {
        source = null;
        return false;
      }

      bool doAdd = true;
      if (source.tracks is null) {
        AnimLibMod.Instance.Logger.Error($"Error constructing {nameof(AnimationSource)} from [{mod.Name}:{type.FullName}]: Tracks is null.");
        doAdd = false;
      }
      if (source.spriteSize.X == 0 || source.spriteSize.Y == 0) {
        AnimLibMod.Instance.Logger.Error($"Error constructing {nameof(AnimationSource)} from [{mod.Name}:{type.FullName}]: Sprite Size cannot contain a value of 0.");
        doAdd = false;
      }
      if (!ModContent.TextureExists(texturePath)) {
        AnimLibMod.Instance.Logger.Error($"Error constructing {nameof(AnimationSource)} from [{mod.Name}:{type.FullName}]: Texture path \"{texturePath}\" is not a valid texture path.");
        doAdd = false;
      }

      if (doAdd) {
        source.mod = mod;
        source.texture = ModContent.GetTexture(texturePath);
        return true;
      }
      source = null;
      return false;
    }

    /// <summary>
    /// Searches for a single type of <see cref="AnimationController"/> from the given <see cref="Mod"/>, and rejects others if more than one if found.
    /// </summary>
    private static Type GetAnimationControllerTypeFromTypes(IEnumerable<Type> types, Mod mod) {
      Type result = null;
      foreach (var type in types) {
        if (type.IsSubclassOf(typeof(AnimationController))) {
          if (result is null) {
            AnimLibMod.Instance.Logger.Info($"From mod {mod.Name} collected {nameof(AnimationController)} \"{type.SafeTypeName(nameof(AnimationController))}\"");
            result = type;
          }
          else {
            AnimLibMod.Instance.Logger.Error($"Error collecting {nameof(AnimationController)} from [{mod.Name}]: More than one {nameof(AnimationController)} found. Keeping {result.GetType().Name}, skipping {type.FullName}");
          }
        }
      }
      return result;
    }

    internal static void CreateAnimationControllersForPlayer(AnimPlayer animPlayer) {
      if (animationControllerTypes is null) {
        return;
      }

      foreach (var pair in animationControllerTypes) {
        var mod = pair.Key;
        var type = pair.Value;
        try {
          var controller = CreateAnimationControllerForPlayer(animPlayer, mod, type);
          controller.Initialize();
          animPlayer.animationControllers[mod] = controller;
        }
        catch (Exception ex) {
          AnimLibMod.Instance.Logger.Error($"Exception thrown when constructing {nameof(AnimationController)} from [{mod.Name}:{type.FullName}]", ex);
        }
      }
    }

    private static AnimationController CreateAnimationControllerForPlayer(AnimPlayer animPlayer, Mod mod, Type type) {
      var controller = Activator.CreateInstance(type, true) as AnimationController;
      controller.player = animPlayer.player;
      controller.mod = mod;

      var modSources = animationSources[mod];
      var animations = new Animation[modSources.Length];
      controller.animations = animations;

      for (int i = 0; i < modSources.Length; i++) {
        animations[i] = new Animation(controller, modSources[i]);
      }

      if (animations.Length > 0) {
        controller.SetMainAnimation(animations[0]);
      }

      if (AnimDebugCommand.DebugEnabled) {
        AnimLibMod.Instance.Logger.Debug($"{nameof(AnimationController)} for mod {mod.Name} created with {animations.Length} animations. Its MainAnimation is {controller.MainAnimation?.source.GetType().Name ?? "null"}");
      }
      return controller;
    }
  }
}

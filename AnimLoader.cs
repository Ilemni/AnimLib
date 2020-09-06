using System;
using System.Collections.Generic;
using System.Linq;
using AnimLib.Animations;
using Terraria.ModLoader;

namespace AnimLib {
  internal class AnimLoader : SingleInstance<AnimLoader> {
    public static bool UseAnimations => Terraria.Main.netMode != Terraria.ID.NetmodeID.Server;
    
    internal Dictionary<Mod, AnimationSource[]> animationSources = new Dictionary<Mod, AnimationSource[]>();
    internal Dictionary<Mod, Type> animationControllerTypes = new Dictionary<Mod, Type>();

    internal static void Load() {
      Initialize();
      var sources = Instance.animationSources;
      var controllerTypes = Instance.animationControllerTypes;

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
            sources[mod] = list.ToArray();
            controllerTypes[mod] = type;
          }
          else {
            AnimLibMod.Instance.Logger.Error($"{mod.Name} error: {mod.Name} contains {(list.Count > 1 ? "classes" : "a class")} extending {nameof(AnimationSource)}, but does not contain any classes extending {nameof(AnimationController)}s");
          }
        }
      }

      if (!Instance.animationSources.Any() && !Instance.animationControllerTypes.Any()) {
        AnimLibMod.Instance.Logger.Warn($"AnimLibMod loaded; no mods contained any {nameof(AnimationSource)}s or {nameof(AnimationController)}s. Currently there is no reason for this mod to be enabled.");
      }
    }

    internal static void PostSetupContent() {
      var sources = Instance.animationSources;
      if (sources is null) {
        return;
      }

      foreach (var modSources in sources.Values) {
        foreach (var source in modSources) {
          string texturePath = source.GetType().FullName.Replace('.', '/');
          source.Load(ref texturePath);
          if (ModContent.TextureExists(texturePath)) {
            source.texture = ModContent.GetTexture(texturePath);
          }
          else {
            AnimLibMod.Instance.Logger.Error($"Mod {source.mod.Name}: {source.GetType().Name}.Load() texturePath \"{texturePath}\" is not a valid texture path.");
          }
        }
      }
    }

    private static List<AnimationSource> GetAnimationSourcesFromTypes(IEnumerable<Type> types, Mod mod) {
      List<AnimationSource> sources = new List<AnimationSource>();

      foreach (var type in types) {
        if (!type.IsSubclassOf(typeof(AnimationSource))) {
          continue;
        }
        try {
          if (TryConstructAnimationSource(type, mod, out var source)) {
            string _ = source.GetType().FullName;
            if (source.Load(ref _)) {
              sources.Add(source);
              AnimLibMod.Instance.Logger.Info($"From mod {mod.Name} collected {nameof(AnimationSource)} \"{type.SafeTypeName(nameof(AnimationSource))}\"");
            }
            else {
              AnimLibMod.Instance.Logger.Info($"Skipped {mod.Name} {nameof(AnimationSource)} \"{type.SafeTypeName(nameof(AnimationSource))}\", Load() returned false.");
            }
          }
        }
        catch (Exception ex) {
          AnimLibMod.Instance.Logger.Error($"Exception thrown when constructing {nameof(AnimationSource)} from [{mod.Name}:{type.FullName}]", ex);
        }
      }

      return sources;
    }

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

    internal static void PlayerInitialize(AnimPlayer animPlayer) {
      var types = Instance.animationControllerTypes;
      if ((types?.Count ?? 0) > 0) {
        foreach (var pair in types) {
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
      else {
        if (AnimDebugCommand.DebugEnabled) {
          AnimLibMod.Instance.Logger.Debug("AnimPlayer instance initialized without animations.");
        }
        return;
      }
    }

    private static AnimationController CreateAnimationControllerForPlayer(AnimPlayer animPlayer, Mod mod, Type type) {
      var controller = Activator.CreateInstance(type, true) as AnimationController;
      controller.player = animPlayer.player;
      controller.mod = mod;

      var modSources = Instance.animationSources[mod];
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

    private static bool TryConstructAnimationSource(Type type, Mod mod, out AnimationSource source) {
      source = Activator.CreateInstance(type, true) as AnimationSource;
      bool doAdd = true;
      if (source.tracks is null) {
        AnimLibMod.Instance.Logger.Error($"Error constructing {nameof(AnimationSource)} from [{mod.Name}:{type.FullName}]: Tracks is null.");
        doAdd = false;
      }
      if (source.spriteSize.X == 0 || source.spriteSize.Y == 0) {
        AnimLibMod.Instance.Logger.Error($"Error constructing {nameof(AnimationSource)} from [{mod.Name}:{type.FullName}]: Sprite Size cannot be 0 width or 0 height.");
        doAdd = false;
      }
      if (doAdd) {
        source.mod = mod;
        return true;
      }
      source = null;
      return false;
    }

    internal static void OnUnload() {
      // In case other mods set a static reference to an AnimationSource, let's just clear out the dicts
      var sources = Instance.animationSources;
      if (!(sources is null)) {
        foreach (var modSources in sources.Values) {
          foreach (var source in modSources) {
            source.tracks?.Clear();
          }
        }
      }
    }
  }
}

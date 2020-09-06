using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AnimLib.Animations;
using Terraria;
using Terraria.ModLoader;

namespace AnimLib {
  internal class AnimLoader : SingleInstance<AnimLoader> {
    internal Dictionary<Mod, AnimationSource[]> animationSources = new Dictionary<Mod, AnimationSource[]>();
    internal Dictionary<Mod, Type> playerAnimationDataTypes = new Dictionary<Mod, Type>();

    internal static void Load() {
      Initialize();
      var animationSources = Instance.animationSources;
      var playerAnimationDataTypes = Instance.playerAnimationDataTypes;

      foreach (var mod in ModLoader.Mods) {
        if (mod is AnimLibMod || mod.Code is null) {
          continue;
        }
        var types = from type in mod.Code.GetTypes()
                    where type.IsSubclassOf(typeof(AnimationSource)) || type.IsSubclassOf(typeof(PlayerAnimationData))
                    select type;

        if (!types.Any()) continue;

        var list = GetAnimationSourcesFromTypes(types, mod);
        if (list.Count > 0) {
          var type = GetPlayerAnimationDataTypeFromTypes(types, mod);
          if (type != null) {
            animationSources[mod] = list.ToArray();
            playerAnimationDataTypes[mod] = type;
          }
          else {
            AnimLibMod.Instance.Logger.Error($"{mod.Name} error: {mod.Name} contains {(list.Count > 1 ? "classes" : "a class")} extending AnimationSource, but does not contain any classes extending {nameof(PlayerAnimationData)}s");
          }
        }
      }

      if (!Instance.animationSources.Any()) {
        Instance.animationSources = null;
        Instance.playerAnimationDataTypes = null;
        AnimLibMod.Instance.Logger.Warn("AnimLibMod loaded; no mods contained any AnimationSources. Currently there is no reason for this mod to be enabled.");
      }
    }

    internal static void PostSetupContent() {
      var sources = Instance.animationSources;
      if (sources is null) {
        return;
      }

      foreach (var modSources in sources.Values) {
        foreach (var source in modSources) {
          string texturePath = source.GetType().FullName;
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
        try {
          if (TryConstructAnimationSource(type, mod, out var source)) {
            string _ = source.GetType().FullName;
            if (source.Load(ref _)) {
              sources.Add(source);
              AnimLibMod.Instance.Logger.Info($"From mod {mod.Name} collected AnimationSource \"{(type.Name != "AnimationSource" ? type.Name : type.FullName)}\"");
            }
            else {
              AnimLibMod.Instance.Logger.Info($"Skipped {mod.Name} AnimationSource \"{(type.Name != "AnimationSource" ? type.Name : type.FullName)}\", Load() returned false.");
            }
          }
        }
        catch (Exception ex) {
          AnimLibMod.Instance.Logger.Error($"Exception thrown when constructing AnimationSource from [{mod.Name}:{type.FullName}]", ex);
        }
      }

      return sources;
    }

    private static Type GetPlayerAnimationDataTypeFromTypes(IEnumerable<Type> types, Mod mod) {
      Type result = null;
      foreach (var type in types) {
        if (type.IsSubclassOf(typeof(PlayerAnimationData))) {
          if (result is null) {
            AnimLibMod.Instance.Logger.Info($"From mod {mod.Name} collected PlayerAnimationData \"{(type.Name != nameof(PlayerAnimationData) ? type.Name : type.FullName)}\"");
            result = type;
          }
          else {
            AnimLibMod.Instance.Logger.Error($"Error collecting PlayerAnimationData from [{type.Name}]: More than one PlayerAnimationData found. Keeping {result.GetType()}, skipping {type.FullName}");
          }
        }
      }
      return result;
    }

    internal static void PlayerInitialize(AnimPlayer animPlayer) {
      var types = Instance.playerAnimationDataTypes;
      if ((types?.Count ?? 0) > 0) {
        foreach (var pair in types) {
          var mod = pair.Key;
          var type = pair.Value;
          try {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var constructor = type.GetConstructor(flags, null, new[] { typeof(Player), typeof(Mod) }, default);
            if (!(constructor is null)) {
              var playerData = Activator.CreateInstance(type, flags, null, args: new object[] { animPlayer.player, mod }, null) as PlayerAnimationData;
              playerData.Initialize();
              animPlayer.animationDatas[mod] = playerData;
            }
            else {
              AnimLibMod.Instance.Logger.Error($"PlayerAnimationData from [{mod.Name}:{type.FullName}] does not contain a constructor with parameters (Player, Mod).");
              continue;
            }
          }
          catch (Exception ex) {
            AnimLibMod.Instance.Logger.Error($"Exception thrown when constructing PlayerAnimationData from [{mod.Name}:{type.FullName}]", ex);
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

    private static bool TryConstructAnimationSource(Type type, Mod mod, out AnimationSource source) {
      source = Activator.CreateInstance(type, true) as AnimationSource;
      bool doAdd = true;
      if (source.tracks is null) {
        AnimLibMod.Instance.Logger.Error($"Error constructing AnimationSource from [{mod.Name}:{type.FullName}]: Tracks is null.");
        doAdd = false;
      }
      if (source.texture is null) {
        AnimLibMod.Instance.Logger.Error($"Error constructing AnimationSource from [{mod.Name}:{type.FullName}]: Texture is null.");
        doAdd = false;
      }
      if (source.spriteSize.X == 0 || source.spriteSize.Y == 0) {
        AnimLibMod.Instance.Logger.Error($"Error constructing AnimationSource from [{mod.Name}:{type.FullName}]: Sprite Size cannot be 0 width or 0 height.");
        doAdd = false;
      }
      if (doAdd) {
        source.mod = mod;
        return true;
      }
      source = null;
      return false;
    }
  }
}

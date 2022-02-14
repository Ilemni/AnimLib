using System;
using System.Collections.Generic;
using System.Linq;
using AnimLib.Abilities;
using AnimLib.Animations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Exceptions;

namespace AnimLib.Internal {
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
    /// <summary>
    /// Collection of all <see cref="Type"/>s of <see cref="AnimationController"/>, collected during <see cref="Mod.Load"/> and constructed during
    /// <see cref="ModPlayer.Initialize"/>.
    /// </summary>
    internal static Dictionary<Mod, Type> modAnimationControllerTypeDictionary;

    internal static Dictionary<Mod, Type> modAbilityManagerTypeDictionary;
    internal static Dictionary<Mod, IEnumerable<Type>> modAbilityTypeDictionary;

    /// <summary>
    /// Whether or not to use animations during this session. Returns <see langword="true"/> if this is not run on a server; otherwise,
    /// <see langword="false"/>.
    /// </summary>
    public static bool UseAnimations => Main.netMode != NetmodeID.Server;

    /// <summary>
    /// Collection of all <see cref="AnimationSources"/>, constructed during <see cref="Mod.Load"/>.
    /// </summary>
    internal static Dictionary<Mod, AnimationSource[]> AnimationSources { get; private set; }

    private static void Unload() {
      Log.LogDebug($"{nameof(AnimLoader)}.{nameof(Unload)} called.");
      AnimationSources = null;
      modAnimationControllerTypeDictionary = null;
      modAbilityManagerTypeDictionary = null;
      modAbilityTypeDictionary = null;
    }

    /// <summary>
    /// Searches all mods for any and all classes extending <see cref="AnimationSource"/> and <see cref="AnimationController"/>.
    /// <para>For <see cref="AnimationSource"/>s, they will be constructed, check for loading, log errors and skip if applicable, and added to the dict.</para>
    /// </summary>
    internal static void Load() {
      AnimLibMod.OnUnload += Unload;

      AnimationSources = new Dictionary<Mod, AnimationSource[]>();
      modAnimationControllerTypeDictionary = new Dictionary<Mod, Type>();
      modAbilityManagerTypeDictionary = new Dictionary<Mod, Type>();
      modAbilityTypeDictionary = new Dictionary<Mod, IEnumerable<Type>>();

      foreach (Mod mod in ModLoader.Mods) {
        if (CanLoadMod(mod, out var types))
          LoadMod(mod, types);
      }

      if (!AnimationSources.Any() && !modAnimationControllerTypeDictionary.Any())
        Log.LogInfo("AnimLibMod loaded; no mods contained Animations or Abilities. Currently there is no reason for this mod to be enabled.");
    }

    private static bool CanLoadMod(Mod mod, out Type[] types) {
      if (mod is AnimLibMod || mod.Code is null) {
        types = null;
        return false;
      }

      types = (from t in mod.Code.GetTypes()
        where !t.IsAbstract &&
              (t.IsSubclassOf(typeof(AnimationSource)) ||
               t.IsSubclassOf(typeof(AnimationController)) ||
               t.IsSubclassOf(typeof(AbilityManager)) ||
               t.IsSubclassOf(typeof(Ability)))
        select t).ToArray();
      return types.Any();
    }

    private static void LoadMod(Mod mod, Type[] types) {
      if (UseAnimations) AnimationLoader.Load(mod, types);

      AbilityLoader.Load(mod, types);
    }
  }

  internal static class AnimationLoader {
    public static void Load(Mod mod, Type[] types) {
      if (GetSourcesFromTypes(types, mod, out var sources)) {
        if (GetControllerTypeFromTypes(types, mod, out Type controllerType)) {
          AnimLoader.AnimationSources[mod] = sources.ToArray();
          AnimLoader.modAnimationControllerTypeDictionary[mod] = controllerType;
        }
        else {
          Log.LogWarning(
            $"{mod.Name} error: {mod.Name} contains {(sources.Count > 1 ? "classes" : "a class")} extending {nameof(AnimationSource)}, but does not contain any classes extending {nameof(AnimationController)}s");
        }
      }
    }

    /// <summary>
    /// Searches all types from the given <see cref="Mod"/> for <see cref="AnimationSource"/>, and checks if they should be included.
    /// </summary>
    private static bool GetSourcesFromTypes(IEnumerable<Type> types, Mod mod, out List<AnimationSource> sources) {
      sources = new List<AnimationSource>();
      foreach (Type type in types) {
        if (!type.IsSubclassOf(typeof(AnimationSource))) continue;

        if (!TryConstructSource(type, mod, out AnimationSource source)) continue;
        sources.Add(source);
        Log.LogInfo($"[{mod.Name}]: Collected {nameof(AnimationSource)} \"{type.UniqueTypeName()}\"");
      }

      return sources.Any();
    }

    /// <summary>
    /// Searches for a single type of <see cref="AnimationController"/> from the given <see cref="Mod"/>, and rejects others if more than one if found.
    /// </summary>
    private static bool GetControllerTypeFromTypes(IEnumerable<Type> types, Mod mod, out Type result) {
      result = null;
      foreach (Type type in types) {
        if (!type.IsSubclassOf(typeof(AnimationController))) continue;
        if (!(result is null))
          throw new CustomModDataException(mod, $"Cannot have more than one {nameof(AnimationController)} per mod.",
            null);

        Log.LogInfo($"[{mod.Name}]: Collected {nameof(AnimationController)} \"{type.UniqueTypeName()}\"");
        result = type;
      }

      return result != null;
    }


    /// <summary>
    /// Attempts to construct the animation source, and rejects any that have bad inputs.
    /// </summary>
    private static bool TryConstructSource(Type type, Mod mod, out AnimationSource source) {
      source = (AnimationSource)Activator.CreateInstance(type, true);

      string fullName = source.GetType().FullName;
      if (fullName is null) throw new ArgumentException($"Invalid full type name from type {source.GetType()}", nameof(type));

      string texturePath = fullName.Replace('.', '/');
      if (!source.Load(ref texturePath)) {
        source = null;
        return false;
      }

      if (source.tracks is null)
        throw new Exception($"[{mod.Name}:{type.FullName}]: Error constructing {type.Name}: Tracks cannot be null.");

      if (source.spriteSize.x == 0 || source.spriteSize.y == 0)
        throw new Exception($"[{mod.Name}:{type.FullName}]: Error constructing {type.Name}: Sprite Size cannot contain a value of 0.");

      if (!ModContent.TextureExists(texturePath))
        throw new MissingResourceException($"[{mod.Name}:{type.FullName}]: Error constructing {type.Name}: Invalid texture path \"{texturePath}\".");

      source.mod = mod;
      source.texture = ModContent.GetTexture(texturePath);
      return true;
    }

    internal static void CreateControllersForPlayer(AnimPlayer animPlayer) {
      foreach ((Mod mod, Type type) in AnimLoader.modAnimationControllerTypeDictionary) {
        try {
          animPlayer.animationControllers[mod] = CreateControllerForPlayer(animPlayer, mod, type);
        }
        catch (Exception ex) {
          Log.LogError($"Exception thrown when constructing {nameof(AnimationController)} from [{mod.Name}:{type.FullName}]", ex);
          throw;
        }
      }
    }

    private static AnimationController CreateControllerForPlayer(AnimPlayer animPlayer, Mod mod, Type type) {
      AnimationController controller = (AnimationController)Activator.CreateInstance(type, true);
      controller.player = animPlayer.player;
      controller.mod = mod;
      controller.SetupAnimations();
      controller.Initialize();

      Log.LogDebug(
        $"{nameof(AnimationController)} for mod {mod.Name} created with {controller.animations.Length} animations. " +
        $"{(controller.animations[0] != null ? $"Its MainAnimation is {controller.MainAnimation.source.GetType().Name}" : "It has no MainAnimation.")}");

      return controller;
    }
  }

  internal static class AbilityLoader {
    public static void Load(Mod mod, Type[] types) {
      if (GetAbilityTypesFromTypes(types, mod, out var abilityTypes)) {
        AnimLoader.modAbilityTypeDictionary[mod] = abilityTypes;
        if (GetAbilityManagerTypeFromTypes(types, mod, out Type managerType)) AnimLoader.modAbilityManagerTypeDictionary[mod] = managerType;
      }
    }

    private static bool GetAbilityManagerTypeFromTypes(IEnumerable<Type> types, Mod mod, out Type type) {
      type = null;
      foreach (Type t in types) {
        if (!t.IsSubclassOf(typeof(AbilityManager))) continue;
        if (!(type is null)) throw new CustomModDataException(mod, $"Cannot have more than one {nameof(AbilityManager)} per mod.", null);

        Log.LogInfo($"[{mod.Name}]: Collected {nameof(AbilityManager)} \"{t.UniqueTypeName()}\"");
        type = t;
      }

      return type != null;
    }

    private static bool GetAbilityTypesFromTypes(IEnumerable<Type> types, Mod mod, out List<Type> abilityTypes) {
      abilityTypes = new List<Type>();
      foreach (Type type in types) {
        if (!type.IsSubclassOf(typeof(Ability))) continue;
        abilityTypes.Add(type);
        Log.LogInfo($"From mod {mod.Name} collected {nameof(AnimationSource)} \"{type.UniqueTypeName()}\"");
      }

      return abilityTypes.Any();
    }

    public static void CreateAbilityManagersForPlayer(AnimPlayer animPlayer) {
      foreach ((Mod mod, var abilityTypes) in AnimLoader.modAbilityTypeDictionary) {
        Type managerType = AnimLoader.modAbilityManagerTypeDictionary.TryGetValue(mod, out Type t) ? t : typeof(AbilityManager);
        try {
          animPlayer.abilityManagers[mod] = CreateAbilityManagerForPlayer(animPlayer, mod, managerType, abilityTypes);
        }
        catch (Exception ex) {
          Log.LogError($"Exception thrown when constructing {nameof(AbilityManager)} from [{mod.Name}:{managerType.FullName}]", ex);
          throw;
        }
      }
    }

    private static AbilityManager CreateAbilityManagerForPlayer(AnimPlayer animPlayer, Mod mod, Type managerType, IEnumerable<Type> abilityTypes) {
      AbilityManager manager = (AbilityManager)Activator.CreateInstance(managerType);
      manager.mod = mod;
      manager.animPlayer = animPlayer;
      manager.player = animPlayer.player;

      if (manager.Autoload) {
        var list = new List<Ability>();
        foreach (Type abilityType in abilityTypes) {
          if (AutoloadAbility(abilityType, manager, out Ability ability))
            list.Add(ability);
        }

        list.Sort((a1, a2) => a1.Id.CompareTo(a2.Id));
        manager.abilityArray = list.ToArray();
      }

      InitializeAbilityManager(manager);
      return manager;
    }

    private static bool AutoloadAbility(Type abilityType, AbilityManager manager, out Ability ability) {
      ability = (Ability)Activator.CreateInstance(abilityType);
      ability.abilities = manager;
      ability.player = manager.player;
      return ability.Autoload;
    }

    private static void InitializeAbilityManager(AbilityManager manager) {
      manager.Initialize();
      // ReSharper disable once ConditionIsAlwaysTrueOrFalse, HeuristicUnreachableCode
      if (manager.abilityArray is null) return;
      foreach (Ability ability in manager.abilityArray) {
        ability.Initialize();
      }
    }
  }
}

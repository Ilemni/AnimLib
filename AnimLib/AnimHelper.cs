using System;
using System.Linq;
using AnimLib.Abilities;
using AnimLib.Animations;
using AnimLib.Internal;
using Terraria.ModLoader;

namespace AnimLib {
  internal static class AnimHelper {
    public static Mod GetModFromController(Type type) =>
      AnimLoader.modAnimationControllerTypeDictionary.FirstOrDefault(x => x.Value == type).Key;

    public static Mod GetModFromSource(Type type) =>
      AnimLoader.AnimationSources.FirstOrDefault(x => x.Value.Any(y => y.GetType() == type)).Key;

    public static Mod GetModFromManager(Type type) =>
      AnimLoader.modAbilityManagerTypeDictionary.FirstOrDefault(x => x.Value == type).Key;

    public static Mod GetModFromAbility(Type type) =>
      AnimLoader.modAbilityTypeDictionary.FirstOrDefault(x => x.Value.Contains(type)).Key;

    public static Mod GetModFromController<T>() where T : AnimationController => GetModFromController(typeof(T));

    public static Mod GetModFromSource<T>() where T : AnimationSource => GetModFromSource(typeof(T));

    public static Mod GetModFromManager<T>() where T : AbilityManager => GetModFromManager(typeof(T));

    public static Mod GetModFromAbility<T>() where T : Ability => GetModFromAbility(typeof(T));
  }
}

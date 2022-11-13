using System;
using Terraria.ModLoader;

namespace AnimLib {
  internal static class ThrowHelper {
    public static ArgumentException BadType<TExpected>(object badType, Mod mod) =>
      new($"{badType.GetType()} is not a valid type of {typeof(TExpected)} in Mod {mod.Name}");

    public static ArgumentException BadType<TExpected>(object badType, Mod mod, string paramName) =>
      new($"{badType.GetType()} is not a valid type of {typeof(TExpected)} in Mod {mod.Name}", paramName);

    public static ArgumentException NoType(Mod mod) =>
      new($"Mod {mod.Name} has no AnimLib types.");
  }
}

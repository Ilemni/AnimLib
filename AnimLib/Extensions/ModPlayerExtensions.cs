using AnimLib.Abilities;
using AnimLib.Animations;
using JetBrains.Annotations;
using Terraria.ModLoader;

namespace AnimLib.Extensions {
  /// <summary>
  /// Contains extension methods for the <see cref="ModPlayer"/> class.
  /// </summary>
  [PublicAPI]
  public static class ModPlayerExtensions {
    /// <summary>
    /// Gets the <see cref="AnimCharacter"/> instance that belongs to the <see cref="Mod"/> of <paramref name="modPlayer"/>.
    /// </summary>
    /// <param name="modPlayer">Your <see cref="ModPlayer"/> instance.</param>
    /// <returns>The <see cref="AnimCharacter"/> instance of <paramref name="modPlayer"/> for your <see cref="Mod"/></returns>
    [NotNull]
    public static AnimCharacter GetAnimCharacter(this ModPlayer modPlayer) {
      AnimPlayer animPlayer = modPlayer.player.GetModPlayer<AnimPlayer>();
      return animPlayer.characters.TryGetValue(modPlayer.mod, out AnimCharacter c) ? c : throw ThrowHelper.NoType(modPlayer.mod);
    }

    /// <summary>
    /// Gets a wrapped <see cref="AnimCharacter{T, T}"/> instance that belongs to the <see cref="Mod"/> of <paramref name="modPlayer"/>.
    /// </summary>
    /// <param name="modPlayer">Your <see cref="ModPlayer"/> instance.</param>
    /// <returns>An <see cref="AnimCharacter{T, T}"/> instance of <paramref name="modPlayer"/> for your <see cref="Mod"/></returns>
    public static AnimCharacter<TAnimation, TAbility> GetAnimCharacter<TAnimation, TAbility>(this ModPlayer modPlayer)
      where TAnimation : AnimationController where TAbility : AbilityManager
      => GetAnimCharacter(modPlayer).As<TAnimation, TAbility>();
  }
}

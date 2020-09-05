using Terraria.ModLoader;

namespace AnimLib.Animations {
  /// <summary>
  /// Internal interface for writing of the <see cref="mod"/> property.
  /// </summary>
  internal interface IWriteMod {
    /// <inheritdoc cref="IAnimationSource.mod"/>
    Mod mod { set; }
  }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace AnimLib.Animations {
  /// <summary>
  /// Contains all animation data for a single animation set. This animation data is treated as a database, and is used for all players. 
  /// </summary>
  public interface IAnimationSource {
    /// <summary>
    /// Size of all sprites in the spritesheet.
    /// </summary>
    PointByte spriteSize { get; }

    /// <summary>
    /// All <see cref="Track"/>s in the animation set.
    /// </summary>
    Dictionary<string, Track> tracks { get; }

    /// <summary>
    /// Default spritesheet used for animations.
    /// </summary>
    Texture2D texture { get; }

    /// <summary>
    /// The mod that this <see cref="IAnimationSource"/> belongs to.
    /// </summary>
    Mod mod { get; }

    /// <summary>
    /// Shorthand for accessing <see cref="tracks"/>.
    /// </summary>
    Track this[string name] { get; }
  }
}

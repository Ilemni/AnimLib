using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace AnimLib.Animations {
  /// <summary>
  /// Contains all animation data for a single animation set. This animation data is used for all players. 
  /// <see cref="AnimationSource"/>s from all mods are collected and created during <see cref="AnimLibMod.PostSetupContent"/>.
  /// After initialization, values should not be modified.
  /// </summary>
  public abstract class AnimationSource {
    /// <summary>
    /// Base constructor. Ensures that this is not constructed on a server.
    /// </summary>
    /// <exception cref="InvalidOperationException">Animation classes are not allowed to be constructed on servers.</exception>
    private AnimationSource() {
      if (!AnimLoader.UseAnimations) {
        throw new InvalidOperationException($"{GetType().Name} is not allowed to be constructed on servers.");
      }
    }

    /// <inheritdoc/>
    public abstract PointByte spriteSize { get; }

    /// <inheritdoc/>
    public abstract Dictionary<string, Track> tracks { get; }

    /// <inheritdoc/>
    public Texture2D texture { get; internal set; }

    /// <inheritdoc/>
    public Mod mod { get; internal set; }

    /// <inheritdoc/>
    public Track this[string name] => tracks[name];


    /// <summary>
    /// Whether or not this <see cref="AnimationSource"/> should be used. Return <see langword="false"/> to prevent this from being used.
    /// Returns <see langword="true"/> by default.
    /// </summary>
    /// <param name="texturePath">The file name of this <see cref="AnimationSource"/>'s texture file in the mod loader's file space.</param>
    /// <returns></returns>
    public virtual bool Load(ref string texturePath) => true;

    /// <summary>
    /// <para>Shorthand for <see cref="Frame(byte, byte, ushort)"/></para>
    /// <inheritdoc cref="Frame(int, int, int)"/>
    /// </summary>
    /// <inheritdoc cref="Frame(int, int, int)"/>
    protected static Frame F(int x, int y, int duration = 0) => new Frame((byte)x, (byte)y, (ushort)duration);
  }
}

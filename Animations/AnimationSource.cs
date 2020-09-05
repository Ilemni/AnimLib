using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AnimLib.Animations {
  /// <summary>
  /// Contains all animation data for a single animation set. This animation data is used for all players. 
  /// <see cref="AnimationSource{T}"/>s from all mods are collected and created during <see cref="AnimLibMod.PostSetupContent"/>.
  /// After initialization, values should not be modified.
  /// </summary>
  public abstract class AnimationSource<T> : SingleInstance<T>, IAnimationSource, IWriteMod where T : AnimationSource<T> {
    /// <summary>
    /// Base constructor. Ensures that this is not constructed on a server.
    /// </summary>
    /// <exception cref="InvalidOperationException">Animation classes are not allowed to be constructed on servers.</exception>
    protected AnimationSource() {
      if (Main.netMode == NetmodeID.Server) {
        throw new InvalidOperationException($"{GetType().Name} is not allowed to be constructed on servers.");
      }
    }

    /// <inheritdoc/>
    public abstract PointByte spriteSize { get; }

    /// <inheritdoc/>
    public abstract Dictionary<string, Track> tracks { get; }

    /// <inheritdoc/>
    public abstract Texture2D texture { get; }

    /// <inheritdoc/>
    public Mod mod { get; private set; }
    Mod IWriteMod.mod { set => mod = value; }

    /// <inheritdoc/>
    public Track this[string name] => tracks[name];

    /// <summary>
    /// <para>Shorthand for <see cref="Frame(byte, byte, ushort)"/></para>
    /// <inheritdoc cref="Frame(int, int, int)"/>
    /// </summary>
    /// <inheritdoc cref="Frame(int, int, int)"/>
    protected static Frame F(int x, int y, int duration = 0) => new Frame((byte)x, (byte)y, (ushort)duration);
  }
}

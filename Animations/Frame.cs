using System;
using Microsoft.Xna.Framework.Graphics;

namespace AnimLib.Animations {
  /// <inheritdoc cref="IFrame"/>
  public readonly struct Frame : IFrame {
    /// <summary>
    /// Creates a <see cref="Frame"/> with the given X and Y position, and frame duration to play. These values will be cast to smaller data types.
    /// </summary>
    /// <param name="x">X position of the tile. This will be cast to a <see cref="byte"/>.</param>
    /// <param name="y">Y position of the tile. This will be cast to a <see cref="byte"/>.</param>
    /// <param name="duration">Duration of the frame. This will be cast to a <see cref="ushort"/>.</param>
    public Frame(int x, int y, int duration = 0) : this((byte)x, (byte)y, (ushort)duration) { }

    /// <summary>
    /// Creates a <see cref="Frame"/> with the given X and Y position, and frame duration to play.
    /// </summary>
    /// <param name="x">X position of the tile.</param>
    /// <param name="y">Y position of the tile.</param>
    /// <param name="duration">Duration of the frame.</param>
    public Frame(byte x, byte y, ushort duration = 0) {
      tile = new PointByte(x, y);
      this.duration = duration;
    }

    /// <inheritdoc/>
    public PointByte tile { get; }

    /// <inheritdoc/>
    public ushort duration { get; }

    /// <summary>
    /// For a <see cref="Track"/>, adds another <see cref="Texture2D"/> to the track, and switches to that texture when this track is played.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="texturePath"/> is <see langword="null"/> or empty.</exception>
    public SwitchTextureFrame WithTexture(string texturePath) => new SwitchTextureFrame(tile.X, tile.Y, duration, texturePath);

    /// <summary>
    /// Returns a <see cref="string"/> containing the X and Y value of the <see cref="tile"/>, and the <see cref="duration"/> of this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> containing the X and Y value of the <see cref="tile"/>, and the <see cref="duration"/>.</returns>
    public override string ToString() => $"x:{tile.X}, y:{tile.Y}, duration:{duration}";

    /// <inheritdoc cref="Frame(byte, byte, ushort)"/>
    public static explicit operator Frame(SwitchTextureFrame stf) => new Frame(stf.tile.X, stf.tile.Y, stf.duration);
  }
}

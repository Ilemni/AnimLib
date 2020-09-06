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
    /// <remarks>
    /// This should only ever be used if a single <strong><see cref="Track"/></strong> on its own needs to use more than one spritesheet.
    /// This does not apply to cases where an <see cref="AnimationSource"/> needs more than one <see cref="Texture2D"/> for its <see cref="Track"/>s, but rather,
    /// when a single <see cref="Track"/> needs more than one <see cref="Texture2D"/>.
    /// If all of one <see cref="Track"/> can fit on a 2048x2048 spritesheet, use <see cref="Track.WithTexture(string)"/> instead.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="texturePath"/> is <see langword="null"/> or empty.</exception>
    public SwitchTextureFrame WithNextSpritesheet(string texturePath) {
      if (string.IsNullOrWhiteSpace(texturePath)) {
        throw new ArgumentException("message", nameof(texturePath));
      }

      return new SwitchTextureFrame(tile.X, tile.Y, duration, texturePath);
    }

    /// <summary>
    /// Returns a <see cref="string"/> containing the X and Y value of the <see cref="tile"/>, and the <see cref="duration"/> of this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> containing the X and Y value of the <see cref="tile"/>, and the <see cref="duration"/>.</returns>
    public override string ToString() => $"x:{tile.X}, y:{tile.Y}, duration:{duration}";
  }
}

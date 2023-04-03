using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace AnimLib.Animations {
  /// <summary>
  /// Single frame of animation. Stores sprite position on the sprite sheet, and duration of the frame.
  /// </summary>
  [PublicAPI]
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

    /// <summary>
    /// The X and Y position of the tile.
    /// </summary>
    public PointByte tile { get; }

    /// <inheritdoc/>
    public ushort duration { get; }

    /// <summary>
    /// Gets a <see cref="Rectangle"/> that represents the sprite in the <see cref="AnimationSource"/>.
    /// </summary>
    /// <param name="source">The <see cref="AnimationSource"/>.</param>
    /// <returns></returns>
    public Rectangle ToRectangle(AnimationSource source) {
      PointByte size = source.spriteSize;
      return new Rectangle(tile.x * size.x, tile.y * size.y, size.x, size.y);
    }

    /// <summary>
    /// Returns a <see cref="string"/> containing the X and Y value of the <see cref="tile"/>, and the <see cref="duration"/> of this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> containing the X and Y value of the <see cref="tile"/>, and the <see cref="duration"/>.</returns>
    public override string ToString() => $"x:{tile.x}, y:{tile.y}, duration:{duration}";

    /// <inheritdoc cref="Frame(byte, byte, ushort)"/>
    public static explicit operator Frame(SwitchTextureFrame stf) => new(stf.tile.x, stf.tile.y, stf.duration);
  }
}

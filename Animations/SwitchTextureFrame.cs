using Microsoft.Xna.Framework.Graphics;

namespace AnimLib.Animations {
  /// <summary>
  /// Single frame of animation that switches to another spritesheet. Stores sprite position on the sprite sheet, duration of the frame, and the next spritesheet to use.
  /// </summary>
  /// <remarks>
  /// This should only ever be used if a single <strong><see cref="Track"/></strong> needs to use more than one spritesheet.
  /// If all of one <see cref="Track"/> can fit on a 2048x2048 track, use that instead.
  /// </remarks>
  public readonly struct SwitchTextureFrame : IFrame {
    /// <summary>
    /// Creates a <see cref="Frame"/> with the given X and Y position, frame duration, and spritesheet. These values will be cast to smaller data types.
    /// </summary>
    /// <param name="x">X position of the tile. This will be cast to a <see cref="byte"/>.</param>
    /// <param name="y">Y position of the tile. This will be cast to a <see cref="byte"/>.</param>
    /// <param name="duration">Duration of the frame. This will be cast to a <see cref="ushort"/>.</param>
    /// /// <param name="texture">Spritesheet that this track will switch to.</param>
    public SwitchTextureFrame(int x, int y, int duration, Texture2D texture) : this((byte)x, (byte)y, (ushort)duration, texture) { }

    /// <summary>
    /// Creates a <see cref="SwitchTextureFrame"/> with the given X and Y position, frame duration, and spritesheet.
    /// </summary>
    /// <param name="x">X position of the tile.</param>
    /// <param name="y">Y position of the tile.</param>
    /// <param name="duration">Duration of the frame.</param>
    /// <param name="texture">Spritesheet that this track will switch to upon reaching this frame.</param>
    public SwitchTextureFrame(byte x, byte y, ushort duration, Texture2D texture) {
      tile = new PointByte(x, y);
      this.duration = duration;
      this.texture = texture;
    }

    /// <inheritdoc/>
    public PointByte tile { get; }

    /// <inheritdoc/>
    public ushort duration { get; }

    /// <summary>
    /// Spritesheet this frame will switch to.
    /// </summary>
    public Texture2D texture { get; }

    /// <summary>
    /// Returns a <see cref="string"/> containing the X and Y value of the <see cref="tile"/>, the <see cref="duration"/>, and the name of <see cref="texture"/> of this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> containing the X and Y value of the <see cref="tile"/>, the <see cref="duration"/>, and the name of <see cref="texture"/>.</returns>
    public override string ToString() => $"x:{tile.X}, y:{tile.Y}, duration:{duration}, texture:{texture.Name}";
  }
}

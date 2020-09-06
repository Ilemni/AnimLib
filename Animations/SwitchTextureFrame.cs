using System;

namespace AnimLib.Animations {
  /// <summary>
  /// Single frame of animation that switches to another spritesheet. Stores sprite position on the sprite sheet, duration of the frame, and the next spritesheet to use.
  /// </summary>
  public readonly struct SwitchTextureFrame : IFrame {
    /// <summary>
    /// Creates a <see cref="Frame"/> with the given X and Y position, frame duration, and spritesheet. These values will be cast to smaller data types.
    /// </summary>
    /// <param name="x">X position of the tile. This will be cast to a <see cref="byte"/>.</param>
    /// <param name="y">Y position of the tile. This will be cast to a <see cref="byte"/>.</param>
    /// <param name="duration">Duration of the frame. This will be cast to a <see cref="ushort"/>.</param>
    /// <param name="texturePath">Spritesheet that this track will switch to.</param>
    /// <exception cref="ArgumentException"><paramref name="texturePath"/> is <see langword="null"/> or white space.</exception>
    public SwitchTextureFrame(int x, int y, int duration, string texturePath) : this((byte)x, (byte)y, (ushort)duration, texturePath) { }

    /// <summary>
    /// Creates a <see cref="SwitchTextureFrame"/> with the given X and Y position, frame duration, and spritesheet.
    /// </summary>
    /// <param name="x">X position of the tile.</param>
    /// <param name="y">Y position of the tile.</param>
    /// <param name="duration">Duration of the frame.</param>
    /// <param name="texturePath">Spritesheet that this track will switch to upon reaching this frame.</param>
    /// <exception cref="ArgumentException"><paramref name="texturePath"/> is <see langword="null"/> or white space.</exception>
    public SwitchTextureFrame(byte x, byte y, ushort duration, string texturePath) {
      if (string.IsNullOrWhiteSpace(texturePath)) {
        throw new ArgumentException("message", nameof(texturePath));
      }

      tile = new PointByte(x, y);
      this.duration = duration;
      this.texturePath = texturePath;
    }

    /// <inheritdoc/>
    public PointByte tile { get; }

    /// <inheritdoc/>
    public ushort duration { get; }

    /// <summary>
    /// Spritesheet this frame will switch to.
    /// </summary>
    public string texturePath { get; }

    /// <summary>
    /// Returns a <see cref="string"/> containing the X and Y value of the <see cref="tile"/>, the <see cref="duration"/>, and the name of <see cref="texturePath"/> of this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> containing the X and Y value of the <see cref="tile"/>, the <see cref="duration"/>, and the name of <see cref="texturePath"/>.</returns>
    public override string ToString() => $"x:{tile.X}, y:{tile.Y}, duration:{duration}, texturePath:{texturePath}";
  }
}

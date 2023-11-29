using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace AnimLib.Animations {
  /// <summary>
  /// Single frame of animation that switches to another spritesheet.
  /// Stores sprite position on the sprite sheet, duration of the frame, and the next spritesheet to use.
  /// </summary>
  [PublicAPI]
  public readonly struct SwitchTextureFrameAsset : IFrame {
    /// <summary>
    /// Creates a <see cref="SwitchTextureFrame"/> with the given X and Y position, frame duration, and spritesheet.
    /// These values will be cast to smaller data types.
    /// </summary>
    /// <param name="x">X position of the tile. This will be cast to a <see cref="byte"/>.</param>
    /// <param name="y">Y position of the tile. This will be cast to a <see cref="byte"/>.</param>
    /// <param name="duration">Duration of the frame. This will be cast to a <see cref="ushort"/>.</param>
    /// <param name="textureAsset">
    /// Spritesheet that this track will switch to upon reaching this frame,
    /// -or- <see langword="null"/> to use the <see cref="AnimationSource"/>'s texture.
    /// The first texture replacement cannot be a <see langword="null"/> value.
    /// </param>
    public SwitchTextureFrameAsset(int x, int y, int duration, [CanBeNull] Asset<Texture2D> textureAsset) : this((byte)x, (byte)y, (ushort)duration, textureAsset) { }

    /// <summary>
    /// Creates a <see cref="SwitchTextureFrame"/> with the given X and Y position, frame duration, and spritesheet.
    /// </summary>
    /// <param name="x">X position of the tile.</param>
    /// <param name="y">Y position of the tile.</param>
    /// <param name="duration">Duration of the frame.</param>
    /// <param name="textureAsset">
    /// Spritesheet that this track will switch to upon reaching this frame, -or- <see langword="null"/> to use the
    /// <see cref="AnimationSource"/>'s texture.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="textureAsset"/> is <see langword="null"/> or white space.</exception>
    public SwitchTextureFrameAsset(byte x, byte y, ushort duration, [CanBeNull] Asset<Texture2D> textureAsset) {
      tile = new PointByte(x, y);
      this.duration = duration;
      this.textureAsset = textureAsset;
    }

    /// <inheritdoc/>
    public PointByte tile { get; }

    /// <inheritdoc/>
    public ushort duration { get; }

    /// <summary>
    /// Spritesheet this frame will switch to.
    /// </summary>
    public Asset<Texture2D> textureAsset { get; }

    /// <summary>
    /// Returns a <see cref="string"/> containing the X and Y value of the <see cref="tile"/>,
    /// the <see cref="duration"/>, and the name of this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> containing the X and Y value of the <see cref="tile"/>,
    /// the <see cref="duration"/>, and the name.
    /// </returns>
    public override string ToString() =>
      $"x:{tile.x}, y:{tile.y}, duration:{duration}, texture{(textureAsset is null ? " is null" : ": " + textureAsset.Name)}";
  }
}

namespace AnimLib.Animations {
  /// <summary>
  /// Single frame of animation. Stores sprite position on the sprite sheet, and duration of the frame.
  /// This may be either a <see cref="Frame"/> or <see cref="SwitchTextureFrame"/>.
  /// </summary>
  public interface IFrame {
    /// <summary>
    /// Position of the tile, in sprite-space.
    /// </summary>
    PointByte tile { get; }

    /// <summary>
    /// Duration of the tile. If this value is <see langword="0"/>, the animation will stay on this frame.
    /// </summary>
    ushort duration { get; }
  }
}

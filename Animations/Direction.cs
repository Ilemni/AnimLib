namespace AnimLib.Animations {
  /// <summary>
  /// Used to determine the direction that frames in a track are played.
  /// </summary>
  public enum Direction : byte {
    /// <summary>
    /// Frames are played forward.
    /// </summary>
    Forward = 0,
    /// <summary>
    /// Frames alternate between playing forward and backwards when reaching their last frames.
    /// </summary>
    PingPong = 1,
    /// <summary>
    /// Frames are played backwards.
    /// </summary>
    Reverse = 2,
  }
}

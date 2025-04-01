using System.Diagnostics;

namespace AnimLib.Animations;

/// <summary>
/// Represents one frame of animation as defined in the Aseprite file.
/// </summary>
[DebuggerDisplay("FrameIndex = {AtlasFrameIndex}, Duration = {Duration.ToString(\"F3\")}")]
public readonly struct AnimFrame {
  /// <summary>
  /// Index of the frame, as defined in the Aseprite file.
  /// </summary>
  public readonly int AtlasFrameIndex;

  /// <summary>
  /// Duration of the frame, in seconds, as defined in the Aseprite file.
  /// </summary>
  public readonly float Duration;

  /// <summary>
  /// Represents one frame of animation as defined in the Aseprite file.
  /// </summary>
  public AnimFrame(int atlasFrameIndex, TimeSpan duration) {
    AtlasFrameIndex = atlasFrameIndex;
    Duration = (float)duration.TotalSeconds;
  }
}

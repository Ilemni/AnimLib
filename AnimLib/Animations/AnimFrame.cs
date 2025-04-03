using System.Diagnostics;

namespace AnimLib.Animations;

/// <summary>
/// Represents one frame of animation as defined in the Aseprite file.
/// </summary>
[DebuggerDisplay("FrameIndex = {AtlasFrameIndex}, Duration = {Duration.ToString(\"F3\")}, UserData = ({UserData})")]
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
  /// UserData representing the frame, as defined in the Aseprite file.
  /// </summary>
  /// <remarks>
  /// Since Aseprite doesn't support UserData in frames, this value is instead taken from a cel of the same index,
  /// from a layer named "data" in the Aseprite file.
  /// </remarks>
  public readonly AnimUserData UserData;

  /// <summary>
  /// Represents one frame of animation as defined in the Aseprite file.
  /// </summary>
  public AnimFrame(int atlasFrameIndex, TimeSpan duration, AnimUserData userData) {
    (AtlasFrameIndex, Duration, UserData) = (atlasFrameIndex, (float)duration.TotalSeconds, userData);
  }
}

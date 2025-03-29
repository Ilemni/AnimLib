using System.Diagnostics;

namespace AnimLib.Animations;

/// <summary>
/// Represents one frame of animation as defined in the Aseprite file.
/// </summary>
[DebuggerDisplay("FrameIndex = {AtlasFrameIndex}, Duration = {Duration.ToString(\"F3\")}")]
public readonly record struct AnimFrame(int AtlasFrameIndex, float Duration) {
  /// <summary>
  /// Index of the frame, as defined in the Aseprite file.
  /// </summary>
  public readonly int AtlasFrameIndex = AtlasFrameIndex;

  /// <summary>
  /// Duration of the frame, in seconds, as defined in the Aseprite file.
  /// </summary>
  public readonly float Duration = Duration;

  public static AnimFrame FromAse(AsepriteDotNet.AnimationFrame aseFrame) {
    return new AnimFrame(aseFrame.FrameIndex, (float)aseFrame.Duration.TotalSeconds);
  }
}

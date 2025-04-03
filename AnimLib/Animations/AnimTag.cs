using System.Diagnostics;
using System.Linq;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;

namespace AnimLib.Animations;

// AnimLib copy of AsepriteDotNet class so that mods dependent on AnimLib don't need to access AsepriteDotNet assembly
/// <summary>
/// Represents on animation tag as defined in the Aseprite file.
/// </summary>
[DebuggerDisplay("Name = {Name}, FrameCount = {Frames.Length}, UserData = ({UserData})")]
public class AnimTag {
  public ReadOnlySpan<AnimFrame> Frames => _frames;
  private readonly AnimFrame[] _frames;

  /// <summary>
  /// The name of the animation, as defined in the Aseprite file.
  /// </summary>
  public readonly string Name;

  /// <summary>
  /// Number of times the animation will play before stopping, as defined in the Aseprite file.
  /// </summary>
  public readonly int LoopCount;

  /// <summary>
  /// Whether the animation will play in reverse, as defined in the Aseprite file.
  /// </summary>
  public readonly bool IsReversed;

  /// <summary>
  /// Whether the animation should ping-pong once reaching the last frame, as defined in the Aseprite file.
  /// </summary>
  public readonly bool IsPingPong;

  /// <summary>
  /// The total duration of the animation, in seconds.
  /// </summary>
  public readonly float TotalDuration;

  public readonly AnimUserData UserData;

  private AnimTag(AnimFrame[] frames, string name, int loopCount, bool isReversed, bool isPingPong, AnimUserData data) {
    _frames = frames;
    Name = name;
    LoopCount = loopCount;
    IsReversed = isReversed;
    IsPingPong = isPingPong;
    TotalDuration = frames.Sum(frame => frame.Duration);
    UserData = data;
  }

  internal static AnimTag FromAse(AsepriteTag aseTag, ReadOnlySpan<AsepriteFrame> aseFrames, ReadOnlySpan<AnimUserData> frameDatas) {
    int frameCount = aseTag.To - aseTag.From + 1;
    var frames = new AnimFrame[frameCount];
    bool hasUserData = frameDatas.Length > 0;

    for (int i = 0; i < frameCount; i++) {
      AnimUserData frameData = hasUserData ? frameDatas[i] : AnimUserData.Empty;
      frames[i] = new AnimFrame(aseTag.From + i, aseFrames[i].Duration, frameData);
    }

    int loopCount = aseTag.Repeat;
    bool isReversed = aseTag.LoopDirection is AsepriteLoopDirection.Reverse or AsepriteLoopDirection.PingPongReverse;
    bool isPingPong = aseTag.LoopDirection is AsepriteLoopDirection.PingPong or AsepriteLoopDirection.PingPongReverse;

    return new AnimTag(frames, aseTag.Name, loopCount, isReversed, isPingPong, aseTag.UserData);
  }
}

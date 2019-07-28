using System.Collections.Generic;

namespace AnimLib {

  /// <summary> This class holds an array of Frames </summary>
  public class Track {

    /// <summary> Header data for this track </summary>
    public Header Header { get; }

    /// <summary> Frame data for this track </summary>
    public Frame[] Frames { get; }

    /// <summary> Combined duration of all frames in this track </summary>
    public int Duration { get; }

    /// <summary> Shorthand for Track.Frames[idx]</summary>
    public Frame this[int idx] => Frames[idx];
    
    /// <summary>
    /// Initializes a new Track.
    /// 
    /// Allows frames to either have a duration or no duration.
    /// </summary>
    /// <param name="header"></param>
    /// <param name="frames"></param>
    public Track(Header header, params Frame[] frames) {
      Frame[] InitRange() {
        List<Frame> newFrames = new List<Frame>();
        for (int i = 0; i < frames.Length - 1; i++) {
          Frame startFrame = frames[i];
          Frame endFrame = frames[i + 1];
          for (int y = startFrame.Y; y < endFrame.Y; y++) {
            newFrames.Add(new Frame(startFrame.X, y, startFrame.Duration));
          }
        }
        newFrames.Add(frames[frames.Length - 1]);
        frames = null;
        return newFrames.ToArray();
      }

      Header = header;
      Frames = (Header.Init == InitType.Range && frames.Length > 1) ? InitRange() : frames;
      foreach (Frame f in Frames) {
        if (f.Duration == -1) {
          Duration = -1;
          break;
        }
        Duration += f.Duration;
      }
    }
    
    /// <summary>
    /// Initializes a new Track.
    /// 
    /// This overload requires all frames to have a duration for proper looping.
    /// </summary>
    /// <param name="header"></param>
    /// <param name="frames">Array of frames with values X, Y, and duration.</param>
    /// <returns></returns>
    public Track(Header header, params (int, int, int)[] frames) : this(header, Utils.ToFrames(frames)) { }

    /// <summary>
    /// Initializes a new Track.
    /// 
    /// No frames will have a duration, therefore animations do not play back.
    /// </summary>
    /// <param name="header"></param>
    /// <param name="frames">Array of frames with values X and Y. No frame will have a duration.</param>
    /// <returns></returns>
    public Track(Header header, params (int, int)[] frames) : this(header, Utils.ToFramesNoDur(frames)) { }
  }
}

using System.Collections.Generic;

namespace BetterAnimations {
  public class Track {
    public Header Header { get; }
    public Frame[] Frames { get; }
    public int Duration { get; }
    public Frame this[int idx] => Frames[idx];
    
    public Track(Header header, params Frame[] frames) {
      Header = header;
      Frame[] newFrames = (header.Init == InitType.Range && frames.Length > 1) ? InitRange(frames) : frames;
      Frames = newFrames;
      foreach (Frame f in frames) {
        if (f.Duration == -1) {
          Duration = -1;
          break;
        }
        Duration += f.Duration;
      }
    }
    
    private Frame[] InitRange(Frame[] frames) {
      List<Frame> newFrames = new List<Frame>();
      for (int i = 0; i < frames.Length - 1; i++) {
        Frame startFrame = frames[i];
        Frame endFrame = frames[i + 1];
        for (int y = startFrame.Y; y < endFrame.Y; y++) {
          newFrames.Add(new Frame(startFrame.X, y, startFrame.Duration));
        }
      }
      newFrames.Add(frames[frames.Length - 1]);
      return newFrames.ToArray();
    }
  }
}

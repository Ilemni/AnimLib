using System;
using System.Collections.Generic;

namespace AnimLib.Animations {
  internal static class TrackHelper {
    public static Frame[] GetRange(AnimationSource source, Frame start, Frame end, SpriteRangeDirection direction) {
      switch (direction) {
        case SpriteRangeDirection.Down:
        case SpriteRangeDirection.DownUncapped:
          if (end.tile.X < start.tile.X) {
            throw new ArgumentException($"Sprite range Down, end tile cannot be to the left of start tile. Start:{start}, End:{end}");
          }
          return GetRangeX(source, start, end, direction == SpriteRangeDirection.DownUncapped);

        case SpriteRangeDirection.Right:
        case SpriteRangeDirection.RightUncapped:
          if (end.tile.Y > start.tile.Y) {
            throw new ArgumentException($"Sprite range Right, end tile cannot be above of start tile. Start:{start}, End:{end}");
          }
          return GetRangeY(source, start, end, direction == SpriteRangeDirection.RightUncapped);

        default: throw new ArgumentOutOfRangeException(nameof(direction), $"{direction} is not a valid {nameof(SpriteRangeDirection)}.");
      }
    }

    private static Frame[] GetRangeX(AnimationSource source, Frame start, Frame end, bool uncapped) {
      var frames = new List<Frame>();
      var grid = GetGrid(source);
      int x = start.tile.X;
      int y = start.tile.Y;
      frames.Add(start);

      while (true) {
        frames.Add(new Frame(x, y, start.duration));
        x++;
        if (x > grid.X) {
          x = uncapped ? 0 : start.tile.X;
          y++;
        }
        if (x == end.tile.X && y == end.tile.Y) {
          frames.Add(end);
          break;
        }
      }

      return frames.ToArray();
    }

    private static Frame[] GetRangeY(AnimationSource source, Frame start, Frame end, bool uncapped) {
      var frames = new List<Frame>();
      var grid = GetGrid(source);
      int x = start.tile.X;
      int y = start.tile.Y;
      frames.Add(start);

      while (true) {
        frames.Add(new Frame(x, y, start.duration));
        y++;
        if (y > grid.Y) {
          y = uncapped ? 0 : start.tile.Y;
          x++;
        }
        if (y == end.tile.Y && x == end.tile.X) {
          frames.Add(end);
          break;
        }
      }

      return frames.ToArray();
    }

    // For getting how far down or right a tile can go.
    // I.e. texture of 2000x2000, spriteSize 200x100, this would return (10, 20)
    // This is to keep tiles in-bounds during TrackHelper.GetRange
    private static PointByte GetGrid(AnimationSource source) {
      var tex = source.texture;
      var spriteSize = source.spriteSize;
      var x = Math.Floor((double)tex.Width / spriteSize.X);
      var y = Math.Floor((double)tex.Height / spriteSize.Y);
      if (x > 255) { x = 255; }
      if (y > 255) { y = 255; }
      var result = new PointByte((byte)x, (byte)y);

      return result;
    }
  }
}

using Microsoft.Xna.Framework;

namespace BetterAnimations {
  public class Frame {
    public byte X;
    public byte Y;
    public int Duration;
    public Point Tile => new Point(X, Y);
    public Frame (byte x, byte y, int duration=-1) {
      X = x;
      Y = y;
      Duration = duration;
    }
    public Frame(int x, int y, int duration=-1) : this((byte)x, (byte)y, duration) { }
    public override string ToString() => $"Tile [{X}, {Y}] Duration {Duration}";
  }
}

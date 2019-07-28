using Microsoft.Xna.Framework;

namespace AnimLib {
  /// <summary>
  /// Defines the DrawData source rect position of one frame of animation, as well as duration of animation
  /// </summary>
  public class Frame {
    /// <summary> X position of the source rect, in tiles </summary>
    public ushort X;
    
    /// <summary> Y position of the source rect, in tiles </summary>
    public ushort Y;
    
    /// <summary>
    /// How many `Update()` calls this frame lasts before transitioning
    /// 
    /// -1 means the frame will never transition from time elapsed
    /// </summary>
    public int Duration;
    
    /// <summary>
    /// X and Y in one Point
    /// </summary>
    public Point Tile => new Point(X, Y);

    /// <summary> Creates a new frame </summary>
    /// <param name="x">Tile X position.</param>
    /// <param name="y">Tile Y position.</param>
    /// <param name="dur">How many `Update()` calls this frame lasts before transitioning.
    /// 
    /// Defaults to `-1`, which lasts infinite calls and must transition manually.</param>
    public Frame (ushort x, ushort y, int dur=-1) {
      X = x;
      Y = y;
      Duration = dur;
    }
    
    /// <summary> Creates a new frame </summary>
    /// <param name="x">Tile X position. Must be between `0` and `65535`</param>
    /// <param name="y">Tile Y position. Must be between `0` and `65535`</param>
    /// <param name="dur">How many `Update()` calls this frame lasts before transitioning.
    /// 
    /// Defaults to `-1`, which lasts infinite calls and must transition manually.</param>
    public Frame(int x, int y, int dur=-1) : this(checked((ushort)x), checked((ushort)y), dur) { }

    /// <summary> Returns the X, Y, and Duration of this frame </summary>
    public override string ToString() => $"{{Tile [{X}, {Y}] Duration {Duration}";
  }
}

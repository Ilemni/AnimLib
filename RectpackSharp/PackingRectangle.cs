// ReSharper disable once CheckNamespace

namespace RectpackSharp;

/// <summary>
/// A rectangle that can be used for a rectangle packing operation.
/// </summary>
public struct PackingRectangle : IEquatable<PackingRectangle>, IComparable<PackingRectangle> {
  /// <summary>
  /// A value that can be used to identify this <see cref="PackingRectangle"/>. This value is
  /// never touched by the rectangle packing algorithm.
  /// </summary>
  public readonly int Id;

  /// <summary>
  /// Addition by AnimLib, to treat this rect as a duplicate of a rect with the specified <see cref="Id"/>.
  /// If not -1, this rect will be ignored when packing, and be placed in the same position as that rect.
  /// </summary>
  public readonly int LinkedId = -1;

  /// <summary>A value used internally by the packing algorithm for sorting rectangles.</summary>
  public uint SortKey;

  /// <summary>The X coordinate of the left edge of this <see cref="PackingRectangle"/>.</summary>
  public uint X;

  /// <summary>The Y coordinate of the top edge of this <see cref="PackingRectangle"/>.</summary>
  public uint Y;

  /// <summary>The width of this <see cref="PackingRectangle"/>.</summary>
  public uint Width;

  /// <summary>The height of this <see cref="PackingRectangle"/>.</summary>
  public uint Height;

  /// <summary>
  /// Gets or sets the X coordinate of the right edge of this <see cref="PackingRectangle"/>.
  /// </summary>
  /// <remarks>Setting this will only modify the <see cref="Width"/>.</remarks>
  public uint Right {
    readonly get => X + Width;
    set => Width = value - X;
  }

  /// <summary>
  /// Gets or sets the Y coordinate of the bottom edge of this <see cref="PackingRectangle"/>.
  /// </summary>
  /// <remarks>Setting this will only modify the <see cref="Height"/>.</remarks>
  public uint Bottom {
    readonly get => Y + Height;
    set => Height = value - Y;
  }

  /// <summary>Calculates this <see cref="PackingRectangle"/>'s area.</summary>
  public uint Area => Width * Height;

  /// <summary>Calculates this <see cref="PackingRectangle"/>'s perimeter.</summary>
  public uint Perimeter => Width + Width + Height + Height;

  /// <summary>Gets this <see cref="PackingRectangle"/>'s bigger side.</summary>
  public uint BiggerSide => Math.Max(Width, Height);

  /// <summary>Calculates this <see cref="PackingRectangle"/>'s pathological multiplier.</summary>
  /// <remarks>This is calculated as: <code>max(width, height) / min(width, height) * width * height</code></remarks>
  public uint PathologicalMultiplier => (Width > Height
    ? Width / Math.Max(Height, 1)
    : Height / Math.Max(Width, 1)) * Width * Height;

  /// <summary>
  /// Creates a <see cref="PackingRectangle"/> with the specified values.
  /// </summary>
  public PackingRectangle(uint x, uint y, uint width, uint height, int id = 0, int linkedId = -1) {
    X = x;
    Y = y;
    Width = width;
    Height = height;
    Id = id;
    LinkedId = linkedId;
    SortKey = 0;
  }

  /// <summary>
  /// Creates a <see cref="PackingRectangle"/> from a <see cref="Rectangle"/>.
  /// </summary>
  public PackingRectangle(Rectangle rectangle, int id, int linkedId)
    : this((uint)rectangle.X, (uint)rectangle.Y, (uint)rectangle.Width, (uint)rectangle.Height, id, linkedId) {
  }

  public static implicit operator Rectangle(PackingRectangle rectangle)
    => new((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);

  public static implicit operator PackingRectangle(Rectangle rectangle)
    => new((uint)rectangle.X, (uint)rectangle.Y, (uint)rectangle.Width, (uint)rectangle.Height);

  public static bool operator ==(PackingRectangle left, PackingRectangle right) => left.Equals(right);
  public static bool operator !=(PackingRectangle left, PackingRectangle right) => !left.Equals(right);

  /// <summary>
  /// Returns whether the given <see cref="PackingRectangle"/> is contained
  /// entirely within this <see cref="PackingRectangle"/>.
  /// </summary>
  public readonly bool Contains(ref readonly PackingRectangle other) {
    return X <= other.X && Y <= other.Y && Right >= other.Right && Bottom >= other.Bottom;
  }

  /// <summary>
  /// Returns whether the given <see cref="PackingRectangle"/> intersects with
  /// this <see cref="PackingRectangle"/>.
  /// </summary>
  public readonly bool Intersects(ref readonly PackingRectangle other) {
    return other.X < X + Width && X < other.X + other.Width
      && other.Y < Y + Height && Y < other.Y + other.Height;
  }

  /// <summary>
  /// Calculates the intersection of this <see cref="PackingRectangle"/> with another.
  /// </summary>
  public readonly PackingRectangle Intersection(ref readonly PackingRectangle other) {
    uint x1 = Math.Max(X, other.X);
    uint x2 = Math.Min(Right, other.Right);
    uint y1 = Math.Max(Y, other.Y);
    uint y2 = Math.Min(Bottom, other.Bottom);

    if (x2 >= x1 && y2 >= y1)
      return new PackingRectangle(x1, y1, x2 - x1, y2 - y1);

    return default;
  }

  public readonly override string ToString() {
    return $"{{ X={X}, Y={Y}, Width={Width}, Height={Height}, Id={Id}, LinkedId={LinkedId} }}";
  }

  public readonly override int GetHashCode() {
    return HashCode.Combine(X, Y, Width, Height, Id);
  }

  public readonly bool Equals(PackingRectangle other) {
    return X == other.X && Y == other.Y && Width == other.Width
      && Height == other.Height && Id == other.Id;
  }

  public readonly override bool Equals(object? obj) {
    if (obj is PackingRectangle viewport)
      return Equals(viewport);

    return false;
  }

  /// <summary>
  /// Compares this <see cref="SortKey"/> with another <see cref="PackingRectangle"/>'s.
  /// </summary>
  public readonly int CompareTo(PackingRectangle other) {
    return -SortKey.CompareTo(other.SortKey);
  }
}

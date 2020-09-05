using System;
using Microsoft.Xna.Framework;

namespace AnimLib {
  /// <summary>
  /// Uses <see cref="byte"/>s to represent a point.
  /// </summary>
  public struct PointByte : IEquatable<PointByte> {
    /// <summary>
    /// Creates a new instance of <see cref="PointByte"/> with the given X and Y value.
    /// </summary>
    /// <param name="x">X value.</param>
    /// <param name="y">Y value.</param>
    public PointByte(byte x, byte y) {
      X = x;
      Y = y;
    }

    /// <summary>
    /// X value.
    /// </summary>
    public byte X;
    /// <summary>
    /// Y value.
    /// </summary>
    public byte Y;

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is PointByte point && Equals(point);

    /// <inheritdoc/>
    public bool Equals(PointByte other) => X.Equals(other.X) && Y.Equals(other.Y);

    /// <inheritdoc/>
    public override int GetHashCode() {
      int hash = 17;
      hash += X.GetHashCode() * 34;
      hash <<= 3;
      hash += 17 + Y.GetHashCode() * 34;
      return hash;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{X}, {Y}";

    /// <summary>
    /// Indicates whether the current <see cref="PointByte"/> is equal to another instance of <see cref="PointByte"/>.
    /// </summary>
    /// <param name="left">This <see cref="PointByte"/>.</param>
    /// <param name="right">The other <see cref="PointByte"/> to compare to this <see cref="PointByte"/>.</param>
    /// <returns><see langword="true"/> if the current <see cref="PointByte"/> is equal to the other <see cref="PointByte"/>, otherwise <see langword="false"/>.</returns>
    public static bool operator ==(PointByte left, PointByte right) => left.Equals(right);

    /// <summary>
    /// Indicates whether the current <see cref="PointByte"/> is not equal to another instance of <see cref="PointByte"/>.
    /// </summary>
    /// <param name="left">This <see cref="PointByte"/>.</param>
    /// <param name="right">The other <see cref="PointByte"/> to compare to this <see cref="PointByte"/>.</param>
    /// <returns><see langword="true"/> if the current <see cref="PointByte"/> is not equal to the other <see cref="PointByte"/>, otherwise <see langword="false"/>.</returns>
    public static bool operator !=(PointByte left, PointByte right) => !(left == right);

    /// <summary>
    /// Creates an instance of <see cref="Point"/> using the values of the <see cref="PointByte"/>.
    /// </summary>
    /// <param name="point">The <see cref="PointByte"/>.</param>
    public static implicit operator Point(PointByte point) => new Point(point.X, point.Y);

    /// <summary>
    /// Creates an instance of <see cref="PointByte"/> casting the <see cref="int"/> values of the <see cref="Point"/> to <see cref="byte"/>s.
    /// </summary>
    /// <param name="point">The <see cref="Point"/>.</param>
    public static explicit operator PointByte(Point point) => new PointByte((byte)point.X, (byte)point.Y);
  }
}

// ReSharper disable once CheckNamespace

namespace RectpackSharp;

/// <summary>
/// Specifies hints that help optimize the rectangle packing algorithm.
/// </summary>
[Flags]
public enum PackingHints {
  /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by area.</summary>
  TryByArea = 1,

  /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by perimeter.</summary>
  TryByPerimeter = 2,

  /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by bigger side.</summary>
  TryByBiggerSide = 4,

  /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by width.</summary>
  TryByWidth = 8,

  /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by height.</summary>
  TryByHeight = 16,

  /// <summary>Tells the rectangle packer to try inserting the rectangles ordered by a pathological multiplier.</summary>
  TryByPathologicalMultiplier = 32,

  /// <summary>Specifies to try all the possible hints, as to find the best packing configuration.</summary>
  FindBest = TryByArea | TryByPerimeter | TryByBiggerSide | TryByWidth | TryByHeight | TryByPathologicalMultiplier,

  /// <summary>Specifies hints to optimize for rectangles who have one side much bigger than the other.</summary>
  UnusualSizes = TryByPerimeter | TryByBiggerSide | TryByPathologicalMultiplier,

  /// <summary>Specifies hints to optimize for rectangles whose sides are relatively similar.</summary>
  MostlySquared = TryByArea | TryByBiggerSide | TryByWidth | TryByHeight,
}

/// <summary>
/// Provides internal values and functions used by the rectangle packing algorithm.
/// </summary>
internal static class PackingHintExtensions {
  /// <summary>The maximum amount of hints that can be specified by a <see cref="PackingHints"/>.</summary>
  internal const int MaxHintCount = 6;

  /// <summary>
  /// Separates a <see cref="PackingHints"/> into the multiple options it contains,
  /// saving each of those separately onto a <see cref="Span{T}"/>.
  /// </summary>
  /// <param name="packingHint">The <see cref="PackingHints"/> to separate.</param>
  /// <param name="span">The span in which to write the resulting hints. This span's excess will be sliced.</param>
  public static void GetFlagsFrom(PackingHints packingHint, ref Span<PackingHints> span) {
    int index = 0;

    if (packingHint.HasFlag(PackingHints.TryByArea))
      span[index++] = PackingHints.TryByArea;
    if (packingHint.HasFlag(PackingHints.TryByPerimeter))
      span[index++] = PackingHints.TryByPerimeter;
    if (packingHint.HasFlag(PackingHints.TryByBiggerSide))
      span[index++] = PackingHints.TryByBiggerSide;
    if (packingHint.HasFlag(PackingHints.TryByWidth))
      span[index++] = PackingHints.TryByWidth;
    if (packingHint.HasFlag(PackingHints.TryByHeight))
      span[index++] = PackingHints.TryByHeight;
    if (packingHint.HasFlag(PackingHints.TryByPathologicalMultiplier))
      span[index++] = PackingHints.TryByPathologicalMultiplier;
    span = span[..index];
  }

  /// <summary>
  /// Sorts the given <see cref="PackingRectangle"/> array using the specified <see cref="PackingHints"/>.
  /// </summary>
  /// <param name="rectangles">The rectangles to sort.</param>
  /// <param name="packingHint">The hint to sort by. Must be a single bit value.</param>
  /// <remarks>
  /// The <see cref="PackingRectangle.SortKey"/> values will be modified.
  /// </remarks>
  public static void SortByPackingHint(Span<PackingRectangle> rectangles, PackingHints packingHint) {
    // Avoid delegate for tML mod unload
    for (int i = 0; i < rectangles.Length; i++) {
      ref PackingRectangle r = ref rectangles[i];
      // Each value should be a single flag
      // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
      r.SortKey = packingHint switch {
        PackingHints.TryByArea => r.Area,
        PackingHints.TryByPerimeter => r.Perimeter,
        PackingHints.TryByBiggerSide => r.BiggerSide,
        PackingHints.TryByWidth => r.Width,
        PackingHints.TryByHeight => r.Height,
        PackingHints.TryByPathologicalMultiplier => r.PathologicalMultiplier,
        _ => throw new ArgumentException(null, nameof(packingHint))
      };
    }

    // We sort the array, using the default rectangle comparison (which compares sort keys).
    rectangles.Sort();
  }
}

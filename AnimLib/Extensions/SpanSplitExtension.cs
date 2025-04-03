namespace AnimLib.Extensions;

// The code in this file is a slimmed down copy from .NET 9 runtime source code.
// https://github.com/dotnet/runtime/blob/5c6d1b3f7b63a3150ce6c737aeb4af03b3cce621/src/libraries/System.Private.CoreLib/src/System/MemoryExtensions.cs#L3363

#if !NET9_0_OR_GREATER
internal static class SpanSplitExtension {
  public static SpanSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> source, T separator) where T : IEquatable<T> =>
    new(source, separator);

  public ref struct SpanSplitEnumerator<T> where T : IEquatable<T> {
    private readonly ReadOnlySpan<T> _span;
    private readonly T _separator = default!;

    private int _startCurrent = 0;
    private int _endCurrent = 0;
    private int _startNext = 0;

    public readonly SpanSplitEnumerator<T> GetEnumerator() => this;

    public readonly ReadOnlySpan<T> Current => _span[_startCurrent.._endCurrent];

    internal SpanSplitEnumerator(ReadOnlySpan<T> span, T separator) {
      _span = span;
      _separator = separator;
    }

    public bool MoveNext() {
      if (_startNext == _span.Length) {
        return false;
      }

      _startCurrent = _startNext;

      // Search for the next separator index.
      int separatorIndex = _span[_startNext..].IndexOf(_separator);
      if (separatorIndex >= 0) {
        _endCurrent = _startCurrent + separatorIndex;
        _startNext = _endCurrent + 1;
      }
      else {
        _startNext = _endCurrent = _span.Length;
      }

      return true;
    }
  }
}
#endif

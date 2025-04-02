using System.Globalization;
using System.Runtime.CompilerServices;
using AsepriteDotNet.Aseprite.Types;

namespace AnimLib.Extensions;

/// <summary>
/// This class
/// </summary>
public static class StringExtensions {
  public static bool TryGetArg<T>(this string? str, ReadOnlySpan<char> key, [NotNullWhen(true)] out T? value)
    where T : ISpanParsable<T> {
    if (key.Length == 0) {
      throw new ArgumentException("Key cannot be empty", nameof(key));
    }

    if (str is null || str.Length < key.Length) {
      value = default;
      return false;
    }

    foreach (var entry in str.AsSpan().Split(',')) {
      bool hasValue = entry.SplitKvp(':', out var entryKey, out var entryVal);

      if (!entryKey.Equals(key, StringComparison.InvariantCulture)) {
        continue;
      }

      if (hasValue) {
        return T.TryParse(entryVal, CultureInfo.InvariantCulture, out value);
      }

      // Treat empty values as though the key is a flag, so value is true
      if (typeof(T) == typeof(bool)) {
        value = (T)(object)true;
        return true;
      }

      break;
    }

    value = default;
    return false;
  }

  public static bool TryGetArg<T>(this AsepriteUserData userData, ReadOnlySpan<char> key,
    [NotNullWhen(true)] out T? value)
    where T : ISpanParsable<T> => TryGetArg(userData.Text, key, out value);

  public static T? ArgOrDefault<T>(this AsepriteUserData userData, ReadOnlySpan<char> key, T? defaultValue = default)
    where T : ISpanParsable<T> => TryGetArg(userData.Text, key, out T? value) ? value : defaultValue;


  private static bool SplitKvp(this ReadOnlySpan<char> kvp, char separator, out ReadOnlySpan<char> key,
    out ReadOnlySpan<char> value) {
    int separatorIndex = kvp.IndexOf(separator);
    if (separatorIndex == -1) {
      key = kvp.Trim();
      value = default;
      return false;
    }

    key = kvp[..separatorIndex].Trim();
    value = kvp[(separatorIndex + 1)..].Trim();
    return true;
  }
}

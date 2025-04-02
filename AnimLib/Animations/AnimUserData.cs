using System.Runtime.CompilerServices;
using AnimLib.Extensions;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.Common;

namespace AnimLib.Animations;

public sealed class AnimUserData {
  private AnimUserData(string? name, Rgba32? color) :
    this(name, Unsafe.As<Rgba32?, Color?>(ref color)) {
  }

  private AnimUserData(string? name, Color? color) {
    Text = name;
    Color = color;
  }

  public static readonly AnimUserData Empty = new(null, null as Color?);

  public readonly string? Text;
  public readonly Color? Color;

  [MemberNotNullWhen(true, nameof(Text))]
  public bool HasText => Text is not null;

  [MemberNotNullWhen(true, nameof(Color))]
  public bool HasColor => Color is not null;

  /// <summary>
  /// Tries to get a value from the UserData text by key.
  /// <br/> If <typeparamref name="T"/> is <see langword="bool"/>,
  /// the value is considered <see langword="true"/> if the key is present and value is missing.
  /// </summary>
  /// <param name="key"></param>
  /// <param name="value"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public bool TryGetArg<T>(ReadOnlySpan<char> key, [NotNullWhen(true)] out T? value) where T : ISpanParsable<T> =>
    Text.TryGetArg(key, out value);

  public T? ArgOrDefault<T>(ReadOnlySpan<char> key, T? defaultValue = default) where T : ISpanParsable<T> =>
    Text.TryGetArg(key, out T? value) ? value : defaultValue;

  public bool HasFlag(ReadOnlySpan<char> key) =>
    Text.TryGetArg(key, out bool flag) && flag;

  public override string ToString() {
    return (HasText, HasColor) switch {
      (true, true) => $"Text = {Text}, Color = {Color}",
      (true, false) => $"Text = {Text}",
      (false, true) => $"Color = {Color}",
      _ => "Empty"
    };
  }

  public static implicit operator AnimUserData(AsepriteUserData aseUserData) {
    return aseUserData is { Text: null, Color: null } ? Empty : new AnimUserData(aseUserData.Text, aseUserData.Color);
  }
}

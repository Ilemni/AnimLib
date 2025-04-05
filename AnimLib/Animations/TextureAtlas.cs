using JetBrains.Annotations;

namespace AnimLib.Animations;

/// <summary>
/// Data class that contains a <see cref="Texture2D"/> that represents one or more layers,
/// <see cref="Rectangle"/>s representing the source and sprite rect of
/// each frame of animation on this <see cref="Texture2D"/>,
/// and a <see cref="AnimUserData"/> for each frame.
/// <para/>A source rect is the rectangle on the generated texture,
/// and the sprite rect is the rectangle on the original sprite canvas.
/// </summary>
[PublicAPI]
public sealed class TextureAtlas : IDisposable {
  internal TextureAtlas(Asset<Texture2D> textureAsset, Rectangle[] sourceRects, Rectangle[] spriteRects,
    AnimUserData[]? celUserData) {
    (TextureAsset, _sourceRects, _spriteRects, _celUserData) = (textureAsset, sourceRects, spriteRects, celUserData);
  }

  private readonly Rectangle[] _sourceRects;
  private readonly Rectangle[] _spriteRects;
  private readonly AnimUserData[]? _celUserData;

  public readonly Asset<Texture2D> TextureAsset;

  /// <summary>
  /// A <see cref="ReadOnlySpan{T}"/> that contains all the <see cref="Rectangle"/>s of the animation.
  /// </summary>
  /// <remarks>
  /// In the case of duplicate or empty frames during import,
  /// some <see cref="Rectangle"/> may map to the same part
  /// of the <see cref="Microsoft.Xna.Framework.Graphics.Texture"/>.
  /// </remarks>
  public ReadOnlySpan<Rectangle> SourceRects => _sourceRects;

  public ReadOnlySpan<Rectangle> SpriteRects => _spriteRects;

  public ReadOnlySpan<AnimUserData> CelUserData => _celUserData;

  /// <summary>
  /// The <see cref="Texture2D"/> of this Texture Atlas.
  /// </summary>
  public Texture2D Texture {
    get {
      if (!TextureAsset.IsLoaded) {
        TextureAsset.Wait();
      }

      return TextureAsset.Value;
    }
  }

  /// <summary>
  /// Gets a <see cref="Rectangle"/> at the frame of the provided <paramref name="index"/>.
  /// The result is the same regardless of the current animation being played.
  /// </summary>
  /// <param name="index">The frame index.</param>
  /// <returns>
  /// The source rect that represents the frame at the provided <paramref name="index"/>.
  /// </returns>
  /// <remarks>
  /// In the case of duplicate or empty frames during import,
  /// some <see cref="Rectangle"/> may map to the same part
  /// of the <see cref="Microsoft.Xna.Framework.Graphics.Texture"/>.
  /// </remarks>
  public Rectangle GetSourceRect(int index) {
    ArgumentOutOfRangeException.ThrowIfNegative(index);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _sourceRects.Length);
    return _sourceRects[index];
  }

  public Rectangle GetSpriteRect(int index) {
    ArgumentOutOfRangeException.ThrowIfNegative(index);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _sourceRects.Length);
    return _spriteRects[index];
  }

  public AnimUserData GetCelUserData(int index) {
    ArgumentOutOfRangeException.ThrowIfNegative(index);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _sourceRects.Length);
    return _celUserData?[index] ?? AnimUserData.Empty;
  }

  public void Dispose() {
    TextureAsset.Dispose();
  }
}

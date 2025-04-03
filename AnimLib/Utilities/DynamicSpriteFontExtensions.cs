using ReLogic.Graphics;
using SpriteCharacterData = ReLogic.Graphics.DynamicSpriteFont.SpriteCharacterData;

namespace AnimLib.Utilities;

/// <summary>
/// This file exists to make use of
/// <see cref="DynamicSpriteFontExtensionMethods.DrawString(SpriteBatch,DynamicSpriteFont,string,Vector2,Color)"/>
/// without allocating strings, by allowing usage of a <see cref="ReadOnlySpan{T}"/> of chars.
/// </summary>
public static class DynamicSpriteFontExtensions {
  static DynamicSpriteFontExtensions() {
    GetSpriteCharacters =
      ClassHacking.CreateGetter<DynamicSpriteFont, Dictionary<char, SpriteCharacterData>>("_spriteCharacters");
    GetDefaultCharacterData =
      ClassHacking.CreateGetter<DynamicSpriteFont, SpriteCharacterData>("_defaultCharacterData");
  }

  private static readonly Func<DynamicSpriteFont, Dictionary<char, SpriteCharacterData>> GetSpriteCharacters;
  private static readonly Func<DynamicSpriteFont, SpriteCharacterData> GetDefaultCharacterData;

  /// <summary>
  /// Draws text to the screen, where the text is represented by a <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>s.
  /// </summary>
  /// <param name="spriteBatch"><see cref="SpriteBatch"/> to draw to.</param>
  /// <param name="spriteFont">The font for the text.</param>
  /// <param name="text">The text to draw.</param>
  /// <param name="position">The screen position to draw at.</param>
  /// <param name="color">The color of the text.</param>
  /// <param name="rotation">The rotation of the text.</param>
  /// <param name="origin">The origin of the text.</param>
  /// <param name="scale">The scale of the text.</param>
  /// <param name="effects">The flip directions of the text.</param>
  /// <param name="layerDepth">The depth of the text.</param>
  public static void DrawString(this SpriteBatch spriteBatch,
    DynamicSpriteFont spriteFont,
    ReadOnlySpan<char> text,
    Vector2 position,
    Color color,
    float rotation = 0,
    Vector2 origin = default,
    float scale = 1f,
    SpriteEffects effects = SpriteEffects.None,
    float layerDepth = 0) {
    InternalDraw(spriteFont, text, spriteBatch, position, color, rotation,
      origin, new Vector2(scale), effects, layerDepth);
  }

  private static void InternalDraw(DynamicSpriteFont font,
    ReadOnlySpan<char> text,
    SpriteBatch spriteBatch,
    Vector2 startPosition,
    Color color,
    float rotation,
    Vector2 origin,
    Vector2 scale,
    SpriteEffects spriteEffects,
    float depth) {
    Matrix matrix =
      Matrix.CreateTranslation(-origin.X * scale.X, -origin.Y * scale.Y, 0) *
      Matrix.CreateRotationZ(rotation);
    Vector2 charPosition = Vector2.Zero;
    Vector2 flipDir = Vector2.One;
    bool isNewLine = true;
    float x = 0f;
    if (spriteEffects != 0) {
      Vector2 stringSize = font.MeasureString(text);
      if (spriteEffects.HasFlag(SpriteEffects.FlipHorizontally)) {
        x = stringSize.X * scale.X;
        flipDir.X = -1f;
      }

      if (spriteEffects.HasFlag(SpriteEffects.FlipVertically)) {
        charPosition.Y = (stringSize.Y - font.LineSpacing) * scale.Y;
        flipDir.Y = -1f;
      }
    }

    charPosition.X = x;
    foreach (char c in text) {
      switch (c) {
        case '\n':
          charPosition.X = x;
          charPosition.Y += font.LineSpacing * scale.Y * flipDir.Y;
          isNewLine = true;
          continue;
        case '\r':
          continue;
      }

      SpriteCharacterData characterData = GetCharacterData(font, c);
      Vector3 kerning = characterData.Kerning;
      Rectangle padding = characterData.Padding;
      if (spriteEffects.HasFlag(SpriteEffects.FlipHorizontally))
        padding.X -= padding.Width;

      if (spriteEffects.HasFlag(SpriteEffects.FlipVertically))
        padding.Y = font.LineSpacing - characterData.Glyph.Height - padding.Y;

      if (isNewLine)
        kerning.X = Math.Max(kerning.X, 0f);
      else
        charPosition.X += font.CharacterSpacing * scale.X * flipDir.X;

      charPosition.X += kerning.X * scale.X * flipDir.X;
      Vector2 position = charPosition + padding.TopLeft() * scale;
      Vector2.Transform(ref position, ref matrix, out position);
      position += startPosition;
      spriteBatch.Draw(characterData.Texture, position, characterData.Glyph, color, rotation, Vector2.Zero, scale,
        spriteEffects, depth);
      charPosition.X += (kerning.Y + kerning.Z) * scale.X * flipDir.X;
      isNewLine = false;
    }
  }

  public static Vector2 MeasureString(this DynamicSpriteFont font, ReadOnlySpan<char> text) {
    if (text.Length == 0)
      return Vector2.Zero;

    Vector2 size = Vector2.Zero;
    size.Y = font.LineSpacing;
    float xMax = 0f;
    int lineCount = 0;
    float kerningZ = 0f;
    bool isNewLine = true;
    foreach (char c in text) {
      switch (c) {
        case '\n':
          xMax = Math.Max(size.X + Math.Max(kerningZ, 0f), xMax);
          kerningZ = 0f;
          size = Vector2.Zero;
          size.Y = font.LineSpacing;
          isNewLine = true;
          lineCount++;
          continue;
        case '\r':
          continue;
      }

      SpriteCharacterData characterData = GetCharacterData(font, c);
      Vector3 kerning = characterData.Kerning;
      if (isNewLine)
        kerning.X = Math.Max(kerning.X, 0f);
      else
        size.X += font.CharacterSpacing + kerningZ;

      size.X += kerning.X + kerning.Y;
      kerningZ = kerning.Z;
      size.Y = Math.Max(size.Y, characterData.Padding.Height);
      isNewLine = false;
    }

    size.X += Math.Max(kerningZ, 0f);
    size.Y += lineCount * font.LineSpacing;
    size.X = Math.Max(size.X, xMax);
    return size;
  }

  private static SpriteCharacterData GetCharacterData(DynamicSpriteFont font, char character) {
    var dict = GetSpriteCharacters(font);
    return dict.TryGetValue(character, out SpriteCharacterData value) ? value : GetDefaultCharacterData(font);
  }
}

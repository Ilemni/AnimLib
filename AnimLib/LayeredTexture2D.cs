using AnimLib.Animations;
using AnimLib.Aseprite;

namespace AnimLib;

/// <summary>
/// This instance is created during <see cref="ModContent.Request{T}"/> by
/// <see cref="Aseprite.Processors.TextureDictionaryProcessor"/>,
/// when <see cref="AseReader"/> processes an Aseprite file.
/// <para/> If your drawing code does not have access to setting <c>sourceRect</c> or <c>spriteRect</c> directly,
/// such as a UI element drawing the whole texture, you should instead use <see cref="TextureDictionary"/>.
/// </summary>
// This stores textures as Asset<Texture2D>s, despite this also being treated as an Asset class.
// This is done because some fields expect an Asset<Texture2D>, and cannot accept a Texture2D.
public sealed class LayeredTexture2D : Dictionary<string, TextureEntry>, IDisposable {
  public LayeredTexture2D(Dictionary<string, TextureAtlas> atlases) {
    foreach ((string key, TextureAtlas atlas) in atlases) {
      int index = atlas.GetIndex(key);
      Add(key, new TextureEntry(atlas.TextureAsset, atlas.SpriteRects[index], atlas.SourceRects[index]));
    }
  }

  public void Dispose() {
    foreach (TextureEntry entry in Values) {
      entry.Dispose();
    }
  }
}

/// <summary>
/// This class is used to store a <see cref="Asset{Texture2D}"/> and its original position in the Aseprite file.
/// </summary>
/// <param name="Texture"></param>
/// <param name="SpriteRect"></param>
public sealed record TextureEntry(Asset<Texture2D> Texture, Rectangle SpriteRect, Rectangle SourceRect) : IDisposable {
  public void Dispose() {
    Texture.Dispose();
  }
}

public static class SpriteBatchExtensions {
  public static void Draw(this SpriteBatch sb, TextureEntry entry, Vector2 position, Color color) {
    sb.Draw(entry.Texture.Value, position, entry.SourceRect, color, 0f, -entry.SpriteRect.TopLeft(), 1f,
      SpriteEffects.None, 0f);
  }
}

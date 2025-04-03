namespace AnimLib.Animations;

/// <summary>
/// Dictionary of textures where each entry represents one layer.
/// <para/> If your drawing code has access to setting <c>sourceRect</c> or <c>spriteRect</c> directly,
/// such as with <see cref="SpriteBatch"/>, you should instead use <see cref="LayeredTexture2D"/>.
/// </summary>
// This class exists to remove need for `ModContent.Request<Dictionary<string, Asset<Texture2D>>>()`
public sealed class TextureDictionary : Dictionary<string, Asset<Texture2D>>;

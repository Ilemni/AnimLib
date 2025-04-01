using AnimLib.Animations;
using AsepriteDotNet.Aseprite;

namespace AnimLib.Aseprite.Processors;

/// <summary>
/// Processes an <see cref="AsepriteFile"/> to a <see cref="Dictionary{TKey,TValue}"/>,
/// where the key is the name of the layer,
/// and the value is the layer processed to a <see cref="Texture2D"/> Asset.
/// </summary>
public sealed class TextureDictionaryProcessor : IAsepriteProcessor<TextureDictionary> {
  TextureDictionary IAsepriteProcessor<TextureDictionary>.Process(AsepriteFile file, AnimProcessorOptions options) {
    options.NoPack = true;
    var textureAtlases = AnimTextureAtlasProcessor.Process(file, options);

    TextureDictionary result = [];
    foreach ((string key, TextureAtlas value) in textureAtlases) {
      result.Add(key, value.TextureAsset);
    }

    return result;
  }
}

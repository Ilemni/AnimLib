using AsepriteDotNet.Aseprite;

namespace AnimLib.Aseprite.Processors;

public sealed class LayeredTextureProcessor : IAsepriteProcessor<LayeredTexture2D> {
  LayeredTexture2D IAsepriteProcessor<LayeredTexture2D>.Process(AsepriteFile file, AnimProcessorOptions options) {
    var textureAtlases = AnimTextureAtlasProcessor.Process(file, options);

    LayeredTexture2D result = new(textureAtlases);
    return result;
  }
}

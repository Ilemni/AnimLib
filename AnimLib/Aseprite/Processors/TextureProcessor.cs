using System.Buffers;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Common;
using AsepriteDotNet.Processors;
using Texture = AsepriteDotNet.Texture;

namespace AnimLib.Aseprite.Processors;

/// <summary>
/// Basic processor to process an <see cref="AsepriteFile"/> into a <see cref="Texture2D"/>.
/// </summary>
public sealed class TextureProcessor : IAsepriteProcessor<Texture2D> {
  public Texture2D Process(AsepriteFile file, AnimProcessorOptions options) {
    // AsepriteDotNet processor
    AsepriteDotNet.TextureAtlas atlas = TextureAtlasProcessor.Process(file,
      options.OnlyVisibleLayers,
      options.IncludeBackgroundLayer,
      options.IncludeTilemapLayers,
      options.MergeDuplicateFrames);

    Texture texture = atlas.Texture;
    int width = texture.Size.Width;
    int height = texture.Size.Height;
    if (!options.Upscale) {
      return AseReader.CreateTexture2DAsset(texture.Name, width, height, texture.Pixels).Value;
    }

    width *= 2;
    height *= 2;
    var array = ArrayPool<Rgba32>.Shared.Rent(width * height);
    try {
      var upscaledPixels = array.AsSpan(0, width * height);
      WriteScaledPixels(texture.Pixels, upscaledPixels, width);

      return AseReader.CreateTexture2DAsset(texture.Name, width, height, upscaledPixels).Value;
    }
    finally {
      ArrayPool<Rgba32>.Shared.Return(array, true);
    }
  }

  private static void WriteScaledPixels(ReadOnlySpan<Rgba32> source, Span<Rgba32> destination, int destinationWidth) {
    int length = source.Length;
    for (int p = 0; p < length; p++) {
      int p2 = p * 2;
      int px = p2 % destinationWidth; // increase x by 2 per pixel
      int py = p2 / destinationWidth * 2; // increase y by 2 per row of pixels
      int row1 = py * destinationWidth + px;
      int row2 = row1 + destinationWidth;

      Rgba32 pixel = source[p];
      destination[row1] = pixel;
      destination[row1 + 1] = pixel;
      destination[row2] = pixel;
      destination[row2 + 1] = pixel;
    }
  }
}

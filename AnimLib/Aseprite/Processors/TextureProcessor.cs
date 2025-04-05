using System.Buffers;
using System.Diagnostics;
using System.Threading.Tasks;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Common;
using AsepriteDotNet.Processors;
using ReLogic.Utilities;
using Texture = AsepriteDotNet.Texture;

namespace AnimLib.Aseprite.Processors;

/// <summary>
/// Basic processor to process an <see cref="AsepriteFile"/> into a <see cref="Texture2D"/>.
/// </summary>
public sealed class TextureProcessor : IAsepriteProcessor<Texture2D> {
  [field: AllowNull, MaybeNull]
  private GraphicsDevice GraphicsDevice =>
    field ??= Main.instance.Services.Get<IGraphicsDeviceService>().GraphicsDevice;

  public async ValueTask<Texture2D> Process(AsepriteFile file, AnimProcessorOptions options,
    MainThreadCreationContext mainThreadCtx) {
    // AsepriteDotNet processor
    AsepriteDotNet.TextureAtlas atlas = TextureAtlasProcessor.Process(file,
      options.OnlyVisibleLayers,
      options.IncludeBackgroundLayer,
      options.IncludeTilemapLayers,
      options.MergeDuplicateFrames);

    Texture texture = atlas.Texture;
    int scale = options.Upscale ? 2 : 1;
    int width = texture.Size.Width * scale;
    int height = texture.Size.Height * scale;
    var array = ArrayPool<Rgba32>.Shared.Rent(width * height);
    var pixels = array.AsSpan(0, width * height);

    try {
      if (options.Upscale) {
        WriteScaledPixels(texture.Pixels, pixels, width);
      }
      else {
        texture.Pixels.CopyTo(pixels);
      }

      await mainThreadCtx;
      Debug.Assert(mainThreadCtx.IsCompleted);

      Texture2D tex = new(GraphicsDevice, width, height);
      tex.SetData(array);
      return tex;
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

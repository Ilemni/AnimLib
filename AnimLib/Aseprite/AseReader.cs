using System.IO;
using System.Runtime.InteropServices;
using AnimLib.Animations;
using AnimLib.Aseprite.Processors;
using AnimLib.Extensions;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Common;
using AsepriteDotNet.IO;
using JetBrains.Annotations;
using ReLogic.Content.Readers;
using Terraria.ModLoader.IO;

namespace AnimLib.Aseprite;

/// <summary>
/// AssetReader to read an Aseprite file (*.ase|*.aseprite) into a format usable by Terraria.
/// This Reader uses AsepriteDotNet to load the file into an <see cref="AsepriteFile"/> object,
/// which this reader then uses to create an object that AnimLib and Terraria can use.
/// https://github.com/AristurtleDev/AsepriteDotNet
/// </summary>
[PublicAPI]
public class AseReader : IAssetReader {
  private static readonly Dictionary<Type, IAsepriteProcessor> Processors = new();

  internal static void AddDefaultProcessors() {
    // Texture atlas
    // Supports frames, expected to be used in PlayerDrawLayers
    AddProcessor<AnimSpriteSheetProcessor, AnimSpriteSheet>();
    // Basic Texture2D
    // Ignores frames, merges all visible layers
    AddProcessor<TextureProcessor, Texture2D>();
    // Texture atlas
    // Ignores frames, separate layers are merged into atlas textures. Requires usage of SourceRect and Origin.
    AddProcessor<LayeredTextureProcessor, LayeredTexture2D>();
    // Dictionary of texture assets
    // Ignores frames, separate layers is are their own texture.
    AddProcessor<TextureDictionaryProcessor, TextureDictionary>();
  }

  /// <summary>
  /// Add a processor of type <typeparam name="TProcessor"></typeparam>
  /// which can process an <see cref="AsepriteFile"/> (*.ase|*.aseprite)
  /// into an object of type <typeparam name="T"></typeparam>.
  /// <para/> This allows for <c>ModContent.Request&lt;MyType&gt;("Path/To/MyAsepriteFile")</c>
  /// <para/> This must be called during your <see cref="Mod"/>'s <see cref="Mod.CreateDefaultContentSource"/>.
  /// <para/> <see cref="Mod.Load"/> is too late in the loading process.
  /// </summary>
  /// <typeparam name="TProcessor">Type of processor.</typeparam>
  /// <typeparam name="T">The resulting type of object from the processor.</typeparam>
  public static void AddProcessor<TProcessor, T>() where T : class where TProcessor : IAsepriteProcessor<T>, new() {
    Processors.Add(typeof(T), new TProcessor());
  }

  private static bool TryGetProcessor<T>([NotNullWhen(true)] out IAsepriteProcessor<T>? processor) where T : class {
    if (Processors.TryGetValue(typeof(T), out IAsepriteProcessor? baseProcessor)) {
      processor = (IAsepriteProcessor<T>)baseProcessor;
      return true;
    }

    processor = null;
    return false;
  }

  public static void Unload() {
    Processors.Clear();
  }

  public T FromStream<T>(Stream stream) where T : class {
    if (!TryGetProcessor(out IAsepriteProcessor<T>? processor)) {
      throw AssetLoadException.FromInvalidReader<AseReader, T>();
    }

    AsepriteFile asepriteFile;
    if (stream.CanSeek) {
      // We have no access to the file name to name this object.
      asepriteFile = AsepriteFileLoader.FromStream("", stream);
    }
    else {
      // AsepriteFileLoader requires the Seek function.
      // We cannot guarantee that the incoming stream supports Seeking (e.g. DeflateStream),
      // So we have to create a new Stream that allows it.
      using MemoryStream newStream = new();
      stream.CopyTo(newStream);
      newStream.Position = 0;
      asepriteFile = AsepriteFileLoader.FromStream("", newStream);
    }

    AnimProcessorOptions options = ProcessorOptionsFromFile(asepriteFile);
    return processor.Process(asepriteFile, options);
  }

  public static AnimProcessorOptions ProcessorOptionsFromFile(AsepriteFile file) {
    AnimProcessorOptions result = AnimProcessorOptions.Default;
    if (!file.UserData.HasText) {
      return result;
    }

    const string trueValue = ":true";
    const string falseValue = ":false";

    // Boolean values
    const string upscale = "upscale";
    const string onlyVisibleLayers = "onlyVisibleLayers";
    const string includeBackgroundLayer = "includeBackgroundLayer";
    const string mergeDuplicateFrames = "mergeDuplicateFrames";
    const string includeTilemapLayers = "includeTilemapLayers";

    // Integer values
    const string borderPadding = "borderPadding:";
    const string spacing = "spacing:";
    const string innerPadding = "innerPadding:";

    var text = file.UserData.Text.AsSpan();
    foreach (Range range in text.Split(',')) {
      var arg = text[range];
      if (arg.StartsWith(upscale)) {
        result.Upscale = arg switch {
          upscale + trueValue or upscale => true,
          upscale + falseValue => false,
          _ => result.Upscale
        };
      }

      if (arg.StartsWith(onlyVisibleLayers)) {
        result.OnlyVisibleLayers = arg switch {
          onlyVisibleLayers + trueValue or onlyVisibleLayers => true,
          onlyVisibleLayers + falseValue => false,
          _ => result.OnlyVisibleLayers
        };
      }

      if (arg.StartsWith(includeBackgroundLayer)) {
        result.IncludeBackgroundLayer = arg switch {
          includeBackgroundLayer + trueValue or includeBackgroundLayer => true,
          includeBackgroundLayer + falseValue => false,
          _ => result.IncludeBackgroundLayer
        };
      }

      if (arg.StartsWith(mergeDuplicateFrames)) {
        result.MergeDuplicateFrames = arg switch {
          mergeDuplicateFrames + trueValue or mergeDuplicateFrames => true,
          mergeDuplicateFrames + falseValue => false,
          _ => result.MergeDuplicateFrames
        };
      }

      if (arg.StartsWith(includeTilemapLayers)) {
        result.IncludeTilemapLayers = arg switch {
          includeTilemapLayers + trueValue or includeTilemapLayers => true,
          includeTilemapLayers + falseValue => false,
          _ => result.IncludeTilemapLayers
        };
      }

      if (arg.StartsWith(borderPadding)) {
        result.BorderPadding = int.TryParse(arg[borderPadding.Length..], out int value) ? value : result.BorderPadding;
      }

      if (arg.StartsWith(spacing)) {
        result.Spacing = int.TryParse(arg[spacing.Length..], out int value) ? value : result.Spacing;
      }

      if (arg.StartsWith(innerPadding)) {
        result.InnerPadding = int.TryParse(arg[innerPadding.Length..], out int value) ? value : result.InnerPadding;
      }
    }


    return result;
  }

  /// <summary>
  /// Creates an XNA <see cref="Texture2D"/> from an AsepriteDotNet <see cref="Texture"/>.
  /// </summary>
  /// <param name="tex">The AsepriteDotNet Texture to create the Texture2D from.</param>
  /// <param name="mode">The mode to request the asset in.</param>
  /// <returns></returns>
  internal static Asset<Texture2D> CreateTexture2DAsset(AsepriteDotNet.Texture tex,
    AssetRequestMode mode = AssetRequestMode.AsyncLoad) {
    Size size = tex.Size;
    return CreateTexture2DAsset(tex.Name, size.Width, size.Height, tex.Pixels, mode);
  }

  // We could have AsepriteProcessors create the Texture2Ds themselves with the async IAssetReader.FromStream method,
  // but some vanilla fields and methods expect an Asset<Texture2D>, not a plain Texture2D,
  // so we still have to nest assets such that we have things like Asset<Dictionary<string, Asset<Texture2D>>>.
  /// <summary>
  /// Creates an XNA <see cref="Texture2D"/> with the specified parameters.
  /// </summary>
  /// <param name="name">Name of the texture.</param>
  /// <param name="width">Width of the texture.</param>
  /// <param name="height">Height of the texture.</param>
  /// <param name="pixels">AsepriteDotNet pixels which represent color data for the texture.</param>
  /// <param name="mode">The mode to request the asset in.</param>
  /// <remarks>
  /// From the <paramref name="pixels"/>, this method creates a stream that represents a "rawimg" so that
  /// <see cref="Terraria.ModLoader.Assets.RawImgReader"/> can create an asset with the <see cref="Texture2D"/> for us.
  /// </remarks>
  internal static Asset<Texture2D> CreateTexture2DAsset(string name, int width, int height, ReadOnlySpan<Rgba32> pixels,
    AssetRequestMode mode = AssetRequestMode.AsyncLoad) {
    int length = width * height;
    if (pixels.Length != length) {
      throw new ArgumentException("Pixel span length does not match the specified size", nameof(pixels));
    }

    // We create a stream that represents a "rawimg" file
    // so that an existing reader can create the Asset<Texture2D> for us.
    byte[] buffer = new byte[12 + length * 4];
    BitConverter.TryWriteBytes(buffer.AsSpan(), ImageIO.VERSION);
    BitConverter.TryWriteBytes(buffer.AsSpan(4), width);
    BitConverter.TryWriteBytes(buffer.AsSpan(8), height);
    MemoryMarshal.Cast<Rgba32, byte>(pixels).CopyTo(buffer.AsSpan(12));

    // We do not use "using" here, the reader will close the stream once it creates the Texture2D.
    // Closed in ImageIO.ReadRaw()
    MemoryStream stream = new(buffer);
    string filename = name + ".rawimg";
    return AnimLibMod.Instance.Assets.CreateUntracked<Texture2D>(stream, filename, mode);
  }
}

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AnimLib.Animations;
using AnimLib.Aseprite.Processors;
using AnimLib.Extensions;
using AnimLib.Utilities;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.Common;
using AsepriteDotNet.IO;
using Ionic.Zlib;
using JetBrains.Annotations;
using ReLogic.Content.Readers;
using Terraria.ModLoader.Core;

namespace AnimLib.Aseprite;

/// <summary>
/// AssetReader to read an Aseprite file (*.ase|*.aseprite) into a format usable by Terraria.
/// This Reader uses AsepriteDotNet to load the file into an <see cref="AsepriteFile"/> object,
/// which this reader then uses to create an object that AnimLib and Terraria can use.
/// https://github.com/AristurtleDev/AsepriteDotNet
/// </summary>
[PublicAPI]
public sealed class AseReader : IAssetReader {
  private readonly Dictionary<Type, object> _processors = [];

  internal void AddDefaultProcessors() {
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
  public void AddProcessor<TProcessor, T>() where T : class where TProcessor : IAsepriteProcessor<T>, new() {
    _processors.Add(typeof(T), new TProcessor());
  }

  private bool TryGetProcessor<T>([NotNullWhen(true)] out IAsepriteProcessor<T>? processor) where T : class {
    if (_processors.TryGetValue(typeof(T), out object? baseProcessor)) {
      processor = (IAsepriteProcessor<T>)baseProcessor;
      return true;
    }

    processor = null;
    return false;
  }

  public void Unload() {
    _processors.Clear();
  }

  public async ValueTask<T> FromStream<T>(Stream stream, MainThreadCreationContext mainThreadCtx) where T : class {
    if (!TryGetProcessor(out IAsepriteProcessor<T>? processor)) {
      throw AssetLoadException.FromInvalidReader<AseReader, T>();
    }

    string name = GetEntryName(stream);
    if (stream.CanSeek) {
      return await ProcessStream(stream, processor, name, mainThreadCtx);
    }

    // AsepriteFileLoader requires the Seek function.
    // We cannot guarantee that the incoming stream supports Seeking (e.g. DeflateStream for large ase files),
    // So we have to create a new Stream that allows it.
    using MemoryStream newStream = new();
    await stream.CopyToAsync(newStream);
    newStream.Position = 0;
    return await ProcessStream(newStream, processor, name, mainThreadCtx);
  }

  private static async ValueTask<T> ProcessStream<T>(Stream stream, IAsepriteProcessor<T> processor, string name, MainThreadCreationContext mainThreadCtx) where T : class {
    AsepriteFile file = AsepriteFileLoader.FromStream(name, stream);
    AnimProcessorOptions options = ProcessorOptionsFromFile(file);
    return await processor.Process(file, options, mainThreadCtx);
  }

  /// <summary>
  /// Creates a <see cref="AnimProcessorOptions"/> from the file's Sprite <see cref="AsepriteFile.UserData"/>.
  /// List is as follows:
  /// <list type="table">
  /// <listheader><term>Option</term><description>Default Value</description></listheader>
  /// <item><term>upscale</term><description><see langword="false"/></description></item>
  /// <item><term>onlyVisibleLayers</term><description><see langword="true"/></description></item>
  /// <item><term>includeBackgroundLayer</term><description><see langword="false"/></description></item>
  /// <item><term>mergeDuplicateFrames</term><description><see langword="true"/></description></item>
  /// <item><term>includeTilemapLayers</term><description><see langword="true"/></description></item>
  /// </list>
  /// </summary>
  /// <param name="file">File to get the options from.</param>
  /// <returns>
  /// Processor options from the file's Sprite UserData.
  /// </returns>
  public static AnimProcessorOptions ProcessorOptionsFromFile(AsepriteFile file) {
    AsepriteUserData data = file.UserData;
    if (!data.HasText) {
      return AnimProcessorOptions.Default;
    }

    return new AnimProcessorOptions {
      Upscale = data.ArgOrDefault("upscale", false),
      OnlyVisibleLayers = data.ArgOrDefault("onlyVisibleLayers", true),
      IncludeBackgroundLayer = data.ArgOrDefault("includeBackgroundLayer", false),
      MergeDuplicateFrames = data.ArgOrDefault("mergeDuplicateFrames", true),
      IncludeTilemapLayers = data.ArgOrDefault("includeTilemapLayers", true),
      NoPack = data.ArgOrDefault("noPack", false)
    };
  }

  /// <summary>
  /// Creates a <see cref="Texture2D"/> with the specified parameters.
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
    AssetRequestMode mode = AssetRequestMode.ImmediateLoad) {
    // We default this to ImmediateLoad, since this asset is being created as part of another asset currently loading.
    if (pixels.Length != width * height) {
      throw new ArgumentException("Pixel span length does not match the specified size", nameof(pixels));
    }

    // We create a stream that represents a "rawimg" file
    // so that an existing reader can create the Asset<Texture2D> for us.
    // Although we can create the Asset instance directly via reflection, and instantiate a Texture2D on main thread,
    // this would require all processors whose asset contains an Asset<Texture2D> to use the AseReader's MainThreadCreationContext
    byte[] bufferArray = new byte[12 + pixels.Length * 4];
    var buffer = bufferArray.AsSpan();
    BitConverter.TryWriteBytes(buffer, 1); // ImageIO.VERSION
    BitConverter.TryWriteBytes(buffer[4..], width);
    BitConverter.TryWriteBytes(buffer[8..], height);
    MemoryMarshal.Cast<Rgba32, byte>(pixels).CopyTo(buffer[12..]);

    // We do not use "using" here, the reader will close the stream once it creates the Texture2D.
    // Closed in ImageIO.ReadRaw()
    MemoryStream stream = new(bufferArray);
    string filename = name + ".rawimg";
    return AnimLibMod.Instance.Assets.CreateUntracked<Texture2D>(stream, filename, mode);
  }

  // Reflection to get the filename from the stream
  private const string EntryReadStreamTypeName = "Terraria.ModLoader.Core.EntryReadStream";
  private const string NotFound = " not found. Inform AnimLib devs.";

  private static readonly Type EntryReadStreamType =
    typeof(ModLoader).Assembly.GetType(EntryReadStreamTypeName) ??
    throw new InvalidOperationException($"Type \"{EntryReadStreamTypeName}\"" + NotFound);

  private static readonly PropertyInfo EntryReadStreamName =
    EntryReadStreamType.GetProperty("Name") ??
    throw new InvalidOperationException($"Property \"{EntryReadStreamTypeName}.Name\"" + NotFound);

  private static readonly FieldInfo EntryReadStreamFile =
    EntryReadStreamType.GetField("file", BindingFlags.NonPublic | BindingFlags.Instance) ??
    throw new InvalidOperationException($"Field \"{EntryReadStreamTypeName}.file\"" + NotFound);

  private static readonly Func<DeflateStream, Stream> GetInnerStream =
    ClassHacking.CreateGetter<DeflateStream, Stream>("_innerStream");


  private static string GetEntryName(Stream stream) {
    if (TryNameFromEntryReadStream(stream, out string name)) {
      return name;
    }

    return stream switch {
      FileStream fileStream => NameFromFileStream(fileStream.Name.Replace('\\', '/')),
      DeflateStream deflateStream => TryNameFromEntryReadStream(GetInnerStream(deflateStream), out name) ? name : "",
      _ => ""
    };
  }

  private static bool TryNameFromEntryReadStream(Stream stream, out string name) {
    if (!EntryReadStreamType.IsInstanceOfType(stream)) {
      name = "";
      return false;
    }

    TmodFile modFile = (TmodFile)EntryReadStreamFile.GetValue(stream)!;
    string fileName = (string)EntryReadStreamName.GetValue(stream)!;
    name = modFile.Name + ':' + fileName[..fileName.LastIndexOf('.')];
    return true;
  }

  private static string NameFromFileStream(string fullPath) {
    Mod? mod = null;
    foreach (Mod m in ModLoader.Mods) {
      if (m.SourceFolder is { Length: > 0 } folderStr && fullPath.StartsWith(folderStr)) {
        mod = m;
        break;
      }
    }

    if (mod is null) {
      return "";
    }

    int pathLen = mod.SourceFolder.Length + 1;

    string path = fullPath[pathLen..];
    return mod.Name + ':' + path[..path.LastIndexOf('.')];
  }
}

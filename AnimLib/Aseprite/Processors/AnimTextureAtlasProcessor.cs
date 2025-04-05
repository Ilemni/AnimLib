using System.Buffers;
using System.Runtime.InteropServices;
using AnimLib.Animations;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.Common;
using AsepriteDotNet.Processors;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AnimLib.Aseprite.Processors;

/// <summary>
/// Defines a processor for processing multiple <see cref="AsepriteDotNet.TextureAtlas"/>es from an <see cref="AsepriteFile"/>,
/// each corresponding to a target layer.
/// <para/> A target layer is any layer, regardless of nesting, whose UserData color is Green.
/// <para/> Creates a Dictionary of the following structure:
/// <b>Key:</b> string representing the target layer path (e.g. "Root/Parent/Child")
/// <b>Value:</b> <see cref="AsepriteDotNet.TextureAtlas"/> where the Texture is either:
///   <li>An image of the image layer, or</li>
///   <li>The flattened image of a group layer.</li>
/// </summary>
public static class AnimTextureAtlasProcessor {
  private const int MaxAtlasWidth = 2048;
  private const int MaxPixelsPerAtlas = MaxAtlasWidth * MaxAtlasWidth;

  /// <summary>
  /// Processes multiple <see cref="AsepriteDotNet.TextureAtlas"/>s from an <see cref="AsepriteFile"/>.
  /// </summary>
  /// <param name="file">The <see cref="AsepriteFile"/> to process.</param>
  /// <param name="options">
  ///   Optional options to use when processing.  If <see langword="null"/>, then
  ///   <see cref="ProcessorOptions.Default"/> will be used.
  /// </param>
  /// <returns>A Dictionary of target layers where each key is the layer name and the value is a flattened representation of that layer.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> is <see langword="null"/>.</exception>
  public static Dictionary<string, TextureAtlas> Process(AsepriteFile file, AnimProcessorOptions options) {
    ArgumentNullException.ThrowIfNull(file);

    var allFrames = GetLayerEntries(file, options);

    var atlasData = CreateTextureAtlases(file, options, allFrames);

    return atlasData;
  }

  private static LayerEntry[] GetLayerEntries(AsepriteFile file, AnimProcessorOptions options) {
    // We want to know what all the valid target layers are
    // These are layers that will process into a texture, and any children will be flattened into them
    var targetLayers = GetTargetLayers(file.Layers, file.Frames, options, out string[] names);

    int frameCount = file.FrameCount;
    int layerCount = targetLayers.Length;
    var allLayersFrames = new LayerEntry[layerCount];

    for (int i = 0; i < layerCount; i++) {
      // Not all layers may have cel userdata, so we only allocate an array for those that do
      AnimUserData[]? celUserDatas = null;

      for (int j = 0; j < frameCount; j++) {
        foreach (AsepriteCel cel in file.Frames[j].Cels) {
          if (ReferenceEquals(cel.Layer, targetLayers[i])) {
            if (celUserDatas is null) {
              celUserDatas = new AnimUserData[frameCount];
              Array.Fill(celUserDatas, AnimUserData.Empty);
            }

            celUserDatas[j] = cel.UserData;
            break;
          }
        }
      }

      allLayersFrames[i] = new LayerEntry(names[i], new FrameEntry[frameCount], celUserDatas);
    }

    for (int frameIndex = 0; frameIndex < frameCount; frameIndex++) {
      var frameDatas = ProcessorHelper.FlattenFrameToTargetLayers(file, frameIndex, options, targetLayers);

      for (int layerIndex = 0; layerIndex < frameDatas.Length; layerIndex++) {
        var frameData = frameDatas[layerIndex];
        allLayersFrames[layerIndex].Frames[frameIndex] = new FrameEntry(frameData, frameIndex, layerIndex);
      }
    }

    // Assign to LayerEntry.NumPixels and .Next
    int scale = options.Upscale ? 2 : 1;
    for (int i = 0; i < layerCount; i++) {
      LayerEntry layer = allLayersFrames[i];
      int sum = 0;
      foreach (FrameEntry f in layer.Frames) {
        sum += (f.Bounds.Width * scale + 2) * (f.Bounds.Height * scale + 2);
      }

      layer.NumPixels = sum;
      if (i != layerCount - 1) {
        allLayersFrames[i].Next = allLayersFrames[i + 1];
      }
    }

    ProcessorHelper.ProcessFileCopyColors(file, targetLayers, allLayersFrames);

    return allLayersFrames;
  }

  /// <summary>
  /// A target is a layer which will represent a single texture atlas after processing.
  /// In the case of group layers, eligible children will be flattened into the target layer.
  /// </summary>
  /// <param name="layers">
  /// All the layers in the <see cref="AsepriteFile"/>.
  /// </param>
  /// <param name="frames">
  /// Used to validate that a layer contains any cels.
  /// <br/> Any otherwise valid target layers that do not contain cels will be ignored.
  /// <br/> A group layer will still be valid if any child layer contains at least one cel.
  /// </param>
  /// <param name="options">
  /// Options to determine whether a layer will be skipped.
  /// </param>
  /// <param name="names">
  /// List of layer names representing the resulting target layers.
  /// <br/> Unlike <see cref="AsepriteLayer.Name"/>, a name represents the full path of the layer.
  /// </param>
  /// <returns></returns>
  private static AsepriteLayer[] GetTargetLayers(
    ReadOnlySpan<AsepriteLayer> layers,
    ReadOnlySpan<AsepriteFrame> frames,
    AnimProcessorOptions options,
    out string[] names) {
    var targetLayers = new List<AsepriteLayer>(layers.Length);
    var namesList = new List<string>(targetLayers.Capacity);

    for (int i = 0; i < layers.Length; i++) {
      AsepriteLayer layer = layers[i];
      AsepriteUserData userData = layer.UserData;
      if (userData.HasColor && userData.Color.Value.PackedValue
            is Colors.Red
            or Colors.Yellow) {
        // Ignore any layer that has Red userdata, regardless of any other settings
        // Some layers we may want to treat as a reference rather than an art asset
        // Ignore any layer that has Yellow userdata, is meant for processing into Vector2s
        if (layer is AsepriteGroupLayer gl) {
          // Ignore all children of Red or Yellow userdata group layer
          i += gl.Children.Length;
        }

        continue;
      }

      if (layer is AsepriteGroupLayer groupLayer) {
        if (layer is { IsVisible: false, UserData.Color: not { PackedValue: Colors.Green or Colors.Blue } }) {
          // Ignore invisible group layer
          continue;
        }

        var children = groupLayer.Children;
        if (children.Length == 0) {
          // Ignore empty group layer
          continue;
        }

        bool isValid = false;
        foreach (AsepriteLayer childLayer in children) {
          if (childLayer is AsepriteGroupLayer) {
            // Ignore nested group layers
            continue;
          }

          if (!childLayer.IsVisible && options.OnlyVisibleLayers) {
            continue;
          }

          // Ignore layer if all are of specific UserData colors
          AsepriteUserData childUserData = childLayer.UserData;
          if (childUserData.Color?.PackedValue
              is Colors.Red or Colors.Green
              or Colors.Yellow or Colors.Blue) {
            continue;
          }

          isValid = true;
          break;
        }

        if (isValid) {
          targetLayers.Add(layer);
          namesList.Add(ProcessorHelper.GetNestedLayerName(layers, i));
        }

        continue;
      }

      if (userData is { HasColor: true, Color.PackedValue: Colors.Green or Colors.Blue }) {
        // Consider any layer that has Green userdata as a root layer, regardless of any other settings
        // Some layers we want imported but not visible while working on them in Aseprite

        // Skip if layer does not contain any cels
        // A use case may be a 1-frame file with a large number of programmatically accessed layers, with some layers not yet drawn
        foreach (AsepriteFrame frame in frames) {
          foreach (AsepriteCel cel in frame.Cels) {
            if (ReferenceEquals(cel.Layer, layer)) {
              goto AddLayer; // aka "break; break;"
            }
          }
        }

        continue;

        AddLayer:
        targetLayers.Add(layer);
        namesList.Add(ProcessorHelper.GetNestedLayerName(layers, i));
        continue;
      }

      if (layer.ChildLevel != 0 ||
          (!layer.IsVisible && options.OnlyVisibleLayers) ||
          (layer.IsBackgroundLayer && !options.IncludeBackgroundLayer)) {
        continue;
      }

      targetLayers.Add(layer);
      namesList.Add(layer.Name);
    }

    names = namesList.ToArray();
    return targetLayers.ToArray();
  }

  private static Dictionary<string, TextureAtlas> CreateTextureAtlases(AsepriteFile file, AnimProcessorOptions options,
    LayerEntry[] layerEntries) {
    bool upscale = options.Upscale;
    bool mergeDuplicates = options.MergeDuplicateFrames;
    int scale = upscale ? 2 : 1;
    int frameCount = file.FrameCount;

    List<Rectangle> sourceRectsList = new(frameCount);
    List<Rectangle[]> spriteRectsList = [];
    List<LayerEntry> atlasEntries = [];
    List<Dictionary<int, int>> duplicateMaps = [];
    int pixelsThisAtlas = 0;

    ModContent.SplitName(file.Name, out string modName, out string assetName);

    Dictionary<string, TextureAtlas> atlasDict = [];

    foreach (LayerEntry layer in layerEntries) {
      pixelsThisAtlas += layer.NumPixels;

      // Get relevant atlas info for this layer
      var layerFrames = layer.Frames;
      var layerRects = new Rectangle[frameCount];
      for (int i = 0; i < layerFrames.Length; i++) {
        Rectangle r = layerFrames[i].Bounds;
        layerRects[i] = new Rectangle(r.X * scale, r.Y * scale, r.Width * scale, r.Height * scale);
      }

      if (mergeDuplicates) {
        var duplicateMap = GetDuplicateMap(layerFrames);
        duplicateMaps.Add(duplicateMap);

        // For atlas size calcs, reduce count by num duplicate pixels
        foreach (int key in duplicateMap.Keys) {
          Rectangle rect = layerRects[key];
          pixelsThisAtlas -= rect.Width * rect.Height;
        }
      }

      atlasEntries.Add(layer);
      spriteRectsList.Add(layerRects);
      sourceRectsList.AddRange(layerRects); // If this is ever changed to list of arrays, use .ToArray()

      // Check if we can fit more into this atlas
      LayerEntry? nextLayer = layer.Next;
      int nextPixels = nextLayer?.NumPixels ?? 0;
      if (!options.NoPack && nextLayer is not null && pixelsThisAtlas + nextPixels < MaxPixelsPerAtlas * 0.85f) {
        // We can still fit more into this atlas. Continue to next layer
        continue;
      }

      // Prepare to draw the atlas
      // If !noPack, we pack the atlas
      ushort width = (ushort)(file.CanvasWidth * scale);
      ushort height = (ushort)(file.CanvasWidth * scale);
      var sourceRects = CollectionsMarshal.AsSpan(sourceRectsList);
      string texName = GetTextureName(assetName, layer.Name, atlasDict.Count, options.NoPack);
      if (!options.NoPack) {
        GetMaxAtlasSize(pixelsThisAtlas, ref width, ref height);
        RectPacker.Pack(sourceRects, modName, texName, ref width, ref height, frameCount, duplicateMaps);
      }

      // Create new atlas
      var atlasPixelArray = ArrayPool<Rgba32>.Shared.Rent(width * height);
      var atlasPixels = atlasPixelArray.AsSpan(0, width * height);
      atlasPixels.Clear();

      WriteAtlasPixels(atlasEntries, atlasPixels, sourceRects, duplicateMaps, frameCount, upscale, width);

      // Output extra PNGs for debug if this asset is from a locally built mod
      RectPacker.SavePng(atlasPixels, modName, texName, width, height);
      if (!options.NoPack) {
        RectPacker.SavePngBounds(atlasPixels, modName, texName, width, height, frameCount, sourceRects);
      }

      var textureAsset = AseReader.CreateTexture2DAsset(texName, width, height, atlasPixels);

      for (int i = 0; i < atlasEntries.Count; i++) {
        LayerEntry layerEntry = atlasEntries[i];
        TextureAtlas atlas = new(textureAsset,
          sourceRects.Slice(i * frameCount, frameCount).ToArray(),
          spriteRectsList[i],
          layerEntry.CelDatas);
        atlasDict.Add(layerEntry.Name, atlas);
      }

      sourceRectsList.Clear();
      spriteRectsList.Clear();
      atlasEntries.Clear();
      duplicateMaps.Clear();
      pixelsThisAtlas = 0;

      ArrayPool<Rgba32>.Shared.Return(atlasPixelArray, true);
    }

    return atlasDict;
  }

  private static void WriteAtlasPixels(List<LayerEntry> atlasEntries, Span<Rgba32> atlasPixels,
    ReadOnlySpan<Rectangle> sourceRects, List<Dictionary<int, int>>? duplicateMaps,
    int frameCount, bool upscale, ushort width) {
    bool mergeDuplicates = duplicateMaps is not null;
    for (int i = 0; i < atlasEntries.Count; i++) {
      var frames = atlasEntries[i].Frames;
      var duplicateMap = mergeDuplicates ? duplicateMaps![i] : null;
      var rects = sourceRects.Slice(i * frameCount, frameCount);

      for (int j = 0; j < frames.Length; j++) {
        if (frames[j].CelData is not { Length: > 0 } cel) {
          continue;
        }

        if (mergeDuplicates && duplicateMap!.ContainsKey(j)) {
          continue;
        }

        // Write the color data
        if (upscale) {
          WriteScaledPixels(atlasPixels, width, cel, rects[j]);
        }
        else {
          WritePixels(atlasPixels, width, cel, rects[j]);
        }
      }
    }
  }

  private static string GetTextureName(string fileName, string layerName, int num, bool noPack) {
    if (string.IsNullOrWhiteSpace(fileName)) {
      fileName = "unknown";
    }

    if (noPack) {
      return $"{fileName}_{layerName}";
    }

    if (num != 0) {
      return $"{fileName}_{num + 1}";
    }

    return fileName;
  }

  private static Dictionary<int, int> GetDuplicateMap(ReadOnlySpan<FrameEntry> layerFrames) {
    int emptyIndex = -1;
    var duplicateMap = new Dictionary<int, int>();

    for (int i = 0; i < layerFrames.Length; i++) {
      FrameEntry frame = layerFrames[i];
      if (frame.IsEmpty) {
        // Frame is empty, map to shared empty and continue to next frame
        if (emptyIndex == -1) {
          // First instance of empty
          emptyIndex = i;
        }
        else {
          duplicateMap.Add(i, emptyIndex);
        }

        continue;
      }

      for (int d = 0; d < i; d++) {
        if (frame.IsDuplicate(layerFrames[d])) {
          duplicateMap.Add(i, d);
          break;
        }
      }
    }

    return duplicateMap;
  }

  private static void WritePixels(Span<Rgba32> imagePixels, int imageWidth, Rgba32[] pixels, Rectangle rect) {
    int x = rect.X;
    int y = rect.Y;
    int w = rect.Width;
    int length = pixels.Length;
    for (int p = 0; p < length; p++) {
      int px = x + p % w;
      int py = y + p / w;
      int index = py * imageWidth + px;
      imagePixels[index] = pixels[p];
    }
  }

  private static void WriteScaledPixels(Span<Rgba32> imagePixels, int imageWidth, Rgba32[] pixels, Rectangle rect) {
    int x = rect.X;
    int y = rect.Y;
    int w = rect.Width;
    int length = pixels.Length;
    for (int p = 0; p < length; p++) {
      int p2 = p * 2;
      int px = x + p2 % w; // increase x by 2 per pixel
      int py = y + p2 / w * 2; // increase y by 2 per row of pixels
      int row1 = py * imageWidth + px;
      int row2 = row1 + imageWidth;

      Rgba32 pixel = pixels[p];
      imagePixels[row1] = pixel;
      imagePixels[row1 + 1] = pixel;
      imagePixels[row2] = pixel;
      imagePixels[row2 + 1] = pixel;
    }
  }

  public static void GetMaxAtlasSize(int area, ref ushort minWidth, ref ushort minHeight) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(area);

    if (area > MaxPixelsPerAtlas) {
      throw new ArgumentException($"Input must be less than or equal to 2048^2.");
    }

    area = (int)(area * 1.25f);
    int sqrt = (int)MathF.Ceiling(MathF.Sqrt(area));
    sqrt = Math.Max(sqrt, Math.Max(minWidth, minHeight));

    // Power of 2
    int n = sqrt - 1;
    n |= n >> 1;
    n |= n >> 2;
    n |= n >> 4;
    n |= n >> 8;
    n |= n >> 16;
    n++;

    ushort result = n > sqrt * 1.25f
      ? (ushort)(sqrt * 1.25f)
      : (ushort)n;

    minWidth = minHeight = result;
  }

  internal record LayerEntry(string Name, FrameEntry[] Frames, AnimUserData[]? CelDatas) {
    public int NumPixels { get; internal set; }
    public LayerEntry? Next { get; internal set; }
  }

  internal readonly record struct FrameEntry(int FrameIndex, int LayerIndex, Rgba32[] CelData, Rectangle Bounds) {
    public bool IsEmpty => CelData.Length == 0;

    public FrameEntry((Rgba32[] pixels, Rectangle bounds) frameData, int frameIndex, int layerIndex) :
      this(frameIndex, layerIndex, frameData.pixels, frameData.bounds) {
    }

    public bool IsDuplicate(FrameEntry other) {
      if (IsEmpty != other.IsEmpty ||
          LayerIndex != other.LayerIndex ||
          Bounds.Size() != other.Bounds.Size()) {
        // We allow different Bounds.Location to be equal if cel data is identical
        return false;
      }

      var span = MemoryMarshal.Cast<Rgba32, byte>(CelData);
      var otherSpan = MemoryMarshal.Cast<Rgba32, byte>(other.CelData);
      return span.SequenceEqual(otherSpan);
    }
  }
}

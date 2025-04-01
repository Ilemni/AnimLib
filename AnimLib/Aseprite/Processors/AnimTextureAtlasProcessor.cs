using System.Buffers;
using System.Linq;
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

    int frameCount = file.Frames.Length;
    var allFrames = new LayerEntry[targetLayers.Length];
    for (int i = 0; i < allFrames.Length; i++) {
      allFrames[i] = new LayerEntry(names[i], new FrameEntry[frameCount]);
    }

    for (int i = 0; i < frameCount; i++) {
      var frameColorData = ProcessorHelper.FlattenFrameToTopLayers(file, i, options, targetLayers);

      for (int targetLayerIndex = 0; targetLayerIndex < frameColorData.Length; targetLayerIndex++) {
        var colorData = frameColorData[targetLayerIndex];
        allFrames[targetLayerIndex].Frames[i] = new FrameEntry(i, targetLayerIndex, colorData);
      }
    }

    return allFrames;
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
        var children = groupLayer.Children;
        if (children.Length == 0) {
          // Ignore empty group layer
          continue;
        }

        bool isValid = false;
        foreach (AsepriteLayer childLayer in children) {
          if (!childLayer.IsVisible && options.OnlyVisibleLayers) {
            continue;
          }

          // Ignore layer if all are of specific UserData colors
          AsepriteUserData childUserData = childLayer.UserData;
          if (childUserData.HasColor && childUserData.Color.Value.PackedValue
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

      if (userData is {HasColor: true, Color.PackedValue: Colors.Green or Colors.Blue }) {
        // Consider any layer that has Green userdata as a root layer, regardless of any other settings
        // Some layers we want imported but not visible while working on them in Aseprite

        // Skip if layer does not contain any cels
        // A use case may be a 1-frame file with a large number of programmatically accessed layers, with some layers not yet drawn
        foreach (AsepriteFrame frame in frames) {
          foreach (AsepriteCel cel in frame.Cels) {
            if (ReferenceEquals(cel.Layer, layer)) {
              // ReSharper disable once GrammarMistakeInComment
              goto AddLayer; // aka "break break;"
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
    LayerEntry[] allFrames) {
    Dictionary<string, TextureAtlas> atlasData = [];

    foreach ((string name, var frames) in allFrames) {
      TextureAtlas atlas = CreateTextureAtlas(file, options, frames, name);
      atlasData.Add(name, atlas);
    }

    return atlasData;
  }

  private static TextureAtlas CreateTextureAtlas(AsepriteFile file, AnimProcessorOptions options, FrameEntry[] frames,
    string name) {
    bool upscale = options.Upscale;
    bool mergeDuplicates = options.MergeDuplicateFrames;
    int scale = upscale ? 2 : 1;
    int frameCount = frames.Length;

    Dictionary<int, int>? duplicateMap = null;
    if (options.MergeDuplicateFrames) {
      duplicateMap = GetDuplicateMap(frames);
      frameCount -= duplicateMap.Count;
    }

    float sqrt = MathF.Sqrt(frameCount);
    int columns = (int)Math.Ceiling(sqrt);
    int rows = (frameCount + columns - 1) / columns;

    int frameWidth = file.CanvasWidth * scale;
    int frameHeight = file.CanvasHeight * scale;
    int atlasWidth = columns * frameWidth
      + options.BorderPadding * 2
      + options.Spacing * (columns - 1)
      + options.InnerPadding * 2 * columns;
    int atlasHeight = columns * frameHeight
        + options.BorderPadding * 2
        + options.Spacing * (rows - 1)
        + options.InnerPadding * 2 * rows;

    var imagePixelArray = ArrayPool<Rgba32>.Shared.Rent(atlasWidth * atlasHeight);
    var imagePixels = imagePixelArray.AsSpan(0, atlasWidth * atlasHeight);
    imagePixels.Clear();

    var regions = new Rectangle[file.Frames.Length];
    int offset = 0;
    var originalToDuplicateLookup = new Dictionary<int, Rectangle>();

    for (int i = 0; i < frames.Length; i++) {
      FrameEntry frame = frames[i];

      // Create region for duplicate frame, don't write to texture
      if (mergeDuplicates && duplicateMap!.TryGetValue(i, out int value)) {
        regions[frame.FrameIndex] = originalToDuplicateLookup[value];
        offset++;
        continue;
      }

      // Get X and Y coords for where to write the color data to
      int column = (i - offset) % columns;
      int row = (i - offset) / columns;

      int x = column * frameWidth
        + options.BorderPadding
        + options.Spacing * column
        + options.InnerPadding * (column + column + 1);

      int y = row * frameHeight
        + options.BorderPadding
        + options.Spacing * row
        + options.InnerPadding * (row + row + 1);

      Rectangle bounds = new(x, y, frameWidth, frameHeight);
      regions[frame.FrameIndex] = bounds;
      originalToDuplicateLookup.Add(i, bounds);

      if (frame.IsEmpty) {
        continue;
      }

      // Write the color data
      if (upscale) {
        WriteScaledPixels(imagePixels, atlasWidth, frame.ColorData, x, y, frameWidth);
      }
      else {
        WritePixels(imagePixels, atlasWidth, frame.ColorData, x, y, frameWidth);
      }
    }

    var textureAsset = AseReader.CreateTexture2DAsset(name, atlasWidth, atlasHeight, imagePixels);

    ArrayPool<Rgba32>.Shared.Return(imagePixelArray, true);
    return new TextureAtlas(regions, textureAsset);
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
        // Expensive checks
        if (IsDuplicate(frame, layerFrames[d])) {
          duplicateMap.Add(i, d);
          break;
        }
      }
    }

    return duplicateMap;
  }

  private static bool IsDuplicate(FrameEntry firstFrame, FrameEntry secondFrame) {
    // Only compare to non-empty candidates that originate from the original layer and are not itself
    if (secondFrame.IsEmpty ||
        secondFrame.TargetLayerIndex != firstFrame.TargetLayerIndex ||
        secondFrame.FrameIndex == firstFrame.FrameIndex) {
      return false;
    }

    var firstData = firstFrame.ColorData!;
    var secondData = secondFrame.ColorData!;

    if (firstData.Length != secondData.Length) {
      return false;
    }

    // Attempt early terminations by comparing sparse individual pixels of the texture
    // Equality is very slow otherwise
    int size = firstData.Length;
    int interval = size > 50 ? size / 50 : 1;
    for (int i = interval; i < size; i += interval) {
      if (firstData[i] != secondData[i]) {
        return false;
      }
    }

    // Last resort, actually compare the arrays
    return firstData.SequenceEqual(secondData, null);
  }

  private static void WritePixels(Span<Rgba32> imagePixels, int imageWidth, Rgba32[] pixels, int x, int y, int w) {
    int length = pixels.Length;
    for (int p = 0; p < length; p++) {
      int px = x + p % w;
      int py = y + p / w;
      int index = py * imageWidth + px;
      imagePixels[index] = pixels[p];
    }
  }

  private static void WriteScaledPixels(Span<Rgba32> imagePixels, int imageWidth, Rgba32[] pixels, int x, int y, int w) {
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
}

internal record LayerEntry(string Name, FrameEntry[] Frames);

internal readonly record struct FrameEntry(
  int FrameIndex,
  int TargetLayerIndex,
  Rgba32[]? ColorData) {
  [MemberNotNullWhen(false, nameof(ColorData))]
  public bool IsEmpty => ColorData is null;
}

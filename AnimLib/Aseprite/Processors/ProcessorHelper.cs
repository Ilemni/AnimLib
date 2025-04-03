using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnimLib.Extensions;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.Common;
using Point = AsepriteDotNet.Common.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AnimLib.Aseprite.Processors;

using AseRectangle = AsepriteDotNet.Common.Rectangle;

public static class ProcessorHelper {
  // This is a non-nullable array where each element represents a target layer at the specified frame.
  // Some layers may not have pixel data at the specified frame, and would thus be null.
  public static (Rgba32[] pixels, Rectangle bounds)[] FlattenFrameToTargetLayers(AsepriteFile file, int frameIndex,
    AnimProcessorOptions options, AsepriteLayer[] targetLayers) {
    ArgumentNullException.ThrowIfNull(file);

    AsepriteFrame frame = file.Frames[frameIndex];

    (Rgba32[] pixels, Rectangle bounds)[] result = new (Rgba32[], Rectangle)[targetLayers.Length];

    for (int i = 0; i < targetLayers.Length; i++) {
      AsepriteLayer targetLayer = targetLayers[i];
      result[i] = ([], default);

      if (targetLayer is AsepriteGroupLayer groupLayer) {
        Log.Debug(
          $"[Aseprite] Merging group layer on file [{file.Name}]: {GetNestedLayerName(file.Layers, groupLayer)} " +
          $"Children layers are: [ {string.Join(", ", GetGroupCels(options, groupLayer, frame).Select(c => GetNestedLayerName(file.Layers, c.Layer)))} ]");
        result[i] = MergeCels(options, frame, groupLayer);
        continue;
      }

      foreach (AsepriteCel cel in frame.Cels) {
        if (ReferenceEquals(cel.Layer, targetLayer)) {
          result[i] = (cel.GetPixels().ToArray(), cel.GetBounds());
          break;
        }
      }
    }

    return result;
  }

  internal static void ProcessFileCopyColors(AsepriteFile file, AsepriteLayer[] targetLayers,
    AnimTextureAtlasProcessor.LayerEntry[] allFrames) {
    for (int layerIndex = 0; layerIndex < targetLayers.Length; layerIndex++) {
      AsepriteLayer targetLayer = targetLayers[layerIndex];
      if (targetLayer.UserData is not {
            HasColor: true,
            Color.PackedValue: Colors.Blue,
            HasText: true
          }) {
        continue;
      }

      if (!targetLayer.UserData.TryGetArg("copyColor", out string? targetLayerName)) {
        continue;
      }

      if (!TryGetLayerIndexFromFullName(file, targetLayers, targetLayerName, out int targetIndex)) {
        Log.Warn(
          $"[Aseprite] In file \"{file.Name}\", " +
          $"Layer {GetNestedLayerName(targetLayers, targetLayer)} to copy layer '{targetLayerName}', " +
          $"but the layer was not found.");

        continue;
      }

      for (int frameIndex = 0; frameIndex < file.FrameCount; frameIndex++) {
        AnimTextureAtlasProcessor.FrameEntry destinationFrame = allFrames[layerIndex].Frames[frameIndex];
        AnimTextureAtlasProcessor.FrameEntry sourceFrame = allFrames[targetIndex].Frames[frameIndex];
        ProcessCopyColors(file.Frames[layerIndex].Cels, targetLayer, destinationFrame, sourceFrame);
      }
    }
  }

  private static bool TryGetLayerIndexFromFullName(AsepriteFile file, AsepriteLayer[] targetLayers,
    string targetLayerName, out int index) {
    for (int j = 0; j < targetLayers.Length; j++) {
      if (NestedLayerNameMatches(file.Layers, targetLayers[j], targetLayerName)) {
        index = j;
        return true;
      }
    }

    index = -1;
    return false;
  }

  private static void ProcessCopyColors(ReadOnlySpan<AsepriteCel> destinationCels, AsepriteLayer targetLayer,
    AnimTextureAtlasProcessor.FrameEntry destinationFrame, AnimTextureAtlasProcessor.FrameEntry sourceFrame) {
    if (destinationFrame.IsEmpty) {
      return;
    }

    if (sourceFrame.IsEmpty) {
      Array.Clear(destinationFrame.CelData);
      return;
    }

    float alphaMultiplier =
      targetLayer.UserData.ArgOrDefault<bool>("ignoreOpacity")
        ? 1
        : targetLayer.Opacity / 255f;

    AsepriteCel? cel = null;
    foreach (AsepriteCel frameCel in destinationCels) {
      if (ReferenceEquals(frameCel.Layer, targetLayer)) {
        cel = frameCel;
        break;
      }
    }

    Debug.Assert(cel is not null, "Cel should not be null");

    alphaMultiplier *= cel.UserData.ArgOrDefault<bool>("ignoreOpacity")
      ? 1
      : cel.Opacity / 255f;

    CopyColor(destinationFrame, sourceFrame, alphaMultiplier);
  }

  private static (Rgba32[], Rectangle) MergeCels(AnimProcessorOptions options, AsepriteFrame frame,
    AsepriteGroupLayer groupLayer) {
    if (GetGroupCels(options, groupLayer, frame) is not { Count: > 0 } groupCels) {
      return ([], default);
    }

    var cels = CollectionsMarshal.AsSpan(groupCels);

    Rectangle bounds = cels[0].GetBounds();
    foreach (AsepriteCel cel in cels[1..]) {
      Rectangle celBounds = cel.GetBounds();
      int boundsRight = bounds.X + bounds.Width;
      int boundsBottom = bounds.Y + bounds.Height;
      int celRight = celBounds.X + celBounds.Width;
      int celBottom = celBounds.Y + celBounds.Height;

      bounds.X = Math.Min(bounds.X, celBounds.X);
      bounds.Y = Math.Min(bounds.Y, celBounds.Y);
      bounds.Width = Math.Max(boundsRight, celRight) - bounds.X;
      bounds.Height = Math.Max(boundsBottom, celBottom) - bounds.Y;
    }

    var backdrop = new Rgba32[bounds.Width * bounds.Height];
    foreach (AsepriteCel asepriteCel in cels) {
      AsepriteCel cel = asepriteCel is AsepriteLinkedCel linkedCel ? linkedCel.Cel : asepriteCel;

      switch (cel) {
        case AsepriteImageCel imageCel:
          AseRectangle aseBounds = Unsafe.As<Rectangle, AseRectangle>(ref bounds);
          BlendCel(imageCel, backdrop, aseBounds);
          break;
        case AsepriteTilemapCel tilemapCel:
          AseBlendTilemapCel(backdrop, tilemapCel, frame.Size.Width);
          break;
      }
    }

    return (backdrop, bounds);
  }


  private static List<AsepriteCel> GetGroupCels(AnimProcessorOptions options, AsepriteGroupLayer groupLayer,
    AsepriteFrame frame) {
    List<AsepriteCel> result = [];

    var children = groupLayer.Children;
    int childrenLength = children.Length;
    var cels = frame.Cels;
    int celsLength = cels.Length;

    int jStart = 0;

    for (int i = 0; i < celsLength; i++) {
      AsepriteCel cel = cels[i];
      AsepriteLayer layer = cel.Layer;

      // Ignore Red: not to be imported at all
      // Ignore Green: imported as its own target layer
      // Ignore Yellow: processed to Vector2s
      // Ignore Blue: copies RGB values from its tagged layer
      if (layer.UserData is
          { HasColor: true, Color.PackedValue: Colors.Red or Colors.Green or Colors.Yellow or Colors.Blue }) {
        continue;
      }

      if (!IsValidCel(options, cel)) {
        continue;
      }

      for (int j = jStart; j < childrenLength; j++) {
        if (ReferenceEquals(children[j], layer)) {
          result.Add(cel);
          jStart = j + 1;
          break;
        }
      }
    }

    return result;
  }

  private static bool IsValidCel(AnimProcessorOptions options, AsepriteCel cel) {
    cel = cel is AsepriteLinkedCel linkedCel ? linkedCel.Cel : cel;

    // Always import layer with Green or Blue userdata
    // Green and Blue cel should be excluded in GetGroupCels
    if (cel.Layer.UserData is { HasColor: true, Color.PackedValue: Colors.Green or Colors.Blue }) {
      return true;
    }

    return (!options.OnlyVisibleLayers || cel.Layer.IsVisible) &&
      (options.IncludeBackgroundLayer || !cel.Layer.IsBackgroundLayer) &&
      (options.IncludeTilemapLayers || cel is not AsepriteTilemapCel);
  }

  private static void BlendCel(AsepriteImageCel imageCel, Rgba32[] backdrop, AseRectangle bounds) {
    var pixels = imageCel.GetPixels();
    AsepriteUserData celData = imageCel.UserData;
    AsepriteUserData layerData = imageCel.Layer.UserData;

    int celOpacity = celData.ArgOrDefault<bool>("ignoreOpacity") ? 255 : imageCel.Opacity;
    int layerOpacity = layerData.ArgOrDefault<bool>("ignoreOpacity") ? 255 : imageCel.Layer.Opacity;

    Point p = new(imageCel.Location.X - bounds.X, imageCel.Location.Y - bounds.Y);
    AseRectangle innerRect = new(p, imageCel.Size);

    AseBlendCel(backdrop, pixels, imageCel.Layer.BlendMode, innerRect, bounds.Width, celOpacity, layerOpacity);
  }

  private static void CopyColor(
    AnimTextureAtlasProcessor.FrameEntry destination,
    AnimTextureAtlasProcessor.FrameEntry source, float alphaMultiplier) {
    var sourcePixels = source.CelData;
    var destinationPixels = destination.CelData;

    Rectangle sourceBounds = source.Bounds;
    Rectangle destinationBounds = destination.Bounds;

    int startX = Math.Max(sourceBounds.X, destinationBounds.X);
    int startY = Math.Max(sourceBounds.Y, destinationBounds.Y);
    int endX = Math.Min(sourceBounds.X + sourceBounds.Width, destinationBounds.X + destinationBounds.Width);
    int endY = Math.Min(sourceBounds.Y + sourceBounds.Height, destinationBounds.Y + destinationBounds.Height);

    for (int y = startY; y < endY; y++) {
      for (int x = startX; x < endX; x++) {
        int sourceIndex = (y - sourceBounds.Y) * sourceBounds.Width + (x - sourceBounds.X);
        int destinationIndex = (y - destinationBounds.Y) * destinationBounds.Width + (x - destinationBounds.X);

        Rgba32 sourceColor = sourcePixels[sourceIndex];
        Rgba32 destinationColor = destinationPixels[destinationIndex];

        if (sourceColor.A == 0 || destinationColor.A == 0) {
          destinationPixels[destinationIndex] = default;
          continue;
        }

        float multi = destinationColor.A / 255f * (sourceColor.A / 255f) * alphaMultiplier;
        sourceColor.R = (byte)(sourceColor.R * multi);
        sourceColor.G = (byte)(sourceColor.G * multi);
        sourceColor.B = (byte)(sourceColor.B * multi);
        sourceColor.A = (byte)(destinationColor.A * multi);
        destinationPixels[destinationIndex] = sourceColor;
      }
    }
  }

  internal static bool NestedLayerNameMatches(ReadOnlySpan<AsepriteLayer> fileLayers, AsepriteLayer layer,
    ReadOnlySpan<char> name) {
    Span<int> nameIndices = stackalloc int[layer.ChildLevel + 1];

    int i = 0;
    for (; i < fileLayers.Length; i++) {
      if (ReferenceEquals(fileLayers[i], layer)) {
        nameIndices[layer.ChildLevel] = i;
        break;
      }
    }

    int parentLevel = layer.ChildLevel - 1;
    for (; i >= 0; i--) {
      AsepriteLayer currentLayer = fileLayers[i];
      if (currentLayer.ChildLevel != parentLevel) {
        continue;
      }

      parentLevel = currentLayer.ChildLevel - 1;
      nameIndices[currentLayer.ChildLevel] = i;
      if (currentLayer.ChildLevel == 0) {
        break;
      }
    }

    var remainingName = name;
    foreach (int nameIndex in nameIndices) {
      string layerName = fileLayers[nameIndex].Name;
      if (!remainingName.StartsWith(layerName, StringComparison.Ordinal)) {
        return false;
      }

      if (remainingName.Length == layerName.Length) {
        return true;
      }

      remainingName = remainingName[(layerName.Length + 1)..];
    }

    return true;
  }

  public static string GetNestedLayerName(ReadOnlySpan<AsepriteLayer> fileLayers, AsepriteLayer layer) {
    for (int i = 0; i < fileLayers.Length; i++) {
      if (ReferenceEquals(fileLayers[i], layer)) {
        return GetNestedLayerName(fileLayers, i);
      }
    }

    return layer.Name;
  }


  public static string GetNestedLayerName(ReadOnlySpan<AsepriteLayer> fileLayers, int index) {
    AsepriteLayer targetLayer = fileLayers[index];
    if (targetLayer.ChildLevel == 0) {
      return targetLayer.Name;
    }

    string[] namesToMerge = new string[targetLayer.ChildLevel + 1];
    namesToMerge[targetLayer.ChildLevel] = targetLayer.Name;

    int parentLevel = targetLayer.ChildLevel - 1;
    for (int i = index; i >= 0; i--) {
      AsepriteLayer layer = fileLayers[i];
      if (layer.ChildLevel != parentLevel) {
        continue;
      }

      parentLevel = layer.ChildLevel - 1;
      namesToMerge[layer.ChildLevel] = layer.Name;
      if (layer.ChildLevel == 0) {
        break;
      }
    }

    return string.Join('/', namesToMerge);
  }

  private static ReadOnlySpan<Rgba32> GetPixels(this AsepriteCel cel) {
    return cel switch {
      AsepriteImageCel imageCel => imageCel.Pixels,
      AsepriteLinkedCel tilemapCel => tilemapCel.Cel.GetPixels(),
      _ => throw new NotSupportedException("Cel type not supported.")
    };
  }

  private static Rectangle GetBounds(this AsepriteCel cel) {
    return cel switch {
      AsepriteImageCel imageCel => XnaRectangle(imageCel.Location, imageCel.Size),
      AsepriteLinkedCel tilemapCel => tilemapCel.Cel.GetBounds(),
      _ => throw new NotSupportedException("Cel type not supported.")
    };

    static Rectangle XnaRectangle(Point location, Size size) =>
      new(location.X, location.Y, size.Width, size.Height);
  }

  private static BlendCelDelegate AseBlendCel { get; } = Func<BlendCelDelegate>("BlendCel");
  private static BlendTilemapCelDelegate AseBlendTilemapCel { get; } = Func<BlendTilemapCelDelegate>("BlendTilemapCel");

  private static T Func<T>(string methodName) where T : Delegate {
    return typeof(AsepriteFrameExtensions)
      .GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)!
      .CreateDelegate<T>();
  }

  delegate void BlendCelDelegate(
    Span<Rgba32> backdrop,
    ReadOnlySpan<Rgba32> source,
    AsepriteBlendMode blendMode,
    AseRectangle bounds,
    int frameWidth,
    int celOpacity,
    int layerOpacity
  );

  delegate void BlendTilemapCelDelegate(
    Span<Rgba32> backdrop,
    AsepriteTilemapCel tilemapCel,
    int frameWidth
  );
}

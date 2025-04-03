using AnimLib.Animations;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.Processors;
using Point = AsepriteDotNet.Common.Point;

namespace AnimLib.Aseprite.Processors;

/// <summary>
/// Defines a processor for processing an <see cref="AnimLibMod"/> <see cref="AsepriteFile"/> from an <see cref="AnimSpriteSheet"/>.
/// </summary>
public sealed class AnimSpriteSheetProcessor : IAsepriteProcessor<AnimSpriteSheet> {
  /// <summary>
  /// Processes an <see cref="AnimLibMod"/> <see cref="AnimSpriteSheet"/>
  /// </summary>
  /// <param name="file">The <see cref="AsepriteFile"/> to process.</param>
  /// <param name="options">Optional <see cref="ProcessorOptions"/> used in processing the <see cref="AsepriteFile"/>.</param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public AnimSpriteSheet Process(AsepriteFile file, AnimProcessorOptions options) {
    ArgumentNullException.ThrowIfNull(file);

    var tags = GetTags(file);

    var textureAtlases = AnimTextureAtlasProcessor.Process(file, options);

    var pointDict = ProcessPoints(file, options);

    int scale = options.Upscale ? 2 : 1;

    return new AnimSpriteSheet(textureAtlases, tags, pointDict, file.UserData,
      new Vector2(file.CanvasWidth * scale, file.CanvasHeight * scale));
  }

  private static AnimTag[] GetTags(AsepriteFile file) {
    var fileTags = file.Tags;

    var tags = new AnimTag[fileTags.Length];
    var tagHashes = fileTags.Length < 256 ? stackalloc int[fileTags.Length] : new int[fileTags.Length];
    for (int i = 0; i < tagHashes.Length; i++) {
      tagHashes[i] = 0;
    }

    AnimUserData[] frameUserData = [];
    bool hasUserData = false;
    foreach (AsepriteLayer layer in file.Layers) {
      if (layer is not { Name: "data", ChildLevel: 0 }) {
        continue;
      }

      frameUserData = new AnimUserData[file.FrameCount];
      Array.Fill(frameUserData, AnimUserData.Empty);
      hasUserData = true;

      for (int frameIndex = 0; frameIndex < file.FrameCount; frameIndex++) {
        AsepriteFrame frame = file.Frames[frameIndex];
        foreach (AsepriteCel cel in frame.Cels) {
          if (ReferenceEquals(cel.Layer, layer)) {
            frameUserData[frameIndex] = cel.UserData;
            break;
          }
        }
      }

      break;
    }

    for (int i = 0; i < fileTags.Length; i++) {
      AsepriteTag aseTag = fileTags[i];
      int hash = aseTag.Name.GetHashCode();
      if (tagHashes.Contains(hash)) {
        throw new InvalidOperationException("Duplicate tag name '" + aseTag.Name +
          "' found.  Tags must have unique names for a sprite sheet");
      }

      tagHashes[i] = hash;

      Range range = aseTag.From..(aseTag.To+1);
      var userData = hasUserData ? frameUserData.AsSpan(range) : [];
      tags[i] = AnimTag.FromAse(aseTag, file.Frames[range], userData);
    }

    return tags;
  }

  /// Hacky method, to parse some layers which are intended to represent positional data, into an array of <see cref="Vector2"/>s.
  /// <para/> This is intended for things like an arm offset, where the shoulder would often move positions,
  /// and the held item would move with it.
  /// <remarks>
  /// If Aseprite were to support positional data, this method should be replaced to parse that.
  /// </remarks>
  private static Dictionary<string, Vector2[]> ProcessPoints(AsepriteFile file, AnimProcessorOptions options) {
    Dictionary<string, Vector2[]> result = [];

    float scale = options.Upscale ? 2 : 1;
    Vector2 center = new Vector2(file.CanvasWidth, file.CanvasHeight) * scale / 2;

    foreach (AsepriteLayer layer in file.Layers) {
      if (layer is { UserData: { HasColor: true, Color.PackedValue: Colors.Yellow } }) {
        var array = new Vector2[file.FrameCount];
        Array.Fill(array, center);
        result.Add(layer.Name, array);
      }
    }

    var frames = file.Frames;
    for (int i = 0; i < frames.Length; i++) {
      foreach (AsepriteCel cel in frames[i].Cels) {
        if (cel is AsepriteImageCel imageCel && result.TryGetValue(cel.Layer.Name, out var array)) {
          Point pos = imageCel.Location;
          array[i] = new Vector2(pos.X + 0.5f, pos.Y + 0.5f) * scale;
        }
      }
    }

    return result;
  }
}

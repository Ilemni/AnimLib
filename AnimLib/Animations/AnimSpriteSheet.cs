using JetBrains.Annotations;
using Terraria.DataStructures;

namespace AnimLib.Animations;

[PublicAPI]
public sealed class AnimSpriteSheet : IDisposable {
  private readonly AnimTag[] _tags;
  private readonly Dictionary<string, TextureAtlas> _atlases;
  private readonly Dictionary<string, AnimTag> _tagDictionary;
  private readonly Dictionary<string, Vector2[]> _points;

  public AnimSpriteSheet(Dictionary<string, TextureAtlas> atlases, AnimTag[] tags,
    Dictionary<string, Vector2[]> points, AnimUserData userData, Vector2 spriteSize) {
    (_tags, _atlases, _points, UserData, SpriteSize) = (tags, atlases, points, userData, spriteSize);

    _tagDictionary = [];
    foreach (AnimTag tag in tags) {
      _tagDictionary.Add(tag.Name, tag);
    }

    Upscaled = userData.HasFlag("upscale");
    Scale = Upscaled ? 2 : 1;

    Offset = new Vector2(
      userData.ArgOrDefault<float>("offsetX") * Scale,
      userData.ArgOrDefault<float>("offsetY") * Scale
    );

    Align = new Vector2(
      userData.ArgOrDefault("alignX", 0.5f),
      userData.ArgOrDefault("alignY", 1.0f)
    );

    Pivot = new Vector2(
      userData.ArgOrDefault<float>("pivotX") * Scale,
      userData.TryGetArg("pivotY", out float pivotY) ? pivotY * Scale : 22
    );

    if (Pivot.Y != 0) {
      Pivot.Y *= -1; // we want positive values in userdata to move the pivot upwards
    }
  }

  /// <summary>
  /// Animation tags as they appear in the Aseprite file in the program.
  /// </summary>
  public ReadOnlySpan<AnimTag> Tags => (ReadOnlySpan<AnimTag>)_tags;

  /// <summary>
  /// Animation tags as they appear in the Aseprite file in the program, indexable by animation name.
  /// </summary>
  public IReadOnlyDictionary<string, AnimTag> TagDictionary => _tagDictionary;

  /// <summary>
  /// Represents pairs of imported layer names and the texture atlas generated from the layer.
  /// </summary>
  public IReadOnlyDictionary<string, TextureAtlas> Atlases => _atlases;

  /// <summary>
  /// Represents pairs of Yellow layer names, and the single pixel position on each frame.
  /// If a frame was missing a pixel, the value will be the center of the sprite.
  /// </summary>
  public IReadOnlyDictionary<string, Vector2[]> Points => _points;

  /// <summary>
  /// Represents the user data of the Aseprite Sprite.
  /// </summary>
  public readonly AnimUserData UserData;

  /// <summary>
  /// Whether this sprite has been upscaled.
  /// </summary>
  public readonly bool Upscaled;

  /// <summary>
  /// The scale of the sprite.
  /// <br/> If <see cref="Upscaled"/> is <see langword="true"/>, this value is 2; otherwise, it is 1.
  /// </summary>
  public readonly int Scale;

  /// <summary>
  /// The original size of the Aseprite canvas.
  /// </summary>
  public readonly Vector2 SpriteSize;

  /// <summary>
  /// The visual offset of the sprite, defaulting to <see cref="Vector2.Zero"/>.
  /// <br/> Value is read from the Aseprite Sprite's user data, where the key is "offsetX" and "offsetY".
  /// <br/> If a value is defined, it is multiplied by <see cref="Scale"/>.
  /// <br/> A positive Y value will visually move the sprite downwards.
  /// </summary>
  /// <remarks>
  /// This offset is useful for when the character's feet are not at the bottom of the sprite.
  /// </remarks>
  public readonly Vector2 Offset;

  /// <summary>
  /// Location of the sprite's pivot point, relative to the bottom of the player.
  /// <br/> Value is read from the Aseprite Sprite's user data, where the key is "pivotX" and "pivotY".
  /// <br/> The X value defaults to 0.
  /// <br/> The Y value defaults to 22, roughly half of the vanilla player's unmodified height.
  /// A positive value will move the pivot upwards.
  /// <br/> If a value is defined, it is multiplied by <see cref="Scale"/>.
  /// </summary>
  public readonly Vector2 Pivot;

  /// <summary>
  /// Alignment of the sprite, similar to UI
  /// <see cref="Terraria.UI.UIElement.HAlign"/>/<see cref="Terraria.UI.UIElement.VAlign"/>.
  /// <br/> Value is read from the Aseprite Sprite's user data, where the key is "pivotX" and "pivotY".
  /// <br/> The X value defaults to 0.5, the center of the sprite.
  /// <br/> The Y value defaults to 1, the bottom of the sprite.
  /// </summary>
  public readonly Vector2 Align;

  internal bool HasSetFallbacks { get; private set; }

  public DrawData GetDrawData(ref readonly PlayerDrawSet drawInfo, string layer, AnimFrame frame,
    SpriteEffects? effects = null) {
    TextureAtlas atlas = GetAtlas(layer);

    SpriteEffects effectsValue = effects ?? drawInfo.playerEffect;
    bool doFlipX = (effectsValue & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
    bool doFlipY = (effectsValue & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
    Vector2 flip = new(doFlipX ? -1 : 1, doFlipY ? -1 : 1);

    Vector2 alignPos = SpriteSize * Align;
    Vector2 flipAlign = new(doFlipX ? 1 - Align.X : Align.X, doFlipY ? 1 - Align.Y : Align.Y);

    // Sprite rect is the original cel position/size in the Aseprite file, after upscaling
    Rectangle sprite = atlas.GetSpriteRect(frame.AtlasFrameIndex);
    if (doFlipX) {
      sprite.X = (int)(alignPos.X * 2 - sprite.X - sprite.Width);
    }

    if (doFlipY) {
      sprite.Y = (int)(alignPos.Y * 2 - sprite.Y - sprite.Height);
    }

    Vector2 localAlignedPos = drawInfo.drawPlayer.Size * flipAlign;
    Vector2 alignedPos = drawInfo.Position - Main.screenPosition + localAlignedPos;

    Vector2 position = alignedPos + (Offset + Pivot) * flip;
    Vector2 origin = -sprite.TopLeft() + alignPos + Pivot * flip;

    return new DrawData {
      texture = atlas.Texture,
      position = position,
      sourceRect = atlas.GetSourceRect(frame.AtlasFrameIndex),
      color = Color.White,
      origin = origin,
      scale = Vector2.One,
      effect = effectsValue
    };
  }

  public TextureAtlas GetAtlas(string layer) {
    ArgumentNullException.ThrowIfNull(layer);
    if (!_atlases.TryGetValue(layer, out TextureAtlas? atlas)) {
      throw new MissingLayerException(layer);
    }

    return atlas;
  }

  public bool HasLayer(string layer) {
    ArgumentNullException.ThrowIfNull(layer);
    return _atlases.ContainsKey(layer);
  }

  public AnimTag GetTag(string tagName) {
    ArgumentNullException.ThrowIfNull(tagName);
    if (!TryGetTag(tagName, out AnimTag? tag)) {
      throw new MissingTagException(tagName);
    }

    return tag;
  }

  public bool HasTag(string tagName) {
    ArgumentNullException.ThrowIfNull(tagName);
    return TryGetTag(tagName, out AnimTag? tag) && tag.Name == tagName;
  }

  /// <summary>
  /// Gets the <see cref="AnimTag"/> with the specified <see cref="AnimTag.Name"/>.
  /// </summary>
  /// <param name="tagName">The name of the <see cref="AnimTag"/> to retrieve.</param>
  /// <param name="tag">The resulting tag.</param>
  /// <returns>
  /// <see langword="true"/> if a tag with the specified name exists; otherwise, <see langword="false"/>.
  /// </returns>
  public bool TryGetTag(string tagName, [NotNullWhen(true)] out AnimTag? tag) {
    ArgumentNullException.ThrowIfNull(tagName);
    return _tagDictionary.TryGetValue(tagName, out tag);
  }

  public int FrameFromTimer(AnimationOptions animation, float seconds, bool loop = true) {
    ArgumentNullException.ThrowIfNull(animation.TagName);

    if (!_tagDictionary.TryGetValue(animation.TagName, out AnimTag? tag)) {
      throw new MissingTagException(animation.TagName);
    }

    var frames = tag.Frames;
    float totalSeconds = tag.TotalDuration;

    if (!loop && seconds >= totalSeconds) {
      return frames.Length - 1;
    }

    float duration = seconds % totalSeconds;

    int frameIndex;
    if (animation.IsReversed ?? tag.IsReversed) {
      frameIndex = frames.Length - 1;
      while (true) {
        duration -= frames[frameIndex].Duration;
        if (duration <= 0) {
          return frameIndex;
        }

        frameIndex--;
      }
    }

    frameIndex = 0;
    while (true) {
      duration -= frames[frameIndex].Duration;
      if (duration <= 0) {
        return frameIndex;
      }

      frameIndex++;
    }
  }

  public (AnimTag, int) FrameFromTimerSequence(ReadOnlySpan<AnimationOptions> animations, int ticks, bool loop = true) {
    return FrameFromTimerSequence(animations, ticks / 60f, loop);
  }

  public (AnimTag, int) FrameFromTimerSequence(ReadOnlySpan<AnimationOptions> animations, float seconds,
    bool loop = true) {
    float totalSequenceDuration = 0;
    AnimTag? tag;
    bool hasInfLoop = false;
    foreach (AnimationOptions animation in animations) {
      if (!_tagDictionary.TryGetValue(animation.TagName, out tag)) {
        throw new MissingTagException(animation.TagName);
      }

      int loopCount = animation.LoopCount ?? tag.LoopCount;
      float tagDuration = animation.FrameIndex is { } index and >= 0
        ? tag.Frames[index].Duration
        : tag.TotalDuration;
      totalSequenceDuration += tagDuration / animation.Speed * loopCount;
      if (loopCount == 0) {
        hasInfLoop = true;
        loop = false; // Inner infinite animation loop prevents sequence loop
      }
    }

    if (!loop && !hasInfLoop && seconds >= totalSequenceDuration) {
      tag = _tagDictionary[animations[^1].TagName];
      return (tag, tag.Frames.Length - 1);
    }

    float duration = hasInfLoop ? seconds : seconds % totalSequenceDuration;

    foreach (AnimationOptions animation in animations) {
      if (!_tagDictionary.TryGetValue(animation.TagName, out tag)) {
        throw new MissingTagException(animation.TagName);
      }

      int loopCount = animation.LoopCount ?? tag.LoopCount;
      float tagDuration = tag.TotalDuration / animation.Speed * loopCount;
      if (loopCount == 0 || duration < tagDuration) {
        return animation.FrameIndex is { } index and >= 0
          ? (tag, index)
          : (tag, FrameFromTimer(animation, duration * animation.Speed));
      }

      duration -= tagDuration;
    }

    tag = _tagDictionary[animations[^1].TagName];
    return (tag, tag.Frames.Length - 1);
  }

  public Vector2 GetPoint(string layer, int index) {
    ArgumentNullException.ThrowIfNull(layer);
    if (!_points.TryGetValue(layer, out var points)) {
      throw new MissingLayerException(layer);
    }

    ArgumentOutOfRangeException.ThrowIfNegative(index);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, points.Length);

    return points[index];
  }

  public AnimUserData GetCelUserData(string layer, AnimFrame frame) {
    return GetAtlas(layer).GetCelUserData(frame.AtlasFrameIndex);
  }

  public bool CelHasFlag(string layer, AnimFrame frame, string flag, bool checkFrame = true) {
    return (checkFrame && frame.UserData.HasFlag(flag)) ||
      GetCelUserData(layer, frame).HasFlag(flag);
  }

  /// <summary>
  /// Suggests whether any custom coloring should be ignored (that is, set to white).
  /// This value is <see langword="true"/> if either the
  /// frame's user data or cel user data contains the key <c>ignoreColor</c>.
  /// <para/> Note that this Ignore Color functionality is only enforced by AnimLib
  /// during <see cref="States.SkinAnimation.GetDrawData(ref readonly PlayerDrawSet, string, AnimFrame)"/>
  /// It is up to the mod developer to respect this suggestion.
  /// </summary>
  /// <param name="layer"></param>
  /// <param name="frame"></param>
  /// <returns>
  /// <see langword="true"/> if the color should be ignored; otherwise, <see langword="false"/>.
  /// </returns>
  public bool IgnoreColor(string layer, AnimFrame frame) {
    return CelHasFlag(layer, frame, "ignoreColor");
  }

  internal void SetFallbackTags(IReadOnlyCollection<(string optional, string fallback)> fallbacks) {
    if (HasSetFallbacks) {
      return;
    }

    HasSetFallbacks = true;
    // A specified fallback tag may itself be an optional tag that needs a fallback
    // Iterate until no changes are made or none missing
    while (true) {
      int changes = 0;
      int missingChanges = 0;
      foreach ((string optional, string fallback) in fallbacks) {
        if (_tagDictionary.ContainsKey(optional)) {
          continue;
        }

        if (_tagDictionary.TryGetValue(fallback, out AnimTag? fallbackTag)) {
          _tagDictionary[optional] = fallbackTag;
          changes++;
        }
        else {
          // It's possible that the fallback tag is optional and has a fallback later in the list
          missingChanges++;
        }
      }

      if (changes == 0 || missingChanges == 0) {
        break;
      }
    }
  }

  void IDisposable.Dispose() {
    foreach (TextureAtlas atlas in _atlases.Values) {
      atlas.Dispose();
    }
  }
}

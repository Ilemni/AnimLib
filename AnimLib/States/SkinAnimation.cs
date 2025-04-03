using System.Linq;
using AnimLib.Animations;
using AnimLib.Skins;
using Terraria.DataStructures;

namespace AnimLib.States;

public abstract class SkinAnimation : IIndexed {
  public EquippedSkinSlot Slot { get; internal set; } = null!; // EquippedSkins.ctor

  public virtual AnimCharacter Character { get; internal set; } = null!; // EquippedSkins.ctor

  public ushort Index { get; internal set; } = ushort.MaxValue; // EquippedSkins.ctor

  /// <summary>
  /// The player which this animation is associated with.
  /// </summary>
  public Player Player => Character.Player;

  /// <summary>
  /// The <see cref="AnimSpriteSheet"/> of the current <see cref="Skin"/>.
  /// <br/> This property is shorthand for <c>this.Slot.Skin.SpriteSheet</c>.
  /// </summary>
  public AnimSpriteSheet SpriteSheet => Slot.Skin.SpriteSheet;

  /// <summary>
  /// Current <see cref="AnimTag"/> being played.
  /// </summary>
  // Cannot store reference to AnimTag, as it may change when <see cref="SkinItem"/> changes.
  public AnimTag CurrentTag {
    get {
      if (SpriteSheet.TryGetTag(_currentTagName, out AnimTag? tag)) {
        return tag;
      }

      // TODO: make default tag configurable?
      tag = SpriteSheet.TagDictionary.Values.First();
      _currentTagName = tag.Name;
      return tag;
    }
    private set => _currentTagName = value.Name;
  }

  private string _currentTagName = string.Empty;

  /// <summary>
  /// Current <see cref="AnimFrame"/> being played.
  /// </summary>
  public AnimFrame CurrentFrame => CurrentTag.Frames[FrameIndex];

  /// <summary>
  /// Current index of the <see cref="AnimTag"/> being played.
  /// </summary>
  public int FrameIndex {
    get;
    private set {
      if (field != value) {
        field = value;
        FrameChangedThisTick = true;
      }
    }
  }

  /// <summary>
  /// Whether the value of <see cref="FrameIndex"/> has changed this tick.
  /// </summary>
  public bool FrameChangedThisTick { get; private set; }

  /// <summary>
  /// Current time of the <see cref="AnimFrame"/> being played, in seconds.
  /// </summary>
  public float FrameTime { get; private set; }

  /// <summary>
  /// The amount of times which this Animation has looped.
  /// </summary>
  public int TimesLooped { get; private set; }

  /// <summary>
  /// Current rotation the sprite is set to.
  /// </summary>
  public float SpriteRotation { get; private set; }

  /// <summary>
  /// Whether the animation is currently being played in reverse.
  /// </summary>
  public bool Reversed { get; private set; }

  /// <summary>
  /// <see cref="SpriteEffects"/> that will determine the flip directions of the sprite.
  /// </summary>
  public SpriteEffects? Effects { get; private set; }


  /// <summary>
  /// Here you would register any tags which should be considered optional.
  /// <br/> Attempting to play an optional tag which is missing in the current spritesheet
  /// will instead play the fallback tag.
  /// </summary>
  protected virtual IReadOnlyCollection<(string optional, string fallback)> RegisterFallbackTags() => [];

  internal void PreUpdateAnimation() {
    AnimSpriteSheet spriteSheet = SpriteSheet;
    if (!spriteSheet.HasSetFallbacks) {
      spriteSheet.SetFallbackTags(RegisterFallbackTags());
    }
  }

  protected internal virtual AnimationOptions? UpdateAnimation() => null;

  /// <summary>
  /// Animation logic to occur when animating in the UI.
  /// </summary>
  /// <param name="uiInfo">
  /// Information about the UI state that is animating the character.
  /// </param>
  public virtual void UpdateUIAnimation(AnimUiInfo uiInfo) {
  }

  public DrawData GetDrawData(ref readonly PlayerDrawSet drawInfo, string layer) {
    return GetDrawData(in drawInfo, layer, CurrentFrame);
  }

  private DrawData GetDrawData(ref readonly PlayerDrawSet drawInfo, string layer, AnimFrame frame) {
    SpriteEffects effects = Effects ?? drawInfo.playerEffect;

    DrawData drawData = SpriteSheet.GetDrawData(in drawInfo, layer, frame, effects);
    drawData.rotation = SpriteRotation;
    bool ignoreColor = SpriteSheet.IgnoreColor(layer, frame);
    if (!ignoreColor && Slot.Slot.LayerColors.TryGetValue(layer, out SkinSlot.LayerColorDelegate? colorFunc)) {
      (drawData.color, drawData.shader) = colorFunc(layer, in drawInfo, this);
    }

    return drawData;
  }

  public bool TryGetDrawData(ref readonly PlayerDrawSet drawInfo, string layer, out DrawData drawData) {
    if (SpriteSheet.HasLayer(layer)) {
      drawData = GetDrawData(in drawInfo, layer, CurrentFrame);
      return true;
    }

    drawData = default;
    return false;
  }

  /// <summary>
  /// Attempts to create a <see cref="DrawData"/> for the animation state at the specified <paramref name="frameIndex"/>.
  /// <para/> If the specified <paramref name="layer"/> is not present in the current <see cref="AnimSpriteSheet"/>,
  /// this method will return <see langword="false"/> and <paramref name="drawData"/> have default values.
  /// </summary>
  /// <param name="drawInfo">Parameter of <see cref="PlayerDrawLayer.Draw">PlayerDrawLayer.Draw</see></param>
  /// <param name="layer">The texture layer to draw.</param>
  /// <param name="tagName">The tag to use for animation.</param>
  /// <param name="frameIndex">The frame index to use for animation.</param>
  /// <param name="drawData">
  /// A <see cref="DrawData"/> which represents the sprite with the specified <paramref name="layer"/>
  /// and <paramref name="frameIndex"/>.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the <see cref="DrawData"/> was created successfully,
  /// </returns>
  public bool TryGetDrawData(ref readonly PlayerDrawSet drawInfo, string layer, string tagName, int frameIndex,
    out DrawData drawData) {
    if (SpriteSheet.HasLayer(layer)) {
      drawData = GetDrawData(in drawInfo, layer, SpriteSheet.GetTag(tagName).Frames[frameIndex]);
      return true;
    }

    drawData = default;
    return false;
  }

  /// <summary>
  /// Gets a <see cref="Rectangle"/> to use for <see cref="DrawData.sourceRect">DrawData.sourceRect</see>
  /// for the current animation state.
  /// </summary>
  /// <param name="layer">The layer, as named in the Aseprite file.</param>
  /// <returns></returns>
  /// <remarks>
  /// This returned value will likely differ between <paramref name="layer"/>s,
  /// even if the animation state is identical. Make sure you are also using the correct texture.
  /// </remarks>
  public Rectangle GetSourceRect(string layer) => SpriteSheet.GetAtlas(layer).GetSourceRect(CurrentFrame.AtlasFrameIndex);

  public Vector2 GetPoint(string layer) => SpriteSheet.GetPoint(layer, CurrentFrame.AtlasFrameIndex);

  public void UIAnimation(AnimationOptions options, int counter) {
    options.FrameIndex ??= SpriteSheet.FrameFromTimer(options, counter / 60f);
    UpdateAnimationInternal(options);
  }

  /// <summary>
  /// For use in UI, this will play an animation sequence based on the <see name="Character"/>'s UI properties.
  /// The counter for all tags will be <see cref="AnimUiInfo.CategoryAnimationCounter"/>.
  /// </summary>
  /// <param name="tagOptions">
  /// The sequence of tags to play.
  /// </param>
  /// <param name="counter">
  ///
  /// </param>
  /// <param name="syncLastTag">
  /// If <see langword="true"/>, the last tag in <paramref name="tagOptions"/> will instead be played
  /// with the counter <see cref="AnimUiInfo.AnimationCounter"/> instead of <paramref name="counter"/>.
  /// </param>
  public void UIAnimationSequence(ReadOnlySpan<AnimationOptions> tagOptions, int counter, bool syncLastTag) {
    AnimUiInfo uiInfo = Character.UiInfo;
    (AnimTag tag, int frameIndex) =
      SpriteSheet.FrameFromTimerSequence(tagOptions, counter, loop: false);
    if (syncLastTag && tag.Name == tagOptions[^1].TagName) {
      (tag, frameIndex) = SpriteSheet.FrameFromTimerSequence(tagOptions, uiInfo.AnimationCounter, loop: false);
    }

    UpdateAnimationInternal(new AnimationOptions(tag.Name) {
      FrameIndex = frameIndex
    });
  }

  /// <summary>
  /// For use in UI, this will play a looping animation sequence based on the <see name="Character"/>'s UI properties.
  /// The counter for all tags will be <see cref="AnimUiInfo.CategoryAnimationCounter"/>.
  /// </summary>
  /// <param name="tagOptions">
  /// The sequence of tags to play.
  /// </param>
  /// <param name="counter">
  /// The counter to get an animation frame based on.
  /// </param>
  public void UILoopedAnimationSequence(ReadOnlySpan<AnimationOptions> tagOptions, int counter) {
    (AnimTag tag, int frameIndex) = SpriteSheet.FrameFromTimerSequence(tagOptions, counter);
    UpdateAnimationInternal(new AnimationOptions(tag.Name) {
      FrameIndex = frameIndex
    });
  }

  /// <summary>
  /// Changes <see cref="CurrentTag"/> if needed, and
  /// assigns various properties of <paramref name="options"/> to member values.
  /// <para/> Calls <see cref="Play"/> when <see cref="AnimationOptions.FrameIndex"/> is <see langword="null"/>
  /// </summary>
  /// <param name="options"></param>
  /// <param name="delta"></param>
  /// <exception cref="ArgumentException"></exception>
  internal void UpdateAnimationInternal(AnimationOptions options, float delta = 1 / 60f) {
    string tagName = options.TagName;
    ArgumentNullException.ThrowIfNull(tagName);
    AnimTag tag = SpriteSheet.GetTag(tagName);

    if (options.FrameIndex.HasValue) {
      int index = options.FrameIndex.Value;
      ArgumentOutOfRangeException.ThrowIfNegative(index);
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, tag.Frames.Length);
    }

    ArgumentOutOfRangeException.ThrowIfNegative(options.Speed);

    // Set member values
    bool isNewTag = !ReferenceEquals(CurrentTag, tag);

    SpriteRotation = options.Rotation + (options.RotationOffset && !isNewTag ? SpriteRotation : 0);
    Effects = options.Effects;

    if (isNewTag) {
      SetTag(tag, options.IsReversed);
    }

    if (options.FrameIndex.HasValue) {
      FrameIndex = options.FrameIndex.Value;
      FrameTime = 0;
      return;
    }

    // Loop logic
    Play(options, delta);
  }

  /// <summary>
  /// Advances <see cref="FrameTime"/>.
  /// <para/> Advances <see cref="FrameIndex"/> if time exceeds <see cref="AnimFrame.Duration"/>.
  /// <para/> Advances <see cref="TimesLooped"/> if index exceeds end of frame.
  /// <para/> Flips <see cref="Reversed"/> if
  /// <see cref="AnimationOptions.IsPingPong">AnimationOptions.IsPingPong</see> or
  /// <see cref="AnimTag.IsPingPong">AnimTag.IsPingPong</see> is <see langword="true"/>.
  /// </summary>
  /// <param name="options"></param>
  /// <param name="delta"></param>
  private void Play(AnimationOptions options, float delta) {
    float duration = CurrentFrame.Duration;
    float newFrameTime = FrameTime + options.Speed * delta;

    // Do nothing if not enough time has passed to advance to the next frame
    if (newFrameTime < duration || duration <= 0) {
      FrameTime = newFrameTime;
      return;
    }

    AnimTag currentTag = CurrentTag;
    var frames = currentTag.Frames;
    int loopCount = options.LoopCount ?? currentTag.LoopCount;
    bool isReversed = options.IsReversed ?? currentTag.IsReversed;
    bool isPingPong = options.IsPingPong ?? currentTag.IsPingPong;

    int lastFrameIndex = Reversed ? 0 : frames.Length - 1;

    int newFrameIndex = FrameIndex;
    while (newFrameTime >= duration) {
      // Determine next frame
      bool endOfFrame = newFrameIndex == lastFrameIndex;
      if (endOfFrame) {
        if (loopCount > 0 && TimesLooped + 1 >= loopCount || frames.Length == 1) {
          // Do not change frame
          break;
        }

        TimesLooped++;

        // Ping-pong: flip state, otherwise set to argument or default state
        Reversed = isPingPong ? !Reversed : isReversed;
        newFrameIndex = Reversed ? frames.Length - 1 : 0;
        if (isPingPong) {
          // Skip first ping-pong frame, as it's same as last frame
          newFrameIndex += Reversed ? -1 : 1;
        }

        lastFrameIndex = Reversed ? 0 : frames.Length - 1;
      }
      else {
        newFrameIndex += !Reversed ? 1 : -1;
      }

      newFrameTime -= duration;
      duration = currentTag.Frames[newFrameIndex].Duration;
    }

    FrameIndex = newFrameIndex;
    FrameTime = newFrameTime;
  }

  private void SetTag(AnimTag tag, bool? isReversed = null) {
    CurrentTag = tag;
    FrameTime = 0;
    Reversed = isReversed ?? tag.IsReversed;
    FrameIndex = Reversed ? tag.Frames.Length - 1 : 0;
    TimesLooped = 0;
  }

  internal void PreUpdateInternal() {
    FrameChangedThisTick = false;
  }

  public void OnSkinChanged(Skin newSkin) {
    AnimSpriteSheet newSpriteSheet = newSkin.SpriteSheet;
    AnimTag tag = newSpriteSheet.TryGetTag(_currentTagName, out AnimTag? newTag)
      ? newTag
      // TODO: make default tag configurable?
      : newSpriteSheet.TagDictionary.Values.First();
    SetTag(tag);
  }
}

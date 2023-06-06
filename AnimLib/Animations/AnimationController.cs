using System;
using System.Collections.Generic;
using System.Linq;
using AnimLib.Compat;
using AnimLib.Extensions;
using AnimLib.Internal;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace AnimLib.Animations {
  /// <summary>
  /// This class plays various <see cref="Animation"/>s and manages advancement of frames.
  /// Your <see cref="AnimationController"/> is automatically created by <see cref="AnimLibMod"/> when a player is initialized.
  /// <para>For your mod, you must have exactly one class derived from <see cref="AnimationController"/>, else your player cannot be animated.</para>
  /// <para>To get your <see cref="AnimationController"/> instance on the player, use extension method <see cref="ModPlayerExtensions.GetAnimCharacter"/>.</para>
  /// </summary>
  /// <remarks>
  /// Alongside your <see cref="AnimationSource"/>s, that stores what animations are, such as their positions on spritesheets and duration,
  /// your <see cref="AnimationController"/> determines which track plays depending on whatever conditions you have, and how they are played.
  /// </remarks>
  [PublicAPI]
  [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
  public abstract class AnimationController {
    private string _trackName = "Default";
    // TODO: maybe automate playerlayers
    // There's a PR in tML to overhaul them to be more OoP, and that can go a long ways to automating them here.
    // Currently, PlayerLayers require IndexOf() for inserting in the list in ModifyDrawLayers to get desirable results.
    // That cannot be automated with desirable results without making this code clunky AF
    // Or making the assumption that all playerlayers should be inserted in the same point OriMod's do.
    // Let's not make that assumption.


    /// <summary>
    /// The <see cref="Animation"/> to retrieve track data from, such as frame duration. This <see cref="Animation"/>'s <see cref="AnimationSource"/>
    /// must contain all tracks that can be used.
    /// <para>By default this is the first <see cref="Animation"/> in <see cref="animations"/>.</para>
    /// </summary>
    public Animation MainAnimation { get; private set; }


    /// <summary>
    /// The name of the animation track currently playing. This value cannot be set to a null or whitespace value.
    /// </summary>
    /// <exception cref="ArgumentException">A set operation cannot be performed with a null or whitespace value.</exception>
    [NotNull] public string TrackName {
      get => _trackName;
      set {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{nameof(value)} cannot be empty.");
        if (value == _trackName) return;
        _trackName = value;
        Validate(value, true);
      }
    }

    /// <summary>
    /// Current index of the <see cref="Track"/> being played.
    /// </summary>
    public int FrameIndex { get; private set; }

    /// <summary>
    /// Current time of the <see cref="Frame"/> being played.
    /// </summary>
    public float FrameTime { get; internal set; }

    /// <summary>
    /// Current rotation the sprite is set to.
    /// </summary>
    public float SpriteRotation { get; private set; }

    /// <summary>
    /// Whether or not the animation is currently being played in reverse.
    /// </summary>
    public bool Reversed { get; private set; }

    /// <summary>
    /// <see cref="SpriteEffects"/> that will determine the flip directions of the sprite.
    /// </summary>
    public SpriteEffects Effects { get; private set; }

    /// <summary>
    /// All <see cref="Animation"/>s that belong to this mod.
    /// </summary>
    public Animation[] animations { get; internal set; }

    /// <summary>
    /// The <see cref="Player"/> that is being animated.
    /// </summary>
    public Player player { get; internal set; }

    /// <summary>
    /// The <see cref="Mod"/> that owns this <see cref="AnimationController"/>.
    /// </summary>
    public Mod mod { get; internal set; }

    internal void SetupAnimations() {
      var modSources = AnimLoader.AnimationSources[mod];
      animations = new Animation[modSources.Length];
      if (modSources.Length == 0) return;

      for (int i = 0; i < modSources.Length; i++) animations[i] = new Animation(this, modSources[i]);

      SetMainAnimation(animations[0]);
    }

    /// <summary>
    /// Allows you to do things after this <see cref="AnimationController"/> is constructed.
    /// Useful for getting references to <see cref="Animation"/>s via <see cref="GetAnimation{T}"/>.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Determines whether or not the animation should update. Return <see langword="false"/> to stop the animation from updating.
    /// Consider using "base.PreUpdate() and" when overriding to ensure compatibility modules proper operation.
    /// Returns <see langword="true"/> by default.
    /// </summary>
    /// <returns><see langword="true"/> to update the animation, or <see langword="false"/> to stop it.</returns>
    public virtual bool PreUpdate() => AnimationUpdEnabledCompat;

    /// <summary>
    /// Updates the player animation by one frame. This is where you choose what tracks are played, and how they are played.
    /// <para>
    /// You must make calls to <see cref="PlayTrack(string, int?, float?, int?, float, LoopMode?, Direction?, SpriteEffects?)"/>
    /// to continue or change the animation.
    /// </para>
    /// </summary>
    /// <example>
    /// Here is an example of updating the animation based on player movement.
    /// This code assumes your <see cref="MainAnimation"/> have tracks for "Running", "Jumping", "Falling", and "Idle".
    /// <code>
    /// public override void Update() {
    ///   if (Math.Abs(player.velocity.X) &gt; 0.1f) {
    ///     PlayTrack("Running");
    ///     return;
    ///   }
    ///   if (player.velocity.Y != 0) {
    ///     PlayTrack(player.velocity.Y * player.gravDir &lt; 0 ? "Jumping" : "Falling");
    ///     return;
    ///   }
    ///   PlayTrack("Idle");
    /// }
    /// </code>
    /// </example>
    public abstract void Update();


    /// <summary>
    /// Gets the <see cref="Animation"/> where the <see cref="AnimationSource"/> is of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="AnimationSource"/></typeparam>
    /// <returns>The <see cref="Animation"/> with the matching <see cref="AnimationSource"/>.</returns>
    /// <exception cref="ArgumentException"><typeparamref name="T"/> does not belong to this <see cref="mod"/>.</exception>
    [NotNull]
    public Animation GetAnimation<T>() where T : AnimationSource =>
      animations.FirstOrDefault(anim => anim.source is T)
      ?? throw new ArgumentException($"{typeof(T).Name} must be from the same mod as {mod.Name}");

    /// <summary>
    /// Sets the main <see cref="Animation"/> of this player to the given <see cref="Animation"/>.
    /// This can be useful for things like player transformations that use multiple <see cref="AnimationSource"/>s.
    /// </summary>
    /// <param name="animation">Animation to set this player's <see cref="MainAnimation"/> to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="animation"/> is null.</exception>
    public void SetMainAnimation([NotNull] Animation animation) => MainAnimation = animation ?? throw new ArgumentNullException(nameof(animation));

    /// <summary>
    /// Sets the main <see cref="Animation"/> of this player to the animation whose source is <typeparamref name="T"/>.
    /// This can be useful for things like player transformations that use multiple <see cref="AnimationSource"/>s.
    /// </summary>
    /// <typeparam name="T">Animation type to set.</typeparam>
    public void SetMainAnimation<T>() where T : AnimationSource => MainAnimation = GetAnimation<T>();


    /// <summary>
    /// Plays the <see cref="Track"/> of the given name, using default values.
    /// </summary>
    /// <param name="trackName">
    /// Name of the animation track to play/continue.
    /// <para>This must be a valid key in the <see cref="AnimationSource"/> for <see cref="MainAnimation"/>.</para>
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="trackName"/> was null or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">The value of <paramref name="trackName"/> was not a key in the main <see cref="AnimationSource.tracks"/>.</exception>
    protected void PlayTrack([NotNull] string trackName) => PlayTrack(trackName, null);

    /// <summary>
    /// Plays the <see cref="Track"/> with the given name. How the animation advances is based on the given input parameters.
    /// </summary>
    /// <param name="trackName">
    /// Name of the animation track to play/continue.
    /// <para>This must be a valid key in the <see cref="AnimationSource"/> for <see cref="MainAnimation"/>.</para>
    /// </param>
    /// <param name="frameIndex">
    /// The frame to play, -or- <see langword="null"/> to use the current <see cref="Frame"/>.
    /// <para>A non-<see langword="null"/> value prevents normal playback.</para>
    /// </param>
    /// <param name="speed">
    /// Speed to increase <see cref="FrameTime"/> by, -or- <see langword="null"/> to play at the default speed.
    /// <para>This must be a non-negative value. To play in reverse, use <paramref name="direction"/>.</para>
    /// </param>
    /// <param name="duration">
    /// Duration of the frame, -or- 0 to stop playback, -or- <see langword="null"/> to use the <see cref="Frame"/>'s duration.
    /// <para>This must be a positive value.</para>
    /// </param>
    /// <param name="rotation">
    /// Rotation of the sprite, in <strong>radians</strong>.
    /// <para>If degrees are necessary to work with, use <see cref="MathHelper.ToRadians(float)"/> for this parameter.</para>
    /// </param>
    /// <param name="direction">
    /// <see cref="Direction"/> to play the track in, -or- <see langword="null"/>, to use the current <see cref="Track"/>'s <see cref="Direction"/>.
    /// </param>
    /// <param name="loop">
    /// <see cref="LoopMode"/> to play the track with, -or- <see langword="null"/>, to use the current <see cref="Track"/>'s <see cref="LoopMode"/>.
    /// </param>
    /// <param name="effects">
    /// <see cref="SpriteEffects"/> that will determine the flip direction of the sprite, -or- <see langword="null"/>, to use the player directions.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="trackName"/> was <see langword="null"/> or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="frameIndex"/> is less than 0, or greater than the count of
    /// <paramref name="trackName"/>'s frames, -or- <paramref name="speed"/> was negative, -or- <paramref name="duration"/> was negative or 0.
    /// </exception>
    /// <exception cref="KeyNotFoundException">The value of <paramref name="trackName"/> was not a key in the main <see cref="AnimationSource.tracks"/>.</exception>
    // ReSharper disable once MethodOverloadWithOptionalParameter
    protected void PlayTrack([NotNull] string trackName, int? frameIndex = null, float? speed = null, int? duration = null,
      float rotation = 0, LoopMode? loop = null, Direction? direction = null, SpriteEffects? effects = null) {
      if (string.IsNullOrWhiteSpace(trackName)) throw new ArgumentException($"{nameof(trackName)} cannot be null or whitespace.", nameof(trackName));

      if (!Validate(trackName, false))
        throw new KeyNotFoundException($"\"{trackName}\" is not a valid key for the main Animation track {MainAnimation.source.GetType().Name}.");

      // If any arguments are true, they throw. They can only throw if specified by the user to an invalid value.
      if (frameIndex < 0 || frameIndex > MainAnimation.source[trackName].length)
        throw new ArgumentOutOfRangeException(nameof(frameIndex), $"{nameof(frameIndex)} must be between 0 and the length of {trackName}'s frame count.");

      if (speed < 0) throw new ArgumentOutOfRangeException(nameof(speed), $"{nameof(speed)} must be a positive value.");
      if (duration <= 0) throw new ArgumentOutOfRangeException(nameof(duration), $"{nameof(duration)} must be a positive, non-zero value.");

      FrameTime += speed ?? 1;
      SpriteRotation = rotation;
      if (effects is null) {
        Effects = SpriteEffects.None;
        if (player.direction < 0) Effects |= SpriteEffects.FlipHorizontally;
        if (player.gravDir < 0) Effects |= SpriteEffects.FlipVertically;
      }

      if (trackName != TrackName) SwitchTrack(trackName, direction);

      Track track = MainAnimation.source[trackName];
      var frames = track.frames;
      int lastFrame = frames.Length - 1;

      if (frameIndex >= 0 && frameIndex <= lastFrame) {
        FrameIndex = frameIndex.Value;
        FrameTime = 0;
      }

      if (AnimPlayer.Local?.DebugEnabled ?? false) {
        // TODO: replace Main.NewText spam with something better?
        Main.NewText($"Frame called: Tile [{MainAnimation.CurrentFrame.tile}], " +
                     $"{(MainAnimation.CurrentTrack.HasTextures ? $" {MainAnimation.CurrentTexture.Name}" : string.Empty)} " +
                     $"{TrackName}{(Reversed ? " (Reversed)" : "")} " +
                     $"Time: {FrameTime}, " +
                     $"AnimIndex: {FrameIndex}/{MainAnimation.CurrentTrack.length}");
      }

      // Loop logic
      PostPlay(duration, loop, direction);
    }


    private void PostPlay(int? overrideDuration, LoopMode? overrideLoopMode, Direction? overrideDirection) {
      Track track = MainAnimation.CurrentTrack;
      LoopMode loop = overrideLoopMode ?? track.loopMode;
      int duration = overrideDuration ?? MainAnimation.CurrentFrame.duration;
      Direction direction = overrideDirection ?? track.direction;

      if (FrameTime < duration || duration <= 0) return;

      int lastFrame = track.length - 1;

      int framesToAdvance = 0;
      while (FrameTime >= duration) {
        FrameTime -= duration;
        framesToAdvance++;
        if (framesToAdvance + FrameIndex > lastFrame) FrameTime %= duration;
      }

      switch (direction) {
        case Direction.Forward: {
          Reversed = false;
          if (FrameIndex == lastFrame) {
            if (loop == LoopMode.Always) FrameIndex = 0;
          }
          // Forward, middle of track: continue playing track forward
          else
            FrameIndex += framesToAdvance;

          break;
        }
        case Direction.PingPong: {
          // Ping-pong, always loop, reached start of track: play track forward
          if (FrameIndex == 0 && loop == LoopMode.Always) {
            Reversed = false;
            FrameIndex += framesToAdvance;
          }
          // Ping-pong, always loop, reached end of track: play track backwards
          else if (FrameIndex == lastFrame && loop == LoopMode.Always) {
            Reversed = true;
            FrameIndex -= framesToAdvance;
          }
          // Ping-pong, in middle of track: continue playing track either forward or backwards
          else
            FrameIndex += Reversed ? -framesToAdvance : framesToAdvance;

          break;
        }
        case Direction.Reverse: {
          Reversed = true;
          // Reverse, if loop: replay track backwards
          if (FrameIndex == 0) {
            if (loop == LoopMode.Always) FrameIndex = lastFrame;
          }
          // Reverse, middle of track: continue track backwards
          else
            FrameIndex -= framesToAdvance;

          break;
        }
      }

      FrameIndex = (int)MathHelper.Clamp(FrameIndex, 0, lastFrame);
    }

    private void SwitchTrack(string newTrack, Direction? direction = null) {
      if (newTrack == TrackName) return;
      if (!Validate(newTrack, false))
        throw new KeyNotFoundException($"\"{newTrack}\" is not a valid key for the main Animation track.");

      TrackName = newTrack;
      Track track = MainAnimation.source[newTrack];
      FrameTime = 0;
      Reversed = (direction ?? track.direction) == Direction.Reverse;
      FrameIndex = Reversed ? track.length - 1 : 0;
    }

    /// <summary>
    /// Check if the <see cref="Animation"/>s will be valid when the given track name.
    /// If <paramref name="updateValue"/> is <see langword="true"/>, all <see cref="Animation.Valid"/> states will be updated.
    /// Returns <see langword="true"/> if the main <see cref="Animation"/> is valid; otherwise, <see langword="false"/>.
    /// </summary>
    /// <param name="newTrackName">New value of <see cref="TrackName"/>.</param>
    /// <param name="updateValue">Whether or not to update <see cref="Animation.Valid"/>.</param>
    /// <returns><see langword="true"/> if the main <see cref="Animation"/> is valid; otherwise, <see langword="false"/>.</returns>
    private bool Validate(string newTrackName, bool updateValue) {
      if (!updateValue) return MainAnimation.CheckIfValid(newTrackName);

      foreach (Animation anim in animations) anim.CheckIfValid(newTrackName);

      return MainAnimation.Valid;
    }

    /// <summary>
    /// List names of <see cref="AnimCompatSystem"/>s active by default
    /// in order to block their work, when <see cref="AnimCharacter"/>
    /// with this <see cref="AnimationController"/> is active.
    /// </summary>
    public readonly HashSet<string> AnimCompatSystemBlocklist = new();

    private bool _graphicsDisabledDirtectly = false;
    private bool _animationUpdDisabledDirtectly = false;

    /// <summary>
    /// State of GraphicsDisable conditions since previous update's evaluation.
    /// If false, layers should be hidden (add to default visibility hook for
    /// your custom PlayerDrawLayer). if any of conditions return true,
    /// associated flag is turned to false, if none - to true
    /// </summary>
    public bool GraphicsEnabledCompat { get; private set; } = true;

    /// <summary>
    /// State of AnimationsUpdateDisable conditions since previous update's evaluation.
    /// If false, animations updates will be stopped if not overriden
    /// </summary>
    public bool AnimationUpdEnabledCompat { get; private set; } = true;

    /// <summary>
    /// Called at start phase of game state update
    /// evaluates conditions and sets appropriate condition flags
    /// </summary>
    internal void UpdateConditions()
    {
      if (!_graphicsDisabledDirtectly) 
      {
        GraphicsEnabledCompat = 
          !GlobalCompatConditions.EvaluateDisableGraphics(player);
      }
      if (!_animationUpdDisabledDirtectly)
      {
        AnimationUpdEnabledCompat =
          !GlobalCompatConditions.EvaluateDisableAnimationUpd(player);
      }
      UpdateCustomConditions();
    }

    /// <summary>
    /// Called at post phase of game state update
    /// evaluates conditions and sets appropriate condition flags
    /// </summary>
    internal void UpdateConditionsPost()
    {
      _graphicsDisabledDirtectly = false;
      _animationUpdDisabledDirtectly = false;
    }

    /// <summary>
    /// Allows to perform conditon evaluation actions
    /// for custom conditional actions
    /// called right after <see cref="AnimCompatSystem"/>'s conditions evaluation
    /// </summary>
    public virtual void UpdateCustomConditions() { }

    /// <summary>
    /// Use this for compatibility, if you want to trigger directly
    /// disabling of PlayerDrawLayers' changes
    /// (hiding vanilla layers and displaying game character)
    /// (as example, morph ball from NetroidMod should hide players' character)
    /// </summary>
    public void DisableGraphicsDirectly()
    {
      GraphicsEnabledCompat = false;
      _graphicsDisabledDirtectly = true;
    }

    /// <summary>
    /// Use this for compatibility, if you want to trigger directly
    /// disabling of animations of this controller updating
    /// </summary>
    public void DisableAnimationsUpdDirectly()
    {
      AnimationUpdEnabledCompat = false;
      _animationUpdDisabledDirtectly = true;
    }
  }
}

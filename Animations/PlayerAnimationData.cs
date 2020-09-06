using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace AnimLib.Animations {
  /// <summary>
  /// Container for various <see cref="Animation"/>s and data to be attached to an <see cref="AnimPlayer"/>. Manages advancement of frames.
  /// <para>For your mod, you must have exactly one class derived from <see cref="PlayerAnimationData"/>, else your player cannot be animated.</para>
  /// <para>Your <see cref="PlayerAnimationData"/> is automatically created by <see cref="AnimPlayer">AnimLib</see> when a player is initialized.
  /// To get your <see cref="PlayerAnimationData"/>, use <see cref="AnimLibMod.GetPlayerAnimationData{T}(Player)"/></para>
  /// </summary>
  public abstract class PlayerAnimationData {
    private PlayerAnimationData() { }

    /// <summary>
    /// Allows you to do things after this <see cref="PlayerAnimationData"/> is constructed.
    /// Useful for getting references to <see cref="Animation"/>s via <see cref="GetAnimation{T}"/>.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// All <see cref="Animation"/>s that belong to this mod.
    /// </summary>
    public Animation[] animations { get; internal set; }

    /// <summary>
    /// The <see cref="Player"/> that is being animated.
    /// </summary>
    public Player player { get; internal set; }

    /// <summary>
    /// The <see cref="Mod"/> that owns this <see cref="PlayerAnimationData"/>.
    /// </summary>
    public Mod mod { get; internal set; }

    /// <summary>
    /// The <see cref="Animation"/> to retrieve track data from, such as frame duration. This <see cref="Animation"/>'s <see cref="AnimationSource"/> must contain all tracks that can be used.
    /// <para>By default this is the first <see cref="Animation"/> in <see cref="animations"/>.</para>
    /// </summary>
    public Animation MainAnimation { get; private set; }

    /// <summary>
    /// Sets the main <see cref="Animation"/> of this player to the given <see cref="Animation"/>.
    /// This can be useful for things like player transformations that use multiple <see cref="AnimationSource"/>s.
    /// </summary>
    /// <param name="animation">Animation to set this player's <see cref="MainAnimation"/> to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="animation"/> is null.</exception>
    public void SetMainAnimation(Animation animation) {
      MainAnimation = animation ?? throw new ArgumentNullException(nameof(animation));
    }

    /// <summary>
    /// Sets the main <see cref="Animation"/> of this player to the animation whose source is <typeparamref name="T"/>.
    /// This can be useful for things like player transformations that use multiple <see cref="AnimationSource"/>s.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void SetMainAnimation<T>() where T : AnimationSource {
      var result = GetAnimation<T>();
      // This shouldn't ever be null
      if (result != null) {
        MainAnimation = result;
      }
    }

    /// <summary>
    /// The name of the animation track currently playing.
    /// </summary>
    public string TrackName {
      get => _trackName;
      set {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{nameof(value)} cannot be empty.");
        if (value != _trackName) {
          _trackName = value;
          Validate(value, true);
        }
      }
    }
    private string _trackName = "Default";

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
    /// Gets the <see cref="Animation"/> from this <see cref="animations"/> where its <see cref="AnimationSource"/> is of type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// In theory this shouldn't ever return <see langword="null"/>, unless this <see cref="PlayerAnimationData"/> was constructed prior to <see cref="AnimLibMod.PostSetupContent"/>.
    /// </remarks>
    /// <typeparam name="T">Type of <see cref="AnimationSource"/></typeparam>
    /// <returns>The <see cref="Animation"/> with the matching <see cref="AnimationSource"/>.</returns>
    /// <exception cref="ArgumentException"><typeparamref name="T"/> and <see cref="mod"/> are from different assemblies.</exception>
    public Animation GetAnimation<T>() where T : AnimationSource {
      foreach (var anim in animations) {
        if (anim.source is T) {
          return anim;
        }
      }
      string tAsmName = typeof(T).Assembly.FullName;
      string modAsmName = mod.Code.FullName;
      if (tAsmName != modAsmName) {
        throw new ArgumentException($"Assembly mismatch: {typeof(T)} is from {tAsmName}; this {nameof(PlayerAnimationData)} is from {modAsmName}");
      }
      AnimLibMod.Instance.Logger.Warn($"{GetType().Name}.GetAnimation<{typeof(T).Name}>() failed.");
      return null;
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
      if (!updateValue) {
        return MainAnimation.CheckIfValid(newTrackName);
      }

      foreach (var anim in animations) {
        anim.CheckIfValid(newTrackName, updateValue);
      }
      return MainAnimation.Valid;
    }

    /// <summary>
    /// Updates the player animation by one frame. This is where you choose what tracks are played, and how they are played.
    /// <para>You must make calls to <see cref="IncrementFrame(string, int?, float?, int?, float, LoopMode?, Direction?)"/>
    /// or its various overloads to continue or change the animation.</para>
    /// </summary>
    /// <example>
    /// Here is an example of updating the animation based on player movement.
    /// This code assumes your <see cref="MainAnimation"/> have tracks for "Running", "Jumping", "Falling", and "Idle".
    /// <code>
    /// public override void Update() {
    ///   if (Math.Abs(player.velocity.X) &gt; 0.1f) {
    ///     IncrementFrame("Running");
    ///     return;
    ///   }
    ///   if (player.velocity.Y != 0) {
    ///     IncrementFrame(player.velocity.Y * player.gravDir &lt; 0 ? "Jumping" : "Falling");
    ///     return;
    ///   }
    ///   IncrementFrame("Idle");
    /// }
    /// </code>
    /// </example>
    public abstract void Update();

    /// <summary>
    /// Plays the <see cref="Track"/> of the given name.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="trackName"/> was null or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">The value of <paramref name="trackName"/> was not a key in the main <see cref="AnimationSource.tracks"/>.</exception>
    protected void IncrementFrame(string trackName)
      => IncrementFrame(trackName, null, null, null, 0, null, null);

    /// <summary>
    /// Plays the <see cref="Track"/> with the given rotation.
    /// </summary>
    /// <inheritdoc cref="IncrementFrame(string, int?, float?, int?, float, LoopMode?, Direction?)"/>
    protected void IncrementFrame(string trackName, float rotation)
      => IncrementFrame(trackName, null, null, null, rotation, null, null);

    /// <summary>
    /// Plays the <see cref="Track"/> with the given speed.
    /// </summary>
    /// <inheritdoc cref="IncrementFrame(string, int?, float?, int?, float, LoopMode?, Direction?)"/>
    protected void IncrementFrame(string trackName, float? speed)
      => IncrementFrame(trackName, null, speed, null, 0, null, null);

    /// <summary>
    /// Plays the <see cref="Track"/> with the given speed and rotation.
    /// </summary>
    /// <inheritdoc cref="IncrementFrame(string, int?, float?, int?, float, LoopMode?, Direction?)"/>
    protected void IncrementFrame(string trackName, float? speed, float rotation)
      => IncrementFrame(trackName, null, speed, null, rotation, null, null);

    /// <summary>
    /// Plays the <see cref="Track"/> with the given frame. If <paramref name="frameIndex"/> is non-<see langword="null"/>, this prevents normal playback.
    /// </summary>
    /// <inheritdoc cref="IncrementFrame(string, int?, float?, int?, float, LoopMode?, Direction?)"/>
    protected void IncrementFrame(string trackName, int? frameIndex)
      => IncrementFrame(trackName, frameIndex, null, null, 0, null, null);

    /// <summary>
    /// Plays the <see cref="Track"/> with the given frame and rotation. This prevents normal playback.
    /// </summary>
    /// <inheritdoc cref="IncrementFrame(string, int?, float?, int?, float, LoopMode?, Direction?)"/>
    protected void IncrementFrame(string trackName, int? frameIndex, float rotation)
      => IncrementFrame(trackName, frameIndex, null, null, rotation, null, null);

    /// <summary>
    /// Plays the <see cref="Track"/> in the given <see cref="Direction"/>.
    /// </summary>
    /// <param name="trackName">Name of the animation track to play/continue. This must be a valid key in the <see cref="AnimationSource"/> for <see cref="MainAnimation"/>.</param>
    /// <param name="direction"><see cref="Direction"/> for the track to play.</param>
    /// <exception cref="ArgumentException"><paramref name="trackName"/> was null or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">The value of <paramref name="trackName"/> was not a key in the main <see cref="AnimationSource.tracks"/>.</exception>
    protected void IncrementFrame(string trackName, Direction? direction)
      => IncrementFrame(trackName, null, null, null, 0, null, direction);

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
    /// Duration of the frame, -or- <see langword="null"/> to use the <see cref="Frame"/>'s duration.
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
    /// <exception cref="ArgumentException"><paramref name="trackName"/> was null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameIndex"/> is less than 0, or greater than the count of <paramref name="trackName"/>'s frames, -or- <paramref name="speed"/> was negative, -or- <paramref name="duration"/> was negative or 0.</exception>
    /// <exception cref="KeyNotFoundException">The value of <paramref name="trackName"/> was not a key in the main <see cref="AnimationSource.tracks"/>.</exception>
    protected void IncrementFrame(string trackName, int? frameIndex, float? speed, int? duration, float rotation, LoopMode? loop, Direction? direction) {
      if (string.IsNullOrWhiteSpace(trackName)) {
        throw new ArgumentException($"{nameof(trackName)} cannot be null or whitespace.", nameof(trackName));
      }
      if (!Validate(trackName, false)) {
        throw new KeyNotFoundException($"\"{trackName}\" is not a valid key for the main Animation track {MainAnimation.source.GetType().Name}.");
      }
      // If any arguments are true, they throw. They can only throw if specified by the user to an invalid value.
      if (frameIndex < 0 || frameIndex > MainAnimation.source[trackName].Length) {
        throw new ArgumentOutOfRangeException(nameof(frameIndex), $"{nameof(frameIndex)} must be between 0 and the length of {trackName}'s frame count.");
      }
      if (speed < 0) {
        throw new ArgumentOutOfRangeException(nameof(speed), $"{nameof(speed)} must be a positive value.");
      }
      if (duration <= 0) {
        throw new ArgumentOutOfRangeException(nameof(duration), $"{nameof(duration)} must be a positive value.");
      }

      FrameTime += speed ?? 1;
      SpriteRotation = rotation;

      if (trackName != TrackName) {
        SwitchTrack(trackName, direction);
      }

      Track track = MainAnimation.source[trackName];
      IFrame[] frames = track.frames;
      int lastFrame = frames.Length - 1;

      if (frameIndex != null && frameIndex >= 0 && frameIndex <= lastFrame) {
        FrameIndex = frameIndex.Value;
        FrameTime = 0;
      }

      // Loop logic
      PostIncrementFrame(duration, loop, direction);
    }

    private void PostIncrementFrame(int? overrideDuration, LoopMode? overrideLoopMode, Direction? overrideDirection) {
      var track = MainAnimation.CurrentTrack;
      var loop = overrideLoopMode ?? track.loop;
      var duration = overrideDuration ?? MainAnimation.CurrentFrame.duration;
      var direction = overrideDirection ?? track.direction;

      if (FrameTime < duration || duration <= 0) {
        return;
      }
      int lastFrame = track.Length - 1;

      int framesToAdvance = 0;
      while (FrameTime >= duration) {
        FrameTime -= duration;
        framesToAdvance++;
        if (framesToAdvance + FrameIndex > lastFrame) {
          FrameTime %= duration;
        }
      }
      switch (direction) {
        case Direction.Forward: {
            Reversed = false;
            if (FrameIndex == lastFrame) {
              // Forward, end of track w/ transfer: transfer to next track
              if (loop == LoopMode.Transfer) {
                TrackName = track.transferTo;
                FrameIndex = 0;
                FrameTime = 0;
              }
              // Forward, end of track, always loop: replay track forward
              else if (loop == LoopMode.Always) {
                FrameIndex = 0;
              }
            }
            // Forward, middle of track: continue playing track forward
            else {
              FrameIndex += framesToAdvance;
            }
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
            else {
              FrameIndex += Reversed ? -framesToAdvance : framesToAdvance;
            }
            break;
          }
        case Direction.Reverse: {
            Reversed = true;
            // Reverse, if loop: replay track backwards
            if (FrameIndex == 0) {
              if (loop == LoopMode.Transfer) {
                TrackName = track.transferTo;
                FrameIndex = 0;
                FrameTime = 0;
              }
              if (loop == LoopMode.Always) {
                FrameIndex = lastFrame;
              }
            }
            // Reverse, middle of track: continue track backwards
            else {
              FrameIndex -= framesToAdvance;
            }
            break;
          }
      }
      FrameIndex = (int)MathHelper.Clamp(FrameIndex, 0, lastFrame);
    }

    private void SwitchTrack(string newTrack, Direction? direction = null) {
      if (newTrack != TrackName) {
        if (!Validate(newTrack, false)) {
          throw new KeyNotFoundException($"\"{newTrack}\" is not a valid key for the main Animation track.");
        }
        TrackName = newTrack;
        var track = MainAnimation.source[newTrack];
        FrameTime = 0;
        Reversed = (direction ?? track.direction) == Direction.Reverse;
        FrameIndex = Reversed ? (track.Length - 1) : 0;
      }
    }
  }
}

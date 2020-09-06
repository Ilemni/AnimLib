using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.ModLoader.Exceptions;

namespace AnimLib.Animations {
  /// <summary>
  /// Animation Track, stores frame values. This is constructed on startup, contains <see cref="Frame"/> data.
  /// </summary>
  public sealed class Track {
    /// <summary>
    /// Creates a track with <see cref="LoopMode.Always"/> and <see cref="Direction.Forward"/>, with a <see cref="Frame"/> array ranging from <paramref name="start"/> to <paramref name="end"/>.
    /// <para>The range is created along the Y axis, going downward.</para>
    /// </summary>
    /// <param name="start">First <see cref="Frame"/> of the track.</param>
    /// <param name="end">Last <see cref="Frame"/> of the track.</param>
    /// <returns>A new <see cref="Track"/> with the frames ranging from <paramref name="start"/> to <paramref name="end"/>.</returns>
    public static Track Range(Frame start, Frame end) => Range(LoopMode.Always, Direction.Forward, start, end);

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/> and using <see cref="Direction.Forward"/>, with a <see cref="Frame"/> array ranging from <paramref name="start"/> to <paramref name="end"/>.
    /// <para>The range is created along the Y axis, going downward.</para>
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="start">First <see cref="Frame"/> of the track.</param>
    /// <param name="end">Last <see cref="Frame"/> of the track.</param>
    /// <returns>A new <see cref="Track"/> with the frames ranging from <paramref name="start"/> to <paramref name="end"/>.</returns>
    public static Track Range(LoopMode loopMode, Frame start, Frame end) => Range(loopMode, Direction.Forward, start, end);

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/> and <see cref="Direction"/>, with a <see cref="Frame"/> array ranging from <paramref name="start"/> to <paramref name="end"/>.
    /// <para>The range is created along the Y axis, going downward.</para>
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="direction"><see cref="Direction"/> of the track.</param>
    /// <param name="start">First <see cref="Frame"/> of the track.</param>
    /// <param name="end">Last <see cref="Frame"/> of the track.</param>
    /// <returns>A new <see cref="Track"/> with the frames ranging from <paramref name="start"/> to <paramref name="end"/>.</returns>
    /// <exception cref="ArgumentException">The X values of <paramref name="start"/> and <paramref name="end"/> must be equal.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The Y value of <paramref name="start"/> must be less than the Y value of <paramref name="end"/>.</exception>
    public static Track Range(LoopMode loopMode, Direction direction, Frame start, Frame end) {
      // Fill range of frames
      // I.e. if given [(0,1), (0,4)], we make [(0,1), (0,2), (0,3), (0,4)]
      if (start.tile.X != end.tile.X) {
        throw new ArgumentException($"The X values of {nameof(start)} and {nameof(end)} must be equal, instead got {start.tile.X}, {end.tile.X}.");
      }
      if (start.tile.Y >= end.tile.Y) {
        throw new ArgumentOutOfRangeException($"The Y value of {nameof(start)} must be less than the Y value of {nameof(end)}, instead got {start.tile.Y}, {end.tile.Y}.");
      }
      var frames = new List<IFrame>();
      for (int y = start.tile.Y; y < end.tile.Y; y++) {
        frames.Add(new Frame(start.tile.X, y, start.duration));
      }
      frames.Add(end);
      var track = new Track(loopMode, direction, frames.ToArray());
      return track;
    }

    /// <summary>
    /// Creates a track that consists of a single <see cref="Frame"/>.
    /// </summary>
    /// <param name="frame">Assigns to <see cref="frames"/> as a single <see cref="Frame"/>.</param>
    /// <returns>A new <see cref="Track"/> with a single <see cref="Frame"/>.</returns>
    public static Track Single(Frame frame) => new Track(new Frame[] { frame });


    /// <summary>
    /// Creates a track using <see cref="LoopMode.Always"/> and <see cref="Direction.Forward"/>, and with the given <see cref="Frame"/> array.
    /// </summary>
    /// <inheritdoc cref="Track(LoopMode, Direction, Frame[])"/>
    public Track(Frame[] frames) : this(LoopMode.Always, Direction.Forward, frames) { }

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/>, using <see cref="Direction.Forward"/>, and the given <see cref="Frame"/> array.
    /// </summary>
    /// <inheritdoc cref="Track(LoopMode, Direction, Frame[])"/>
    public Track(LoopMode loopMode, Frame[] frames) : this(loopMode, Direction.Forward, frames) { }

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/>, <see cref="Direction"/>, and <see cref="Frame"/> array.
    /// <para>If you want to have your <see cref="Track"/> use multiple textures, use the constructor <see cref="Track(LoopMode, Direction, IFrame[])"/>.</para>
    /// </summary>
    /// <param name="loopMode">The <see cref="LoopMode"/> of the track.</param>
    /// <param name="direction">The <see cref="Direction"/> of the track.</param>
    /// <param name="frames">Assigns to <see cref="frames"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="frames"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="frames"/> is empty.</exception>
    public Track(LoopMode loopMode, Direction direction, Frame[] frames) {
      if (frames.Length == 0) {
        throw new ArgumentException($"{nameof(frames)} cannot be empty", nameof(frames));
      }
      loop = loopMode;
      this.direction = direction;
      this.frames = frames ?? throw new ArgumentNullException(nameof(frames));
      Length = frames.Length;
    }


    /// <summary>
    /// Creates a track using <see cref="LoopMode.Always"/> and <see cref="Direction.Forward"/>, and with the given <see cref="Frame"/> array.
    /// <inheritdoc cref="Track(LoopMode, Direction, IFrame[])"/>
    /// </summary>
    /// <inheritdoc cref="Track(LoopMode, Direction, IFrame[])"/>
    public Track(IFrame[] frames) : this(LoopMode.Always, Direction.Forward, frames) { }

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/>, <see cref="Direction"/>, and <see cref="Frame"/> array.
    /// <inheritdoc cref="Track(LoopMode, Direction, IFrame[])"/>
    /// </summary>
    /// <inheritdoc cref="Track(LoopMode, Direction, IFrame[])"/>
    public Track(LoopMode loopMode, IFrame[] frames) : this(loopMode, Direction.Forward, frames) { }

    /// <summary>
    /// <para>Using an <see cref="IFrame"/>[], this allows for multiple textures within the <see cref="Track"/>, by using either
    /// <see cref="Frame.WithTexture(string)"/> or <see cref="SwitchTextureFrame(byte, byte, ushort, string)"/>.</para>
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="direction"><see cref="Direction"/> of the track.</param>
    /// <param name="frames">Assigns to <see cref="frames"/> as a <see cref="Frame"/> array. All <see cref="SwitchTextureFrame"/>s will have their textures added to this <see cref="Track"/>, and all <see cref="IFrame"/>s will be cast to <see cref="Frame"/></param>
    /// <exception cref="ArgumentNullException"><paramref name="frames"/> is <see langword="null"/> -or- <paramref name="frames"/> contains a <see langword="null"/> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="frames"/> is empty.</exception>
    public Track(LoopMode loopMode, Direction direction, IFrame[] frames) {
      if (frames is null) {
        throw new ArgumentNullException(nameof(frames));
      }
      if (frames.Length == 0) {
        throw new ArgumentException($"{nameof(frames)} cannot be empty", nameof(frames));
      }

      loop = loopMode;
      this.direction = direction;
      var newFrames = new Frame[frames.Length];
      Length = newFrames.Length;
      // We want Frame[] instead of IFrame[]. Frame is a small struct, but IFrame[] treats them as reference types
      // Storing an IFrame[] would make AnimationSource significantly larger than it needs to be.
      this.frames = newFrames;

      for (int i = 0; i < frames.Length; i++) {
        switch (frames[i]) {
          case SwitchTextureFrame stf:
            if (!(stf.texturePath is null)) {
              // Structs... can't trust 'em to not have default values
              AddTexturePathToFrameIndex(stf.texturePath, i);
            }
            newFrames[i] = (Frame)stf;
            break;
          case Frame frame:
            newFrames[i] = frame;
            break;
          case IFrame f:
            newFrames[i] = new Frame(f.tile.X, f.tile.Y, f.duration);
            break;
          case null:
            throw new ArgumentNullException(nameof(frames), $"{nameof(frames)} contains a null value.");
        }
      }
    }


    /// <summary>
    /// All frames used for this track.
    /// </summary>
    public readonly Frame[] frames;

    /// <summary>
    /// The number of frames in this <see cref="Track"/>.
    /// </summary>
    public int Length { get; }

    /// <inheritdoc cref="LoopMode"/>
    public readonly LoopMode loop = LoopMode.Always;

    /// <inheritdoc cref="Direction"/>
    public readonly Direction direction = Direction.Forward;

    /// <summary>
    /// Whether or not this track uses any textures that are not from <see cref="AnimationSource.texture"/>.
    /// <para>This is only <see langword="true"/> if this track construction used <see cref="WithTexture(string)"/>, <see cref="Frame.WithTexture(string)"/>, or new <see cref="SwitchTextureFrame(byte, byte, ushort, string)"/></para>
    /// </summary>
    public bool hasTexture => !(texturePaths is null);

    private SortedDictionary<int, string> texturePaths;

    /// <summary>
    /// Optional spritesheet that may be used instead of <see cref="AnimationSource.texture"/>.
    /// <para>If any frame after or including the current frame (at <paramref name="frameIdx"/>) is a <see cref="SwitchTextureFrame"/>, that <see cref="SwitchTextureFrame.texturePath"/> will be returned.</para>
    /// <para>If this track uses its own <see cref="Texture2D"/> (assigned with <see cref="WithTexture(string)"/> during construction), that is returned. Otherwise, returns <see langword="null"/></para>
    /// </summary>
    /// <param name="frameIdx">Index of the <see cref="IFrame"/> currently being played.</param>
    /// <returns>A valid <see cref="Texture2D"/> if <see cref="AnimationSource.texture"/> should be overridden, else <see langword="null"/>.</returns>
    public Texture2D GetTexture(int frameIdx) {
      if (texturePaths is null) {
        return null;
      }

      frameIdx = (int)MathHelper.Clamp(frameIdx, 0, Length - 1);

      // Short-circuit if this frame has texture.
      if (texturePaths.ContainsKey(frameIdx)) {
        TryGetTexture(frameIdx, out var texture);
        return texture;
      }

      // Get the highest key before
      int currentIdx = -1;
      foreach (var key in texturePaths.Keys) {
        if (key > currentIdx && key < frameIdx) {
          currentIdx = key;
        }
      }
      if (currentIdx >= 0) {
        TryGetTexture(currentIdx, out var texture);
        return texture;
      }

      return null;
    }

    /// <summary>
    /// Assign a spritesheet that will be used instead of <see cref="AnimationSource.texture"/>.
    /// </summary>
    public Track WithTexture(string texturePath) {
      AddTexturePathToFrameIndex(texturePath, 0);
      return this;
    }

    private void AddTexturePathToFrameIndex(string texturePath, int frameIndex) {
      if (string.IsNullOrWhiteSpace(texturePath)) {
        throw new ArgumentException($"{nameof(texturePath)} cannot be null or empty.", nameof(texturePath));
      }
      if (frameIndex < 0 || frameIndex >= Length) {
        throw new ArgumentOutOfRangeException(nameof(frameIndex), $"{nameof(frameIndex)} must be non-negative and less than the length of tracks.");
      }

      if (texturePaths is null) {
        texturePaths = new SortedDictionary<int, string> { [frameIndex] = texturePath };
      }
      else if (texturePaths.ContainsKey(frameIndex)) {
        AnimLibMod.Instance.Logger.Warn("Cannot set the track's texture twice.");
      }
      else {
        texturePaths[frameIndex] = texturePath;
      }
    }

    /// <summary>
    /// Attempts to get the 
    /// </summary>
    /// <param name="frameIdx">Index of the frame. This value <strong>must</strong> be a key for <see cref="texturePaths"/>.</param>
    /// <param name="texture">The texture from the index of <see cref="texturePaths"/>, or the texture for "ModLoader/MysteryTile" if it does not exist.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"><paramref name="frameIdx"/> is not a valid key for <see cref="texturePaths"/>.</exception>
    private void TryGetTexture(int frameIdx, out Texture2D texture) {
      if (texturePaths is null) {
        texture = null;
        return;
      }
      if (!texturePaths.ContainsKey(frameIdx)) {
        throw new KeyNotFoundException("The specified value was not found in the texturePaths list.");
      }
      try {
        texture = ModContent.GetTexture(texturePaths[frameIdx]);
      }
      catch (MissingResourceException ex) {
        AnimLibMod.Instance.Logger.Error("Animation Track texture missing, replacing with tML's MysteryTile", ex);
        texturePaths[frameIdx] = "ModLoader/MysteryTile";
        texture = ModContent.GetTexture("ModLoader/MysteryTile");
      }
    }

    /// <summary>
    /// Animation track to transfer to, if <see cref="LoopMode.Transfer"/> is used.
    /// </summary>
    // Use: /// <param name="transferTo">Track to transfer to. Requires <paramref name="loop"/> to be <see cref="LoopMode.Transfer"/>.</param>
    public readonly string transferTo;
  }
}

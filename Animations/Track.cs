using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

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
    public Track(LoopMode loopMode, Direction direction, Frame[] frames) : this(loopMode, direction) {
      if (frames is null) {
        throw new ArgumentNullException(nameof(frames));
      }
      if (frames.Length == 0) {
        throw new ArgumentException($"{nameof(frames)} cannot be empty", nameof(frames));
      }

      this.frames = frames;
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
    /// <see cref="AnimationSource.F(string, int, int, int)"/> or <see cref="SwitchTextureFrame(byte, byte, ushort, string)"/>.</para>
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="direction"><see cref="Direction"/> of the track.</param>
    /// <param name="frames">Assigns to <see cref="frames"/> as a <see cref="Frame"/> array. All <see cref="SwitchTextureFrame"/>s will have their textures added to this <see cref="Track"/>, and all <see cref="IFrame"/>s will be cast to <see cref="Frame"/></param>
    /// <exception cref="ArgumentNullException"><paramref name="frames"/> is <see langword="null"/> -or- <paramref name="frames"/> contains a <see langword="null"/> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="frames"/> is empty.</exception>
    public Track(LoopMode loopMode, Direction direction, IFrame[] frames) : this(loopMode, direction) {
      if (frames is null) {
        throw new ArgumentNullException(nameof(frames));
      }
      if (frames.Length == 0) {
        throw new ArgumentException($"{nameof(frames)} cannot be empty", nameof(frames));
      }

      var newFrames = new Frame[frames.Length];
      Length = newFrames.Length;
      // We want Frame[] instead of IFrame[]. Frame is a small struct, but IFrame[] treats them as reference types
      // Storing an IFrame[] would make AnimationSource significantly larger than it needs to be.
      this.frames = newFrames;

      for (int i = 0; i < frames.Length; i++) {
        switch (frames[i]) {
          case SwitchTextureFrame stf:
            SetTextureAtFrameIndex(stf.texturePath, i);
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


    private Track(LoopMode loopMode, Direction direction) {
      this.loopMode = loopMode;
      this.direction = direction;
    }


    /// <summary>
    /// All frames used for this track.
    /// </summary>
    public readonly Frame[] frames;

    /// <summary>
    /// The number of frames in this <see cref="Track"/>.
    /// </summary>
    public readonly int Length;

    /// <inheritdoc cref="LoopMode"/>
    public readonly LoopMode loopMode = LoopMode.Always;

    /// <inheritdoc cref="Direction"/>
    public readonly Direction direction = Direction.Forward;


    /// <summary>
    /// Whether or not this track uses any textures that are not from <see cref="AnimationSource.texture"/>.
    /// <para>This is only <see langword="true"/> if this track construction used <see cref="WithTexture(string)"/>, <see cref="AnimationSource.F(string, int, int, int)"/>, or new <see cref="SwitchTextureFrame(byte, byte, ushort, string)"/></para>
    /// </summary>
    public bool hasTextures => !(textures is null);


    /// <summary>
    /// Optional spritesheet that may be used instead of <see cref="AnimationSource.texture"/>.
    /// <para>If any frame after or including the current frame (at <paramref name="frameIdx"/>) is a <see cref="SwitchTextureFrame"/>, that <see cref="SwitchTextureFrame.texturePath"/> will be returned.</para>
    /// <para>If this track uses its own <see cref="Texture2D"/> (assigned with <see cref="WithTexture(string)"/> during construction), that is returned. Otherwise, returns <see langword="null"/></para>
    /// </summary>
    /// <param name="frameIdx">Index of the <see cref="Frame"/> currently being played.</param>
    /// <returns>A valid <see cref="Texture2D"/> if <see cref="AnimationSource.texture"/> should be overridden, else <see langword="null"/>.</returns>
    public Texture2D GetTexture(int frameIdx) {
      if (textures is null) {
        return null;
      }

      frameIdx = (int)MathHelper.Clamp(frameIdx, 0, Length - 1);

      // Short-circuit if this frame has texture.
      if (textures.ContainsKey(frameIdx)) {
        return textures[frameIdx];
      }

      // Get the highest key before
      int idx = -1;
      foreach (var key in textures.Keys) {
        if (key > idx && key < frameIdx) {
          idx = key;
        }
      }

      if (textures.ContainsKey(idx)) {
        return textures[idx];
      }
      return null;
    }

    /// <summary>
    /// Assign a spritesheet to the first frame of this track that will be used instead of <see cref="AnimationSource.texture"/>.
    /// </summary>
    public Track WithTexture(string texturePath) {
      SetTextureAtFrameIndex(texturePath, 0);
      return this;
    }

    /// <summary>
    /// Adds an override texture path at the given frame index. A frame played at this or a later index will use the texture at that path.
    /// </summary>
    /// <param name="texturePath">Path to the texture, -or- <see langword="null"/> to use the <see cref="AnimationSource"/>'s texture.</param>
    /// <param name="frameIndex">Index of the frame that this texture will be used for.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameIndex"/> cannot be less than 0 or greater than the length of frames.</exception>
    public void SetTextureAtFrameIndex(string texturePath, int frameIndex) {
      if (frameIndex < 0 || frameIndex >= Length) {
        throw new ArgumentOutOfRangeException(nameof(frameIndex), $"{nameof(frameIndex)} must be non-negative and less than the length of frames.");
      }
      if (textures is null) {
        textures = new SortedDictionary<int, Texture2D>();
      }
      textures[frameIndex] = ModContent.GetTexture(texturePath);
    }

    internal SortedDictionary<int, Texture2D> textures;
  }
}

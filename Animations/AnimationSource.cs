using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace AnimLib.Animations {
  /// <summary>
  /// Contains all animation data for a single animation set. This animation data is used for all players. 
  /// <see cref="AnimationSource"/>s from all mods are collected and created during <see cref="AnimLibMod.PostSetupContent"/>.
  /// After initialization, values should not be modified.
  /// </summary>
  public abstract class AnimationSource {
    /// <summary>
    /// Base constructor. Ensures that this is not constructed on a server.
    /// </summary>
    /// <exception cref="InvalidOperationException">Animation classes are not allowed to be constructed on servers.</exception>
    protected AnimationSource() {
      if (!AnimLoader.UseAnimations) {
        throw new InvalidOperationException($"{GetType().Name} is not allowed to be constructed on servers.");
      }
    }

    /// <summary>
    /// Size of all sprites in the spritesheet.
    /// </summary>
    public abstract PointByte spriteSize { get; }

    /// <summary>
    /// All <see cref="Track"/>s in the animation set.
    /// </summary>
    public abstract Dictionary<string, Track> tracks { get; }


    /// <summary>
    /// Default spritesheet used for animations.
    /// <para>This may be overwritten if you have any of your <see cref="Track"/>s use their own textures.</para>
    /// </summary>
    public Texture2D texture { get; internal set; }

    /// <summary>
    /// The mod that this <see cref="AnimationSource"/> belongs to.
    /// </summary>
    public Mod mod { get; internal set; }

    /// <summary>
    /// Shorthand for accessing <see cref="tracks"/>.
    /// </summary>
    public Track this[string name] => tracks[name];


    /// <summary>
    /// Whether or not this <see cref="AnimationSource"/> should be used. Return <see langword="false"/> to prevent this from being used.
    /// Returns <see langword="true"/> by default.
    /// </summary>
    /// <param name="texturePath">The file name of this <see cref="AnimationSource"/>'s texture file in the mod loader's file space.</param>
    /// <returns><see langword="true"/> if you want this <see cref="AnimationSource"/> to be loaded; otherwise, false.</returns>
    public virtual bool Load(ref string texturePath) => true;


    /// <summary>
    /// Creates a track with <see cref="LoopMode.Always"/> and <see cref="Direction.Forward"/>, with a <see cref="Frame"/> array ranging from <paramref name="start"/> to <paramref name="end"/>.
    /// <para>The range is created along the Y axis, going downward.</para>
    /// </summary>
    /// <inheritdoc cref="Range(LoopMode, Direction, Frame, Frame)"/>
    protected Track Range(Frame start, Frame end) => Range(LoopMode.Always, Direction.Forward, start, end);

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/> and using <see cref="Direction.Forward"/>, with a <see cref="Frame"/> array ranging from <paramref name="start"/> to <paramref name="end"/>.
    /// <para>The range is created along the Y axis, going downward.</para>
    /// </summary>
    /// <inheritdoc cref="Range(LoopMode, Direction, Frame, Frame)"/>
    protected Track Range(LoopMode loopMode, Frame start, Frame end) => Range(loopMode, Direction.Forward, start, end);

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/> and <see cref="Direction"/>, with a <see cref="Frame"/> array ranging from <paramref name="start"/> to <paramref name="end"/>.
    /// <para>The range is created along the Y axis, going downward.</para>
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="direction"><see cref="Direction"/> of the track.</param>
    /// <param name="start">First <see cref="Frame"/> of the track.</param>
    /// <param name="end">Last <see cref="Frame"/> of the track. Must be in the same column as and below <paramref name="start"/>.</param>
    /// <returns>A new <see cref="Track"/> with the frames ranging from <paramref name="start"/> to <paramref name="end"/>.</returns>
    /// <exception cref="ArgumentException">The X values of <paramref name="start"/> and <paramref name="end"/> must be equal.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The Y value of <paramref name="start"/> must be less than the Y value of <paramref name="end"/>.</exception>
    protected Track Range(LoopMode loopMode, Direction direction, Frame start, Frame end) {
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
      var track = new Track(this, loopMode, direction, frames.ToArray());
      return track;
    }

    /// <summary>
    /// Creates a track that consists of a single <see cref="Frame"/>.
    /// </summary>
    /// <param name="frame">Assigns to <see cref="Track.frames"/> as a single <see cref="Frame"/>.</param>
    /// <returns>A new <see cref="Track"/> with a single <see cref="Frame"/>.</returns>
    protected Track Single(Frame frame) => new Track(this, new Frame[] { frame });


    /// <summary>
    /// <para>Shorthand for <see cref="Frame(byte, byte, ushort)"/></para>
    /// <inheritdoc cref="Frame(int, int, int)"/>
    /// </summary>
    /// <inheritdoc cref="Frame(int, int, int)"/>
    protected static Frame F(int x, int y, int duration = 0) => new Frame((byte)x, (byte)y, (ushort)duration);

    /// <summary>
    /// <para>Shorthand for <see cref="SwitchTextureFrame(byte, byte, ushort, string)"/>. Use this to switch the texture at this frame.</para>
    /// <inheritdoc cref="SwitchTextureFrame(byte, byte, ushort, string)"/>
    /// </summary>
    /// <inheritdoc cref="SwitchTextureFrame(byte, byte, ushort, string)"/>
    protected static SwitchTextureFrame F(string texturePath, int x, int y, int duration = 0) => new SwitchTextureFrame(x, y, duration, texturePath);
  }
}

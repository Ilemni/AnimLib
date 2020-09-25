using System;
using System.Collections.Generic;
using AnimLib.Internal;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace AnimLib.Animations {
  /// <summary>
  /// Contains all animation data for a single animation set. This animation data is used for all players. 
  /// <see cref="AnimationSource"/>s from all mods are collected and created during <see cref="AnimLibMod.PostSetupContent"/>.
  /// After initialization, values should not be modified.
  /// <para>To get your <see cref="AnimationSource"/> instance, use <see cref="AnimLibMod.GetAnimationSource{T}(Mod)"/>.</para>
  /// </summary>
  /// <remarks>
  /// Alongside your <see cref="AnimationController"/>, which determines <i>how</i> track are played,
  /// your <see cref="AnimationSource"/>s stores what the animations are, including their positions on spritesheets, duration, and other spritesheets.
  /// </remarks>
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

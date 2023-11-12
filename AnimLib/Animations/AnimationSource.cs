using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
  [PublicAPI]
  [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
  public abstract class AnimationSource {
    /// <summary>
    /// Size of all sprites in the spritesheet.
    /// </summary>
    public abstract PointByte spriteSize { get; }

    /// <summary>
    /// All <see cref="Track"/>s in the animation set.
    /// </summary>
    public abstract Dictionary<string, Track> tracks { get; }

    /// <summary>
    /// All <see cref="Texture2D"/>s this uses.
    /// </summary>
    internal static Dictionary<string, Asset<Texture2D>> texture_assets = new();


    /// <summary>
    /// Default spritesheet used for animations.
    /// <para>This may be overwritten if you have any of your <see cref="Track"/>s use their own textures.</para>
    /// </summary>
    // ReSharper disable once NotNullMemberIsNotInitialized
    [NotNull] public Asset<Texture2D> texture { get; internal set; }

    public Texture2D GetDefaultTexture() {
      if(!texture.IsLoaded) texture.Wait();
      return texture.Value;
    }

    /// <summary>
    /// The mod that this <see cref="AnimationSource"/> belongs to.
    /// </summary>
    // ReSharper disable once NotNullMemberIsNotInitialized
    [NotNull] public Mod mod { get; internal set; }

    /// <summary>
    /// Shorthand for accessing <see cref="tracks"/>.
    /// </summary>
    public Track this[[NotNull] string name] => tracks[name];


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
    protected static Frame F(int x, int y, int duration = 0) => new((byte)x, (byte)y, (ushort)duration);

    /// <summary>
    /// <para>Shorthand for <see cref="SwitchTextureFrame(byte, byte, ushort, string)"/>. Use this to switch the texture at this frame.</para>
    /// <inheritdoc cref="SwitchTextureFrame(byte, byte, ushort, string)"/>
    /// </summary>
    /// <inheritdoc cref="SwitchTextureFrame(byte, byte, ushort, string)"/>
    protected static SwitchTextureFrame F(string texturePath, int x, int y, int duration = 0) => new(x, y, duration, texturePath);

    /// <summary>
    /// <para>Shorthand for <see cref="SwitchTextureFrameAsset"/>. Use this to switch the texture at this frame.</para>
    /// <inheritdoc cref="SwitchTextureFrameAsset"/>
    /// </summary>
    /// <inheritdoc cref="SwitchTextureFrameAsset"/>
    protected static SwitchTextureFrameAsset F(Asset<Texture2D> textureAsset, int x, int y, int duration = 0) => new(x, y, duration, textureAsset);
  }
}

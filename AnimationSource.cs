using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace AnimLib {
  /// <summary> Base class for AnimationSources of different types.
  /// 
  /// Do not inherit from this class. Instead, use `AnimationSourcePlayer`, `AnimationSourceNpc`, or `AnimationSourceProjectile`
  /// </summary>
  public abstract class AnimationSource {

    /// <summary> Path to your texture. Include your Mod name in the path.
    /// 
    /// Used in `ModContent.GetTexture(TexturePath)`
    /// </summary>
    public abstract string TexturePath { get; }

    /// <summary> Size of the sprite. X and Y values represent width and height in pixels. </summary>
    public abstract Point TileSize { get; }

    /// <summary> Condition that must be met for this animation to draw. </summary>
    public virtual bool Condition(Player player) => true;
    
    /// <summary> All tracks in the animation. </summary>
    public abstract Dictionary<string, Track> Tracks { get; }
    
    /// <summary> Logic for determining which frame is played, and all other components of an animation. </summary>
    /// <param name="entity">Entity. Cast to Player, NPC, or Projectile depending on what you use.</param>
    /// <param name="trackName">Name of the track.</param>
    /// <param name="frame">Index of the active frame in the track.</param>
    /// <param name="time">How long the active frame has played for</param>
    /// <param name="dur">How long the active frame will play for before switching.</param>
    /// <param name="deg">Degreees the sprite is rotated</param>
    /// <param name="loopMode"></param>
    /// <param name="playback"></param>
    /// <param name="force">If the current track has TransferTo set, this will prevent switching back to the previous track when set to `false`. Defaults to `false`</param>
    public abstract void UpdateFrame(Entity entity, ref string trackName, ref int frame, ref double time, ref int dur, ref float deg, ref LoopMode loopMode, ref PlaybackMode playback, ref bool force);

    /// <summary> Default texture of this Source. </summary>
    public Texture2D Texture => _texture ?? (_texture = ModContent.GetTexture(TexturePath));
    private Texture2D _texture;

    /// <summary> Cache of `Tracks.Keys.ToArray()` </summary>
    public string[] TrackNames => _trackNames ?? (_trackNames = Tracks.Keys.ToArray());
    private string[] _trackNames;

    internal Mod mod;

    /// <summary> Shorthand for AnimationSource.Tracks[name] </summary>
    public Track this[string name] => Tracks[name];

    /// <summary> Shorthand for `new Header()`
    /// 
    /// If an optional parameter is not declared, it uses Header.Default, which is normally the desired header type.
    /// </summary>
    /// <param name="i">Init type of this track.</param>
    /// <param name="l">Loop mode of this track.</param>
    /// <param name="p">Playback of this track.</param>
    /// <param name="s">Source texture for this track, if this track uses a different texture than the Animation Source.
    /// 
    /// Most of the time this is left alone.</param>
    /// <param name="t">Track to transfer to when playback for this track ends.
    /// 
    /// Most of the time this is left alone.</param>
    protected static Header h(InitType i=InitType.Range, LoopMode l=LoopMode.Always, PlaybackMode p=PlaybackMode.Normal, string s=null, string t=null)
      => new Header(init:i, loop:l, playback:p, overrideTexturePath:s, transferTo:t);

    /// <summary> Shorthand for `new Frame()`
    /// </summary>
    /// <param name="x">Sprite tile X</param>
    /// <param name="y">Sprite tile Y</param>
    /// <param name="dur">Duration of the frame. If no value is provided, the frame does not transition on its own.</param>
    protected static Frame f(int x, int y, int dur=-1)
      => new Frame(x, y, dur);
  }
  
  /// <summary> AnimationSource for a ModPlayer </summary>
  public abstract class AnimationSourcePlayer : AnimationSource {

    /// <summary> Called in the PlayerLayer's delegate, with `AnimationPlayer` in scope.
    /// 
    /// By default, adds the Animation's default DrawData to Main.playerDrawData
    /// 
    /// `Main.playerDrawData.Add(anim.DefaultDrawData(drawInfo));`
    /// </summary>
    /// <param name="drawInfo"></param>
    /// <param name="anim"></param>
    public virtual void Draw(PlayerDrawInfo drawInfo, AnimationPlayer anim) {
      Main.playerDrawData.Add(anim.DefaultDrawData(drawInfo));
    }
  }

  /// <summary> AnimationSource for a ModNPC </summary>
  [System.Obsolete("NPC animations are not yet supported.")]
  public abstract class AnimationSourceNpc : AnimationSource {
    
    /// <summary> Function that is called for drawing </summary>
    public abstract Action<SpriteBatch> Action { get; }
  }

  /// <summary> AnimationSource for a ModProjectile </summary>
  [System.Obsolete("Projectile animations are not yet supported.")]
  public abstract class AnimationSourceProjectile : AnimationSource {

  }
}
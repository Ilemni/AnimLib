using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace AnimLib {

  /// <summary> How the Frame[] in a Track is initialized. Defaults to Range </summary>
  public enum InitType {
    
    /// <summary> Does not modify frames from how they are declared. </summary> 
    None = 0,
    
    /// <summary>
    /// Automatically add all frames in a frame between two or more declared frames, using duration of the first one.
    /// 
    /// Note that this only works when frames share the same X value.
    /// </summary>
    Range = 1
  }

  /// <summary> Determines if the track repeats or stops when reaching the last Frame. </summary>
  public enum LoopMode {
    
    /// <summary> No data. Use LoopMode.Once if you do not want looping. </summary>
    None = 0,
    
    /// <summary> After finishing the last frame, repeats the loop. </summary>
    Always = 1,
    
    /// <summary> Stays on the last frame. </summary>
    Once = 2,
  }
  
  /// <summary> Determines playback behavior </summary>
  public enum PlaybackMode {
    
    /// <summary> No data. Use PlaybackMode.Normal instead. </summary>
    None = 0,
    
    /// <summary> After reaching the last frame, returns to the first frame. </summary>
    Normal = 1,
    
    /// <summary> After reaching the last frame, plays in reverse until reaching the first frame. </summary>
    PingPong = 2,
    
    /// <summary> Plays only in reverse. </summary>
    Reverse = 3,
    
    /// <summary> Selects a random frame after transitioning. </summary>
    Random = 4,
  }

  /// <summary> Contains data for a track, and optional data that is different from the AnimationSource </summary>
  public class Header {
    
    /// <summary> Initialization behavior of this track </summary>
    public InitType Init;
    
    /// <summary> Looping behavior of this track </summary>
    public LoopMode Loop;
    
    /// <summary> Playback behavior of this track </summary>
    public PlaybackMode Playback;
    
    /// <summary> Texture of this track </summary>
    public Texture2D Texture => _texture ?? (TexturePath != null ? _texture = ModContent.GetTexture(TexturePath) : null);
    private Texture2D _texture;
    
    /// <summary> Path to texture of this track </summary>
    public string TexturePath { get; }
    
    /// <summary> The track this Track transfers to when playback ends </summary>
    public string TransferTo { get; }
    
    /// <summary> Creates a new Header. All parameters are optional. </summary>
    /// <param name="init">This header's InitType</param>
    /// <param name="loop">This header's LoopMode</param>
    /// <param name="playback">This header's Playback</param>
    /// <param name="transferTo">The track this header transfers to when playback ends</param>
    /// <param name="overrideTexturePath">The texture this track uses
    /// 
    /// Only use this if the texture is different from the AnimationSource</param>
    public Header(InitType init=InitType.Range, LoopMode loop=LoopMode.Always, PlaybackMode playback=PlaybackMode.Normal, string transferTo=null, string overrideTexturePath=null) {
      Init = init;
      Loop = loop;
      Playback = playback;
      TexturePath = overrideTexturePath;
    }
    
    /// <summary>
    /// Returns a new Header with this header data overwritten with other header data that is not None
    /// </summary>
    /// <param name="other">Other header</param>
    /// <returns></returns>
    internal Header CopySome(Header other) => new Header(
      other.Init != 0 ? other.Init : Init,
      other.Loop != 0 ? other.Loop : Loop,
      other.Playback != 0 ? other.Playback : Playback,
      other.TransferTo ?? TransferTo,
      other.TexturePath ?? TexturePath
    );
    
    /// <summary> Default header values. Loop is Always and Playback is Normal </summary>
    public static Header Default => new Header(InitType.Range, LoopMode.Always, PlaybackMode.Normal);
    
    /// <summary> Header where Loop and Playback are both None </summary>
    public static Header None => new Header(InitType.None, LoopMode.None, PlaybackMode.None);
    
    /// <summary> Returns Loop, Playback, and TexturePath</summary>
    public override string ToString()
      => $"Init: {Init} | Loop: {Loop} | Playback: {Playback} | Texture Path: {(TexturePath != null ? $"\"{TexturePath}\"" : "No texture")}";
  }
}

using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace BetterAnimations {
  /// <summary>
  /// How the Frame[] in a Track is initialized. Defaults to Range
  /// </summary>
  public enum InitType {
    None = 0,
    /// <summary>
    /// 
    /// </summary>
    Range = 1,
    /// <summary>
    /// 
    /// </summary>
    Select = 2,
  }
  /// <summary>
  /// Determines if the track repeats or stops when reaching the last Frame. Defaults to Always.
  /// </summary>
  public enum LoopMode {
    None = 0,
    Always = 1,
    Once = 2,
  }
  public enum PlaybackMode {
    None = 0,
    Normal = 1,
    PingPong = 2,
    Reverse = 3,
    Random = 4,
  }
  public class Header {
    public InitType Init;
    public LoopMode Loop;
    public PlaybackMode Playback;
    public Texture2D Texture { get; }
    public string TransferTo { get; }
    public Header(InitType init=InitType.None, LoopMode loop=LoopMode.None, PlaybackMode playback=PlaybackMode.None, string transferTo=null, string overrideTexturePath=null) {
      Init = init;
      Loop = loop;
      Playback = playback;
      if (!Main.dedServ && overrideTexturePath != null) Texture = ModLoader.GetMod("OriMod").GetTexture(overrideTexturePath);
    }
    public Header CopySome(Header other) {
      return new Header(
        other.Init != 0 ? other.Init : Init,
        other.Loop != 0 ? other.Loop : Loop,
        other.Playback != 0 ? other.Playback : Playback
      );
    }
    public static Header Default => new Header(InitType.Range, LoopMode.Always, PlaybackMode.Normal);
    public static Header None => new Header(InitType.None, LoopMode.None, PlaybackMode.None);
    public override string ToString()
      => $"Init: {Init} | Loop: {Loop} | Playback: {Playback}" + (Texture != null ? $" | Texture Path: \"{Texture.Name}\"" : "");
  }
}

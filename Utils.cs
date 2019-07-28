using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace AnimLib {
  /// <summary> Utility class that provides a variety of extension methods. </summary>
  public static class Utils {
    /// <summary> AnimLib Extension Method
    /// 
    /// Use a DrawData instead of multiple fields to draw.
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="d"></param>
    /// <param name="layerDepth"></param>
    public static void Draw(this SpriteBatch spriteBatch, DrawData d, int layerDepth=0) =>
      spriteBatch.Draw(d.texture, d.position, d.sourceRect, d.color, d.rotation, d.origin, d.scale, d.effect, layerDepth);

    /// <summary> AnimLib Extension Method. Call in ModifyDrawLayers
    /// 
    /// Used to hide vanilla PlayerLayers so they can be replaced with custom sprites.
    /// </summary>
    /// <param name="modPlayer"></param>
    /// <param name="hideBody">If the player body still shows or is hidden (default hidden)</param>
    /// <param name="hideMount">If the mount still shows or is hidden</param>
    /// <param name="hideWings">If wings still show or are hidden</param>
    /// <param name="hideAcc">If any accessories still show or are hidden</param>
    /// <param name="hideEffects">If any misc effects still show or are hidden</param>
    public static void HidePlayerLayers(this ModPlayer modPlayer, bool hideBody=true, bool hideMount=false, bool hideWings=false, bool hideAcc=false, bool hideEffects=false) {
      if (hideBody) {
        PlayerLayer.Arms.visible = false;
        PlayerLayer.Body.visible = false;
        PlayerLayer.Face.visible = false;
        PlayerLayer.Hair.visible = false;
        PlayerLayer.Head.visible = false;
        PlayerLayer.Legs.visible = false;
        PlayerLayer.Skin.visible = false;
      }
      if (hideMount) {
        PlayerLayer.MountBack.visible = false;
        PlayerLayer.MountFront.visible = false;
      }
      if (hideWings) {
        PlayerLayer.Wings.visible = false;
      }
      if (hideAcc) {
        PlayerLayer.BackAcc.visible = false;
        PlayerLayer.BalloonAcc.visible = false;
        PlayerLayer.FaceAcc.visible = false;
        PlayerLayer.FrontAcc.visible = false;
        PlayerLayer.HairBack.visible = false;      
        PlayerLayer.HandOnAcc.visible = false;
        PlayerLayer.HandOffAcc.visible = false;
        PlayerLayer.NeckAcc.visible = false;
        PlayerLayer.ShieldAcc.visible = false;
        PlayerLayer.ShoeAcc.visible = false;
        PlayerLayer.WaistAcc.visible = false;
      }
      if (hideEffects) {
        PlayerLayer.MiscEffectsBack.visible = false;
        PlayerLayer.MiscEffectsFront.visible = false;
        PlayerLayer.SolarShield.visible = false;
      }
    }

    internal static Frame[] ToFrames((int, int, int)[] tuples) {
      Frame[] frames = new Frame[tuples.Length];
      for (int i = 0; i < tuples.Length; i++) {
        frames[i] = new Frame(tuples[i].Item1, tuples[i].Item2, tuples[i].Item3);
      }
      return frames;
    }
    
    internal static Frame[] ToFramesNoDur((int, int)[] tuples) {
      Frame[] frames = new Frame[tuples.Length];
      for (int i = 0; i < tuples.Length; i++) {
        frames[i] = new Frame(tuples[i].Item1, tuples[i].Item2, -1);
      }
      return frames;
    }
  }
}
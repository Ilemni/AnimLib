using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace BetterAnimations {
  public enum SpriteType {
    Player = 1,
    Npc = 2,
    Projectile = 3,
    Dust = 4
  }

  public abstract class AnimationSource {
    public Texture2D Texture { get; }
    public Point TileSize { get; }
    public bool Synced { get; }
    public Dictionary<string, Track> Tracks { get; }

    public string[] TrackNames { get; }
    public Track this[string name] => Tracks[name];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="texture">Path to texture. Uses `ModContent.GetTexture(texture)`</param>
    /// <param name="x">The width of each tile</param>
    /// <param name="y">The height of each tile</param>
    /// <param name="synced">Determines if this animation is synced to all other animations for this entity.</param>
    /// <param name="tracks"></param>
    public AnimationSource(Mod mod, string texture, int x, int y, bool synced, Dictionary<string, Track> tracks) {
      if (!Main.dedServ) Texture = mod.GetTexture(texture);
      TileSize = new Point(x, y);
      Synced = synced;
      Tracks = tracks;
      TrackNames = tracks.Keys.ToArray();
    }
  }
  public class AnimationSourcePlayer : AnimationSource {
    public AnimationSourcePlayer(Mod mod, string texture, int x, int y, bool synced, PlayerLayer playerLayer, Dictionary<string, Track> tracks) : base(mod, texture, x, y, synced, tracks) {
      this.PlayerLayer = PlayerLayer;
    }

    public PlayerLayer PlayerLayer { get; }
  }
  public class AnimationSourceNpc : AnimationSource {
    public AnimationSourceNpc(Mod mod, string texture, int x, int y, bool synced, Func<SpriteBatch> func, Dictionary<string, Track> tracks) : base(mod, texture, x, y, synced, tracks) {

    }

    public Func<SpriteBatch> Func { get; }
  }
}
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace BetterAnimations {
  public abstract class Animation {
    private string _currentTrack;
    public string CurrentTrack {
      get => _currentTrack;
      set {
        if (_currentTrack != value) {
          OnTrackChange(value);
        }
        _currentTrack = value;
      }
    }
    public int CurrentFrame;
    public float CurrentRotation;
    
    public virtual AnimationSource Source { get; protected set; }
    public virtual Entity Owner { get; protected set; }
    public bool Valid { get; private set; }
    
    public Texture2D Texture => ActiveTrack.Header.Texture ?? Source.Texture;
    public Track ActiveTrack => Valid ? Source.Tracks[CurrentTrack] : Source.Tracks.First().Value;
    public Frame ActiveFrame => ActiveTrack[CurrentFrame < ActiveTrack.Frames.Length ? CurrentFrame : 0];
    public Rectangle ActiveTile => new Rectangle(ActiveFrame.Tile.X * Source.TileSize.X, ActiveFrame.Tile.Y * Source.TileSize.Y, Source.TileSize.X, Source.TileSize.Y);
    
    public void OnTrackChange(string name) => Valid = Source.Tracks.ContainsKey(name);
  }

  public class AnimationPlayer : Animation {
    public AnimationPlayer(AnimationSourcePlayer source, Player player) {
      base.Source = source;
      base.Owner = player;
    }

    public new AnimationSourcePlayer Source => base.Source as AnimationSourcePlayer;
    public new Player Owner => base.Owner as Player;

    public void Draw(List<PlayerLayer> layers, int idx=0, bool force=false) {
      if (Valid || force) layers.Insert(idx, (Source as AnimationSourcePlayer).PlayerLayer);
    }

    public DrawData DefaultDrawData(PlayerDrawInfo drawInfo) {
      Player player = drawInfo.drawPlayer;
      Vector2 pos = drawInfo.position - Main.screenPosition + player.Size / 2;
      Rectangle rect = ActiveTile;
      Vector2 orig = new Vector2(rect.Width / 2, rect.Height / 2 + 5 * player.gravDir);
      SpriteEffects effect = SpriteEffects.None;
      if (player.direction == -1) effect = effect | SpriteEffects.FlipHorizontally;
      if (player.gravDir == -1) effect = effect | SpriteEffects.FlipVertically;
      return new DrawData(Texture, pos, rect, Color.White, CurrentRotation, orig, 1, effect, 0);
    }
  }

  public class AnimationNpc : Animation {
    public AnimationNpc(AnimationSourceNpc source, Projectile proj) {
      base.Source = source;
      base.Owner = proj;
    }

    public new AnimationSourceNpc Source  => base.Source as AnimationSourceNpc;
    public new Projectile Owner => base.Owner as Projectile;

    public DrawData DefaultDrawData(Projectile projectile) {
      Vector2 pos = projectile.Center - Main.screenPosition;
      Rectangle rect = ActiveTile;
      Vector2 orig = new Vector2(rect.Width / 2, rect.Height / 2);
      return new DrawData(Texture, pos, rect, Color.White, CurrentRotation, orig, 1, SpriteEffects.None, 0);
    }

    public void Draw(SpriteBatch spriteBatch) {
      spriteBatch.Draw(DefaultDrawData(Owner));
    }
  }
}

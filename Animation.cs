using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace AnimLib {
  
  /// <summary> Base class for Animations of different types.
  /// 
  /// Do not inherit from this class. Instead, use `AnimationPlayer`, `AnimationNpc`, or `AnimationProjectile`
  /// </summary>
  public abstract class Animation {
    internal Animation(AnimationSource source, Entity entity) {
      Source = source;
      Owner = entity;
    }
    
    private string _currentTrack;
    internal string Track {
      get => _currentTrack;
      set {
        if (_currentTrack != value) {
          OnTrackChange(value);
          _currentTrack = value;
        }
      }
    }
    
    private int _currFrame;
    internal int FrameIdx {
      get => _currFrame;
      set => _currFrame = (int)MathHelper.Clamp(value, 0, ActiveTrack.Frames.Length);
    }
    
    /// <summary> Time since last frame change.</summary>
    public double Time;
    private double OldTime;

    /// <summary> Duration that `Time` must reach until next frame change. </summary>
    public int Duration;
    
    /// <summary> Current rotation of the sprite. </summary>
    public float Rotation;

    /// <summary> Current LoopMode of the animation. </summary>
    public LoopMode Loop;

    /// <summary> Current PlaybackMode of the animation. </summary>
    public PlaybackMode Playback;
    internal bool IsReversed;
    internal bool HasTransitioned;

    /// <summary> AnimationSource that this Animation is based on. </summary>
    public virtual AnimationSource Source { get; internal set; }
    internal virtual Entity Owner { get; }
    internal bool Valid { get; private set; }

    internal bool PreventDraw = false;
    
    internal Texture2D Texture => ActiveTrack.Header.Texture ?? Source.Texture;
    internal Track ActiveTrack => Valid ? Source.Tracks[Track] : Source.Tracks.First().Value;
    internal Frame ActiveFrame => ActiveTrack[FrameIdx < ActiveTrack.Frames.Length ? FrameIdx : 0];
    internal Rectangle ActiveTile => new Rectangle(ActiveFrame.Tile.X * Source.TileSize.X, ActiveFrame.Tile.Y * Source.TileSize.Y, Source.TileSize.X, Source.TileSize.Y);
    
    internal void OnTrackChange(string name) {
      Valid = Source.Tracks.ContainsKey(name);
      if (Valid) {
        var h = Source[name].Header;
        Loop = h.Loop;
        Playback = h.Playback;
      }
      else {
        AnimLib.Instance.Logger.Warn($"{name} is not a valid Track in {nameof(Source)}");
      }
    }

    internal void Update() {
      Rotation = MathHelper.ToDegrees(Rotation);
      Time += Main.time - OldTime;
      OldTime = Main.time;
      bool force = false;
      var newData = (Track, FrameIdx, Time, Duration, Rotation:MathHelper.ToDegrees(Rotation), Loop, Playback);
      
      Source.UpdateFrame(Owner, ref newData.Track, ref newData.FrameIdx, ref newData.Time, ref newData.Duration,
        ref newData.Rotation, ref newData.Loop, ref newData.Playback, ref force);

      newData.Rotation = MathHelper.ToRadians(newData.Rotation);
      
      bool trackChanged = Track != newData.Track;
      bool frameChanged = FrameIdx != newData.FrameIdx;
      bool timeChanged = Time != newData.Time;
      bool durChanged = Duration != newData.Duration;
      bool rotChanged = Rotation != newData.Rotation;
      bool loopChanged = Loop != newData.Loop;
      bool pbChanged = Playback != newData.Playback;

      if (loopChanged) Loop = newData.Loop;
      if (pbChanged) Playback = newData.Playback;
      
      if (trackChanged && (HasTransitioned ? force : true)) {
        Track = newData.Track;
        Rotation = 0;
        if (!loopChanged) Loop = ActiveTrack.Header.Loop;
        if (!pbChanged) Playback = ActiveTrack.Header.Playback;

        FrameIdx = frameChanged ? newData.FrameIdx : Playback == PlaybackMode.Reverse ? ActiveTrack.Frames.Length - 1 : 0;
        Time = 0;
        Duration = ActiveFrame.Duration;
        if (Playback != PlaybackMode.PingPong) IsReversed = Playback == PlaybackMode.Reverse;
      }
      if (!trackChanged && frameChanged) {
        FrameIdx = newData.FrameIdx;
        Time = 0;
        Duration = ActiveFrame.Duration;
        if (Playback != PlaybackMode.PingPong) IsReversed = Playback == PlaybackMode.Reverse;
      }
      if (timeChanged) Time = newData.Time;
      if (durChanged) Duration = newData.Duration;
      if (rotChanged) Rotation = newData.Rotation;

      if (!Valid) {
        if (Track != null && Source.Tracks.Keys.Count > 0) {
          Main.NewText("Error with animation: The animation sequence \"" + Track + "\" does not exist.", Color.Red);
        }
        Track = Source.Tracks.Keys.First();
        IsReversed = false;
        return;
      }
      if (trackChanged || frameChanged) {
        return;
      }
      if (Duration <= 0) return;

      // Check number of frames to advance
      Frame[] frames = ActiveTrack.Frames;
      int framesToAdvance = 0;
      while (Time > Duration) {
        Time -= Duration;
        framesToAdvance++;
        if (framesToAdvance + FrameIdx > frames.Length - 1) {
          Time = Time % Duration;
        }
      }
      if (framesToAdvance == 0) return;

      // Frame advance based on a variety of conditions... lots of if/else
      if (Playback == PlaybackMode.Normal) {
        // Regular normal
        if (FrameIdx < frames.Length - 1) {
          FrameIdx += framesToAdvance;
        }
        // Loop around
        else if (Loop != LoopMode.Once) {
          FrameIdx = 0;
        }
      }
      else if (Playback == PlaybackMode.PingPong) {
        // Loop from reversed to normal
        if (FrameIdx == 0 && Loop != LoopMode.Once) {
          IsReversed = false;
          FrameIdx += framesToAdvance;
        }
        // Loop from normal to reversed
        else if (FrameIdx == frames.Length - 1) {
          IsReversed = true;
          FrameIdx -= framesToAdvance;
        }
        // Regular PingPong
        else {
          FrameIdx += IsReversed ? -framesToAdvance : framesToAdvance;
        }
      }
      else if (Playback == PlaybackMode.Reverse) {
        // Regular reversed
        if (FrameIdx > 0) {
          FrameIdx -= framesToAdvance;
        }
        // Loop around
        else if (Loop != LoopMode.Once) {
          FrameIdx = frames.Length - 1;
        }
      }
      if (Rotation != 0) Main.NewText(this.ToString());
    }

    /// <summary> String containing source, track, frame, time, and rotation. </summary>
    public override string ToString() =>
      $"Source {Source.GetType()} Track {Track}{(Valid ? "" : " (Invalid)")} Frame {FrameIdx}/{ActiveTrack.Frames.Length} Time {Time}/{Duration} Rotation {Rotation}";
  }

  /// <summary> Animation for ModPlayers.
  /// 
  /// This keeps track of current data with an Animation.
  /// </summary>
  public class AnimationPlayer : Animation {
    internal AnimationPlayer(AnimationSourcePlayer source, Player player) : base(source, player) {
      this.PlayerLayer = new PlayerLayer(source.mod.Name, source.Texture.Name, delegate (PlayerDrawInfo drawInfo) {
        source.Draw(drawInfo, this);
      });
    }

    /// <summary> The AnimationSource of this Animation </summary>
    public new AnimationSourcePlayer Source => base.Source as AnimationSourcePlayer;

    /// <summary> PlayerLayer that is drawn </summary>
    public PlayerLayer PlayerLayer { get; }

    /// <summary> Draws this like a PlayerLayer. </summary>
    /// <param name="layers">Layers argument in ModifyDrawLayers</param>
    /// <param name="idx">Index used in `layers.Insert()`</param>
    /// <param name="force">Draw this even when the active track doesn't match any in the Source tracks (draws first track instead)</param>
    public void Draw(List<PlayerLayer> layers, int idx=0, bool force=false) {
      if (!Main.dedServ && (Valid || force)) layers.Insert(idx, this.PlayerLayer);
    }

    /// <summary>
    /// Gets DrawData with assigned values.
    /// 
    /// Assigns the texture, position, rectangle, rotation, origin, and SpriteEffect to correct values.
    /// 
    /// Defaults color to White, scale to 1, and InactiveDepthlayer to 0
    /// </summary>
    /// <param name="drawInfo"></param>
    /// <returns></returns>
    public DrawData DefaultDrawData(PlayerDrawInfo drawInfo) {
      Player player = drawInfo.drawPlayer;
      Vector2 pos = drawInfo.position - Main.screenPosition + player.Size / 2;
      Rectangle rect = ActiveTile;
      Vector2 orig = new Vector2(rect.Width / 2, rect.Height / 2 + 5 * player.gravDir);
      SpriteEffects effect = SpriteEffects.None;
      if (player.direction == -1) effect = effect | SpriteEffects.FlipHorizontally;
      if (player.gravDir == -1) effect = effect | SpriteEffects.FlipVertically;
      return new DrawData(Texture, pos, rect, Color.White, Rotation, orig, 1, effect, 0);
    }
  }

  /// <summary> Animation for ModNPCs
  /// 
  /// This keeps track of current data with an Animation.
  /// </summary>
  [System.Obsolete("NPC animations are not yet supported.")]
  public class AnimationNpc : Animation {
    internal AnimationNpc(AnimationSourceNpc source, NPC npc) : base(source, npc) { }

    internal new AnimationSourceNpc Source  => base.Source as AnimationSourceNpc;
    internal new NPC Owner => base.Owner as NPC;

    /// <summary> Gets DrawData with assigned values.
    /// 
    /// Assigns the texture, position, rectangle, rotation, and origin to correct values.
    /// 
    /// Defaults color to White, scale to 1, SpriteEffects to None, and InactiveDepthlayer to 0
    /// </summary>
    internal DrawData DefaultDrawData() {
      Vector2 pos = Owner.Center - Main.screenPosition;
      Rectangle rect = ActiveTile;
      Vector2 orig = rect.Size() / 2;
      return new DrawData(Texture, pos, rect, Color.White, Rotation, orig, 1, SpriteEffects.None, 0);
    }

    internal void Draw(SpriteBatch spriteBatch) {
      Source.Action.Invoke(spriteBatch);
    }
  }
}

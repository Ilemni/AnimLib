using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AnimLib.Animations {
  /// <summary>
  /// Animation for a single player. This class uses runtime data from a <see cref="PlayerAnimationData"/> to retrieve values from an <see cref="IAnimationSource"/>.
  /// </summary>
  public sealed class Animation {
    /// <summary>
    /// Creates a new instance of <see cref="Animation"/> for the given <see cref="AnimPlayer"/>, using the given <see cref="IAnimationSource"/> and rendering with <see cref="PlayerLayer"/>.
    /// </summary>
    /// <param name="container"><see cref="PlayerAnimationData"/> instance this will belong to.</param>
    /// <param name="source"><see cref="IAnimationSource"/> to determine which sprite is drawn.</param>
    /// <exception cref="System.InvalidOperationException">Animation classes are not allowed to be constructed on a server.</exception>
    public Animation(PlayerAnimationData container, IAnimationSource source) {
      if (Main.netMode == NetmodeID.Server) {
        throw new System.InvalidOperationException($"Animation classes are not allowed to be constructed on servers.");
      }
      this.playerData = container;
      this.source = source;
      CheckIfValid(container.TrackName);
    }

    /// <summary>
    /// Current <see cref="Texture2D"/> that is to be drawn.
    /// <para>If <see cref="Track.GetTexture(int)"/> is not <see langword="null"/>, that is returned, otherwise returns the <see cref="IAnimationSource"/>'s <see cref="Texture2D"/>.</para>
    /// </summary>
    public Texture2D CurrentTexture => CurrentTrack.GetTexture(playerData.FrameIndex) ?? source.Texture;

    /// <summary>
    /// Current <see cref="Track"/> that is being played.
    /// <para>If <see cref="PlayerAnimationData.TrackName"/> is not a valid track name, this returns the first <see cref="Track"/> in the <see cref="IAnimationSource"/>.</para>
    /// </summary>
    public Track CurrentTrack => Valid ? source.tracks[playerData.TrackName] : source.tracks.First().Value;

    /// <summary>
    /// Current <see cref="Frame"/> that is being played.
    /// <para>If <see cref="PlayerAnimationData.FrameIndex"/> is less than 0, this returns the first <see cref="Frame"/> in the <see cref="Track"/>.</para>
    /// <para>If <see cref="PlayerAnimationData.FrameIndex"/> is greater than the <see cref="Track"/> length, this returns the last <see cref="Frame"/> in the <see cref="Track"/>.</para>
    /// </summary>
    public IFrame CurrentFrame {
      get {
        var track = CurrentTrack;
        int idx = (int)MathHelper.Clamp(playerData.FrameIndex, 0, track.Length - 1);
        return track.frames[idx];
      }
    }

    /// <summary>
    /// Current <see cref="Frame"/>'s sprite position and size on the <see cref="GetCurrentTexture()"/>.
    /// </summary>
    public Rectangle CurrentTile {
      get {
        var size = source.spriteSize;
        var tile = CurrentFrame.tile;
        return new Rectangle(tile.X * size.X, tile.Y * size.Y, size.X, size.Y);
      }
    }

    /// <summary>
    /// Whether or not the current <see cref="PlayerAnimationData.TrackName"/> maps to a valid <see cref="Track"/> on this <see cref="IAnimationSource"/>.
    /// </summary>
    public bool Valid { get; private set; }

    /// <summary>
    /// <see cref="PlayerAnimationData"/> this <see cref="Animation"/> belongs to. This is used to get the current <see cref="Track"/>s and <see cref="Frame"/>s.
    /// </summary>
    public readonly PlayerAnimationData playerData;

    /// <summary>
    /// <see cref="IAnimationSource"/> used for this <see cref="Animation"/>.
    /// </summary>
    public readonly IAnimationSource source;

    /// <summary>
    /// Attempts to insert the <see cref="PlayerLayer"/> of this <see cref="Animation"/> to <paramref name="layers"/>. If <see cref="Valid"/> is <see langword="false"/>, this will fail and return <see langword="false"/>.
    /// </summary>
    /// <param name="layers">The <see cref="List{T}"/> of <see cref="PlayerLayer"/> to insert in.</param>
    /// <param name="playerLayer"><see cref="PlayerLayer"/> to use for this <see cref="Animation"/>.</param>
    /// <param name="idx">Position to insert into.</param>
    /// <returns><see langword="true"/> if <paramref name="playerLayer"/> was inserted, otherwise <see langword="false"/>.</returns>
    public bool TryInsertInLayers(List<PlayerLayer> layers, PlayerLayer playerLayer, int idx = 0) {
      if (Valid) {
        layers.Insert(idx, playerLayer);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Updates <see cref="Valid"/> by checking if <paramref name="name"/> is a valid track.
    /// </summary>
    /// <param name="name">Track name to check.</param>
    /// <param name="updateValue">Whether or not to set <see cref="Valid"/> to the result of this method.</param>
    public bool CheckIfValid(string name, bool updateValue = true) {
      bool result = source.tracks.ContainsKey(name);
      if (updateValue) Valid = source.tracks.ContainsKey(name);
      return result;
    }

    /// <summary>
    /// Gets a <see cref="DrawData"/> that is based on this <see cref="Animation"/>.
    /// <list type="bullet">
    /// <item><see cref="DrawData.texture"/> is <see cref="Texture"/> (recommended)</item>
    /// <item><see cref="DrawData.position"/> is the center of the <see cref="PlayerDrawInfo.drawPlayer"/>, in screen-space. (recommended)</item>
    /// <item><see cref="DrawData.sourceRect"/> is <see cref="CurrentTile"/> (recommended)</item>
    /// <item><see cref="DrawData.rotation"/> is <see cref="Entity.direction"/> <see langword="*"/> <see cref="PlayerAnimationData.SpriteRotation"/> (recommended)</item>
    /// <item><see cref="DrawData.origin"/> is half of <see cref="CurrentTile"/>'s size, plus (5 * <see cref="Player.gravDir"/>) on the Y axis. Feel free to modify this.</item>
    /// <item><see cref="DrawData.effect"/> is based on <see cref="Entity.direction"/> and <see cref="Player.gravDir"/>. (recommended)</item>
    /// </list>
    /// </summary>
    /// <param name="drawInfo">Parameter of <see cref="PlayerLayer(string, string, System.Action{PlayerDrawInfo})"/>.</param>
    /// <returns></returns>
    public DrawData GetDrawData(PlayerDrawInfo drawInfo) {
      Player player = drawInfo.drawPlayer;
      Texture2D texture = CurrentTexture;
      Vector2 pos = drawInfo.position - Main.screenPosition + player.Size / 2;
      Rectangle rect = CurrentTile;
      var orig = new Vector2(rect.Width / 2, rect.Height / 2 + 5 * player.gravDir);
      SpriteEffects effect = SpriteEffects.None;
      if (player.direction == -1) {
        effect |= SpriteEffects.FlipHorizontally;
      }

      if (player.gravDir == -1) {
        effect |= SpriteEffects.FlipVertically;
      }

      return new DrawData(texture, pos, rect, Color.White, player.direction * playerData.SpriteRotation, orig, 1, effect, 0);
    }
  }
}

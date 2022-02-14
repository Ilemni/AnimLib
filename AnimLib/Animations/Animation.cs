using System;
using System.Collections.Generic;
using System.Linq;
using AnimLib.Internal;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace AnimLib.Animations {
  /// <summary>
  /// Animation for a single player.
  /// This class uses runtime data from a <see cref="AnimationController"/> to retrieve values from an <see cref="AnimationSource"/>.
  /// One of these will be created for each <see cref="AnimationController"/> you have in your mod, per player.
  /// <para>To get an <see cref="Animation"/> instance from the player, use <see cref="AnimationController.GetAnimation{T}"/>.</para>
  /// </summary>
  /// <remarks>
  /// This class is essentially the glue between your <see cref="AnimationController"/> and all your <see cref="AnimationSource"/>.
  /// </remarks>
  [PublicAPI]
  public sealed class Animation {
    /// <summary>
    /// <see cref="AnimationController"/> this <see cref="Animation"/> belongs to. This is used to get the current <see cref="Track"/>s and
    /// <see cref="Frame"/>s.
    /// </summary>
    [NotNull] public readonly AnimationController controller;

    /// <summary>
    /// <see cref="AnimationSource"/> database used for this <see cref="Animation"/>.
    /// </summary>
    [NotNull] public readonly AnimationSource source;

    /// <summary>
    /// Creates a new instance of <see cref="Animation"/> for the given <see cref="AnimPlayer"/>, using the given <see cref="AnimationSource"/> and
    /// rendering with <see cref="PlayerLayer"/>.
    /// </summary>
    /// <param name="controller"><see cref="AnimationController"/> instance this will belong to.</param>
    /// <param name="source"><see cref="AnimationSource"/> to determine which sprite is drawn.</param>
    /// <exception cref="System.InvalidOperationException">Animation classes are not allowed to be constructed on a server.</exception>
    internal Animation([NotNull] AnimationController controller, [NotNull] AnimationSource source) {
      if (!AnimLoader.UseAnimations) throw new InvalidOperationException("Animation classes are not allowed to be constructed on servers.");
      this.controller = controller;
      this.source = source;
      CheckIfValid(controller.TrackName);
    }


    /// <summary>
    /// Current <see cref="Track"/> that is being played.
    /// <para>
    /// If <see cref="AnimationController.TrackName"/> is not a valid track name, this returns the first <see cref="Track"/> in the
    /// <see cref="AnimationSource"/>.
    /// </para>
    /// </summary>
    public Track CurrentTrack => Valid ? source.tracks[controller.TrackName] : source.tracks.First().Value;

    /// <summary>
    /// Current <see cref="Frame"/> that is being played.
    /// <para>If <see cref="AnimationController.FrameIndex"/> is less than 0, this returns the first <see cref="Frame"/> in the <see cref="Track"/>.</para>
    /// <para>
    /// If <see cref="AnimationController.FrameIndex"/> is greater than the <see cref="Track"/> length, this returns the last <see cref="Frame"/>
    /// in the <see cref="Track"/>.
    /// </para>
    /// </summary>
    public Frame CurrentFrame => CurrentTrack.GetClampedFrame(controller.FrameIndex);

    /// <summary>
    /// Current <see cref="Frame"/>'s sprite position and size on the <see cref="CurrentTexture"/>.
    /// </summary>
    public Rectangle CurrentTile => CurrentFrame.ToRectangle(source);

    /// <summary>
    /// Current <see cref="Texture2D"/> that is to be drawn.
    /// <para>
    /// If <see cref="Track.GetTexture(int)"/> is not <see langword="null"/>, that is returned; otherwise, returns the
    /// <see cref="AnimationSource"/>'s <see cref="Texture2D"/>.
    /// </para>
    /// </summary>
    public Texture2D CurrentTexture => CurrentTrack.GetTexture(controller.FrameIndex) ?? source.texture;

    /// <summary>
    /// Whether or not the current <see cref="AnimationController.TrackName"/> maps to a valid <see cref="Track"/> on this <see cref="AnimationSource"/>.
    /// </summary>
    public bool Valid { get; private set; }

    /// <summary>
    /// Gets the sprite position and size of the <see cref="Frame"/> at the given index of the current <see cref="Track"/>.
    /// If you want to get the rect of the current <see cref="Frame"/>, use <see cref="CurrentTile"/> instead.
    /// </summary>
    /// <param name="frameIndex">Index of the frame to get.</param>
    /// <returns>The <see cref="Rectangle"/> of the <see cref="Frame"/> at the given index.</returns>
    public Rectangle TileAt(int frameIndex) => TileAt(CurrentTrack, frameIndex);

    /// <summary>
    /// Gets the sprite position and size of the <see cref="Frame"/> at the given index of the given <see cref="Track"/>.
    /// If you want to get the rect of the current <see cref="Frame"/>, use <see cref="CurrentTile"/> instead.
    /// </summary>
    /// <param name="frameIndex">Index of the frame to get.</param>
    /// <param name="track">Track that the <paramref name="frameIndex"/> is in.</param>
    /// <returns>The <see cref="Rectangle"/> of the <see cref="Frame"/> at the given index.</returns>
    public Rectangle TileAt(Track track, int frameIndex) => track.GetClampedFrame(frameIndex).ToRectangle(source);


    /// <summary>
    /// Gets a <see cref="DrawData"/> that is based on this <see cref="Animation"/>.
    /// <list type="bullet">
    /// <item><see cref="DrawData.texture"/> is <see cref="CurrentTexture"/> (recommended)</item>
    /// <item><see cref="DrawData.position"/> is the center of the <see cref="PlayerDrawInfo.drawPlayer"/>, in screen-space. (recommended)</item>
    /// <item><see cref="DrawData.sourceRect"/> is <see cref="CurrentTile"/> (recommended)</item>
    /// <item><see cref="DrawData.rotation"/> is <see cref="Entity.direction"/> <see langword="*"/> <see cref="AnimationController.SpriteRotation"/> (recommended)</item>
    /// <item><see cref="DrawData.origin"/> is half of <see cref="CurrentTile"/>'s size, plus (5 * <see cref="Player.gravDir"/>) on the Y axis. Feel free to modify this.</item>
    /// <item><see cref="DrawData.effect"/> is based on <see cref="Entity.direction"/> and <see cref="Player.gravDir"/>. (recommended)</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// If your sprites are asymmetrical and cannot be flipped (i.e. Samus from Metroid),
    /// you should modify <see cref="DrawData.effect"/> and <see cref="DrawData.rotation"/> to get your desired effect.
    /// If your sprites are not correctly positioned in the world, you may need to tweak <see cref="DrawData.origin"/>.
    /// </remarks>
    /// <param name="drawInfo">Parameter of <see cref="PlayerLayer(string, string, System.Action{PlayerDrawInfo})"/>.</param>
    /// <returns>A <see cref="DrawData"/> based on this <see cref="Animation"/>.</returns>
    public DrawData GetDrawData(PlayerDrawInfo drawInfo) {
      Player player = drawInfo.drawPlayer;
      Texture2D texture = CurrentTexture;
      Vector2 pos = drawInfo.position - Main.screenPosition + player.Size / 2;
      Rectangle rect = CurrentTile;
      SpriteEffects effect = controller.Effects;
      Vector2 orig = new Vector2(rect.Width / 2f, rect.Height / 2f);

      return new DrawData(texture, pos, rect, Color.White, controller.SpriteRotation, orig, 1, effect, 0);
    }

    /// <summary>
    /// Attempts to add the given <see cref="PlayerLayer"/> to <paramref name="layers"/>.
    /// If <see cref="Valid"/> is <see langword="false"/>, this will do nothing and return <see langword="false"/>.
    /// </summary>
    /// <param name="layers">The <see cref="List{T}"/> of <see cref="PlayerLayer"/> to insert in.</param>
    /// <param name="playerLayer"><see cref="PlayerLayer"/> to use for this <see cref="Animation"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="playerLayer"/> was inserted; otherwise, <see langword="false"/>.</returns>
    public bool TryAddToLayers(List<PlayerLayer> layers, PlayerLayer playerLayer) {
      if (Valid) layers.Add(playerLayer);
      return Valid;
    }

    /// <summary>
    /// Attempts to insert the given <see cref="PlayerLayer"/> to <paramref name="layers"/>. If <see cref="Valid"/> is <see langword="false"/>,
    /// this will do nothing and return <see langword="false"/>.
    /// </summary>
    /// <param name="layers">The <see cref="List{T}"/> of <see cref="PlayerLayer"/> to insert in.</param>
    /// <param name="playerLayer"><see cref="PlayerLayer"/> to use for this <see cref="Animation"/>.</param>
    /// <param name="idx">Position to insert the <paramref name="playerLayer"/> into.</param>
    /// <returns><see langword="true"/> if <paramref name="playerLayer"/> was inserted; otherwise, <see langword="false"/>.</returns>
    public bool TryAddToLayers(List<PlayerLayer> layers, PlayerLayer playerLayer, int idx) {
      if (Valid) layers.Insert(idx, playerLayer);
      return Valid;
    }


    /// <summary>
    /// Check if <paramref name="name"/> is a valid track, without changing <see cref="Valid"/>.
    /// </summary>
    /// <param name="name">Track name to check.</param>
    internal bool CheckIfValidNoUpdate(string name) => source.tracks.ContainsKey(name);

    /// <summary>
    /// Updates <see cref="Valid"/> by checking if <paramref name="name"/> is a valid track.
    /// </summary>
    /// <param name="name">Track name to check.</param>
    internal bool CheckIfValid(string name) => Valid = source.tracks.ContainsKey(name);
  }
}

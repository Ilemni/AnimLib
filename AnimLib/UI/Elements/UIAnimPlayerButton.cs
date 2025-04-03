using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace AnimLib.UI.Elements;

/// <summary>
/// Base class for a button which operates on <see cref="AnimCharacter"/>s.
/// </summary>
public abstract class UIAnimPlayerButton : UIElement {
  public readonly Player Player;
  public readonly AnimCharacterCollection Characters;

  private readonly Asset<Texture2D> _basePanel;
  private readonly Asset<Texture2D> _border;
  private readonly Asset<Texture2D> _hoveredBorder;

  private bool _soundedHover;

  protected abstract bool IsSelected { get; }

  protected UIAnimPlayerButton(Player player) {
    Player = player;
    Characters = player.GetState<AnimCharacterCollection>();

    Width = StyleDimension.FromPixels(44f);
    Height = StyleDimension.FromPixels(44f);

    _basePanel = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanel");
    _border = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanelHighlight");
    _hoveredBorder = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanelBorder");
  }

  protected override void DrawSelf(SpriteBatch spriteBatch) {
    base.DrawSelf(spriteBatch);
    if (IsMouseHovering && !_soundedHover) {
      SoundEngine.PlaySound(in SoundID.MenuTick);
    }

    _soundedHover = IsMouseHovering;

    CalculatedStyle dimensions = GetDimensions();
    int w = (int)dimensions.Width;
    int h = (int)dimensions.Height;
    int x = (int)dimensions.X;
    int y = (int)dimensions.Y;
    Utils.DrawSplicedPanel(spriteBatch, _basePanel.Value, x, y, w, h, 10, 10, 10, 10, Color.White * 0.5f);

    if (IsSelected) {
      Utils.DrawSplicedPanel(spriteBatch, _border.Value, x + 3, y + 3, w - 6, h - 6, 10, 10, 10, 10, Color.White);
    }

    DrawContents(spriteBatch);

    if (IsMouseHovering) {
      Utils.DrawSplicedPanel(spriteBatch, _hoveredBorder.Value, x, y, w, h, 10, 10, 10, 10, Color.White);
    }
  }

  /// <summary>
  /// Draws the contents of the button, over the background panel and under the border.
  /// </summary>
  /// <param name="spriteBatch"></param>
  protected virtual void DrawContents(SpriteBatch spriteBatch) {
  }


  public override void LeftMouseDown(UIMouseEvent evt) {
    SoundEngine.PlaySound(in SoundID.MenuTick);
    base.LeftMouseDown(evt);
  }

  protected void GetIconRects(Texture2D icon, out Rectangle source, out Rectangle dest) {
    Rectangle uiRect = GetInnerDimensions().ToRectangle();
    dest = uiRect;
    source = icon.Bounds;

    if (source.Width < dest.Width && source.Height < dest.Height) {
      dest.X += (dest.Width - source.Width) / 2;
      dest.Y += (dest.Height - source.Height) / 2;
      dest.Width = source.Width;
      dest.Height = source.Height;
      return;
    }

    if (source.Width > dest.Width || source.Height > dest.Height) {
      float scale = Math.Min((float)dest.Width / source.Width, (float)dest.Height / source.Height);
      dest.Width = (int)(source.Width * scale);
      dest.Height = (int)(source.Height * scale);
      dest.X += (uiRect.Width - dest.Width) / 2;
      dest.Y += (uiRect.Height - dest.Height) / 2;
    }
  }
}

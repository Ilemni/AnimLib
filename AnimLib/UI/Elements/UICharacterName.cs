using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace AnimLib.UI.Elements;

public sealed class UICharacterName : UIElement {
  private readonly Player _player;

  private readonly Asset<Texture2D> _basePanel;
  private readonly Asset<Texture2D> _hoveredBorder;

  private readonly UIText _name;

  private bool _hovered;
  private bool _soundedHover;

  public UICharacterName(Player player) {
    _player = player;
    Width = StyleDimension.FromPixels(400f);
    Height = StyleDimension.FromPixels(40f);
    _basePanel = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanel");
    _hoveredBorder = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanelBorder");

    UIText title = new("Character:") {
      HAlign = 0f,
      VAlign = 0.5f,
      Left = StyleDimension.FromPixels(10f)
    };
    Append(title);

    _name = new UIText(GetCharacterName()) {
      HAlign = 0f,
      VAlign = 0.5f,
      Left = StyleDimension.FromPixels(title.GetDimensions().ToRectangle().Right + 10f),
      TextOriginX = 0f
    };
    Append(_name);
  }

  protected override void DrawSelf(SpriteBatch spriteBatch) {
    base.DrawSelf(spriteBatch);
    if (_hovered && !_soundedHover) {
      SoundEngine.PlaySound(in SoundID.MenuTick);
    }

    _soundedHover = _hovered;

    CalculatedStyle dimensions = GetDimensions();
    int x = (int)dimensions.X;
    int y = (int)dimensions.Y;
    int w = (int)dimensions.Width;
    int h = (int)dimensions.Height;
    Utils.DrawSplicedPanel(spriteBatch, _basePanel.Value, x, y, w, h, 10, 10, 10, 10, Color.White * 0.5f);
    if (_hovered) {
      Utils.DrawSplicedPanel(spriteBatch, _hoveredBorder.Value, x, y, w, h, 10, 10, 10, 10, Color.White);
    }

    _name.SetText(GetCharacterName());
  }

  public override void MouseOver(UIMouseEvent evt) {
    base.MouseOver(evt);
    _hovered = true;
  }

  public override void MouseOut(UIMouseEvent evt) {
    base.MouseOut(evt);
    _hovered = false;
  }

  private string GetCharacterName() {
    AnimCharacter? character = _player.GetActiveCharacter();
    return character?.DisplayName ?? character?.Name ?? "None";
  }
}

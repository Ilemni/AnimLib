using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AnimLib.UI.Elements;

public sealed class UIAnimCharacterButton : UIAnimPlayerButton {
  /// <summary> Colors used by the null character (vanilla character/human)</summary>
  private static readonly AnimCharacterStyle DefaultStyle = new() {
    HairColor = new Color(215, 90, 55),
    SkinColor = new Color(255, 125, 90),
    EyeColor = new Color(105, 90, 75),
    ShirtColor = new Color(175, 165, 140),
    UnderShirtColor = new Color(160, 180, 215),
    PantsColor = new Color(255, 230, 175),
    ShoeColor = new Color(160, 105, 60)
  };

  /// <summary> Character which this Button would assign. </summary>
  public readonly AnimCharacter? Character;

  private readonly UICharacter _char;

  /// <summary> Character which is currently selected by the user. </summary>
  private AnimCharacter? _realCharacter;

  private int _realSkinVariant;
  private int _realHair;
  private readonly AnimCharacterStyle _realStyle = new();

  protected override bool IsSelected {
    get {
      if (_realCharacter is null || Character is null) {
        return _realCharacter is null && Character is null;
      }

      return _realCharacter.Index == Character.Index;
    }
  }

  public UIAnimCharacterButton(Player player, AnimCharacter? character) : base(player) {
    Character = character is not null ? (AnimCharacter)player.GetState(character) : null;
    Width = StyleDimension.FromPixels(44f);
    Height = StyleDimension.FromPixels(80f);

    _char = new UICharacter(Player, hasBackPanel: false) {
      HAlign = 0.5f,
      VAlign = 0.5f
    };
    Append(_char);
  }

  public override void Draw(SpriteBatch spriteBatch) {
    // Store
    GetRealValues();

    // Override
    SetCharacterValues();
    base.Draw(spriteBatch);

    // Restore
    SetRealValues();
  }

  public override void LeftClick(UIMouseEvent evt) {
    Characters.SetCharacter(Character);
    SetCharacterValues();
    base.LeftClick(evt);
  }

  public override void MouseOver(UIMouseEvent evt) {
    base.MouseOver(evt);
    _char.SetAnimated(true);
  }

  public override void MouseOut(UIMouseEvent evt) {
    base.MouseOut(evt);
    _char.SetAnimated(false);
  }

  /// <summary>
  /// Assign fields from the current active <see cref="AnimCharacter"/> to this UI element's fields.
  /// </summary>
  private void GetRealValues() {
    _realCharacter = Characters.ActiveCharacter;
    _realSkinVariant = Player.skinVariant;
    _realHair = Player.hair;
    _realStyle.AssignFromPlayer(Player);
  }

  /// <summary>
  /// Set the player's appearance to match the active <see cref="AnimCharacter"/>.
  /// </summary>
  private void SetRealValues() {
    Characters.SetCharacter(_realCharacter);
    Player.skinVariant = _realSkinVariant;
    Player.hair = _realHair;
    _realStyle.AssignToPlayer(Player);
  }

  /// <summary>
  /// Set the player's appearance to match this button's <see cref="AnimCharacter"/>.
  /// </summary>
  private void SetCharacterValues() {
    AnimCharacter? character = Character;
    Characters.SetCharacter(character);

    AnimCharacterStyle style = character?.Style ?? DefaultStyle;
    style.AssignToPlayer(Player);
    if (character is null) {
      return;
    }

    Player.hairColor = style.HairColor;
    Player.skinColor = style.SkinColor;
    Player.eyeColor = style.EyeColor;
    Player.shirtColor = style.ShirtColor;
    Player.underShirtColor = style.UnderShirtColor;
    Player.pantsColor = style.PantsColor;
    Player.shoeColor = style.ShoeColor;
  }
}

using AnimLib.Skins;

namespace AnimLib.UI.Elements;

public sealed class UISkinSlotButton(Player player) : UIAnimPlayerButton(player) {
  /// <summary>
  /// The <see cref="AnimCharacter"/> which this button will display
  /// the <see cref="Slot"/>'s current <see cref="Skin"/>.
  /// </summary>
  public AnimCharacter? Character { get; private set; }

  /// <summary> The slot on <see cref="Character"/> which this button will open a menu for. </summary>
  public SkinSlot? Slot { get; private set; }

  /// <summary> This value is always <see langword="false"/>. </summary>
  protected override bool IsSelected => false;

  /// <summary> Used as a guard clause to ensure the state of the button is valid for interactions. </summary>
  [MemberNotNullWhen(true, nameof(Character), nameof(Slot))]
  public bool IsSet => Character is not null && Slot is not null;

  public void Set(AnimCharacter character, SkinSlot slot) {
    Character = character;
    Slot = slot;
  }

  public void Clear() {
    Character = null;
    Slot = null;
  }

  protected override void DrawContents(SpriteBatch spriteBatch) {
    if (!ReferenceEquals(Characters.ActiveCharacter, Character)) {
      return;
    }

    if (IsSet && Character.Skins.GetSkin(Slot).Icon?.Value is { } icon) {
      GetIconRects(icon, out Rectangle source, out Rectangle dest);
      spriteBatch.Draw(icon, dest, source, Color.White);
    }
  }
}

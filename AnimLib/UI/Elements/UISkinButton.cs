using AnimLib.Skins;
using Terraria.UI;

namespace AnimLib.UI.Elements;

public sealed class UISkinButton(Player player) : UIAnimPlayerButton(player) {
  /// <summary> The <see cref="AnimCharacter"/> which this button may assign <see cref="Skin"/> to. </summary>
  public AnimCharacter? Character { get; private set; }

  /// <summary> The slot on <see cref="Character"/> which this button may assign <see cref="Skin"/> to. </summary>
  public SkinSlot? Slot { get; private set; }

  /// <summary> Skin which this Button would assign. </summary>
  public Skin? Skin { get; private set; }

  /// <summary> Indicates whether <see cref="Skin"/> is currently equipped to the player. </summary>
  protected override bool IsSelected => IsSet && ReferenceEquals(Character.Skins.GetSkin(Slot), Skin);

  /// <summary> Used as a guard clause to ensure the state of the button is valid for interactions. </summary>
  [MemberNotNullWhen(true, nameof(Character), nameof(Slot), nameof(Skin))]
  public bool IsSet => Character is not null && Slot is not null && Skin is not null;

  public void Set(AnimCharacter character, SkinSlot slot, Skin skin) {
    Character = character;
    Slot = slot;
    Skin = skin;
  }

  public void Clear() {
    Character = null;
    Slot = null;
    Skin = null;
  }

  protected override void DrawContents(SpriteBatch spriteBatch) {
    if (!ReferenceEquals(Characters.ActiveCharacter, Character)) {
      return;
    }

    // Draw slot icon
    if (IsSet && Skin.Icon?.Value is { } icon) {
      GetIconRects(icon, out Rectangle source, out Rectangle dest);
      spriteBatch.Draw(icon, dest, source, Color.White);
    }
  }

  public override void LeftClick(UIMouseEvent evt) {
    if (IsSet) {
      Character.Skins.SetSkin(Slot, Skin);
    }

    base.LeftClick(evt);
  }
}

using System.Linq;

namespace AnimLib.Skins;

public sealed class EquippedSkinSlot(SkinSlot slot) {
  public Skin Skin { get; private set; } = slot.DefaultSkin;

  public readonly SkinSlot Slot = slot;

  // add event OnSkinChanged
  public event Action<Skin>? OnSkinChanged;

  public void SetSkin(Skin skin) {
    ArgumentNullException.ThrowIfNull(skin);

    if (!skin.ValidSlots.Contains(Slot)) {
      throw new ArgumentException($"Skin \"{skin.DisplayName}\" cannot be equipped to slot \"{Slot.DisplayName}\"", nameof(skin));
    }

    Skin = skin;
    OnSkinChanged?.Invoke(skin);
  }

  public void SetSkin<T>() where T : Skin => SetSkin(ModContent.GetInstance<T>());
}

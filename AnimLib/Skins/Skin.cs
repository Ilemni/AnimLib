using System.Linq;
using AnimLib.Animations;
using JetBrains.Annotations;
using Terraria.Localization;

namespace AnimLib.Skins;

/// <summary>
/// Represents a skin item which a player can select to display on their character.
/// <para/> Note that when inheriting from this class,
/// you should use the generic version, <see cref="Skin{T}"/>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class Skin : ModType, ILocalizedModType, IIndexed, IComparable<Skin> {
  public virtual LocalizedText? DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);
  public virtual LocalizedText? Description => this.GetLocalization(nameof(Description));

  public ushort Index { get; internal set; }

  /// <summary>
  /// Sorting order for this skin when displayed in the UI.
  /// This is sorted against other skins in the same slot.
  /// </summary>
  public virtual int SortOrder => 0;

  /// <summary>
  /// The valid slots which may equip this skin.
  /// <br/> Common case is only one slot, but more than one slot may be used separately.
  /// </summary>
  [field: AllowNull, MaybeNull]
  public IReadOnlyList<SkinSlot> ValidSlots => field ??= GetValidSlots().ToArray();

  /// <summary>
  /// The file name of this type's sprite sheet file (*.ase/*.aseprite) in the mod loader's file space.
  /// </summary>
  public virtual string SpriteSheetPath => (GetType().Namespace + "/" + Name).Replace('.', '/');

  public Asset<AnimSpriteSheet> SpriteSheetAsset { get; private set; } = null!; // SetupContent

  public AnimSpriteSheet SpriteSheet {
    get {
      if (!SpriteSheetAsset.IsLoaded) {
        SpriteSheetAsset.Wait();
      }

      return SpriteSheetAsset.Value;
    }
  }

  /// <summary>
  /// The file name of this type's icon image file, in the mod loader's file space.
  /// </summary>
  public virtual string IconPath => SpriteSheetPath + "_Icon";

  /// <summary>
  /// UI Icon to represent this skin item.
  /// </summary>
  public Asset<Texture2D>? Icon { get; private set; }

  /// <summary>
  /// Enumerable that represents the valid slots which may equip this skin.
  /// To indicate a slot, use <see cref="ModContent.GetInstance{T}"/>
  /// </summary>
  /// <returns></returns>
  protected abstract IEnumerable<SkinSlot> GetValidSlots();

  protected internal abstract AnimCharacter TemplateCharacter { get; }

  public virtual string LocalizationCategory => "Skins";

  protected sealed override void Register() {
    ModTypeLookup<Skin>.Register(this);
    SkinLoader.RegisterSkin(this);
  }

  public sealed override void SetupContent() {
    SpriteSheetAsset = ModContent.Request<AnimSpriteSheet>(SpriteSheetPath);

    if (ModContent.RequestIfExists<Texture2D>(IconPath, out var icon)) {
      Icon = icon;
    }

    // Trigger localization
    _ = DisplayName;
    _ = Description;

    SetStaticDefaults();
  }

  public int CompareTo(Skin? other) {
    return other is null ? 1 : SortOrder.CompareTo(other.SortOrder);
  }
}

/// <summary>
/// Represents a skin item which a player can select to display on their character.
/// </summary>
/// <typeparam name="T">
/// Type of <see cref="AnimCharacter"/> this slot is associated with.
/// Used to override <see cref="TemplateCharacter"/>.
/// </typeparam>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class Skin<T> : Skin where T : AnimCharacter {
  [field: AllowNull, MaybeNull]
  protected internal override T TemplateCharacter => field ??= ModContent.GetInstance<T>();
}

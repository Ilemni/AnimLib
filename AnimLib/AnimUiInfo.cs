using AnimLib.Menus;
using AnimLib.Skins;
using Terraria.GameContent.UI.States;

namespace AnimLib;

public sealed class AnimUiInfo {
  /// <summary>
  /// Whether the character is currently being drawn
  /// as part of a <see cref="Terraria.GameContent.UI.Elements.UICharacter"/>.
  /// <para/> Used for custom drawing behaviour, such as in the character creation screen.
  /// </summary>
  public bool IsDrawingInUI { get; internal set; }

  /// <summary>
  /// When <see cref="IsDrawingInUI"/> is <see langword="true"/>,
  /// this is whether the character is currently animated in the UI.
  /// </summary>
  public bool Animated { get; internal set; }

  /// <summary>
  /// When <see cref="IsDrawingInUI"/> is <see langword="true"/>,
  /// this is the current counter for the character's animation in the UI.
  /// </summary>
  public int AnimationCounter { get; internal set; } = -1;

  /// <summary>
  /// When <see cref="IsDrawingInUI"/> is <see langword="true"/>,
  /// this is the index of the category for character color picker
  /// Used to determine which animation to play in the character UI.
  /// <para/> This value is based on the <see cref="UICharacterCreation.CategoryId"/> value.
  /// <li>0 = CharInfo</li>
  /// <li>1 = Clothing</li>
  /// <li>2 = HairStyle</li>
  /// <li>3 = HairColor</li>
  /// <li>4 = Eye</li>
  /// <li>5 = Skin</li>
  /// <li>6 = Shirt</li>
  /// <li>7 = Undershirt</li>
  /// <li>8 = Pants</li>
  /// <li>9 = Shoes</li>
  /// </summary>
  public int CategoryIndex { get; internal set; } = -1;

  /// Used to detect category changes
  internal int CategoryIndexLastFrame { get; set; } = -1;

  /// <summary>
  /// When <see cref="IsDrawingInUI"/> is <see langword="true"/>,
  /// this is the index of the category that was previously selected, before <see cref="CategoryIndex"/>.
  /// Used to determine which animation to play in the character UI.
  /// <para/> This value is based on the <see cref="UICharacterCreation.CategoryId"/> value.
  /// </summary>
  public int LastCategoryIndex { get; internal set; } = -1;

  public int CategoryCounterStart { get; internal set; }

  /// <summary>
  /// This value is not <see langword="null"/> if the UI is in a menu for selecting a <see cref="Skin"/>.
  /// such as <see cref="SkinSlotSelectMenu"/>.
  /// </summary>
  public SkinSlot? CurrentSlot { get; internal set; }

  public int SlotCounter { get; internal set; }

  /// <summary>
  /// When <see cref="IsDrawingInUI"/> is <see langword="true"/>,
  /// this is the current counter for the character's animation in the UI,
  /// since the last category change.
  /// </summary>
  public int CategoryAnimationCounter => AnimationCounter - CategoryCounterStart;

  internal StoredInfo Store() => new(this);

  internal readonly struct StoredInfo(AnimUiInfo info) : IDisposable {
    private readonly bool _isDrawingInUI = info.IsDrawingInUI;
    private readonly bool _animated = info.Animated;
    private readonly int _animationCounter = info.AnimationCounter;
    private readonly int _categoryIndex = info.CategoryIndex;
    private readonly int _lastCategoryIndex = info.LastCategoryIndex;
    private readonly int _categoryCounterStart = info.CategoryCounterStart;

    public void Restore() {
      info.IsDrawingInUI = _isDrawingInUI;
      info.Animated = _animated;
      info.AnimationCounter = _animationCounter;
      info.CategoryIndex = _categoryIndex;
      info.LastCategoryIndex = _lastCategoryIndex;
      info.CategoryCounterStart = _categoryCounterStart;
    }

    public void Dispose() => Restore();
  }
}

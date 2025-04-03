using System.Linq;
using System.Reflection;
using AnimLib.States;
using AnimLib.UI.Elements;
using AnimLib.Utilities;
using JetBrains.Annotations;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

// ReSharper disable PossibleLossOfFraction - UI rounding

namespace AnimLib.Systems;

using ColoredButtonTextures = (Asset<Texture2D>? texture, Asset<Texture2D>? middleTexture);
using MouseEvents = ReadOnlySpan<UIElement.MouseEvent>;

/// <summary>
/// This ModSystem modifies the vanilla <see cref="UICharacterCreation"/> instance
/// to include AnimLib characters and related functionality.
/// <para/> This is the main system for modifying the character creation menu.
/// </summary>
[UsedImplicitly]
public sealed class UICharacterCreationAnimCharacterSystem : ModSystem {
  public UICharacterCreationAnimCharacterSystem() {
    UnselectAllCategories = Method("UnselectAllCategories").CreateDelegate<Action<UICharacterCreation>>();
    UpdateColorPickers = Method("UpdateColorPickers").CreateDelegate<Action<UICharacterCreation>>();
    // Value is cast from int to private enum type
    SetSelectedPicker = ClassHacking.CreateSetterWithCast<UICharacterCreation, int>("_selectedPicker");

    GetMiddleContainer = GetField<UIElement>("_middleContainer");
    GetClothesStyleContainer = GetField<UIElement>("_clothStylesContainer");
    GetHairStylesContainer = GetField<UIElement>("_hairstylesContainer");
    GetColorPickers = GetField<UIColoredImageButton[]>("_colorPickers");
    GetClothingStylesCategoryButton = GetField<UIColoredImageButton>("_clothingStylesCategoryButton");
    GetHairStylesCategoryButton = GetField<UIColoredImageButton>("_hairStylesCategoryButton");
    GetCharInfoCategoryButton = GetField<UIColoredImageButton>("_charInfoCategoryButton");

    GetButtonMiddleTexture = GetButtonField<Asset<Texture2D>>("_middleTexture");
    GetButtonTexture = GetButtonField<Asset<Texture2D>>("_texture");
    return;

    static Func<UICharacterCreation, TOut> GetField<TOut>(string name) =>
      ClassHacking.CreateGetter<UICharacterCreation, TOut>(name);

    static Func<UIColoredImageButton, TOut> GetButtonField<TOut>(string name) =>
      ClassHacking.CreateGetter<UIColoredImageButton, TOut>(name);

    static MethodInfo Method(string name) =>
      typeof(UICharacterCreation).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!;
  }

  // ReSharper disable InconsistentNaming - Actions/Funcs
  private readonly Action<UICharacterCreation> UnselectAllCategories;
  private readonly Action<UICharacterCreation> UpdateColorPickers;

  private readonly Action<UICharacterCreation, int> SetSelectedPicker;

  private readonly Func<UICharacterCreation, UIElement> GetMiddleContainer;
  private readonly Func<UICharacterCreation, UIElement> GetClothesStyleContainer;
  private readonly Func<UICharacterCreation, UIElement> GetHairStylesContainer;
  private readonly Func<UICharacterCreation, UIColoredImageButton[]> GetColorPickers;
  private readonly Func<UICharacterCreation, UIColoredImageButton> GetClothingStylesCategoryButton;
  private readonly Func<UICharacterCreation, UIColoredImageButton> GetHairStylesCategoryButton;
  private readonly Func<UICharacterCreation, UIColoredImageButton> GetCharInfoCategoryButton;

  private readonly Func<UIColoredImageButton, Asset<Texture2D>> GetButtonTexture;
  private readonly Func<UIColoredImageButton, Asset<Texture2D>> GetButtonMiddleTexture;
  // ReSharper restore InconsistentNaming

  private enum CategoryId {
    CharInfo,
    Clothing,
    HairStyle,
    HairColor,
    Eye,
    Skin,
    Shirt,
    Undershirt,
    Pants,
    Shoes
  }

  // Values are expected to be not null when these methods are run,
  // but will be null when no UICharacterCreation instance is active
  private UICharacterCreation _self = null!;
  private Player _player = null!;
  private AnimCharacterCollection _characters = null!;
  private UIElement _characterSelectContainer = null!;
  private UIElement _categoryContainer = null!;
  private UIElement _vanillaHairStylesListElement = null!;

  private ColoredButtonTextures? _vanillaCharInfo;
  private ColoredButtonTextures? _vanillaClothing;
  private ColoredButtonTextures? _vanillaHairStyleIcon;
  private ColoredButtonTextures? _vanillaHairColor;
  private ColoredButtonTextures? _vanillaSkin;
  private ColoredButtonTextures? _vanillaEye;
  private ColoredButtonTextures? _vanillaShirt;
  private ColoredButtonTextures? _vanillaUndershirt;
  private ColoredButtonTextures? _vanillaPants;
  private ColoredButtonTextures? _vanillaShoes;

  /// <summary> Categories ordered by how we want them displayed in the menu. </summary>
  private readonly List<UIColoredImageButton> _orderedCategories = [];

  public override void PostSetupContent() {
    // Skip doing any UI changes if there are no AnimLib characters.
    if (StateLoader.SelectableCharacters.Count == 0) {
      return;
    }

    Log.Debug("Adding hooks to UICharacterCreation, to modify menu to display AnimLib Characters.");

    // Set and unset static reference to UICharacterCreation
    On_UICharacterCreation.ctor += (orig, self, player) => {
      SetFields(self, player);
      orig.Invoke(self, player);
    };
    On_UICharacterCreation.FinishCreatingCharacter += (orig, self) => {
      Unset();
      orig.Invoke(self);
    };
    On_UICharacterCreation.Click_GoBack += (orig, self, evt, listeningElement) => {
      Unset();
      orig.Invoke(self, evt, listeningElement);
    };

    // Add our custom category button, and some logic to handle categories
    On_UICharacterCreation.MakeCategoriesBar += (orig, self, categoryContainer) => {
      orig.Invoke(self, categoryContainer);
      PostMakeCategoriesBar(categoryContainer);
    };

    // Create the menu which will contain the character select elements
    On_UICharacterCreation.MakeClothStylesMenu += (orig, self, middleInnerPanel) => {
      orig.Invoke(self, middleInnerPanel);
      MakeAnimCharacterSelectMenu(middleInnerPanel);
    };

    On_UICharacterCreation.CreateColorPicker += (orig, self, id, texturePath, xPositionStart, xPositionPerId) => {
      UIColoredImageButton result = orig.Invoke(self, id, texturePath, xPositionStart, xPositionPerId);
      result.OnLeftClick += (_, _) => { _characters.UiInfo.CategoryIndex = id; };

      return result;
    };

    On_UICharacterCreation.CreatePickerWithoutClick +=
      (orig, self, id, texturePath, xPositionStart, xPositionPerId) => {
        UIColoredImageButton result = orig.Invoke(self, id, texturePath, xPositionStart, xPositionPerId);
        result.OnLeftClick += (_, _) => { _characters.UiInfo.CategoryIndex = id; };
        return result;
      };

    // Include our categories in "unselect *all* categories"
    On_UICharacterCreation.UnselectAllCategories += (orig, self) => {
      orig.Invoke(self);
      UnselectAnimCharacterCategory();
    };

    On_UICharacterCreation.Draw += (orig, self, spriteBatch) => {
      AnimUiInfo uiInfo = _characters.UiInfo;
      using AnimUiInfo.StoredInfo _ = uiInfo.Store();
      uiInfo.IsDrawingInUI = true;
      uiInfo.CurrentSlot = _skinSlotSelectMenu.CurrentSlot;
      uiInfo.SlotCounter = _skinSlotSelectMenu.SlotCounter;
      orig.Invoke(self, spriteBatch);
    };
  }

  /// On class construction, set static references in this class to the UICharacterCreation instance and its player
  private void SetFields(UICharacterCreation self, Player player) {
    _self = self;
    _player = player;
    _characters = player.GetState<AnimCharacterCollection>();
  }

  public override void Unload() {
    Unset();
  }

  /// Remove any references to the UICharacterCreation instance and its member references
  private void Unset() {
    _self = null!;
    _player = null!;
    _characterSelectContainer = null!;
    _categoryContainer = null!;
    _orderedCategories.Clear();

    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    if (_characters is not null) {
      _characters.UiInfo.CategoryIndex = -1;
      _characters = null!;
    }
  }

  /// After the categories bar is created, modify it to include our custom categories
  private void PostMakeCategoriesBar(UIElement categoryContainer) {
    _categoryContainer = categoryContainer;
    _vanillaHairStylesListElement = GetHairStylesContainer(_self)
      .Children.ElementAt(0) // UIList
      .Children.ElementAt(0) // UIList.InnerList
      .Children.ElementAt(0); // Desired item
    AddVanillaToOrderedCategories();
    StoreVanillaTextures();
    AddCustomCategories();
    RecalculateCategoryPositions();
    TweakClothesStyleMenuChildrenPosition();
    TweakHairStyleMenuChildrenPosition();
  }

  private void AddVanillaToOrderedCategories() {
    var pickers = GetColorPickers(_self);

    // In vanilla, these elements are null.
    // Adding these shouldn't change any vanilla behavior,
    // but allows us to index pickers by these CategoryIds.
    pickers[(int)CategoryId.CharInfo] = GetCharInfoCategoryButton(_self);
    pickers[(int)CategoryId.Clothing] = GetClothingStylesCategoryButton(_self);
    pickers[(int)CategoryId.HairStyle] = GetHairStylesCategoryButton(_self);

    _orderedCategories.Clear();
    _orderedCategories.AddRange(pickers);
  }

  private void AddCustomCategories() {
    AddCategoryButton(2, 10, "AnimLib/AnimLib/UI/CategorySelect", _characterSelectContainer);
  }

  private void AddCategoryButton(int pos, int id, string texturePath, UIElement categoryPanel) {
    UIColoredImageButton button = new(ModContent.Request<Texture2D>(texturePath));
    button.OnLeftClick += (_, _) => {
      SoundEngine.PlaySound(in SoundID.MenuTick);
      UnselectAllCategories(_self);
      SetSelectedPicker(_self, id);
      GetMiddleContainer(_self).Append(categoryPanel);
      _orderedCategories[pos].SetSelected(true);
    };
    button.OnLeftClick += (_, _) => { _characters.UiInfo.CategoryIndex = id; };

    _categoryContainer.Append(button);

    // Shift all categories to the right of added category
    _orderedCategories.Add(null!);
    for (int i = _orderedCategories.Count - 2; i >= pos; i--) {
      _orderedCategories[i + 1] = _orderedCategories[i];
    }

    _orderedCategories[pos] = button;
  }

  private void StoreVanillaTextures() {
    var pickers = GetColorPickers(_self);
    _vanillaCharInfo = GetTexture(pickers[(int)CategoryId.CharInfo]);
    _vanillaClothing = GetTexture(pickers[(int)CategoryId.Clothing]);
    _vanillaHairStyleIcon = GetTexture(pickers[(int)CategoryId.HairStyle]);
    _vanillaHairColor = GetTexture(pickers[(int)CategoryId.HairColor]);
    _vanillaSkin = GetTexture(pickers[(int)CategoryId.Skin]);
    _vanillaEye = GetTexture(pickers[(int)CategoryId.Eye]);
    _vanillaShirt = GetTexture(pickers[(int)CategoryId.Shirt]);
    _vanillaUndershirt = GetTexture(pickers[(int)CategoryId.Undershirt]);
    _vanillaPants = GetTexture(pickers[(int)CategoryId.Pants]);
    _vanillaShoes = GetTexture(pickers[(int)CategoryId.Shoes]);
    return;

    ColoredButtonTextures GetTexture(UIColoredImageButton button) {
      return (GetButtonTexture(button), GetButtonMiddleTexture(button));
    }
  }

  #region Character Select methods

  private void MakeAnimCharacterSelectMenu(UIElement middleInnerPanel) {
    UIElement characterSelectContainer = new() {
      Width = StyleDimension.FromPixelsAndPercent(-20f, 1f),
      Height = StyleDimension.Fill,
      HAlign = 0.5f,
      VAlign = 0.5f
    };
    _characterSelectContainer = characterSelectContainer;
    middleInnerPanel.Append(characterSelectContainer);
    characterSelectContainer.SetPadding(0);

    characterSelectContainer.Append(new UICharacterName(_player) {
      Width = StyleDimension.FromPercent(0.5f),
      HAlign = 0,
      Left = StyleDimension.FromPixels(0)
    });

    const float num = -4;
    const float percent = 0.4f;
    UIPanel characterListPanel = new() {
      Width = StyleDimension.FromPixelsAndPercent(-310, 1),
      Height = StyleDimension.Fill,
      HAlign = 0.5f,
      VAlign = 0.5f,
      Left = StyleDimension.FromPixels(num + 158),
      BackgroundColor = Color.Transparent,
      BorderColor = Color.Transparent
    };
    characterListPanel.SetPadding(0);
    characterSelectContainer.Append(characterListPanel);

    UIList characterList = new() {
      Width = StyleDimension.FromPixelsAndPercent(-18, 1),
      Height = StyleDimension.FromPixelsAndPercent(-6, 1)
    };
    characterListPanel.Append(characterList);

    UIScrollbar characterListScrollBar = new() {
      HAlign = 1,
      Height = StyleDimension.FromPixelsAndPercent(-30, 1),
      Top = StyleDimension.FromPixels(10)
    };
    characterListScrollBar.SetView(100, 1000);
    characterList.SetScrollbar(characterListScrollBar);
    characterListPanel.Append(characterListScrollBar);

    int count = StateLoader.SelectableCharacters.Count;
    UIElement characterListChild = new() {
      Width = StyleDimension.Fill,
      Height = StyleDimension.FromPixels(82 * (count / 5 + (count % 5 != 0 ? 1 : 0)) + 52)
    };
    characterListChild.SetPadding(0);
    characterList.Add(characterListChild);

    AddEmptyCharacterButton(_player, characterListChild, [Click_SelectAnimCharacter]);
    AddCharacterSelectItems(_player, characterListChild, [Click_SelectAnimCharacter]);

    UIPanel statInfoBackground = new() {
      HAlign = 0,
      VAlign = 1,
      Width = StyleDimension.FromPixelsAndPercent(num, percent),
      Height = StyleDimension.FromPixelsAndPercent(-50f, 1),
      BackgroundColor = Color.Green
    };
    statInfoBackground.SetPadding(0);
    characterSelectContainer.Append(statInfoBackground);
  }

  private void AddEmptyCharacterButton(Player player, UIElement list, MouseEvents onClicks) {
    int index = ((List<UIElement>)list.Children).Count;
    list.Append(AddCharacterButton(player, null, onClicks, index));
  }

  internal void AddCharacterSelectItems(Player player, UIElement list, MouseEvents onClicks) {
    int index = ((List<UIElement>)list.Children).Count;
    foreach (AnimCharacter character in StateLoader.SelectableCharacters) {
      list.Append(AddCharacterButton(player, character, onClicks, index++));
    }
  }

  private UIAnimCharacterButton AddCharacterButton(Player player, AnimCharacter? character,
    MouseEvents onClicks, int index) {
    UIAnimCharacterButton button = new(player, character) {
      Left = StyleDimension.FromPixels(index % 5 * 48),
      Top = StyleDimension.FromPixels(index / 5 * 84)
    };

    foreach (UIElement.MouseEvent onClick in onClicks) {
      button.OnLeftClick += onClick;
    }

    return button;
  }


  /// On click, select the character and update various UI elements to reflect the character's style
  internal void Click_SelectAnimCharacter(UIMouseEvent evt, UIElement listeningElement) {
    UpdateColorPickers(_self);
    UIAnimCharacterButton listeningButton = (UIAnimCharacterButton)listeningElement;
    AnimCharacter? character = listeningButton.Character;

    var categoryButtons = GetColorPickers(_self);
    _categoryContainer.RemoveAllChildren();
    foreach (UIColoredImageButton button in _orderedCategories) {
      _categoryContainer.Append(button);
    }

    UIList hairStyleListElement = (UIList)GetHairStylesContainer(_self).Children.ElementAt(0);
    if (character is null) {
      AddOrRemoveCategory(false, CategoryId.HairStyle, _vanillaHairStyleIcon);
      AddOrRemoveCategory(false, CategoryId.HairColor, _vanillaHairColor);
      AddOrRemoveCategory(false, CategoryId.Skin, _vanillaSkin);
      AddOrRemoveCategory(false, CategoryId.Eye, _vanillaEye);
      AddOrRemoveCategory(false, CategoryId.Shirt, _vanillaShirt);
      AddOrRemoveCategory(false, CategoryId.Undershirt, _vanillaUndershirt);
      AddOrRemoveCategory(false, CategoryId.Pants, _vanillaPants);
      AddOrRemoveCategory(false, CategoryId.Shoes, _vanillaShoes);
      RecalculateCategoryPositionsAfterHiding();
      Main.Hairstyles.UpdateUnlocks();
      hairStyleListElement.Clear();
      hairStyleListElement.Add(_vanillaHairStylesListElement);
      return;
    }


    if (character.Style.UiSettings is { } style) {
      AddOrRemoveCategory(style.HideHairStyleOption, CategoryId.HairStyle, style.HairStyleIcon);
      AddOrRemoveCategory(style.HideHairColorOption, CategoryId.HairColor, style.HairColorIcon);
      AddOrRemoveCategory(style.HideSkinColorOption, CategoryId.Skin, style.SkinColorIcon);
      AddOrRemoveCategory(style.HideEyeColorOption, CategoryId.Eye, style.EyeColorIcon);
      AddOrRemoveCategory(style.HideShirtColorOption, CategoryId.Shirt, style.ShirtColorIcon);
      AddOrRemoveCategory(style.HideUnderShirtColorOption, CategoryId.Undershirt, style.UnderShirtColorIcon);
      AddOrRemoveCategory(style.HidePantsColorOption, CategoryId.Pants, style.PantsColorIcon);
      AddOrRemoveCategory(style.HideShoeColorOption, CategoryId.Shoes, style.ShoeColorIcon);

      style.InvokeCategoriesBarChanged(categoryButtons, _categoryContainer);
    }

    _vanillaHairStylesListElement.Remove();

    int count = character.HairStyleCount;
    UIElement characterHairStylesListElement = new() {
      Width = StyleDimension.Fill,
      Height = StyleDimension.FromPixels(48 * (count / 10 + (count % 10 != 0 ? 1 : 0)))
    };

    for (int i = 0; i < count; i++) {
      UIHairStyleButton uIHairStyleButton = new(_player, Main.Hairstyles.AvailableHairstyles[i]) {
        Left = StyleDimension.FromPixels(i % 10 * 46f + 6f),
        Top = StyleDimension.FromPixels(i / 10 * 48f + 1f)
      };

      uIHairStyleButton.SetSnapPoint("Middle", i);
      uIHairStyleButton.SkipRenderingContent(i);
      characterHairStylesListElement.Append(uIHairStyleButton);
    }

    hairStyleListElement.Remove(_vanillaHairStylesListElement);
    hairStyleListElement.Add(characterHairStylesListElement);
    RecalculateCategoryPositionsAfterHiding();
    // _self.Ex_MakeHairStylesMenu();

    return;

    void AddOrRemoveCategory(bool hide, CategoryId id, ColoredButtonTextures? tex) {
      UIColoredImageButton button = categoryButtons[(int)id];
      _categoryContainer.AddOrRemoveChild(button, !hide);

      if (tex is { } textures) {
        if (textures.texture is not null) {
          button.SetImage(textures.texture);
        }

        button.SetMiddleTexture(textures.middleTexture);
      }

      button.Width.Pixels = 44;
      button.Height.Pixels = 44;
    }
  }

  #endregion

  /// Method appended to On_UICharacterCreation.UnselectAllCategories to also unselect the AnimCharacter category
  private void UnselectAnimCharacterCategory() {
    foreach (UIColoredImageButton button in _orderedCategories) {
      button.SetSelected(false);
    }

    _characterSelectContainer.Remove();
  }

  /// Repositions the category buttons after initial modifications by this system
  private void RecalculateCategoryPositions() {
    const int xPositionPerId = 48;
    int categoryCount = _orderedCategories.Count;

    for (int i = 0; i < categoryCount; i++) {
      _orderedCategories[i].SetSnapPoint("Top", i);
    }

    int offset = 0;
    int xPositionStart = categoryCount * -xPositionPerId / 2;
    foreach (UIColoredImageButton button in _orderedCategories.Where(b => b.Parent is not null)) {
      button.SetSnapPoint("Top", offset);
      int pos = xPositionStart + xPositionPerId * offset++;
      button.Left = StyleDimension.FromPixelsAndPercent(pos, 0.5f);
    }

    UIElement parent = GetMiddleContainer(_self).Parent.Parent;
    parent.Width.Pixels = categoryCount * xPositionPerId + 20;
    parent.Recalculate();
  }

  /// Repositions the category buttons after hiding an arbitrary number of them
  private void RecalculateCategoryPositionsAfterHiding() {
    const int xPositionPerId = 48;

    for (int i = 0; i < _categoryContainer.Children.Count(); i++) {
      _categoryContainer.Children.ElementAt(i).SetSnapPoint("Top", i);
    }

    int offset = 0;
    int xPositionStart = _orderedCategories.Count * -xPositionPerId / 2;
    foreach (UIElement button in _categoryContainer.Children
               .Where(b => b.Parent is not null && b is UIColoredImageButton)) {
      int pos = xPositionStart + xPositionPerId * offset;
      offset++;
      button.Left = StyleDimension.FromPixelsAndPercent(pos, 0.5f);
    }

    UIElement parent = GetMiddleContainer(_self).Parent.Parent;
    parent.Width.Pixels = _orderedCategories.Count * xPositionPerId + 20;
    parent.Recalculate();
  }

  /// After widening menu, this will shift the children of the clothes style menu
  private void TweakClothesStyleMenuChildrenPosition() {
    UIElement container = GetClothesStyleContainer(_self);
    int newCategoryCount = _orderedCategories.Count - 10;
    int offset = newCategoryCount * 24;

    foreach (UIElement child in container.Children) {
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (child.HAlign != 0.5f) {
        child.Left.Pixels += offset;
      }
    }
  }

  /// After widening menu, we need to shift the children of the hairstyle menu
  private void TweakHairStyleMenuChildrenPosition() {
    UIElement container = GetClothesStyleContainer(_self);
    UIElement listElement = container.Children.ElementAt(0);
    UIElement scrollBarElement = container.Children.ElementAt(1);
    int newCategoryCount = _orderedCategories.Count - 10;
    int offset = newCategoryCount * 24;

    listElement.Left.Pixels += offset;
    scrollBarElement.Left.Pixels -= offset;
  }
}

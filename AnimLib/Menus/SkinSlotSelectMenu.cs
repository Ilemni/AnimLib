using System.Linq;
using AnimLib.Skins;
using AnimLib.UI.Elements;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

// ReSharper disable PossibleLossOfFraction - UI positioning

namespace AnimLib.Menus;

/// <summary>
/// Menu for selecting the <b>slot</b> of a skin.
/// This menu switches between showing slots, and showing available skins for a selected slot.
/// </summary>
public sealed class SkinSlotSelectMenu : UIElement {
  private const int SlotsPerRow = 3;
  private const int SkinsPerRow = 5;
  private const float SkinButtonRatio = 1.2f;

  private readonly AnimCharacterCollection _characters;
  private AnimCharacter? _character;

  public SkinSlot? CurrentSlot { get; private set; }
  public int SlotCounter { get; private set; }

  private readonly UIElement _listChild;
  private readonly UIScrollbar _listScrollBar;
  private readonly UISkinSlotButton[] _slotButtons;
  private readonly UISkinButton[] _skinButtons;
  private readonly UIImageButton _backToSlotsButton;

  private readonly UIText _slotLabel;
  private readonly UIText _nameLabel;
  private readonly UIText _descLabel;

  private float _slotScrollViewPosition;

  public SkinSlotSelectMenu(Player player) {
    _characters = player.GetState<AnimCharacterCollection>();

    Width = StyleDimension.Fill;
    Height = StyleDimension.Fill;

    Append(new UICharacterName(player) {
      Width = StyleDimension.FromPercent(0.5f),
      HAlign = 0,
      Left = StyleDimension.Empty
    });

    // Infobox about the hovered slot, or skin item when in skin menu
    UIPanel leftPanel = new() {
      Width = StyleDimension.FromPixelsAndPercent(-20, 0.5f),
      Height = StyleDimension.FromPixelsAndPercent(-44, 1),
      VAlign = 1
    };
    Append(leftPanel);

    _slotLabel = new UIText("", 0.8f) {
      HAlign = 0,
      TextOriginX = 0
    };
    leftPanel.Append(_slotLabel);
    _nameLabel = new UIText("", 0.8f) {
      HAlign = 0,
      Top = StyleDimension.FromPixels(20),
      TextOriginX = 0
    };
    leftPanel.Append(_nameLabel);
    _descLabel = new UIText("", 0.8f) {
      HAlign = 0,
      Top = StyleDimension.FromPixels(40),
      TextOriginX = 0
    };
    leftPanel.Append(_descLabel);

    // Contains the slots
    UIList listElement = new() {
      Width = StyleDimension.FromPixelsAndPercent(-10, 0.5f),
      Height = StyleDimension.Fill,
      HAlign = 1,
      Left = StyleDimension.FromPixels(10),
    };
    Append(listElement);

    // Scrollbar for slot window
    _listScrollBar = new UIScrollbar {
      Height = StyleDimension.Fill,
      Width = StyleDimension.FromPixels(16),
      Top = StyleDimension.FromPixels(0),
      Left = StyleDimension.FromPixelsAndPercent(-16, 1)
    };
    listElement.SetScrollbar(_listScrollBar);
    Append(_listScrollBar);

    var characterSlots = SkinLoader.AllCharacterSkins;
    int maxSlots = characterSlots.Values.Max(x => x.Slots.Count);
    int maxSkins = characterSlots.Values.Max(x => x.SkinsBySlot.Max(y => y.Count));

    // List element which will hold the slot buttons.
    // Slot buttons are added/removed to this element just before drawing.
    _listChild = new UIElement {
      Width = StyleDimension.FromPixelsAndPercent(-40, 1),
      HAlign = 0,
      Height = StyleDimension.Fill // Modified in SetMenuToSlots/SetMenuToSkins
    };
    listElement.Add(_listChild);

    const float slotButtonWidth = 0.95f / SlotsPerRow;
    const float skinButtonWidth = 0.95f / SkinsPerRow;

    // Add buttons for each slot, up to max possible slots
    // These buttons are reused when character changes
    _slotButtons = new UISkinSlotButton[maxSlots];
    for (int i = 0; i < maxSlots; i++) {
      UISkinSlotButton button = new(player) {
        Width = StyleDimension.FromPercent(slotButtonWidth),
        Height = StyleDimension.FromPixels(1000),
        Left = StyleDimension.FromPercent((float)i % SlotsPerRow / SlotsPerRow),
        Top = StyleDimension.FromPixels(1000 * (i / SlotsPerRow))
      };
      button.SetPadding(8);
      button.OnLeftClick += OnSlotButtonClick;
      button.OnMouseOver += OnSlotButtonHover;
      _slotButtons[i] = button;
    }

    // Add buttons for each skin, up to max possible skins
    // These buttons are reused when character or slot changes
    _skinButtons = new UISkinButton[maxSkins];
    for (int i = 0; i < maxSkins; i++) {
      float pos = i + 1;
      UISkinButton button = new(player) {
        Width = StyleDimension.FromPercent(skinButtonWidth),
        Height = StyleDimension.FromPercent(SkinButtonRatio),
        Left = StyleDimension.FromPercent(pos % SkinsPerRow / SkinsPerRow),
        Top = StyleDimension.FromPercent(SkinButtonRatio * ((int)pos / SkinsPerRow))
      };
      button.SetPadding(6);
      button.OnMouseOver += OnSkinButtonHover;
      button.OnMouseOut += (_, _) => ExitHover();
      _skinButtons[i] = button;
    }

    var backTex = AnimLibMod.Instance.Assets.Request<Texture2D>("AnimLib/UI/Button_Back");

    _backToSlotsButton = new UIImageButton(backTex) {
      Width = StyleDimension.FromPercent(skinButtonWidth),
      Height = StyleDimension.FromPercent(SkinButtonRatio)
    };
    _backToSlotsButton.MinWidth = _backToSlotsButton.Width;
    _backToSlotsButton.MinHeight = _backToSlotsButton.Height;
    _backToSlotsButton.OnLeftClick += (_, _) => SetMenuToSlots();
  }

  public override void Draw(SpriteBatch spriteBatch) {
    UpdateCharacter();
    if (CurrentSlot is not null) {
      SlotCounter++;
    }
    else {
      SlotCounter = 0;
    }

    base.Draw(spriteBatch);
  }

  private void OnSlotButtonClick(UIMouseEvent evt, UIElement listeningElement) {
    if (listeningElement is UISkinSlotButton { Character: { } character, Slot: { } slot }) {
      SetMenuToSkins(character, slot);
    }
  }

  private void OnSlotButtonHover(UIMouseEvent evt, UIElement listeningElement) {
    if (listeningElement is UISkinSlotButton { Character: { } character, Slot: { } slot }) {
      SetText(slot, character.Skins.GetSkin(slot));
    }
  }

  private void OnSkinButtonHover(UIMouseEvent evt, UIElement listeningElement) {
    if (listeningElement is UISkinButton { Slot: { } slot, Skin: { } skin }) {
      SetText(slot, skin);
    }
  }

  private void ExitHover() {
    if (_listChild.Children.OfType<UISkinButton>().FirstOrDefault() is { Slot: { } slot }) {
      SetText(slot, _character!.Skins.GetSkin(slot));
    }
  }

  private void SetText(SkinSlot slot, Skin skin) {
    _slotLabel.SetText(slot.DisplayName);
    _nameLabel.SetText(skin.DisplayName);
    _descLabel.SetText(skin.Description);
  }

  private void UpdateCharacter() {
    AnimCharacter? character = _characters.ActiveCharacter;
    if (ReferenceEquals(_character, character)) {
      return;
    }

    _character = character;
    SetMenuToSlots();
  }

  public void SetMenuToSlots() {
    CurrentSlot = null;
    _listScrollBar.ViewPosition = _slotScrollViewPosition;
    _listChild.RemoveAllChildren();

    foreach (UISkinSlotButton button in _slotButtons) {
      button.Clear();
    }

    if (_character is null) {
      return;
    }

    var slots = SkinLoader.GetSortedSlots(_character);
    for (int i = 0; i < slots.Count; i++) {
      UISkinSlotButton button = _slotButtons[i];
      button.Set(_character, slots[i]);
      _listChild.Append(button);
    }

    SetListHeight(slots.Count, SlotsPerRow, 74);
  }

  private void SetMenuToSkins(AnimCharacter character, SkinSlot slot) {
    CurrentSlot = slot;
    if (_listChild.Children.FirstOrDefault() is UISkinSlotButton) {
      // Store position from slot menu
      _slotScrollViewPosition = _listScrollBar.ViewPosition;
    }

    _listChild.RemoveAllChildren();
    foreach (UISkinButton button in _skinButtons) {
      button.Clear();
    }

    _listChild.Append(_backToSlotsButton);

    var skins = SkinLoader.GetSkins(slot);
    for (int i = 0; i < skins.Count; i++) {
      UISkinButton button = _skinButtons[i];
      button.Set(character, slot, skins[i]);
      _listChild.Append(button);
    }

    SetListHeight(skins.Count, SkinsPerRow, 44);
  }

  private void SetListHeight(int items, int itemsPerRow, int rowHeight) {
    _listChild.Height = StyleDimension.FromPixels(rowHeight * ((items + itemsPerRow - 1) / itemsPerRow));
  }
}

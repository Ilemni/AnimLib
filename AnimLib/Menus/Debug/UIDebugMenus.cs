using AnimLib.Animations;
using AnimLib.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AnimLib.Menus.Debug;

/// <summary>
/// Debug UI state that displays info about a player's <see cref="AnimCharacterCollection"/>.
/// <para/> This menu does not draw anything, but contains all the UI panels for the character collection.
/// </summary>
public sealed class UIDebugMenus : UIState {
  public AnimCharacterCollection? Characters { get; private set; }

  private UIStateList _stateList = null!;
  private UIAbilityList _abilityList = null!;
  private UIInterruptList _interruptList = null!;
  private UIAnimatedStatesList _animatedStatesList = null!;
  private UICharacterList _characterList = null!;

  private UIImageButton _stateListButton = null!;
  private UIImageButton _abilityListButton = null!;
  private UIImageButton _interruptListButton = null!;
  private UIImageButton _animatedStatesListButton = null!;
  private UIImageButton _characterListButton = null!;

  private UIElement _container = null!;
  private UIElement _categoryListContainer = null!;

  private readonly (int x, int y, int w, int h)[] _categoryRects = [
    (0, 10, 40, 32),
    (0, 50, 40, 32),
    (0, 90, 40, 32),
    (0, 130, 40, 32),
    (0, 170, 40, 32)
  ];

  private readonly (int x, int y, int w, int h)[] _menuRects = [
    (100, 100, 200, 300),
    (100 + 200 + 10, 100, 480, 300),
    (100 + 200 + 10 + 480 + 10, 100, 200, 300),
    (100, 100 + 300 + 10, 360, 500),
    (460 + 10, 100 + 300 + 10, 240, 180)
  ];

  public void SetCharacters(AnimCharacterCollection? collection) {
    Characters = collection;
    _characterList.SetState(collection);
    SetCharacter(collection?.ActiveCharacter);
  }

  public void SetCharacter(AnimCharacter? character) {
    _animatedStatesList.SetState(character);
    _stateList.SetState(character);
    _abilityList.SetState(character);
    _interruptList.SetState(character);
  }

  public override void OnInitialize() {
    _container = new UIElement {
      Width = StyleDimension.Fill,
      Height = StyleDimension.Fill
    };
    Append(_container);

    _categoryListContainer = new DraggablePanel {
      Width = StyleDimension.FromPixels(56),
      Height = StyleDimension.FromPixels(20 + 8 + 40 * 5),
      Top = StyleDimension.FromPixels(270),
      Left = StyleDimension.FromPixels(20)
    };
    Append(_categoryListContainer);

    TextureDictionary tex = AnimLibMod.Instance.Assets.Request<TextureDictionary>(
      "AnimLib/UI/CategoryIcons", AssetRequestMode.ImmediateLoad).Value;

    _stateList = AddMenu<UIStateList>(0, tex["State List"], out _stateListButton);
    _abilityList = AddMenu<UIAbilityList>(1, tex["Ability List"], out _abilityListButton);
    _interruptList = AddMenu<UIInterruptList>(2, tex["Interrupt List"], out _interruptListButton);
    _animatedStatesList = AddMenu<UIAnimatedStatesList>(3, tex["Animated State List"], out _animatedStatesListButton);
    _characterList = AddMenu<UICharacterList>(4, tex["Character List"], out _characterListButton);

    return;

    T AddMenu<T>(int index, Asset<Texture2D> texture, out UIImageButton button) where T : UIElement, new() {
      (int x, int y, int w, int h) = _menuRects[index];
      T menu = new() {
        Width = StyleDimension.FromPixels(w),
        Height = StyleDimension.FromPixels(h),
        Left = StyleDimension.FromPixels(x),
        Top = StyleDimension.FromPixels(y)
      };
      menu.Activate();

      (x, y, w, h) = _categoryRects[index];
      button = new UIImageButton(texture) {
        Width = StyleDimension.FromPixels(w),
        Height = StyleDimension.FromPixels(h),
        Left = StyleDimension.FromPixels(x),
        Top = StyleDimension.FromPixels(y)
      };
      button.OnLeftClick += (_, _) => {
        if (menu.Parent is not null) {
          menu.Remove();
        }
        else {
          _container.Append(menu);
        }
      };

      (x, y, w, h) = _menuRects[index];
      button.OnLeftDoubleClick += (_, _) => {
        menu.Left.Pixels = x;
        menu.Top.Pixels = y;
        menu.Width.Pixels = w;
        menu.Height.Pixels = h;
      };
      _categoryListContainer.Append(button);

      return menu;
    }
  }

  protected override void DrawSelf(SpriteBatch spriteBatch) {
    base.DrawSelf(spriteBatch);
    if (!IsMouseHovering) {
      return;
    }

    if (_stateListButton.ContainsPoint(Main.MouseScreen)) {
      Main.instance.MouseText("State List");
    }
    else if (_abilityListButton.ContainsPoint(Main.MouseScreen)) {
      Main.instance.MouseText("Ability List");
    }
    else if (_interruptListButton.ContainsPoint(Main.MouseScreen)) {
      Main.instance.MouseText("Interrupt List");
    }
    else if (_animatedStatesListButton.ContainsPoint(Main.MouseScreen)) {
      Main.instance.MouseText("Animated States List");
    }
    else if (_characterListButton.ContainsPoint(Main.MouseScreen)) {
      Main.instance.MouseText("Character List");
    }
  }
}

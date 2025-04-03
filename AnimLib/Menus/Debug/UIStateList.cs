using System.Linq;
using AnimLib.States;
using AnimLib.UI.Debug;
using AnimLib.UI.Elements;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AnimLib.Menus.Debug;

/// <summary>
/// Debug UI that displays info about a specified <see cref="State"/>.
/// </summary>
public sealed class UIStateList : DebugUIElement<AnimCharacter> {
  protected override string HeaderHoverText => "Displays info about all active states on the selected character.";

  private readonly Dictionary<int, List<UIStateListItem>> _stateItems = [];

  private UIList _stateList = null!;

  public override void OnInitialize() {
    base.OnInitialize();
    SetHeaderText("State List");

    _stateList = new UIList {
      Width = StyleDimension.FromPixelsAndPercent(-24, 1),
      Height = StyleDimension.FromPixelsAndPercent(-6, 1),
      HAlign = 1f,
      VAlign = 1f,
      ListPadding = 4
    };
    _stateList.SetPadding(2);
    _stateList.PaddingRight = 4;
    _stateList.ManualSortMethod = list => list.Sort((a, b) => {
      IStateUIElement itemA = (IStateUIElement)a;
      IStateUIElement itemB = (IStateUIElement)b;

      return itemA.CompareTo(itemB);
    });
    BodyContainer.Append(_stateList);

    UIScrollbar scrollbar = new() {
      Width = StyleDimension.FromPixelsAndPercent(-18, 1),
      Height = StyleDimension.FromPixelsAndPercent(-10, 1),
      Top = StyleDimension.FromPixels(4)
    };
    scrollbar.SetView(100f, 1000f);
    _stateList.SetScrollbar(scrollbar);
    BodyContainer.Append(scrollbar);

    foreach (AnimCharacter character in StateLoader.TemplateStates.OfType<AnimCharacter>()) {
      if (!character.Children.Any()) {
        continue;
      }

      var list = new List<UIStateListItem>();
      _stateItems[character.Index] = list;
      foreach (State state in StateLoader.TemplateStates.Where(s => s.Hierarchy.ParentIds.Contains(character.Index))) {
        UIStateListItem item = new(state, this);
        item.Activate();
        list.Add(item);
      }
    }
  }

  protected override void OnSetState(AnimCharacter? character) {
    _stateList.Clear();
    if (character is null) {
      return;
    }

    if (!_stateItems.TryGetValue(character.Index, out var newItems)) {
      return;
    }

    foreach (UIStateListItem item in newItems) {
      _stateList.Add(item);
    }
  }

  internal sealed class UIStateListItem : UIElement, IStateUIElement {
    private readonly int _stateIndex;
    private readonly UIStateList _parent;

    private UIText _name = null!;
    private UIImageButton? _button;

    private UIStateInfo _stateInfo = null!;

    public State State => _parent.State?.GetState(_stateIndex) ?? StateLoader.TemplateStates[_stateIndex];

    public UIStateListItem(State state, UIStateList uiStateList) {
      _stateIndex = state.Index;
      _parent = uiStateList;

      Width = StyleDimension.Fill;
      Height = StyleDimension.FromPixels(16);
      SetPadding(0);
    }

    public override void OnInitialize() {
      base.OnInitialize();
      State templateState = StateLoader.TemplateStates[_stateIndex];
      _name = new UIText(templateState.Name, 0.9f) {
        HAlign = 0,
        VAlign = 0.5f,
        Left = StyleDimension.FromPixels((templateState.Hierarchy.ParentIds.Length - 1) * 12),
        IgnoresMouseInteraction = true,
        DynamicallyScaleDownToWidth = true
      };
      Append(_name);

      if (StateLoader.Instance.DebugText.HasOverride(State)) {
        _button = new UIImageButton(
          Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay", AssetRequestMode.ImmediateLoad)) {
          HAlign = 1f,
          VAlign = 0.5f
        };
        _button.SetVisibility(1, 0.4f);
        _button.OnLeftClick += Button_OnClick;
        Append(_button);
      }

      _stateInfo = new UIStateInfo(this) {
        Width = StyleDimension.FromPixels(300),
        Height = StyleDimension.FromPixels(60)
      };
      _stateInfo.SetPadding(6);
      _stateInfo.Activate();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
      _name.TextColor = State.Active ? Color.Lime : Color.LightGray;
      base.DrawSelf(spriteBatch);

      if (_button?.IsMouseHovering ?? false) {
        Main.instance.MouseText(_stateInfo.Parent is null
          ? "View state data."
          : "Hide state data.");
      }
    }

    private void Button_OnClick(UIMouseEvent evt, UIElement listeningElement) {
      UIImageButton button = (UIImageButton)listeningElement;
      if (_stateInfo.Parent is null) {
        UIDebugMenus parent = GetParent<UIDebugMenus>();
        parent.Append(_stateInfo);
        _stateInfo.Left.Pixels = Main.MouseScreen.X + 10;
        _stateInfo.Top.Pixels = Main.MouseScreen.Y + 10;
        button.SetImage(AnimLibMod.Instance.Assets.Request<Texture2D>("AnimLib/UI/ButtonExit", AssetRequestMode.ImmediateLoad));
      }
      else {
        _stateInfo.Remove();
        button.SetImage(Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay"));
      }
    }

    private T GetParent<T>() where T : UIElement {
      UIElement? parent = Parent;
      while (parent is not T and not null) {
        parent = parent.Parent;
      }

      return parent as T ?? throw new InvalidOperationException($"Parent {typeof(T).Name} not found.");
    }
  }
}

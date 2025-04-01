using System.Linq;
using AnimLib.States;
using AnimLib.UI.Debug;
using AnimLib.UI.Elements;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AnimLib.Menus.Debug;

/// <summary>
/// Debug UI that displays a list of interruptible states on currently active states.
/// </summary>
public sealed class UIInterruptList : DebugUIElement<AnimCharacter> {
  protected override string HeaderHoverText =>
    "Displays a list of states which can interrupt the\ncurrent active states on the selected character.";

  private readonly Dictionary<int, List<UIInterruptListItem>> _interruptItems = [];

  private UIList _interruptList = null!;

  public override void OnInitialize() {
    base.OnInitialize();
    SetHeaderText("Interrupts");

    _interruptList = new UIList {
      Width = StyleDimension.FromPixelsAndPercent(-24, 1),
      Height = StyleDimension.FromPixelsAndPercent(-6, 1),
      HAlign = 1f,
      VAlign = 1f,
      ListPadding = 4,
      ManualSortMethod = list => list.Sort(SortState)
    };
    _interruptList.SetPadding(2);
    _interruptList.PaddingRight = 4;
    BodyContainer.Append(_interruptList);

    UIScrollbar scrollbar = new() {
      Width = StyleDimension.FromPixelsAndPercent(-18, 1),
      Height = StyleDimension.FromPixelsAndPercent(-10, 1),
      Top = StyleDimension.FromPixels(4)
    };
    scrollbar.SetView(100f, 1000f);
    _interruptList.SetScrollbar(scrollbar);
    BodyContainer.Append(scrollbar);

    foreach (AnimCharacter character in StateLoader.TemplateStates.OfType<AnimCharacter>()) {
      if (!character.Children.Any()) {
        continue;
      }

      var list = new List<UIInterruptListItem>();
      _interruptItems[character.Index] = list;
      foreach (State state in StateLoader.TemplateStates
                 .Where(s => s.Hierarchy.ChildrenInterruptibleIds.Count > 0)) {
        var ids = state.Hierarchy.ChildrenInterruptibleIds;
        foreach (var id in ids) {
          UIInterruptListItem item = new(id.Key, this);
          item.Activate();
          list.Add(item);
        }
      }
    }
  }

  private static int SortState(UIElement a, UIElement b) {
    IStateUIElement itemA = (IStateUIElement)a;
    IStateUIElement itemB = (IStateUIElement)b;

    return itemA.CompareTo(itemB);
  }

  protected override void DrawSelf(SpriteBatch spriteBatch) {
    base.DrawSelf(spriteBatch);

    _interruptList.Clear();
    if (State is null) {
      return;
    }

    if (!_interruptItems.TryGetValue(State.Index, out var items)) {
      return;
    }

    foreach (UIInterruptListItem item in items.Where(i => i.State.Active)) {
      _interruptList.Add(item);
    }
  }

  protected override void OnSetState(AnimCharacter? character) {
    _interruptList.Clear();
  }

  private sealed class UIInterruptListItem : UIPanel, IStateUIElement {
    private readonly int _stateIndex;
    private readonly UIInterruptList _parent;

    private readonly Dictionary<int, UIText> _interruptTexts = [];

    private UIText _name = null!;
    public State State => _parent.State?.GetState(_stateIndex) ?? StateLoader.TemplateStates[_stateIndex];

    public UIInterruptListItem(int stateIndex, UIInterruptList parent) {
      (_stateIndex, _parent) = (stateIndex, parent);

      StateHierarchy hierarchy = StateLoader.TemplateHierarchy[stateIndex];

      const int indentSize = 10;
      int indentCount = hierarchy.ParentIds.Length - 3;
      Width = StyleDimension.FromPixelsAndPercent(-indentSize * indentCount, 1);
      Left = StyleDimension.FromPixels(indentSize * indentCount);
    }

    public override void OnInitialize() {
      base.OnInitialize();

      _name = new UIText(State.Name, 0.9f) {
        Left = StyleDimension.FromPixels(5),
        Top = StyleDimension.FromPixels(0),
      };
      Append(_name);

      var states = StateLoader.TemplateHierarchy
        .Where(s => s.InterruptibleIds.Contains((ushort)_stateIndex))
        .Select(s => StateLoader.TemplateStates[s.Index]);

      int index = 0;
      foreach (State state in states) {
        UIText text = new(state.Name, 0.7f) {
          Left = StyleDimension.FromPixels(10),
          Top = StyleDimension.FromPixels(20 + 16 * index),
        };
        Append(text);
        _interruptTexts[state.Index] = text;
        index++;
      }

      Height = StyleDimension.FromPixels(40 + 16 * index);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
      base.DrawSelf(spriteBatch);
      AnimCharacter? parent = _parent.State;
      if (parent is null) {
        return;
      }

      foreach ((int id, UIText? text) in _interruptTexts) {
        text.TextColor = parent.AllStates[id].CanEnter() ? Green : Color.LightGray;
      }
    }
  }
}

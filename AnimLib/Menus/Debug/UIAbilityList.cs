using System.Linq;
using AnimLib.States;
using AnimLib.UI.Debug;
using AnimLib.UI.Elements;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AnimLib.Menus.Debug;

/// <summary>
/// Debug UI that displays info about all <see cref="AbilityState"/>s in an <see cref="AnimCharacter"/>.
/// Info includes ability name, level, max level, cooldown time, and whether the ability is on cooldown.
/// </summary>
public sealed class UIAbilityList : DebugUIElement<AnimCharacter> {
  protected override string HeaderHoverText => "Displays info about all abilities on the selected character.";

  private readonly Dictionary<int, List<UIAbilityListItem>> _abilityItems = new();

  private UIElement _tableHeaderContainer = null!;
  private UIList _abilityList = null!;

  public override void OnInitialize() {
    base.OnInitialize();
    SetHeaderText("Abilities");

    _tableHeaderContainer = new UIElement {
      Width = StyleDimension.Fill,
      Height = StyleDimension.FromPixels(40f),
      IgnoresMouseInteraction = true
    };
    _tableHeaderContainer.SetPadding(0);

    AddHeaderText("Ability", 65);
    AddHeaderText("Level", 195);
    AddHeaderText("(Max)", 245);
    AddHeaderText("CD Time", 325);
    AddHeaderText("On CD", 405);
    BodyContainer.Append(_tableHeaderContainer);

    _abilityList = new UIList {
      Width = StyleDimension.FromPixelsAndPercent(-24, 1),
      Height = StyleDimension.FromPixelsAndPercent(-34, 1),
      Top = StyleDimension.FromPixels(34),
      HAlign = 1,
      ListPadding = -1
    };
    _abilityList.SetPadding(0);
    BodyContainer.Append(_abilityList);

    UIScrollbar scrollbar = new() {
      Width = StyleDimension.FromPixelsAndPercent(-18, 1),
      Height = StyleDimension.FromPixelsAndPercent(-46, 1),
      Top = StyleDimension.FromPixels(40)
    };
    scrollbar.SetView(100f, 1000f);
    _abilityList.SetScrollbar(scrollbar);
    BodyContainer.Append(scrollbar);

    foreach (AnimCharacter character in StateLoader.TemplateStates.OfType<AnimCharacter>()) {
      if (character.AbilityStates.Count == 0) {
        continue;
      }

      var list = new List<UIAbilityListItem>();
      _abilityItems[character.Index] = list;
      foreach (AbilityState ability in character.AbilityStates) {
        UIAbilityListItem item = new(ability, this);
        item.Activate();
        list.Add(item);
      }
    }

    return;

    void AddHeaderText(string text, float left) {
      _tableHeaderContainer.Append(new UIText(text) {
        VAlign = 0.5f,
        Left = StyleDimension.FromPixels(left),
      });
    }
  }

  protected override void OnSetState(AnimCharacter? character) {
    _abilityList.Clear();
    if (character is null) {
      return;
    }

    if (!_abilityItems.TryGetValue(character.Index, out var newItems)) {
      return;
    }

    foreach (UIAbilityListItem item in newItems) {
      _abilityList.Add(item);
    }
  }

  private sealed class UIAbilityListItem : UIPanel, IStateUIElement<AbilityState> {
    private readonly int _abilityIndex;
    private readonly UIAbilityList _parent;

    public AbilityState? State => _parent.State?.GetState(_abilityIndex) as AbilityState;
    State? IStateUIElement.State => State;


    private UIImageButton _button = null!;
    private UIText _levelText = null!;
    private UIText _maxLevelText = null!;
    private UIText _cooldownText = null!;
    private UIText _onCooldownText = null!;

    public UIAbilityListItem(AbilityState ability, UIAbilityList parent) {
      _abilityIndex = ability.Index;
      _parent = parent;
      Width = StyleDimension.Fill;
      Height = StyleDimension.FromPixels(32);
      SetPadding(0);
    }

    public override void OnInitialize() {
      base.OnInitialize();

      AbilityState templateAbility = (AbilityState)StateLoader.TemplateStates[_abilityIndex];

      _button = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay")) {
        HAlign = 0,
        VAlign = 0.5f,
        Left = StyleDimension.FromPixels(8)
      };
      _button.OnLeftClick += Button_OnClick;
      Append(_button);

      AddTextElement(40, out _, templateAbility.Name);
      AddTextElement(190, out _levelText);
      AddTextElement(240, out _maxLevelText, templateAbility.MaxLevel.ToString());
      AddTextElement(320, out _cooldownText);
      AddTextElement(380, out _onCooldownText);
      return;

      void AddTextElement(float left, out UIText element, string text = "") {
        element = new UIText(text) {
          HAlign = 0,
          VAlign = 0.5f,
          Left = StyleDimension.FromPixels(left),
          IgnoresMouseInteraction = true
        };
        Append(element);
      }
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
      base.DrawSelf(spriteBatch);
      AbilityState? ability = State;
      if (ability is null) {
        BackgroundColor = new Color(151, 151, 151) * 0.7f;
        _cooldownText.SetText("");
        _onCooldownText.SetText("");
        return;
      }

      if (_button.ContainsPoint(Main.MouseScreen)) {
        Main.instance.MouseText("Change Ability Level");
      }

      int level = ability.Level;
      BackgroundColor = level > 0
        ? level > ability.MaxLevel
          ? new Color(151, 151, 82) * 0.7f // Yellow, over max level
          : level == ability.MaxLevel
            ? new Color(63, 151, 82) * 0.7f // Green, max level
            : new Color(82, 122, 181) * 0.7f // Blue, not max level
        : new Color(151, 151, 151) * 0.7f; // Gray, level 0
      _levelText.SetText(level.ToString());
      _maxLevelText.SetText(ability.MaxLevel.ToString());

      if (ability is { Unlocked: true, SupportsCooldown: true }) {
        _cooldownText.SetText((ability.CooldownLeft / 60f).ToString("F"));
        _onCooldownText.SetText(ability.IsOnCooldown ? "On CD" : "Ready");
        _onCooldownText.TextColor = ability.IsOnCooldown ? Color.Red : Color.Lime;
      }
      else {
        _cooldownText.SetText("");
        _onCooldownText.SetText("");
      }
    }

    private void Button_OnClick(UIMouseEvent evt, UIElement listeningElement) {
      AbilityState? ability = State;
      if (ability is null) {
        return;
      }

      if (ability.Level == ability.MaxLevel) {
        ability.Level = 0;
      }
      else {
        ability.Level++;
      }
    }
  }
}

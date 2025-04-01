using System.Linq;
using AnimLib.States;
using AnimLib.UI.Debug;
using MrPlagueRaces;
using MrPlagueRaces.Common.Races;
using MrPlagueRaces.Common.Races.Human;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AnimLib.Menus.Debug;

/// <summary>
/// Debug UI that displays info about a character collection's list of <see cref="AnimCharacter"/>.
/// </summary>
public sealed class UICharacterList : DebugUIElement<AnimCharacterCollection> {
  protected override string HeaderHoverText =>
    "Displays a list of all available characters for the selected player.";

  private UIList _characterList = null!; // OnInitialize()

  public override void OnInitialize() {
    base.OnInitialize();
    SetHeaderText("Character List");

    _characterList = new UIList {
      HAlign = 1,
      Width = StyleDimension.FromPixelsAndPercent(-24, 1),
      Height = StyleDimension.FromPixelsAndPercent(-6, 1),
      ListPadding = -1
    };
    _characterList.SetPadding(2);
    _characterList.PaddingRight = 4;
    foreach (AnimCharacter character in StateLoader.TemplateStates.OfType<AnimCharacter>()) {
      _characterList.Add(new UICharacterListItem(character, this));
    }

    if (ModLoader.HasMod("MrPlagueRaces")) {
      AddRaceListItems();
    }

    BodyContainer.Append(_characterList);

    UIScrollbar scrollbar = new() {
      HAlign = 0,
      VAlign = 1,
      Width = StyleDimension.FromPixelsAndPercent(-18, 1),
      Height = StyleDimension.FromPixelsAndPercent(-18, 1),
      Left = StyleDimension.FromPixels(4),
      Top = StyleDimension.FromPixels(-10)
    };
    scrollbar.SetView(100f, 1000f);
    _characterList.SetScrollbar(scrollbar);
    BodyContainer.Append(scrollbar);
  }

  [JITWhenModsEnabled("MrPlagueRaces")]
  private void AddRaceListItems() {
    foreach (Race race in RaceLoader.Races) {
      if (race is not Human) {
        _characterList.Add(new UIPlagueRaceListItem(race, this));
      }
    }
  }

  private sealed class UICharacterListItem : UIPanel {
    private readonly int _characterIndex;
    private readonly UICharacterList _parent;

    private AnimCharacterCollection? Characters => _parent.State;

    private UIImageButton _button = null!;

    public UICharacterListItem(AnimCharacter character, UICharacterList parent) {
      _characterIndex = character.Index;
      _parent = parent;
      Width = StyleDimension.Fill;
      Height = StyleDimension.FromPixels(40f);

      SetPadding(0);
      PaddingLeft = PaddingRight = 12;
    }

    private bool IsSelectedCharacter([NotNullWhen(true)] out AnimCharacter? character) {
      character = Characters?.ActiveCharacter;
      return character is not null && character.Index == _characterIndex;
    }

    public override void OnInitialize() {
      var tex = Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay");
      _button = new UIImageButton(tex) {
        HAlign = 0f,
        VAlign = 0.5f,
        Left = StyleDimension.FromPixels(0),
        Top = StyleDimension.Empty
      };
      _button.OnLeftClick += Button_OnClick;
      Append(_button);

      AnimCharacter state = (AnimCharacter)StateLoader.TemplateStates[_characterIndex];
      Append(new UIText(state.DisplayName ?? state.Name) {
        HAlign = 0f,
        VAlign = 0.5f,
        Left = StyleDimension.FromPixels(_button.Width.Pixels + 8),
        IgnoresMouseInteraction = true
      });
    }

    public override void Update(GameTime gameTime) {
      bool isSelectedCharacter = IsSelectedCharacter(out _);
      _button.SetImage(isSelectedCharacter
        ? AnimLibMod.Instance.Assets.Request<Texture2D>("AnimLib/UI/ButtonExit", AssetRequestMode.ImmediateLoad)
        : Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay"));
      BackgroundColor = isSelectedCharacter
        ? new Color(63, 151, 82) * 0.7f
        : Color.Transparent;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
      if (_button.IsMouseHovering) {
        Main.instance.MouseText(IsSelectedCharacter(out _)
          ? "Disable this character."
          : "Enable this character.");
      }

      base.DrawSelf(spriteBatch);
    }

    private void Button_OnClick(UIMouseEvent evt, UIElement listeningElement) {
      if (IsSelectedCharacter(out AnimCharacter? character)) {
        character.Disable();
      }
      else {
        Characters?.GetCharacter(_characterIndex).Enable();
      }
    }
  }

  [JITWhenModsEnabled("MrPlagueRaces")]
  private sealed class UIPlagueRaceListItem : UIPanel {
    private readonly Race _race;
    private readonly UICharacterList _parent;

    private MrPlagueRacesPlayer? GetRacePlayer() => _parent.State?.Player.GetModPlayer<MrPlagueRacesPlayer>();

    private bool IsSelectedRace => GetRacePlayer()?.race is { } playerRace && ReferenceEquals(playerRace, _race);

    private UIImageButton _button = null!;

    public UIPlagueRaceListItem(Race race, UICharacterList parent) {
      _race = race;
      _parent = parent;
      Width = StyleDimension.Fill;
      Height = StyleDimension.FromPixels(40f);

      SetPadding(0);
      PaddingLeft = PaddingRight = 12;
      BackgroundColor = Color.Purple;
    }

    public override void OnInitialize() {
      var tex = Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay");
      _button = new UIImageButton(tex) {
        HAlign = 0f,
        VAlign = 0.5f,
        Left = StyleDimension.FromPixels(0),
        Top = StyleDimension.Empty
      };
      _button.OnLeftClick += Button_OnClick;
      Append(_button);

      Append(new UIText(_race.DisplayName ?? _race.Name) {
        HAlign = 0f,
        VAlign = 0.5f,
        Left = StyleDimension.FromPixels(_button.Width.Pixels + 8),
        IgnoresMouseInteraction = true
      });
    }

    public override void Update(GameTime gameTime) {
      bool isSelectedCharacter = IsSelectedRace;
      _button.SetImage(isSelectedCharacter
        ? AnimLibMod.Instance.Assets.Request<Texture2D>("AnimLib/UI/ButtonExit", AssetRequestMode.ImmediateLoad)
        : Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay"));
      BackgroundColor = isSelectedCharacter
        ? new Color(131, 63, 221) * 0.7f
        : new Color(121, 63, 151) * 0.4f;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
      if (_button.IsMouseHovering) {
        Main.instance.MouseText(IsSelectedRace
          ? "Disable this race."
          : "Enable this race.");
      }

      base.DrawSelf(spriteBatch);
    }

    private void Button_OnClick(UIMouseEvent evt, UIElement listeningElement) {
      if (_parent.State is null) {
        return;
      }

      MrPlagueRacesPlayer racePlayer = GetRacePlayer()!;
      if (IsSelectedRace) {
        RaceLoader.TryGetRace("MrPlagueRaces/Human", out Race human);
        racePlayer.race = human;
        return;
      }

      racePlayer.race = _race;
      if (_parent.State.ActiveCharacter is { } character) {
        character.Disable();
      }
    }
  }
}

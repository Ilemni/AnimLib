using AnimLib.Menus.Debug;
using JetBrains.Annotations;
using Terraria.UI;

namespace AnimLib.Systems;

[Autoload(Side = ModSide.Client)]
[UsedImplicitly]
public sealed class DebugUISystem : ModSystem {
  private UserInterface? _uiInterface;
  private UIDebugMenus? _ui;
  private LegacyGameInterfaceLayer? _debugAnimationLayer;
  private GameTime? _gameTime;


  public void SetCharacters(AnimCharacterCollection collection) {
    _ui?.SetCharacters(collection);
  }

  /// <summary>
  /// Attempts to set the character displayed in the UI to the specified collection's active character.
  /// <para/> This method does nothing if the UI is not currently displaying the specified collection.
  /// </summary>
  /// <param name="collection"></param>
  public void TrySetActiveCharacter(AnimCharacterCollection collection) {
    if (ReferenceEquals(_ui?.Characters, collection)) {
      _ui?.SetCharacter(collection.ActiveCharacter);
    }
  }

  public override void PostSetupContent() {
    _ui = new UIDebugMenus();
    _ui.Activate();
    _uiInterface = new UserInterface();
    _uiInterface.SetState(AnimLibMod.DebugEnabled ? _ui : null);
    _debugAnimationLayer = new LegacyGameInterfaceLayer("AnimLib: Debug Animation", DrawMethod, InterfaceScaleType.UI);
  }

  private bool DrawMethod() {
    if (_gameTime is not null && _uiInterface?.CurrentState is not null) {
      _uiInterface.Draw(Main.spriteBatch, _gameTime);
    }

    return true;
  }

  public override void UpdateUI(GameTime gameTime) {
    _gameTime = gameTime;
    _uiInterface?.SetState(AnimLibMod.DebugEnabled ? _ui : null);
    _uiInterface?.Update(gameTime);
  }

  public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
    if (_debugAnimationLayer is null) {
      return;
    }

    int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
    if (mouseTextIndex != -1) {
      layers.Insert(mouseTextIndex, _debugAnimationLayer);
    }
  }
}

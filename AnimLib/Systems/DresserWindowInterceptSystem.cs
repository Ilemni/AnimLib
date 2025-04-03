using System.Diagnostics;
using JetBrains.Annotations;
using Terraria.GameContent;

namespace AnimLib.Systems;

/// <summary>
/// This ModSystem performs two roles:
/// <br/> - Draw over the dresser window icons with custom textures, representing the active character's appearance.
/// <br/> - Inform <see cref="AnimCharacterCollection"/> when the dresser window is being drawn.
/// </summary>
[UsedImplicitly]
public sealed class DresserWindowInterceptSystem : ModSystem {
  public override void Load() {
    Log.Debug("Adding hooks to Main.DrawClothesWindow, for updating AnimCharacter UI fields");
    On_Main.DrawClothesWindow += OnDrawClothesWindow;
  }

  private static void OnDrawClothesWindow(On_Main.orig_DrawClothesWindow orig, Main self) {
    AnimCharacterCollection dummyCollection = Main.dresserInterfaceDummy.GetState<AnimCharacterCollection>();
    AnimCharacterCollection localCollection = Main.LocalPlayer.GetState<AnimCharacterCollection>();
    if (localCollection.ActiveCharacter is null) {
      dummyCollection.ActiveCharacter?.Disable();

      // Enabling or disabling characters normally closes the window
      // Force it to stay open
      Main.clothesWindow = true;
      orig(self);
      return;
    }

    dummyCollection.Enable(dummyCollection.GetState(localCollection.ActiveCharacter));
    Main.clothesWindow = true;

    // category value here is based on UICharacterCreation.CategoryId value
    int category = Main.selClothes switch {
      0 => 6, // shirtColor,
      1 => 7, // underShirtColor,
      2 => 8, // pantsColor,
      3 => 9, // shoeColor,
      4 => 4, // eyeColor,
      5 => 5, // skinColor,
      _ => throw new UnreachableException("Main.selClothes was not in range [0, 5]")
    };

    using (dummyCollection.UiInfo.Store()) {
      dummyCollection.UiInfo.CategoryIndex = category;
      orig(self);
    }

    // Draw our textures

    Player p = Main.LocalPlayer;
    AnimCharacterStyle.UISettings? s = localCollection.ActiveCharacter.Style.UiSettings;
    if (s is null) {
      return;
    }

    const int invBgW = 462;
    int invX = Main.screenWidth / 2 - invBgW / 2;
    int invY = Main.screenHeight / 2 + 60;
    int selX = invX + invBgW - 180;
    int selY = invY + 10;

    var vTex = TextureAssets.Clothes;
    var eyeBack = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/ColorEyeBack");

    ReadOnlySpan<(int x, int y, (Asset<Texture2D>? tex, Asset<Texture2D>? midTex), bool hide, Color color)> buttons = [
      (0, 0, s.ShirtColorIcon ?? (vTex[0], null), s.HideShirtColorOption, p.shirtColor),
      (1, 0, s.UnderShirtColorIcon ?? (vTex[1], null), s.HideUnderShirtColorOption, p.underShirtColor),
      (0, 1, s.PantsColorIcon ?? (vTex[2], null), s.HidePantsColorOption, p.pantsColor),
      (1, 1, s.ShoeColorIcon ?? (vTex[3], null), s.HideShoeColorOption, p.shoeColor),
      (2, 0, s.EyeColorIcon ?? (vTex[4], eyeBack), s.HideEyeColorOption, p.eyeColor),
      (2, 1, s.SkinColorIcon ?? (vTex[5], null), s.HideSkinColorOption, p.skinColor)
    ];

    for (int i = 0; i < buttons.Length; i++) {
      (int x, int y, var (tex, midTex), bool hide, Color color) = buttons[i];
      Texture2D? bgTex = Main.selClothes == i
        ? TextureAssets.InventoryBack14.Value
        : TextureAssets.InventoryBack8.Value;
      Vector2 position = new(selX + x * 56, selY + y * 56);
      Main.spriteBatch.Draw(bgTex, position, new Color(200, 200, 200, 255));
      if (hide) {
        continue;
      }

      position += bgTex.Size() / 2;
      if (midTex is not null) {
        Main.spriteBatch.Draw(midTex.Value, position - midTex.Size() / 2, midTex.Frame(), Color.White);
      }

      if (tex is not null) {
        Main.spriteBatch.Draw(tex.Value, position - tex.Size() / 2, tex.Frame(), color);
      }
    }
  }
}

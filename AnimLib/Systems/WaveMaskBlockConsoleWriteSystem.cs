using AnimLib.Utilities;
using JetBrains.Annotations;
using Terraria.GameContent.Liquid;

namespace AnimLib.Systems;

/// <summary>
/// This ModSystem exists just to block the
/// "WaveMaskData texture recreated." console spam in
/// <see cref="LiquidRenderer.SetWaveMaskData"/>.
/// </summary>
[UsedImplicitly]
public sealed class WaveMaskBlockConsoleWriteSystem : ModSystem {
  // ReSharper disable InconsistentNaming - Funcs
  private readonly Func<LiquidRenderer, Rectangle> GetDrawArea =
    ClassHacking.CreateGetter<LiquidRenderer, Rectangle>("_drawArea");

  private readonly Func<LiquidRenderer, Color[]> GetWaveMask =
    ClassHacking.CreateGetter<LiquidRenderer, Color[]>("_waveMask");
  // ReSharper restore InconsistentNaming

  public override bool IsLoadingEnabled(Mod mod) => !string.IsNullOrWhiteSpace(mod.SourceFolder);

  public override void Load() {
    Log.Debug(
      "Adding hook to LiquidRenderer.SetWaveMaskData, to block \"WaveMaskData texture recreated\" console spam");
    On_LiquidRenderer.SetWaveMaskData += On_LiquidRendererOnSetWaveMaskData;
  }

  private void On_LiquidRendererOnSetWaveMaskData(On_LiquidRenderer.orig_SetWaveMaskData orig, LiquidRenderer self,
    ref Texture2D? texture) {
    Rectangle drawArea = GetDrawArea(self);
    var waveMask = GetWaveMask(self);
    try {
      if (texture is null || texture.Width < drawArea.Height || texture.Height < drawArea.Width) {
        // Console.WriteLine("WaveMaskData texture recreated. {0}x{1}", drawArea.Height, drawArea.Width);
        if (texture is not null) {
          try {
            texture.Dispose();
          }
          catch {
            // ignored
          }
        }

        texture = new Texture2D(Main.instance.GraphicsDevice, drawArea.Height, drawArea.Width, mipMap: false,
          SurfaceFormat.Color);
      }

      texture.SetData(0, new Rectangle(0, 0, drawArea.Height, drawArea.Width), waveMask, 0,
        drawArea.Width * drawArea.Height);
    }
    catch {
      texture = new Texture2D(Main.instance.GraphicsDevice, drawArea.Height, drawArea.Width, mipMap: false,
        SurfaceFormat.Color);
      texture.SetData(0, new Rectangle(0, 0, drawArea.Height, drawArea.Width), waveMask, 0,
        drawArea.Width * drawArea.Height);
    }
  }
}

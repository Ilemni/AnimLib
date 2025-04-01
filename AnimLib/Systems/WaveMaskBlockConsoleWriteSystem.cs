using System.Reflection;
using JetBrains.Annotations;
using Terraria.GameContent.Liquid;

namespace AnimLib.Systems;

/// <summary>
/// This ModSystem exists just to block the
/// "WaveMaskData texture recreated." console spam in
/// <see cref="LiquidRenderer.SetWaveMaskData"/>.
/// </summary>
[UsedImplicitly]
public class WaveMaskBlockConsoleWriteSystem : ModSystem {
  private static readonly FieldInfo DrawAreaField =
    typeof(LiquidRenderer).GetField("_drawArea", BindingFlags.NonPublic | BindingFlags.Instance)!;

  private static readonly FieldInfo WaveMaskField =
    typeof(LiquidRenderer).GetField("_waveMask", BindingFlags.NonPublic | BindingFlags.Instance)!;

#if DEBUG
  public override void Load() {
    Log.Debug("Adding hook to LiquidRenderer.SetWaveMaskData, to block \"WaveMaskData texture recreated\" console spam");
    On_LiquidRenderer.SetWaveMaskData += On_LiquidRendererOnSetWaveMaskData;
  }
#endif

  private static void On_LiquidRendererOnSetWaveMaskData(On_LiquidRenderer.orig_SetWaveMaskData orig,
    LiquidRenderer self, ref Texture2D? texture) {
    Rectangle drawArea = (Rectangle)DrawAreaField.GetValue(self)!;
    var waveMask = (Color[])WaveMaskField.GetValue(self)!;
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

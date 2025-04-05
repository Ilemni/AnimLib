using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AsepriteDotNet.Common;
using AsepriteDotNet.IO;
using RectpackSharp;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AnimLib.Aseprite.Processors;

/// <summary>
/// This class packs a set of rectangles into a texture atlas.
/// <para/> This class additionally saves the packed rectangles to a png file for debugging purposes.
/// </summary>
internal static class RectPacker {
  private static readonly string CachePath = Path.Combine(Program.SavePathShared, "Cache", "AnimLib");
  private static readonly string SavePath = Path.Combine(CachePath, "RectPacking");

  public static void Pack(Span<Rectangle> rects, string mod, string name, ref ushort w, ref ushort h, int frameCount,
    List<Dictionary<int, int>> duplicateMaps) {
    int rectCount = rects.Length;
    if (rectCount == 1) {
      ref Rectangle first = ref rects[0];
      first.X = 0;
      first.Y = 0;
      w = (ushort)first.Width;
      h = (ushort)first.Height;
      return;
    }

    int numUnique = 0;
    int numDuplicate = 0;
    var packingRects = rectCount < 256 ? stackalloc PackingRectangle[rectCount] : new PackingRectangle[rectCount];
    for (int i = 0; i < rects.Length; i++) {
      int layerIndex = i / frameCount;
      int frameIndex = i % frameCount;
      var duplicateMap = duplicateMaps.Count > layerIndex ? duplicateMaps[layerIndex] : null;
      int linkedIndex = duplicateMap?.GetValueOrDefault(frameIndex, -1) ?? -1;
      Rectangle r = rects[i];

      if (linkedIndex == -1) {
        numUnique++;
      }
      else {
        linkedIndex += frameCount * layerIndex;
        if (r is { Width: > 0, Height: > 0 }) {
          numDuplicate++;
        }
      }

      // Add padding to prevent texture bleeding
      if (r is { Width: > 0, Height: > 0 }) {
        r.Width += 1;
        r.Height += 1;
      }

      packingRects[i] = new PackingRectangle(r, i, linkedIndex);
    }

    Stopwatch sw = new();
    sw.Start();
    RectanglePacker.Pack(packingRects, out PackingRectangle bounds, maxBoundsWidth: w, maxBoundsHeight: h);
    sw.Stop();

    // Remove outer padding from atlas
    bounds.X -= 1;
    bounds.Y -= 1;

    int numPixels = 0;
    for (int i = 0; i < packingRects.Length; i++) {
      ref PackingRectangle packingRect = ref packingRects[i];
      // remove sourceRect padding
      if (packingRect is { Width: > 0, Height: > 0 }) {
        packingRect.Width -= 1;
        packingRect.Height -= 1;
      }

      if (packingRect.LinkedId == -1) {
        numPixels += (ushort)packingRect.Area;
      }
    }

    // Only log if asset is from a mod that is actually developing
    if (CanSaveFile(mod)) {
      Log.Debug($"[{mod}:{name}] " +
        $"Packed {numUnique} rects " +
        (numDuplicate > 0 ? $"(and merged {numDuplicate}) " : "") +
        $"to {bounds.Width}x{bounds.Height}. " +
        $"Density: {numPixels / (float)bounds.Area:P2}. " +
        $"Completed in {sw.ElapsedTicks / 10000f:F}ms.");
    }

    foreach (PackingRectangle pr in packingRects) {
      rects[pr.Id] = pr.LinkedId != -1 ? packingRects[pr.LinkedId] : pr;
    }

    w = (ushort)bounds.Width;
    h = (ushort)bounds.Height;

    SavePngBoxes(rects, mod, name, w, h);
  }


  /// Output files if a mod developer opts into it by creating folder "AnimLibCache" in their Terraria folder.
  /// Disallow saving if a user is not actually developing the specified mod.
  private static bool CanSaveFile(string modName) {
    if (!ModLoader.TryGetMod(modName, out Mod mod)) {
      return false;
    }

    if (string.IsNullOrWhiteSpace(mod.SourceFolder) || !Directory.Exists(mod.SourceFolder)) {
      return false;
    }

    if (!Directory.Exists(CachePath)) {
      return false;
    }

    Directory.CreateDirectory(SavePath);
    return true;
  }

  private static string GetPath(string name, string modName) {
    string fullPath = Path.Combine(SavePath, modName, $"{name}.png");
    string directoryName = Path.GetDirectoryName(fullPath)!;
    Directory.CreateDirectory(directoryName);

    return fullPath;
  }

  /// Saves to "name (boxes).png" and "name (empty).png"
  /// Png with boxes represents the packed rectangles as white rects, with padding transparent.
  /// Png with empty represents the negative space filled in red, excluding padding.
  private static void SavePngBoxes(ReadOnlySpan<Rectangle> rects, string mod, string name, ushort w, ushort h) {
    if (!CanSaveFile(mod)) {
      return;
    }

    var data = new Rgba32[w * h];
    Array.Clear(data);

    Rgba32 white = new(255, 255, 255, 255);

    foreach (Rectangle rect in rects) {
      for (int y = rect.Y; y < rect.Bottom; y++) {
        for (int x = rect.X; x < rect.Right; x++) {
          data[y * w + x] = white;
        }
      }
    }

    string path = GetPath($"{name} (boxes)", mod);
    PngWriter.SaveTo(path, w, h, data);

    Array.Fill(data, new Rgba32(255, 0, 0, 255));

    Rgba32 clear = new(0, 0, 0, 0);
    foreach (Rectangle rect in rects) {
      // Clear padding
      int left = Math.Max(rect.X - 1, 0);
      int top = Math.Max(rect.Y - 1, 0);
      int bottom = Math.Min(rect.Bottom, h - 1);
      int right = Math.Min(rect.Right, w - 1);
      for (int y = top; y < bottom; y++) {
        data[y * w + left] = clear;
        data[y * w + right] = clear;
      }

      for (int x = left; x < right; x++) {
        data[top * w + x] = clear;
        data[bottom * w + x] = clear;
      }

      // Clear rect
      for (int y = rect.Y; y < rect.Bottom; y++) {
        for (int x = rect.X; x < rect.Right; x++) {
          data[y * w + x] = clear;
        }
      }
    }

    string path2 = GetPath($"{name} (empty)", mod);
    Task.Run(() => PngWriter.SaveTo(path2, w, h, data));
  }

  /// Saves to "name (art).png"
  /// Png is the unmodified atlas, as it is used in game.
  internal static void SavePng(ReadOnlySpan<Rgba32> pixels, string mod, string name, ushort w, ushort h) {
    if (!CanSaveFile(mod)) {
      return;
    }

    var data = new Rgba32[w * h];
    pixels.CopyTo(data);

    string path = GetPath($"{name} (art)", mod);
    Task.Run(() => PngWriter.SaveTo(path, w, h, data));
  }

  /// Saves to "name (bounds).png"
  /// Png with bounds represents the atlas as used in game, with bounding rects drawn in color.
  internal static void SavePngBounds(ReadOnlySpan<Rgba32> pixels, string mod, string name, ushort w, ushort h,
    int frameCount, ReadOnlySpan<Rectangle> rects) {
    if (!CanSaveFile(mod)) {
      return;
    }

    var data = new Rgba32[w * h];
    pixels.CopyTo(data);
    for (int i = 0; i < rects.Length; i++) {
      Rectangle rect = rects[i];
      if (rect is not { Width: > 0, Height: > 0, X: >= 0, Y: >= 0 }) {
        continue;
      }

      Rgba32 color = (i / frameCount) switch {
        0 => new Rgba32(255, 0, 0, 255),
        1 => new Rgba32(192, 64, 0, 255),
        2 => new Rgba32(128, 128, 0, 255),
        3 => new Rgba32(64, 192, 0, 255),
        4 => new Rgba32(0, 255, 0, 255),
        5 => new Rgba32(0, 192, 64, 255),
        6 => new Rgba32(0, 128, 128, 255),
        7 => new Rgba32(0, 64, 192, 255),
        8 => new Rgba32(0, 0, 255, 255),
        9 => new Rgba32(64, 0, 192, 255),
        10 => new Rgba32(128, 0, 128, 255),
        11 => new Rgba32(192, 0, 64, 255),
        _ => new Rgba32(255, 255, 255, 255)
      };

      // Draw border around rect
      int top = Math.Max(rect.Y, 0);
      int bottom = Math.Min(rect.Bottom, h);
      for (int y = top; y < bottom; y++) {
        int l = Math.Max(rect.X, 0);
        int r = Math.Min(rect.Right - 1, w);
        data[y * w + l] = color;
        data[y * w + r] = color;
      }

      int left = Math.Max(rect.X, 0);
      int right = Math.Min(rect.Right, w);
      for (int x = left; x < right; x++) {
        int t = Math.Max(rect.Y, 0);
        int b = Math.Min(rect.Bottom - 1, h - 1);
        data[t * w + x] = color;
        data[b * w + x] = color;
      }
    }

    string path = GetPath($"{name} (bounds)", mod);
    PngWriter.SaveTo(path, w, h, data);
  }
}

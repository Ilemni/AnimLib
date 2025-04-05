using System.IO;
using System.Threading.Tasks;
using AnimLib.Animations;
using AnimLib.Aseprite;
using AnimLib.Utilities;
using JetBrains.Annotations;
using ReLogic.Content.Sources;

namespace AnimLib.Systems;

[UsedImplicitly]
public sealed class AsepriteFileWatcherSystem : ModSystem {
  // ReSharper disable InconsistentNaming - Actions/Funcs
  private readonly Func<AssetRepository, Dictionary<string, IAsset>> GetAssets =
    ClassHacking.CreateGetter<AssetRepository, Dictionary<string, IAsset>>("_assets");

  private readonly Action<Asset<AnimSpriteSheet>, AnimSpriteSheet, IContentSource> SubmitLoadedContent =
    ClassHacking.CreateDelegate<Asset<AnimSpriteSheet>, AnimSpriteSheet, IContentSource>("SubmitLoadedContent");
  // ReSharper restore InconsistentNaming


  private FileSystemWatcher? _modSourcesWatcher; // Load()
  private readonly string _modSourcesPath = Path.Combine(Program.SavePathShared, "ModSources");

  private readonly Dictionary<string, (Mod mod, Asset<AnimSpriteSheet> asset)> _changedAssets = [];

  // TODO: perhaps create a watcher for each mod where SourceFolder that is not the standard folder?
  public override void Load() {
    _modSourcesWatcher?.Dispose();
    _modSourcesWatcher = new FileSystemWatcher {
      Path = _modSourcesPath,
      NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
      IncludeSubdirectories = true
    };
    _modSourcesWatcher.Filters.Add("*.ase");
    _modSourcesWatcher.Filters.Add("*.aseprite");
    _modSourcesWatcher.Changed += (_, e) => OnFileChanged(e.FullPath);
    _modSourcesWatcher.Renamed += (_, e) => OnFileChanged(e.FullPath);
    _modSourcesWatcher.EnableRaisingEvents = true;
  }

  public override void Unload() {
    _modSourcesWatcher?.Dispose();
    _modSourcesWatcher = null;
  }

  private void OnFileChanged(string fullPath) {
    if (_changedAssets.ContainsKey(fullPath)) {
      return;
    }

    string name = Path.ChangeExtension(AssetPathHelper.CleanPath(fullPath[(_modSourcesPath.Length + 1)..]), null);

    // Asset names in AssetRepository use "\" as separator.
    // Cannot use ModContent.SplitName. It will throw since it does not support "\" as a valid separator.
    int split = name.IndexOf('\\');
    string modName = name[..split];
    string assetName = name[(split + 1)..];

    if (!ModLoader.TryGetMod(modName, out Mod mod)) {
      return;
    }

    // We do NOT want to create any new assets not yet requested, or replace non-AnimSpriteSheet asset types.
    if (!GetAssets(mod.Assets).TryGetValue(assetName, out IAsset? assetObj)) {
      return;
    }

    // TODO: Support more types to reload, rather than just AnimSpriteSheet.
    //  This may require using MethodInfo.Invoke, or registering delegates for our known types.
    if (assetObj is { IsLoaded: true } and Asset<AnimSpriteSheet> typedAsset) {
      _changedAssets.Add(fullPath, (mod, typedAsset));
    }
  }

  public override void PostUpdateEverything() {
    if (_changedAssets.Count == 0) {
      return;
    }

    AseReader reader = AnimLibMod.Instance.AseReader;

    foreach ((string fullPath, (Mod mod, var asset)) in _changedAssets) {
      Task.Run(async () => {
        try {
          await using FileStream stream = new(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
          AnimSpriteSheet result = await reader.FromStream<AnimSpriteSheet>(stream, default);
          SubmitLoadedContent(asset, result, null!);
          Main.NewText($"Spritesheet updated from ModSources folder: {mod.Name}:{asset.Name}");
        }
        catch (Exception e) {
          Main.NewText($"Spritesheet failed to update from ModSources folder: {mod.Name}:{asset.Name}");
          Main.NewText(e.Message);
        }
      });
    }

    _changedAssets.Clear();
  }
}

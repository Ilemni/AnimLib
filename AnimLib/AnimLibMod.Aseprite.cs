using System.Linq;
using System.Reflection;
using AnimLib.Aseprite;
using ReLogic.Content.Readers;
using ReLogic.Content.Sources;
using ReLogic.Utilities;

namespace AnimLib;

public sealed partial class AnimLibMod {
  public AseReader AseReader { get; private set; } = null!; // CreateDefaultContentSource

  /// <summary>
  /// Add the Aseprite reader <see cref="AseReader"/>.
  /// <br/> This will allow mods using AnimLib to request Aseprite files from Assets.
  /// </summary>
  /// <remarks>
  /// Autoload for aseprite files will not work for mods that load earlier than Animlib,
  /// unless this <see cref="AseReader"/>, or a derivative of it, is added to tML proper.
  /// </remarks>
  public override IContentSource CreateDefaultContentSource() {
    if (Main.dedServ) {
      return base.CreateDefaultContentSource();
    }

    AseReader = new AseReader();
    AseReader.AddDefaultProcessors();
    GetAssetReaderCollection().RegisterReader(AseReader, ".ase", ".aseprite");
    return base.CreateDefaultContentSource();
  }

  private static AssetReaderCollection GetAssetReaderCollection() =>
    Main.instance.Services.Get<AssetReaderCollection>();

  public void UnloadAse() {
    // Method exists just to remove our AseReader which was registered in CreateDefaultContentSource()
    AseReader.Unload();

    const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

    AssetReaderCollection collection = GetAssetReaderCollection();

    Type type = collection.GetType();
    var readers = (Dictionary<string, IAssetReader>)type
      .GetField("_readersByExtension", flags)!
      .GetValue(collection)!;

    // Bitwise OR is intended
    if (readers.Remove(".ase") | readers.Remove(".aseprite")) {
      type.GetField("_extensions", flags)!
        .SetValue(collection, readers.Keys.ToArray());
    }
  }
}

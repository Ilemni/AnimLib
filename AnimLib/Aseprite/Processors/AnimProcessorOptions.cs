namespace AnimLib.Aseprite.Processors;

public struct AnimProcessorOptions {
  /// <summary>
  /// Initializes a new instance of the <see cref="AnimProcessorOptions"/> struct.
  /// </summary>
  /// <param name="upscale">
  /// Whether to upscale the aseprite pixels to Terraria's 2x2 style.
  /// </param>
  /// <param name="onlyVisibleLayers">
  /// Whether to ignore hidden layers.
  /// Hidden layers marked with Green userdata will be imported regardless.
  /// </param>
  /// <param name="includeBackgroundLayer">
  /// Currently only used by AsepriteDotNet processors.
  /// </param>
  /// <param name="mergeDuplicateFrames">
  /// Whether to combine identical cels into a single frame.
  /// Cels can be combined even if they're offset differently.
  /// </param>
  /// <param name="includeTilemapLayers">
  /// Currently only used by AsepriteDotNet processors.
  /// </param>
  // Super basic tilemap support could be useful for importing Aseprite files to structures.
  public AnimProcessorOptions(bool upscale,
    bool onlyVisibleLayers,
    bool includeBackgroundLayer,
    bool mergeDuplicateFrames,
    bool includeTilemapLayers) {
    Upscale = upscale;
    OnlyVisibleLayers = onlyVisibleLayers;
    IncludeBackgroundLayer = includeBackgroundLayer;
    MergeDuplicateFrames = mergeDuplicateFrames;
    IncludeTilemapLayers = includeTilemapLayers;
  }

  /// <summary>
  /// The default <see cref="AnimProcessorOptions"/> instance.
  /// <list type="table">
  /// <listheader>
  /// <term>Property</term>
  /// <description>Value</description>
  /// </listheader>
  /// <item><term><see cref="Upscale"/></term> <description><see langword="false"/></description></item>
  /// <item><term><see cref="OnlyVisibleLayers"/></term><description><see langword="true"/></description></item>
  /// <item><term><see cref="IncludeBackgroundLayer"/></term><description><see langword="false"/></description></item>
  /// <item><term><see cref="MergeDuplicateFrames"/></term><description><see langword="true"/></description></item>
  /// <item><term><see cref="IncludeTilemapLayers"/></term><description><see langword="true"/></description></item>
  /// </list>
  /// </summary>
  public static AnimProcessorOptions Default => new(false, true, false, true, true);

  /// <summary>
  /// Whether to upscale the aseprite pixels to Terraria's 2x2 style.
  /// </summary>
  public bool Upscale { get; set; } = true;

  /// <summary>
  /// Whether to ignore hidden layers.
  /// Hidden layers marked with Green userdata will be imported regardless.
  /// </summary>
  public bool OnlyVisibleLayers { get; set; } = true;

  /// <summary>
  /// Currently only used by AsepriteDotNet processors.
  /// </summary>
  public bool IncludeBackgroundLayer { get; set; }

  /// <summary>
  /// Whether to combine identical cels into a single frame.
  /// Cels can be combined even if they're offset differently.
  /// </summary>
  public bool MergeDuplicateFrames { get; set; } = true;

  /// <summary>
  /// Currently only used by AsepriteDotNet processors.
  /// </summary>
  public bool IncludeTilemapLayers { get; set; } = true;
}

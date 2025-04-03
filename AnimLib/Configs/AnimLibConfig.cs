using JetBrains.Annotations;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;

#pragma warning disable CA1822 // Mark members as static
namespace AnimLib.Configs;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AnimLibConfig : ModConfig {
  public override ConfigScope Mode => ConfigScope.ClientSide;

  public bool DebugModeOnStart { get; set; }

  [ReloadRequired]
  public bool DisablePlayerCreationMenuChanges { get; set; }

  [JsonIgnore, ShowDespiteJsonIgnore]
  public bool DebugModeEnabled {
    get => AnimLibMod.DebugEnabled;
    set => AnimLibMod.DebugEnabled = value;
  }

  public override void OnLoaded() {
    DebugModeEnabled = DebugModeOnStart;
  }
}

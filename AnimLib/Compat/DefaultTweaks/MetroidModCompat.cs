using Terraria.DataStructures;

namespace AnimLib.Compat.Implementations;

/// <summary>
/// Hides character sprite when MetroidMod's
/// MorphBall is active
/// </summary>
internal sealed class MetroidModMorphBallCompat : AnimCompatSystem {
  private const string ModName = "MetroidMod";

  public override void PostSetupContent() {
    if (!ModLoader.HasMod(ModName)) {
      return;
    }

    if (!ModContent.TryFind(ModName, "BallLayer", out PlayerDrawLayer ballLayer)) {
      Log.Warn($"{Name} compat subsystem is unable to start, " +
        $"due to desired content was not found, " +
        $"though mod {ModName} is present, " +
        $"please notify developers of AnimLib");
      return;
    }

    GlobalCompatConditions.AddGraphicsDisableCondition(
      GetStandardPredicate(
        p => ballLayer.GetDefaultVisibility(new PlayerDrawSet { drawPlayer = p })
      ));
    Initialized = true;
  }
}

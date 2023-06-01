using System.Linq;
using Terraria.ModLoader;

namespace AnimLib.Compat.Implementations
{
  /// <summary>
  /// Hides character sprite when MetroidMod's
  /// MorphBall is active
  /// </summary>
  internal class MetroidModMorphBallCompat : AnimCompatSystem
  {
    public const string ModName = "MetroidMod";
    public override void PostSetupContent()
    {
      if (ModLoader.Mods.Any(m => m.Name == ModName))
      {
        if (ModContent.TryFind(ModName, "BallLayer", out PlayerDrawLayer BallLayer))
        {
          GlobalCompatConditions.AddGraphicsDisableCondition(
            GetStandardPredicate(
              p => BallLayer.GetDefaultVisibility(new() { drawPlayer = p })
            ));
          _initialized = true;
        }
        else
        {
          Log.LogWarning($"{Name} compat subsystem is unable to start, " +
            $"due to desired content was not found, " +
            $"though mod {ModName} is present, " +
            $"please notify developers of AnimLib");
        }
      }
    }
  }
}

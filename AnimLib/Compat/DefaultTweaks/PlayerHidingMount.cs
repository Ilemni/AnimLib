using Terraria.ID;

namespace AnimLib.Compat.Implementations;

/// <summary>
/// Hides character sprite when
/// mount which hides vanilla player
/// sprite is active
/// </summary>
public sealed class PlayerHidingMount : AnimCompatSystem {
  private readonly List<int> _mountIds = [MountID.Wolf];

  public override void PostSetupContent() {
    Span<(string modName, IEnumerable<string> mounts)> modMounts = [
      ("MountAndJourney", [
        "MAJ_SquirrelTransformation",
        "MAJ_ArcticFoxTransformation"
      ])
    ];

    foreach ((string modName, var mounts) in modMounts) {
      if (!ModLoader.TryGetMod(modName, out Mod mod)) {
        continue;
      }

      foreach (string mount in mounts) {
        if (mod.TryFind(mount, out ModMount m)) {
          _mountIds.Add(m.Type);
        }
        else {
          Log.Warn($"Desired Player Hiding Mount " +
            $"({mount}) was not found, " +
            $"though mod {modName} is present, " +
            $"please notify developers of AnimLib");
        }
      }
    }

    GlobalCompatConditions.AddGraphicsDisableCondition(
      GetStandardPredicate(p => p.mount.Active && _mountIds.Contains(p.mount.Type)));
    Initialized = true;
  }
}

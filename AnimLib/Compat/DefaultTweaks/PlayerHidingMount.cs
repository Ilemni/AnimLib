using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;

namespace AnimLib.Compat.Implementations {
  /// <summary>
  /// Hides character sprite when 
  /// mount which hides vanilla player 
  /// sprite is active
  /// </summary>
  public class PlayerHidingMount : AnimCompatSystem {
    public readonly List<int> mount_ids = new() { MountID.Wolf };

    public override void PostSetupContent() {
      var mod_mounts = new[] {
        ("MountAndJourney", new[] {"MAJ_SquirrelTransformation",
          "MAJ_ArcticFoxTransformation"})
      };

      foreach (var mount_list in mod_mounts) {
        if(ModLoader.Mods.Any(x => x.Name==mount_list.Item1)) {
          foreach (var mount in mount_list.Item2) {
            if (ModContent.TryFind(mount_list.Item1, mount, out ModMount m)) {
              mount_ids.Add(m.Type);
            }
            else {
              Log.Warn($"Desired Player Hiding Mount " +
                $"({mount}) was not found, " +
                $"though mod {mount_list.Item1} is present, " +
                $"please notify developers of AnimLib");
            }
          }
        }
      }

      GlobalCompatConditions.AddGraphicsDisableCondition(
        GetStandardPredicate(p => {
          if (p.mount is not null && p.mount.Active) {
            return mount_ids.Contains(p.mount.Type);
          }
          return false;
        }
      ));
      _initialized = true;
    }
  }
}

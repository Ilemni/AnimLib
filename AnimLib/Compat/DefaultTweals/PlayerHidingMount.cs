using System.Collections.Generic;
using Terraria.ID;

namespace AnimLib.Compat.Implementations {
  /// <summary>
  /// Hides character sprite when 
  /// mount which hides vanilla player 
  /// sprite is active
  /// </summary>
  public class PlayerHidingMount : AnimCompatSystem {
    public readonly HashSet<int> mount_ids = new() { MountID.Wolf };

    public override void PostSetupContent() {
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

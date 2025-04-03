using AnimLib.Networking;
using AnimLib.States;
using Terraria.ModLoader.IO;

namespace AnimLib;

public abstract partial class AnimCharacter {
  /// <summary>
  /// Key for storing value of <see cref="State.Active"/>.
  /// This value is saved separately from collection.ActiveCharacter to allow
  /// re-enabling characters whose mod was previously disabled,
  /// but wil not re-enable if a new character is enabled.
  /// </summary>
  private const string ActiveKey = "active";
  private const string StyleKey = "style";

  /// <summary>
  /// Syncs <see cref="AbilityStates"/>'s <see cref="AbilityState.Level"/>,
  /// and <see cref="Style"/>.
  /// </summary>
  internal override void NetSyncInternal(NetSyncer sync) {
    foreach (AbilityState abilityState in AbilityStates) {
      abilityState.SyncLevel(sync);
    }

    Style.NetSync(sync);
    Skins.NetSync(sync);

    base.NetSyncInternal(sync);
  }

  public override void SaveData(TagCompound tag) {
    tag[ActiveKey] = Active;
    if (Active) {
      Style.AssignFromPlayer(Player);
    }

    if (Style.Save(_defaultStyle, out TagCompound? styleTag)) {
      tag[StyleKey] = styleTag;
    }
  }

  public override void LoadData(TagCompound tag) {
    if (tag.TryGet(ActiveKey, out bool active) && active && Characters.ActiveCharacter is null) {
      Enable();
    }

    if (tag.TryGet(StyleKey, out TagCompound styleTag)) {
      Style.Load(styleTag, _defaultStyle);
    }

    if (Active) {
      Style.AssignToPlayer(Player);
    }
  }
}

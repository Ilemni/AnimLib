using AnimLib.Networking;
using Terraria.ModLoader.IO;

namespace AnimLib.States;

public abstract partial class AbilityState {
  /// <summary>
  /// Save data that is specific to this <see cref="AbilityState"/>.
  /// By default, saves the ability's level.
  /// </summary>
  /// <returns>
  /// A <see cref="TagCompound"/> with data specific to this <see cref="AbilityState"/>.
  /// </returns>
  /// <seealso cref="LoadData"/>
  public override void SaveData(TagCompound tag) {
    if (Level != 0) {
      tag[nameof(Level)] = Level;
    }
  }

  /// <summary>
  /// Load data that is specific to this <see cref="AbilityState"/>.
  /// <br/> By default, loads the ability's level.
  /// <para/> The level will be clamped between 0 and <see cref="MaxLevel"/>.
  /// </summary>
  /// <param name="tag">The tag to load ability data from.</param>
  /// <seealso cref="SaveData"/>
  public override void LoadData(TagCompound tag) {
    Level = tag.TryGet(nameof(Level), out int level) ? level : 0;
  }

  /// <summary>
  /// Syncs the values of
  /// <see cref="Level"/>, <see cref="CooldownLeft"/>, and <see cref="IsOnCooldown"/>.
  /// <para/> Calls State:
  /// <para><inheritdoc cref="State.NetSyncInternal"/></para>
  /// </summary>
  /// <param name="sync"></param>
  internal override void NetSyncInternal(NetSyncer sync) {
    sync.Sync7BitEncodedInt(ref _level);
    sync.Sync7BitEncodedInt(ref _cooldownLeft);
    sync.Sync(ref _isOnCooldown);
    base.NetSyncInternal(sync);
  }

  internal void SyncLevel(NetSyncer sync) {
    sync.Sync7BitEncodedInt(ref _level);
  }
}

using System.IO;
using AnimLib.Networking;

namespace AnimLib.States;

public abstract partial class State {
  /// <summary>
  /// Whether this <see cref="State"/> needs to be synced with other clients.
  /// This value is always <see langword="false"/> on non-local <see cref="Player"/> instances,
  /// and setting to the value is ignored.
  /// </summary>
  public bool NetUpdate {
    get;
    protected internal set {
      // Ignore NetUpdate for non-local player
      if (IsLocal || Main.dedServ) {
        field = value;
      }
    }
  }

  /// <summary>
  /// Syncs <see cref="ActiveTime"/>, and calls the non-internal <see cref="NetSync"/>.
  /// </summary>
  /// <param name="sync"></param>
  /// <remarks>
  /// If overriding this method, make sure to always call the base method.
  /// </remarks>
  internal virtual void NetSyncInternal(NetSyncer sync) {
    sync.Sync7BitEncodedInt(ref _activeTime);
    sync.Sync7BitEncodedInt(ref _inactiveTime);
    NetSync(sync);
  }

  /// <summary>
  /// Sync additional data with other clients.
  /// </summary>
  /// <param name="sync">
  /// An instance of either <see cref="ReadSyncer"/> or <see cref="WriteSyncer"/>.
  /// If you need to access the underlying <see cref="BinaryReader"/> or <see cref="BinaryWriter"/>,
  /// cast as <see cref="ReadSyncer"/> or <see cref="WriteSyncer"/> to access their respective
  /// <see cref="ReadSyncer.Reader"/> or <see cref="WriteSyncer.Writer"/>.
  /// </param>
  /// <remarks>
  /// To reduce packet size, if there's anything that only needs to be synced at the start of the state,
  /// check <c>if (ActiveTime == 0)</c>,
  /// and set <see cref="NetUpdate"/> to <see langword="true"/> in <see cref="OnEnter"/>.
  /// </remarks>
  protected virtual void NetSync(NetSyncer sync) {
  }
}

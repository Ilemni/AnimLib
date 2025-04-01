using AnimLib.Networking;

namespace AnimLib.States;

public abstract partial class StateMachine {
  /// <summary>
  /// <inheritdoc cref="NetSyncActiveChild"/>
  /// <para/> Calls State:
  /// <br/><inheritdoc cref="State.NetSyncInternal"/>
  /// <br/> End State.
  /// </summary>
  /// <param name="sync"></param>
  internal override void NetSyncInternal(NetSyncer sync) {
    NetSyncActiveChild(sync);
    base.NetSyncInternal(sync);
  }

  /// <summary>
  /// Syncs the value of <see cref="ActiveChild"/>, by <see cref="State.Index"/>.
  /// </summary>
  /// <param name="sync"></param>
  private void NetSyncActiveChild(NetSyncer sync) {
    bool hasChild = ActiveChild is not null;
    sync.Sync(ref hasChild);

    switch (sync) {
      case WriteSyncer writeSync when hasChild: {
        writeSync.Writer.Write7BitEncodedInt(ActiveChild!.Index);
        return;
      }
      case ReadSyncer readSync when hasChild: {
        int id = readSync.Reader.Read7BitEncodedInt();
        State child = GetState(id);
        TrySetActiveChild(child, checkTransition: false);
        return;
      }
      case ReadSyncer:
        ClearActiveChild(silent: false);
        return;
    }
  }
}

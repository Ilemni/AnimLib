using System.Linq;
using AnimLib.States;

namespace AnimLib.Networking;

/// <summary>
/// Sends and receives <see cref="ModPacket"/>s that sync <see cref="AnimCharacterCollection"/> and all children.
/// </summary>
internal sealed class StatePacketHandler(byte handlerType) : PacketHandler(handlerType) {
  protected override void HandlePacket(ReadSyncer reader, int fromWho) {
    var states = GetStates(fromWho);
    int count = reader.Reader.Read7BitEncodedInt();

    for (int i = 0; i < count; i++) {
      int index = reader.Reader.Read7BitEncodedInt();
      State state = states[index];
      state.NetSyncInternal(reader);
      state.NetUpdate = Main.dedServ; // Used when server broadcasts this change to all other clients
    }

    if (Main.dedServ) {
      // Packet was sent by a client to the server,
      // Server sends same type of packet back
      SendPacket(fromWho: fromWho);
    }
  }

  protected override void OnSendPacket(WriteSyncer writer, int fromWho) {
    var states = GetStates(fromWho);

    int count = states.Count(s => s.NetUpdate);
    writer.Writer.Write7BitEncodedInt(count);

    foreach (State state in states) {
      if (!state.NetUpdate) {
        continue;
      }

      writer.Writer.Write7BitEncodedInt(state.Index);
      state.NetSyncInternal(writer);
      state.NetUpdate = false;
    }
  }
}

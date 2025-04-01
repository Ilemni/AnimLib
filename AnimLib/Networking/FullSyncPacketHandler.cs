using System.IO;
using AnimLib.States;

namespace AnimLib.Networking;

internal sealed class FullSyncPacketHandler(byte handlerType) : PacketHandler(handlerType) {
  protected override void HandlePacket(ReadSyncer readSyncer, int fromWho) {
    var states = GetStates(fromWho);
    BinaryReader reader = readSyncer.Reader;

    int count = reader.Read7BitEncodedInt();
    for (int i = 0; i < count; i++) {
      int index = reader.Read7BitEncodedInt();
      State state = states[index];
      state.NetSyncInternal(readSyncer);
    }

    if (Main.dedServ) {
      // Packet was sent by a client to the server,
      // Server sends same type of packet back
      SendPacket(fromWho: fromWho);
    }
  }

  protected override void OnSendPacket(WriteSyncer writer, int fromWho) {
    var states = GetStates(fromWho);
    int count = states.Length;

    writer.Writer.Write7BitEncodedInt(count);

    foreach (State state in states) {
      writer.Writer.Write7BitEncodedInt(state.Index);
      state.NetSyncInternal(writer);
    }
  }
}

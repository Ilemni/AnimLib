using System.IO;
using JetBrains.Annotations;

namespace AnimLib.Networking;

/// <summary>
/// Receives all <see cref="ModPacket"/>s and distributes them to the desired <see cref="PacketHandler"/>.
/// </summary>
[UsedImplicitly]
internal sealed class ModNetHandler : ModSystem {
  /// <summary> Identifier for <see cref="StatePacketHandler"/>. </summary>
  private const byte SyncStates = 1;

  /// <summary> Identifier for <see cref="FullSyncPacketHandler"/> </summary>
  private const byte FullSyncStates = 2;

  /// <inheritdoc cref="StatePacketHandler"/>
  internal readonly StatePacketHandler StatePacketHandler = new(SyncStates);

  /// <inheritdoc cref="FullSyncPacketHandler"/>
  internal readonly FullSyncPacketHandler FullSyncHandler = new(FullSyncStates);

  /// <summary>
  /// Sends the received <see cref="ModPacket"/> to the desired <see cref="PacketHandler"/> based on data read from <paramref name="reader"/>.
  /// </summary>
  /// <param name="reader">The <see cref="BinaryReader"/> that reads the received <see cref="ModPacket"/>.</param>
  /// <param name="fromWho">The player that this packet is from.</param>
  internal void HandlePacket(BinaryReader reader, int fromWho) {
    byte packetClass = reader.ReadByte();
    PacketHandler? handler = packetClass switch {
      SyncStates => StatePacketHandler,
      FullSyncStates => FullSyncHandler,
      _ => null
    };

    if (handler is null) {
      Log.Warn($"Unknown Packet {packetClass}");
      return;
    }


    handler.HandlePacket(reader, fromWho);
  }
}

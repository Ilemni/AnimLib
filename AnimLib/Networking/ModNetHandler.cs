using System.IO;
using Terraria.ModLoader;

namespace AnimLib.Networking {
  /// <summary>
  /// Receives all <see cref="ModPacket"/>s and distributes them to the desired <see cref="PacketHandler"/>.
  /// </summary>
  internal class ModNetHandler : SingleInstance<ModNetHandler> {
    /// <summary>
    /// Type for <see cref="AbilityPacketHandler"/>.
    /// </summary>
    private const byte AbilityState = 1;

    /// <inheritdoc cref="AbilityPacketHandler"/>
    internal readonly AbilityPacketHandler abilityPacketHandler = new(AbilityState);

    private ModNetHandler() { }

    /// <summary>
    /// Sends the received <see cref="ModPacket"/> to the desired <see cref="PacketHandler"/> based on data read from <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> that reads the received <see cref="ModPacket"/>.</param>
    /// <param name="fromWho">The player that this packet is from.</param>
    internal void HandlePacket(BinaryReader reader, int fromWho) {
      byte packetClass = reader.ReadByte();
      switch (packetClass) {
        case AbilityState:
          abilityPacketHandler.HandlePacket(reader, fromWho);
          break;
        default:
          Log.Warn($"Unknown Packet {packetClass}");
          break;
      }
    }
  }
}

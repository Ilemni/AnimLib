using System.IO;
using Terraria.ModLoader;

namespace AnimLib {
  internal static class ModPacketExtension {
    /// <summary>
    /// Writes <paramref name="value"/> to this <paramref name="packet"/> with the smallest cast possible,
    /// where <paramref name="upperBounds"/> is the net-synced upper limit of <paramref name="value"/>.
    /// </summary>
    /// <param name="packet">Packet to write to.</param>
    /// <param name="value">Value to write.</param>
    /// <param name="upperBounds">Maximum possible value of <paramref name="value"/>.</param>
    /// <seealso cref="ReadLowestCast"/>
    internal static void WriteLowestCast(this ModPacket packet, int value, int upperBounds) {
      if (upperBounds <= byte.MaxValue)
        packet.Write((byte)value);
      else if (upperBounds <= ushort.MaxValue)
        packet.Write((ushort)value);
      else
        packet.Write(value);
    }

    /// <summary>
    /// Reads a value with the smallest size possible, and returns it. The size is dependent on
    /// <paramref name="upperBounds"/>, the net-synced upper limit of the return value.
    /// </summary>
    /// <param name="reader">Reader to read from.</param>
    /// <param name="upperBounds">Maximum possible value of the return value.</param>
    /// <seealso cref="WriteLowestCast"/>
    internal static int ReadLowestCast(this BinaryReader reader, int upperBounds) =>
      upperBounds <= byte.MaxValue ? reader.ReadByte() :
      upperBounds <= ushort.MaxValue ? reader.ReadInt16() :
      reader.ReadInt32();
  }
}

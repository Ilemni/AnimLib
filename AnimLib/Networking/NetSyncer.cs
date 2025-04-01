using System.IO;
using AnimLib.States;
using JetBrains.Annotations;

namespace AnimLib.Networking;

/// <summary>
/// Used when a <see cref="State.NetSync"/> method is currently receiving a <see cref="ModPacket" />,
/// and should make changes to the client.
/// </summary>
public sealed class ReadSyncer : NetSyncer {
  internal void SetReader(BinaryReader reader) => Reader = reader;
  internal void ClearReader() => Reader = null!;

  // For the purposes of reading, this is not null. Outside of reading, this will be null.
  public BinaryReader Reader { get; private set; } = null!;

  public override bool Reading => true;
  public override void Sync(ref bool value) => value = Reader.ReadBoolean();

  public override void Sync(ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5, ref bool b6, ref bool b7,
    ref bool b8) {
    BitsByte bb = Reader.ReadByte();
    bb.Retrieve(ref b1, ref b2, ref b3, ref b4, ref b5, ref b6, ref b7, ref b8);
  }

  public override void Sync(ref byte value) => value = Reader.ReadByte();

  public override void Sync(ref sbyte value) => value = Reader.ReadSByte();

  public override void Sync(ref byte[] buffer) {
    byte len = Reader.ReadByte();
    if (len > buffer.Length) {
      buffer = Reader.ReadBytes(len);
      return;
    }

    for (int i = 0; i < len; i++) {
      buffer[i] = Reader.ReadByte();
    }

    if (len == buffer.Length) {
      return;
    }

    for (int i = len; i < buffer.Length; i++) {
      buffer[i] = 0;
    }
  }

  public override void Sync(ref char ch) => ch = Reader.ReadChar();

  public override void Sync(ref char[] buffer) {
    byte len = Reader.ReadByte();
    if (len > buffer.Length) {
      buffer = Reader.ReadChars(len);
      return;
    }

    for (int i = 0; i < len; i++) {
      buffer[i] = Reader.ReadChar();
    }

    if (len == buffer.Length) {
      return;
    }

    for (int i = len; i < buffer.Length; i++) {
      buffer[i] = (char)0;
    }
  }

  public override void Sync(ref short value) => value = Reader.ReadInt16();
  public override void Sync(ref ushort value) => value = Reader.ReadUInt16();

  public override void Sync(ref int value) => value = Reader.ReadInt32();

  public override void Sync(ref uint value) => value = Reader.ReadUInt32();

  public override void Sync(ref long value) => value = Reader.ReadInt64();

  public override void Sync(ref ulong value) => value = Reader.ReadUInt64();

  public override void Sync(ref double value) => value = Reader.ReadDouble();

  public override void Sync(ref decimal value) => value = Reader.ReadDecimal();

  public override void Sync(ref float value) => value = Reader.ReadSingle();

  public override void Sync(ref Half value) => value = Reader.ReadHalf();

  public override void Sync(ref string value) => value = Reader.ReadString();
  public override void Sync(Func<bool> write, Action<bool> read) => read(Reader.ReadBoolean());

  public override void Sync(Func<byte> write, Action<byte> read) => read(Reader.ReadByte());

  public override void Sync(Func<sbyte> write, Action<sbyte> read) => read(Reader.ReadSByte());

  public override void Sync(Func<char> write, Action<char> read) => read(Reader.ReadChar());

  public override void Sync(Func<short> write, Action<short> read) => read(Reader.ReadInt16());

  public override void Sync(Func<ushort> write, Action<ushort> read) => read(Reader.ReadUInt16());
  public override void Sync(Func<int> write, Action<int> read) => read(Reader.ReadInt32());

  public override void Sync(Func<uint> write, Action<uint> read) => read(Reader.ReadUInt32());

  public override void Sync(Func<long> write, Action<long> read) => read(Reader.ReadInt64());

  public override void Sync(Func<ulong> write, Action<ulong> read) => read(Reader.ReadUInt64());

  public override void Sync(Func<double> write, Action<double> read) => read(Reader.ReadDouble());

  public override void Sync(Func<decimal> write, Action<decimal> read) => read(Reader.ReadDecimal());

  public override void Sync(Func<float> write, Action<float> read) => read(Reader.ReadSingle());

  public override void Sync(Func<Half> write, Action<Half> read) => read(Reader.ReadHalf());

  public override void Sync(Func<string> write, Action<string> read) => read(Reader.ReadString());

  public override void Sync(ref Vector2 value) => value = Reader.ReadVector2();

  public override void Sync(ref Color value) => value = Reader.ReadRGB();

  public override void SyncSign(ref int value) => value = Reader.ReadSByte();

  public override void Sync7BitEncodedInt(ref int value) => value = Reader.Read7BitEncodedInt();

  public override void Sync7BitEncodedInt64(ref long value) => value = Reader.Read7BitEncodedInt64();

  public override void SyncFunc<TOwner>(TOwner owner, Action<TOwner, BinaryWriter> writeFunc,
    Action<TOwner, BinaryReader> readFunc) where TOwner : class => readFunc(owner, Reader);

  public override void SyncPositionAndVelocity(Entity entity) {
    entity.position = Reader.ReadVector2();
    entity.velocity = Reader.ReadVector2();
  }

  public override void SyncNullable<T>(ref T? value, Action<BinaryWriter, T> onWriteNotNull,
    Func<BinaryReader, T> onReadNotNull,
    Action? onReadNull = null) where T : default {
    bool isNotNull = Reader.ReadBoolean();
    if (isNotNull) {
      value = onReadNotNull(Reader);
    }
    else {
      value = default;
      onReadNull?.Invoke();
    }
  }

  public override void SyncNullable<TOwner, TValue>(TOwner owner, ref TValue? value,
    Action<BinaryWriter, TValue> onWriteNotNull,
    Action<BinaryReader, TOwner> onReadNotNull,
    Action<TOwner>? onReadNull = null) where TOwner : class where TValue : default {
    bool isNotNull = Reader.ReadBoolean();
    if (isNotNull) {
      onReadNotNull(Reader, owner);
    }
    else {
      value = default;
      onReadNull?.Invoke(owner);
    }
  }

  public override void SyncEntity(ref Entity? entity) {
    byte type = Reader.ReadByte();
    entity = type switch {
      Null => null,
      PlayerType => Main.player[Reader.Read7BitEncodedInt()],
      NpcType => Main.npc[Reader.Read7BitEncodedInt()],
      ProjectileType => FindProjectile((ushort)Reader.Read7BitEncodedInt()),
      ItemType => Main.item[Reader.Read7BitEncodedInt()],
      _ => null
    };
    return;

    Projectile? FindProjectile(ushort identity) {
      foreach (Projectile p in Main.projectile) {
        if (p.identity == identity)
          return p;
      }

      return null;
    }
  }

  public override IEnumerable<TValue> SyncEnumerate<TOwner, TValue>(TOwner owner,
    Func<TOwner, int> onWriteCount,
    Func<TOwner, IEnumerable<TValue>> onWriteIterator,
    Action<BinaryWriter, TValue> writeFunc,
    Func<TOwner, BinaryReader, TValue> readFunc) where TOwner : class {
    int loopCount = Reader.Read7BitEncodedInt();
    for (int i = 0; i < loopCount; i++) {
      yield return readFunc(owner, Reader);
    }
  }
}

/// <summary>
/// Used when a <see cref="State.NetSync"/> method is currently writing to a <see cref="ModPacket"/>.
/// </summary>
public sealed class WriteSyncer : NetSyncer {
  internal void SetWriter(BinaryWriter writer) => Writer = writer;

  internal void ClearWriter() => Writer = null!;

  // For the purposes of writing, this is not null. Outside of writing, this will be null.
  public BinaryWriter Writer { get; private set; } = null!;

  public override bool Reading => false;
  public override void Sync(ref bool value) => Writer.Write(value);

  public override void Sync(ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5, ref bool b6, ref bool b7,
    ref bool b8) {
    BitsByte bb = new(b1, b2, b3, b4, b5, b6, b7, b8);
    Writer.Write(bb);
  }

  public override void Sync(ref byte value) => Writer.Write(value);
  public override void Sync(ref sbyte value) => Writer.Write(value);

  public override void Sync(ref byte[] buffer) =>
    Writer.Write(buffer, 0, (ushort)(buffer.Length < ushort.MaxValue ? buffer.Length : ushort.MaxValue));

  public override void Sync(ref char ch) => Writer.Write(ch);

  public override void Sync(ref char[] buffer) =>
    Writer.Write(buffer, 0, (ushort)(buffer.Length < ushort.MaxValue ? buffer.Length : ushort.MaxValue));

  public override void Sync(ref short value) => Writer.Write(value);
  public override void Sync(ref ushort value) => Writer.Write(value);
  public override void Sync(ref int value) => Writer.Write(value);
  public override void Sync(ref uint value) => Writer.Write(value);
  public override void Sync(ref long value) => Writer.Write(value);
  public override void Sync(ref ulong value) => Writer.Write(value);
  public override void Sync(ref double value) => Writer.Write(value);
  public override void Sync(ref decimal value) => Writer.Write(value);
  public override void Sync(ref float value) => Writer.Write(value);
  public override void Sync(ref Half value) => Writer.Write(value);
  public override void Sync(ref string value) => Writer.Write(value);
  public override void Sync(Func<bool> write, Action<bool> read) => Writer.Write(write());
  public override void Sync(Func<byte> write, Action<byte> read) => Writer.Write(write());
  public override void Sync(Func<sbyte> write, Action<sbyte> read) => Writer.Write(write());
  public override void Sync(Func<char> write, Action<char> read) => Writer.Write(write());
  public override void Sync(Func<short> write, Action<short> read) => Writer.Write(write());
  public override void Sync(Func<ushort> write, Action<ushort> read) => Writer.Write(write());
  public override void Sync(Func<int> write, Action<int> read) => Writer.Write(write());
  public override void Sync(Func<uint> write, Action<uint> read) => Writer.Write(write());
  public override void Sync(Func<long> write, Action<long> read) => Writer.Write(write());
  public override void Sync(Func<ulong> write, Action<ulong> read) => Writer.Write(write());
  public override void Sync(Func<double> write, Action<double> read) => Writer.Write(write());
  public override void Sync(Func<decimal> write, Action<decimal> read) => Writer.Write(write());
  public override void Sync(Func<float> write, Action<float> read) => Writer.Write(write());
  public override void Sync(Func<Half> write, Action<Half> read) => Writer.Write(write());
  public override void Sync(Func<string> write, Action<string> read) => Writer.Write(write());

  public override void Sync(ref Vector2 value) => Writer.WriteVector2(value);
  public override void Sync(ref Color value) => Writer.WriteRGB(value);

  public override void SyncSign(ref int value) => Writer.Write((sbyte)(value > 0 ? 1 : value < 0 ? -1 : 0));

  public override void Sync7BitEncodedInt(ref int value) => Writer.Write7BitEncodedInt(value);

  public override void Sync7BitEncodedInt64(ref long value) => Writer.Write7BitEncodedInt64(value);

  public override void SyncFunc<TOwner>(TOwner owner,
    Action<TOwner, BinaryWriter> writeFunc,
    Action<TOwner, BinaryReader> readFunc) where TOwner : class => writeFunc(owner, Writer);

  public override void SyncPositionAndVelocity(Entity entity) {
    Writer.WriteVector2(entity.position);
    Writer.WriteVector2(entity.velocity);
  }

  public override void SyncNullable<T>(ref T? value, Action<BinaryWriter, T> onWriteNotNull,
    Func<BinaryReader, T> onReadNotNull,
    Action? onReadNull = null) where T : default {
    Writer.Write(value is not null);
    if (value is not null) {
      onWriteNotNull(Writer, (T)value);
    }
  }

  public override void SyncNullable<TOwner, TValue>(TOwner owner, ref TValue? value,
    Action<BinaryWriter, TValue> onWriteNotNull,
    Action<BinaryReader, TOwner> onReadNotNull,
    Action<TOwner>? onReadNull = null) where TOwner : class where TValue : default {
    Writer.Write(value is not null);
    if (value is not null) {
      onWriteNotNull(Writer, value);
    }
  }

  public override void SyncEntity(ref Entity? entity) {
    switch (entity) {
      case null:
        Writer.Write(Null);
        break;
      case Player:
        Writer.Write(PlayerType);
        Writer.Write7BitEncodedInt(entity.whoAmI);
        break;
      case NPC:
        Writer.Write(NpcType);
        Writer.Write7BitEncodedInt(entity.whoAmI);
        break;
      case Item:
        Writer.Write(ItemType);
        Writer.Write7BitEncodedInt(entity.whoAmI);
        break;
      case Projectile projectile:
        Writer.Write(ProjectileType);
        Writer.Write7BitEncodedInt(projectile.identity);
        break;
    }
  }

  public override IEnumerable<TValue> SyncEnumerate<TOwner, TValue>(TOwner owner,
    Func<TOwner, int> onWriteCount,
    Func<TOwner, IEnumerable<TValue>> onWriteIterator,
    Action<BinaryWriter, TValue> writeFunc,
    Func<TOwner, BinaryReader, TValue> readFunc) where TOwner : class {
    Writer.Write7BitEncodedInt(onWriteCount(owner));
    foreach (TValue t in onWriteIterator(owner)) {
      writeFunc(Writer, t);
      yield return t;
    }
  }
}

/// <summary>
/// Base class, with implementations <see cref="ReadSyncer"/> and <see cref="WriteSyncer"/>.
/// This enables reading and writing functionality to occur in the same method.
/// </summary>
[PublicAPI]
public abstract class NetSyncer {
  private protected const byte Null = 0;
  private protected const byte PlayerType = 1;
  private protected const byte NpcType = 2;
  private protected const byte ItemType = 3;
  private protected const byte ProjectileType = 4;
  private static bool _null;

  /// <summary>
  /// Whether the sync method is currently receiving a <see cref="!:ModPacket" />,
  /// and should make changes to the client.
  /// </summary>
  public abstract bool Reading { get; }

  /// <summary>
  /// Whether the sync method is currently writing to a <see cref="ModPacket"/>.
  /// </summary>
  public bool Writing => !Reading;

  public abstract void Sync(ref bool value);

  public void Sync(ref bool b1, ref bool b2) =>
    Sync(ref b1, ref b2, ref _null, ref _null, ref _null, ref _null, ref _null, ref _null);

  public void Sync(ref bool b1, ref bool b2, ref bool b3) =>
    Sync(ref b1, ref b2, ref b3, ref _null, ref _null, ref _null, ref _null, ref _null);

  public void Sync(ref bool b1, ref bool b2, ref bool b3, ref bool b4) =>
    Sync(ref b1, ref b2, ref b3, ref b4, ref _null, ref _null, ref _null, ref _null);

  public void Sync(ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5) =>
    Sync(ref b1, ref b2, ref b3, ref b4, ref b5, ref _null, ref _null, ref _null);

  public void Sync(ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5, ref bool b6) =>
    Sync(ref b1, ref b2, ref b3, ref b4, ref b5, ref b6, ref _null, ref _null);

  public void Sync(ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5, ref bool b6, ref bool b7) =>
    Sync(ref b1, ref b2, ref b3, ref b4, ref b5, ref b6, ref b7, ref _null);

  public abstract void Sync(ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5, ref bool b6, ref bool b7,
    ref bool b8);

  public abstract void Sync(ref byte value);
  public abstract void Sync(ref sbyte value);
  public abstract void Sync(ref byte[] buffer);
  public abstract void Sync(ref char ch);
  public abstract void Sync(ref char[] buffer);
  public abstract void Sync(ref short value);
  public abstract void Sync(ref ushort value);
  public abstract void Sync(ref int value);
  public abstract void Sync(ref uint value);
  public abstract void Sync(ref long value);
  public abstract void Sync(ref ulong value);
  public abstract void Sync(ref double value);
  public abstract void Sync(ref decimal value);
  public abstract void Sync(ref float value);
  public abstract void Sync(ref Half value);
  public abstract void Sync(ref string value);
  public abstract void Sync(Func<bool> write, Action<bool> read);
  public abstract void Sync(Func<byte> write, Action<byte> read);
  public abstract void Sync(Func<sbyte> write, Action<sbyte> read);
  public abstract void Sync(Func<char> write, Action<char> read);
  public abstract void Sync(Func<short> write, Action<short> read);
  public abstract void Sync(Func<ushort> write, Action<ushort> read);
  public abstract void Sync(Func<int> write, Action<int> read);
  public abstract void Sync(Func<uint> write, Action<uint> read);
  public abstract void Sync(Func<long> write, Action<long> read);
  public abstract void Sync(Func<ulong> write, Action<ulong> read);
  public abstract void Sync(Func<double> write, Action<double> read);
  public abstract void Sync(Func<decimal> write, Action<decimal> read);
  public abstract void Sync(Func<float> write, Action<float> read);
  public abstract void Sync(Func<Half> write, Action<Half> read);
  public abstract void Sync(Func<string> write, Action<string> read);
  public abstract void Sync(ref Vector2 value);
  public abstract void Sync(ref Color value);

  /// <summary>
  /// Sync the specified value as a byte representing the sign of the value.
  /// </summary>
  /// <param name="value"></param>
  public abstract void SyncSign(ref int value);

  public abstract void Sync7BitEncodedInt(ref int value);
  public abstract void Sync7BitEncodedInt64(ref long value);

  /// <summary>
  /// Sync arbitrary data
  /// </summary>
  /// <param name="owner">
  /// The owner of the data, used as argument for <paramref name="readFunc" />/<paramref name="writeFunc" />
  /// </param>
  /// <param name="writeFunc">Function called when writing. Ignored when reading.</param>
  /// <param name="readFunc">Function called when reading. Ignored when writing.</param>
  /// <typeparam name="TOwner">The type of object which owns the data.</typeparam>
  public abstract void SyncFunc<TOwner>(TOwner owner, Action<TOwner, BinaryWriter> writeFunc,
    Action<TOwner, BinaryReader> readFunc) where TOwner : class;

  public abstract void SyncPositionAndVelocity(Entity entity);

  /// <summary>
  /// Sync a value which is expected to be null.
  /// </summary>
  /// <param name="value">Value to check for null.</param>
  /// <param name="onWriteNotNull">Function to write if outgoing value is not null.</param>
  /// <param name="onReadNotNull">Function to read if incoming value is not null.</param>
  /// <param name="onReadNull">Function to read if incoming value is null.</param>
  /// <typeparam name="T"></typeparam>
  public abstract void SyncNullable<T>(ref T? value, Action<BinaryWriter, T> onWriteNotNull,
    Func<BinaryReader, T> onReadNotNull,
    Action? onReadNull = null);

  /// <summary>
  /// Sync a value which is expected to be null some of the time.
  /// This overload does not assign the result of <paramref name="onReadNotNull" /> to <paramref name="value" />.
  /// </summary>
  /// <param name="owner">To be called</param>
  /// <param name="value">Value to check for null.</param>
  /// <param name="onWriteNotNull">Function to write if outgoing value is not null.</param>
  /// <param name="onReadNotNull">Function to read if incoming value is not null.</param>
  /// <param name="onReadNull">Function to read if incoming value is null.</param>
  /// <typeparam name="TOwner">Should always be "<see langword="this" />"</typeparam>
  /// <typeparam name="TValue">Value to be synced</typeparam>
  /// <remarks>
  /// <typeparamref name="TOwner" />/<paramref name="owner" /> exists to avoid closures.
  /// </remarks>
  public abstract void SyncNullable<TOwner, TValue>(TOwner owner, ref TValue? value,
    Action<BinaryWriter, TValue> onWriteNotNull,
    Action<BinaryReader, TOwner> onReadNotNull, Action<TOwner>? onReadNull = null) where TOwner : class;

  /// <summary>
  /// Syncs the reference to the specified <see cref="!:Entity" />.
  /// </summary>
  /// <param name="entity">
  /// A <see cref="!:Player" />, <see cref="!:NPC" />, <see cref="!:Projectile" />, <see cref="!:Item" />, or <see langword="null" />.
  /// </param>
  public abstract void SyncEntity(ref Entity? entity);

  /// <summary>
  /// Syncs an enumeration of values.
  /// </summary>
  /// <param name="owner">
  /// The object which owns the enumeration.
  /// This is used as an argument for <paramref name="onWriteCount" />, <paramref name="onWriteIterator" />, and <paramref name="readFunc" />.
  /// </param>
  /// <param name="onWriteCount">
  /// Function to get number of times to enumerate for.
  /// Used only when writing, ignored when reading.
  /// </param>
  /// <param name="onWriteIterator">
  /// <see cref="!:IEnumerable{T}" /> to loop through for when writing.
  /// Used only when writing, ignored when reading.
  /// </param>
  /// <param name="writeFunc">
  /// Function to write for identifying the IEnumerable variable.
  /// Used only when writing, ignored when reading.
  /// </param>
  /// <param name="readFunc">
  /// Function to read to get the enumerated variable of type <typeparamref name="TValue" />.
  /// Used only when reading, ignored while writing.
  /// </param>
  /// <typeparam name="TOwner">The type of object which owns the enumeration.</typeparam>
  /// <typeparam name="TValue">The type of enumerated value.</typeparam>
  /// <returns></returns>
  /// <remarks>
  /// When writing, this method writes the return value of <paramref name="onWriteCount" />.
  /// It will iterate over <paramref name="onWriteIterator" />,
  /// calling <paramref name="writeFunc" /> on each of them.
  /// <para/> When reading, this method will read the count,
  /// and in a for loop, call <paramref name="readFunc" />.
  /// <typeparamref name="TOwner" />/<paramref name="owner" /> exists to avoid closures.
  /// </remarks>
  public abstract IEnumerable<TValue> SyncEnumerate<TOwner, TValue>(TOwner owner, Func<TOwner, int> onWriteCount,
    Func<TOwner, IEnumerable<TValue>> onWriteIterator, Action<BinaryWriter, TValue> writeFunc,
    Func<TOwner, BinaryReader, TValue> readFunc) where TOwner : class;
}

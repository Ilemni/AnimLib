using System.IO;
using System.Linq;
using AnimLib.Abilities;
using AnimLib.Extensions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AnimLib.Networking {
  /// <summary>
  /// Sends and receives <see cref="ModPacket"/>s that handle the <see cref="Abilities.AbilityManager"/> state.
  /// </summary>
  internal class AbilityPacketHandler : PacketHandler {
    internal AbilityPacketHandler(byte handlerType) : base(handlerType) { }

    // AnimLib Ability Packet structure:
    //
    // 1 or 2 bytes Mod_Count
    // foreach Mod_Count:
    //   X bytes Mod_Name
    //   1 or 2 bytes Ability_Count
    //   foreach Ability_Count:
    //     1 or 2 bytes Ability_ID
    //     [ Remaining handled by Ability[AbilityID].Read()/.Write() ]
    internal override void HandlePacket(BinaryReader reader, int fromWho) {
      AnimPlayer fromPlayer = Main.player[fromWho].GetModPlayer<AnimPlayer>();
      int modCount = reader.ReadLowestCast(ModNet.NetModCount);
      for (int i = 0; i < modCount; i++) {
        Mod mod = ModLoader.GetMod(reader.ReadString());
        AbilityManager manager = fromPlayer.characters[mod].abilityManager;
        if (manager is null) continue;
        int abilityCount = reader.ReadLowestCast(manager.abilityArray.Length);
        for (int j = 0; j < abilityCount; j++) {
          int abilityId = reader.ReadLowestCast(manager.abilityArray.Length);
          Ability ability = manager[abilityId];
          ability.PreReadPacket(reader);
          if(Main.netMode == NetmodeID.Server) ability.netUpdate = true;
        }
      }

      if (Main.netMode == NetmodeID.Server) {
        SendPacket(-1, fromWho);
        fromPlayer.abilityNetUpdate = false;
      }
    }

    internal void SendPacket(int toWho, int fromWho) {
      ModPacket packet = GetPacket(fromWho);
      AnimPlayer fromPlayer = Main.player[fromWho].GetModPlayer<AnimPlayer>();

      var modsToUpdate = (from pair in fromPlayer.characters.dict
        where pair.Value.abilityManager?.netUpdate ?? false
        select pair).ToList();

      packet.WriteLowestCast(modsToUpdate.Count, ModNet.NetModCount);

      foreach ((Mod mod, AnimCharacter character) in modsToUpdate) {
        packet.Write(mod.Name);
        var abilities = character.abilityManager?.abilityArray;
        var abilitiesToUpdate = (from a in abilities
          where a.netUpdate
          select a).ToList();

        if (abilities is null) {
          packet.Write((byte)0);
          continue;
        }

        packet.WriteLowestCast(abilitiesToUpdate.Count, abilities.Length);
        foreach (Ability ability in abilitiesToUpdate) {
          packet.WriteLowestCast(ability.Id, abilities.Length);
          ability.PreWritePacket(packet);
        }
      }

      packet.Send(toWho, fromWho);
    }
  }
}

using AnimLib.Skins;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;

namespace AnimLib.States;

internal static class StateIO {
  private const string ModName = "mod";
  private const string StateName = "state";
  private const string CharacterName = "character";
  private const string Error = "error";
  private const string Data = "data";
  private const string SlotName = "slot";
  private const string SkinName = "skin";
  private const string SlotMod = "slotMod";
  private const string SkinMod = "skinMod";
  private const string SkinList = "skins";

  internal static List<TagCompound> SaveStateData(Player player) {
    List<TagCompound> list = [];
    TagCompound stateData = [];

    foreach (State state in player.GetStates()) {
      try {
        state.SaveData(stateData);
      }
      catch (Exception e) {
        Log.Error($"Failed to save state {state.Name} from mod {state.Mod.Name}.", e);
        list.Add(new TagCompound {
          [ModName] = state.Mod.Name,
          [StateName] = state.Name,
          [Error] = e.ToString()
        });
        stateData = [];
        continue;
      }

      if (stateData.Count == 0) {
        continue;
      }

      list.Add(new TagCompound {
        [ModName] = state.Mod.Name,
        [StateName] = state.Name,
        [Data] = stateData
      });
      stateData = [];
    }

    return list;
  }

  internal static void LoadStateData(Player player, IList<TagCompound> list) {
    UnloadedStatesPlayer unloadedPlayer = player.GetModPlayer<UnloadedStatesPlayer>();

    foreach (TagCompound tag in list) {
      string modName = tag.GetString(ModName);
      string stateName = tag.GetString(StateName);
      if (tag.TryGet(Error, out string error)) {
        Log.Error($"Failed to save state {stateName} from mod {modName}.", new Exception(error));
        continue;
      }

      if (!ModContent.TryFind(modName, stateName, out State templateState)) {
        unloadedPlayer.AddUnloadedState(tag);
        continue;
      }

      State state = player.GetState(templateState);

      try {
        state.LoadData(tag.GetCompound(Data));
      }
      catch (Exception e) {
        throw new CustomModDataException(state.Mod, $"Error in reading state {stateName} from mod {modName}.", e);
      }
    }
  }

  internal static List<TagCompound> SaveSkinData(Player player) {
    List<TagCompound> charList = [];
    List<TagCompound> skinList = [];

    foreach (AnimCharacter character in player.GetState<AnimCharacterCollection>().Characters) {
      foreach (EquippedSkinSlot equippedSlot in character.Skins.Slots) {
        SkinSlot slot = equippedSlot.Slot;
        Skin skin = equippedSlot.Skin;
        if (ReferenceEquals(skin, slot.DefaultSkin)) {
          continue;
        }

        TagCompound skinData = new() {
          [SlotName] = slot.Name,
          [SkinName] = skin.Name
        };

        // I don't see a use case for a *slot* being from a different mod, but let's support it anyway
        if (slot.Mod != character.Mod) {
          skinData[SlotMod] = slot.Mod.Name;
        }

        if (skin.Mod != character.Mod) {
          skinData[SkinMod] = skin.Mod.Name;
        }

        skinList.Add(skinData);
      }

      if (skinList.Count <= 0) {
        continue;
      }

      charList.Add(new TagCompound {
        [ModName] = character.Mod.Name,
        [CharacterName] = character.Name,
        [SkinList] = skinList
      });
      skinList = [];
    }

    return charList;
  }

  internal static void LoadSkinData(Player player, IList<TagCompound> list) {
    UnloadedStatesPlayer unloadedPlayer = player.GetModPlayer<UnloadedStatesPlayer>();

    foreach (TagCompound tag in list) {
      string characterName = tag.GetString(CharacterName);
      string modName = tag.GetString(ModName);
      var skinTags = tag.GetList<TagCompound>(SkinList);
      if (tag.TryGet(Error, out string error)) {
        Log.Error($"Failed to save skin data for character {characterName} from mod {modName}.", new Exception(error));
        continue;
      }

      ModContent.TryFind(modName, characterName, out AnimCharacter? character);
      character = character is not null ? player.GetState<AnimCharacter>(character.Index) : null;

      foreach (TagCompound skinTag in skinTags) {
        string slotName = skinTag.GetString(SlotName);
        string skinName = skinTag.GetString(SkinName);
        skinTag.TryGet(SlotMod, out string? slotModName);
        skinTag.TryGet(SkinMod, out string? skinModName);

        Mod? slotMod = character is not null ? TryGetTagMod(slotModName, character.Mod) : null;
        Mod? skinMod = character is not null ? TryGetTagMod(skinModName, character.Mod) : null;
        if (character is null ||
            slotMod is null ||
            skinMod is null ||
            !slotMod.TryFind(slotName, out SkinSlot slot) ||
            !skinMod.TryFind(skinName, out Skin skin)) {
          unloadedPlayer.AddUnloadedSkin(modName, characterName, skinTag);
          continue;
        }

        try {
          character.Skins.SetSkin(slot, skin);
        }
        catch (Exception e) {
          Log.Error($"Failed to load skin for character {characterName} from mod {modName}.", e);
        }
      }
    }

    return;

    // Unspecified mod -> skin belongs to same mod as character
    // Specified mod present -> mod
    // Specified mod missing -> null
    Mod? TryGetTagMod(string? modName, Mod defaultMod) {
      return modName switch {
        null => defaultMod,
        not null when ModLoader.TryGetMod(modName, out Mod mod) => mod,
        not null => null
      };
    }
  }
}

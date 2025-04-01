using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;

namespace AnimLib.States;

internal static class StateIO {
  private const string ModName = "mod";
  private const string StateName = "state";
  private const string Error = "error";
  private const string Data = "data";

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
        unloadedPlayer.UnloadedStates.Add(tag);
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
}

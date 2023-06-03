using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnimLib.Abilities;
using AnimLib.Internal;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace AnimLib.Commands {
  [UsedImplicitly]
  internal class AnimAbilityCommand : ModCommand {
    public override string Command => "animability";

    public override string Usage => "/animability <mod> [ability] [level]";
    public override string Description => "Get a list of abilities in a mod, or get or set an ability's level.";

    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
      var (message, color) = Action(caller, args);
      caller.Reply(message, color);
    }

    [NotNull]
    private (string message, Color color) Action(CommandCaller caller, IReadOnlyList<string> args) {
      int idx = 0;
      string arg = string.Empty;

      bool NextArg() {
        if (args.Count <= idx) return false;
        arg = args[idx++];
        return true;
      }

      AnimPlayer player = caller.Player.GetModPlayer<AnimPlayer>();
      if (!player.DebugEnabled) return Error("This command cannot be used outside of debug mode.");
      if (!AnimLoader.GetLoadedMods(out var loadedMods)) return Warn($"No mods are using {nameof(AnimLib)}.");
      
      // "/animability" - List all mods that have abilities
      if (!NextArg()) {
        StringBuilder sb = new();
        sb.AppendLine("Available Mods with Abilities:");
        foreach (Mod mod in loadedMods.Where(mod => player.characters[mod].abilityManager is not null)) {
          sb.AppendLine($"  {mod.Name} ({mod.DisplayName})");
        }

        return SuccessWarn(sb.ToString());
      }

      // "/animability [<mod>] ..." - Validate <mod>
      if (!ModLoader.TryGetMod(arg, out Mod targetMod)) {
        if (loadedMods.Count > 1) return Error($"Must specify mod when more than one mod is using {nameof(AnimLib)}.");

        // Since only one mod is loaded, command implicitly refers to that mod
        targetMod = loadedMods[0];
        idx--;
      }
      
      if (!AnimLoader.LoadedMods.Contains(targetMod)) return Error($"Mod {targetMod} does not use AnimLib.");

      AbilityManager manager = player.characters[targetMod].abilityManager;
      if (manager is null) return Error($"Mod {targetMod} uses AnimLib but does not have abilities.");
      
      // "/animability <mod>" - List all abilities in mod
      if (!NextArg()) {
        StringBuilder sb = new();
        sb.AppendLine($"Available Abilities for {targetMod.Name} ({targetMod.DisplayName}):");
        foreach (Ability a in manager.abilityArray) {
          sb.Append($"[{a.Id}] {a.GetType().Name}: {(a.Unlocked ? "Unlocked" : "Locked")}");
          if (a is ILevelable l) sb.Append($" at level {l.Level}/{l.MaxLevel}");
          sb.AppendLine();
        }

        return SuccessWarn(sb.ToString());
      }

      // "/animability <mod> [<ability>] ..." - Validate <ability>
      Ability ability = null;
      if (int.TryParse(arg, out int id)) {
        if (!manager.TryGet(id, out ability)) return Error($"Specified ability ID \"{id}\" is out of range.");
        ability = manager[id];
      }
      else {
        foreach (Ability a in manager) {
          if (string.Equals(a.GetType().Name, arg, StringComparison.OrdinalIgnoreCase)) {
            ability = a;
            break;
          }
        }

        if (ability is null) return Error($"Specified ability name \"{arg}\" is not valid.");
      }

      ILevelable levelable = ability as ILevelable;
      // "/animability <mod> <ability>" - Get ability level
      if (!NextArg()) {
        return Success(levelable is null
          ? $"{ability.GetType().Name} [{ability.Id}] is currently {(ability.Unlocked ? "Unlocked" : "Locked")} "
          : $"{ability.GetType().Name} [{ability.Id}] is currently {(ability.Unlocked ? "Unlocked" : "Locked")} at level {levelable.Level}/{levelable.MaxLevel}");
      }

      // "/animability <mod> <ability> [<level>]" - Validate <level>
      if (levelable is null) return Error($"Ability {ability} cannot be leveled.");
      if (!int.TryParse(arg, out int level)) return Error($"Specified level \"{arg}\" must be a number.");
      if (level < 0) return Error($"Specified level \"{level}\" must be a positive number.");
      
      // "/animability <mod> <ability> <level>" - Set ability level
      levelable.Level = level;
      return level > levelable.MaxLevel
        ? SuccessWarn($"{ability.GetType().Name} [{ability.Id}] level set to {levelable.Level}/{levelable.MaxLevel}. This level is above max level, and is not supported.")
        : Success($"{ability.GetType().Name} [{ability.Id}] level set to {levelable.Level}/{levelable.MaxLevel}.");

    }

    private static (string message, Color color) Error(string message) => new(message, Color.Red);
    private static (string message, Color color) Warn(string message) => new(message, Color.Yellow);
    private static (string message, Color color) SuccessWarn(string message) => new(message, Color.GreenYellow);
    private static (string message, Color color) Success(string message) => new(message, Color.LightGreen);
  }
}

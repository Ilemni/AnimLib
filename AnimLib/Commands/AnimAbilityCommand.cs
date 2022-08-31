using System;
using System.Collections.Generic;
using AnimLib.Abilities;
using AnimLib.Internal;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace AnimLib.Commands {
  [UsedImplicitly]
  internal class AnimAbilityCommand : ModCommand {
    public override string Command => "animability";

    public override string Usage => "/animability <mod> <ability> [level]";
    public override string Description => "Get or set the ability level.";

    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
      Message message = Action(caller, args);
      Main.NewText(message.message, message.color);
    }

    [NotNull]
    private Message Action(CommandCaller caller, IReadOnlyList<string> args) {
      int idx = 0;
      string arg = string.Empty;

      bool NextArg() {
        if (args.Count <= idx) return false;
        arg = args[idx++];
        return true;
      }

      AnimPlayer player = caller.Player.GetModPlayer<AnimPlayer>();
      if (!player.DebugEnabled) return Error("This command cannot be used outside of debug mode.");
      if (!AnimLoader.HasMods) return Error($"This command cannot be used when no mods are using {nameof(AnimLib)}.");
      if (!NextArg()) return Error($"This command requires arguments. Usage: {Usage}");

      Mod targetMod = ModLoader.GetMod(arg);
      if (targetMod is null) {
        // We'll allow not specifying mod only if exactly one mod is using AnimLib
        AnimLoader.GetLoadedMods(out var loadedMod);
        if (loadedMod.Count > 1) return Error($"Must specify mod when more than one mod is using {nameof(AnimLib)}.");

        // Only one mod is loaded, command implicitly refers to that mod
        targetMod = loadedMod[0];
        idx--;
      }

      if (!NextArg()) return Error("This command requires at least 2 arguments.");
      if (!AnimLoader.LoadedMods.Contains(targetMod)) return Error($"Mod {targetMod} does not use AnimLib.");

      AbilityManager manager = player.characters[Mod].abilityManager;
      if (manager is null) return Error($"Mod {targetMod} does not have abilities.");

      Ability ability = null;
      if (int.TryParse(arg, out int id)) {
        if (!manager.TryGet(id, out ability)) return Error("Specified ability ID is out of range.");
        ability = manager[id];
      }
      else {
        foreach (Ability a in manager) {
          if (string.Equals(a.GetType().Name, arg, StringComparison.OrdinalIgnoreCase)) {
            ability = a;
            break;
          }
        }

        if (ability is null) return Error($"\"{arg}\" is not a valid ability name.");
      }

      ILevelable levelable = ability as ILevelable;
      if (NextArg()) {
        if (!int.TryParse(arg, out int level)) return Error("Argument must be a number.");
        if (level < 0) return Error("Argument must be a positive number.");
        if (levelable is null) return new Message($"{ability} cannot be leveled.");
        levelable.Level = level;
        return level > levelable.MaxLevel
          ? SuccessWarn($"{ability.GetType().Name} level set to {levelable.Level}/{levelable.MaxLevel}. This level is above max level, and is not supported.")
          : Success($"{ability.GetType().Name} level set to {levelable.Level}/{levelable.MaxLevel}.");
      }

      return Success(levelable is null
        ? $"{ability.GetType().Name} is currently {(ability.Unlocked ? "Unlocked" : "Locked")} "
        : $"{ability.GetType().Name} is currently {(ability.Unlocked ? "Unlocked" : "Locked")} at level {levelable.Level}/{levelable.MaxLevel}");
    }

    private static Message Error(string message) => new Message(message, Color.Red);
    private static Message SuccessWarn(string message) => new Message(message, Color.GreenYellow);
    private static Message Success(string message) => new Message(message, Color.LightGreen);

    private class Message {
      public readonly Color color;

      public readonly string message;
      public Message(string message) : this(message, Color.White) { }

      public Message(string message, Color color) {
        this.message = message;
        this.color = color;
      }
    }
  }
}

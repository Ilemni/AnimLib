using JetBrains.Annotations;
using Terraria.ModLoader;

namespace AnimLib.Internal {
  [UsedImplicitly]
  internal class AnimDebugCommand : ModCommand {
    public override string Command => "animdebug";

    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
      DebugEnabled ^= true;
    }

    public static bool DebugEnabled { get; private set; }
  }
}

using JetBrains.Annotations;

namespace AnimLib.Commands;

[UsedImplicitly]
internal sealed class AnimDebugCommand : ModCommand {
  public override string Command => "animdebug";
  public override CommandType Type => CommandType.Chat;

  public override void Action(CommandCaller caller, string input, string[] args) {
    AnimLibMod.ToggleDebugMode();
    caller.Reply($"Set AnimLib debug mode to {AnimLibMod.DebugEnabled}");
  }
}

using JetBrains.Annotations;
using Terraria.ModLoader;

namespace AnimLib.Commands {
  [UsedImplicitly]
  internal class AnimDebugCommand : ModCommand {
    public override string Command => "animdebug";
    public override string Usage => "/animdebug";
    public override string Description => "Toggle AnimLib debug mode, used for /animability.";
    
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
      var player = caller.Player?.GetModPlayer<AnimPlayer>();
      if(player is not null) {
        player.DebugEnabled ^= true;
        caller.Reply(player.DebugEnabled ? EnabledString : DisabledString);
      }
    }

    const string EnabledString = "AnimLib debug mode enabled.";
    const string DisabledString = "AnimLib debug mode disabled.";
  }
}

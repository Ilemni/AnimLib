using JetBrains.Annotations;
using Terraria.ModLoader;

namespace AnimLib.Commands {
  [UsedImplicitly]
  internal class AnimDebugCommand : ModCommand {
    public override string Command => "animdebug";
    public override CommandType Type => CommandType.Chat;
    public override void Action(CommandCaller caller, string input, string[] args) 
      { if(AnimPlayer.Local is not null) AnimPlayer.Local.DebugEnabled ^= true; }
  }
}

using System.Collections.Generic;
using Terraria.ModLoader;

namespace AnimLib {
#pragma warning disable CS1591
  public class AnimPlayer : ModPlayer {
    internal List<AnimationPlayer> Anims { get; private set; }

    /// <summary> I do this because Initialize() is just barely too early and many things are too late </summary>
    public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
      if (Anims == null) {
        Anims = new List<AnimationPlayer>(AnimLib.PlayerSources.Count);
        AnimLib.PlayerSources.ForEach(s => Anims.Add(new AnimationPlayer(s, player)));
      }
    }
    public override void ModifyDrawLayers(List<PlayerLayer> layers) {
      Anims.ForEach(anim => {
        if (!anim.PreventDraw && anim.Source.Condition(player)) {
          anim.Draw(layers);
        }
      });
    }

    public override void PostUpdate() {
      Anims.ForEach(anim => {
        if (anim.Source.Condition(player)) {
          anim.Update();
        }
      });
    }
  }
}
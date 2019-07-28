using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace AnimLib {
#pragma warning disable CS1591
  public class AnimNpc : GlobalNPC {
    public override bool InstancePerEntity => true;

    internal AnimationNpc[] Animations { get; private set; }

    private bool SourceSet = false;

    public override void SetDefaults(NPC npc) {
      if (!AnimLib.NpcSources.ContainsKey(npc.type)) {
        SourceSet = false;
        return;
      }
      SourceSet = true;
      
      var sources = AnimLib.NpcSources[npc.type].ToArray();
      Animations = new AnimationNpc[sources.Length];
      for (int i = 0; i < sources.Length; i++) {
        Animations[i] = new AnimationNpc(sources[i], npc);
      }
    }
    
    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) => !SourceSet;
    
    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor) {
      if (!SourceSet) return;
      foreach(var anim in Animations) {
        anim.Draw(spriteBatch);
      }
    }
  }
}
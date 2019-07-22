using Terraria.ModLoader;

namespace BetterAnimations {
  public class AnimPlayer : ModPlayer {
    public string AnimName;
    public int AnimIndex;
    public float AnimRotation;
  }
  public class AnimNpc : GlobalNPC {
    public override bool InstancePerEntity => true;

    public string AnimName;
    public int AnimIndex;
    public float AnimRotation;
  }
  public class AnimProjectile : GlobalProjectile {
    public override bool InstancePerEntity => true;

    public string AnimName;
    public int AnimIndex;
    public float AnimRotation;
  }
}
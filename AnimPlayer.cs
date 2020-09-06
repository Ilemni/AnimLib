using System;
using System.Collections.Generic;
using System.Reflection;
using AnimLib.Animations;
using Terraria;
using Terraria.ModLoader;

namespace AnimLib {
  /// <summary>
  /// Main <see cref="ModPlayer"/> class for <see cref="AnimLibMod"/>, contains and updates <see cref="PlayerAnimationData"/>.
  /// </summary>
  public sealed class AnimPlayer : ModPlayer {
    internal readonly Dictionary<Mod, PlayerAnimationData> animationDatas = new Dictionary<Mod, PlayerAnimationData>();

    /// <summary>
    /// Constructs and collects all <see cref="PlayerAnimationData"/>s across all mods onto this <see cref="Player"/>.
    /// </summary>
    public override void Initialize() {
      AnimLoader.PlayerInitialize(this);
    }

    /// <summary>
    /// Updates all <see cref="PlayerAnimationData"/>s on this <see cref="Player"/>.
    /// </summary>
    public override void PostUpdate() {
      foreach (var anim in animationDatas.Values) {
        anim.Update();
      }
    }
  }
}
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
      var types = AnimLibMod.Instance.playerAnimationDataTypes;
      if ((types?.Count ?? 0) > 0) {
        foreach (var pair in types) {
          var mod = pair.Key;
          var type = pair.Value;
          try {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var constructor = type.GetConstructor(flags, null, new[] { typeof(Player), typeof(Mod) }, default);
            if (!(constructor is null)) {
              var playerData = Activator.CreateInstance(type, flags, null, args: new object[] { player, mod }, null) as PlayerAnimationData;
              playerData.Initialize();
              animationDatas[mod] = playerData;
            }
            else {
              AnimLibMod.Instance.Logger.Error($"PlayerAnimationData from [{mod.Name}:{type.FullName}] does not contain a constructor with parameters (Player, Mod).");
              continue;
            }
          }
          catch (Exception ex) {
            AnimLibMod.Instance.Logger.Error($"Exception thrown when constructing PlayerAnimationData from [{mod.Name}:{type.FullName}]", ex);
          }
        }
      }
      else {
        if (AnimDebugCommand.DebugEnabled) {
          AnimLibMod.Instance.Logger.Debug("AnimPlayer instance initialized without animations.");
        }
        return;
      }
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
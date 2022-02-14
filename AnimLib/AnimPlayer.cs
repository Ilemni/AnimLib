using System;
using System.Collections.Generic;
using System.Linq;
using AnimLib.Animations;
using AnimLib.Internal;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AnimLib {
  /// <summary>
  /// Main <see cref="ModPlayer"/> class for <see cref="AnimLibMod"/>, contains and updates <see cref="AnimationController"/>.
  /// </summary>
  [UsedImplicitly]
  public sealed class AnimPlayer : ModPlayer {
    /// <summary>
    /// Max 1 <see cref="AnimationController"/> per mod, requires inheritance. Unlimited <see cref="AnimationSource"/> types per mod.
    /// </summary>
    internal Dictionary<Mod, AnimationController> animationControllers { get; private set; }

    private bool hasInitialized { get; set; }

    /// <summary>
    /// Gets the <see cref="AnimationController"/> of the given type from this <see cref="AnimPlayer"/>.
    /// Use this if you want your code to use values such as the current track and frame.
    /// <para>This <strong>cannot</strong> be used during the <see cref="ModPlayer.Initialize"/> method.</para>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="AnimationController"/> to get.</typeparam>
    /// <returns>An <see cref="AnimationController"/> of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">This was called during <see cref="ModPlayer.Initialize"/>, or was called by code run by a server.</exception>
    public T GetAnimationController<T>() where T : AnimationController {
      if (Main.netMode == NetmodeID.Server)
        throw new InvalidOperationException($"Cannot call {nameof(GetAnimationController)} on code run by a server.");
      if (!hasInitialized)
        throw new InvalidOperationException($"Cannot call {nameof(GetAnimationController)} during ModPlayer.Initialize");

      return animationControllers.Values.FirstOrDefault(c => c is T) as T
             ?? throw new Exception($"{typeof(T).Name} is not loaded.");
    }

    /// <summary>
    /// Constructs and collects all <see cref="AnimationController"/>s across all mods onto this <see cref="Player"/>.
    /// </summary>
    public override void Initialize() {
      if (AnimLoader.UseAnimations) {
        animationControllers = new Dictionary<Mod, AnimationController>();
        AnimationLoader.CreateControllersForPlayer(this);
      }

      hasInitialized = true;
    }

    /// <summary>
    /// Updates all <see cref="AnimationController"/>s on this <see cref="Player"/>.
    /// </summary>
    public override void PostUpdate() {
      if (AnimLoader.UseAnimations) UpdateAnimations();
    }

    private void UpdateAnimations() {
      foreach (AnimationController anim in animationControllers.Values) {
        // Probably not a good idea to crash when a purely cosmetic effect fails.
        try {
          if (anim.PreUpdate()) anim.Update();
        }
        catch (Exception ex) {
          Log.LogError($"[{anim.mod.Name}{anim.GetType().UniqueTypeName()}]: Caught exception.", ex);
          Main.NewText($"AnimLib -> {anim.mod.Name}: Caught exception while updating animations. See client.log for more information.", Color.Red);
        }
      }
    }
  }
}

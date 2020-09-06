using System;
using AnimLib.Animations;
using Terraria;
using Terraria.ModLoader;

namespace AnimLib {
  /// <summary>
  /// Interface for any mods using this mod to interact with.
  /// </summary>
	public sealed class AnimLibMod : Mod {
    /// <summary>
    /// Creates a new instance of <see cref="AnimLibMod"/>. Only for use by tModloader.
    /// </summary>
    public AnimLibMod() {
      if (Instance is null) {
        Instance = this;
      }
      else {
        throw new InvalidOperationException($"{typeof(AnimLibMod)} can only be constructed by tModLoader.");
      }
      Properties = new ModProperties() {
        Autoload = AnimLoader.UseAnimations,
      };
    }

    /// <summary>
    /// The active instance of <see cref="AnimLibMod"/>.
    /// </summary>
		public static AnimLibMod Instance { get; private set; }

    /// <summary>
    /// Gets the <see cref="PlayerAnimationData"/> of the given type from the given <see cref="ModPlayer"/>.
    /// Use this if you want your code to use values such as the current track and frame.
    /// <para>This <strong>cannot</strong> be used during the <see cref="ModPlayer.Initialize"/> method.</para>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="PlayerAnimationData"/> to get.</typeparam>
    /// <param name="modPlayer">The <see cref="ModPlayer"/>.</param>
    /// <returns>A <see cref="PlayerAnimationData"/> of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="modPlayer"/> cannot be null.</exception>
    public static T GetPlayerAnimationData<T>(ModPlayer modPlayer) where T : PlayerAnimationData {
      if (modPlayer is null) {
        throw new ArgumentNullException(nameof(modPlayer));
      }

      return GetPlayerAnimationData<T>(modPlayer.player);
    }

    /// <summary>
    /// Gets the <see cref="PlayerAnimationData"/> of the given type from the given <see cref="Player"/>.
    /// Use this if you want your code to use values such as the current track and frame.
    /// <para>This <strong>cannot</strong> be used during the <see cref="ModPlayer.Initialize"/> method.</para>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="PlayerAnimationData"/> to get.</typeparam>
    /// <param name="player">The <see cref="Player"/>.</param>
    /// <returns>A <see cref="PlayerAnimationData"/> of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="player"/> cannot be null.</exception>
    public static T GetPlayerAnimationData<T>(Player player) where T : PlayerAnimationData {
      if (player is null) {
        throw new ArgumentNullException(nameof(player));
      }

      var animPlayer = player.GetModPlayer<AnimPlayer>();
      foreach (var playerData in animPlayer.animationDatas) {
        if (playerData.Value is T t) {
          return t;
        }
      }
      return null;
    }

    /// <summary>
    /// Gets the <see cref="AnimationSource"/> of the given type from the given <see cref="Mod"/>.
    /// Use this if you want to access one of your <see cref="AnimationSource"/>s.
    /// <para>This <strong>cannot</strong> be used during the <see cref="Mod.PostSetupContent"/> method or earlier.</para>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="AnimationSource"/> to get.</typeparam>
    /// <typeparam name="TMod">Your mod type.</typeparam>
    /// <returns>A <see cref="AnimationSource"/> of type <typeparamref name="T"/>.</returns>
    public static T GetAnimationSource<T, TMod>() where T : AnimationSource where TMod : Mod {
      var srcs = AnimLoader.Instance.animationSources;
      foreach (var mod in srcs.Keys) {
        if (mod is TMod) {
          return GetAnimationSource<T>(mod);
        }
      }
      return null;
    }

    /// <summary>
    /// Gets the <see cref="AnimationSource"/> of the given type.
    /// Use this if you want to access one of your <see cref="AnimationSource"/>s.
    /// <para>This <strong>cannot</strong> be used during the <see cref="Mod.PostSetupContent"/> method or earlier.</para>
    /// </summary>
    /// <param name="mod">Your mod.</param>
    /// <typeparam name="T">Type of <see cref="AnimationSource"/> to get.</typeparam>
    /// <returns>A <see cref="AnimationSource"/> of type <typeparamref name="T"/>.</returns>
    public static T GetAnimationSource<T>(Mod mod) where T : AnimationSource {
      foreach (var source in AnimLoader.Instance.animationSources[mod]) {
        if (source is T t) {
          return t;
        }
      }
      return null;
    }

    /// <summary>
    /// Use this to null static reference types on unload.
    /// </summary>
    internal static event Action OnUnload;


    public override void Load() {
      if (AnimLoader.UseAnimations) {
        AnimLoader.Load();
      }
    }

    /// <summary>
    /// Collects and constructs all <see cref="AnimationSource"/>s across all other <see cref="Mod"/>s.
    /// </summary>
    public override void PostSetupContent() {
      if (AnimLoader.UseAnimations) {
        AnimLoader.PostSetupContent();
      }
    }

    /// <inheritdoc/>
    public override void Unload() {
      OnUnload?.Invoke();
      OnUnload = null;
      Instance = null;
    }
  }
}
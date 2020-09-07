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
    /// GitHub profile that the mod's repository is stored on.
    /// </summary>
    public static string GithubUserName => "TwiliChaos";

    /// <summary>
    /// Name of the GitHub repository this mod is stored on.
    /// </summary>
    public static string GithubProjectName => "AnimLib";

    /// <summary>
    /// Gets the <see cref="AnimationController"/> of the given type from the given <see cref="ModPlayer"/>.
    /// Use this if you want your code to use values such as the current track and frame.
    /// <para>This <strong>cannot</strong> be used during the <see cref="ModPlayer.Initialize"/> method.</para>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="AnimationController"/> to get.</typeparam>
    /// <param name="modPlayer">The <see cref="ModPlayer"/>.</param>
    /// <returns>An <see cref="AnimationController"/> of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="modPlayer"/> cannot be null.</exception>
    public static T GetAnimationController<T>(ModPlayer modPlayer) where T : AnimationController {
      if (modPlayer is null) {
        throw new ArgumentNullException(nameof(modPlayer));
      }

      return modPlayer.player.GetModPlayer<AnimPlayer>().GetAnimationController<T>();
    }

    /// <summary>
    /// Gets the <see cref="AnimationController"/> of the given type from the given <see cref="Player"/>.
    /// Use this if you want your code to use values such as the current track and frame.
    /// <para>This <strong>cannot</strong> be used during the <see cref="ModPlayer.Initialize"/> method.</para>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="AnimationController"/> to get.</typeparam>
    /// <param name="player">The <see cref="Player"/>.</param>
    /// <returns>An <see cref="AnimationController"/> of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="player"/> cannot be null.</exception>
    public static T GetAnimationController<T>(Player player) where T : AnimationController {
      if (player is null) {
        throw new ArgumentNullException(nameof(player));
      }

      var animPlayer = player.GetModPlayer<AnimPlayer>();
      return animPlayer.GetAnimationController<T>();
    }

    /// <summary>
    /// Gets the <see cref="AnimationSource"/> of the given type.
    /// Use this if you want to access one of your <see cref="AnimationSource"/>s.
    /// <para>This <strong>cannot</strong> be used during the <see cref="Mod.PostSetupContent"/> method or earlier.</para>
    /// </summary>
    /// <param name="mod">Your mod.</param>
    /// <typeparam name="T">Type of <see cref="AnimationSource"/> to get.</typeparam>
    /// <returns>An <see cref="AnimationSource"/> of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="mod"/> has no <see cref="AnimationSource"/>.</exception>
    public static T GetAnimationSource<T>(Mod mod) where T : AnimationSource {
      var sources = AnimLoader.animationSources;
      if (!sources.ContainsKey(mod)) {
        throw new ArgumentException($"The mod {mod.Name} has no {nameof(AnimationSource)}");
      }
      foreach (var source in sources[mod]) {
        if (source is T t) {
          return t;
        }
      }
      return null;
    }


    /// <summary>
    /// Creates a new instance of <see cref="AnimLibMod"/>.
    /// </summary>
    public AnimLibMod() {
      if (Instance is null) {
        Instance = this;
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
    /// Use this to null static reference types on unload.
    /// </summary>
    internal static event Action OnUnload;


    /// <summary>
    /// Collects the classes extending <see cref="AnimationSource"/> and <see cref="AnimationController"/> from all mods.
    /// </summary>
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
      AnimLoader.OnUnload();
      OnUnload?.Invoke();
      OnUnload = null;
      Instance = null;
    }
  }
}
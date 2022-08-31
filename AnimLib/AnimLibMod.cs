using System;
using System.Diagnostics;
using System.Linq;
using AnimLib.Abilities;
using AnimLib.Animations;
using AnimLib.Extensions;
using AnimLib.Internal;
using JetBrains.Annotations;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Animation = AnimLib.Animations.Animation;

namespace AnimLib {
  /// <summary>
  /// Interface for any mods using this mod to interact with.
  /// </summary>
  [PublicAPI]
  public sealed class AnimLibMod : Mod {
    /// <summary>
    /// Creates a new instance of <see cref="AnimLibMod"/>.
    /// </summary>
    public AnimLibMod() {
      if (Instance is null) Instance = this;
    }

    /// <summary>
    /// The active instance of <see cref="AnimLibMod"/>.
    /// </summary>
    public static AnimLibMod Instance { get; private set; }


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
    /// <exception cref="ArgumentException">
    /// The <see cref="Mod"/> in <paramref name="modPlayer"/> does not have an <see cref="AnimationController"/> of type <typeparamref name="T"/>.
    /// </exception>
    [NotNull]
    public static T GetAnimationController<T>([NotNull] ModPlayer modPlayer) where T : AnimationController {
      if (modPlayer is null) throw new ArgumentNullException(nameof(modPlayer));
      AnimationController controller = modPlayer.GetAnimCharacter().animationController;
      return controller is T t ? t : throw ThrowHelper.BadType<T>(controller, modPlayer.Mod, nameof(T));
    }

    /// <summary>
    /// Gets the <see cref="AnimationSource"/> of the given type.
    /// Use this if you want to access one of your <see cref="AnimationSource"/>s.
    /// <para>This <strong>cannot</strong> be used during the <see cref="Mod.PostSetupContent"/> method or earlier.</para>
    /// </summary>
    /// <param name="mod">Your mod.</param>
    /// <typeparam name="T">Type of <see cref="AnimationSource"/> to get.</typeparam>
    /// <returns>An <see cref="AnimationSource"/> of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mod"/> cannot be <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="mod"/> has no <see cref="AnimationSource"/>, or source from the wrong mod was used.</exception>
    [NotNull]
    public static T GetAnimationSource<T>([NotNull] Mod mod) where T : AnimationSource {
      if (mod is null) throw new ArgumentNullException(nameof(mod));
      if (!AnimLoader.AnimationSources.TryGetValue(mod, out var sources))
        throw new ArgumentException($"The mod {mod.Name} does not have any {nameof(AnimationSource)}s loaded.");

      return sources.FirstOrDefault(s => s is T) as T
             ?? throw new ArgumentException($"{typeof(T)} does not belong to {mod.Name}");
    }


    /// <summary>
    /// Gets the <see cref="AbilityManager"/> of the given type from the given <see cref="ModPlayer"/>.
    /// Use this if you want your code to access ability information.
    /// <para>This <strong>cannot</strong> be used during the <see cref="ModPlayer.Initialize"/> method.</para>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="AbilityManager"/> to get.</typeparam>
    /// <param name="modPlayer">The <see cref="ModPlayer"/>.</param>
    /// <returns>An <see cref="AbilityManager"/> of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="modPlayer"/> cannot be null.</exception>
    /// <exception cref="ArgumentException">
    /// The <see cref="Mod"/> in <paramref name="modPlayer"/> does not have an <see cref="AbilityManager"/> of type <typeparamref name="T"/>.
    /// </exception>
    [NotNull]
    public static T GetAbilityManager<T>([NotNull] ModPlayer modPlayer) where T : AbilityManager {
      if (modPlayer is null) throw new ArgumentNullException(nameof(modPlayer));
      AbilityManager manager = modPlayer.GetAnimCharacter().abilityManager;
      return manager is T t ? t : throw ThrowHelper.BadType<T>(manager, modPlayer.Mod, nameof(T));
    }


    /// <summary>
    /// Gets a <see cref="DrawData"/> from the given <see cref="PlayerDrawInfo"/>, based on your <see cref="AnimationController"/> and
    /// <see cref="AnimationSource"/>.
    /// <para>
    /// This can be a quick way to get a <see cref="DrawData"/> that's ready to use for your <see cref="PlayerLayer"/>s.
    /// For a more performant way of getting a <see cref="DrawData"/>, cache your <see cref="AnimationController"/> in your <see cref="ModPlayer"/>
    /// and <see cref="Animations.Animation"/> in your <see cref="AnimationController"/>, and use
    /// <see cref="Animations.Animation.GetDrawData(PlayerDrawInfo)"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TController"> Your type of <see cref="AnimationController"/>.</typeparam>
    /// <typeparam name="TSource"> Your type of <see cref="AnimationSource"/>.</typeparam>
    /// <param name="drawInfo">The <see cref="PlayerDrawInfo"/> to get the <see cref="DrawData"/> from.</param>
    /// <returns>A <see cref="DrawData"/> that is ready to be drawn. Feel free to modify it.</returns>
    public static DrawData GetDrawData<TController, TSource>(PlayerDrawSet drawInfo)
      where TController : AnimationController where TSource : AnimationSource {
      AnimPlayer animPlayer = drawInfo.drawPlayer.GetModPlayer<AnimPlayer>();
      Mod mod = AnimHelper.GetModFromController<TController>();
      AnimationController controller = animPlayer.characters[mod].animationController;
      Debug.Assert(controller != null);
      Animation anim = controller.GetAnimation<TSource>();
      return anim.GetDrawData(drawInfo);
    }

    /// <summary>
    /// Use this to null static reference types on unload.
    /// </summary>
    internal static event Action OnUnload;

    /// <summary>
    /// Collects and constructs all <see cref="AnimationSource"/>s across all other <see cref="Mod"/>s.
    /// </summary>
    public override void PostSetupContent() => AnimLoader.Load();

    /// <inheritdoc/>
    public override void Unload() {
      OnUnload?.Invoke();
      OnUnload = null;
      Instance = null;
    }
  }
}

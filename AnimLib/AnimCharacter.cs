using System;
using System.Collections.Generic;
using AnimLib.Abilities;
using AnimLib.Animations;
using AnimLib.Extensions;
using AnimLib.Internal;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace AnimLib {
  /// <summary>
  /// Generic wrapper for non-generic type <see cref="AnimCharacter"/>. Only <see cref="AnimationController"/> is wrapped as <typeparamref name="TAnimation"/>.
  /// </summary>
  /// <typeparam name="TAnimation">Your type of <see cref="AnimationController"/></typeparam>
  public class AnimCharacter<TAnimation> : AnimCharacter<TAnimation, AbilityManager> where TAnimation : AnimationController {
    /// <summary>
    /// Returns an object that wraps the specified <see cref="AnimCharacter"/>. Consider using <see cref="AnimCharacter.As{T}"/>
    /// </summary>
    /// <param name="wrapped"></param>
    /// <exception cref="ArgumentException">The wrapped <see cref="AnimationController"/> is not of the specified type.</exception>
    public AnimCharacter(AnimCharacter wrapped) : base(wrapped) { }
  }

  /// <summary>
  /// Generic wrapper for non-generic type <see cref="AnimCharacter"/>.
  /// </summary>
  /// <typeparam name="TAnimation">Your type of <see cref="AnimationController"/></typeparam>
  /// <typeparam name="TAbility">Your type of <see cref="AbilityManager"/></typeparam>
  [PublicAPI]
  public class AnimCharacter<TAnimation, TAbility> where TAnimation : AnimationController where TAbility : AbilityManager {
    private AnimCharacter wrapped;

    /// <inheritdoc/>
    internal AnimCharacter(ModPlayer modPlayer) : this(modPlayer.player.GetModPlayer<AnimPlayer>(), modPlayer.mod) { }

    internal AnimCharacter(AnimPlayer animPlayer, Mod mod) : this(
      animPlayer.characters.TryGetValue(mod, out AnimCharacter character)
        ? character
        : throw ThrowHelper.NoType(mod)) { }

    /// <summary>
    /// Returns an object that wraps the specified <see cref="AnimCharacter"/>. Consider using <see cref="AnimCharacter.As{T,T}"/>
    /// </summary>
    /// <param name="wrapped"></param>
    /// <exception cref="ArgumentException">Either the wrapped <see cref="AnimationController"/> or <see cref="AbilityManager"/> are not of the specified types.</exception>
    public AnimCharacter(AnimCharacter wrapped) {
      if (!(wrapped.animationController is TAnimation)) throw ThrowHelper.BadType<TAnimation>(wrapped.animationController, wrapped.mod);
      if (!(wrapped.abilityManager is TAbility)) throw ThrowHelper.BadType<TAbility>(wrapped.abilityManager, wrapped.mod);
      this.wrapped = wrapped;
    }

    /// <inheritdoc cref="AnimCharacter.animationController"/>
    public TAnimation animationController => wrapped.animationController as TAnimation;

    /// <inheritdoc cref="AnimCharacter.abilityManager"/>
    public TAbility abilityManager => wrapped.abilityManager as TAbility;

    /// <inheritdoc cref="AnimCharacter.IsEnabled"/>
    public bool IsEnabled => wrapped.IsEnabled;

    /// <inheritdoc cref="AnimCharacter.IsActive"/>
    public bool IsActive => wrapped.IsActive;

    /// <inheritdoc cref="AnimCharacter.Enable(AnimCharacter.Priority)"/>
    public void Enable(AnimCharacter.Priority priority = AnimCharacter.Priority.Default) => wrapped.Enable(priority);

    /// <inheritdoc cref="AnimCharacter.Disable"/>
    public void Disable() => wrapped.Disable();

    /// <inheritdoc cref="AnimCharacter.OnEnable"/>
    public event Action OnEnable {
      add => wrapped.OnEnable += value;
      remove => wrapped.OnEnable -= value;
    }

    /// <inheritdoc cref="AnimCharacter.OnDisable"/>
    public event Action OnDisable {
      add => wrapped.OnDisable -= value;
      remove => wrapped.OnDisable -= value;
    }

    /// <summary>
    /// Wraps the non-generic <see cref="AnimCharacter"/> for generic access of members.
    /// </summary>
    /// <param name="animCharacter"></param>
    /// <returns></returns>
    public static explicit operator AnimCharacter<TAnimation, TAbility>(AnimCharacter animCharacter) => new AnimCharacter<TAnimation, TAbility>(animCharacter);

    /// <summary>
    /// Unwraps the generic <see cref="AnimCharacter{T, T}"/> to its non-generic <see cref="AnimCharacter"/> instance.
    /// </summary>
    /// <param name="animCharacter"></param>
    /// <returns></returns>
    public static implicit operator AnimCharacter(AnimCharacter<TAnimation, TAbility> animCharacter) => animCharacter.wrapped;
  }

  /// <summary>
  /// Class with references for
  /// </summary>
  [PublicAPI]
  public class AnimCharacter {
    /// <summary>
    /// Enum representing the priority of the active character, for determining replacing the active state of a character..
    /// Used to determine if <see cref="AnimCharacter"/> can disable by other <see cref="AnimCharacter">AnimCharacters</see>.
    /// </summary>
    public enum Priority {
      /// <summary>
      /// Low priority. This character can only be enabled if no other characters are in use,
      /// and can be deactivated by any other character.
      /// </summary>
      Lowest = 1,

      /// <summary>
      /// The standard priority. This priority is typically for when the character is enabled by toggle (i.e. right-click item or tile, temporary buff),
      /// and can be disabled by other characters of <see cref="Priority.High">Priority.High</see> or higher priority.
      /// </summary>
      Default = 2,

      /// <summary>
      /// The character should be enabled is enabled by the player wearing equipment,
      /// and cannot be disabled by other characters (except by <see cref="Priority.Highest">Priority.Highest</see>.
      /// <para/>
      /// This character can only be disabled by this mod, ideally by the player unequipping the items that enabled it.
      /// </summary>
      High = 3,

      /// <summary>
      /// This character should be enabled no matter what. Consider only using this if you need to force your character state onto a player (i.e. debuff).
      /// This character cannot replace already-enabled characters of the same priority, and cannot be disabled by any other character.
      /// </summary>
      Highest = 4
    }

    internal AnimCharacter(AnimPlayer animPlayer, Mod mod) {
      if (AnimLoader.modAnimationControllerTypeDictionary.TryGetValue(mod, out Type controllerType))
        animationController = TryCreateControllerForPlayer(animPlayer, mod, controllerType);

      bool hasManager = AnimLoader.modAbilityManagerTypeDictionary.TryGetValue(mod, out Type managerType);
      bool hasAbilities = AnimLoader.modAbilityTypeDictionary.TryGetValue(mod, out var abilityTypes);
      if (hasManager || hasAbilities) abilityManager = CreateAbilityManagerForPlayer(animPlayer, mod, managerType ?? typeof(AbilityManager), abilityTypes);

      this.mod = mod;
      characters = animPlayer.characters;
    }

    /// <summary>
    /// The <see cref="Mod"/> that this <see cref="AnimCharacter"/> instance belongs to.
    /// </summary>
    [NotNull] public Mod mod { get; internal set; }

    /// <summary>
    /// The <see cref="AnimationController"/> of this character.
    /// <para/>
    /// This value is your type of <see cref="AnimationController"/> if your mod has a type inheriting <see cref="AnimationController"/>;
    /// otherwise, it is <see langword="null"/>.
    /// </summary>
    [CanBeNull] public AnimationController animationController { get; internal set; }

    /// <summary>
    /// The <see cref="AbilityManager"/> of this character.
    /// This value is <see langword="null"/> if your mod does not have any types inheriting types in the <see cref="AnimLib.Abilities"/> namespace.
    /// This value is your type of <see cref="AbilityManager"/> if your mod has a type inheriting <see cref="AbilityManager"/>;
    /// otherwise, it is <see cref="AnimLib"/>'s type <see cref="AbilityManager"/>.
    /// </summary>
    [CanBeNull] public AbilityManager abilityManager { get; internal set; }

    [NotNull] internal AnimCharacterCollection characters { get; private set; }


    /// <summary>
    /// Whether or not this <see cref="AnimCharacter"/> is intended to be enabled on the <see cref="Terraria.Player"/>.
    /// <para/>
    /// This being <see langword="true"/> does not guarantee this <see cref="AnimCharacter"/> is active,
    /// as another character of a higher <see cref="Priority"/> may be active instead.
    /// </summary>
    /// <seealso cref="TryEnable"/>
    /// <seealso cref="IsActive"/>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// Whether or not this <see cref="AnimCharacter"/> is the current active character on the <see cref="Terraria.Player"/>.
    /// <para/>
    /// Only one <see cref="AnimCharacter"/> instance may be active on a character at a given time.
    /// </summary>
    /// <seealso cref="TryEnable"/>
    /// <seealso cref="IsEnabled"/>
    public bool IsActive => IsEnabled && ReferenceEquals(this, characters.ActiveCharacter);

    /// <summary>
    /// The way that the character was enabled by the player.
    /// </summary>
    public Priority currentPriority { get; private set; }

    /// <summary>
    /// Cast this <see cref="AnimCharacter"/> to <see cref="AnimCharacter{T, T}"/>
    /// </summary>
    /// <typeparam name="TAnimation">Your type of <see cref="AnimationController"/>.</typeparam>
    /// <typeparam name="TAbility">Your type of <see cref="AbilityManager"/></typeparam>
    /// <returns></returns>
    [NotNull]
    public AnimCharacter<TAnimation, TAbility> As<TAnimation, TAbility>() where TAnimation : AnimationController where TAbility : AbilityManager =>
      new AnimCharacter<TAnimation, TAbility>(this);

    /// <summary>
    /// Cast this <see cref="AnimCharacter"/> to <see cref="AnimCharacter{T}"/>.
    /// </summary>
    /// <typeparam name="TAnimation">Your type of <see cref="AnimationController"/>.</typeparam>
    /// <returns></returns>
    [NotNull]
    public AnimCharacter<TAnimation> As<TAnimation>() where TAnimation : AnimationController =>
      new AnimCharacter<TAnimation>(this);

    /// <summary>
    /// Returns a value representing whether or not you are able to enable the character at this time.
    /// This will return <see langword="false"/> if another <see cref="AnimCharacter"/> of an equal or higher <see cref="Priority"/> is already enabled.
    /// </summary>
    /// <param name="priority">The priority you would be using.</param>
    /// <returns></returns>
    public bool CanEnable(Priority priority) => characters.CanEnable(priority);

    /// <summary>
    /// Attempt to enable your character. Note that you may not be able to enable your character
    /// if another character of a similar or higher <see cref="Priority"/> is already active.
    /// </summary>
    /// <param name="priority">The way that the player enabled the character.</param>
    /// <returns></returns>
    public bool TryEnable(Priority priority = Priority.Default) {
      if (!CanEnable(priority)) return false;
      characters.Enable(this, priority);
      return true;
    }

    internal void Enable(Priority priority = Priority.Default) {
      IsEnabled = true;
      currentPriority = priority;
      _onEnable?.Invoke();
    }

    /// <summary>
    /// Disable your character.
    /// </summary>
    public void Disable() {
      IsEnabled = false;
      _onDisable?.Invoke();
    }

    /// <summary>
    /// Event called when the <see cref="AnimCharacter"/> is enabled.
    /// </summary>
    public event Action OnEnable {
      add => _onEnable += value;
      remove => _onEnable -= value;
    }

    /// <summary>
    /// Event called when the <see cref="AnimCharacter"/> is disabled.
    /// </summary>
    public event Action OnDisable {
      add => _onDisable += value;
      remove => _onDisable -= value;
    }

    private event Action _onEnable;
    private event Action _onDisable;

    internal void Update() => abilityManager?.Update();

    internal void PostUpdate() {
      if (abilityManager != null) {
        try {
          abilityManager.PostUpdate();
        }
        catch (Exception ex) {
          Log.LogError($"[{abilityManager.mod.Name}:{abilityManager.GetType().UniqueTypeName()}]: Caught exception.", ex);
          Main.NewText($"AnimLib -> {abilityManager.mod.Name}: Caught exception while updating abilities. See client.log for more information.", Color.Red);
        }
      }

      if (animationController is null) return;
      try {
        if (animationController.PreUpdate()) animationController.Update();
      }
      catch (Exception ex) {
        Log.LogError($"[{animationController.mod.Name}:{animationController.GetType().UniqueTypeName()}]: Caught exception.", ex);
        Main.NewText($"AnimLib -> {animationController.mod.Name}: Caught exception while updating animations. See client.log for more information.", Color.Red);
      }
    }

    #region Constructor Methods
    internal static AnimationController TryCreateControllerForPlayer(AnimPlayer animPlayer, Mod mod, Type type) {
      try {
        AnimationController controller = (AnimationController)Activator.CreateInstance(type, true);
        controller.player = animPlayer.player;
        controller.mod = mod;
        controller.SetupAnimations();
        controller.Initialize();
        return controller;
      }
      catch (Exception ex) {
        Log.LogError($"Exception thrown when constructing {nameof(AnimationController)} from [{mod.Name}:{type.FullName}]", ex);
        throw;
      }
    }

    private static AbilityManager CreateAbilityManagerForPlayer(AnimPlayer animPlayer, Mod mod, Type managerType, IEnumerable<Type> abilityTypes) {
      try {
        AbilityManager manager = (AbilityManager)Activator.CreateInstance(managerType);
        manager.animPlayer = animPlayer;
        manager.player = animPlayer.player;
        manager.mod = mod;

        if (manager.Autoload && abilityTypes != null) AutoloadAbilities(manager, abilityTypes);

        InitializeAbilityManager(manager);
        return manager;
      }
      catch (Exception ex) {
        Log.LogError($"Exception thrown when constructing {nameof(AbilityManager)} from [{mod.Name}:{managerType.FullName}]", ex);
        throw;
      }
    }

    private static void AutoloadAbilities(AbilityManager manager, IEnumerable<Type> abilityTypes) {
      var list = new List<Ability>();
      foreach (Type abilityType in abilityTypes) {
        if (AutoloadAbility(abilityType, manager, out Ability ability))
          list.Add(ability);
      }

      // Sort by ability ID - lowest ID should always be first in the array.
      list.Sort((a1, a2) => a1.Id.CompareTo(a2.Id));
      manager.abilityArray = list.ToArray();
    }

    private static bool AutoloadAbility(Type abilityType, AbilityManager manager, out Ability ability) {
      ability = (Ability)Activator.CreateInstance(abilityType);
      ability.abilities = manager;
      ability.player = manager.player;
      return ability.Autoload;
    }

    private static void InitializeAbilityManager(AbilityManager manager) {
      manager.Initialize();
      // ReSharper disable once ConditionIsAlwaysTrueOrFalse, HeuristicUnreachableCode
      if (manager.abilityArray is null) return;
      foreach (Ability ability in manager.abilityArray) ability.Initialize();
    }
    #endregion
  }
}

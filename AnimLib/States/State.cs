using System.Linq;
using AnimLib.Animations;
using AnimLib.Menus.Debug;
using JetBrains.Annotations;

namespace AnimLib.States;

/// <summary>
/// Base class for all States.
/// Represents a single state which the specified <see cref="Player"/> can be in.
/// <para />
/// Most update methods are called only when
/// <see cref="Character"/>.Active is <see langword="true"/>, or
/// <see cref="Active"/> is <see langword="true"/>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
[PublicAPI]
[DebuggerDisplay("Name = {Name}, Player = {Player?.name}, Active = {Active}, ActiveTime = {ActiveTime}")]
public abstract partial class State {
  /// <summary>
  /// The string used to represent this State in the UI.
  /// </summary>
  public virtual string? DisplayName { get; }

  /// <summary>
  /// The parent which this instance belongs to, -or-
  /// <see langword="null"/> if this instance has no parent.
  /// </summary>
  public State? Parent { get; internal set; }

  /// <summary>
  /// The <see cref="AnimCharacter"/> which this instance belongs to, -or-
  /// <see langword="null"/> if this instance does not belong to a character.
  /// </summary>
  public virtual AnimCharacter? Character { get; internal set; }

  /// <summary>
  /// Whether <see cref="Player"/> is the local player.
  /// </summary>
  public bool IsLocal => Player.whoAmI == Main.myPlayer;


  /// <summary>
  /// Span which represents all <see cref="State"/>s that are on the <see cref="Player"/>.
  /// </summary>
  public ReadOnlySpan<State> AllStates => AllStatesArray;

  /// <summary>
  /// Collection of children which are the immediate children of this instance.
  /// </summary>
  public IEnumerable<State> Children => Hierarchy.ChildrenIds.Select(id => AllStates[id]);

  public IEnumerable<State> AllChildren => Hierarchy.AllChildrenIds.Select(id => AllStates[id]);

  /// <summary>
  /// All active children of this instance.
  /// </summary>
  /// <remarks>
  /// To turn this State into a "Concurrent State",
  /// override this to point to <see cref="Children"/>.
  /// </remarks>
  public virtual IEnumerable<State> ActiveChildren => Children.Where(child => child.ActiveSelf);


  /// <summary>
  /// Whether this State is currently active. Accounts for whether the parents are active.
  /// </summary>
  public bool Active => ActiveSelf && ActiveCondition &&
    (Parent is null || Parent.Active && Parent.IsChildActiveToThis(this));

  /// <summary>
  /// Time which this State was active, in ticks.
  /// <para />
  /// This is incremented every tick just before <see cref="State.PreUpdate"/>,
  /// and is reset to 0 before <see cref="State.OnEnter"/> and after <see cref="State.OnExit"/>.
  /// <para /> This property is always synced for calls to <see cref="NetSync"/>, and can safely be used for conditional syncing.
  /// </summary>
  public int ActiveTime {
    get => _activeTime;
    internal set => _activeTime = value;
  }

  /// <summary>
  /// Time since this State was previously active, in ticks.
  /// If the State has never been active, this value is <see cref="int.MaxValue"/>.
  /// <para />
  /// This is incremented every tick just before <see cref="State.PreUpdate"/>,
  /// and is reset to 0 before <see cref="State.OnEnter"/> and after <see cref="State.OnExit"/>.
  /// <para /> This property is always synced for calls to <see cref="NetSync"/>, and can safely be used for conditional syncing.
  /// </summary>
  public int InactiveTime {
    get => _inactiveTime;
    internal set => _inactiveTime = value;
  }

  /// <summary>
  /// Whether this State is currently active, irrespective of parent states.
  /// </summary>
  public bool ActiveSelf { get; private set; }

  /// <summary>
  /// Additional condition to determine whether <see cref="Active"/> should return <see langword="true"/>.
  /// </summary>
  protected virtual bool ActiveCondition => true;


  internal StateHierarchy Hierarchy { get; set; } = null!; // StateLoader.NewInstances()

  internal State[] AllStatesArray { get; set; } = null!; // StateLoader.NewInstances()

  // Not auto properties, used by ref in NetSync
  private int _activeTime;
  private int _inactiveTime = int.MaxValue;


  /// <summary>
  /// Set this State to be active or inactive.
  /// </summary>
  /// <param name="active">
  /// Whether to set this State to be active or inactive.
  /// If <see langword="true"/>, calls <see cref="Enter"/>;
  /// otherwise, calls <see cref="Exit"/>.
  /// </param>
  /// <param name="otherState">
  /// Optional parameter for a State that was previously active, which is transitioning to this State.
  /// <br /> If <paramref name="active"/> is <see langword="false"/>,
  /// this is the State that is being transitioned to, and will become active.
  /// <br /> If <paramref name="active"/> is <see langword="true"/>,
  /// this is the State that is being transitioned from, and will become inactive.
  /// </param>
  public void SetActive(bool active, State? otherState = null) {
    if (ActiveSelf == active) {
      return;
    }

    bool wasActive = Active;
    ActiveSelf = active;
    if (Active != wasActive) {
      if (Active) {
        Enter(otherState);
      }
      else {
        Exit(otherState);
      }
    }
  }

  /// <summary>
  /// Whether this instance, as the immediate parent of <paramref name="child"/>,
  /// considers the specified <paramref name="child"/> to be active.
  /// </summary>
  /// <param name="child">The child State.</param>
  /// <returns>
  /// <see langword="true"/> if <paramref name="child"/> is to be considered active to this instance;
  /// otherwise, <see langword="false"/>.
  /// </returns>
  protected virtual bool IsChildActiveToThis(State child) => child.ActiveSelf;

  /// <summary>
  /// This is where you add any children to this State.
  /// <para />
  /// Note that this is only called on the template instance of this State, and is not called on Player instances.
  /// </summary>
  public virtual void RegisterChildren(List<State> statesToAdd) {
  }

  /// <summary>
  /// Add any States to this list via <see cref="GetState{T}()"/> to have <see cref="UpdateInterrupt"/>
  /// be called when any of the specified states are active.
  /// </summary>
  public virtual void RegisterInterruptibles(List<State> interruptibles) {
  }

  /// <summary>
  /// Whether this <see cref="State"/> can be entered, irrespective of other state conditions.
  /// </summary>
  /// <returns><see langword="true"/> to allow entering the State.</returns>
  public virtual bool CanEnter() => true;

  /// <summary>
  /// Method called once the parent <see cref="State"/> actives this as its child <see cref="State"/>.
  /// <para />
  /// If anything set here, or in <see cref="UpdateInterrupt"/>, needs to be synced,
  /// set <see cref="NetUpdate"/> to <see langword="true"/> here.
  /// </summary>
  /// <seealso cref="OnExit"/>
  protected virtual void OnEnter(State? fromState) {
  }

  /// <summary>
  /// Method called once the parent <see cref="State"/> switches from this child <see cref="State"/>
  /// to a different <see cref="State"/>.
  /// </summary>
  /// <remarks>
  /// Since a <see cref="State"/> can exit for any reason, including "stun" states or dying,
  /// this method should be used to clean up things, such as killing or restoring dependent entities,
  /// rather than perform "happy path" last tick behaviours.
  /// </remarks>
  /// <seealso cref="OnEnter"/>
  protected virtual void OnExit(State? toState) {
  }

  /// <summary>
  /// Whether this state can be transitioned to, from the specified active state.
  /// By default, returns <see langword="true"/>.
  /// </summary>
  /// <param name="fromState">The currently active state to transition from.</param>
  /// <returns><see langword="true"/> to allow transitioning to this state, from the provided state.</returns>
  protected internal virtual bool CanTransitionFrom(State fromState) => true;

  /// <summary>
  /// Called every tick immediately after <see cref="ProcessTriggers"/>,
  /// if any instance that was added during
  /// <see cref="State.RegisterInterruptibles"/> is currently active.
  /// <para />
  /// If both this and <see cref="CanEnter"/> return <see langword="true"/>,
  /// the interrupt will be successful.
  /// If any state successfully interrupts during this tick,
  /// any remaining interrupts will not be called.
  /// <para />
  /// By default, returns <see langword="false"/>.
  /// </summary>
  /// <param name="activeState">
  /// The currently active state, which would be cancelled if this returns <see langword="true"/>.
  /// <br />This will be a state that was added during <see cref="State.RegisterInterruptibles"/>.
  /// </param>
  /// <returns>
  /// <see langword="true"/> to change the active state to this instance,
  /// <see langword="false"/> to do nothing.</returns>
  /// <seealso cref="State.RegisterInterruptibles"/>
  /// <remarks>
  /// Where <see cref="CanEnter"/> is intended to show when a State is eligible to be entered,
  /// <see cref="UpdateInterrupt"/> is intended for when a player is also actively trying to enter the state.
  /// <para />
  /// As an example, an "Air Attack" state's
  /// <br/><see cref="CanEnter"/> may check for the player being in the air, while its
  /// <br/><see cref="UpdateInterrupt"/> may check for the player pressing the relevant attack button.
  /// </remarks>
  protected internal virtual bool UpdateInterrupt(State activeState) => false;

  /// <summary>
  /// Displays runtime information about this <see cref="State"/> in the Debug UI.
  /// <para />
  /// Information is mostly displayed by using methods
  /// `DrawAppendX`
  /// </summary>
  /// <param name="ui">The UI in which this information will be displayed.</param>
  protected internal virtual void DebugText(UIStateInfo ui) {
  }

  // [OverloadResolutionPriority(1)]
  /// <summary>
  /// Get the <see cref="State"/> of type <typeparamref name="T"/>,
  /// which is on the same <see cref="Terraria.Player"/> as this instance.
  /// </summary>
  /// <typeparam name="T">The <see cref="State"/> type.</typeparam>
  /// <returns>
  /// The <see cref="State"/> of type <typeparamref name="T"/> which is on the same <see cref="Terraria.Player"/>
  /// </returns>
  public T GetState<T>() where T : State, new() {
    ushort index = ModContent.GetInstance<T>().Index;
    return (T)AllStates[index];
  }

  /// <summary>
  /// Get a <see cref="State"/> that inherits from <typeparamref name="T"/>,
  /// which is on the same <see cref="Terraria.Player"/> as this instance,
  /// and whose type is the same as the provided <paramref name="template"/> State.
  /// </summary>
  /// <param name="template">
  /// The template <see cref="State"/> to use to get the State with the matching concrete type.
  /// </param>
  /// <typeparam name="T">The base <see cref="State"/> type.</typeparam>
  /// <returns>
  /// The <see cref="State"/> of type <typeparamref name="T"/> which is on the same <see cref="Terraria.Player"/>
  /// </returns>
  public T GetState<T>(T template) where T : State => (T)AllStates[template.Index];

  /// <summary>
  /// Gets a <see cref="State"/> by its <see cref="State.Index"/>.
  /// </summary>
  /// <param name="index">The <see cref="State.Index"/> value.</param>
  /// <returns>
  /// The <see cref="State"/> with the specified <see cref="State.Index"/>.
  /// </returns>
  public State GetState(int index) => AllStates[index];

  /// <summary>
  /// Get the child <see cref="State"/> of type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">The <see cref="State"/> type.</typeparam>
  /// <returns>The child <see cref="State"/> of type <typeparamref name="T"/>.</returns>
  /// <exception cref="ArgumentException">
  /// This instance has no children of the specified type <typeparamref name="T"/>.
  /// </exception>
  public T GetChild<T>() where T : State, new() {
    T child = GetState<T>();
    if (child.Hierarchy.ParentId != Index) {
      throw new ArgumentException($"{Name} does not contain Child {child.Name}");
    }

    return child;
  }

  /// <summary>
  /// Attempt to trigger the <see cref="State"/> of type <typeparamref name="T"/> to activate.
  /// <para />
  /// This method does nothing if the current instance's <see cref="Active"/> is <see langword="false"/>.
  /// <para />
  /// The transition will not occur if the target state's <see cref="State.CanEnter()"/> returns <see langword="false"/>.
  /// <para />
  /// This method will throw if the State Machine tree does not allow the transition,
  /// i.e. the target state is missing, or is not a child of any parent
  /// of the state which this method is called from.
  /// </summary>
  /// <typeparam name="T">
  /// The <see cref="State"/> of type <typeparamref name="T"/> to activate.
  /// This type is required to be an immediate child of a <see cref="StateMachine"/>.
  /// </typeparam>
  /// <returns>
  /// <see langword="true"/> if the active state was successfully changed; otherwise, <see langword="false"/>.
  /// </returns>
  /// <exception cref="ArgumentException">
  /// There is no parent which has a child of type <typeparamref name="T"/>.
  /// </exception>
  public bool TriggerState<T>() where T : State, new() {
    T state = GetState<T>();
    if (state.Parent is not StateMachine parent) {
      throw new ArgumentException($"The parent of child {state.Name} is not a StateMachine", nameof(T));
    }

    AnimCharacter? character = Hierarchy.ParentIds.Select(id => AllStates[id]).OfType<AnimCharacter>().FirstOrDefault();
    if (character is null) {
      throw new ArgumentException($"No parent of {Name} is an AnimCharacter", nameof(T));
    }

    if (!Active || Index == state.Index || !character.Active) {
      return false;
    }

    if (!parent.TrySetActiveChild(state)) {
      return false;
    }

    // Ensure that the chain of parents for the triggered state are active
    State parentState = parent;
    while (!parentState.Active && parentState.Index != Index) {
      if (parentState.Parent is StateMachine sm) {
        sm.TrySetActiveChild(parentState);
      }
      else {
        parentState.SetActive(true);
      }

      parentState = parentState.Parent!;
    }

    return true;
  }

  /// <summary>
  /// Determines the frame of animation to play for the current character state.
  /// </summary>
  /// <returns></returns>
  /// <remarks>
  /// For simple animations, all that is needed is
  /// <para><c>
  /// protected override AnimationOptions? GetAnimationOptions() => new("MyAnimationName");
  /// </c></para>
  /// More complex animations may modify the various properties of <see cref="AnimationOptions"/>.
  /// </remarks>
  protected virtual AnimationOptions? GetAnimationOptions() => null;

  /// <summary>
  /// Whether to not call <see cref="State.GetAnimationOptions"/> of this <see cref="ActiveChildren"/>.
  /// <br /> If there are no children, this method has no effect.
  /// </summary>
  /// <returns>
  /// <see langword="true"/> to use this instance's <see cref="State.GetAnimationOptions"/>,
  /// <br /><see langword="false"/> to use <see cref="ActiveChildren"/>'s <see cref="State.GetAnimationOptions"/>.
  /// <br />By default, returns <see langword="false"/>.
  /// </returns>
  protected virtual bool BlockChildAnimation() => false;

  public override string ToString() => Name;

  /// <summary>
  /// Enter logic to set <see cref="ActiveTime"/> to 0,
  /// call this <see cref="OnEnter"/> and children <see cref="Enter"/>.
  /// Any overrides should still make those calls.
  /// </summary>
  internal virtual void Enter(State? fromState) {
    ActiveTime = 0;
    InactiveTime = 0;
    OnEnter(fromState);

    foreach (State child in ActiveChildren) {
      child.Enter(fromState);
    }
  }

  /// <summary>
  /// Exit logic to call children <see cref="Exit"/>, this <see cref="OnExit"/>,
  /// and set <see cref="ActiveTime"/> to 0.
  /// Currently, no reason to allow overrides.
  /// </summary>
  internal virtual void Exit(State? toState) {
    OnExit(toState);

    foreach (State child in ActiveChildren) {
      child.Exit(toState);
    }

    ActiveTime = 0;
    InactiveTime = 0;
  }

  internal virtual AnimationOptions? GetAnimationOptionsInternal() {
    State? child = BlockChildAnimation() ? null : ActiveChildren.FirstOrDefault();
    return child?.GetAnimationOptionsInternal() ?? GetAnimationOptions();
  }
}

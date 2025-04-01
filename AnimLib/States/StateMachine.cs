using System.Linq;
using AnimLib.Animations;

namespace AnimLib.States;

/// <summary>
/// <see cref="State"/> where up to one child State is active at a time.
/// Supports transitions to and from children States.
/// Includes logic for <see cref="State.RegisterInterruptibles"/> and <see cref="TrySetActiveChild"/>
/// <para/> The first child added with <see cref="State.RegisterChildren"/> will be the <see cref="ActiveChild"/>
/// when <see cref="Enter"/> is used.
/// </summary>
public abstract partial class StateMachine : State {
  /// <summary>
  /// The current active child instance to receive game updates, -or-
  /// <see langword="null"/> if this instance has no active child instance.
  /// Setting this value will call <see cref="State.Exit"/> on the previous child,
  /// and <see cref="State.Enter"/> on the new child.
  /// </summary>
  public State? ActiveChild { get; private set; }

  /// <summary>
  /// Whether to set <see cref="ActiveChild"/> to the first child of <see cref="State.Children"/> when <see cref="Enter"/> is called.
  /// <para/> Set this value to <see langword="false"/> if you want to start this state without any active state.
  /// </summary>
  protected virtual bool SetActiveChildOnEnter => true;

  /// <summary>
  /// Overridden to represent up to one active child at a time.
  /// </summary>
  public sealed override IEnumerable<State> ActiveChildren {
    get {
      if (ActiveChild is not null) {
        yield return ActiveChild;
      }
    }
  }


  private State? _initialChild;

  private readonly Dictionary<ushort, State[]> _interrupts = [];


  protected override bool IsChildActiveToThis(State child) =>
    ActiveChild is not null && ActiveChild.Index == child.Index;

  public override void Initialize() {
    // Additionally set first child to _initialChild
    if (SetActiveChildOnEnter) {
      _initialChild ??= Children.FirstOrDefault();
    }

    var interruptIds = Hierarchy.ChildrenInterruptibleIds;
    foreach ((ushort fromState, ushort[]? toStates) in interruptIds) {
      _interrupts[fromState] = toStates.Select(i => AllStates[i]).ToArray();
    }
  }

  internal sealed override void Enter(State? fromState) {
    if (SetActiveChildOnEnter) {
      _initialChild ??= Children.FirstOrDefault();
    }

    ActiveTime = 0;
    InactiveTime = 0;
    if (ActiveChild is null && _initialChild is not null) {
      ActiveChild = _initialChild;
    }

    OnEnter(fromState);
    ActiveChild?.SetActive(true);
  }

  internal void UpdateInterruptChildren() {
    foreach ((ushort id, var interruptStates) in _interrupts) {
      State child = GetState(id);
      if (!child.Active) {
        continue;
      }

      foreach (State interruptState in interruptStates.Where(state =>
                 state is not AbilityState ability || ability.Unlocked)) {
        if (!interruptState.UpdateInterrupt(child) || !interruptState.CanEnter()) {
          continue;
        }

        ActiveChild?.SetActive(false, interruptState);
        ActiveChild = interruptState;
        ActiveChild.SetActive(true, child);
        NetUpdate = true;
        return;
      }
    }
  }

  protected bool TrySetActiveChild<T>(bool checkTransition = true, bool silent = false) where T : State, new() {
    return TrySetActiveChild(GetState<T>(), checkTransition, silent);
  }

  /// <summary>
  /// Attempt to set the <see cref="ActiveChild"/> to the current child.
  /// This method will do nothing if <see cref="ActiveChild"/> and <paramref name="toChild"/> are the same.
  /// </summary>
  /// <param name="toChild">
  /// The child to attempt to set <see cref="ActiveChild"/> to.
  /// </param>
  /// <param name="checkTransition">
  /// Whether to require
  /// <see cref="State.CanTransitionFrom"/>,
  /// and <see cref="State.CanEnter"/> be checked.
  /// The change will not occur if any of those methods return false.
  /// </param>
  /// <param name="silent">
  /// Whether to prevent NetUpdate if this method succeeds.
  /// </param>
  protected internal bool TrySetActiveChild(State toChild, bool checkTransition = true, bool silent = false) {
    ArgumentNullException.ThrowIfNull(toChild);

    // Ensure that the specified instance is part of AllStates, and not a template instance.
    toChild = AllStates[toChild.Index];

    State? lastActiveChild = ActiveChild;
    if (ReferenceEquals(lastActiveChild, toChild)) {
      return false;
    }

    if (!Hierarchy.ChildrenIds.Contains(toChild.Index)) {
      throw new ArgumentException($"{Name} does not contain child {toChild.Name}");
    }

    if (checkTransition && !CanTransition(toChild, lastActiveChild)) {
      return false;
    }

    SetActiveChild(toChild, silent, lastActiveChild);
    return true;
  }

  private void SetActiveChild(State toChild, bool silent, State? lastActiveChild) {
    lastActiveChild?.SetActive(false, toChild);
    ActiveChild = toChild;
    toChild.SetActive(true, lastActiveChild);
    if (!silent) {
      NetUpdate = true;
    }
  }

  private static bool CanTransition(State toChild, State? lastActiveChild) {
    return lastActiveChild is null || (toChild.CanEnter() && toChild.CanTransitionFrom(lastActiveChild));
  }

  public void ClearActiveChild(bool silent = false) {
    if (ActiveChild is null) {
      return;
    }

    ActiveChild.SetActive(false);
    ActiveChild = null;
    if (!silent) {
      NetUpdate = true;
    }
  }

  public override AnimationOptions? GetAnimationOptions() {
    return ActiveChild?.GetAnimationOptions() ?? null;
  }
}

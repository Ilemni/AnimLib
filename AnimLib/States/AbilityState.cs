using AnimLib.Menus.Debug;
using AnimLib.UI.Debug;

namespace AnimLib.States;

/// <summary>
/// <see cref="State"/> with additional logic such as Leveling and Cooldown.
/// <para/> This class also includes <see cref="SaveData"/>/<see cref="LoadData"/> functionality
/// <para/> This class requires that <see cref="State.Entity"/> is of type <see cref="Player"/>.
/// </summary>
public abstract partial class AbilityState : State {
  /// <summary>
  /// Current level of the Ability on this <see cref="Player"/>.
  /// If this is 0, the ability is considered locked and cannot be used.
  /// </summary>
  public int Level {
    get => _level;
    set => _level = Math.Clamp(value, 0, MaxLevel);
  }

  /// <summary>
  /// The maximum level this Ability may reach through normal gameplay.
  /// </summary>
  public virtual int MaxLevel => 1;

  /// <summary>
  /// Whether the value of <see cref="Level"/> is equal to <see cref="MaxLevel"/>.
  /// </summary>
  public bool IsMaxLevel => Level >= MaxLevel;

  /// <summary>
  /// Whether the value of <see cref="Level"/> is at least 1.
  /// </summary>
  public bool Unlocked => Level >= 1;


  /// <summary>
  /// The value which <see cref="CooldownLeft"/> will be set to when <see cref="StartCooldown"/> is called.
  /// By default, 0.
  /// </summary>
  public virtual int MaxCooldown => 0;

  /// <summary>
  /// Whether this ability uses cooldown features.
  /// <br/> By default, returns <see langword="true"/> if <see cref="MaxCooldown"/> is greater than 0.
  /// <br/> Set this to <see langword="true"/> if some other condition prevents this ability from leaving cooldown.
  /// <para/> As an example, an airborne ability may have no cooldown timer,
  /// but may still stay on cooldown until the player touches the ground.
  /// </summary>
  /// <remarks>
  /// Currently this is only used in the Debug UI, and doesn't affect gameplay.
  /// </remarks>
  public virtual bool SupportsCooldown => MaxCooldown > 0;

  /// <summary>
  /// Whether this ability should start its cooldown upon entering this state.
  /// <br/> By default, <see langword="false"/>.
  /// </summary>
  protected virtual bool StartCooldownOnEnter => false;

  /// <summary>
  /// Whether this ability should start its cooldown upon exiting this state.
  /// <br/> By default, <see langword="false"/>.
  /// </summary>
  protected virtual bool StartCooldownOnExit => false;

  /// <summary>
  /// Remaining time until this ability may be used again. This is set to <see cref="MaxCooldown"/>
  /// when <see cref="StartCooldown"/> is called.
  /// The ability may still be unusable if <see cref="CanRefresh"/> returns <see langword="false"/>
  /// despite being cooled down.
  /// </summary>
  public int CooldownLeft {
    get => _cooldownLeft;
    private set => _cooldownLeft = value;
  }

  /// <summary>
  /// Whether the ability is on cooldown and cannot be used.
  /// <br/> May return <see langword="true"/> even when <see cref="CooldownLeft"/> is <see langword="0"/>,
  /// if <see cref="CanRefresh"/> has only returned <see langword="false"/>.
  /// </summary>
  /// <remarks>
  /// On a multiplayer player which is not the local player, this will be false.
  /// </remarks>
  public bool IsOnCooldown {
    get => Player.whoAmI == Main.myPlayer && _isOnCooldown;
    private set => _isOnCooldown = value;
  }

  private int _level;
  private int _cooldownLeft;
  private bool _isOnCooldown;


  /// <summary>
  /// Whether this ability can be transitioned to, from the provided state.
  /// By default, returns <see langword="true"/> if
  /// <see cref="Unlocked"/> is <see langword="true"/>, and <see cref="IsOnCooldown"/> is <see langword="false"/>.
  /// </summary>
  public override bool CanEnter() => Unlocked && !IsOnCooldown;

  /// <summary>
  /// Whether this ability should go off cooldown.
  /// By default, returns <see langword="true"/> once <see cref="CooldownLeft"/> reaches 0,
  /// <see langword="false"/> otherwise.
  /// </summary>
  /// <param name="cooledDown">Whether <see cref="CooldownLeft"/> reached 0.</param>
  /// <returns>
  /// <see langword="true"/> to allow this ability to go off cooldown,
  /// <see langword="false"/> to keep this ability on cooldown.
  /// </returns>
  /// <remarks>
  /// This method is called for every <see cref="AbilityState"/> every update.
  /// </remarks>
  protected virtual bool CanRefresh(bool cooledDown) => cooledDown;

  /// <summary>
  /// Put the ability on cooldown.
  /// </summary>
  public void StartCooldown() {
    CooldownLeft = MaxCooldown;
    IsOnCooldown = true;
    OnStartCooldown();
  }

  /// <summary>
  /// End the cooldown of this ability, allowing it to be used again.
  /// </summary>
  /// <param name="force">
  /// Whether to trigger <see cref="OnEndCooldown(bool)"/> even if the ability was not on cooldown.
  /// This may be useful for abilities which were only partially used but have not yet gone on cooldown.
  /// </param>
  public void EndCooldown(bool force = false) {
    if (!force && !IsOnCooldown) {
      return;
    }

    bool wasOnCooldown = IsOnCooldown;

    CooldownLeft = 0;
    IsOnCooldown = false;
    OnEndCooldown(wasOnCooldown);
  }

  /// <summary>
  /// Called when <see cref="StartCooldown"/> is called.
  /// </summary>
  protected virtual void OnStartCooldown() {
  }

  /// <summary>
  /// Called when <see cref="EndCooldown"/> is called.
  /// </summary>
  /// <param name="justCooledDown">
  /// Whether this ability was previously on cooldown.
  /// This may be false if <see cref="EndCooldown(bool)"/> was called with force:<see langword="true"/>.
  /// </param>
  protected virtual void OnEndCooldown(bool justCooledDown) {
  }

  /// <summary>
  /// Calls <see cref="StartCooldown"/> if <see cref="MaxCooldown"/> is greater than 0.
  /// <inheritdoc cref="State.Enter"/>
  /// </summary>
  internal override void Enter(State? fromState) {
    if (StartCooldownOnEnter) {
      StartCooldown();
    }

    base.Enter(fromState);
  }

  internal override void Exit(State? toState) {
    if (StartCooldownOnExit) {
      StartCooldown();
    }

    base.Exit(toState);
  }

  internal void UpdateCooldown() {
    if (!IsOnCooldown) {
      return;
    }

    if (CooldownLeft > 0) {
      CooldownLeft--;
    }

    if (CanRefresh(CooldownLeft <= 0)) {
      EndCooldown();
    }
  }

  protected internal override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelValue("Level", Level, MaxLevel, color: Level > MaxLevel ? Color.Yellow : null);
    ui.DrawAppendLabelValue("Active Time:", ActiveTime);

    if (SupportsCooldown) {
      ui.DrawAppendLabelValue("Is Off Cooldown", !IsOnCooldown ? "Yes" : "No",
        IsOnCooldown ? DebugUIElement.Red : DebugUIElement.Green);
      if (MaxCooldown > 0) {
        ui.DrawAppendLabelProgressBar("Cooldown", CooldownLeft, MaxCooldown, new Color(191, 83, 83));
      }

      if (IsOnCooldown && CooldownLeft <= 0) {
        DebugCooldownReason(ui);
      }
    }
  }

  /// <summary>
  /// For abilities which override <see cref="CanRefresh"/> to return <see langword="false"/> when <c>cooledDown</c>,
  /// this method is called to provide additional debug information for <see cref="DebugText"/>.
  /// </summary>
  /// <param name="ui"></param>
  protected virtual void DebugCooldownReason(UIStateInfo ui) {
    ui.DrawAppendLine("Another condition prevents cooldown.", Color.LightGray);
  }
}

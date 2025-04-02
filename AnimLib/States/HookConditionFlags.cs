namespace AnimLib.States;

/// <summary>
/// Enum representing the conditions under which a hook should be called.
/// </summary>
[Flags]
public enum HookConditionFlags {
  /// <summary>
  /// Indicates that the hook has no conditions and will always be called.
  /// </summary>
  Always = 0,

  /// <summary>
  /// Indicates that the hook requires the <see cref="State"/> to be <see cref="State.Active"/>.
  /// </summary>
  SelfActive = 1,

  /// <summary>
  /// Indicates that the hook requires the <see cref="State.Character"/> to be <see cref="State.Active"/>.
  /// </summary>
  CharacterActive = 2,

  /// <summary>
  /// Indicates that the hook requires the <see cref="AbilityState"/> to be <see cref="AbilityState.Unlocked"/>.
  /// <br/> This condition is ignored if the <see cref="State"/> is not an <see cref="AbilityState"/>.
  /// </summary>
  AbilityUnlocked = 4,

  /// <summary>
  /// Indicates that the hook requires the <see cref="State"/> instance to be on the <see cref="Main.LocalPlayer"/>.
  /// <br/> This condition has no effect on methods which are already not called on the client.
  /// </summary>
  LocalPlayer = 8,

  /// <summary>
  /// The default condition for most hooks, which requires the <see cref="State.Character"/> to be <see cref="State.Active"/>.
  /// <br/> If this instance is an <see cref="AbilityState"/>, this condition also requires it to be <see cref="AbilityState.Unlocked"/>.
  /// </summary>
  Default = CharacterActive | AbilityUnlocked,

  /// <summary>
  /// Indicates that the hook requires the <see cref="State"/> to be <see cref="State.Active"/>.
  /// <br/> If this instance is an <see cref="AbilityState"/>, this condition also requires it to be <see cref="AbilityState.Unlocked"/>.
  /// </summary>
  Strict = SelfActive | AbilityUnlocked
}

namespace AnimLib.Abilities {
  /// <summary>
  /// States that the <see cref="Ability"/> can be in. Determines update logic.
  /// </summary>
  public enum AbilityState : byte {
    /// <summary>
    /// The ability will not perform any Update logic.
    /// </summary>
    Inactive = 0,

    /// <summary>
    /// The ability will use <see cref="Ability.UpdateStarting"/> and <see cref="Ability.UpdateUsing"/>.
    /// </summary>
    Starting = 1,

    /// <summary>
    /// The ability will use <see cref="Ability.UpdateActive"/> and <see cref="Ability.UpdateUsing"/>.
    /// </summary>
    Active = 2,

    /// <summary>
    /// The ability will use <see cref="Ability.UpdateEnding"/> and <see cref="Ability.UpdateUsing"/>.
    /// </summary>
    Ending = 3
  }
}

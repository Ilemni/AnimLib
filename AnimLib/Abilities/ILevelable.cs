using JetBrains.Annotations;

namespace AnimLib.Abilities {
  /// <summary>
  /// Interface for level data.
  /// </summary>
  [PublicAPI]
  public interface ILevelable {
    /// <summary>
    /// The level of the ability.
    /// </summary>
    int Level { get; set; }

    /// <summary>
    /// The intended max level of the ability.
    /// </summary>
    int MaxLevel { get; }
  }
}

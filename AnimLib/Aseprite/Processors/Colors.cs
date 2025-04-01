namespace AnimLib.Aseprite.Processors;

/// <summary>
/// Represents colors selectable in Aseprite for a UserData. A sprite, layer, tag, or cel can have a color.
/// </summary>
internal static class Colors {
  /// <summary>
  /// The Red color as shown in the Color selection for a UserData.
  /// <br/> This value is equal to <c>new Rgba32(254, 91, 89).PackedValue</c>.
  /// </summary>
  internal const uint Red = 254u | (91u << 8) | (89u << 16) | (255u << 24);


  /// <summary>
  /// The Orange color as shown in the Color selection for a UserData.
  /// <br/> This value is equal to <c>new Rgba32(247, 165, 71).PackedValue</c>.
  /// </summary>
  internal const uint Orange = 247u | (165u << 8) | (71u << 16) | (255u << 24);


  /// <summary>
  /// The Yellow color as shown in the Color selection for a UserData.
  /// <br/> This value is equal to <c>new Rgba32(243, 206, 82).PackedValue</c>.
  /// </summary>
  internal const uint Yellow = 243u | (206u << 8) | (82u << 16) | (255u << 24);


  /// <summary>
  /// The Green color as shown in the Color selection for a UserData.
  /// <br/> This value is equal to <c>new Rgba32(106, 205, 91).PackedValue</c>.
  /// </summary>
  internal const uint Green = 106u | (205u << 8) | (91u << 16) | (255u << 24);


  /// <summary>
  /// The Blue color as shown in the Color selection for a UserData.
  /// <br/> This value is equal to <c>new Rgba32(87, 185, 242).PackedValue</c>.
  /// </summary>
  internal const uint Blue = 87u | (185u << 8) | (242u << 16) | (255u << 24);


  /// <summary>
  /// The Purple color as shown in the Color selection for a UserData.
  /// <br/> This value is equal to <c>new Rgba32(209, 134, 223).PackedValue</c>.
  /// </summary>
  internal const uint Purple = 209u | (134u << 8) | (223u << 16) | (255u << 24);


  /// <summary>
  /// The Gray color as shown in the Color selection for a UserData.
  /// <br/> This value is equal to <c>new Rgba32(165, 165, 167).PackedValue</c>.
  /// </summary>
  internal const uint Gray = 165u | (165u << 8) | (167u << 16) | (255u << 24);
}

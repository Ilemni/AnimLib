using System.Runtime.CompilerServices;
using AnimLib.States;

namespace AnimLib;

public static class ModPlayerExtensions {
  /// <summary>
  /// Gets the <see cref="AnimCharacter"/> of type <typeparamref name="T"/>
  /// on the specified <paramref name="player"/>.
  /// </summary>
  /// <remarks>
  /// This is functionally identical to <see cref="GetState{T}(Player)"/>
  /// with <see cref="AnimCharacter"/> as the type parameter,
  /// and is included for readability.
  /// </remarks>
  public static T GetCharacter<T>(this Player player) where T : AnimCharacter, new() => player.GetAnimPlayer().GetState<T>();

  /// <summary>
  /// Gets the <see cref="State"/> of type <typeparamref name="T"/>
  /// on the specified <paramref name="player"/>.
  /// </summary>
  public static T GetState<T>(this Player player) where T : State, new() => player.GetAnimPlayer().GetState<T>();

  /// <summary>
  /// Get the <see cref="State"/> of base type <typeparamref name="T"/>,
  /// whose concrete type's index matches the specified <paramref name="index"/>.
  /// </summary>
  /// <param name="player"></param>
  /// <param name="index"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public static T GetState<T>(this Player player, int index) where T : State => player.GetAnimPlayer().GetState<T>(index);

  /// <summary>
  /// Gets the <see cref="State"/> whose type matches the specified <paramref name="template"/>
  /// </summary>
  public static State GetState(this Player player, State template) => player.GetAnimPlayer().GetState(template);

  /// <summary>
  /// Gets the <see cref="State"/> with the specified <paramref name="index"/>
  /// </summary>
  public static State GetState(this Player player, int index) => player.GetAnimPlayer().GetState(index);

  public static AnimCharacter? GetActiveCharacter(this Player player) => player.GetState<AnimCharacterCollection>().ActiveCharacter;

  internal static State[] GetStates(this Player player) => player.GetAnimPlayer().States;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static AnimPlayer GetAnimPlayer(this Player player) => player.GetModPlayer<AnimPlayer>();
}

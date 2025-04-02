using JetBrains.Annotations;

namespace AnimLib.States;

[AttributeUsage(AttributeTargets.Method)]
[PublicAPI]
public class HookConditionAttribute(HookConditionFlags conditions) : Attribute {
  public HookConditionFlags Conditions { get; private set; } = conditions;

  public bool RequireSelfActive => (Conditions & HookConditionFlags.SelfActive) != 0;

  public bool RequireCharacterActive => (Conditions & HookConditionFlags.CharacterActive) != 0;

  public bool RequireAbilityUnlocked => (Conditions & HookConditionFlags.AbilityUnlocked) != 0;

  public bool RequireLocalPlayer => (Conditions & HookConditionFlags.LocalPlayer) != 0;

  // Virtual in case some mod wants a MyHookConditionAttribute
  /// <summary>
  /// Determines whether the hook should be called based on the current state.
  /// </summary>
  /// <param name="state">The state to evaluate.</param>
  public virtual bool Evaluate(State state) {
    if (Conditions is HookConditionFlags.Always) {
      return true;
    }

    if (RequireSelfActive && !state.Active) {
      return false;
    }

    if (RequireCharacterActive && state.Character is { Active: false }) {
      return false;
    }

    if (RequireAbilityUnlocked && state is AbilityState { Unlocked: false }) {
      return false;
    }

    if (RequireLocalPlayer && !state.IsLocal) {
      return false;
    }

    return true;
  }
}

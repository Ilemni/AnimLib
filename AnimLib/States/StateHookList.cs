using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.ModLoader.Core;

namespace AnimLib.States;

/// <summary>
/// Like <see cref="HookList{T}"/>, but for <see cref="State"/>s.
/// <br/> Enumeration is additionally filtered by requiring the enumerated <see cref="State"/> methods
/// to meet certain conditions, specified by the attribute <see cref="HookConditionAttribute"/>.
/// </summary>
public sealed class StateHookList(HookList<State> hookList) {
  public readonly HookList<State> HookList = hookList;
  public ReadOnlySpan<HookConditionAttribute?> StateHookConditions => _stateHookConditions;
  private HookConditionAttribute?[] _stateHookConditions = [];

  public void Update(IReadOnlyList<State> templateStates) {
    HookList.Update(templateStates);
    var conditionsList = new List<HookConditionAttribute?>();

    var binder = HookList.HookOverrideQuery.Binder;
    foreach (State state in HookList.Enumerate()) {
      MethodInfo? method = binder(state)?.Method;
      if (HasOverride(method)) {
        conditionsList.Add(method.GetCustomAttributes<HookConditionAttribute>().FirstOrDefault());
      }
    }

    _stateHookConditions = conditionsList.ToArray();
  }

  public bool HasOverride(State state) => HasOverride(HookList.HookOverrideQuery.Binder(state)?.Method);

  private bool HasOverride([NotNullWhen(true)] MethodInfo? method) {
    return method is not null && method != HookList.Method;
  }

  public static StateHookList Create<T>(Expression<Func<State, T>> func) where T : Delegate {
    var hookList = HookList<State>.Create(func);
    return new StateHookList(hookList);
  }
}

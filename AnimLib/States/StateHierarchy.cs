using System.Linq;
using System.Text;

namespace AnimLib.States;

/// <summary>
/// Represents the connections between a <see cref="State"/> and its parent and children states.
/// </summary>
public sealed class StateHierarchy : IComparable<StateHierarchy> {
  /// <summary>
  /// Index of the <see cref="State"/>
  /// </summary>
  public ushort Index { get; private init; }

  /// <summary>
  /// Index of the Parent <see cref="State"/>
  /// </summary>
  public int ParentId { get; private set; } = -1;

  /// <summary>
  /// Index of the Parent <see cref="State"/>s, ordered from immediate parent to the root parent.
  /// </summary>
  public ushort[] ParentIds { get; private set; } = [];

  /// <summary>
  /// Index of all States in which this <see cref="State"/> is the Parent state.
  /// </summary>
  public ushort[] ChildrenIds { get; private set; } = [];

  /// <summary>
  /// Index of all States in which this <see cref="State"/>'s <see cref="Index"/>
  /// is contained in its <see cref="ParentIds"/>.
  /// </summary>
  public ushort[] AllChildrenIds { get; private set; } = [];

  public Dictionary<ushort, ushort[]> ChildrenInterruptibleIds { get; } = [];

  /// <summary>
  /// Index of all <see cref="State"/>s, in which, if any of those <see cref="State"/>s are active,
  /// this <see cref="State"/> can interrupt them.
  /// </summary>
  public ushort[] InterruptibleIds { get; private set; } = [];

  public int CompareTo(StateHierarchy? b) {
    if (b is null) {
      return -1;
    }

    ushort indexA = Index;
    ushort indexB = b.Index;

    if (ParentId == b.ParentId) {
      // Siblings
      return indexA.CompareTo(indexB);
    }

    if (ParentIds.Contains(indexB)) {
      return 1;
    }

    if (b.ParentIds.Contains(indexA)) {
      return -1;
    }

    foreach (ushort idA in ParentIds) {
      if (StateLoader.TemplateHierarchy[idA].ParentIds.Contains(indexB)) {
        return 1;
      }
    }

    foreach (ushort idB in b.ParentIds) {
      if (StateLoader.TemplateHierarchy[idB].ParentIds.Contains(indexA)) {
        return -1;
      }
    }

    return indexA.CompareTo(indexB);
  }

  public override string ToString() {
    return _toStringResult ??= ToStringCompute();
  }

  private string ToStringCompute() {
    StringBuilder sb = new();
    sb.Append($"State {StateLoader.TemplateStates[Index].Name} ({Index});");
    if (ParentId != -1) {
      sb.Append($" Parent: {IdToName((ushort)ParentId)};");
    }

    if (ChildrenIds.Length > 0) {
      sb.Append($" Children: {string.Join(',', ChildrenIds.Select(IdToName))};");
    }

    if (ChildrenInterruptibleIds.Count > 0) {
      sb.Append(" Interruptibles: ");
      foreach ((ushort key, ushort[] value) in ChildrenInterruptibleIds.OrderBy(kvp => kvp.Key)) {
        sb.Append($"{IdToName(key)} -> [{string.Join(',', value.Select(IdToName))}] ");
      }
    }

    return sb.ToString();

    static string IdToName(ushort i) => StateLoader.TemplateStates[i].Name;
  }

  private string? _toStringResult;

  // Named as such because tModLoader's ResizeArrays would be the perfect time for this method to be used.
  /// <summary>
  /// This calls <see cref="State.RegisterChildren"/> on each state in <paramref name="templateStates"/>
  /// </summary>
  /// <param name="templateStates"></param>
  public static StateHierarchy[] ResizeArrays(IList<State> templateStates) {
    var templateStatesArray = templateStates.ToArray();
    var hierarchies = new StateHierarchy[templateStatesArray.Length];

    // Initialize hierarchies, setup templateStates
    foreach (State templateState in templateStatesArray) {
      StateHierarchy hierarchy = new() {
        Index = templateState.Index
      };
      hierarchies[templateState.Index] = hierarchy;
      templateState.AllStatesArray = templateStatesArray;
      templateState.Hierarchy = hierarchy;
    }

    List<State> childrenStatesToAdd = [];
    foreach (State templateState in templateStatesArray) {
      // Assigns to ChildrenIds and children ParentId
      SetupChildren(templateState, childrenStatesToAdd);
    }

    List<ushort> parentIds = [];
    foreach (StateHierarchy hierarchy in hierarchies) {
      // Assigns to ParentIds array
      SetupParentIds(hierarchy, hierarchies, parentIds);
    }

    foreach (StateHierarchy hierarchy in hierarchies) {
      // Assigns to AllChildrenIds
      hierarchy.AllChildrenIds = hierarchies
        .Where(h => h.ParentIds.Contains(hierarchy.Index))
        .Select(h => h.Index).ToArray();
    }

    var interruptibleDict = new Dictionary<ushort, List<ushort>>?[templateStatesArray.Length];
    foreach (State state in templateStatesArray) {
      // Assigns to InterruptibleIds
      SetupInterruptibles(state, hierarchies, childrenStatesToAdd, interruptibleDict);
    }

    for (int i = 0; i < interruptibleDict.Length; i++) {
      // Assigns to ChildrenInterruptibleIds
      var dict = interruptibleDict[i];
      if (dict is null) {
        continue;
      }

      StateHierarchy hierarchy = hierarchies[i];
      foreach ((ushort key, var value) in dict) {
        hierarchy.ChildrenInterruptibleIds[key] = value.ToArray();
      }
    }

    return hierarchies;
  }

  private static void SetupChildren(State parent, List<State> childrenToAdd) {
    parent.RegisterChildren(childrenToAdd);
    if (childrenToAdd.Count <= 0) {
      return;
    }

    parent.Hierarchy.ChildrenIds = childrenToAdd.Select(s => s.Index).ToArray();
    foreach (State child in childrenToAdd) {
      if (child.Hierarchy.ParentId != -1) {
        throw new InvalidOperationException(
          $"State {child.Name} is already a child of {parent.Name}");
      }

      child.Hierarchy.ParentId = parent.Index;
    }

    childrenToAdd.Clear();
  }

  private static void SetupParentIds(StateHierarchy hierarchy, StateHierarchy[] hierarchies, List<ushort> parentIds) {
    int parentId = hierarchy.ParentId;
    while (parentId != -1) {
      parentIds.Add((ushort)parentId);
      parentId = hierarchies[parentId].ParentId;
    }

    if (parentIds.Count > 0) {
      hierarchy.ParentIds = parentIds.ToArray();
      parentIds.Clear();
    }
  }

  private static void SetupInterruptibles(State state, StateHierarchy[] hierarchies, List<State> statesToAdd,
    Dictionary<ushort, List<ushort>>?[] interruptibleDict) {
    state.RegisterInterruptibles(statesToAdd);
    if (statesToAdd.Count <= 0) {
      return;
    }

    hierarchies[state.Index].InterruptibleIds = statesToAdd.Select(s => s.Index).ToArray();

    foreach (State toInterrupt in statesToAdd) {
      ushort[] parentIds = hierarchies[state.Index].ParentIds;
      ushort[] otherParentIds = hierarchies[toInterrupt.Index].ParentIds;
      ushort commonParent = parentIds.Intersect(otherParentIds).FirstOrDefault(ushort.MaxValue);
      if (commonParent == ushort.MaxValue) {
        throw new InvalidOperationException($"State {toInterrupt.Name} is not a child of any parent of {state.Name}");
      }

      var dict = interruptibleDict[commonParent] ??= [];
      if (!dict.TryGetValue(toInterrupt.Index, out var list)) {
        dict[toInterrupt.Index] = list = [];
      }

      if (list.Contains(state.Index)) {
        throw new InvalidOperationException(
          $"Parent {StateLoader.TemplateStates[commonParent].Name}: State {state.Name} is already an interruptible of {toInterrupt.Name}");
      }

      list.Add(state.Index);
    }

    statesToAdd.Clear();
  }
}

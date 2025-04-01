using AnimLib.States;

namespace AnimLib.UI.Elements;

public interface IStateUIElement : IComparable<IStateUIElement> {
  public State? State { get; }
  public StateHierarchy? Hierarchy => State?.Hierarchy;

  int IComparable<IStateUIElement>.CompareTo(IStateUIElement? other) {
    return (Hierarchy, other?.Hierarchy) switch {
      (null, null) => 0,
      (null, _) => -1,
      (_, null) => 1,
      var (a, b) => a.CompareTo(b)
    };
  }
}

public interface IStateUIElement<out T> : IStateUIElement where T : State {
  public new T? State => ((IStateUIElement)this).State as T;
}

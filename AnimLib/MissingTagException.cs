namespace AnimLib;

public class MissingTagException(string tag) : Exception {
  public string Tag { get; } = tag;

  public override string Message { get; } = $"Animation with name \"{tag}\" not found.";
}

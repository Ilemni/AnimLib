namespace AnimLib;

public class MissingLayerException(string layer) : Exception {
  public string Layer { get; } = layer;

  public override string Message { get; } = $"Layer with name \"{layer}\" not found.";
}

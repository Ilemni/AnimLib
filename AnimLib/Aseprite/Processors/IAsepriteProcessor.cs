using AsepriteDotNet.Aseprite;

namespace AnimLib.Aseprite.Processors;

public interface IAsepriteProcessor<out T> where T : class {
  /// <summary>
  /// Create an instance of <typeparamref name="T"/> from the provided <paramref name="file"/>.
  /// </summary>
  public T Process(AsepriteFile file, AnimProcessorOptions options);
}

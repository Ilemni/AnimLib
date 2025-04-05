using System.Threading.Tasks;
using AsepriteDotNet.Aseprite;

namespace AnimLib.Aseprite.Processors;

public interface IAsepriteProcessor<T> where T : class {
  ValueTask<T> Process(AsepriteFile file, AnimProcessorOptions options, MainThreadCreationContext mainThreadCtx) => ValueTask.FromResult(Process(file, options));

  protected T Process(AsepriteFile file, AnimProcessorOptions options) => throw new NotImplementedException();
}

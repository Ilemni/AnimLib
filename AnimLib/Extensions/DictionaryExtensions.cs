using System.Runtime.InteropServices;

namespace AnimLib.Extensions;

public static class DictionaryExtensions {
  public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
    where TKey : notnull
    where TValue : class, new() {
    ref TValue? result = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out bool exists);
    if (!exists) {
      result = new TValue();
    }
    return result!;
  }
}

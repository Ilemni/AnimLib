using System;
using JetBrains.Annotations;

namespace AnimLib {
  /// <summary>
  /// Class used to hold a single static reference to an instance of <typeparamref name="T"/>.
  /// <para>Classes inheriting from this should use a private constructor.</para>
  /// </summary>
  /// <typeparam name="T">The type to make Singleton.</typeparam>
  [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
  public abstract class SingleInstance<T> where T : SingleInstance<T> {
    // ReSharper disable once StaticMemberInGenericType
    private static readonly object _lock = new object();
    private static T _instance;

    /// <summary>
    /// The singleton instance of this type.
    /// </summary>
    public static T Instance {
      get {
        if (_instance is null) Initialize();
        return _instance;
      }
    }

    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> if it does not already exist, and returns the instance.
    /// </summary>
    /// <returns>The value of <see cref="Instance"/>.</returns>
    public static T Initialize() {
      if (!(_instance is null)) return _instance;
      lock (_lock) {
        if (!(_instance is null)) return _instance;
        _instance = (T)Activator.CreateInstance(typeof(T), true);
        AnimLibMod.OnUnload += Unload;
      }

      return Instance;
    }

    /// <summary>
    /// Sets the static reference of <see cref="SingleInstance{T}"/> to <see langword="null"/>.
    /// Calls <see cref="IDisposable.Dispose"/> first, if applicable.
    /// </summary>
    private static void Unload() {
      // ReSharper disable once SuspiciousTypeConversion.Global
      if (_instance is IDisposable disposable) disposable.Dispose();
      _instance = null;
    }
  }
}

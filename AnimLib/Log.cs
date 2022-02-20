using System;

namespace AnimLib {
  internal static class Log {
    internal static void LogError(string message, Exception ex) => AnimLibMod.Instance.Logger.Error(message, ex);

    internal static void LogWarning(string message) => AnimLibMod.Instance.Logger.Warn(message);

    internal static void LogDebug(string message) {
      if (AnimPlayer.Local?.DebugEnabled ?? false) AnimLibMod.Instance.Logger.Debug(message);
    }

    internal static void LogInfo(string message) => AnimLibMod.Instance.Logger.Info(message);
  }
}

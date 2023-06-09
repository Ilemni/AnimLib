using System;

namespace AnimLib {
  internal static class Log {
    internal static void Error(string message, Exception ex) => AnimLibMod.Instance.Logger.Error(message, ex);

    internal static void Warn(string message) => AnimLibMod.Instance.Logger.Warn(message);

    internal static void Debug(string message) {
      if (AnimPlayer.Local?.DebugEnabled ?? false) AnimLibMod.Instance.Logger.Debug(message);
    }

    internal static void Info(string message) => AnimLibMod.Instance.Logger.Info(message);
  }
}

using System.Diagnostics;

namespace AnimLib;

internal static class Log {
  internal static void Error(string message, Exception ex) {
    AnimLibMod.Instance.Logger.Error(message, ex);
  }

  internal static void Warn(string message) {
    AnimLibMod.Instance.Logger.Warn(message);
  }

  [Conditional("DEBUG")]
  internal static void Debug(string message) {
    AnimLibMod.Instance.Logger.Debug(message);
    if (Program.IsMainThread) {
      Main.NewText(message, Color.Gray);
    }
  }

  internal static void Info(string message) {
    AnimLibMod.Instance.Logger.Info(message);
  }

  internal static bool IsErrorEnabled => AnimLibMod.Instance.Logger.IsErrorEnabled;
  internal static bool IsWarnEnabled => AnimLibMod.Instance.Logger.IsWarnEnabled;
  internal static bool IsDebugEnabled => AnimLibMod.Instance.Logger.IsDebugEnabled;
  internal static bool IsInfoEnabled => AnimLibMod.Instance.Logger.IsInfoEnabled;
}

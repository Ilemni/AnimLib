using System;

namespace AnimLib {
  internal static class Utils {
    /// <summary>
    /// Returns the name of the <see cref="Type"/> if it is not equal to the given <see cref="string"/>, otherwise returns the full name of the <see cref="Type"/>.
    /// </summary>
    /// <param name="type">Type to get the name or full name of.</param>
    /// <param name="compareTo">String to compare against the name.</param>
    /// <returns>The <see cref="Type"/>'s name, or its full name if the name matches the given <see cref="string"/>.</returns>
    public static string SafeTypeName(this Type type, string compareTo) {
      return type.Name != compareTo ? type.Name : type.FullName;
    }
  }
}

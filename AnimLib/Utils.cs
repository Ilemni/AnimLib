using System;

namespace AnimLib {
  internal static class Utils {
    /// <summary>
    /// Returns the name of the <see cref="Type"/> if it is not the same name as any of its base types; otherwise, return the full name of the
    /// <see cref="Type"/>.
    /// </summary>
    /// <param name="type">Type to get the name or full name of.</param>
    /// <returns>The <see cref="Type"/>'s name, or its full name if the name matches any of its base types.</returns>
    public static string UniqueTypeName(this Type type) {
      if (type is null) throw new ArgumentNullException(nameof(type));
      string name = type.Name;
      Type baseType = type;
      while ((baseType = baseType.BaseType) != null)
        if (baseType.Name == name)
          return type.FullName;

      return name;
    }
  }
}

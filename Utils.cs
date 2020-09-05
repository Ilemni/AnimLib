using System;
using System.Reflection;

namespace AnimLib {
  internal static class ReflectionUtils {
    /// <summary>
    /// Alternative version of <see cref="Type.IsSubclassOf"/> that supports raw generic types (generic types without
    /// any type parameters).
    /// </summary>
    /// <param name="baseType">The base type class for which the check is made.</param>
    /// <param name="toCheck">To type to determine for whether it derives from <paramref name="baseType"/>.</param>
    internal static bool IsSubclassOfRawGeneric(this Type toCheck, Type baseType) {
      if (toCheck is null || baseType is null || toCheck.IsInterface) {
        return false;
      }
      while (toCheck != typeof(object)) {
        Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
        if (baseType == cur) {
          return true;
        }

        toCheck = toCheck.BaseType;
      }

      return false;
    }

    /// <summary>
    /// Search for a method by name, parameter types, and binding flags.  
    /// Unlike <see cref="Type.GetMethod(string)"/>, this does 'loose' matching on generic
    /// parameter types, and searches base interfaces.
    /// </summary>
    /// <exception cref="AmbiguousMatchException"/>
    public static MethodInfo GetMethodExt(this Type thisType, string name, params Type[] parameterTypes) {
      return GetMethodExt(thisType, name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, parameterTypes);
    }

    /// <summary>
    /// Search for a method by name, parameter types, and binding flags.  
    /// Unlike <see cref="Type.GetMethod(string)"/>, this does 'loose' matching on generic
    /// parameter types, and searches base interfaces.
    /// </summary>
    /// <exception cref="AmbiguousMatchException"/>
    public static MethodInfo GetMethodExt(this Type thisType, string name, BindingFlags bindingFlags, params Type[] parameterTypes) {
      MethodInfo matchingMethod = null;

      // Check all methods with the specified name, including in base classes
      GetMethodExt(ref matchingMethod, thisType, name, bindingFlags, parameterTypes);

      // If we're searching an interface, we have to manually search base interfaces
      if (matchingMethod == null && thisType.IsInterface) {
        foreach (Type interfaceType in thisType.GetInterfaces()) {
          GetMethodExt(ref matchingMethod, interfaceType, name, bindingFlags, parameterTypes);
        }
      }

      return matchingMethod;
    }

    private static void GetMethodExt(ref MethodInfo matchingMethod, Type type, string name, BindingFlags bindingFlags, params Type[] parameterTypes) {
      // Check all methods with the specified name, including in base classes
      foreach (MethodInfo methodInfo in type.GetMember(name, MemberTypes.Method, bindingFlags)) {
        // Check that the parameter counts and types match, 
        // with 'loose' matching on generic parameters
        ParameterInfo[] parameterInfos = methodInfo.GetParameters();
        if (parameterInfos.Length == (parameterTypes?.Length ?? 0)) {
          int i = 0;
          for (; i < parameterInfos.Length; ++i) {
            if (!parameterInfos[i].ParameterType.IsSimilarType(parameterTypes[i])) {
              break;
            }
          }
          if (i == parameterInfos.Length) {
            if (matchingMethod != null) {
              throw new AmbiguousMatchException("More than one matching method found!");
            }
            matchingMethod = methodInfo;
          }
        }
      }
    }

    /// <summary>
    /// Special type used to match any generic parameter type in <see cref="GetMethodExt(Type, string, BindingFlags, Type[])"/>.
    /// </summary>
    internal class T { }

    /// <summary>
    /// Determines if the two types are either identical, or are both generic 
    /// parameters or generic types with generic parameters in the same
    /// locations (generic parameters match any other generic paramter,
    /// but NOT concrete types).
    /// </summary>
    private static bool IsSimilarType(this Type thisType, Type type) {
      // Ignore any 'ref' types
      if (thisType.IsByRef) {
        thisType = thisType.GetElementType();
      }
      if (type.IsByRef) {
        type = type.GetElementType();
      }

      // Handle array types
      if (thisType.IsArray && type.IsArray) {
        return thisType.GetElementType().IsSimilarType(type.GetElementType());
      }

      // If the types are identical, or they're both generic parameters 
      // or the special 'T' type, treat as a match
      if (thisType == type || (thisType.IsGenericParameter || thisType == typeof(T)) && (type.IsGenericParameter || type == typeof(T))) {
        return true;
      }

      // Handle any generic arguments
      if (thisType.IsGenericType && type.IsGenericType) {
        Type[] thisArguments = thisType.GetGenericArguments();
        Type[] arguments = type.GetGenericArguments();
        if (thisArguments.Length == arguments.Length) {
          for (int i = 0; i < thisArguments.Length; ++i) {
            if (!thisArguments[i].IsSimilarType(arguments[i])) {
              return false;
            }
          }
          return true;
        }
      }

      return false;
    }
  }
}

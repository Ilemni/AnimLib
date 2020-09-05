using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;

namespace AnimLib {
  /// <summary>
  /// Class for various utility methods used by AnimLib.
  /// </summary>
  public static class AnimUtils {
    #region Point Arithmetic
    /// <summary>
    /// Addition between two points.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="point">Point to add.</param>
    /// <returns></returns>
    internal static Point Add(this Point self, Point point) {
      Point p3;
      p3.X = self.X + point.X;
      p3.Y = self.Y + point.Y;
      return p3;
    }
    #endregion

    /// <summary>
    /// Readonly version of Normalize().
    /// </summary>
    internal static Vector2 Normalized(this Vector2 vect) {
      if (vect == Vector2.Zero) {
        return vect;
      }
      vect.Normalize();
      return vect;
    }

    /// <summary>
    /// Checks if any active <see cref="NPC"/>s are bosses.
    /// </summary>
    public static bool AnyBossAlive() {
      for (int i = 0, len = Main.npc.Length; i < len; i++) {
        var npc = Main.npc[i];
        if (npc.active && (npc.boss || Terraria.ID.NPCID.Sets.TechnicallyABoss[npc.type])) {
          return true;
        }
      }
      return false;
    }

    #region Distance Checking
    /// <summary>
    /// Gets the closest <see cref="Entity"/>. Returns true if any are in range, the closest <see cref="Entity"/> passed out as <paramref name="entity"/>.
    /// </summary>
    /// <param name="me">Y'know...</param>
    /// <param name="arr">Array of entities, such as <see cref="Main.player"/>, <see cref="Main.npc"/>, <see cref="Main.projectile"/>. These are the candidates for <paramref name="entity"/>.</param>
    /// <param name="distance">Maximum distance to be considered in-range. If passed in as 0 or negative, range is infinite.
    /// <para>If this method returns <see langword="true"/>, this value is the distance to <paramref name="entity"/>.</para>
    /// <para>If this method returns <see langword="false"/>, this value is not modified.</para>
    /// </param>
    /// <param name="entity">Closest <see cref="Entity"/> in-range, or <see langword="null"/> if no entities are in range.</param>
    /// <param name="distanceSquaredCheck">How the closest distance is checked. Defaults to `<see cref="DistanceBetweenTwoEntitiesSquared(Entity, Entity)"/>`, getting closest distance between two entities.</param>
    /// <param name="condition">Extra condition to filter out entities. If <paramref name="condition"/> returns false, the entity is skipped.</param>
    /// <typeparam name="T">Type of Entity (i.e. <see cref="Player"/>, <see cref="NPC"/>, <see cref="Projectile"/>).</typeparam>
    /// <returns><see langword="true"/> if there is an <see cref="Entity"/> closer than <paramref name="distance"/> (<paramref name="entity"/> is not <see langword="null"/>), otherwise <see langword="false"/>.</returns>
    internal static bool GetClosestEntity<T>(this Entity me, T[] arr, ref float distance, out T entity, Func<Entity, T, float> distanceSquaredCheck = null, Func<T, bool> condition = null) where T : Entity {
      // Setup method
      float startDistance = distance;
      if (distance <= 0) {
        distance = float.MaxValue; // Infinite range detect
      }
      else {
        distance *= distance; // Squared for DistanceSquared
      }
      if (distanceSquaredCheck is null) {
        distanceSquaredCheck = (e1, e2) => DistanceBetweenTwoEntitiesSquared(e1, e2);
      }

      // Search for closest entity
      int id = -1;
      for (int i = 0, len = arr.Length; i < len; i++) {
        T e = arr[i];
        if (e is null || !e.active || ReferenceEquals(e, me) || condition?.Invoke(e) == false) {
          continue;
        }

        float newDist = distanceSquaredCheck(me, e);
        if (newDist < distance) {
          distance = newDist;
          id = e.whoAmI;
        }
      }

      if (id != -1) {
        // Entity found
        distance = (float)Math.Sqrt(distance);
        entity = arr[id];
        return true;
      }

      // No entity found
      distance = startDistance;
      entity = null;
      return false;
    }

    /// <summary>
    /// Distance between the hitboxes of two entities. If the entity hitboxes overlap, this will return <see langword="0"/>.
    /// </summary>
    /// <param name="entity1">First entity.</param>
    /// <param name="entity2">Second entity.</param>
    /// <returns>The squared value between two entities, or <see langword="0"/> if they overlap.</returns>
    internal static float DistanceBetweenTwoEntitiesSquared(Entity entity1, Entity entity2) {
      return DistanceBetweenTwoRectsSquared(
        new Rectangle((int)entity1.Left.X, (int)entity1.Top.Y, entity1.width, entity1.height),
        new Rectangle((int)entity2.Left.X, (int)entity2.Top.Y, entity2.width, entity2.height));
    }

    /// <summary>
    /// Distance between two rectangles. If the rectangles overlap, this will return <see langword="0"/>.
    /// </summary>
    /// <param name="rect1">First rectangle.</param>
    /// <param name="rect2">Second rectangle.</param>
    /// <returns>The squared distance between two rectangles, or <see langword="0"/> if they overlap.</returns>
    internal static float DistanceBetweenTwoRectsSquared(Rectangle rect1, Rectangle rect2) {
      float xAxis = rect1.Right < rect2.Left ? rect2.Left - rect1.Right : rect2.Right < rect1.Left ? rect1.Left - rect2.Right : 0;
      float yAxis = rect1.Bottom < rect2.Top ? rect2.Top - rect1.Bottom : rect2.Bottom < rect1.Top ? rect1.Top - rect2.Bottom : 0;
      return (float)(Math.Pow(xAxis, 2) + Math.Pow(yAxis, 2));
    }
    #endregion

    /// <summary>
    /// Linear interpoation between two <paramref name="f1"/> and <paramref name="f2"/> by <paramref name="by"/>.
    /// </summary>
    /// <param name="f1">First point.</param>
    /// <param name="f2">Second point.</param>
    /// <param name="by">Weight between the two, between <see langword="0"/> and <see langword="1"/>.</param>
    /// <returns></returns>
    internal static float Lerp(float f1, float f2, float by) => f1 * (1 - by) + f2 * by;

    /// <summary>
    /// Assigns multiple indexes of an array to <paramref name="value"/>.
    /// </summary>
    /// <param name="arr">The array to assign values to.</param>
    /// <param name="value">The value to assign to.</param>
    /// <param name="keys">Indices of the array to assign to.</param>
    internal static void AssignValueToKeys<T>(this T[] arr, T value, params int[] keys) {
      for (int i = 0, len = keys.Length; i < len; i++) {
        arr[keys[i]] = value;
      }
    }

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
  }
}

internal static class ReflectionUtils {
  /// <summary>
  /// Search for a method by name and parameter types.  
  /// Unlike GetMethod(), does 'loose' matching on generic
  /// parameter types, and searches base interfaces.
  /// </summary>
  /// <exception cref="AmbiguousMatchException"/>
  public static MethodInfo GetMethodExt(this Type thisType,
                                          string name,
                                          params Type[] parameterTypes) {
    return GetMethodExt(thisType,
                        name,
                        BindingFlags.Instance
                        | BindingFlags.Static
                        | BindingFlags.Public
                        | BindingFlags.NonPublic
                        | BindingFlags.FlattenHierarchy,
                        parameterTypes);
  }

  /// <summary>
  /// Search for a method by name, parameter types, and binding flags.  
  /// Unlike GetMethod(), does 'loose' matching on generic
  /// parameter types, and searches base interfaces.
  /// </summary>
  /// <exception cref="AmbiguousMatchException"/>
  public static MethodInfo GetMethodExt(this Type thisType,
                                          string name,
                                          BindingFlags bindingFlags,
                                          params Type[] parameterTypes) {
    MethodInfo matchingMethod = null;

    // Check all methods with the specified name, including in base classes
    GetMethodExt(ref matchingMethod, thisType, name, bindingFlags, parameterTypes);

    // If we're searching an interface, we have to manually search base interfaces
    if (matchingMethod == null && thisType.IsInterface) {
      foreach (Type interfaceType in thisType.GetInterfaces())
        GetMethodExt(ref matchingMethod, interfaceType, name, bindingFlags, parameterTypes);
    }

    return matchingMethod;
  }

  private static void GetMethodExt(ref MethodInfo matchingMethod,
                                      Type type,
                                      string name,
                                      BindingFlags bindingFlags,
                                      params Type[] parameterTypes) {
    // Check all methods with the specified name, including in base classes
    foreach (MethodInfo methodInfo in type.GetMember(name, MemberTypes.Method, bindingFlags)) {
      // Check that the parameter counts and types match, 
      // with 'loose' matching on generic parameters
      ParameterInfo[] parameterInfos = methodInfo.GetParameters();
      if (parameterInfos.Length == (parameterTypes?.Length ?? 0)) {
        int i = 0;
        for (; i < parameterInfos.Length; ++i) {
          if (!parameterInfos[i].ParameterType.IsSimilarType(parameterTypes[i]))
            break;
        }
        if (i == parameterInfos.Length) {
          if (matchingMethod == null) {
            matchingMethod = methodInfo;
          }
          else {
            throw new AmbiguousMatchException("More than one matching method found!");
          }
        }
      }
    }
  }

  /// <summary>
  /// Special type used to match any generic parameter type in GetMethodExt().
  /// </summary>
  public class T { }

  /// <summary>
  /// Determines if the two types are either identical, or are both generic 
  /// parameters or generic types with generic parameters in the same
  ///  locations (generic parameters match any other generic paramter,
  /// but NOT concrete types).
  /// </summary>
  private static bool IsSimilarType(this Type thisType, Type type) {
    // Ignore any 'ref' types
    if (thisType.IsByRef)
      thisType = thisType.GetElementType();
    if (type.IsByRef)
      type = type.GetElementType();

    // Handle array types
    if (thisType.IsArray && type.IsArray)
      return thisType.GetElementType().IsSimilarType(type.GetElementType());

    // If the types are identical, or they're both generic parameters 
    // or the special 'T' type, treat as a match
    if (thisType == type || ((thisType.IsGenericParameter || thisType == typeof(T))
                         && (type.IsGenericParameter || type == typeof(T))))
      return true;

    // Handle any generic arguments
    if (thisType.IsGenericType && type.IsGenericType) {
      Type[] thisArguments = thisType.GetGenericArguments();
      Type[] arguments = type.GetGenericArguments();
      if (thisArguments.Length == arguments.Length) {
        for (int i = 0; i < thisArguments.Length; ++i) {
          if (!thisArguments[i].IsSimilarType(arguments[i]))
            return false;
        }
        return true;
      }
    }

    return false;
  }
}
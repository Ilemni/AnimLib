using System.Linq.Expressions;
using System.Reflection;

namespace AnimLib.Utilities;

public static class ClassHacking {
  private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

  /// <summary>
  /// Allows you to generate getter for any (including nonpublic) member of class
  /// </summary>
  private static Func<TClass, TOut> CreateGetter<TClass, TOut>(FieldInfo field) {
    ParameterExpression instanceExp = Expression.Parameter(typeof(TClass), "instance");
    MemberExpression fieldExp = Expression.Field(instanceExp, field);
    return Expression.Lambda<Func<TClass, TOut>>(fieldExp, instanceExp).Compile();
  }

  public static Func<TClass, TOut> CreateGetter<TClass, TOut>(string fieldName) where TClass : class {
    FieldInfo? field = typeof(TClass).GetField(fieldName, Flags);
    if (field is null) {
      throw new ArgumentException($"Field '{fieldName}' not found in type '{typeof(TClass).Name}'");
    }

    return CreateGetter<TClass, TOut>(field);
  }

  /// <summary>
  /// Allows you to generate setter for any (including nonpublic) member of class
  /// </summary>
  private static Action<TClass, TIn> CreateSetter<TClass, TIn>(FieldInfo field) {
    ParameterExpression instanceExp = Expression.Parameter(typeof(TClass), "instance");
    ParameterExpression valueExp = Expression.Parameter(typeof(TIn), "value");
    MemberExpression fieldExp = Expression.Field(instanceExp, field);
    BinaryExpression assignExp = Expression.Assign(fieldExp, valueExp);
    return Expression.Lambda<Action<TClass, TIn>>(assignExp, instanceExp, valueExp).Compile();
  }

  private static Action<TClass, TIn> CreateSetterWithCast<TClass, TIn>(FieldInfo field) {
    ParameterExpression instanceExp = Expression.Parameter(typeof(TClass), "instance");
    ParameterExpression valueExp = Expression.Parameter(typeof(TIn), "value");
    MemberExpression fieldExp = Expression.Field(instanceExp, field);
    UnaryExpression asTInExp = Expression.Convert(valueExp, field.FieldType);
    BinaryExpression assignExp = Expression.Assign(fieldExp, asTInExp);
    return Expression.Lambda<Action<TClass, TIn>>(assignExp, instanceExp, valueExp).Compile();
  }

  public static Action<TClass, TIn> CreateSetter<TClass, TIn>(string fieldName) where TClass : class {
    FieldInfo field = GetField<TClass>(fieldName);
    return CreateSetter<TClass, TIn>(field);
  }

  public static Action<TClass, TIn> CreateSetterWithCast<TClass, TIn>(string fieldName) where TClass : class {
    FieldInfo field = GetField<TClass>(fieldName);
    return CreateSetterWithCast<TClass, TIn>(field);
  }

  public static Action<TClass> CreateDelegate<TClass>(string methodName) where TClass : class =>
    GetMethod<TClass>(methodName).CreateDelegate<Action<TClass>>();

  public static Action<TClass, T1> CreateDelegate<TClass, T1>(string methodName) where TClass : class =>
    GetMethod<TClass>(methodName).CreateDelegate<Action<TClass, T1>>();

  public static Action<TClass, T1, T2> CreateDelegate<TClass, T1, T2>(string methodName) where TClass : class =>
    GetMethod<TClass>(methodName).CreateDelegate<Action<TClass, T1, T2>>();

  public static Action<TClass, T1, T2, T3> CreateDelegate<TClass, T1, T2, T3>(string methodName) where TClass : class =>
    GetMethod<TClass>(methodName).CreateDelegate<Action<TClass, T1, T2, T3>>();

  private static MethodInfo GetMethod<TClass>(string methodName) {
    return typeof(TClass).GetMethod(methodName, Flags) ??
      throw new ArgumentException($"Method '{methodName}' not found in type '{typeof(TClass).Name}'");
  }

  private static FieldInfo GetField<TClass>(string fieldName) {
    return typeof(TClass).GetField(fieldName, Flags) ??
      throw new ArgumentException($"Field '{fieldName}' not found in type '{typeof(TClass).Name}'");
  }
}

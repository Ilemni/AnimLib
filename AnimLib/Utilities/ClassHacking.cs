using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AnimLib.Utilities {
  public static class ClassHacking {

    /// <summary>
    /// Allows you to generate getter for any (including nonpublic) member of class
    /// </summary>
    public static Func<TClass, TOut> GenerateGetter<TClass, TOut>(FieldInfo field) {
      ParameterExpression instanceExp = Expression.Parameter(typeof(TClass), "instance");
      MemberExpression fieldExp = Expression.Field(instanceExp, field);
      return Expression.Lambda<Func<TClass, TOut>>(fieldExp, instanceExp).Compile();
    }

    /// <summary>
    /// Allows you to generate setter for any (including nonpublic) member of class
    /// </summary>
    public static Action<TClass, TIn> GenerateSetter<TClass, TIn>(FieldInfo field) {
      ParameterExpression instanceExp = Expression.Parameter(typeof(TClass), "instance");
      ParameterExpression valueExp = Expression.Parameter(typeof(TIn), "value");
      MemberExpression fieldExp = Expression.Field(instanceExp, field);
      BinaryExpression assignExp = Expression.Assign(fieldExp, valueExp);
      return Expression.Lambda<Action<TClass, TIn>>(assignExp, instanceExp, valueExp).Compile();
    }
  }
}

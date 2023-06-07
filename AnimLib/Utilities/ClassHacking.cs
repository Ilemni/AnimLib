using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AnimLib.Utilities {
  public static class ClassHacking {

    /// <summary>
    /// Allows you to generate getter for any (including nonpublic) member of class
    /// </summary>
    public static Func<Tclass, Tout> GenerateGetter<Tclass, Tout>(FieldInfo field) {
      ParameterExpression instanceExp = Expression.Parameter(typeof(Tclass), "instance");
      MemberExpression fieldExp = Expression.Field(instanceExp, field);
      return Expression.Lambda<Func<Tclass, Tout>>(fieldExp, instanceExp).Compile();
    }

    /// <summary>
    /// Allows you to generate setter for any (including nonpublic) member of class
    /// </summary>
    public static Action<Tclass, Tin> GenerateSetter<Tclass, Tin>(FieldInfo field) {
      ParameterExpression instanceExp = Expression.Parameter(typeof(Tclass), "instance");
      ParameterExpression valueExp = Expression.Parameter(typeof(Tin), "value");
      MemberExpression fieldExp = Expression.Field(instanceExp, field);
      BinaryExpression assignExp = Expression.Assign(fieldExp, valueExp);
      return Expression.Lambda<Action<Tclass, Tin>>(assignExp, instanceExp, valueExp).Compile();
    }
  }
}

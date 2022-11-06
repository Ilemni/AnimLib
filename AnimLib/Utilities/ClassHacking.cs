using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AnimLib.Utilities {
  public static class ClassHacking {

    /// <summary>
    /// Allows you to generate getter for any (including nonpublic) member of class
    /// </summary>
    public static Func<Tclass, Tout> GenerateGetter<Tout, Tclass>(FieldInfo field) {
      DynamicMethod dm = new($"_Get{field.Name}_", typeof(Tout),
                            new Type[] { typeof(Tclass) },
                            field.DeclaringType, true);
      ILGenerator generator = dm.GetILGenerator();

      generator.Emit(OpCodes.Ldarg_0);
      generator.Emit(OpCodes.Ldfld, field);
      generator.Emit(OpCodes.Ret);

      return dm.CreateDelegate<Func<Tclass, Tout>>();
    }

    /// <summary>
    /// Allows you to generate setter for any (including nonpublic) member of class
    /// </summary>
    public static Action<Tclass, Tin> GenerateSetter<Tin, Tclass>(FieldInfo field) {
      DynamicMethod dm = new($"_Set{field.Name}_", typeof(void),
                            new Type[] { typeof(Tclass), typeof(Tin) },
                            field.DeclaringType, true);
      ILGenerator generator = dm.GetILGenerator();

      generator.Emit(OpCodes.Ldarg_0);
      generator.Emit(OpCodes.Ldarg_1);
      generator.Emit(OpCodes.Stfld, field);
      generator.Emit(OpCodes.Ret);

      return dm.CreateDelegate<Action<Tclass, Tin>>();
    }
  }
}

using AnimLib.Utilities;
using Terraria.Graphics.Shaders;

namespace AnimLib.Extensions;

public static class ArmorShaderDataGetSet {
  private static readonly Func<ArmorShaderData, Vector3> UColor;
  private static readonly Func<ArmorShaderData, Vector3> USecondaryColor;
  private static readonly Func<ArmorShaderData, float> USaturation;
  private static readonly Func<ArmorShaderData, float> UOpacity;
  private static readonly Func<ArmorShaderData, Vector2> UTargetPosition;

  static ArmorShaderDataGetSet() {
    UColor = GenerateGetter<Vector3>("_uColor");
    USecondaryColor = GenerateGetter<Vector3>("_uSecondaryColor");
    USaturation = GenerateGetter<float>("_uSaturation");
    UOpacity = GenerateGetter<float>("_uOpacity");
    UTargetPosition = GenerateGetter<Vector2>("_uTargetPosition");
    return;

    Func<ArmorShaderData, TOut> GenerateGetter<TOut>(string fieldName) {
      return ClassHacking.CreateGetter<ArmorShaderData, TOut>(fieldName);
    }
  }

  public static Color GetColor(this ArmorShaderData a) => new(UColor(a));

  public static Vector3 GetUColor(this ArmorShaderData a) => UColor(a);

  public static Color GetSecondaryColor(this ArmorShaderData a) => new(USecondaryColor(a));

  public static Vector3 GetUSecondaryColor(this ArmorShaderData a) => USecondaryColor(a);

  public static float GetSaturation(this ArmorShaderData a) => USaturation(a);

  public static float GetOpacity(this ArmorShaderData a) => UOpacity(a);

  public static Vector2 GetTargetPos(this ArmorShaderData a) => UTargetPosition(a);
}

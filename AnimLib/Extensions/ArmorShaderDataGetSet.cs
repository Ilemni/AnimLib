using AnimLib.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria.Graphics.Shaders;

namespace AnimLib.Extensions {
  public static class ArmorShaderDataGetSet {
    private static readonly Func<ArmorShaderData, Vector3> _uColor;
    private static readonly Func<ArmorShaderData, Vector3> _uSecondaryColor;
    private static readonly Func<ArmorShaderData, float> _uSaturation;
    private static readonly Func<ArmorShaderData, float> _uOpacity;
    private static readonly Func<ArmorShaderData, Vector2> _uTargetPosition;

    static ArmorShaderDataGetSet() {
      Type _type = typeof(ArmorShaderData);
      _uColor = ClassHacking.GenerateGetter<ArmorShaderData, Vector3>(
        _type.GetField("_uColor", BindingFlags.Instance | BindingFlags.NonPublic));
      _uSecondaryColor = ClassHacking.GenerateGetter<ArmorShaderData, Vector3>(
        _type.GetField("_uSecondaryColor", BindingFlags.Instance | BindingFlags.NonPublic));
      _uSaturation = ClassHacking.GenerateGetter<ArmorShaderData, float>(
        _type.GetField("_uSaturation", BindingFlags.Instance | BindingFlags.NonPublic));
      _uOpacity = ClassHacking.GenerateGetter<ArmorShaderData, float>(
        _type.GetField("_uOpacity", BindingFlags.Instance | BindingFlags.NonPublic));
      _uTargetPosition = ClassHacking.GenerateGetter<ArmorShaderData, Vector2>(
        _type.GetField("_uTargetPosition", BindingFlags.Instance | BindingFlags.NonPublic));
    }

    public static Color GetColor(this ArmorShaderData a) => new(_uColor(a));

    public static Vector3 GetUColor(this ArmorShaderData a) => _uColor(a);

    public static Color GetSecondaryColor(this ArmorShaderData a) => new(_uSecondaryColor(a));

    public static Vector3 GetUSecondaryColor(this ArmorShaderData a) => _uSecondaryColor(a);

    public static float GetSaturation(this ArmorShaderData a) => _uSaturation(a);

    public static float GetOpacity(this ArmorShaderData a) => _uOpacity(a);

    public static Vector2 GetTargetPos(this ArmorShaderData a) => _uTargetPosition(a);
  }
}

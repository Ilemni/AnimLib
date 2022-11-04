using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria.Graphics.Shaders;

namespace AnimLib.Shaders {
  public sealed class ArmorShaderDeconstruct : SingleInstance<ArmorShaderDeconstruct> {
    private readonly Type _type;
    private readonly FieldInfo _uColor;
    private readonly FieldInfo _uSecondaryColor;
    private readonly FieldInfo _uSaturation;
    private readonly FieldInfo _uOpacity;
    private readonly FieldInfo _uTargetPosition;

    private ArmorShaderDeconstruct() {
      _type = typeof(ArmorShaderData);
      _uColor = _type.GetField("_uColor", BindingFlags.Instance | BindingFlags.NonPublic);
      _uSecondaryColor = _type.GetField("_uSecondaryColor", BindingFlags.Instance | BindingFlags.NonPublic);
      _uSaturation = _type.GetField("_uSaturation", BindingFlags.Instance | BindingFlags.NonPublic);
      _uOpacity = _type.GetField("_uOpacity", BindingFlags.Instance | BindingFlags.NonPublic);
      _uTargetPosition = _type.GetField("_uTargetPosition", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public Vector3 GetUColor(ArmorShaderData armsh) => (Vector3)_uColor.GetValue(armsh);
    public Vector3 GetUSecondaryColor(ArmorShaderData armsh) => (Vector3)_uSecondaryColor.GetValue(armsh);
    public float GetUSaturation(ArmorShaderData armsh) => (float)_uSaturation.GetValue(armsh);
    public float GetUOpacity(ArmorShaderData armsh) => (float)_uOpacity.GetValue(armsh);
    public Vector2 GetUTargetPosition(ArmorShaderData armsh) => (Vector2)_uTargetPosition.GetValue(armsh);
  }
}

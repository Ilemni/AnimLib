using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

namespace AnimLib.Shaders {
  public sealed class ArmorShaderFields {
    public ArmorShaderFields(ArmorShaderData armsh) { _shader = armsh; }

    private readonly ArmorShaderData _shader;
    private Color? _uColor;
    private Color? _uSecondaryColor;
    private float? _uSaturation;
    private float? _uOpacity;
    private Vector2? _uTargetPosition;

    public Color uColor =>
      (_uColor ?? (_uColor = new(ArmorShaderDeconstruct.Instance.GetUColor(_shader)))).Value;
    public Color uSecondaryColor =>
      (_uSecondaryColor ?? (_uSecondaryColor =
        new(ArmorShaderDeconstruct.Instance.GetUSecondaryColor(_shader)))).Value;
    public float uSaturation =>
      (_uSaturation ?? (_uSaturation = ArmorShaderDeconstruct.Instance.GetUSaturation(_shader))).Value;
    public float uOpacity =>
      (_uOpacity ?? (_uOpacity = ArmorShaderDeconstruct.Instance.GetUOpacity(_shader))).Value;
    public Vector2 uTargetPosition =>
      (_uTargetPosition ?? (_uTargetPosition = ArmorShaderDeconstruct.Instance.GetUTargetPosition(_shader))).Value;

  }
}

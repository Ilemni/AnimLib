using AnimLib.States;
using JetBrains.Annotations;
using Terraria.DataStructures;

namespace AnimLib;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class AnimPlayerDrawLayer<TCharacter, TAnimation> : PlayerDrawLayer
  where TCharacter : AnimCharacter, new()
  where TAnimation : SkinAnimation, new() {
  private static void Get(ref readonly PlayerDrawSet drawInfo, out TCharacter character, out TAnimation anim) {
    character = drawInfo.drawPlayer.GetCharacter<TCharacter>();
    anim = character.Skins.GetAnimation<TAnimation>();
  }

  /// <summary>
  /// <b>Avoid calling this base method from derived class!</b>
  /// This will cause a stack overflow!
  /// Instead, call
  /// <see cref="GetDefaultVisibility(PlayerDrawSet,TCharacter,TAnimation)"/>.
  /// </summary>
  /// <param name="drawInfo"></param>
  /// <returns></returns>
  public sealed override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
    Get(ref drawInfo, out TCharacter character, out TAnimation anim);
    return GetDefaultVisibility(drawInfo, character, anim);
  }

  public virtual bool GetDefaultVisibility(PlayerDrawSet drawInfo, TCharacter character, TAnimation anim) {
    return character is { Active: true, GraphicsEnabledCompat: true };
  }

  protected sealed override void Draw(ref PlayerDrawSet drawInfo) {
    Get(in drawInfo, out TCharacter character, out TAnimation anim);
    Draw(ref drawInfo, character, anim);
  }

  protected abstract void Draw(ref PlayerDrawSet drawInfo, TCharacter character, TAnimation anim);
}

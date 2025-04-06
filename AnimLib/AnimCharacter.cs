using System.Diagnostics;
using System.Linq;
using AnimLib.Skins;
using AnimLib.States;
using JetBrains.Annotations;

namespace AnimLib;

/// <summary>
/// <see cref="State"/> which represents a Character.
/// <br/> States which belong to this character will not receive most hook updates when this character is disabled.
/// </summary>
[PublicAPI]
[DebuggerDisplay("Name = {Name}, Active = {Active}, Player = {Player?.name}")]
public abstract partial class AnimCharacter : State {
  protected AnimCharacter() {
    // We want Colors to always default to whatever GetDefaultColors() returns.
    // ReSharper disable VirtualMemberCallInConstructor
    _defaultStyle = GetDefaultStyle();
    Style ??= GetDefaultStyle();
    Style.MaxAlpha();
    // ReSharper restore VirtualMemberCallInConstructor
  }

  /// <summary>
  /// Whether this character can be selected at the Character Creation menu during Player Creation.
  /// </summary>
  public virtual bool Selectable => true;

  [field: AllowNull, MaybeNull]
  public EquippedSkins Skins => field ??= SkinLoader.CreateEquippedSkins(this);

  /// <summary>
  /// The colors and styles to apply to the <see cref="Player"/>.
  /// <para/> This field store what the colors and styles would be when the character isn't active,
  /// assigns them to the player when this character becomes active,
  /// and is copied from the player when this character becomes inactive.
  /// </summary>
  public readonly AnimCharacterStyle Style;

  public AnimCharacterStyleUISettings StyleUISettings { get; internal set; } = null!; // StateLoader.NewInstance()

  /// <summary>
  /// Used to prevent extra saving of default style values.
  /// </summary>
  private readonly AnimCharacterStyle _defaultStyle;

  /// <summary>
  /// The number of hairstyles available for this character.
  /// </summary>
  public virtual int HairStyleCount { get; }

  /// <summary>
  /// The number of clothing styles available for this character.
  /// </summary>
  public virtual int ClothingCount { get; }

  public AnimUiInfo UiInfo => Characters.UiInfo;

  public AnimCharacterCollection Characters => (AnimCharacterCollection)Parent!;

  [field: AllowNull, MaybeNull]
  public ICollection<AbilityState> AbilityStates => field ??= AllChildren.OfType<AbilityState>().ToArray();

  /// <summary>
  /// Whether this <see cref="AnimCharacter"/> is the current active character on the <see cref="Player"/>.
  /// <para/> Only one <see cref="AnimCharacter"/> instance may be active on a character at a given time.
  /// </summary>
  protected override bool ActiveCondition => AnimationUpdEnabledCompat;

  /// <summary>
  /// Enable your character. This disables the previous character, if there was one.
  /// </summary>
  public void Enable() => Characters.Enable(this);

  /// <summary>
  /// Disable your character. This would restore vanilla gameplay.
  /// </summary>
  public void Disable() => Characters.Disable(this);

  public void Toggle() {
    if (Active) {
      Disable();
    }
    else {
      Enable();
    }
  }

  public abstract AnimCharacterStyle GetDefaultStyle();

  public virtual AnimCharacterStyleUISettings? GetStyleUISettings() => null;

  public override T GetAnimation<T>() => Skins.GetAnimation<T>();

  internal void UpdateAnimations(float delta) {
    if (!AnimationUpdEnabledCompat || Main.dedServ) {
      return;
    }

    foreach (SkinAnimation anim in Skins.Animations) {
      anim.PreUpdateAnimation();
      try {
        if (anim.UpdateAnimation() is { } options) {
          anim.UpdateAnimationInternal(options, delta);
        }
      }
      catch (MissingTagException ex) {
        string animName = anim.GetType().Name;
        Main.NewText($"{Name} -> {animName}: Missing tag \"{ex.Tag}\"", Color.Red);
      }
      catch (Exception ex) {
        string animName = anim.GetType().Name;
        Log.Error($"[{Name}.UpdateAnimations] -> {animName}]: Caught exception.", ex);
        Main.NewText($"{Mod.Name}:{Name}:{animName}: Caught exception.", Color.Red);
      }
    }
  }

  internal void UpdateUIAnimation(AnimUiInfo uiInfo) {
    if (!AnimationUpdEnabledCompat || Main.dedServ) {
      return;
    }

    foreach (SkinAnimation anim in Skins.Animations) {
      try {
        anim.UpdateUIAnimation(uiInfo);
      }
      catch (Exception ex) {
        string animName = anim.GetType().Name;
        Log.Error($"[{Name}.UpdateUIAnimation] -> [{animName}]: Caught exception.", ex);
        Main.NewText($"{Mod.Name}:{Name}:{animName}: Caught exception.\nSee client.log for more information.", Color.Red);
      }
    }
  }

  /// <summary>
  /// Sets the <see cref="AbilityState.Level"/> of all Abilities to their <see cref="AbilityState.MaxLevel"/>.
  /// </summary>
  public void UnlockAllAbilities() {
    foreach (AbilityState state in AbilityStates) {
      state.Level = state.MaxLevel;
    }
  }

  /// <summary>
  /// Sets the <see cref="AbilityState.Level"/> of all Abilities to 0.
  /// </summary>
  public void ResetAllAbilities() {
    foreach (AbilityState state in AbilityStates) {
      state.Level = 0;
    }
  }
}

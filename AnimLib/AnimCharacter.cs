using System.Diagnostics;
using System.Linq;
using AnimLib.Animations;
using AnimLib.States;
using JetBrains.Annotations;
using Terraria.GameContent.UI.States;

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

  /// <summary>
  /// The colors and styles to apply to the <see cref="Player"/>.
  /// <para/> This field store what the colors and styles would be when the character isn't active,
  /// assigns them to the player when this character becomes active,
  /// and is copied from the player when this character becomes inactive.
  /// </summary>
  public readonly AnimCharacterStyle Style;

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

  /// <summary>
  /// Whether the character is currently being drawn
  /// as part of a <see cref="Terraria.GameContent.UI.Elements.UICharacter"/>.
  /// <para />
  /// Used for custom drawing behaviour, such as in the character creation screen.
  /// </summary>
  public bool IsDrawingInUI => Characters.IsDrawingInUI;

  /// <summary>
  /// When <see cref="IsDrawingInUI"/> is <see langword="true"/>,
  /// this is whether the character is currently animated in the UI.
  /// </summary>
  public bool UIAnimated => Characters.UIAnimated;

  /// <summary>
  /// When <see cref="IsDrawingInUI"/> is <see langword="true"/>,
  /// this is the current counter for the character's animation in the UI.
  /// </summary>
  public int UIAnimationCounter => Characters.UIAnimationCounter;

  /// <summary>
  /// When <see cref="IsDrawingInUI"/> is <see langword="true"/>,
  /// this is the index of the category for character color picker
  /// Used to determine which animation to play in the character UI.
  /// <para />
  /// This value is based on the <see cref="UICharacterCreation.CategoryId"/> value.
  /// </summary>
  public int UICategoryIndex => Characters.UICategoryIndex;

  /// <summary>
  /// When <see cref="IsDrawingInUI"/> is <see langword="true"/>,
  /// this is the index of the category that was previously selected, before <see cref="UICategoryIndex"/>.
  /// Used to determine which animation to play in the character UI.
  /// <para />
  /// This value is based on the <see cref="UICharacterCreation.CategoryId"/> value.
  /// </summary>
  public int UILastCategoryIndex => Characters.UILastCategoryIndex;

  /// <summary>
  /// When <see cref="IsDrawingInUI"/> is <see langword="true"/>,
  /// this is the current counter for the character's animation in the UI,
  /// since the last category change.
  /// </summary>
  public int UICategoryAnimationCounter => Characters.UICategoryAnimationCounter;

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

  internal void UpdateAnimations(float delta) {
    if (!AnimationUpdEnabledCompat || Main.dedServ) {
      return;
    }

    foreach (AnimatedStateMachine animatedState in ActiveChildren.OfType<AnimatedStateMachine>()) {
      AnimationOptions? options = null;
      try {
        options = animatedState.GetAnimationOptionsInternal();
      }
      catch (Exception ex) {
        Log.Error($"[{Name}.UpdateAnimations] -> [{animatedState.Name}]: Caught exception.", ex);
        Main.NewText(
          $"AnimLib -> [{Name}.UpdateAnimations] -> {animatedState.Name}: Caught exception.\nSee client.log for more information.",
          Color.Red);
      }

      if (options is not null) {
        animatedState.UpdateAnimation(options.Value, delta);
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

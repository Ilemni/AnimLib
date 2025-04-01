using AnimLib.Compat;

namespace AnimLib;

public abstract partial class AnimCharacter {
  /// <summary>
  /// List names of <see cref="AnimCompatSystem"/>s active by default
  /// in order to block their work, when <see cref="AnimCharacter"/> is active.
  /// </summary>
  /*[Obsolete("Will be reworked")]*/ public readonly HashSet<string> AnimCompatSystemBlocklist = [];

  private bool _graphicsDisabledDirectly;
  private bool _animationUpdateDisabledDirectly;

  /// <summary>
  /// State of GraphicsDisable conditions since previous update's evaluation.
  /// If <see langword="false"/>, <see cref="PlayerDrawLayer"/>s should not draw.
  /// <para/> If any of conditions return <see langword="true"/>,
  /// associated flag is turned to <see langword="false"/>.
  /// If none - to <see langword="true"/>
  /// </summary>
  /// <remarks>
  /// For AnimLib mods to properly utilize this, their <see cref="PlayerDrawLayer"/>'s
  /// <see cref="PlayerDrawLayer.GetDefaultVisibility"/> hook must return <see langword="false"/> when this is <see langword="false"/>.
  /// </remarks>
  public bool GraphicsEnabledCompat { get; private set; } = true;

  /// <summary>
  /// State of AnimationsUpdateDisable conditions since previous update's evaluation.
  /// If <see langword="false"/>, <see cref="States.AnimatedStateMachine"/> will not receive
  /// <see cref="States.AnimatedStateMachine.UpdateAnimation"/> calls.
  /// </summary>
  public bool AnimationUpdEnabledCompat { get; private set; } = true;

  /// <summary>
  /// Called at start phase of game state update
  /// evaluates conditions and sets appropriate condition flags
  /// </summary>
  internal void UpdateConditions() {
    if (!_graphicsDisabledDirectly) {
      GraphicsEnabledCompat = !GlobalCompatConditions.EvaluateDisableGraphics(Player);
    }

    if (!_animationUpdateDisabledDirectly) {
      AnimationUpdEnabledCompat = !GlobalCompatConditions.EvaluateDisableAnimationUpdate(Player);
    }

    UpdateCustomConditions();
  }

  /// <summary>
  /// Called at post phase of game state update
  /// evaluates conditions and sets appropriate condition flags
  /// </summary>
  internal void UpdateConditionsPost() {
    _graphicsDisabledDirectly = false;
    _animationUpdateDisabledDirectly = false;
  }

  /// <summary>
  /// Allows to perform condition evaluation actions
  /// for custom conditional actions
  /// called right after <see cref="AnimCompatSystem"/>'s conditions evaluation
  /// </summary>
  public virtual void UpdateCustomConditions() {
  }

  /// <summary>
  /// Use this for compatibility, if you want to trigger directly
  /// disabling of PlayerDrawLayers' changes
  /// (hiding vanilla layers and displaying game character)
  /// (as example, morph ball from MetroidMod should hide players' character)
  /// </summary>
  public void DisableGraphicsDirectly() {
    GraphicsEnabledCompat = false;
    _graphicsDisabledDirectly = true;
  }

  /// <summary>
  /// Use this for compatibility, if you want to trigger directly
  /// disabling of animations of this controller updating
  /// </summary>
  public void DisableAnimationsUpdDirectly() {
    AnimationUpdEnabledCompat = false;
    _animationUpdateDisabledDirectly = true;
  }
}

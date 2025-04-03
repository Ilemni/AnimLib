using AnimLib.Utilities;
using JetBrains.Annotations;
using Terraria.GameContent.UI.Elements;

namespace AnimLib.Systems;

/// <summary>
/// This ModSystem exists to inform <see cref="AnimCharacterCollection"/>
/// when a <see cref="UICharacter"/> is being drawn.
/// </summary>
[UsedImplicitly]
public sealed class UICharacterIntercept : ModSystem {
  // ReSharper disable InconsistentNaming - Actions/Funcs
  private readonly Func<UICharacter, Player> GetUICharacterPlayer = ClassHacking.CreateGetter<UICharacter, Player>("_player");
  private readonly Func<UICharacter, int> GetAnimationCounter = ClassHacking.CreateGetter<UICharacter, int>("_animationCounter");
  private readonly Func<UIHairStyleButton, Player> GetHairStylePlayer = ClassHacking.CreateGetter<UIHairStyleButton, Player>("_player");
  // ReSharper restore InconsistentNaming

  // This dict exist solely to have AnimCharacter.UICategoryCounterStart be accurate on a per-UICharacter basis
  private readonly Dictionary<WeakReference<UICharacter>, int> _categoryStartTimers = [];

  public override void Load() {
    Log.Debug(
      "Adding hooks to UICharacter.DrawSelf and UIHairStyleButton.DrawSelf, for updating AnimCharacter UI fields");
    On_UICharacter.DrawSelf += On_UICharacterDrawSelf;
    On_UIHairStyleButton.DrawSelf += On_UIHairStyleButtonOnDrawSelf;
  }

  private void On_UICharacterDrawSelf(On_UICharacter.orig_DrawSelf orig, UICharacter self, SpriteBatch spritebatch) {
    AnimCharacterCollection collection = GetUICharacterPlayer(self).GetState<AnimCharacterCollection>();

    bool hasAdded = false;
    foreach (var weakRef in _categoryStartTimers.Keys) {
      if (!weakRef.TryGetTarget(out UICharacter? target)) {
        _categoryStartTimers.Remove(weakRef);
      }
      else if (ReferenceEquals(self, target)) {
        hasAdded = true;
      }
    }

    int animationCounter = GetAnimationCounter(self);
    if (!hasAdded) {
      _categoryStartTimers.Add(new WeakReference<UICharacter>(self), animationCounter);
    }

    AnimUiInfo uiInfo = collection.UiInfo;
    int categoryIndex = uiInfo.CategoryIndex;
    if (categoryIndex != -1) {
      if (uiInfo.CategoryIndexLastFrame != categoryIndex) {
        uiInfo.LastCategoryIndex = uiInfo.CategoryIndexLastFrame;
        uiInfo.CategoryIndexLastFrame = categoryIndex;
        foreach (var weakRef in _categoryStartTimers.Keys) {
          if (weakRef.TryGetTarget(out UICharacter? uiCharacter)) {
            _categoryStartTimers[weakRef] = GetAnimationCounter(uiCharacter);
          }
        }
      }
    }

    // Set AnimCharacter category start time to the time we have stored
    foreach ((var weakRef, int categoryStartTimer) in _categoryStartTimers) {
      if (weakRef.TryGetTarget(out UICharacter? uiCharacter) && ReferenceEquals(self, uiCharacter)) {
        uiInfo.CategoryCounterStart = categoryStartTimer;
        break;
      }
    }

    using AnimUiInfo.StoredInfo _ = uiInfo.Store();
    uiInfo.IsDrawingInUI = true;
    uiInfo.Animated = self.IsAnimated;
    uiInfo.AnimationCounter = animationCounter;
    orig(self, spritebatch);
  }

  private void On_UIHairStyleButtonOnDrawSelf(On_UIHairStyleButton.orig_DrawSelf orig, UIHairStyleButton self,
    SpriteBatch spritebatch) {
    AnimCharacterCollection collection = GetHairStylePlayer(self).GetState<AnimCharacterCollection>();
    AnimUiInfo uiInfo = collection.UiInfo;
    using AnimUiInfo.StoredInfo _ = uiInfo.Store();

    uiInfo.IsDrawingInUI = true;
    uiInfo.Animated = false;
    uiInfo.AnimationCounter = 0;
    uiInfo.CategoryIndex = 2; // HairStyleButton is category 2

    orig(self, spritebatch);
  }
}

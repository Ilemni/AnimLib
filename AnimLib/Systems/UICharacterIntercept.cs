using System.Reflection;
using JetBrains.Annotations;
using Terraria.GameContent.UI.Elements;

namespace AnimLib.Systems;

/// <summary>
/// Interception class to inform <see cref="AnimCharacterCollection"/> when a <see cref="UICharacter"/> is being drawn.
/// </summary>
[UsedImplicitly]
public class UICharacterIntercept : ModSystem {
  private static readonly FieldInfo PlayerField = Field<UICharacter>("_player");
  private static readonly FieldInfo AnimationCounterField = Field<UICharacter>("_animationCounter");
  private static readonly FieldInfo HairStylePlayerField = Field<UIHairStyleButton>("_player");

  // This dict exist solely to have AnimCharacter.UICategoryCounterStart be accurate on a per-UICharacter basis
  private static readonly Dictionary<WeakReference<UICharacter>, int> CategoryStartTimers = new();

  private static Player GetPlayer(UICharacter self) => (Player)PlayerField.GetValue(self)!;

  private static Player GetPlayer(UIHairStyleButton self) => (Player)HairStylePlayerField.GetValue(self)!;

  private static int GetAnimationCounter(UICharacter self) => (int)AnimationCounterField.GetValue(self)!;

  public override void Load() {
    Log.Debug("Adding hooks to UICharacter.DrawSelf and UIHairStyleButton.DrawSelf, for updating AnimCharacter UI fields");
    On_UICharacter.DrawSelf += (orig, self, spritebatch) => {
      AnimCharacterCollection collection = GetPlayer(self).GetState<AnimCharacterCollection>();

      bool hasAdded = false;
      foreach (var weakRef in CategoryStartTimers.Keys) {
        if (!weakRef.TryGetTarget(out UICharacter? target)) {
          CategoryStartTimers.Remove(weakRef);
        }
        else if (ReferenceEquals(self, target)) {
          hasAdded = true;
        }
      }

      int animationCounter = GetAnimationCounter(self);
      if (!hasAdded) {
        CategoryStartTimers.Add(new WeakReference<UICharacter>(self), animationCounter);
      }

      AnimUiInfo uiInfo = collection.UiInfo;
      int categoryIndex = uiInfo.CategoryIndex;
      if (categoryIndex != -1) {
        if (uiInfo.CategoryIndexLastFrame != categoryIndex) {
          uiInfo.LastCategoryIndex = uiInfo.CategoryIndexLastFrame;
          uiInfo.CategoryIndexLastFrame = categoryIndex;
          foreach (var weakRef in _categoryStartTimers.Keys) {
            if (weakRef.TryGetTarget(out UICharacter? uiCharacter)) {
              CategoryStartTimers[weakRef] = GetAnimationCounter(uiCharacter);
            }
          }
        }
      }

      // Set AnimCharacter category start time to the time we have stored
      foreach ((var weakRef, int categoryStartTimer) in CategoryStartTimers) {
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

  private static void On_UIHairStyleButtonOnDrawSelf(On_UIHairStyleButton.orig_DrawSelf orig, UIHairStyleButton self, SpriteBatch spritebatch) {
    AnimCharacterCollection collection = GetPlayer(self).GetState<AnimCharacterCollection>();
    AnimUiInfo uiInfo = collection.UiInfo;
    using AnimUiInfo.StoredInfo _ = uiInfo.Store();

    uiInfo.IsDrawingInUI = true;
    uiInfo.Animated = false;
    uiInfo.AnimationCounter = 0;
    uiInfo.CategoryIndex = 2; // HairStyleButton is category 2

    orig(self, spritebatch);
  }

  private static FieldInfo Field<T>(string field) =>
    typeof(T).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!;
}

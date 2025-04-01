using System.Globalization;
using System.Linq;
using System.Text;
using AnimLib.Animations;
using AnimLib.States;
using AnimLib.UI.Debug;
using AnimLib.UI.Elements;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AnimLib.Menus.Debug;

/// <summary>
/// Debug UI that displays info about all <see cref="AnimatedStateMachine"/>s in an <see cref="AnimCharacter"/>.
/// </summary>
public sealed class UIAnimatedStatesList : DebugUIElement<AnimCharacter> {
  protected override string HeaderHoverText => "Displays info about all animated states on the selected character.";

  private readonly Dictionary<int, List<UIAnimatedStateListItem>> _stateItems = [];
  private UIList _stateList = null!;

  public override void OnInitialize() {
    base.OnInitialize();
    SetHeaderText("Animated States");

    _stateList = new UIList {
      Width = StyleDimension.FromPixelsAndPercent(-28, 1),
      Height = StyleDimension.FromPixelsAndPercent(-6, 1),
      HAlign = 1f,
      ListPadding = 4
    };
    _stateList.SetPadding(0);
    BodyContainer.Append(_stateList);

    UIScrollbar scrollbar = new() {
      HAlign = 0,
      VAlign = 1,
      Width = StyleDimension.FromPixelsAndPercent(-18, 1),
      Height = StyleDimension.FromPixelsAndPercent(-18, 1),
      Left = StyleDimension.FromPixels(4),
      Top = StyleDimension.FromPixels(-10)
    };
    scrollbar.SetView(100f, 1000f);
    _stateList.SetScrollbar(scrollbar);
    BodyContainer.Append(scrollbar);

    foreach (AnimCharacter character in StateLoader.TemplateStates.OfType<AnimCharacter>()) {
      if (!character.Hierarchy.AllChildrenIds.Any(id => StateLoader.TemplateStates[id] is AnimatedStateMachine)) {
        continue;
      }

      var list = new List<UIAnimatedStateListItem>();
      _stateItems[character.Index] = list;
      foreach (AnimatedStateMachine asm in character.Hierarchy.AllChildrenIds
                 .Select(id => character.GetState(id))
                 .OfType<AnimatedStateMachine>()) {
        UIAnimatedStateListItem item = new(asm, this);
        item.Activate();
        list.Add(item);
      }
    }
  }

  protected override void OnSetState(AnimCharacter? element) {
    _stateList.Clear();
    if (element is null) {
      return;
    }

    if (!_stateItems.TryGetValue(element.Index, out var newItems)) {
      return;
    }

    foreach (UIAnimatedStateListItem item in newItems) {
      _stateList.Add(item);
    }
  }

  private class UIAnimatedStateListItem : UIPanel, IStateUIElement<AnimatedStateMachine> {
    private readonly int _stateIndex;
    private readonly UIAnimatedStatesList _parent;

    private UIText _fileNameValue = null!;
    private UIText _tagNameValue = null!;
    private UIAnimTagProgressBar _tagProgressValue = null!;
    private UIText _timesLoopedValue = null!;
    private UIText _rotationValue = null!;
    private UIText _reversedValue = null!;
    private UIText _spriteEffectsValue = null!;

    private readonly Dictionary<int, string> _hoverText = [];

    public AnimatedStateMachine? State => _parent.State?.GetState(_stateIndex) as AnimatedStateMachine;
    State? IStateUIElement.State => State;

    public UIAnimatedStateListItem(AnimatedStateMachine asm, UIAnimatedStatesList parent) {
      _stateIndex = asm.Index;
      _parent = parent;
      Width = StyleDimension.Fill;
      Height = StyleDimension.FromPixels(164);
      SetPadding(6);
    }

    public override void OnInitialize() {
      base.OnInitialize();

      AddLabelValueElements("Aseprite File:", 0, out _fileNameValue);
      AddLabelValueElements("Current Tag:", 1, out _tagNameValue);

      UIText tagProgressLabel = new("Tag Progress:", 0.9f) {
        HAlign = 0,
        Height = StyleDimension.FromPixels(16),
        Top = StyleDimension.FromPixels(44),
        Left = StyleDimension.FromPixels(4),
      };
      Append(tagProgressLabel);
      _tagProgressValue = new UIAnimTagProgressBar {
        Width = StyleDimension.FromPixelsAndPercent(-(4 + tagProgressLabel.MinWidth.Pixels + 20), 1),
        Height = StyleDimension.FromPixels(12),
        Top = StyleDimension.FromPixels(46),
        Left = StyleDimension.FromPixels(4 + tagProgressLabel.MinWidth.Pixels + 6),
      };
      Append(_tagProgressValue);

      AddLabelValueElements("Times Looped:", 3, out _timesLoopedValue);
      AddLabelValueElements("Rotation:", 4, out _rotationValue);
      AddLabelValueElements("Direction:", 5, out _reversedValue);
      AddLabelValueElements("Sprite Effects:", 6, out _spriteEffectsValue);

      return;

      void AddLabelValueElements(string labelName, int index, out UIText value) {
        UIText labelElement = new(labelName, 0.9f) {
          HAlign = 0,
          Height = StyleDimension.FromPixels(16),
          Top = StyleDimension.FromPixels(4 + 20 * index),
          Left = StyleDimension.FromPixels(4),
        };
        Append(labelElement);

        value = new UIText(string.Empty, 0.9f) {
          HAlign = 0,
          Height = StyleDimension.FromPixels(16),
          Top = StyleDimension.FromPixels(4 + 20 * index),
          Left = StyleDimension.FromPixels(4 + 6 + labelElement.MinWidth.Pixels),
        };
        Append(value);
      }
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
      base.DrawSelf(spriteBatch);
      AnimatedStateMachine? state = State;
      if (state is null) {
        return;
      }

      _fileNameValue.SetText(state.SpriteSheetAsset.Name);
      _fileNameValue.Recalculate();
      _tagNameValue.SetText(state.CurrentTag.Name);
      _tagNameValue.Recalculate();
      _timesLoopedValue.SetText(state.TimesLooped.ToString());
      _timesLoopedValue.Recalculate();
      _rotationValue.SetText(state.SpriteRotation.ToString("F", CultureInfo.CurrentCulture));
      _rotationValue.Recalculate();
      _reversedValue.SetText(state.Reversed ? "Reverse" : "Forward");
      _reversedValue.Recalculate();
      _spriteEffectsValue.SetText(state.Effects.ToString());
      _spriteEffectsValue.Recalculate();

      int frameIndex = state.FrameIndex;
      float progressA = frameIndex / (float)state.CurrentTag.Frames.Length;
      float progressB = Math.Min(state.FrameTime / state.CurrentFrame.Duration, 1) / state.CurrentTag.Frames.Length;
      _tagProgressValue.SetProgress(progressA, progressB);

      if (_tagProgressValue.ContainsPoint(Main.MouseScreen)) {
        int atlasIndex = state.CurrentFrame.AtlasFrameIndex;
        if (!_hoverText.TryGetValue(atlasIndex, out string? hoverText)) {
          hoverText = SetupHoverText(atlasIndex, frameIndex, state.CurrentTag, state.CurrentFrame);
          _hoverText[atlasIndex] = hoverText;
        }

        Main.instance.MouseText(hoverText);
      }
    }

    private static string SetupHoverText(int atlasIndex, int frameIndex, AnimTag tag, AnimFrame frame) {
      StringBuilder sb = new();
      sb.AppendLine($"Frame Index: {frameIndex + 1} / {tag.Frames.Length}");
      sb.AppendLine($"Atlas Index: {atlasIndex}");
      sb.AppendLine($"Duration: {frame.Duration:F}s");
      return sb.ToString();
    }
  }

  private sealed class UIAnimTagProgressBar : UIElement {
    private float _progress;
    private float _subProgress;

    public void SetProgress(float progress, float subProgress = 0) {
      if (subProgress >= 1) {
        progress = 1;
        subProgress = 0;
      }

      _progress = progress;
      _subProgress = subProgress;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
      DrawProgressBar(spriteBatch);
    }

    private void DrawProgressBar(SpriteBatch spriteBatch) {
      Rectangle rect = GetInnerDimensions().ToRectangle();
      Vector2 position = rect.TopLeft();
      Vector2 size = rect.Size();
      Vector2 subPosition = position + size * new Vector2(_progress, 0);
      float width = size.X;
      float barHeight = size.Y;

      Color green = new(83, 191, 102);
      Color yellow = new(191, 191, 63);
      DrawBar(position, width, Color.LightGray);
      DrawBar(position, width * _progress, green);
      DrawBar(subPosition, width * _subProgress, yellow);

      return;

      void DrawBar(Vector2 pos, float w, Color color) {
        Vector2 barSize = new(w, barHeight);
        barSize.Y /= 1000f;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, null, color, 0f, Vector2.Zero, barSize,
          SpriteEffects.None, 0f);
      }
    }
  }
}

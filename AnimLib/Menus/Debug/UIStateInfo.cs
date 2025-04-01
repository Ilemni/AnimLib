using System.Runtime.CompilerServices;
using AnimLib.States;
using AnimLib.UI.Debug;
using AnimLib.UI.Elements;
using AnimLib.Utilities;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AnimLib.Menus.Debug;

public sealed class UIStateInfo : DebugUIElement, IStateUIElement<State> {
  internal UIStateInfo(UIStateList.UIStateListItem parent) {
    _parent = parent;
  }

  private const int MaxStackallocSize = 128;

  private readonly UIStateList.UIStateListItem _parent;

  public State State => _parent.State;

  private Vector2 _textPosition = Vector2.Zero;
  private SpriteBatch? _spriteBatch;

  protected override string HeaderHoverText => "Information about the selected state";

  public override void OnInitialize() {
    base.OnInitialize();
    SetHeaderText(State.Name);
    BodyContainer.PaddingTop += 4;
    BodyContainer.PaddingBottom += 4;
    BodyContainer.IgnoresMouseInteraction = true;
  }

  protected override void DrawSelf(SpriteBatch spriteBatch) {
    base.DrawSelf(spriteBatch);
    HeaderText.TextColor = State.Active ? Color.Lime : Color.White;
    _spriteBatch = spriteBatch;

    _textPosition = BodyContainer.GetInnerDimensions().Position();
    State.DebugText(this);

    CalculatedStyle dims = BodyContainer.GetInnerDimensions();
    float bottom = dims.Y + dims.Height;
    float delta = _textPosition.Y - bottom;
    if (delta > 0) {
      Height.Pixels += delta;
    }

    _spriteBatch = null;
  }

  #region DrawAppend Methods

  /// <summary>
  /// Draws the specified boolean value, color either the specified color, or green if <paramref name="value"/> is <see langword="true"/>, red if <see langword="false"/>.
  /// The label, if not specified, will be the argument expression for <paramref name="value"/>.
  /// </summary>
  /// <param name="value">The boolean value to draw.</param>
  /// <param name="color">The color for the text. If null, the color will be green if <paramref name="value"/> is <see langword="true"/>, red if <see langword="false"/>.</param>
  /// <param name="key">The label to draw.</param>
  public void DrawAppendBoolean(bool value, Color? color = null,
    [CallerArgumentExpression(nameof(value))] string key = null!) {
    DrawAppendLabelValue(key, value.ToString().AsSpan(), color ?? (value ? Green : Red));
  }

  /// <summary>
  /// Draws the specified text, as a label-value pair, and moves the draw position downward.
  /// The drawn value will be displayed as a fraction, where the numerator is the value and the denominator is the max value.
  /// </summary>
  /// <param name="label">The label to draw.</param>
  /// <param name="value">The value to draw.</param>
  /// <param name="max">The expected maximum value of <paramref name="value"/>.</param>
  /// <param name="color">The color for the text.</param>
  /// <param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format.</param>
  /// <param name="provider">An optional object that supplies culture-specific formatting information.</param>
  /// <typeparam name="T">Type that implements <see cref="ISpanFormattable"/>, where <see cref="ISpanFormattable.TryFormat"/> is used for the value.</typeparam>
  public void DrawAppendLabelValue<T>(string label, T value, T max, Color? color = null,
    ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable {
    StackString str = new(stackalloc char[MaxStackallocSize]);
    str.Append(value, format, provider);
    str.Append(" / ");
    str.Append(max, format, provider);
    DrawAppendLabelValue(label, !str.ExceededCapacity ? str.AsReadOnlySpan() : "...".AsSpan(), color);
  }

  /// <summary>
  /// Draws the specified text, as a label-value pair, and moves the draw position downward.
  /// The label, if not specified, will be the argument expression for <paramref name="value"/>.
  /// </summary>
  /// <param name="value">The value to draw.</param>
  /// <param name="color">The color for the text.</param>
  /// <param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format.</param>
  /// <param name="provider">An optional object that supplies culture-specific formatting information.</param>
  /// <param name="label">The label to draw. If not specified, is equal to the argument expression for <paramref name="value"/>.</param>
  /// <typeparam name="T">Type that implements <see cref="ISpanFormattable"/>, where <see cref="ISpanFormattable.TryFormat"/> is used for the value.</typeparam>
  public void DrawAppendLabelValue<T>(T value,
    Color? color = null, ReadOnlySpan<char> format = default, IFormatProvider? provider = null,
    [CallerArgumentExpression(nameof(value))] string label = null!) where T : ISpanFormattable =>
    DrawAppendLabelValue(label, value, color, format, provider);

  /// <summary>
  /// Draws the specified text, as a label-value pair, and moves the draw position downward.
  /// </summary>
  /// <param name="label">The label to draw.</param>
  /// <param name="value">The value to draw.</param>
  /// <param name="color">The color for the text.</param>
  /// <param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format.</param>
  /// <param name="provider">An optional object that supplies culture-specific formatting information.</param>
  /// <typeparam name="T">Type that implements <see cref="ISpanFormattable"/>, where <see cref="ISpanFormattable.TryFormat"/> is used for the value.</typeparam>
  public void DrawAppendLabelValue<T>(string label, T value, Color? color = null,
    ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    where T : ISpanFormattable {
    Span<char> buffer = stackalloc char[MaxStackallocSize];
    var result = value.TryFormat(buffer, out int charsWritten, format, provider)
      ? buffer[..charsWritten]
      : "...".AsSpan();
    DrawAppendLabelValue(label, result, color);
  }

  /// <summary>
  /// Draws the specified text, as a label-value pair, and moves the draw position downward.
  /// If the value is <see langword="null"/>, "null" will be drawn instead.
  /// </summary>
  /// <param name="label">The label to draw.</param>
  /// <param name="value">The value to draw.</param>
  /// <param name="color">The color for the text.</param>
  public void DrawAppendLabelValue(string label, string? value, Color? color = null) =>
    DrawAppendLabelValue(label, (value ?? "null").AsSpan(), color);

  /// <summary>
  /// Draws the specified text, as a label-value pair, and moves the draw position downward.
  /// </summary>
  /// <param name="label">The label to draw.</param>
  /// <param name="value">The value to draw.</param>
  /// <param name="color">The color for the text.</param>
  private void DrawAppendLabelValue(string label, ReadOnlySpan<char> value, Color? color = null) {
    if (_spriteBatch is null) {
      throw new InvalidOperationException("Method must be called during Draw.");
    }

    CalculatedStyle dims = GetInnerDimensions();
    DynamicSpriteFont font = FontAssets.MouseText.Value;
    float width = dims.Width / 2;
    float maxTextWidth = width - 8;

    Vector2 stringSize = font.MeasureString(label);
    float scale = stringSize.X > maxTextWidth ? maxTextWidth / stringSize.X : 1f;

    color ??= Color.White;

    ChatManager.DrawColorCodedStringShadow(_spriteBatch, font, label, _textPosition, Color.Black, 0f, Vector2.Zero,
      new Vector2(scale), -1f, 1.5f);

    _spriteBatch.DrawString(font, label, _textPosition, color.Value, scale: scale);
    _textPosition.X += width;

    stringSize = font.MeasureString(value);
    scale = stringSize.X > maxTextWidth ? maxTextWidth / stringSize.X : 1f;

    ChatManager.DrawColorCodedStringShadow(_spriteBatch, font, value.ToString(), _textPosition, Color.Black, 0f,
      Vector2.Zero,
      new Vector2(scale), -1f, 1.5f);
    _spriteBatch.DrawString(font, value, _textPosition, color.Value, scale: scale);
    _textPosition.X -= width;
    _textPosition.Y += stringSize.Y * scale;
  }

  /// <summary>
  /// Draws the specified text, and moves the draw position downward.
  /// </summary>
  /// <param name="text">The line of text to draw.</param>
  /// <param name="color">The color for the text.</param>
  /// <exception cref="InvalidOperationException"></exception>
  public void DrawAppendLine(ReadOnlySpan<char> text, Color? color = null) {
    if (_spriteBatch is null) {
      throw new InvalidOperationException("Method must be called during Draw.");
    }

    DrawLabel(text, out Vector2 stringSize, color);
    _textPosition.Y += stringSize.Y;
  }

  private void DrawLabel(ReadOnlySpan<char> value, out Vector2 stringSize, Color? color = null) {
    if (_spriteBatch is null) {
      throw new InvalidOperationException("Method must be called during Draw.");
    }

    DynamicSpriteFont? font = FontAssets.MouseText.Value;
    stringSize = font.MeasureString(value);
    float maxTextWidth = GetInnerDimensions().Width - 8;
    float scale = stringSize.X > maxTextWidth ? maxTextWidth / stringSize.X : 1f;
    stringSize *= scale;

    ChatManager.DrawColorCodedStringShadow(_spriteBatch, font, value.ToString(), _textPosition, Color.Black, 0f,
      Vector2.Zero,
      new Vector2(scale), -1f, 1.5f);
    _spriteBatch.DrawString(font, value, _textPosition, color ?? Color.White, scale: scale);
  }


  /// <summary>
  /// Draws a simple progress bar, and moves the draw position downward.
  /// </summary>
  /// <param name="label">
  /// Label for the progress bar.
  /// </param>
  /// <param name="progress">
  /// Amount which the bar will be filled,
  /// relative to <paramref name="maxProgress"/>.
  /// </param>
  /// <param name="maxProgress">
  /// Max progress value, which determines how much the bar
  /// will be filled by <paramref name="progress"/>.
  /// </param>
  /// <param name="color">
  /// The color for the filled portion of the bar.
  /// By default, the color will be yellow if <paramref name="progress"/>
  /// is less than <paramref name="maxProgress"/>, blue otherwise.
  /// </param>
  public void DrawAppendLabelProgressBar(string label, float progress, float maxProgress, Color? color = null) {
    if (_spriteBatch is null) {
      throw new InvalidOperationException("Method must be called during Draw.");
    }

    DrawLabel(label, out Vector2 stringSize, Color.White);

    float labelWidth = GetInnerDimensions().Width / 2;
    _textPosition.X += labelWidth;
    _textPosition.Y += 6;
    DrawProgressBar(progress, maxProgress, color ?? (progress < maxProgress ? Color.Yellow : Color.Blue));
    _textPosition.X -= labelWidth;
    _textPosition.Y += stringSize.Y - 6;
  }

  /// <summary>
  /// Draws a progress bar with multiple steps, and moves the draw position downward.
  /// <br/>These values should be in ascending order, and the last value should be the maximum progress.
  /// <br/>The bar will be filled with <paramref name="filledColor"/>
  /// up to the last value in <paramref name="subProgresses"/> which is less than <paramref name="progress"/>,
  /// then with <paramref name="inProgressColor"/> up to <paramref name="progress"/>.
  /// </summary>
  /// <param name="label">
  /// Label for the progress bar.
  /// </param>
  /// <param name="progress">
  /// Amount which the bar will be filled,
  /// relative to the last value of <paramref name="subProgresses"/>.
  /// </param>
  /// <param name="subProgresses"></param>
  /// <param name="filledColor">
  /// The color for the filled portion of the bar,
  /// up to the last value in <paramref name="subProgresses"/>.
  /// <br/> By default, the color will be blue.
  /// </param>
  /// <param name="inProgressColor">
  /// The color for the filled portion of the bar,
  /// which is between the last value in <paramref name="subProgresses"/> and <paramref name="progress"/>.
  /// <br/> By default, the color will be yellow.
  /// </param>
  public void DrawAppendLabelProgressBar(string label, float progress, ReadOnlySpan<float> subProgresses,
    Color? filledColor = null, Color? inProgressColor = null) {
    filledColor ??= Color.Blue;
    inProgressColor ??= Color.Yellow;
    progress = MathHelper.Clamp(progress, 0, subProgresses[^1]);
    if (_spriteBatch is null) {
      throw new InvalidOperationException("Method must be called during Draw.");
    }

    DrawLabel(label, out Vector2 stringSize, Color.White);

    float labelWidth = GetInnerDimensions().Width / 2;
    _textPosition.X += labelWidth;
    _textPosition.Y += 6;
    DrawProgressBar(progress, subProgresses, filledColor.Value, inProgressColor.Value);
    _textPosition.X -= labelWidth;
    _textPosition.Y += stringSize.Y - 6;
  }

  private void DrawProgressBar(float progress, float maxProgress, Color color) {
    int right = GetInnerDimensions().ToRectangle().Right;
    int width = right - (int)_textPosition.X;
    const int height = 12;

    progress = MathHelper.Clamp(progress, 0, maxProgress);
    float progressWidth = width * (progress / maxProgress);

    Point pos = _textPosition.ToPoint();

    if (IsMouseHovering && new Rectangle(pos.X, pos.Y, width, height).Contains(Main.MouseScreen.ToPoint())) {
      Main.instance.MouseText($"{progress:N0} / {maxProgress:N0}");
    }


    DrawBar(pos, width, height, Color.LightGray);
    DrawBar(pos, progressWidth, height, color);
  }

  private void DrawProgressBar(float progress, ReadOnlySpan<float> subProgresses,
    Color filledColor, Color inProgressColor) {

    int right = GetInnerDimensions().ToRectangle().Right;
    int width = right - (int)_textPosition.X;
    const int height = 12;

    float maxProgress = subProgresses[^1];

    Point pos = _textPosition.ToPoint();

    if (IsMouseHovering && new Rectangle(pos.X, pos.Y, width, height).Contains(Main.MouseScreen.ToPoint())) {
      Main.instance.MouseText($"{progress:N0} / {maxProgress:N0}");
    }

    float finishedProgress = 0;
    foreach (float subProgress in subProgresses) {
      if (progress >= subProgress) {
        finishedProgress = subProgress;
      }
      else {
        break;
      }
    }

    float finishedProgressWidth = width * (finishedProgress / maxProgress);
    float remainingProgressWidth = width * ((progress - finishedProgress) / maxProgress);

    DrawBar(pos, width, height, Color.LightGray);
    if (finishedProgress > 0) {
      DrawBar(pos, finishedProgressWidth, height, filledColor);
    }
    Point subPos = new(pos.X + (int)finishedProgressWidth, pos.Y);

    DrawBar(subPos, remainingProgressWidth, height, inProgressColor);
  }

  private void DrawBar(Point pos, float width, float height, Color color) {
    Vector2 barSize = new(width, height);
    barSize.Y /= 1000f;
    _spriteBatch!.Draw(TextureAssets.MagicPixel.Value, pos.ToVector2(), null, color, 0f, Vector2.Zero, barSize,
      SpriteEffects.None, 0f);
  }

  #endregion
}

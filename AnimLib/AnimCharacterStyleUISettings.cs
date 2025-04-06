using Terraria.UI;

namespace AnimLib;

public sealed class AnimCharacterStyleUISettings {
  /// <summary>
  /// Whether to hide the HairStyle option in the Character Creation screen.
  /// </summary>
  public bool HideHairStyleOption;

  /// <summary>
  /// Whether to hide the Hair Color option in the Character Creation screen.
  /// </summary>
  public bool HideHairColorOption;

  /// <summary>
  /// Whether to hide the Skin Color option in the Character Creation screen.
  /// </summary>
  public bool HideSkinColorOption;

  /// <summary>
  /// Whether to hide the Eye Color option in the Character Creation screen.
  /// </summary>
  public bool HideEyeColorOption;

  /// <summary>
  /// Whether to hide the Shirt Color option in the Character Creation screen.
  /// </summary>
  public bool HideShirtColorOption;

  /// <summary>
  /// Whether to hide the Undershirt Color option in the Character Creation screen.
  /// </summary>
  public bool HideUnderShirtColorOption;

  /// <summary>
  /// Whether to hide the Pants Color option in the Character Creation screen.
  /// </summary>
  public bool HidePantsColorOption;

  /// <summary>
  /// Whether to hide the Shoe Color option in the Character Creation screen.
  /// </summary>
  public bool HideShoeColorOption;

  /// <summary>
  /// Texture assets to draw for HairStyle UI icon instead of the vanilla icons.
  /// </summary>
  public (Asset<Texture2D>? texture, Asset<Texture2D>? middleTexture)? HairStyleIcon;

  /// <summary>
  /// Texture assets to draw for Hair Color UI icon instead of the vanilla icons.
  /// </summary>
  public (Asset<Texture2D>? texture, Asset<Texture2D>? middleTexture)? HairColorIcon;

  /// <summary>
  /// Texture assets to draw for Skin Color UI icon instead of the vanilla icons.
  /// </summary>
  public (Asset<Texture2D>? texture, Asset<Texture2D>? middleTexture)? SkinColorIcon;

  /// <summary>
  /// Texture assets to draw for Eye Color UI icon instead of the vanilla icons.
  /// </summary>
  public (Asset<Texture2D>? texture, Asset<Texture2D>? middleTexture)? EyeColorIcon;

  /// <summary>
  /// Texture assets to draw for Shirt Color UI icon instead of the vanilla icons.
  /// </summary>
  public (Asset<Texture2D>? texture, Asset<Texture2D>? middleTexture)? ShirtColorIcon;

  /// <summary>
  /// Texture assets to draw for Undershirt Color UI icon instead of the vanilla icons.
  /// </summary>
  public (Asset<Texture2D>? texture, Asset<Texture2D>? middleTexture)? UnderShirtColorIcon;

  /// <summary>
  /// Texture assets to draw for Pants Color UI icon instead of the vanilla icons.
  /// </summary>
  public (Asset<Texture2D>? texture, Asset<Texture2D>? middleTexture)? PantsColorIcon;

  /// <summary>
  /// Texture assets to draw for Shoe Color UI icon instead of the vanilla icons.
  /// </summary>
  public (Asset<Texture2D>? texture, Asset<Texture2D>? middleTexture)? ShoeColorIcon;

  public delegate void SelectionCategoriesChanged(ReadOnlySpan<UIElement> pickers, UIElement categories);

  /// <summary>
  /// Used to modify the position of the color pickers in the Character Creation screen.
  /// <br/> This should not be used to add or remove any elements from the UI.
  /// <br/> Reposition by calling <see cref="UIElement.Append(UIElement)"/>, indexing on the "pickers" array.
  /// <br/> The pickers are indexed as follows:
  /// <li>0 = CharInfo</li>
  /// <li>1 = Clothing</li>
  /// <li>2 = HairStyle</li>
  /// <li>3 = HairColor</li>
  /// <li>4 = Eye</li>
  /// <li>5 = Skin</li>
  /// <li>6 = Shirt</li>
  /// <li>7 = Undershirt</li>
  /// <li>8 = Pants</li>
  /// <li>9 = Shoes</li>
  /// </summary>
  public event SelectionCategoriesChanged? OnCategoriesBarChanged;

  internal void InvokeCategoriesBarChanged(ReadOnlySpan<UIElement> pickers, UIElement categories) {
    OnCategoriesBarChanged?.Invoke(pickers, categories);
  }

  public bool IsDefault => ReferenceEquals(this, Default);

  public static readonly AnimCharacterStyleUISettings Default = new();
}

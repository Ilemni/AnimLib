using AnimLib.Networking;
using JetBrains.Annotations;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace AnimLib;

/// <summary>
/// Colors and styles for the player when an AnimCharacter is active.
/// <br/> Also contains options for displaying icons, and replacing their textures, in the Character Creation screen.
/// <br/> These values are stored independently of the vanilla player's colors,
/// and are applied to the player when the character is enabled.
/// </summary>
[PublicAPI]
public sealed class AnimCharacterStyle {
  public sealed class UISettings {
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
  }

  private const string HairColorKey = "hair";
  private const string SkinColorKey = "skin";
  private const string EyeColorKey = "eye";
  private const string ShirtColorKey = "shirt";
  private const string UndershirtColorKey = "underShirt";
  private const string PantsColorsKey = "pants";
  private const string ShoeColorKey = "shoes";
  private const string HairStyleKey = "hairStyle";
  private const string SkinVariantKey = "skinVariant";

  public UISettings? UiSettings;

  public Color HairColor;
  public Color SkinColor;
  public Color EyeColor;
  public Color ShirtColor;
  public Color UnderShirtColor;
  public Color PantsColor;
  public Color ShoeColor;
  public int HairStyle;
  public int SkinVariant;

  public void AssignFromPlayer(Player player) {
    HairColor = player.hairColor;
    SkinColor = player.skinColor;
    EyeColor = player.eyeColor;
    ShirtColor = player.shirtColor;
    UnderShirtColor = player.underShirtColor;
    PantsColor = player.pantsColor;
    ShoeColor = player.shoeColor;
    SkinVariant = player.skinVariant;
    HairStyle = player.hair;
  }

  public void AssignToPlayer(Player player) {
    player.hairColor = HairColor;
    player.skinColor = SkinColor;
    player.eyeColor = EyeColor;
    player.shirtColor = ShirtColor;
    player.underShirtColor = UnderShirtColor;
    player.pantsColor = PantsColor;
    player.shoeColor = ShoeColor;
    player.skinVariant = SkinVariant;
    player.hair = HairStyle;
  }

  public void MaxAlpha() {
    HairColor.A = 255;
    SkinColor.A = 255;
    EyeColor.A = 255;
    ShirtColor.A = 255;
    UnderShirtColor.A = 255;
    PantsColor.A = 255;
    ShoeColor.A = 255;
  }

  public void Load(TagCompound tag, AnimCharacterStyle defaultStyle) {
    HairColor = tag.TryGet(HairStyleKey, out Color hairColor) ? hairColor : defaultStyle.HairColor;
    SkinColor = tag.TryGet(SkinColorKey, out Color skinColor) ? skinColor : defaultStyle.SkinColor;
    EyeColor = tag.TryGet(EyeColorKey, out Color eyeColor) ? eyeColor : defaultStyle.EyeColor;
    ShirtColor = tag.TryGet(ShirtColorKey, out Color shirtColor) ? shirtColor : defaultStyle.ShirtColor;
    UnderShirtColor = tag.TryGet(UndershirtColorKey, out Color underShirtColor) ? underShirtColor : defaultStyle.UnderShirtColor;
    PantsColor = tag.TryGet(PantsColorsKey, out Color pantsColor) ? pantsColor : defaultStyle.PantsColor;
    ShoeColor = tag.TryGet(ShoeColorKey, out Color shoeColor) ? shoeColor : defaultStyle.ShoeColor;
    HairStyle = tag.TryGet(HairStyleKey, out int hairStyle) ? hairStyle : defaultStyle.HairStyle;
    SkinVariant = tag.TryGet(SkinVariantKey, out int skinVariant) ? skinVariant : defaultStyle.SkinVariant;
    MaxAlpha();
  }

  public bool Save(AnimCharacterStyle defaultStyle, [NotNullWhen(true)] out TagCompound? tag) {
    bool hasHairColor = HairColor != defaultStyle.HairColor;
    bool hasSkinColor = SkinColor != defaultStyle.SkinColor;
    bool hasEyeColor = EyeColor != defaultStyle.EyeColor;
    bool hasShirtColor = ShirtColor != defaultStyle.ShirtColor;
    bool hasUnderShirtColor = UnderShirtColor != defaultStyle.UnderShirtColor;
    bool hasPantsColor = PantsColor != defaultStyle.PantsColor;
    bool hasShoeColor = ShoeColor != defaultStyle.ShoeColor;
    bool hasHairStyle = HairStyle != defaultStyle.HairStyle;
    bool hasSkinVariant = SkinVariant != defaultStyle.SkinVariant;

    if (!hasHairColor && !hasSkinColor && !hasEyeColor && !hasShirtColor && !hasUnderShirtColor &&
        !hasPantsColor && !hasShoeColor && !hasHairStyle && !hasSkinVariant) {
      tag = null;
      return false;
    }

    tag = [];
    if (hasHairColor) {
      tag.Set(HairColorKey, HairColor);
    }

    if (hasSkinColor) {
      tag.Set(SkinColorKey, SkinColor);
    }

    if (hasEyeColor) {
      tag.Set(EyeColorKey, EyeColor);
    }

    if (hasShirtColor) {
      tag.Set(ShirtColorKey, ShirtColor);
    }

    if (hasUnderShirtColor) {
      tag.Set(UndershirtColorKey, UnderShirtColor);
    }

    if (hasPantsColor) {
      tag.Set(PantsColorsKey, PantsColor);
    }

    if (hasShoeColor) {
      tag.Set(ShoeColorKey, ShoeColor);
    }

    if (hasHairStyle) {
      tag.Set(HairStyleKey, HairStyle);
    }

    if (hasSkinVariant) {
      tag.Set(SkinVariantKey, SkinVariant);
    }

    return true;
  }

  public void NetSync(NetSyncer sync) {
    sync.Sync(ref HairColor);
    sync.Sync(ref SkinColor);
    sync.Sync(ref EyeColor);
    sync.Sync(ref ShirtColor);
    sync.Sync(ref UnderShirtColor);
    sync.Sync(ref PantsColor);
    sync.Sync(ref ShoeColor);
    sync.Sync(ref HairStyle);
    sync.Sync(ref SkinVariant);
  }
}

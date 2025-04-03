using System.Diagnostics;
using AnimLib.States;
using JetBrains.Annotations;
using Terraria.DataStructures;
using Terraria.Localization;

namespace AnimLib.Skins;

/// <summary>
/// Option for a single skin item from a set of Skins. One instance of this will exist per type.
/// <para/> Note that when inheriting from this class,
/// you should use the generic version, <see cref="SkinSlot{T,T}"/>.
/// </summary>
[DebuggerDisplay("Name = {Name}")]
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class SkinSlot : ModType, ILocalizedModType, IIndexed, IEquatable<SkinSlot>, IComparable<SkinSlot> {
  /// <summary>
  /// Display name of the options/category for this set of Skins.
  /// <br/> This is the value that users should see.
  /// </summary>
  public virtual LocalizedText? DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

  public virtual LocalizedText? Description => this.GetLocalization(nameof(Description), () => "");

  /// <summary>
  /// Sorting order for this slot when displayed in the UI.
  /// This is only sorted against other slots on the same character.
  /// </summary>
  public virtual int SortOrder => 0;

  /// <summary>
  /// The skin that would be assigned to this slot by default when no skin is selected.
  /// </summary>
  public abstract Skin DefaultSkin { get; }

  /// <summary>
  /// Get a template for the type of character that this slot is for.
  /// In case of lacking access to the character type, use <see cref="ModContent.Find{T}(string,string)"/>
  /// </summary>
  protected internal abstract AnimCharacter TemplateCharacter { get; }

  public delegate (Color color, int shader) LayerColorDelegate(string layer, ref readonly PlayerDrawSet drawInfo,
    SkinAnimation skinAnimation);

  /// <summary>
  /// Colors which are applied to <see cref="DrawData"/> based on the layer.
  /// </summary>
  public readonly Dictionary<string, LayerColorDelegate> LayerColors = [];

  public readonly List<string> RequiredLayers = [];

  public readonly List<string> RequiredAnimations = [];

  public abstract SkinAnimation CreateAnimation();

  protected sealed override void Register() {
    ModTypeLookup<SkinSlot>.Register(this);
    SkinLoader.RegisterSlot(this);
  }

  public ushort Index { get; internal set; } = ushort.MaxValue;

  public bool Equals(SkinSlot? other) {
    if (other is null) {
      return false;
    }

    if (ReferenceEquals(this, other)) {
      return true;
    }

    return Index == other.Index;
  }

  public override bool Equals(object? obj) {
    if (obj is null) {
      return false;
    }

    if (ReferenceEquals(this, obj)) {
      return true;
    }

    return obj is SkinSlot slot && Equals(slot);
  }

  public int CompareTo(SkinSlot? other) {
    return other is null ? 1 : SortOrder.CompareTo(other.SortOrder);
  }

  public override int GetHashCode() {
    // ReSharper disable once NonReadonlyMemberInGetHashCode - Index set at load time
    return Index.GetHashCode();
  }

  public string LocalizationCategory => "SkinSlots";

  public sealed override void SetupContent() {
    // Trigger localization
    _ = DisplayName;
    _ = Description;

    SetStaticDefaults();
  }
}

/// <summary>
/// Option for a single skin item from a set of Skins. One instance of this will exist per type.
/// </summary>
/// <typeparam name="TCharacter">
/// Type of <see cref="AnimCharacter"/> this slot is associated with.
/// Used to override <see cref="TemplateCharacter"/>.
/// </typeparam>
/// <typeparam name="TAnimation">
/// Type of <see cref="SkinAnimation"/> associated with this slot.
/// Used to override <see cref="CreateAnimation"/>.
/// </typeparam>
public abstract class SkinSlot<TCharacter, TAnimation> : SkinSlot
  where TCharacter : AnimCharacter
  where TAnimation : SkinAnimation, new() {
  [field: AllowNull, MaybeNull]
  protected internal override TCharacter TemplateCharacter => field ??= ModContent.GetInstance<TCharacter>();

  public sealed override TAnimation CreateAnimation() => new();
}

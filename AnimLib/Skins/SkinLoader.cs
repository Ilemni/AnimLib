using System.Linq;
using AnimLib.Extensions;
using AnimLib.States;
using JetBrains.Annotations;

namespace AnimLib.Skins;

[UsedImplicitly]
public sealed class SkinLoader : ModSystem {
  /// <summary>
  /// Whether there exists any slot with at least two skins.
  /// </summary>
  public static bool HasSelectableSkins { get; private set; }

  internal static readonly Dictionary<int, CharacterSkins> AllCharacterSkins = [];
  private static Skin[] Skins { get; set; } = null!; // PostSetupContent, used for net syncing
  private static readonly List<Skin> RegisteredSkins = [];
  private static readonly List<SkinSlot> RegisteredSlots = [];

  internal static void RegisterSkin(Skin skin) => RegisteredSkins.Add(skin);

  internal static void RegisterSlot(SkinSlot slot) => RegisteredSlots.Add(slot);

  public override void Unload() {
    AllCharacterSkins.Clear();
    RegisteredSkins.Clear();
    RegisteredSlots.Clear();
    HasSelectableSkins = false;
  }

  public override void PostSetupContent() {
    foreach (SkinSlot slot in RegisteredSlots) {
      ushort characterIndex = slot.TemplateCharacter.Index;
      AllCharacterSkins.GetOrCreate(characterIndex).AddSlot(slot, out ushort slotIndex);

      SkinAnimation animation = slot.CreateAnimation();
      animation.Index = slotIndex;
      ContentInstance.Register(animation); // Enables usage of `ModContent.GetInstance<TSkinAnimation>().Index`
    }

    Skins = RegisteredSkins.ToArray();
    for (int i = 0; i < RegisteredSkins.Count; i++) {
      Skin skin = RegisteredSkins[i];
      skin.Index = (ushort)i;
      ushort characterIndex = skin.TemplateCharacter.Index;
      AllCharacterSkins.GetOrCreate(characterIndex).AddSkin(skin);
    }

    foreach (CharacterSkins characterSkins in AllCharacterSkins.Values) {
      characterSkins.Sort();
    }

    HasSelectableSkins = AllCharacterSkins.Values.Any(charSkins => charSkins.SkinsBySlot.Any(s => s.Count > 1));

    RegisteredSlots.Clear();
    RegisteredSkins.Clear();
  }

  /// <summary>
  /// Gets a list of <see cref="SkinSlot"/>, ordered by <see cref="SkinSlot.Index"/>.
  /// </summary>
  /// <param name="character">
  /// The character or template that the slots belong to.
  /// </param>
  public static IReadOnlyList<SkinSlot> GetSlots(AnimCharacter character) {
    return AllCharacterSkins[character.Index].Slots;
  }

  /// <summary>
  /// Gets a list of <see cref="SkinSlot"/>, ordered by mod-defined <see cref="SkinSlot.SortOrder"/>.
  /// </summary>
  /// <param name="character">The character or template that the slots belong to.</param>
  public static IReadOnlyList<SkinSlot> GetSortedSlots(AnimCharacter character) {
    return AllCharacterSkins[character.Index].SortedSlots;
  }

  public static IReadOnlyList<Skin> GetSkins(SkinSlot slot) {
    ushort characterIndex = slot.TemplateCharacter.Index;
    return AllCharacterSkins[characterIndex].SkinsBySlot[slot.Index];
  }

  public static Skin GetSkin(ushort index) {
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Skins.Length);
    return Skins[index];
  }

  internal static EquippedSkins CreateEquippedSkins(AnimCharacter character) {
    return new EquippedSkins(character, GetSlots(character));
  }

  internal sealed class CharacterSkins {
    public readonly List<SkinSlot> Slots = []; // Sorted by SkinSlot.Index
    public readonly List<SkinSlot> SortedSlots = []; // Sorted by SkinSlot.SortOrder
    public readonly List<List<Skin>> SkinsBySlot = []; // Sorted by SkinSlot.Index then by SkinItem.SortOrder

    public void AddSlot(SkinSlot slot, out ushort index) {
      index = slot.Index = (ushort)Slots.Count;
      Slots.Add(slot);
      SkinsBySlot.Add([]);
    }

    public void AddSkin(Skin skin) {
      foreach (SkinSlot slot in skin.ValidSlots) {
        SkinsBySlot[slot.Index].Add(skin);
      }
    }

    internal void Sort() {
      SortedSlots.Clear();
      SortedSlots.AddRange(Slots.OrderBy(s => s.SortOrder));

      foreach (var skins in SkinsBySlot) {
        skins.Sort();
      }
    }
  }
}

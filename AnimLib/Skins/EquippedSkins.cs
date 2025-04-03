using System.IO;
using AnimLib.Networking;
using AnimLib.States;

namespace AnimLib.Skins;

/// <summary>
/// Represents all the skins that are currently equipped to a player.
/// One of these will exist per instance of <see cref="AnimCharacter"/>.
/// </summary>
public sealed class EquippedSkins {
  public EquippedSkins(AnimCharacter character, IReadOnlyList<SkinSlot> slots) {
    Character = character;
    Slots = new EquippedSkinSlot[slots.Count];
    Animations = new SkinAnimation[slots.Count];

    foreach (SkinSlot slot in slots) {
      ushort index = slot.Index;
      EquippedSkinSlot equippedSlot = new(slot);
      SkinAnimation skinAnimation = slot.CreateAnimation();
      skinAnimation.Character = character;
      skinAnimation.Slot = equippedSlot;
      skinAnimation.Index = index;
      if (!Main.dedServ) {
        equippedSlot.OnSkinChanged += skinAnimation.OnSkinChanged;
        equippedSlot.OnSkinChanged += _ => Character.NetUpdate = true;
      }

      Slots[index] = equippedSlot;
      Animations[index] = skinAnimation;
    }
  }

  public AnimCharacter Character { get; }

  public EquippedSkinSlot[] Slots { get; }

  public SkinAnimation[] Animations { get; }

  public EquippedSkinSlot GetSlot(SkinSlot slot) {
    AssertValidSlot(slot, out EquippedSkinSlot equippedSlot);
    return equippedSlot;
  }

  public Skin GetSkin(SkinSlot slot) {
    return GetSlot(slot).Skin;
  }

  public void SetSkin(SkinSlot slot, Skin skin) {
    GetSlot(slot).SetSkin(skin);
  }

  public EquippedSkinSlot GetSlot<T>() where T : SkinSlot {
    return GetSlot(ModContent.GetInstance<T>());
  }

  public Skin GetSkin<T>() where T : SkinSlot {
    return GetSkin(ModContent.GetInstance<T>());
  }

  public void SetSkin<T>(Skin skin) where T : SkinSlot {
    SetSkin(ModContent.GetInstance<T>(), skin);
  }

  public T GetAnimation<T>() where T : SkinAnimation {
    T? template = ModContent.GetInstance<T>();
    if (template.Index >= Animations.Length || Animations[template.Index] is not T result) {
      throw new ArgumentException($"No animation of type {typeof(T).Name} found in equipped animations.");
    }

    return result;
  }

  public void AssertValidSlot(SkinSlot slot, out EquippedSkinSlot equippedSlot) {
    if (Character.Index != slot.TemplateCharacter.Index) {
      throw new ArgumentException($"Slot {slot.Name} is not valid for the character {Character.Name}.");
    }

    equippedSlot = Slots[slot.Index];
  }

  public void NetSync(NetSyncer sync) {
    sync.SyncFunc(this, WriteSkin, ReadSkin);
    if (!Main.dedServ) {
      Main.NewText($"Skin sync! {sync.GetType()}");
    }
  }

  private static void WriteSkin(EquippedSkins self, BinaryWriter writer) {
    foreach (EquippedSkinSlot skinSlot in self.Slots) {
      Skin skin = skinSlot.Skin;
      writer.Write7BitEncodedInt(skin.Index);
    }
  }

  private static void ReadSkin(EquippedSkins self, BinaryReader reader) {
    foreach (EquippedSkinSlot skinSlot in self.Slots) {
      ushort index = (ushort)reader.Read7BitEncodedInt();
      Skin skin = SkinLoader.GetSkin(index);
      skinSlot.SetSkin(skin);
    }
  }
}

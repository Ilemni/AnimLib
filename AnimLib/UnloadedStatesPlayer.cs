using AnimLib.States;
using JetBrains.Annotations;
using Terraria.ModLoader.IO;

namespace AnimLib;

[UsedImplicitly]
public sealed class UnloadedStatesPlayer : ModPlayer {
  private const string UnloadedStates = "unloadedStates";
  private const string ModName = "mod";
  private const string UnloadedSkins = "unloadedSkins";
  private const string CharacterName = "character";
  private const string SkinKey = "skin";
  private const string SkinListKey = "skins";

  private readonly IList<TagCompound> _unloadedStates = [];
  private readonly IList<TagCompound> _unloadedSkins = [];

  public override void SaveData(TagCompound tag) {
    if (_unloadedStates.Count > 0) {
      tag[UnloadedStates] = _unloadedStates;
    }

    if (_unloadedSkins.Count > 0) {
      tag[UnloadedSkins] = _unloadedSkins;
    }
  }

  public override void LoadData(TagCompound tag) {
    if (tag.TryGet(UnloadedStates, out IList<TagCompound> states)) {
      StateIO.LoadStateData(Player, states);
    }

    if (tag.TryGet(UnloadedSkins, out IList<TagCompound> skins)) {
      StateIO.LoadSkinData(Player, skins);
    }
  }

  public void AddUnloadedState(TagCompound tag) => _unloadedStates.Add(tag);

  public void AddUnloadedSkin(string mod, string character, TagCompound skinTag) {
    var characterTag = GetUnloadedSkinCharacterTag(mod, character);
    characterTag.Add(skinTag);
  }

  private List<TagCompound> GetUnloadedSkinCharacterTag(string mod, string character) {
    List<TagCompound> skinTags;
    foreach (TagCompound tag in _unloadedSkins) {
      if (tag.GetString(ModName) == mod &&
          tag.GetString(CharacterName) == character &&
          tag.TryGet(SkinKey, out skinTags)) {
        return skinTags;
      }
    }

    skinTags = [];
    _unloadedSkins.Add(new TagCompound {
      [ModName] = mod,
      [CharacterName] = character,
      [SkinListKey] = skinTags
    });

    return skinTags;
  }
}

using System.Collections.Generic;
using AnimLib.Abilities;
using AnimLib.Animations;
using AnimLib.Extensions;
using AnimLib.Networking;
using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AnimLib {
  /// <summary>
  /// Main <see cref="ModPlayer"/> class for <see cref="AnimLibMod"/>, contains and updates <see cref="AnimationController"/>.
  /// </summary>
  [UsedImplicitly]
  public sealed class AnimPlayer : ModPlayer {
    private const string AllAbilityTagKey = "abilities";

    private static AnimPlayer _local;

    private bool _abilityNetUpdate;

    /// <summary>
    /// We need to make sure we don't lose ability data whenever a previous mod was not used during this session.
    /// </summary>
    private Dictionary<string, TagCompound> _unloadedModTags;

    internal AnimCharacterCollection characters;

    internal static AnimPlayer Local {
      get {
        if (_local is null) {
          if (Main.gameMenu) return null;
          _local = Main.LocalPlayer?.GetModPlayer<AnimPlayer>();
          if (_local is not null) AnimLibMod.OnUnload += () => _local = null;
        }

        return _local;
      }
    }

    /// <summary>
    /// The current active <see cref="AnimCharacter"/>.
    /// </summary>
    [CanBeNull] private AnimCharacter ActiveCharacter => characters.ActiveCharacter;

    internal bool abilityNetUpdate {
      get => _abilityNetUpdate;
      set {
        _abilityNetUpdate = value;
        if (value) return;
        // Propagate false netUpdate downstream
        foreach (AnimCharacter character in characters.Values) {
          if (character.abilityManager != null)
            character.abilityManager.netUpdate = false;
        }
      }
    }

    internal bool DebugEnabled { get; set; }

    /// <summary>
    /// Constructs and collects all <see cref="AnimationController"/>s across all mods onto this <see cref="Player"/>.
    /// </summary>
    public override void Initialize() => characters = new AnimCharacterCollection(this);

    /// <inheritdoc/>
    public override void SendClientChanges(ModPlayer clientPlayer) {
      if (abilityNetUpdate) {
        SendAbilityChanges();
        abilityNetUpdate = false;
      }
    }
    //???
    public override void clientClone(ModPlayer clientClone) => base.clientClone(clientClone);

    private void SendAbilityChanges() => ModNetHandler.Instance.abilityPacketHandler.SendPacket(255, Player.whoAmI);

    /// <summary>
    /// Updates the <see cref="AnimCharacterCollection.ActiveCharacter"/>.
    /// </summary>
    public override void PostUpdateRunSpeeds() => ActiveCharacter?.Update();

    /// <summary>
    /// Updates all <see cref="AnimationController"/>s on this <see cref="Player"/>.
    /// </summary>
    public override void PostUpdate() => ActiveCharacter?.PostUpdate();

    // AnimLibMod Save/Load TagCompound structure:
    // 
    // "abilities":
    //   "{mod_name}":
    //     "{ability_name}":
    //       [ Remaining handled by Ability[AbilityID].Save()/.Load() ]

    // TODO: Look into injecting ability save data into the owning mod rather than saving into this mod.
    /// <summary>
    /// Saves all <see cref="Ability"/> data across all mods.
    /// </summary>
    /// <remarks>
    /// This will save all ability data to this mod regardless of <see cref="AbilityManager.AutoSave"/> condition.
    /// <see cref="AbilityManager.AutoSave"/> will only prevent automatic loading of ability data.
    /// This is set up so that player ability data is not lost if the mod author changes AutoSave from false to true.
    /// </remarks>
    /// <seealso cref="AbilityManager.AutoSave">AbilityManager.AutoSave</seealso>
    public override void SaveData(TagCompound tag)/* tModPorter Suggestion: Edit tag parameter instead of returning new TagCompound */
        {
            TagCompound allAbilitiesTag = new TagCompound();
            foreach ((Mod aMod, AnimCharacter character) in characters) allAbilitiesTag[aMod.Name] = character.abilityManager?.Save();

        if (_unloadedModTags != null)
            foreach ((string modName, TagCompound _utag) in _unloadedModTags)
                allAbilitiesTag[modName] = _utag;

        if (allAbilitiesTag.Count > 0)
        {
            tag[AllAbilityTagKey] = allAbilitiesTag;
        }
    }

    /// <summary>
    /// Loads all <see cref="Ability"/> data across all mods where <see cref="AbilityManager.AutoSave"/> is <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// This mod will save all ability data to this mod regardless of <see cref="AbilityManager.AutoSave"/> condition.
    /// <see cref="AbilityManager.AutoSave"/> will only prevent automatic loading of ability data.
    /// This is set up so that player ability data is not lost if the mod author changes AutoSave from false to true.
    /// </remarks>
    public override void LoadData(TagCompound tag) {
      TagCompound allAbilitiesTag = tag.GetCompound(AllAbilityTagKey);
      if (allAbilitiesTag is null) return;

      // TODO: Consider serializing AnimCharacterCollection character enabled state.
      foreach ((string key, object value) in allAbilitiesTag) {
        if (!(value is TagCompound abilityTag)) continue;
        Mod aMod = ModLoader.GetMod(key);
        // Store unloaded data if mod not loaded, character collection missing mod, or character missing ability manager (mod removed implementation?) 
        if (aMod is null || !characters.TryGetValue(Mod, out AnimCharacter character) || character?.abilityManager == null) {
          if (_unloadedModTags is null) _unloadedModTags = new Dictionary<string, TagCompound>();
          _unloadedModTags[key] = abilityTag;
        }
        else {
          AbilityManager manager = character.abilityManager;
          if (!manager.AutoSave) continue;
          manager.Load(abilityTag);
        }
      }
    }
  }
}

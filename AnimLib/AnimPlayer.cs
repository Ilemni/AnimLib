using AnimLib.Abilities;
using AnimLib.Animations;
using AnimLib.Networking;
using JetBrains.Annotations;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AnimLib {
  /// <summary>
  /// Main <see cref="ModPlayer"/> class for <see cref="AnimLibMod"/>, contains and updates <see cref="AnimationController"/>.
  /// </summary>
  [UsedImplicitly]
  public sealed class AnimPlayer : ModPlayer {
    /// <summary>
    /// Old ability save data from AnimLib goes here
    /// </summary>
    public TagCompound OldAbilities { get; internal set; }

    [Obsolete]
    private const string AllAbilityTagKey = "abilities";

    private static AnimPlayer _local;

    private bool _abilityNetUpdate;

    internal AnimCharacterCollection characters =>
      _characters ??= new AnimCharacterCollection(this);
    private AnimCharacterCollection _characters;

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

    /// <summary>
    /// Whether any <see cref="AnimCharacter"/>s need to be net-synced.<br />
    /// When this property is set to <b><see langword="false"/></b>, all
    /// <see cref="AbilityManager.netUpdate">AbilityManager.netUpdate</see> on this player
    /// will also be set to <b><see langword="false"/></b>.<br />
    /// When any <see cref="AbilityManager.netUpdate">AbilityManager.netUpdate</see> on this player
    /// is set to <b><see langword="true"/></b>, this property will also be set to <b><see langword="true"/></b>.
    /// </summary>
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
    public override void Initialize() => _characters = new AnimCharacterCollection(this);

    /// <inheritdoc/>
    public override void SendClientChanges(ModPlayer clientPlayer) {
      if (abilityNetUpdate) {
        SendAbilityChanges();
        abilityNetUpdate = false;
      }
    }

    public override void CopyClientState(ModPlayer targetCopy) => base.CopyClientState(targetCopy);

    private void SendAbilityChanges() => ModNetHandler.Instance.abilityPacketHandler.SendPacket(255, Player.whoAmI);

    /// <summary>
    /// Updates the <see cref="AnimCharacterCollection.ActiveCharacter"/>.
    /// </summary>
    public override void PostUpdateRunSpeeds() => ActiveCharacter?.Update();

    /// <summary>
    /// Updates all <see cref="AnimationController"/>s on this <see cref="Player"/>.
    /// </summary>
    public override void PostUpdate() => ActiveCharacter?.PostUpdate();

    /// <summary>
    /// Saves all <see cref="Ability"/> data across all mods.
    /// </summary>
    /// <remarks>
    /// This will save all ability data to this mod regardless of <see cref="AbilityManager.AutoSave"/> condition.
    /// <see cref="AbilityManager.AutoSave"/> will only prevent automatic loading of ability data.
    /// This is set up so that player ability data is not lost if the mod author changes AutoSave from false to true.
    /// </remarks>
    /// <seealso cref="AbilityManager.AutoSave">AbilityManager.AutoSave</seealso>
    [Obsolete]
    public override void SaveData(TagCompound tag)/* tModPorter Suggestion: Edit tag parameter instead of returning new TagCompound */
    {
      if ((OldAbilities?.Count ?? 0) > 0)
      {
        tag[AllAbilityTagKey] = OldAbilities;
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
    [Obsolete]
    public override void LoadData(TagCompound tag) {
      if(tag.ContainsKey(AllAbilityTagKey))
        OldAbilities = tag.GetCompound(AllAbilityTagKey);
    }
  }
}

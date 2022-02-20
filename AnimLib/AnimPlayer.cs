using System;
using System.Collections.Generic;
using System.Linq;
using AnimLib.Abilities;
using AnimLib.Animations;
using AnimLib.Internal;
using AnimLib.Networking;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AnimLib {
  /// <summary>
  /// Main <see cref="ModPlayer"/> class for <see cref="AnimLibMod"/>, contains and updates <see cref="AnimationController"/>.
  /// </summary>
  [UsedImplicitly]
  public sealed class AnimPlayer : ModPlayer {
    private const string AllAbilityTagKey = "abilities";

    private bool _abilityNetUpdate;

    /// <summary>
    /// We need to make sure we don't lose ability data whenever a previous mod was not used during this session.
    /// </summary>
    private Dictionary<string, TagCompound> _unloadedModTags;


    /// <summary>
    /// Max 1 <see cref="AnimationController"/> per mod, requires inheritance. Unlimited <see cref="AnimationSource"/> types per mod.
    /// </summary>
    internal Dictionary<Mod, AnimationController> animationControllers { get; private set; }

    /// <summary>
    /// Max 1 <see cref="AbilityManager"/> per mod, inheritance optional. Unlimited <see cref="Ability"/> types per mod.
    /// </summary>
    internal Dictionary<Mod, AbilityManager> abilityManagers { get; private set; }

    private bool hasInitialized { get; set; }

    internal static AnimPlayer Local {
      get {
        if (_local is null) {
          _local = Main.LocalPlayer?.GetModPlayer<AnimPlayer>();
          if (!(_local is null)) {
            AnimLibMod.OnUnload += () => _local = null;
          }
        }
        return _local;
      }
    }

    private static AnimPlayer _local;

    internal bool abilityNetUpdate {
      get => _abilityNetUpdate;
      set {
        _abilityNetUpdate = value;
        if (value) return;
        // Propagate false netUpdate downstream
        foreach (AbilityManager manager in abilityManagers.Values) manager.netUpdate = false;
      }
    }

    /// <summary>
    /// Gets the <see cref="AnimationController"/> of the given type from this <see cref="AnimPlayer"/>.
    /// Use this if you want your code to use values such as the current track and frame.
    /// <para>This <strong>cannot</strong> be used during the <see cref="ModPlayer.Initialize"/> method.</para>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="AnimationController"/> to get.</typeparam>
    /// <returns>An <see cref="AnimationController"/> of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">This was called during <see cref="ModPlayer.Initialize"/>, or was called by code run by a server.</exception>
    public T GetAnimationController<T>() where T : AnimationController {
      if (Main.netMode == NetmodeID.Server)
        throw new InvalidOperationException($"Cannot call {nameof(GetAnimationController)} on code run by a server.");
      if (!hasInitialized)
        throw new InvalidOperationException($"Cannot call {nameof(GetAnimationController)} during ModPlayer.Initialize");

      return animationControllers.Values.FirstOrDefault(c => c is T) as T
             ?? throw new Exception($"{typeof(T).Name} is not loaded.");
    }

    /// <summary>
    /// Gets the <see cref="AbilityManager"/> of the given type from this <see cref="AnimPlayer"/>.
    /// Use this if you want your code to access ability information.
    /// <para>This <strong>cannot</strong> be used during the <see cref="ModPlayer.Initialize"/> method.</para>
    /// </summary>
    /// <typeparam name="T">Type of <see cref="AbilityManager"/> to get.</typeparam>
    /// <returns>An <see cref="AbilityManager"/> of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">This was called during <see cref="ModPlayer.Initialize"/>.</exception>
    public T GetAbilityManager<T>() where T : AbilityManager {
      if (!hasInitialized)
        throw new InvalidOperationException($"Cannot call {nameof(GetAbilityManager)} during ModPlayer.Initialize");

      return abilityManagers?.Values.FirstOrDefault(m => m is T) as T;
    }

    /// <summary>
    /// Constructs and collects all <see cref="AnimationController"/>s across all mods onto this <see cref="Player"/>.
    /// </summary>
    public override void Initialize() {
      base.Initialize();
      if (AnimLoader.UseAnimations) {
        animationControllers = new Dictionary<Mod, AnimationController>();
        AnimationLoader.CreateControllersForPlayer(this);
      }

      abilityManagers = new Dictionary<Mod, AbilityManager>();
      AbilityLoader.CreateAbilityManagersForPlayer(this);

      hasInitialized = true;
    }

    /// <inheritdoc/>
    public override void SendClientChanges(ModPlayer clientPlayer) {
      if (abilityNetUpdate) {
        SendAbilityChanges();
        abilityNetUpdate = false;
      }
    }

    private void SendAbilityChanges() => ModNetHandler.Instance.abilityPacketHandler.SendPacket(255, player.whoAmI);

    public override void PostUpdateRunSpeeds() {
      foreach (AbilityManager manager in abilityManagers.Values) {
        manager.Update();
      }
    }

    /// <summary>
    /// Updates all <see cref="AnimationController"/>s on this <see cref="Player"/>.
    /// </summary>
    public override void PostUpdate() {
      if (AnimLoader.UseAnimations) UpdateAnimations();
      foreach (AbilityManager manager in abilityManagers.Values) {
        manager.PostUpdate();
      }
    }

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
    public override TagCompound Save() {
      TagCompound allAbilitiesTag = new TagCompound();
      foreach ((Mod aMod, AbilityManager manager) in abilityManagers) {
        TagCompound abilityTag = manager.Save();
        if (abilityTag != null && abilityTag.Count > 0) allAbilitiesTag.Add(aMod.Name, abilityTag);
      }

      if (_unloadedModTags != null) {
        foreach ((string modName, TagCompound tag) in _unloadedModTags) allAbilitiesTag[modName] = tag;
      }

      if (allAbilitiesTag.Count > 0) {
        return new TagCompound {
          [AllAbilityTagKey] = allAbilitiesTag
        };
      }

      return null;
    }

    /// <summary>
    /// Loads all <see cref="Ability"/> data across all mods where <see cref="AbilityManager.AutoSave"/> is <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// This mod will save all ability data to this mod regardless of <see cref="AbilityManager.AutoSave"/> condition.
    /// <see cref="AbilityManager.AutoSave"/> will only prevent automatic loading of ability data.
    /// This is set up so that player ability data is not lost if the mod author changes AutoSave from false to true.
    /// </remarks>
    public override void Load(TagCompound tag) {
      TagCompound allAbilitiesTag = tag.GetCompound(AllAbilityTagKey);
      if (allAbilitiesTag is null) return;

      foreach ((string key, object value) in allAbilitiesTag) {
        if (!(value is TagCompound abilityTag)) continue;
        Mod aMod = ModLoader.GetMod(key);
        if (aMod is null || !abilityManagers.ContainsKey(aMod)) {
          if (_unloadedModTags is null) _unloadedModTags = new Dictionary<string, TagCompound>();
          _unloadedModTags[key] = abilityTag;
        }
        else {
          AbilityManager manager = abilityManagers[aMod];
          if (!manager.AutoSave) continue;
          manager.Load(abilityTag);
        }
      }
    }

    private void UpdateAnimations() {
      foreach (AnimationController anim in animationControllers.Values) {
        // Probably not a good idea to crash when a purely cosmetic effect fails.
        try {
          if (anim.PreUpdate()) anim.Update();
        }
        catch (Exception ex) {
          Log.LogError($"[{anim.mod.Name}{anim.GetType().UniqueTypeName()}]: Caught exception.", ex);
          Main.NewText($"AnimLib -> {anim.mod.Name}: Caught exception while updating animations. See client.log for more information.", Color.Red);
        }
      }
    }
  }
}

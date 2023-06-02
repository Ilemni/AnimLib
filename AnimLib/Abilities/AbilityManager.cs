using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AnimLib.Abilities {
  /// <summary>
  /// Class for containing and updating all <see cref="Ability"> Abilities </see> in a <see cref="Player"/>.
  /// Consider saving data using your mod's modplayer SaveData and obtain abilities TagCompound using Save
  /// Load ability levels getting the same compound from Save method from LoadData and provide it to
  /// load method to load ability levels and etc
  /// </summary>
  [PublicAPI]
  [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
  public class AbilityManager : IEnumerable<Ability> {
    /// <summary>
    /// Gets the Ability of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The ability type.</typeparam>
    /// <returns>The <see cref="Ability"/> of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException"><typeparamref name="T"/> does not belong to this <see cref="mod"/></exception>
    public T Get<T>() where T : Ability =>
      (T)abilityArray.FirstOrDefault(a => a is T)
      ?? throw new ArgumentException($"{typeof(T).Name} does not belong to {mod}");

    #region Properties - Common
    // Initialized properties that are set by other mods (virtual or abstract properties) are kept in this region.

    // ReSharper disable NotNullMemberIsNotInitialized
    /// <summary>
    /// Array of <see cref="Ability"> Abilities </see> in this <see cref="AbilityManager"/>.
    /// </summary>
    [NotNull] protected internal Ability[] abilityArray;

    /// <summary>
    /// The <see cref="Player"/> that this <see cref="AbilityManager"/> belongs to.
    /// </summary>
    [NotNull] public Player player { get; internal set; }

    [NotNull] internal AnimPlayer animPlayer { get; set; }

    /// <summary>
    /// The <see cref="Mod"/> that this <see cref="AbilityManager"/> belongs to.
    /// </summary>
    [NotNull] public Mod mod { get; internal set; }
    // ReSharper restore NotNullMemberIsNotInitialized

    /// <summary>
    /// Get the <see cref="Ability"/> with the matching <see cref="Ability.Id"/>.
    /// </summary>
    /// <param name="id">Index that corresponds to an <see cref="Ability.Id"/>.</param>
    /// <returns>An <see cref="Ability"/> with the matching <see cref="Ability.Id"/>.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">The value does not match any <see cref="Ability.Id"/>.</exception>
    [NotNull] public Ability this[int id] =>
      abilityArray.FirstOrDefault(a => a.Id == id)
      ?? throw new ArgumentOutOfRangeException($"No ability matches {id}");

    /// <summary>
    /// Gets the <see cref="Ability"/> whose <see cref="Ability.Id"/> is associated with the given <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The <see cref="Ability.Id"/> to locate.</param>
    /// <param name="ability">
    /// The <see cref="Ability"/> associated with the specified <see cref="Ability.Id"/>,
    /// if any <see cref="Ability"/> has an <see cref="Ability.Id"/> matching <paramref name="key"/>;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if an <see cref="Ability"/> has an <see cref="Ability.Id"/> matching <paramref name="key"/>; otherwise, <see langword="false"/>.</returns>
    public bool TryGet(int key, out Ability ability) => (ability = abilityArray.FirstOrDefault(a => a.Id == key)) != null;

    /// <summary>
    /// Returns an enumerator that iterates through all <see cref="Ability"/> instances in this <see cref="AbilityManager"/>.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<Ability> GetEnumerator() => ((IEnumerable<Ability>)abilityArray).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Provides an enumerator that supports iterating through all unlocked <see cref="Ability"/> instances in this <see cref="AbilityManager"/>.
    /// </summary>
    public IEnumerable<Ability> UnlockedAbilities => this.Where(ability => ability.Unlocked);
    #endregion

    #region Properties - Mod-defined
    /// <summary>
    /// Whether or not to automatically load abilities onto this mod.
    /// <para>
    /// When <see langword="true"/>, <see cref="AnimLibMod"/> will create all abilities in your mod
    /// that have <see cref="Ability.Autoload"/> as <see landword="true"/>.
    /// </para>
    /// <para>Set to false if you wish to construct all abilities yourself.</para>
    /// <para>By default this returns the mod's autoload property.</para>
    /// </summary>
    public virtual bool Autoload => mod.ContentAutoloadingEnabled;

    /// <summary>
    /// Whether or not to automatically save ability data during <see cref="ModPlayer.Save"/>.
    /// If <see langword="true"/>, this will save ability data in <see cref="AnimLibMod"/>.
    /// Set to <see langword="false"/> if you wish to save ability data in your own mod.
    /// </summary>
    [Obsolete]
    public virtual bool AutoSave => true;
    #endregion

    #region Properties - Runtime
    // Properties that are expected to change throughout the ability manager's lifespan are kept in this region.
    /// <summary>
    /// Whether or not this ability needs to be synced.
    /// </summary>
    public bool netUpdate {
      get => _netUpdate;
      set {
        _netUpdate = value;
        if (value) // Propagate true netUpdate upstream
          animPlayer.abilityNetUpdate = true;
        else {
          // Propagate false netUpdate downstream
          foreach (Ability ability in this) ability.netUpdate = false;
        }
      }
    }

    private bool _netUpdate;

    /// <summary>
    /// List names of <see cref="AnimCompatSystem"/>s active by default
    /// in order to block their work, when <see cref="AnimCharacter"/>
    /// with this <see cref="AnimationController"/> is active.
    /// </summary>
    public readonly HashSet<string> AnimCompatSystemBlocklist = new();
    #endregion

    #region Methods - Mod-defined
    // Methods defined by other mods (abstract or empty virtual methods) are kept in this region.

    /// <summary>
    /// Called during <see cref="ModPlayer.Initialize"> ModPlayer.Initialize() </see>, after <see cref="AbilityManager.Initialize"> AbilityManager.Initialize() </see>
    /// Abilities are initialized in order of their <see cref="Ability.Id"/>, from lowest to highest.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Condition for if the player can use any abilities. By default returns <see langword="true"/> as long as the player is alive.
    /// <para>Used to disable all abilities.</para>
    /// </summary>
    /// <returns><see langword="true"/> if any ability can be used; otherwise, <see langword="false"/>.</returns>
    public virtual bool CanUseAnyAbilities() => !player.dead;
    #endregion

    #region Update logic
    /// <summary>
    /// Physics update method
    /// <para>Can be used for synchronized player movement modifications</para>
    /// <para>
    /// Is called even when 
    /// <see cref="Ability.CanUseAnyAbilities"/> is false
    /// </para>
    /// </summary>
    public virtual void PhysicsPreUpdate() { }

    /// <summary>
    /// Update loop for abilities.
    /// <para>If unable to use abilities: disables all abilities.</para>
    /// <para>
    /// Calls <see cref="Ability.PreUpdate"/>, <see cref="Ability.Update"/>, and
    /// <see cref="Ability.PostUpdateAbilities"/> on all unlocked abilities.
    /// Responsible for <see cref="Ability.stateTime"/> and <see cref="Ability.cooldownLeft"/> ticking.
    /// </para>
    /// </summary>
    internal void Update() {
      PhysicsPreUpdate();

      if (!CanUseAnyAbilities()) {
        DisableAllAbilities();
        return;
      }

      // PreUpdate
      foreach (Ability ability in UnlockedAbilities) {
        ability.stateTime++;
        if (ability.Inactive)
          ability.UpdateCooldown();
        ability.PreUpdate();
      }

      // Update
      foreach (Ability ability in UnlockedAbilities) ability.Update();

      // Post-update after all abilities update
      foreach (Ability ability in UnlockedAbilities) ability.PostUpdateAbilities();
    }

    internal void PostUpdate() {
      if (!CanUseAnyAbilities()) return;

      foreach (Ability ability in UnlockedAbilities) ability.PostUpdate();
    }

    /// <summary>
    /// Deactivates all abilities.
    /// </summary>
    public void DisableAllAbilities() {
      foreach (Ability ability in this) ability.SetState(AbilityState.Inactive);
    }

    /// <summary>
    /// Sets the level of all Levelable Abilities to their max level.
    /// </summary>
    public void UnlockAllAbilities() {
      foreach (Ability ability in this) {
        if (ability is ILevelable levelable)
          levelable.Level = levelable.MaxLevel;
      }
    }

    /// <summary>
    /// Sets the level of all Levelable Abilities to 0.
    /// </summary>
    public void ResetAllAbilities() {
      foreach (Ability ability in this) {
        if (ability is ILevelable levelable)
          levelable.Level = 0;
      }
    }
    #endregion

    #region Serializing
    /// <summary>
    /// Serializes all <see cref="Ability"/> data into a new <see cref="TagCompound"/> and returns it.
    /// If <see cref="AutoSave"/> is <see langword="false"/>, you will want to call this in <see cref="ModPlayer.Save"> ModPlayer.Save() </see>
    /// </summary>
    /// <returns>An instance of <see cref="TagCompound"/> containing <see cref="Ability"/> save data.</returns>
    public TagCompound Save() {
      TagCompound tag = new TagCompound();
      foreach (Ability ability in this) {
        TagCompound abilityTag = ability.Save();
        if (abilityTag is null) continue;

        tag.Add(ability.GetType().Name, abilityTag);
      }

      SaveCustomAbilityData(tag);

      return tag;
    }

    /// <summary>
    /// Allows to add additional data to tag compound for that ability manager
    /// </summary>
    /// <param name="tag"> An instance of <see cref="TagCompound"/> containing <see cref="Ability"/> save data. </param>
    public virtual void SaveCustomAbilityData(TagCompound tag) { }

    /// <summary>
    /// Deserializes all <see cref="Ability"/> data from the given <see cref="TagCompound"/>.
    /// If <see cref="AutoSave"/> is <see langword="false"/>, you will want to call this in <see cref="ModPlayer.Load"> ModPlayer.Load() </see>
    /// </summary>
    public void Load(TagCompound tag) {
      foreach (Ability ability in this) {
        string name = ability.GetType().Name;
        if (!tag.ContainsKey(name)) continue;
        TagCompound aTag = tag.Get<TagCompound>(name);
        ability.Load(aTag);
      }
      LoadCustomAbilityData(tag);
    }

    /// <summary>
    /// Allows to get additional data from tag compound for that ability manager
    /// </summary>
    /// <param name="tag"> An instance of <see cref="TagCompound"/> containing <see cref="Ability"/> save data. </param>
    public virtual void LoadCustomAbilityData(TagCompound tag) { }
    #endregion
  }
}

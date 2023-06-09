using System.IO;
using AnimLib.Projectiles;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace AnimLib.Abilities {
  /// <summary>
  /// Base class for all player abilities. Generic type allows you to specify your own type of <see cref="AbilityManager"/>.
  /// </summary>
  /// <typeparam name="TManager">Type of manager you wish to use.</typeparam>
  [PublicAPI]
  public abstract class Ability<TManager> : Ability where TManager : AbilityManager {
    /// <inheritdoc cref="Ability.abilities"/>
    public new TManager abilities {
      get => (TManager)base.abilities;
      internal set => base.abilities = value;
    }
  }

  /// <summary>
  /// Base class for all player abilities.
  /// </summary>
  [PublicAPI]
  [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
  public abstract class Ability {
    #region Properties - Common
    // Initialized properties that are set by AnimLib are kept in this region.

    /// <summary>
    /// The player that this ability belongs to.
    /// </summary>
    // ReSharper disable once NotNullMemberIsNotInitialized
    [NotNull] public Player player { get; internal set; }

    /// <summary>
    /// The <see cref="Mod"/> that this ability belongs to.
    /// </summary>
    [NotNull] public Mod mod => abilities.mod;

    /// <summary>
    /// The <see cref="AbilityManager"/> that this ability belongs to.
    /// </summary>
    // ReSharper disable once NotNullMemberIsNotInitialized
    [NotNull] public AbilityManager abilities { get; internal set; }

    /// <summary>
    /// <see langword="true"/> if this ability belongs to the client <see cref="Player"/> instance.
    /// </summary>
    public bool IsLocal => player.whoAmI == Main.myPlayer;
    #endregion

    #region Properties - Mod-defined
    // Initialized properties that are set by other mods (virtual or abstract properties) are kept in this region.

    /// <summary>
    /// Whether or not you want this ability to be automatically loaded. This only matters if <see cref="AbilityManager.Autoload"/> is also true.
    /// </summary>
    public virtual bool Autoload => mod.ContentAutoloadingEnabled;

    /// <summary>
    /// Id of the ability. This should be a unique number that is different from other ability types.
    /// This determines the order that abilities are updated, with the lower value updated first.
    /// Also used to access Abilities through <see cref="AbilityManager.this[int]"> AbilityManager's array accessor </see>.
    /// </summary>
    /// <remarks>
    /// It's recommended to have your own static "AbilityID" class for managing these IDs.
    /// </remarks>
    /// <example>
    /// <code>
    /// class MyFunAbility : Ability {
    ///   public override int Id => AbilityID.MyFunAbility;
    ///   // omitted for brevity ...
    /// }
    ///
    /// static class AbilityID {
    ///   public static int MyFunAbility => 1;
    ///   public static int MyOtherFunAbility => 2;
    ///   // More IDs ...
    /// }
    /// </code>
    /// </example>
    public abstract int Id { get; }

    /// <summary>
    /// Current level of the ability. If this is 0, the ability is not unlocked and cannot be used.
    /// </summary>
    public virtual int Level => 1;

    /// <summary>
    /// The ability whose level is responsible for this abilities' level. By default, this ability.
    /// <para>Override this if this ability's <see cref="Level"/> is dependent on a different <see cref="Ability"/>'s <see cref="Level"/>.</para>
    /// </summary>
    public virtual ILevelable levelableDependency => this as ILevelable;
    #endregion

    #region Properties - Runtime
    // Properties that are expected to change throughout the ability's lifespan are kept in this region.
    /// <summary>
    /// Condition required for the player to activate this ability.
    /// </summary>
    public virtual bool CanUse => Unlocked && !IsOnCooldown;

    /// <summary>
    /// If true, the ability will be put into the next ability packet.
    /// </summary>
    public bool netUpdate {
      get => _netUpdate;
      protected internal set {
        _netUpdate = value;
        // Propagate true netUpdate upstream
        if (value) abilities.netUpdate = true;
      }
    }

    private bool _netUpdate;

    /// <summary>
    /// Whether or not the player has access to this ability. When <see langword="false"/>, the player cannot use this ability.
    /// </summary>
    public virtual bool Unlocked => true;
    #endregion

    #region State Management
    /// <summary>
    /// Current <see cref="AbilityState"/> of the ability.
    /// <para>Setting this value is done with <see cref="SetState(AbilityState, bool)"/>. This should only be done within <see cref="PreUpdate"/>.</para>
    /// </summary>
    public AbilityState state { get; private set; }

    /// <summary>
    /// Time the ability was in the current State.
    /// <para>This is automatically incremented every frame, and is reset to 0 during <see cref="SetState(AbilityState, bool)"/>.</para>
    /// </summary>
    /// <remarks>
    /// This value is incremented prior to calling <see cref="PreUpdate"/>, so if the state is changed in <see cref="PreUpdate"/>,
    /// this value will start at 0 for any UpdateX() calls.
    /// </remarks>
    public int stateTime { get; internal set; }

    /// <summary>
    /// Sets the <see cref="AbilityState"/> of the ability to <paramref name="abilityState"/>.
    /// <para>This should only be used within the <see cref="PreUpdate"/> method.</para>
    /// </summary>
    /// <param name="abilityState">State to set <see cref="state"/> to.</param>
    /// <param name="preserveCurrentTime">Whether to preserve or reset <see cref="stateTime"/>. Resets by default.</param>
    public void SetState(AbilityState abilityState, bool preserveCurrentTime = false) {
      if (abilityState == state) return;
      netUpdate = true;
      state = abilityState;
      if (!preserveCurrentTime) stateTime = 0;
    }

    /// <summary>
    /// If <see cref="state"/> is <see cref="AbilityState.Inactive"/>.
    /// </summary>
    public bool Inactive => state == AbilityState.Inactive;

    /// <summary>
    /// If <see cref="state"/> is <see cref="AbilityState.Starting"/>.
    /// </summary>
    public bool Starting => state == AbilityState.Starting;

    /// <summary>
    /// If <see cref="state"/> is <see cref="AbilityState.Active"/>.
    /// </summary>
    public bool Active => state == AbilityState.Active;

    /// <summary>
    /// If <see cref="state"/> is <see cref="AbilityState.Ending"/>.
    /// </summary>
    public bool Ending => state == AbilityState.Ending;

    /// <summary>
    /// If <see cref="state"/> is either <see cref="AbilityState.Starting"/>, <see cref="AbilityState.Active"/>, or <see cref="AbilityState.Ending"/>.
    /// </summary>
    /// <value> <see langword="true"/> if the state is either <see cref="AbilityState.Starting"/>, <see cref="AbilityState.Active"/>, or <see cref="AbilityState.Ending"/>, otherwise <see langword="false"/>. </value>
    public bool InUse => Starting || Active || Ending;
    #endregion

    #region Cooldown Management
    /// <summary>
    /// Cooldown of the ability. Indicates the amount of time, in frames, that must pass after ability activation before the ability can be used again.
    /// This is <see langword="0"/> by default.
    /// </summary>
    /// <seealso cref="RefreshCondition"/>
    public virtual int Cooldown => 0;

    /// <summary>
    /// Time left until the ability is no longer on cooldown.
    /// </summary>
    public int cooldownLeft;

    /// <summary>
    /// Whether or not the ability is currently on cooldown.
    /// </summary>
    public bool IsOnCooldown { get; private set; }

    /// <summary>
    /// Set this ability on cooldown.
    /// </summary>
    public virtual void StartCooldown() {
      cooldownLeft = Cooldown;
      IsOnCooldown = true;
    }

    /// <summary>
    /// End the cooldown for this ability, making it ready to use.
    /// </summary>
    public virtual void EndCooldown() {
      cooldownLeft = 0;
      IsOnCooldown = false;
      OnRefreshed();
    }

    /// <summary>
    /// Simple cooldown updating. Can be overridden.
    /// </summary>
    public virtual void UpdateCooldown() {
      if (!IsOnCooldown) return;
      cooldownLeft--;
      if (cooldownLeft <= 0 && RefreshCondition()) EndCooldown();
    }

    /// <summary>
    /// Additional condition required for the ability to go off cooldown.
    /// Return <see langword="false"/> during conditions where the ability should not go off cooldown.
    /// Returns <see langword="true"/> by default.
    /// </summary>
    /// <returns></returns>
    public virtual bool RefreshCondition() => true;

    /// <summary>
    /// Logic that executes immediately after <see cref="EndCooldown"/> is called.
    /// </summary>
    public virtual void OnRefreshed() { }
    #endregion

    #region Update
    /// <summary>
    /// Calls all UpdateX methods in this class.
    /// <para>Called in <see cref="ModPlayer.PostUpdateRunSpeeds"/>, directly after <see cref="PreUpdate"/>.</para>
    /// <para>Only called if <see cref="AbilityState"/> is <see cref="AbilityState.Starting"/>, <see cref="AbilityState.Active"/>, or <see cref="AbilityState.Ending"/>.</para>
    /// </summary>
    internal void Update() {
      switch (state) {
        case AbilityState.Active:
          UpdateActive();
          break;
        case AbilityState.Starting:
          UpdateStarting();
          break;
        case AbilityState.Ending:
          UpdateEnding();
          break;
        case AbilityState.Inactive:
          return;
      }

      UpdateUsing();
    }

    /// <summary>
    /// Called during <see cref="ModPlayer.Initialize"> ModPlayer.Initialize() </see>, after <see cref="AbilityManager.Initialize"> AbilityManager.Initialize() </see>
    /// Abilities are initialized in order of their <see cref="Ability.Id"/>, from lowest to highest.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Called in <see cref="ModPlayer.PostUpdateRunSpeeds"/>, directly before <see cref="Update"/>.
    /// Always called, this should be used primarily for managing <see cref="AbilityState"/>.
    /// </summary>
    /// <remarks>
    /// As some changes are only possible to make on the local client (i.e. dependent on player input), the only changes made in this method should be to state.
    /// If some other changes must be made here and not in any Update methods, they must be synced in <see cref="ReadPacket(BinaryReader)"/> and <see cref="WritePacket(ModPacket)"/>.
    /// </remarks>
    public virtual void PreUpdate() { }


    /// <summary>
    /// Called before <see cref="Update"/> when this <see cref="AbilityState"/> is <see cref="AbilityState.Starting"/>.
    /// </summary>
    public virtual void UpdateStarting() { }

    /// <summary>
    /// Called before <see cref="Update"/> when this <see cref="AbilityState"/> is <see cref="AbilityState.Active"/>.
    /// <para>It is recommended to make any changes to <c> player.control* </c> in <see cref="UpdateUsing"/>, if it is overridden.</para>
    /// </summary>
    public virtual void UpdateActive() { }

    /// <summary>
    /// Called before <see cref="Update"/> when this <see cref="AbilityState"/> is <see cref="AbilityState.Ending"/>.
    /// </summary>
    public virtual void UpdateEnding() { }

    /// <summary>
    /// Called directly after <see cref="UpdateStarting"/>, <see cref="UpdateActive"/>, and <see cref="UpdateEnding"/>.
    /// <para>Any modifications to <c> player.control* </c> should be at the end of this method.</para>
    /// </summary>
    public virtual void UpdateUsing() { }

    /// <summary>
    /// Called after all other Abilities are updated. This is called regardless of current <see cref="AbilityState"/>.
    /// </summary>
    public virtual void PostUpdateAbilities() { }

    /// <summary>
    /// Called in <see cref="ModPlayer.PostUpdate"/>. This is called regardless of current <see cref="AbilityState"/>.
    /// </summary>
    public virtual void PostUpdate() { }
    #endregion

    #region Networking
    /// <summary>
    /// For <see cref="Networking.AbilityPacketHandler"/>.
    /// </summary>
    internal void PreReadPacket([NotNull] BinaryReader r) {
      if (levelableDependency != null)
        levelableDependency.Level = r.ReadInt32();
      state = (AbilityState)r.ReadByte();
      stateTime = r.ReadInt32();
      ReadPacket(r);
    }

    /// <summary>
    /// For <see cref="Networking.AbilityPacketHandler"/>.
    /// </summary>
    internal void PreWritePacket([NotNull] ModPacket packet) {
      if (levelableDependency != null) 
        packet.Write(levelableDependency.Level);
      packet.Write((byte)state);
      packet.Write(stateTime);
      WritePacket(packet);
    }

    /// <summary>
    /// Ability-specific data to read from packet. Use in conjunction with <see cref="WritePacket(ModPacket)"/>.
    /// </summary>
    public virtual void ReadPacket([NotNull] BinaryReader r) { }

    /// <summary>
    /// Ability-specific data to write to packet. Use in conjunction with <see cref="ReadPacket(BinaryReader)"/>.
    /// </summary>
    public virtual void WritePacket([NotNull] ModPacket packet) { }
    #endregion

    #region Serializing
    /// <summary>
    /// Save data that is specific to this <see cref="Ability"/>.
    /// By default saves the ability's level, if it implements <see cref="ILevelable"/>.
    /// </summary>
    /// <returns>A <see cref="TagCompound"/> with data specific to this <see cref="Ability"/>.</returns>
    /// <seealso cref="Load"/>
    [CanBeNull]
    public virtual TagCompound Save() {
      if (this is ILevelable levelable) {
        return new TagCompound {
          [nameof(Level)] = levelable.Level
        };
      }

      return null;
    }

    /// <summary>
    /// Load data that is specific to this <see cref="Ability"/>.
    /// By default loads the ability's level, if it implements <see cref="ILevelable"/>.
    /// </summary>
    /// <param name="tag">The tag to load ability data from.</param>
    /// <seealso cref="Save"/>
    public virtual void Load([NotNull] TagCompound tag) {
      if (this is ILevelable levelable) levelable.Level = tag.GetInt(nameof(Level));
    }
    #endregion

    #region Misc
    /// <summary>
    /// Creates a new <see cref="AbilityProjectile"/> of type <typeparamref name="T"/>.
    /// Assigns <see cref="AbilityProjectile.ability"/> to this ability, and <see cref="AbilityProjectile.level"/> if this ability is <see cref="ILevelable"/>.
    /// </summary>
    /// <param name="offset">Positional offset from the player's center.</param>
    /// <param name="velocity">Starting speed of the projectile.</param>
    /// <param name="damage">Damage value of the projectile.</param>
    /// <param name="knockBack">Knockback strength of the projectile.</param>
    /// <typeparam name="T">Type of ability projectile.</typeparam>
    /// <returns>A new <see cref="AbilityProjectile"/> of type <typeparamref name="T"/>.</returns>
    public T NewAbilityProjectile<T>(Vector2 offset = default, Vector2 velocity = default, int damage = 0, float knockBack = 0)
      where T : AbilityProjectile {
      int type = ModContent.ProjectileType<T>();
      Projectile projectile = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center + offset, velocity, type, damage, knockBack, player.whoAmI);

      T modProjectile = (T)projectile.ModProjectile;
      modProjectile.ability = this;
      if (this is ILevelable levelable)
        modProjectile.level = levelable.Level;
      return modProjectile;
    }

    /// <summary>
    /// String representation of the ability. ID, name, level/max level, current time, and cooldown if applicable.
    /// </summary>
    /// <returns>String with ID, name, level and max level, current time, and cooldown if applicable.</returns>
    public override string ToString() =>
      $"Ability ID:{Id} Name:{GetType().Name} " +
      $"(Level {Level}{(this is ILevelable levelable ? $"/{levelable.MaxLevel}" : string.Empty)}) " +
      $"Player:{player.whoAmI} State:{state} " +
      $"Time:{stateTime} " +
      (Cooldown != 0 ? $" Cooldown:{cooldownLeft}/{Cooldown}" : string.Empty);

    /// <summary>
    /// Whether or not the <see cref="Ability"/> is in use.
    /// </summary>
    /// <seealso cref="InUse"/>
    public static implicit operator bool(Ability ability) => !(ability is null) && ability.InUse;
    #endregion
  }

  // ReSharper disable once InconsistentNaming
  internal class __LevelableAbility : Ability, ILevelable {
    private int _level;
    public override int Id => -1;
    public override int Level => _level;

    int ILevelable.Level {
      get => _level;
      set => _level = value;
    }

    public int MaxLevel => 0;
  }
}

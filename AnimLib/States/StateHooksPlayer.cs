using System.Linq;
using JetBrains.Annotations;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.Core;

namespace AnimLib.States;

/// <summary>
/// A <see cref="ModPlayer"/> which conditionally passes all hooks the player's <see cref="State"/> instances.
/// <para/> Most hooks for a state require at least their <see cref="State.Character"/> to be <see cref="State.Active"/>.
/// <para/> Hooks which are called every frame require the State itself to be active
/// (except <see cref="PreUpdate"/> and <see cref="PostUpdate"/>, which are always called).
/// <para/> Some hooks are always called, such as <see cref="State.Initialize"/>, <see cref="State.ProcessTriggers"/>,
/// <see cref="State.PlayerConnect"/>, <see cref="State.PreSavePlayer"/>, <see cref="State.OnEnterWorld"/>.
/// </summary>
/// <remarks>
/// Some additional logic for States is called during these methods,
/// during <see cref="PreUpdate"/>, <see cref="PostUpdate"/>, and <see cref="ProcessTriggers"/>.
/// </remarks>
[UsedImplicitly]
internal sealed class StateHooksPlayer : ModPlayer {
  #region Enumeration Methods

  [field: AllowNull, MaybeNull]
  private State[] States => field ??= Player.GetStates();

  /// <summary>
  /// Updates all states which have hooks.
  /// <para /> Used primarily for one-time hooks, net-syncing,
  /// <see cref="State.PreUpdate"/> and <see cref="State.PostUpdate"/>, and saving.
  /// </summary>
  private ConditionalEnumerator<State> EnumerateAll(HookList<State> list) {
    return States.Length > 0
      ? new ConditionalEnumerator<State>(list.Enumerate(States))
      : ConditionalEnumerator<State>.Empty;
  }

  /// <summary>
  /// Common enumerator for states that should hook update.
  /// <br /> Updates when the character is active, and the state is not an ability which is locked.
  /// <para /> Used for nearly all hooks.
  /// </summary>
  private ConditionalEnumerator<State> EnumerateActiveCharacterStates(HookList<State> list) {
    return States.Length > 0
      ? new ConditionalEnumerator<State>(list.Enumerate(States), ShouldHookUpdate)
      : ConditionalEnumerator<State>.Empty;
  }

  private static bool ShouldHookUpdate(State state) {
    return state switch {
      AnimCharacterCollection => true,
      AnimCharacter => state.Active,
      AbilityState { Unlocked: false } => false,
      _ => state.Character?.Active ?? false
    };
  }

  /// <summary>
  /// Restrictive enumerator, requires the state to be active.
  /// <para /> Used primarily on hooks which update every frame.
  /// </summary>
  private ConditionalEnumerator<State> EnumerateActiveStates(HookList<State> list) {
    return States.Length > 0
      ? new ConditionalEnumerator<State>(list.Enumerate(States), state => state.Active)
      : ConditionalEnumerator<State>.Empty;
  }

  private bool TryGetActiveCharacter([NotNullWhen(true)] out AnimCharacter? character) {
    character = Player.GetActiveCharacter();
    return character is not null;
  }

  #endregion

  #region Hooks

  public override void Initialize() {
    foreach (State state in EnumerateAll(StateLoader.HookInitialize)) {
      state.Initialize();
    }

    foreach (State state in EnumerateAll(StateLoader.HookPostInitialize)) {
      state.PostInitialize();
    }
  }

  public override void ResetEffects() {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookResetEffects)) {
      state.ResetEffects();
    }
  }

  public override void ResetInfoAccessories() {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookResetInfoAccessories)) {
      state.ResetInfoAccessories();
    }
  }

  public override void RefreshInfoAccessoriesFromTeamPlayers(Player player) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookRefreshInfoAccessoriesFromTeamPlayers)) {
      state.RefreshInfoAccessoriesFromTeamPlayers(player);
    }
  }

  public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
    base.ModifyMaxStats(out health, out mana);
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyMaxStats)) {
      state.ModifyMaxStats(out health, out mana);
    }
  }

  public override void UpdateDead() {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookUpdateDead)) {
      state.UpdateDead();
    }
  }

  public override void PreSavePlayer() {
    foreach (State state in EnumerateAll(StateLoader.HookPreSavePlayer)) {
      state.PreSavePlayer();
    }
  }

  public override void PostSavePlayer() {
    foreach (State state in EnumerateAll(StateLoader.HookPostSavePlayer)) {
      state.PostSavePlayer();
    }
  }

  public override void CopyClientState(ModPlayer clientPlayer) {
    foreach (State state in EnumerateAll(StateLoader.HookCopyClientState)) {
      state.CopyClientState(clientPlayer);
    }
  }

  public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
    foreach (State state in EnumerateAll(StateLoader.HookSyncPlayer)) {
      state.SyncPlayer(toWho, fromWho, newPlayer);
    }
  }

  public override void SendClientChanges(ModPlayer clientPlayer) {
    foreach (State state in EnumerateAll(StateLoader.HookSendClientChanges)) {
      state.SendClientChanges(clientPlayer);
    }
  }

  public override void UpdateBadLifeRegen() {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookUpdateBadLifeRegen)) {
      state.UpdateBadLifeRegen();
    }
  }

  public override void UpdateLifeRegen() {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookUpdateLifeRegen)) {
      state.UpdateLifeRegen();
    }
  }

  public override void NaturalLifeRegen(ref float regen) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookNaturalLifeRegen)) {
      state.NaturalLifeRegen(ref regen);
    }
  }

  public override void UpdateAutopause() {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookUpdateAutopause)) {
      state.UpdateAutoPause();
    }
  }

  public override void PreUpdate() {
    foreach (State state in States) {
      if (state.Active) {
        state.ActiveTime++;
      }
      else if (state.InactiveTime is >= 0 and < int.MaxValue) {
        state.InactiveTime++;
      }

      if (state is AnimCharacter character) {
        character.UpdateConditions();
      }

      if (state is AbilityState ability) {
        ability.UpdateCooldown();
      }
    }

    foreach (State state in EnumerateAll(StateLoader.HookPreUpdate)) {
      state.PreUpdate();
    }
  }

  public override void ProcessTriggers(TriggersSet triggersSet) {
    foreach (State state in EnumerateAll(StateLoader.HookProcessTriggers)) {
      state.ProcessTriggers(triggersSet);
    }

    if (!TryGetActiveCharacter(out AnimCharacter? character)) {
      return;
    }

    foreach (StateMachine state in character.AllStatesArray.OfType<StateMachine>()) {
      if (state.Active) {
        state.UpdateInterruptChildren();
      }
    }
  }

  public override void ArmorSetBonusActivated() {
    foreach (State state in EnumerateAll(StateLoader.HookArmorSetBonusActivated)) {
      state.ArmorSetBonusActivated();
    }
  }

  public override void ArmorSetBonusHeld(int type) {
    foreach (State state in EnumerateAll(StateLoader.HookArmorSetBonusHeld)) {
      state.ArmorSetBonusHeld(type);
    }
  }

  public override void SetControls() {
    foreach (State state in EnumerateActiveStates(StateLoader.HookSetControls)) {
      state.SetControls();
    }
  }

  public override void PreUpdateBuffs() {
    foreach (State state in EnumerateActiveStates(StateLoader.HookPreUpdateBuffs)) {
      state.PreUpdateBuffs();
    }
  }

  public override void PostUpdateBuffs() {
    foreach (State state in EnumerateActiveStates(StateLoader.HookPostUpdateBuffs)) {
      state.PostUpdateBuffs();
    }
  }

  public override void UpdateEquips() {
    foreach (State state in EnumerateActiveStates(StateLoader.HookUpdateEquips)) {
      state.UpdateEquips();
    }
  }

  public override void PostUpdateEquips() {
    foreach (State state in EnumerateActiveStates(StateLoader.HookPostUpdateEquips)) {
      if (state.Active) {
        state.PostUpdateEquips();
      }
    }
  }

  public override void UpdateVisibleAccessories() {
    foreach (State state in EnumerateActiveStates(StateLoader.HookUpdateVisibleAccessories)) {
      state.UpdateVisibleAccessories();
    }
  }

  public override void UpdateVisibleVanityAccessories() {
    foreach (State state in EnumerateActiveStates(StateLoader.HookUpdateVisibleVanityAccessories)) {
      state.UpdateVisibleVanityAccessories();
    }
  }

  public override void UpdateDyes() {
    foreach (State state in EnumerateActiveStates(StateLoader.HookUpdateDyes)) {
      state.UpdateDyes();
    }
  }

  public override void PostUpdateMiscEffects() {
    foreach (State state in EnumerateActiveStates(StateLoader.HookPostUpdateMiscEffects)) {
      state.PostUpdateMiscEffects();
    }
  }

  public override void PostUpdateRunSpeeds() {
    foreach (State state in EnumerateActiveStates(StateLoader.HookPostUpdateRunSpeeds)) {
      state.PostUpdateRunSpeeds();
    }
  }

  public override void PreUpdateMovement() {
    foreach (State state in EnumerateActiveStates(StateLoader.HookPreUpdateMovement)) {
      state.PreUpdateMovement();
    }
  }

  public override void PostUpdate() {
    foreach (State state in States) {
      if (state is AnimCharacter character) {
        character.UpdateConditionsPost();
      }
    }

    foreach (State state in EnumerateAll(StateLoader.HookPostUpdate)) {
      state.PostUpdate();
    }
  }

  public override void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyExtraJumpDurationMultiplier)) {
      state.ModifyExtraJumpDurationMultiplier(jump, ref duration);
    }
  }

  public override bool CanStartExtraJump(ExtraJump jump) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanStartExtraJump)) {
      if (!state.CanStartExtraJump(jump)) {
        return false;
      }
    }

    return base.CanStartExtraJump(jump);
  }

  public override void OnExtraJumpStarted(ExtraJump jump, ref bool playSound) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookExtraJumpStarted)) {
      state.OnExtraJumpStarted(jump, ref playSound);
    }
  }

  public override void OnExtraJumpEnded(ExtraJump jump) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnExtraJumpEnded)) {
      state.OnExtraJumpEnded(jump);
    }
  }

  public override void OnExtraJumpRefreshed(ExtraJump jump) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnExtraJumpRefreshed)) {
      state.OnExtraJumpRefreshed(jump);
    }
  }

  public override void ExtraJumpVisuals(ExtraJump jump) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookExtraJumpVisuals)) {
      state.ExtraJumpVisuals(jump);
    }
  }

  public override bool CanShowExtraJumpVisuals(ExtraJump jump) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanShowExtraJumpVisuals)) {
      if (!state.CanShowExtraJumpVisuals(jump)) {
        return false;
      }
    }

    return true;
  }

  public override void OnExtraJumpCleared(ExtraJump jump) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnExtraJumpCleared)) {
      state.OnExtraJumpCleared(jump);
    }
  }

  public override void FrameEffects() {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookFrameEffects)) {
      state.FrameEffects();
    }
  }

  public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownSlot, bool pvp) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookImmuneTo)) {
      if (state.ImmuneTo(damageSource, cooldownSlot, pvp)) {
        return true;
      }
    }

    return false;
  }

  public override bool FreeDodge(Player.HurtInfo info) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookFreeDodge)) {
      if (state.FreeDodge(info)) {
        return true;
      }
    }

    return false;
  }

  public override bool ConsumableDodge(Player.HurtInfo info) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookConsumableDodge)) {
      if (state.ConsumableDodge(info)) {
        return true;
      }
    }

    return base.ConsumableDodge(info);
  }

  public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyHurt)) {
      state.ModifyHurt(ref modifiers);
    }
  }

  public override void OnHurt(Player.HurtInfo info) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnHurt)) {
      state.OnHurt(info);
    }
  }

  public override void PostHurt(Player.HurtInfo info) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookPostHurt)) {
      state.PostHurt(info);
    }
  }

  public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore,
    ref PlayerDeathReason damageSource) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookPreKill)) {
      if (!state.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource)) {
        return false;
      }
    }

    return true;
  }

  public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookKill)) {
      state.Kill(damage, hitDirection, pvp, damageSource);
    }
  }

  public override bool PreModifyLuck(ref float luck) {
    bool result = true;
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookPreModifyLuck)) {
      result &= state.PreModifyLuck(ref luck);
    }

    return result;
  }

  public override void ModifyLuck(ref float luck) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyLuck)) {
      state.ModifyLuck(ref luck);
    }
  }

  public override bool PreItemCheck() {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookPreItemCheck)) {
      if (!state.PreItemCheck()) {
        return false;
      }
    }

    return true;
  }

  public override void PostItemCheck() {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookPostItemCheck)) {
      state.PostItemCheck();
    }
  }

  public override float UseTimeMultiplier(Item item) {
    float result = base.UseTimeMultiplier(item);
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookUseTimeMultiplier)) {
      result *= state.UseTimeMultiplier(item);
    }

    return result;
  }

  public override float UseAnimationMultiplier(Item item) {
    float result = base.UseAnimationMultiplier(item);
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookUseAnimationMultiplier)) {
      result *= state.UseAnimationMultiplier(item);
    }

    return result;
  }

  public override float UseSpeedMultiplier(Item item) {
    float result = base.UseSpeedMultiplier(item);
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookUseSpeedMultiplier)) {
      result *= state.UseSpeedMultiplier(item);
    }

    return result;
  }

  public override void GetHealLife(Item item, bool quickHeal, ref int healValue) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookGetHealLife)) {
      state.GetHealLife(item, quickHeal, ref healValue);
    }
  }

  public override void GetHealMana(Item item, bool quickHeal, ref int healValue) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookGetHealMana)) {
      state.GetHealMana(item, quickHeal, ref healValue);
    }
  }

  public override void ModifyManaCost(Item item, ref float reduce, ref float mult) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyManaCost)) {
      state.ModifyManaCost(item, ref reduce, ref mult);
    }
  }

  public override void OnMissingMana(Item item, int neededMana) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnMissingMana)) {
      state.OnMissingMana(item, neededMana);
    }
  }

  public override void OnConsumeMana(Item item, int manaConsumed) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnConsumeMana)) {
      state.OnConsumeMana(item, manaConsumed);
    }
  }

  public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyWeaponDamage)) {
      state.ModifyWeaponDamage(item, ref damage);
    }
  }

  public override void ModifyWeaponKnockback(Item item, ref StatModifier knockback) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyWeaponKnockback)) {
      state.ModifyWeaponKnockback(item, ref knockback);
    }
  }

  public override void ModifyWeaponCrit(Item item, ref float crit) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyWeaponCrit)) {
      state.ModifyWeaponCrit(item, ref crit);
    }
  }

  public override bool CanConsumeAmmo(Item weapon, Item ammo) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanConsumeAmmo)) {
      if (!state.CanConsumeAmmo(weapon, ammo)) {
        return false;
      }
    }

    return true;
  }

  public override void OnConsumeAmmo(Item weapon, Item ammo) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnConsumeAmmo)) {
      state.OnConsumeAmmo(weapon, ammo);
    }
  }

  public override bool CanShoot(Item item) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanShoot)) {
      if (!state.CanShoot(item)) {
        return false;
      }
    }

    return true;
  }

  public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type,
    ref int damage, ref float knockback) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyShootStats)) {
      state.ModifyShootStats(item, ref position, ref velocity, ref type, ref damage, ref knockback);
    }
  }

  public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
    int type, int damage, float knockback) {
    bool result = true;
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookShoot)) {
      result &= state.Shoot(item, source, position, velocity, type, damage, knockback);
    }

    return result;
  }

  public override void MeleeEffects(Item item, Rectangle hitbox) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookMeleeEffects)) {
      state.MeleeEffects(item, hitbox);
    }
  }

  public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 position, int width, int height) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookEmitEnchantmentVisualsAt)) {
      state.EmitEnchantmentVisualsAt(projectile, position, width, height);
    }
  }

  public override bool? CanCatchNPC(NPC npc, Item item) {
    bool? result = null;
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanCatchNPC)) {
      if (state.CanCatchNPC(npc, item) is not { } value) {
        continue;
      }

      if (!value) {
        return false;
      }

      result = true;
    }

    return result;
  }

  public override void OnCatchNPC(NPC npc, Item item, bool caught) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnCatchNPC)) {
      state.OnCatchNPC(npc, item, caught);
    }
  }

  public override void ModifyItemScale(Item item, ref float scale) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyItemScale)) {
      state.ModifyItemScale(item, ref scale);
    }
  }

  public override void OnHitAnything(float x, float y, Entity victim) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnHitAnything)) {
      state.OnHitAnything(x, y, victim);
    }
  }

  public override bool CanHitNPC(NPC target) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanHitNPC)) {
      if (!state.CanHitNPC(target)) {
        return false;
      }
    }

    return true;
  }

  public override bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle hitbox, NPC target) {
    bool? result = null;
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanMeleeAttackCollideWithNPC)) {
      if (state.CanMeleeAttackCollideWithNPC(item, hitbox, target) is not { } value) {
        continue;
      }

      if (!value) {
        return false;
      }

      result = true;
    }

    return result;
  }

  public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyHitNPC)) {
      state.ModifyHitNPC(target, ref modifiers);
    }
  }

  public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnHitNPC)) {
      state.OnHitNPC(target, hit, damageDone);
    }
  }

  public override bool? CanHitNPCWithItem(Item item, NPC target) {
    bool? result = null;
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanHitNPCWithItem)) {
      if (state.CanHitNPCWithItem(item, target) is not { } value) {
        continue;
      }

      if (!value) {
        return false;
      }

      result = true;
    }

    return result;
  }

  public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyHitNPCWithItem)) {
      state.ModifyHitNPCWithItem(item, target, ref modifiers);
    }
  }

  public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnHitNPCWithItem)) {
      state.OnHitNPCWithItem(item, target, hit, damageDone);
    }
  }

  public override bool? CanHitNPCWithProj(Projectile proj, NPC target) {
    bool? result = null;
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanHitNPCWithProj)) {
      if (state.CanHitNPCWithProj(proj, target) is not { } value) {
        continue;
      }

      if (!value) {
        return false;
      }

      result = true;
    }

    return result;
  }

  public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyHitNPCWithProj)) {
      state.ModifyHitNPCWithProj(proj, target, ref modifiers);
    }
  }

  public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnHitNPCWithProj)) {
      state.OnHitNPCWithProj(proj, target, hit, damageDone);
    }
  }

  public override bool CanHitPvp(Item item, Player target) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanHitPvp)) {
      if (!state.CanHitPvp(item, target)) {
        return false;
      }
    }

    return true;
  }

  public override bool CanHitPvpWithProj(Projectile proj, Player target) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanHitPvpWithProj)) {
      if (!state.CanHitPvpWithProj(proj, target)) {
        return false;
      }
    }

    return true;
  }

  public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanBeHitByNPC)) {
      if (!state.CanBeHitByNPC(npc, ref cooldownSlot)) {
        return false;
      }
    }

    return true;
  }

  public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyHitByNPC)) {
      state.ModifyHitByNPC(npc, ref modifiers);
    }
  }

  public override void OnHitByNPC(NPC npc, Player.HurtInfo info) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnHitByNPC)) {
      state.OnHitByNPC(npc, info);
    }
  }

  public override bool CanBeHitByProjectile(Projectile proj) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanBeHitByProjectile)) {
      if (!state.CanBeHitByProjectile(proj)) {
        return false;
      }
    }

    return true;
  }

  public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyHitByProjectile)) {
      state.ModifyHitByProjectile(proj, ref modifiers);
    }
  }

  public override void OnHitByProjectile(Projectile proj, Player.HurtInfo info) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnHitByProjectile)) {
      state.OnHitByProjectile(proj, info);
    }
  }

  public override void ModifyFishingAttempt(ref FishingAttempt attempt) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyFishingAttempt)) {
      state.ModifyFishingAttempt(ref attempt);
    }
  }

  public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int enemySpawn,
    ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCatchFish)) {
      state.CatchFish(attempt, ref itemDrop, ref enemySpawn, ref sonar, ref sonarPosition);
    }
  }

  public override void ModifyCaughtFish(Item fish) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyCaughtFish)) {
      state.ModifyCaughtFish(fish);
    }
  }

  public override bool? CanConsumeBait(Item bait) {
    bool? result = null;
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanConsumeBait)) {
      if (state.CanConsumeBait(bait) is { } value) {
        result = (result ?? true) && value;
      }
    }

    return result;
  }

  public override void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookGetFishingLevel)) {
      state.GetFishingLevel(fishingRod, bait, ref fishingLevel);
    }
  }

  public override void AnglerQuestReward(float rarityMultiplier, List<Item> rewardItems) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookAnglerQuestReward)) {
      state.AnglerQuestReward(rarityMultiplier, rewardItems);
    }
  }

  public override void GetDyeTraderReward(List<int> rewardPool) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookGetDyeTraderReward)) {
      state.GetDyeTraderReward(rewardPool);
    }
  }

  public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a,
    ref bool fullBright) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookDrawEffects)) {
      state.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);
    }
  }

  public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyDrawInfo)) {
      state.ModifyDrawInfo(ref drawInfo);
    }
  }

  public override void HideDrawLayers(PlayerDrawSet drawInfo) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyDrawLayers)) {
      state.HideDrawLayers(drawInfo);
    }
  }

  public override void ModifyScreenPosition() {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyScreenPosition)) {
      state.ModifyScreenPosition();
    }
  }

  public override void ModifyZoom(ref float zoom) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyZoom)) {
      state.ModifyZoom(ref zoom);
    }
  }

  public override void PlayerConnect() {
    foreach (State state in EnumerateAll(StateLoader.HookPlayerConnect)) {
      state.PlayerConnect();
    }
  }

  public override void PlayerDisconnect() {
    foreach (State state in EnumerateAll(StateLoader.HookPlayerDisconnect)) {
      state.PlayerDisconnect();
    }
  }

  public override void OnEnterWorld() {
    foreach (State state in EnumerateAll(StateLoader.HookOnEnterWorld)) {
      state.OnEnterWorld();
    }
  }

  public override void OnRespawn() {
    foreach (State state in EnumerateAll(StateLoader.HookOnRespawn)) {
      state.OnRespawn();
    }
  }

  public override bool ShiftClickSlot(Item[] inventory, int context, int slot) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookShiftClickSlot)) {
      if (!state.ShiftClickSlot(inventory, context, slot)) {
        return false;
      }
    }

    return true;
  }

  public override bool HoverSlot(Item[] inventory, int context, int slot) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookHoverSlot)) {
      if (!state.HoverSlot(inventory, context, slot)) {
        return false;
      }
    }

    return true;
  }

  public override void PostSellItem(NPC npc, Item[] shopInventory, Item item) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookPostSellItem)) {
      state.PostSellItem(npc, shopInventory, item);
    }
  }

  public override bool CanSellItem(NPC npc, Item[] shopInventory, Item item) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanSellItem)) {
      if (!state.CanSellItem(npc, shopInventory, item)) {
        return false;
      }
    }

    return true;
  }

  public override void PostBuyItem(NPC npc, Item[] shopInventory, Item item) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookPostBuyItem)) {
      state.PostBuyItem(npc, shopInventory, item);
    }
  }

  public override bool CanBuyItem(NPC npc, Item[] shopInventory, Item item) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanBuyItem)) {
      if (!state.CanBuyItem(npc, shopInventory, item)) {
        return false;
      }
    }

    return true;
  }

  public override bool CanUseItem(Item item) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanUseItem)) {
      if (!state.CanUseItem(item)) {
        return false;
      }
    }

    return true;
  }

  public override bool? CanAutoReuseItem(Item item) {
    bool? result = null;
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookCanAutoReuseItem)) {
      if (state.CanAutoReuseItem(item) is not { } value) {
        continue;
      }

      if (!value) {
        return false;
      }

      result = true;
    }

    return result;
  }

  public override bool ModifyNurseHeal(NPC npc, ref int health, ref bool removeDebuffs, ref string chatText) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyNurseHeal)) {
      if (!state.ModifyNurseHeal(npc, ref health, ref removeDebuffs, ref chatText)) {
        return false;
      }
    }

    return true;
  }

  public override void ModifyNursePrice(NPC npc, int health, bool removeDebuffs, ref int price) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookModifyNursePrice)) {
      state.ModifyNursePrice(npc, health, removeDebuffs, ref price);
    }
  }

  public override void PostNurseHeal(NPC npc, int health, bool removeDebuffs, int price) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookPostNurseHeal)) {
      state.PostNurseHeal(npc, health, removeDebuffs, price);
    }
  }

  public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) {
    return TryGetActiveCharacter(out AnimCharacter? character)
      ? character.AddStartingItems(mediumCoreDeath)
      : [];
  }

  public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> startingInventory,
    bool mediumCoreDeath) {
    if (TryGetActiveCharacter(out AnimCharacter? character)) {
      character.ModifyStartingInventory(startingInventory, mediumCoreDeath);
    }
  }

  public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback onUsedForCrafting) {
    return TryGetActiveCharacter(out AnimCharacter? character)
      ? character.AddMaterialsForCrafting(out onUsedForCrafting!)
      : base.AddMaterialsForCrafting(out onUsedForCrafting);
  }

  public override bool OnPickup(Item item) {
    foreach (State state in EnumerateActiveCharacterStates(StateLoader.HookOnPickup)) {
      if (!state.OnPickup(item)) {
        return false;
      }
    }

    return true;
  }

  #endregion

  /// <summary>
  /// Struct iterator which wraps around <see cref="FilteredArrayEnumerator{T}"/> and applies a condition.
  /// </summary>
  /// <param name="enumerator">The hook list enumerator to wrap around.</param>
  /// <param name="condition">The condition to apply for each element to be yielded.</param>
  /// <typeparam name="T">The type of the elements in the array.</typeparam>
  private ref struct ConditionalEnumerator<T>(FilteredArrayEnumerator<T> enumerator, Func<T, bool>? condition = null)
    where T : class {
    private FilteredArrayEnumerator<T> _enumerator = enumerator;

    public T Current { get; private set; } = null!;

    public bool MoveNext() {
      while (_enumerator.MoveNext()) {
        if (condition is null || condition(_enumerator.Current)) {
          Current = _enumerator.Current;
          return true;
        }
      }

      return false;
    }

    public ConditionalEnumerator<T> GetEnumerator() => this;

    public static ConditionalEnumerator<T> Empty => new(new FilteredArrayEnumerator<T>([], []));
  }
}

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using AnimLib.Menus.Debug;
using JetBrains.Annotations;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.Core;

namespace AnimLib.States;

/// <summary>
/// Largely a copy of <see cref="PlayerLoader"/>, but for <see cref="State"/>s.
/// </summary>
[UsedImplicitly]
public sealed class StateLoader : ModSystem {
  public static StateLoader Instance => ModContent.GetInstance<StateLoader>();

  private static readonly List<StateHookList> HookLists = [];
  internal static readonly List<State> TemplateStates = [];
  internal static readonly List<AnimCharacter> SelectableCharacters = [];

  internal static StateHierarchy[] TemplateHierarchy { get; private set; } = null!; // ResizeArrays

  internal static void Add(State state) {
    state.Index = (ushort)TemplateStates.Count;
    TemplateStates.Add(state);
    if (state is AnimCharacter { Selectable: true } character) {
      SelectableCharacters.Add(character);
    }
  }

  private static StateHookList AddHook<T>(Expression<Func<State, T>> func) where T : Delegate {
    StateHookList hookList = StateHookList.Create(func);
    HookLists.Add(hookList);
    return hookList;
  }

  public override void ResizeArrays() {
    TemplateHierarchy = StateHierarchy.ResizeArrays(CollectionsMarshal.AsSpan(TemplateStates));

    foreach (StateHookList hookList in HookLists) {
      hookList.Update(TemplateStates);
    }
  }

  public override void Unload() {
    HookLists.Clear();
    TemplateStates.Clear();
    SelectableCharacters.Clear();
    TemplateHierarchy = null!;

    // tML does not clear the cache for MethodOverrideQuery, so we have to do it ourselves
    Type type = typeof(LoaderUtils.MethodOverrideQuery<State>);
    FieldInfo? cacheField = type.GetField("_cache", BindingFlags.Static | BindingFlags.NonPublic);
    var cache = (ConcurrentDictionary<MethodInfo, LoaderUtils.MethodOverrideQuery<State>>)cacheField!.GetValue(null)!;
    cache.Clear();
  }

  internal static void NewInstance(AnimPlayer animPlayer) {
    var templateStates = CollectionsMarshal.AsSpan(TemplateStates);
    var states = new State[templateStates.Length];
    animPlayer.States = states;

    foreach (State templateState in templateStates) {
      State newState = templateState.NewInstance(animPlayer.Player);
      newState.AllStatesArray = states;
      newState.Hierarchy = templateState.Hierarchy;
      states[templateState.Index] = newState;
    }

    // Assign State.Parent, and .Character
    foreach (State state in states) {
      int parentId = state.Hierarchy.ParentId;
      if (parentId != -1) {
        state.Parent = states[parentId];
      }

      // Assign State.Character to self
      if (state is AnimCharacter animCharacter) {
        animCharacter.Character = animCharacter;
        continue;
      }

      // Assign State.Character to the Character in parents
      ushort[] parentIds = state.Hierarchy.ParentIds;
      for (int i = parentIds.Length - 1; i >= 0; i--) {
        if (state.GetState(parentIds[i]) is AnimCharacter character) {
          state.Character = character;
          break;
        }
      }
    }
  }

  public readonly StateHookList DebugText =
    AddHook<Action<UIStateInfo>>(s => s.DebugText);

  public readonly StateHookList HookInitialize =
    AddHook<Action>(s => s.Initialize);

  public readonly StateHookList HookPostInitialize =
    AddHook<Action>(s => s.PostInitialize);

  public readonly StateHookList HookResetEffects =
    AddHook<Action>(s => s.ResetEffects);

  public readonly StateHookList HookResetInfoAccessories =
    AddHook<Action>(s => s.ResetInfoAccessories);

  public readonly StateHookList HookRefreshInfoAccessoriesFromTeamPlayers =
    AddHook<Action<Player>>(s => s.RefreshInfoAccessoriesFromTeamPlayers);

  private delegate void DelegateModifyMaxStats(out StatModifier health, out StatModifier mana);

  public readonly StateHookList HookModifyMaxStats =
    AddHook<DelegateModifyMaxStats>(s => s.ModifyMaxStats);

  public readonly StateHookList HookUpdateDead =
    AddHook<Action>(s => s.UpdateDead);

  public readonly StateHookList HookPreSavePlayer =
    AddHook<Action>(s => s.PreSavePlayer);

  public readonly StateHookList HookPostSavePlayer =
    AddHook<Action>(s => s.PostSavePlayer);

  public readonly StateHookList HookCopyClientState =
    AddHook<Action<ModPlayer>>(s => s.CopyClientState);

  public readonly StateHookList HookSyncPlayer =
    AddHook<Action<int, int, bool>>(s => s.SyncPlayer);

  public readonly StateHookList HookSendClientChanges =
    AddHook<Action<ModPlayer>>(s => s.SendClientChanges);

  public readonly StateHookList HookUpdateBadLifeRegen =
    AddHook<Action>(s => s.UpdateBadLifeRegen);

  public readonly StateHookList HookUpdateLifeRegen =
    AddHook<Action>(s => s.UpdateLifeRegen);

  private delegate void DelegateNaturalLifeRegen(ref float regen);

  public readonly StateHookList HookNaturalLifeRegen =
    AddHook<DelegateNaturalLifeRegen>(s => s.NaturalLifeRegen);

  public readonly StateHookList HookUpdateAutopause =
    AddHook<Action>(s => s.UpdateAutoPause);

  public readonly StateHookList HookPreUpdate =
    AddHook<Action>(s => s.PreUpdate);

  public readonly StateHookList HookSetControls =
    AddHook<Action>(s => s.SetControls);

  public readonly StateHookList HookPreUpdateBuffs =
    AddHook<Action>(s => s.PreUpdateBuffs);

  public readonly StateHookList HookPostUpdateBuffs =
    AddHook<Action>(s => s.PostUpdateBuffs);

  public readonly StateHookList HookUpdateEquips =
    AddHook<Action>(s => s.UpdateEquips);

  public readonly StateHookList HookPostUpdateEquips =
    AddHook<Action>(s => s.PostUpdateEquips);

  public readonly StateHookList HookUpdateVisibleAccessories =
    AddHook<Action>(s => s.UpdateVisibleAccessories);

  public readonly StateHookList HookUpdateVisibleVanityAccessories =
    AddHook<Action>(s => s.UpdateVisibleVanityAccessories);

  public readonly StateHookList HookUpdateDyes =
    AddHook<Action>(s => s.UpdateDyes);

  public readonly StateHookList HookPostUpdateMiscEffects =
    AddHook<Action>(s => s.PostUpdateMiscEffects);

  public readonly StateHookList HookPostUpdateRunSpeeds =
    AddHook<Action>(s => s.PostUpdateRunSpeeds);

  public readonly StateHookList HookPreUpdateMovement =
    AddHook<Action>(s => s.PreUpdateMovement);

  public readonly StateHookList HookPostUpdate =
    AddHook<Action>(s => s.PostUpdate);

  private delegate void DelegateModifyExtraJumpDuration(ExtraJump jump, ref float duration);

  public readonly StateHookList HookModifyExtraJumpDurationMultiplier =
    AddHook<DelegateModifyExtraJumpDuration>(s => s.ModifyExtraJumpDurationMultiplier);

  public readonly StateHookList HookCanStartExtraJump =
    AddHook<Func<ExtraJump, bool>>(s => s.CanStartExtraJump);

  private delegate void DelegateExtraJumpStarted(ExtraJump jump, ref bool playSound);

  public readonly StateHookList HookExtraJumpStarted =
    AddHook<DelegateExtraJumpStarted>(s => s.OnExtraJumpStarted);

  public readonly StateHookList HookOnExtraJumpEnded =
    AddHook<Action<ExtraJump>>(s => s.OnExtraJumpEnded);

  public readonly StateHookList HookOnExtraJumpRefreshed =
    AddHook<Action<ExtraJump>>(s => s.OnExtraJumpRefreshed);

  public readonly StateHookList HookExtraJumpVisuals =
    AddHook<Action<ExtraJump>>(s => s.ExtraJumpVisuals);

  public readonly StateHookList HookCanShowExtraJumpVisuals =
    AddHook<Func<ExtraJump, bool>>(s => s.CanShowExtraJumpVisuals);

  public readonly StateHookList HookOnExtraJumpCleared =
    AddHook<Action<ExtraJump>>(s => s.OnExtraJumpCleared);

  public readonly StateHookList HookFrameEffects =
    AddHook<Action>(s => s.FrameEffects);

  public readonly StateHookList HookImmuneTo =
    AddHook<Func<PlayerDeathReason, int, bool, bool>>(s => s.ImmuneTo);

  public readonly StateHookList HookFreeDodge =
    AddHook<Func<Player.HurtInfo, bool>>(s => s.FreeDodge);

  public readonly StateHookList HookConsumableDodge =
    AddHook<Func<Player.HurtInfo, bool>>(s => s.ConsumableDodge);

  private delegate void DelegateModifyHurt(ref Player.HurtModifiers modifiers);

  public readonly StateHookList HookModifyHurt =
    AddHook<DelegateModifyHurt>(s => s.ModifyHurt);

  public readonly StateHookList HookOnHurt =
    AddHook<Action<Player.HurtInfo>>(s => s.OnHurt);

  public readonly StateHookList HookPostHurt =
    AddHook<Action<Player.HurtInfo>>(s => s.PostHurt);

  private delegate bool DelegatePreKill(double damage, int hitDirection, bool pvp, ref bool playSound,
    ref bool genGore, ref PlayerDeathReason damageSource);

  public readonly StateHookList HookPreKill =
    AddHook<DelegatePreKill>(s => s.PreKill);

  public readonly StateHookList HookKill =
    AddHook<Action<double, int, bool, PlayerDeathReason>>(s => s.Kill);

  private delegate bool DelegatePreModifyLuck(ref float luck);

  public readonly StateHookList HookPreModifyLuck =
    AddHook<DelegatePreModifyLuck>(s => s.PreModifyLuck);

  private delegate void DelegateModifyLuck(ref float luck);

  public readonly StateHookList HookModifyLuck =
    AddHook<DelegateModifyLuck>(s => s.ModifyLuck);

  public readonly StateHookList HookPreItemCheck =
    AddHook<Func<bool>>(s => s.PreItemCheck);

  public readonly StateHookList HookPostItemCheck =
    AddHook<Action>(s => s.PostItemCheck);

  public readonly StateHookList HookUseTimeMultiplier =
    AddHook<Func<Item, float>>(s => s.UseTimeMultiplier);

  public readonly StateHookList HookUseAnimationMultiplier =
    AddHook<Func<Item, float>>(s => s.UseAnimationMultiplier);

  public readonly StateHookList HookUseSpeedMultiplier =
    AddHook<Func<Item, float>>(s => s.UseSpeedMultiplier);

  private delegate void DelegateGetHealLife(Item item, bool quickHeal, ref int healValue);

  public readonly StateHookList HookGetHealLife =
    AddHook<DelegateGetHealLife>(s => s.GetHealLife);

  private delegate void DelegateGetHealMana(Item item, bool quickHeal, ref int healValue);

  public readonly StateHookList HookGetHealMana =
    AddHook<DelegateGetHealMana>(s => s.GetHealMana);

  private delegate void DelegateModifyManaCost(Item item, ref float reduce, ref float mult);

  public readonly StateHookList HookModifyManaCost =
    AddHook<DelegateModifyManaCost>(s => s.ModifyManaCost);

  public readonly StateHookList HookOnMissingMana =
    AddHook<Action<Item, int>>(s => s.OnMissingMana);

  public readonly StateHookList HookOnConsumeMana =
    AddHook<Action<Item, int>>(s => s.OnConsumeMana);

  private delegate void DelegateModifyWeaponDamage(Item item, ref StatModifier damage);

  public readonly StateHookList HookModifyWeaponDamage =
    AddHook<DelegateModifyWeaponDamage>(s => s.ModifyWeaponDamage);

  public readonly StateHookList HookProcessTriggers =
    AddHook<Action<TriggersSet>>(s => s.ProcessTriggers);

  private delegate void DelegateModifyWeaponKnockback(Item item, ref StatModifier knockback);

  public readonly StateHookList HookModifyWeaponKnockback =
    AddHook<DelegateModifyWeaponKnockback>(s => s.ModifyWeaponKnockback);

  private delegate void DelegateModifyWeaponCrit(Item item, ref float crit);

  public readonly StateHookList HookModifyWeaponCrit =
    AddHook<DelegateModifyWeaponCrit>(s => s.ModifyWeaponCrit);

  public readonly StateHookList HookCanConsumeAmmo =
    AddHook<Func<Item, Item, bool>>(s => s.CanConsumeAmmo);

  public readonly StateHookList HookOnConsumeAmmo =
    AddHook<Action<Item, Item>>(s => s.OnConsumeAmmo);

  public readonly StateHookList HookCanShoot =
    AddHook<Func<Item, bool>>(s => s.CanShoot);

  private delegate void DelegateModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type,
    ref int damage, ref float knockback);

  public readonly StateHookList HookModifyShootStats =
    AddHook<DelegateModifyShootStats>(s => s.ModifyShootStats);

  public readonly StateHookList HookShoot =
    AddHook<Func<Item, EntitySource_ItemUse_WithAmmo, Vector2, Vector2, int, int, float, bool>>(s => s.Shoot);

  public readonly StateHookList HookMeleeEffects =
    AddHook<Action<Item, Rectangle>>(s => s.MeleeEffects);

  public readonly StateHookList HookEmitEnchantmentVisualsAt =
    AddHook<Action<Projectile, Vector2, int, int>>(s => s.EmitEnchantmentVisualsAt);

  public readonly StateHookList HookCanCatchNPC =
    AddHook<Func<NPC, Item, bool?>>(s => s.CanCatchNPC);

  public readonly StateHookList HookOnCatchNPC =
    AddHook<Action<NPC, Item, bool>>(s => s.OnCatchNPC);

  private delegate void DelegateModifyItemScale(Item item, ref float scale);

  public readonly StateHookList HookModifyItemScale =
    AddHook<DelegateModifyItemScale>(s => s.ModifyItemScale);

  public readonly StateHookList HookOnHitAnything =
    AddHook<Action<float, float, Entity>>(s => s.OnHitAnything);

  public readonly StateHookList HookCanHitNPC =
    AddHook<Func<NPC, bool>>(s => s.CanHitNPC);

  public readonly StateHookList HookCanMeleeAttackCollideWithNPC =
    AddHook<Func<Item, Rectangle, NPC, bool?>>(s => s.CanMeleeAttackCollideWithNPC);

  private delegate void DelegateModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers);

  public readonly StateHookList HookModifyHitNPC =
    AddHook<DelegateModifyHitNPC>(s => s.ModifyHitNPC);

  public readonly StateHookList HookOnHitNPC =
    AddHook<Action<NPC, NPC.HitInfo, int>>(s => s.OnHitNPC);

  public readonly StateHookList HookCanHitNPCWithItem =
    AddHook<Func<Item, NPC, bool?>>(s => s.CanHitNPCWithItem);

  private delegate void DelegateModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers);

  public readonly StateHookList HookModifyHitNPCWithItem =
    AddHook<DelegateModifyHitNPCWithItem>(s => s.ModifyHitNPCWithItem);

  public readonly StateHookList HookOnHitNPCWithItem =
    AddHook<Action<Item, NPC, NPC.HitInfo, int>>(s => s.OnHitNPCWithItem);

  public readonly StateHookList HookCanHitNPCWithProj =
    AddHook<Func<Projectile, NPC, bool?>>(s => s.CanHitNPCWithProj);

  private delegate void DelegateModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers);

  public readonly StateHookList HookModifyHitNPCWithProj =
    AddHook<DelegateModifyHitNPCWithProj>(s => s.ModifyHitNPCWithProj);

  public readonly StateHookList HookOnHitNPCWithProj =
    AddHook<Action<Projectile, NPC, NPC.HitInfo, int>>(s => s.OnHitNPCWithProj);

  public readonly StateHookList HookCanHitPvp =
    AddHook<Func<Item, Player, bool>>(s => s.CanHitPvp);

  public readonly StateHookList HookCanHitPvpWithProj =
    AddHook<Func<Projectile, Player, bool>>(s => s.CanHitPvpWithProj);

  private delegate bool DelegateCanBeHitByNPC(NPC npc, ref int cooldownSlot);

  public readonly StateHookList HookCanBeHitByNPC =
    AddHook<DelegateCanBeHitByNPC>(s => s.CanBeHitByNPC);

  private delegate void DelegateModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers);

  public readonly StateHookList HookModifyHitByNPC =
    AddHook<DelegateModifyHitByNPC>(s => s.ModifyHitByNPC);

  public readonly StateHookList HookOnHitByNPC =
    AddHook<Action<NPC, Player.HurtInfo>>(s => s.OnHitByNPC);

  public readonly StateHookList HookCanBeHitByProjectile =
    AddHook<Func<Projectile, bool>>(s => s.CanBeHitByProjectile);

  private delegate void DelegateModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers);

  public readonly StateHookList HookModifyHitByProjectile =
    AddHook<DelegateModifyHitByProjectile>(s => s.ModifyHitByProjectile);

  public readonly StateHookList HookOnHitByProjectile =
    AddHook<Action<Projectile, Player.HurtInfo>>(s => s.OnHitByProjectile);

  private delegate void DelegateModifyFishingAttempt(ref FishingAttempt attempt);

  public readonly StateHookList HookModifyFishingAttempt =
    AddHook<DelegateModifyFishingAttempt>(s => s.ModifyFishingAttempt);

  private delegate void DelegateCatchFish(FishingAttempt attempt, ref int itemDrop, ref int enemySpawn,
    ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition);

  public readonly StateHookList HookCatchFish =
    AddHook<DelegateCatchFish>(s => s.CatchFish);

  private delegate void DelegateModifyCaughtFish(Item fish);

  public readonly StateHookList HookModifyCaughtFish =
    AddHook<DelegateModifyCaughtFish>(s => s.ModifyCaughtFish);

  private delegate bool? DelegateCanConsumeBait(Item bait);

  public readonly StateHookList HookCanConsumeBait =
    AddHook<DelegateCanConsumeBait>(s => s.CanConsumeBait);

  private delegate void DelegateGetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel);

  public readonly StateHookList HookGetFishingLevel =
    AddHook<DelegateGetFishingLevel>(s => s.GetFishingLevel);

  public readonly StateHookList HookAnglerQuestReward =
    AddHook<Action<float, List<Item>>>(s => s.AnglerQuestReward);

  public readonly StateHookList HookGetDyeTraderReward =
    AddHook<Action<List<int>>>(s => s.GetDyeTraderReward);

  private delegate void DelegateDrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a,
    ref bool fullBright);

  public readonly StateHookList HookDrawEffects =
    AddHook<DelegateDrawEffects>(s => s.DrawEffects);

  private delegate void DelegateModifyDrawInfo(ref PlayerDrawSet drawInfo);

  public readonly StateHookList HookModifyDrawInfo =
    AddHook<DelegateModifyDrawInfo>(s => s.ModifyDrawInfo);

  public readonly StateHookList HookModifyDrawLayers =
    AddHook<Action<PlayerDrawSet>>(s => s.HideDrawLayers);

  public readonly StateHookList HookModifyScreenPosition =
    AddHook<Action>(s => s.ModifyScreenPosition);

  private delegate void DelegateModifyZoom(ref float zoom);

  public readonly StateHookList HookModifyZoom =
    AddHook<DelegateModifyZoom>(s => s.ModifyZoom);

  public readonly StateHookList HookPlayerConnect =
    AddHook<Action>(s => s.PlayerConnect);

  public readonly StateHookList HookPlayerDisconnect =
    AddHook<Action>(s => s.PlayerDisconnect);

  public readonly StateHookList HookOnEnterWorld =
    AddHook<Action>(s => s.OnEnterWorld);

  public readonly StateHookList HookOnRespawn =
    AddHook<Action>(s => s.OnRespawn);

  public readonly StateHookList HookShiftClickSlot =
    AddHook<Func<Item[], int, int, bool>>(s => s.ShiftClickSlot);

  public readonly StateHookList HookHoverSlot =
    AddHook<Func<Item[], int, int, bool>>(s => s.HoverSlot);

  public readonly StateHookList HookPostSellItem =
    AddHook<Action<NPC, Item[], Item>>(s => s.PostSellItem);

  public readonly StateHookList HookCanSellItem =
    AddHook<Func<NPC, Item[], Item, bool>>(s => s.CanSellItem);

  public readonly StateHookList HookPostBuyItem =
    AddHook<Action<NPC, Item[], Item>>(s => s.PostBuyItem);

  public readonly StateHookList HookCanBuyItem =
    AddHook<Func<NPC, Item[], Item, bool>>(s => s.CanBuyItem);

  public readonly StateHookList HookCanUseItem =
    AddHook<Func<Item, bool>>(s => s.CanUseItem);

  public readonly StateHookList HookCanAutoReuseItem =
    AddHook<Func<Item, bool?>>(s => s.CanAutoReuseItem);

  private delegate bool DelegateModifyNurseHeal(NPC npc, ref int health, ref bool removeDebuffs, ref string chatText);

  public readonly StateHookList HookModifyNurseHeal =
    AddHook<DelegateModifyNurseHeal>(s => s.ModifyNurseHeal);

  private delegate void DelegateModifyNursePrice(NPC npc, int health, bool removeDebuffs, ref int price);

  public readonly StateHookList HookModifyNursePrice =
    AddHook<DelegateModifyNursePrice>(s => s.ModifyNursePrice);

  public readonly StateHookList HookPostNurseHeal =
    AddHook<Action<NPC, int, bool, int>>(s => s.PostNurseHeal);

  public readonly StateHookList HookOnPickup =
    AddHook<Func<Item, bool>>(s => s.OnPickup);

  public readonly StateHookList HookArmorSetBonusActivated =
    AddHook<Action>(s => s.ArmorSetBonusActivated);

  public readonly StateHookList HookArmorSetBonusHeld =
    AddHook<Action<int>>(s => s.ArmorSetBonusHeld);
}

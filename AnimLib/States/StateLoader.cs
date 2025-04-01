using System.Linq.Expressions;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.Core;

namespace AnimLib.States;

using HookList = HookList<State>;

/// <summary>
/// Largely a copy of <see cref="PlayerLoader"/>, but for <see cref="State"/>s.
/// </summary>
[UsedImplicitly]
public sealed class StateLoader : ModSystem {
  public static StateLoader Instance => ModContent.GetInstance<StateLoader>();

  private static readonly List<HookList> HookLists = [];
  internal static readonly List<State> TemplateStates = [];
  internal static readonly List<AnimCharacter> SelectableCharacters = [];

  // Lazy load, OnModLoad is too early, PostSetupContent is too late, ResizeArrays overrides are internal to tML
  internal static StateHierarchy[] TemplateHierarchy { get; private set; } = null!;

  internal static void Add(State state) {
    state.Index = (ushort)TemplateStates.Count;
    TemplateStates.Add(state);
    if (state is AnimCharacter { Selectable: true } character) {
      SelectableCharacters.Add(character);
    }
  }

  private static HookList AddHook<T>(Expression<Func<State, T>> func) where T : Delegate {
    var hookList = HookList.Create(func);
    HookLists.Add(hookList);
    return hookList;
  }

  public override void PostSetupContent() {
    foreach (var hookList in HookLists) {
      hookList.Update(TemplateStates);
    }
  }

  public override void Unload() {
    HookLists.Clear();
    TemplateStates.Clear();
    TemplateHierarchy = null!;
  }

  internal static void NewInstance(AnimPlayer animPlayer) =>
    NewInstance(animPlayer, CollectionsMarshal.AsSpan(TemplateStates));

  private static void NewInstance(AnimPlayer animPlayer, ReadOnlySpan<State> templateStates) {
    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    // No good place to initialize besides lazy load
    // TODO: When ResizeArrays is open to modders, place this there
    TemplateHierarchy ??= StateHierarchy.ResizeArrays(TemplateStates);

    var states = new State[templateStates.Length];
    animPlayer.States = states;

    foreach (State templateState in templateStates) {
      State newState = templateState.NewInstance(animPlayer.Player);
      newState.AllStatesArray = states;
      newState.Hierarchy = templateState.Hierarchy;
      states[templateState.Index] = newState;
    }

    foreach (State state in states) {
      int parentId = state.Hierarchy.ParentId;
      if (parentId != -1) {
        state.Parent = states[parentId];
      }

      ushort[] parentIds = state.Hierarchy.ParentIds;
      for (int i = parentIds.Length - 1; i >= 0; i--) {
        // Assign State.Character
        if (state.GetState(parentIds[i]) is AnimCharacter character) {
          state.Character = character;
          break;
        }
      }
    }
  }

  public static readonly HookList HookInitialize =
    AddHook<Action>(s => s.Initialize);

  public static readonly HookList HookPostInitialize =
    AddHook<Action>(s => s.PostInitialize);

  public static readonly HookList HookResetEffects =
    AddHook<Action>(s => s.ResetEffects);

  public static readonly HookList HookResetInfoAccessories =
    AddHook<Action>(s => s.ResetInfoAccessories);

  public static readonly HookList HookRefreshInfoAccessoriesFromTeamPlayers =
    AddHook<Action<Player>>(s => s.RefreshInfoAccessoriesFromTeamPlayers);

  private delegate void DelegateModifyMaxStats(out StatModifier health, out StatModifier mana);

  public static readonly HookList HookModifyMaxStats =
    AddHook<DelegateModifyMaxStats>(s => s.ModifyMaxStats);

  public static readonly HookList HookUpdateDead =
    AddHook<Action>(s => s.UpdateDead);

  public static readonly HookList HookPreSavePlayer =
    AddHook<Action>(s => s.PreSavePlayer);

  public static readonly HookList HookPostSavePlayer =
    AddHook<Action>(s => s.PostSavePlayer);

  public static readonly HookList HookCopyClientState =
    AddHook<Action<ModPlayer>>(s => s.CopyClientState);

  public static readonly HookList HookSyncPlayer =
    AddHook<Action<int, int, bool>>(s => s.SyncPlayer);

  public static readonly HookList HookSendClientChanges =
    AddHook<Action<ModPlayer>>(s => s.SendClientChanges);

  public static readonly HookList HookUpdateBadLifeRegen =
    AddHook<Action>(s => s.UpdateBadLifeRegen);

  public static readonly HookList HookUpdateLifeRegen =
    AddHook<Action>(s => s.UpdateLifeRegen);

  private delegate void DelegateNaturalLifeRegen(ref float regen);

  public static readonly HookList HookNaturalLifeRegen =
    AddHook<DelegateNaturalLifeRegen>(s => s.NaturalLifeRegen);

  public static readonly HookList HookUpdateAutopause =
    AddHook<Action>(s => s.UpdateAutoPause);

  public static readonly HookList HookPreUpdate =
    AddHook<Action>(s => s.PreUpdate);

  public static readonly HookList HookSetControls =
    AddHook<Action>(s => s.SetControls);

  public static readonly HookList HookPreUpdateBuffs =
    AddHook<Action>(s => s.PreUpdateBuffs);

  public static readonly HookList HookPostUpdateBuffs =
    AddHook<Action>(s => s.PostUpdateBuffs);

  public static readonly HookList HookUpdateEquips =
    AddHook<Action>(s => s.UpdateEquips);

  public static readonly HookList HookPostUpdateEquips =
    AddHook<Action>(s => s.PostUpdateEquips);

  public static readonly HookList HookUpdateVisibleAccessories =
    AddHook<Action>(s => s.UpdateVisibleAccessories);

  public static readonly HookList HookUpdateVisibleVanityAccessories =
    AddHook<Action>(s => s.UpdateVisibleVanityAccessories);

  public static readonly HookList HookUpdateDyes =
    AddHook<Action>(s => s.UpdateDyes);

  public static readonly HookList HookPostUpdateMiscEffects =
    AddHook<Action>(s => s.PostUpdateMiscEffects);

  public static readonly HookList HookPostUpdateRunSpeeds =
    AddHook<Action>(s => s.PostUpdateRunSpeeds);

  public static readonly HookList HookPreUpdateMovement =
    AddHook<Action>(s => s.PreUpdateMovement);

  public static readonly HookList HookPostUpdate =
    AddHook<Action>(s => s.PostUpdate);

  private delegate void DelegateModifyExtraJumpDuration(ExtraJump jump, ref float duration);

  public static readonly HookList HookModifyExtraJumpDurationMultiplier =
    AddHook<DelegateModifyExtraJumpDuration>(s => s.ModifyExtraJumpDurationMultiplier);

  public static readonly HookList HookCanStartExtraJump =
    AddHook<Func<ExtraJump, bool>>(s => s.CanStartExtraJump);

  private delegate void DelegateExtraJumpStarted(ExtraJump jump, ref bool playSound);

  public static readonly HookList HookExtraJumpStarted =
    AddHook<DelegateExtraJumpStarted>(s => s.OnExtraJumpStarted);

  public static readonly HookList HookOnExtraJumpEnded =
    AddHook<Action<ExtraJump>>(s => s.OnExtraJumpEnded);

  public static readonly HookList HookOnExtraJumpRefreshed =
    AddHook<Action<ExtraJump>>(s => s.OnExtraJumpRefreshed);

  public static readonly HookList HookExtraJumpVisuals =
    AddHook<Action<ExtraJump>>(s => s.ExtraJumpVisuals);

  public static readonly HookList HookCanShowExtraJumpVisuals =
    AddHook<Func<ExtraJump, bool>>(s => s.CanShowExtraJumpVisuals);

  public static readonly HookList HookOnExtraJumpCleared =
    AddHook<Action<ExtraJump>>(s => s.OnExtraJumpCleared);

  public static readonly HookList HookFrameEffects =
    AddHook<Action>(s => s.FrameEffects);

  public static readonly HookList HookImmuneTo =
    AddHook<Func<PlayerDeathReason, int, bool, bool>>(s => s.ImmuneTo);

  public static readonly HookList HookFreeDodge =
    AddHook<Func<Player.HurtInfo, bool>>(s => s.FreeDodge);

  public static readonly HookList HookConsumableDodge =
    AddHook<Func<Player.HurtInfo, bool>>(s => s.ConsumableDodge);

  private delegate void DelegateModifyHurt(ref Player.HurtModifiers modifiers);

  public static readonly HookList HookModifyHurt =
    AddHook<DelegateModifyHurt>(s => s.ModifyHurt);

  public static readonly HookList HookOnHurt =
    AddHook<Action<Player.HurtInfo>>(s => s.OnHurt);

  public static readonly HookList HookPostHurt =
    AddHook<Action<Player.HurtInfo>>(s => s.PostHurt);

  private delegate bool DelegatePreKill(double damage, int hitDirection, bool pvp, ref bool playSound,
    ref bool genGore, ref PlayerDeathReason damageSource);

  public static readonly HookList HookPreKill =
    AddHook<DelegatePreKill>(s => s.PreKill);

  public static readonly HookList HookKill =
    AddHook<Action<double, int, bool, PlayerDeathReason>>(s => s.Kill);

  private delegate bool DelegatePreModifyLuck(ref float luck);

  public static readonly HookList HookPreModifyLuck =
    AddHook<DelegatePreModifyLuck>(s => s.PreModifyLuck);

  private delegate void DelegateModifyLuck(ref float luck);

  public static readonly HookList HookModifyLuck =
    AddHook<DelegateModifyLuck>(s => s.ModifyLuck);

  public static readonly HookList HookPreItemCheck =
    AddHook<Func<bool>>(s => s.PreItemCheck);

  public static readonly HookList HookPostItemCheck =
    AddHook<Action>(s => s.PostItemCheck);

  public static readonly HookList HookUseTimeMultiplier =
    AddHook<Func<Item, float>>(s => s.UseTimeMultiplier);

  public static readonly HookList HookUseAnimationMultiplier =
    AddHook<Func<Item, float>>(s => s.UseAnimationMultiplier);

  public static readonly HookList HookUseSpeedMultiplier =
    AddHook<Func<Item, float>>(s => s.UseSpeedMultiplier);

  private delegate void DelegateGetHealLife(Item item, bool quickHeal, ref int healValue);

  public static readonly HookList HookGetHealLife =
    AddHook<DelegateGetHealLife>(s => s.GetHealLife);

  private delegate void DelegateGetHealMana(Item item, bool quickHeal, ref int healValue);

  public static readonly HookList HookGetHealMana =
    AddHook<DelegateGetHealMana>(s => s.GetHealMana);

  private delegate void DelegateModifyManaCost(Item item, ref float reduce, ref float mult);

  public static readonly HookList HookModifyManaCost =
    AddHook<DelegateModifyManaCost>(s => s.ModifyManaCost);

  public static readonly HookList HookOnMissingMana =
    AddHook<Action<Item, int>>(s => s.OnMissingMana);

  public static readonly HookList HookOnConsumeMana =
    AddHook<Action<Item, int>>(s => s.OnConsumeMana);

  private delegate void DelegateModifyWeaponDamage(Item item, ref StatModifier damage);

  public static readonly HookList HookModifyWeaponDamage =
    AddHook<DelegateModifyWeaponDamage>(s => s.ModifyWeaponDamage);

  public static readonly HookList HookProcessTriggers =
    AddHook<Action<TriggersSet>>(s => s.ProcessTriggers);

  private delegate void DelegateModifyWeaponKnockback(Item item, ref StatModifier knockback);

  public static readonly HookList HookModifyWeaponKnockback =
    AddHook<DelegateModifyWeaponKnockback>(s => s.ModifyWeaponKnockback);

  private delegate void DelegateModifyWeaponCrit(Item item, ref float crit);

  public static readonly HookList HookModifyWeaponCrit =
    AddHook<DelegateModifyWeaponCrit>(s => s.ModifyWeaponCrit);

  public static readonly HookList HookCanConsumeAmmo =
    AddHook<Func<Item, Item, bool>>(s => s.CanConsumeAmmo);

  public static readonly HookList HookOnConsumeAmmo =
    AddHook<Action<Item, Item>>(s => s.OnConsumeAmmo);

  public static readonly HookList HookCanShoot =
    AddHook<Func<Item, bool>>(s => s.CanShoot);

  private delegate void DelegateModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type,
    ref int damage, ref float knockback);

  public static readonly HookList HookModifyShootStats =
    AddHook<DelegateModifyShootStats>(s => s.ModifyShootStats);

  public static readonly HookList HookShoot =
    AddHook<Func<Item, EntitySource_ItemUse_WithAmmo, Vector2, Vector2, int, int, float, bool>>(s => s.Shoot);

  public static readonly HookList HookMeleeEffects =
    AddHook<Action<Item, Rectangle>>(s => s.MeleeEffects);

  public static readonly HookList HookEmitEnchantmentVisualsAt =
    AddHook<Action<Projectile, Vector2, int, int>>(s => s.EmitEnchantmentVisualsAt);

  public static readonly HookList HookCanCatchNPC =
    AddHook<Func<NPC, Item, bool?>>(s => s.CanCatchNPC);

  public static readonly HookList HookOnCatchNPC =
    AddHook<Action<NPC, Item, bool>>(s => s.OnCatchNPC);

  private delegate void DelegateModifyItemScale(Item item, ref float scale);

  public static readonly HookList HookModifyItemScale =
    AddHook<DelegateModifyItemScale>(s => s.ModifyItemScale);

  public static readonly HookList HookOnHitAnything =
    AddHook<Action<float, float, Entity>>(s => s.OnHitAnything);

  public static readonly HookList HookCanHitNPC =
    AddHook<Func<NPC, bool>>(s => s.CanHitNPC);

  public static readonly HookList HookCanMeleeAttackCollideWithNPC =
    AddHook<Func<Item, Rectangle, NPC, bool?>>(s => s.CanMeleeAttackCollideWithNPC);

  private delegate void DelegateModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers);

  public static readonly HookList HookModifyHitNPC =
    AddHook<DelegateModifyHitNPC>(s => s.ModifyHitNPC);

  public static readonly HookList HookOnHitNPC =
    AddHook<Action<NPC, NPC.HitInfo, int>>(s => s.OnHitNPC);

  public static readonly HookList HookCanHitNPCWithItem =
    AddHook<Func<Item, NPC, bool?>>(s => s.CanHitNPCWithItem);

  private delegate void DelegateModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers);

  public static readonly HookList HookModifyHitNPCWithItem =
    AddHook<DelegateModifyHitNPCWithItem>(s => s.ModifyHitNPCWithItem);

  public static readonly HookList HookOnHitNPCWithItem =
    AddHook<Action<Item, NPC, NPC.HitInfo, int>>(s => s.OnHitNPCWithItem);

  public static readonly HookList HookCanHitNPCWithProj =
    AddHook<Func<Projectile, NPC, bool?>>(s => s.CanHitNPCWithProj);

  private delegate void DelegateModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers);

  public static readonly HookList HookModifyHitNPCWithProj =
    AddHook<DelegateModifyHitNPCWithProj>(s => s.ModifyHitNPCWithProj);

  public static readonly HookList HookOnHitNPCWithProj =
    AddHook<Action<Projectile, NPC, NPC.HitInfo, int>>(s => s.OnHitNPCWithProj);

  public static readonly HookList HookCanHitPvp =
    AddHook<Func<Item, Player, bool>>(s => s.CanHitPvp);

  public static readonly HookList HookCanHitPvpWithProj =
    AddHook<Func<Projectile, Player, bool>>(s => s.CanHitPvpWithProj);

  private delegate bool DelegateCanBeHitByNPC(NPC npc, ref int cooldownSlot);

  public static readonly HookList HookCanBeHitByNPC =
    AddHook<DelegateCanBeHitByNPC>(s => s.CanBeHitByNPC);

  private delegate void DelegateModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers);

  public static readonly HookList HookModifyHitByNPC =
    AddHook<DelegateModifyHitByNPC>(s => s.ModifyHitByNPC);

  public static readonly HookList HookOnHitByNPC =
    AddHook<Action<NPC, Player.HurtInfo>>(s => s.OnHitByNPC);

  public static readonly HookList HookCanBeHitByProjectile =
    AddHook<Func<Projectile, bool>>(s => s.CanBeHitByProjectile);

  private delegate void DelegateModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers);

  public static readonly HookList HookModifyHitByProjectile =
    AddHook<DelegateModifyHitByProjectile>(s => s.ModifyHitByProjectile);

  public static readonly HookList HookOnHitByProjectile =
    AddHook<Action<Projectile, Player.HurtInfo>>(s => s.OnHitByProjectile);

  private delegate void DelegateModifyFishingAttempt(ref FishingAttempt attempt);

  public static readonly HookList HookModifyFishingAttempt =
    AddHook<DelegateModifyFishingAttempt>(s => s.ModifyFishingAttempt);

  private delegate void DelegateCatchFish(FishingAttempt attempt, ref int itemDrop, ref int enemySpawn,
    ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition);

  public static readonly HookList HookCatchFish =
    AddHook<DelegateCatchFish>(s => s.CatchFish);

  private delegate void DelegateModifyCaughtFish(Item fish);

  public static readonly HookList HookModifyCaughtFish =
    AddHook<DelegateModifyCaughtFish>(s => s.ModifyCaughtFish);

  private delegate bool? DelegateCanConsumeBait(Item bait);

  public static readonly HookList HookCanConsumeBait =
    AddHook<DelegateCanConsumeBait>(s => s.CanConsumeBait);

  private delegate void DelegateGetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel);

  public static readonly HookList HookGetFishingLevel =
    AddHook<DelegateGetFishingLevel>(s => s.GetFishingLevel);

  public static readonly HookList HookAnglerQuestReward =
    AddHook<Action<float, List<Item>>>(s => s.AnglerQuestReward);

  public static readonly HookList HookGetDyeTraderReward =
    AddHook<Action<List<int>>>(s => s.GetDyeTraderReward);

  private delegate void DelegateDrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a,
    ref bool fullBright);

  public static readonly HookList HookDrawEffects =
    AddHook<DelegateDrawEffects>(s => s.DrawEffects);

  private delegate void DelegateModifyDrawInfo(ref PlayerDrawSet drawInfo);

  public static readonly HookList HookModifyDrawInfo =
    AddHook<DelegateModifyDrawInfo>(s => s.ModifyDrawInfo);

  public static readonly HookList HookModifyDrawLayers =
    AddHook<Action<PlayerDrawSet>>(s => s.HideDrawLayers);

  public static readonly HookList HookModifyScreenPosition =
    AddHook<Action>(s => s.ModifyScreenPosition);

  private delegate void DelegateModifyZoom(ref float zoom);

  public static readonly HookList HookModifyZoom =
    AddHook<DelegateModifyZoom>(s => s.ModifyZoom);

  public static readonly HookList HookPlayerConnect =
    AddHook<Action>(s => s.PlayerConnect);

  public static readonly HookList HookPlayerDisconnect =
    AddHook<Action>(s => s.PlayerDisconnect);

  public static readonly HookList HookOnEnterWorld =
    AddHook<Action>(s => s.OnEnterWorld);

  public static readonly HookList HookOnRespawn =
    AddHook<Action>(s => s.OnRespawn);

  public static readonly HookList HookShiftClickSlot =
    AddHook<Func<Item[], int, int, bool>>(s => s.ShiftClickSlot);

  public static readonly HookList HookHoverSlot =
    AddHook<Func<Item[], int, int, bool>>(s => s.HoverSlot);

  public static readonly HookList HookPostSellItem =
    AddHook<Action<NPC, Item[], Item>>(s => s.PostSellItem);

  public static readonly HookList HookCanSellItem =
    AddHook<Func<NPC, Item[], Item, bool>>(s => s.CanSellItem);

  public static readonly HookList HookPostBuyItem =
    AddHook<Action<NPC, Item[], Item>>(s => s.PostBuyItem);

  public static readonly HookList HookCanBuyItem =
    AddHook<Func<NPC, Item[], Item, bool>>(s => s.CanBuyItem);

  public static readonly HookList HookCanUseItem =
    AddHook<Func<Item, bool>>(s => s.CanUseItem);

  public static readonly HookList HookCanAutoReuseItem =
    AddHook<Func<Item, bool?>>(s => s.CanAutoReuseItem);

  private delegate bool DelegateModifyNurseHeal(NPC npc, ref int health, ref bool removeDebuffs, ref string chatText);

  public static readonly HookList HookModifyNurseHeal =
    AddHook<DelegateModifyNurseHeal>(s => s.ModifyNurseHeal);

  private delegate void DelegateModifyNursePrice(NPC npc, int health, bool removeDebuffs, ref int price);

  public static readonly HookList HookModifyNursePrice =
    AddHook<DelegateModifyNursePrice>(s => s.ModifyNursePrice);

  public static readonly HookList HookPostNurseHeal =
    AddHook<Action<NPC, int, bool, int>>(s => s.PostNurseHeal);

  public static readonly HookList HookOnPickup =
    AddHook<Func<Item, bool>>(s => s.OnPickup);

  public static readonly HookList HookArmorSetBonusActivated =
    AddHook<Action>(s => s.ArmorSetBonusActivated);

  public static readonly HookList HookArmorSetBonusHeld =
    AddHook<Action<int>>(s => s.ArmorSetBonusHeld);
}

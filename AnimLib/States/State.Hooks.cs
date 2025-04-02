using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.IO;

namespace AnimLib.States;

public abstract partial class State {
  /// <summary>
  /// This is always called, even when <see cref="Character"/> is not <see cref="Active"/>.
  /// <br/> This is also called on <see cref="AbilityState"/>
  /// regardless of if it is <see cref="AbilityState.Unlocked"/>.
  /// </summary>
  private static void _XmlDoc_CalledAlways() {
    // This is a placeholder method for reused XML documentation.
  }

  /// <summary>
  /// <b>NOTE:</b> This is called only when <see cref="Character"/> is <see cref="Active"/>.
  /// <br/> On <see cref="AbilityState"/> when <see cref="AbilityState.Unlocked"/> is <see langword="false"/>,
  /// this is never called.
  /// </summary>
  private static void _XmlDoc_CalledWhenCharacterActive() {
    // This is a placeholder method for reused XML documentation.
  }

  /// <summary>
  /// <b>NOTE:</b> This is called only when <b>this State</b> is <see cref="Active"/>.
  /// </summary>
  private static void _XmlDoc_CalledWhenSelfActive() {
    // This is a placeholder method for reused XML documentation.
  }

  // Inheritdoc for ModPlayer summary to append the inherited doc after notice of when the method is called
  // Inheritdoc for ModPlayer outside of summary to include additional info like params and remarks.

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.Initialize"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.Initialize"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void Initialize() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/> Called after <see cref="Initialize"/> has been called on all States on the <see cref="Player"/>.
  /// </summary>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void PostInitialize() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ResetEffects"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ResetEffects"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ResetEffects() {
  }

  // Not inheriting docs for remarks, as it incorrectly uses the seealso tag
  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ResetInfoAccessories"/>
  /// </summary>
  /// <remarks>
  /// This method is called in tandem with <see cref="ResetEffects"/>,
  /// but it also called in <see cref="Player.RefreshInfoAccs"/> even when the game is paused;
  /// this allows for info accessories to keep properly updating while the game is paused,
  /// a feature/fix added in 1.4.4.
  /// </remarks>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ResetInfoAccessories() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.RefreshInfoAccessoriesFromTeamPlayers"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.RefreshInfoAccessoriesFromTeamPlayers"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyMaxStats"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyMaxStats"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
    health = StatModifier.Default;
    mana = StatModifier.Default;
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.UpdateDead"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.UpdateDead"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void UpdateDead() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.SaveData"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.SaveData"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void SaveData(TagCompound tag) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.LoadData"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.LoadData"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void LoadData(TagCompound tag) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.PreSavePlayer"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PreSavePlayer"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void PreSavePlayer() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.PostSavePlayer"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PostSavePlayer"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void PostSavePlayer() {
  }

  // "<br/>" is used instead of "<para/>" because the inherited summary begins with a <br/> tag.
  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <br/><inheritdoc cref="ModPlayer.CopyClientState"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CopyClientState"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void CopyClientState(ModPlayer targetCopy) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.SyncPlayer"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.SyncPlayer"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.SendClientChanges"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.SendClientChanges"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void SendClientChanges(ModPlayer clientPlayer) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.UpdateBadLifeRegen"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.UpdateBadLifeRegen"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void UpdateBadLifeRegen() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.UpdateLifeRegen"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.UpdateLifeRegen"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void UpdateLifeRegen() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.NaturalLifeRegen"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.NaturalLifeRegen"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void NaturalLifeRegen(ref float regen) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.UpdateAutopause"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.UpdateAutopause"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void UpdateAutoPause() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.PreUpdate"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PreUpdate"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void PreUpdate() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.ProcessTriggers"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ProcessTriggers"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void ProcessTriggers(TriggersSet triggersSet) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.ArmorSetBonusActivated"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ArmorSetBonusActivated"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void ArmorSetBonusActivated() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.ArmorSetBonusHeld"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ArmorSetBonusHeld"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void ArmorSetBonusHeld(int holdTime) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenSelfActive"/>
  /// <para/><inheritdoc cref="ModPlayer.SetControls"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.SetControls"/>
  [HookCondition(HookConditionFlags.Strict)]
  public virtual void SetControls() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenSelfActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PreUpdateBuffs"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PreUpdateBuffs"/>
  [HookCondition(HookConditionFlags.Strict)]
  public virtual void PreUpdateBuffs() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenSelfActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PostUpdateBuffs"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PostUpdateBuffs"/>
  [HookCondition(HookConditionFlags.Strict)]
  public virtual void PostUpdateBuffs() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenSelfActive"/>
  /// <para/><inheritdoc cref="ModPlayer.UpdateEquips"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.UpdateEquips"/>
  [HookCondition(HookConditionFlags.Strict)]
  public virtual void UpdateEquips() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenSelfActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PostUpdateEquips"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PostUpdateEquips"/>
  [HookCondition(HookConditionFlags.Strict)]
  public virtual void PostUpdateEquips() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenSelfActive"/>
  /// <para/><inheritdoc cref="ModPlayer.UpdateVisibleAccessories"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.UpdateVisibleAccessories"/>
  [HookCondition(HookConditionFlags.Strict)]
  public virtual void UpdateVisibleAccessories() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenSelfActive"/>
  /// <para/><inheritdoc cref="ModPlayer.UpdateVisibleVanityAccessories"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.UpdateVisibleVanityAccessories"/>
  [HookCondition(HookConditionFlags.Strict)]
  public virtual void UpdateVisibleVanityAccessories() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenSelfActive"/>
  /// <para/><inheritdoc cref="ModPlayer.UpdateDyes"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.UpdateDyes"/>
  [HookCondition(HookConditionFlags.Strict)]
  public virtual void UpdateDyes() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenSelfActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PostUpdateMiscEffects"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PostUpdateMiscEffects"/>
  [HookCondition(HookConditionFlags.Strict)]
  public virtual void PostUpdateMiscEffects() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenSelfActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PostUpdateRunSpeeds"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PostUpdateRunSpeeds"/>
  [HookCondition(HookConditionFlags.Strict)]
  public virtual void PostUpdateRunSpeeds() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenSelfActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PreUpdateMovement"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PreUpdateMovement"/>
  [HookCondition(HookConditionFlags.Strict)]
  public virtual void PreUpdateMovement() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.PostUpdate"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PostUpdate"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void PostUpdate() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyExtraJumpDurationMultiplier"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyExtraJumpDurationMultiplier"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanStartExtraJump"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanStartExtraJump"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanStartExtraJump(ExtraJump jump) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnExtraJumpStarted"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnExtraJumpStarted"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnExtraJumpStarted(ExtraJump jump, ref bool playSound) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnExtraJumpEnded"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnExtraJumpEnded"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnExtraJumpEnded(ExtraJump jump) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnExtraJumpRefreshed"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnExtraJumpRefreshed"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnExtraJumpRefreshed(ExtraJump jump) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ExtraJumpVisuals"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ExtraJumpVisuals"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ExtraJumpVisuals(ExtraJump jump) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanShowExtraJumpVisuals"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanShowExtraJumpVisuals"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanShowExtraJumpVisuals(ExtraJump jump) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnExtraJumpCleared"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnExtraJumpCleared"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnExtraJumpCleared(ExtraJump jump) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.FrameEffects"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.FrameEffects"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void FrameEffects() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ImmuneTo"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ImmuneTo"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) => false;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.FreeDodge"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.FreeDodge"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool FreeDodge(Player.HurtInfo info) => false;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ConsumableDodge"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ConsumableDodge"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool ConsumableDodge(Player.HurtInfo info) => false;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyHurt"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyHurt"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyHurt(ref Player.HurtModifiers modifiers) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnHurt"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnHurt"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnHurt(Player.HurtInfo info) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PostHurt"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PostHurt"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void PostHurt(Player.HurtInfo info) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PreKill"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PreKill"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust,
    ref PlayerDeathReason damageSource) =>
    true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.Kill"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.Kill"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PreModifyLuck"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PreModifyLuck"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool PreModifyLuck(ref float luck) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyLuck"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyLuck"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyLuck(ref float luck) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PreItemCheck"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PreItemCheck"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool PreItemCheck() => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PostItemCheck"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PostItemCheck"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void PostItemCheck() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.UseTimeMultiplier"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.UseTimeMultiplier"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual float UseTimeMultiplier(Item item) => 1f;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.UseAnimationMultiplier"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.UseAnimationMultiplier"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual float UseAnimationMultiplier(Item item) => 1f;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.UseSpeedMultiplier"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.UseSpeedMultiplier"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual float UseSpeedMultiplier(Item item) => 1f;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.GetHealLife"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.GetHealLife"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void GetHealLife(Item item, bool quickHeal, ref int healValue) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.GetHealMana"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.GetHealMana"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void GetHealMana(Item item, bool quickHeal, ref int healValue) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyManaCost"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyManaCost"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyManaCost(Item item, ref float reduce, ref float mult) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnMissingMana"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnMissingMana"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnMissingMana(Item item, int neededMana) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnConsumeMana"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnConsumeMana"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnConsumeMana(Item item, int manaConsumed) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyWeaponDamage"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyWeaponDamage"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyWeaponDamage(Item item, ref StatModifier damage) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyWeaponKnockback"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyWeaponKnockback"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyWeaponKnockback(Item item, ref StatModifier knockback) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyWeaponCrit"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyWeaponCrit"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyWeaponCrit(Item item, ref float crit) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanConsumeAmmo"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanConsumeAmmo"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanConsumeAmmo(Item weapon, Item ammo) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnConsumeAmmo"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnConsumeAmmo"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnConsumeAmmo(Item weapon, Item ammo) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanShoot"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanShoot"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanShoot(Item item) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyShootStats"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyShootStats"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type,
    ref int damage, ref float knockback) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.Shoot"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.Shoot"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
    int type, int damage, float knockback) =>
    true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.MeleeEffects"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.MeleeEffects"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void MeleeEffects(Item item, Rectangle hitbox) {
  }

  // Inheritdoc from ModProjectile is intended
  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModProjectile.EmitEnchantmentVisualsAt"/>
  /// </summary>
  /// <inheritdoc cref="ModProjectile.EmitEnchantmentVisualsAt"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void
    EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanCatchNPC"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanCatchNPC"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool? CanCatchNPC(NPC target, Item item) => null;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnCatchNPC"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnCatchNPC"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnCatchNPC(NPC npc, Item item, bool failed) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyItemScale"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyItemScale"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyItemScale(Item item, ref float scale) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnHitAnything"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnHitAnything"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnHitAnything(float x, float y, Entity victim) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanHitNPC"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanHitNPC"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanHitNPC(NPC target) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanMeleeAttackCollideWithNPC"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanMeleeAttackCollideWithNPC"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool? CanMeleeAttackCollideWithNPC(Item item, Rectangle meleeAttackHitbox, NPC target) => null;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyHitNPC"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyHitNPC"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnHitNPC"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnHitNPC"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanHitNPCWithItem"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanHitNPCWithItem"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool? CanHitNPCWithItem(Item item, NPC target) => null;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyHitNPCWithItem"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyHitNPCWithItem"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnHitNPCWithItem"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnHitNPCWithItem"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanHitNPCWithProj"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanHitNPCWithProj"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool? CanHitNPCWithProj(Projectile proj, NPC target) => null;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyHitNPCWithProj"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyHitNPCWithProj"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnHitNPCWithProj"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnHitNPCWithProj"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanHitPvp"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanHitPvp"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanHitPvp(Item item, Player target) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanHitPvpWithProj"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanHitPvpWithProj"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanHitPvpWithProj(Projectile proj, Player target) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanBeHitByNPC"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanBeHitByNPC"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyHitByNPC"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyHitByNPC"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnHitByNPC"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnHitByNPC"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanBeHitByProjectile"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanBeHitByProjectile"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanBeHitByProjectile(Projectile proj) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyHitByProjectile"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyHitByProjectile"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnHitByProjectile"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnHitByProjectile"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyFishingAttempt"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyFishingAttempt"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyFishingAttempt(ref FishingAttempt attempt) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CatchFish"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CatchFish"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn,
    ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyCaughtFish"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyCaughtFish"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyCaughtFish(Item fish) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanConsumeBait"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanConsumeBait"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool? CanConsumeBait(Item bait) => null;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.GetFishingLevel"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.GetFishingLevel"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.AnglerQuestReward"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.AnglerQuestReward"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.GetDyeTraderReward"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.GetDyeTraderReward"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void GetDyeTraderReward(List<int> rewardPool) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.DrawEffects"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.DrawEffects"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a,
    ref bool fullBright) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyDrawInfo"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyDrawInfo"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyDrawLayerOrdering"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyDrawLayerOrdering"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.HideDrawLayers"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.HideDrawLayers"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void HideDrawLayers(PlayerDrawSet drawInfo) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyScreenPosition"/>
  /// </summary>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyScreenPosition() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyZoom"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyZoom"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyZoom(ref float zoom) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.PlayerConnect"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PlayerConnect"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void PlayerConnect() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.PlayerDisconnect"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PlayerDisconnect"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void PlayerDisconnect() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.OnEnterWorld"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnEnterWorld"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void OnEnterWorld() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledAlways"/>
  /// <para/><inheritdoc cref="ModPlayer.OnRespawn"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnRespawn"/>
  [HookCondition(HookConditionFlags.Always)]
  public virtual void OnRespawn() {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ShiftClickSlot"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ShiftClickSlot"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool ShiftClickSlot(Item[] inventory, int context, int slot) => false;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.HoverSlot"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.HoverSlot"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool HoverSlot(Item[] inventory, int context, int slot) => false;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PostSellItem"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PostSellItem"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void PostSellItem(NPC vendor, Item[] shopInventory, Item item) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanSellItem"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanSellItem"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanSellItem(NPC vendor, Item[] shopInventory, Item item) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PostBuyItem"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PostBuyItem"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanBuyItem"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanBuyItem"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanUseItem"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanUseItem"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool CanUseItem(Item item) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.CanAutoReuseItem"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.CanAutoReuseItem"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool? CanAutoReuseItem(Item item) => null;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyNurseHeal"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyNurseHeal"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText) => true;

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.ModifyNursePrice"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.ModifyNursePrice"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.PostNurseHeal"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.PostNurseHeal"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price) {
  }

  /// <summary>
  /// <inheritdoc cref="_XmlDoc_CalledWhenCharacterActive"/>
  /// <para/><inheritdoc cref="ModPlayer.OnPickup"/>
  /// </summary>
  /// <inheritdoc cref="ModPlayer.OnPickup"/>
  [HookCondition(HookConditionFlags.Default)]
  public virtual bool OnPickup(Item item) => true;
}

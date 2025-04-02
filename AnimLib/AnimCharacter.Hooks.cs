using AnimLib.States;

namespace AnimLib;

public abstract partial class AnimCharacter {
  /// <inheritdoc cref="ModPlayer.AddStartingItems"/>
  [HookCondition(HookConditionFlags.CharacterActive)]
  public virtual IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) => [];

  /// <inheritdoc cref="ModPlayer.ModifyStartingInventory"/>
  [HookCondition(HookConditionFlags.CharacterActive)]
  public virtual void
    ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath) {
  }

  /// <inheritdoc cref="ModPlayer.AddMaterialsForCrafting"/>
  [HookCondition(HookConditionFlags.CharacterActive)]
  public virtual IEnumerable<Item> AddMaterialsForCrafting(out ModPlayer.ItemConsumedCallback? itemConsumedCallback) {
    itemConsumedCallback = null;
    return [];
  }
}

using System;
using Terraria;
using Terraria.ModLoader;

namespace AnimLib.Compat
{
  public abstract class AnimCompatSystem : ModSystem
  {
    /// <summary>
    /// Set this flag to true, if system activation 
    /// succeed and predicates were registered
    /// </summary>
    protected bool _initialized = false;


    /// <summary>
    /// Set this flag to true, if system activation 
    /// succeed and predicates were registered
    /// </summary>
    protected bool _fault = false;

    /// <summary>
    /// Used for determining, if this compat system
    /// should be active for that player in current
    /// situation
    /// </summary>
    public virtual bool IsAllowed(Player player) =>
      _initialized && !_fault && !IsBlockListed(player);

    /// <summary>
    /// Checks that this compat system is not 
    /// blacklisted in player's character controllers
    /// </summary>
    public bool IsBlockListed(Player player) =>
      IsBlockListed(player.GetModPlayer<AnimPlayer>());

    /// <summary>
    /// Checks that this compat system is not 
    /// blacklisted in AnimPlayer's character controllers
    /// </summary>
    public bool IsBlockListed(AnimPlayer player)
    {
      var character = player.characters.ActiveCharacter;
      return character != null && (
        (character.abilityManager != null &&
          character.abilityManager.AnimCompatSystemBlocklist.Contains(Name)) ||
        (character.animationController != null &&
          character.animationController.AnimCompatSystemBlocklist.Contains(Name))
      );
    }

    /// <summary>
    /// Returns wrapped predicate
    /// for safe operation
    /// (prevents running when is not allowed and throwing exceptions outside)
    /// </summary>
    public Predicate<Player> GetStandardPredicate(Predicate<Player> predicate) => p =>
    {
      if (!IsAllowed(p)) return false;
      try
      {
        return predicate(p);
      }
      catch (Exception ex)
      {
        _fault = true;
        Log.LogError($"Something went wrong in {Name} compat module, it was disabled...", ex);
        return false;
      }
    };

    public override void Unload()
    {
      _initialized = false;
      _fault = false;
    }
  }
}

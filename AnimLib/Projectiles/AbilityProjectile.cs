using AnimLib.Abilities;
using JetBrains.Annotations;
using Terraria;
using Terraria.ModLoader;

namespace AnimLib.Projectiles {
  /// <summary>
  /// Base class for ability projectiles.
  /// </summary>
  [PublicAPI]
  public abstract class AbilityProjectile : ModProjectile {
    private Ability _ability;

    private AnimPlayer _aPlayer;

    /// <summary>
    /// Correlates to a <see cref="Ability.Id"/>.
    /// </summary>
    public abstract int Id { get; }

    /// <summary>
    /// The level of the <see cref="Ability"/> when this <see cref="AbilityProjectile"/> was created.
    /// </summary>
    public int level {
      get => (int)projectile.ai[0];
      set => projectile.ai[0] = value;
    }

    /// <summary>
    /// THe <see cref="AnimPlayer"/> that this <see cref="AbilityProjectile"/> belongs to.
    /// </summary>
    internal AnimPlayer aPlayer => _aPlayer ?? (_aPlayer = Main.player[projectile.owner].GetModPlayer<AnimPlayer>());

    /// <summary>
    /// The <see cref="Ability"/> that this <see cref="AbilityProjectile"/> belongs to.
    /// </summary>
    public Ability ability {
      get => _ability ?? (_ability = aPlayer.characters[mod].abilityManager?[Id]);
      internal set => _ability = value;
    }
  }
}

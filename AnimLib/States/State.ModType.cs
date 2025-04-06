using Terraria.ModLoader.Core;

namespace AnimLib.States;

// All members here pertain to ModType
public abstract partial class State : ModType<Player, State>, IIndexed {
  /// <summary>
  /// Index of this instance in <see cref="AllStates"/>
  /// </summary>
  public ushort Index { get; internal set; }

  /// <summary>
  /// The <see cref="Terraria.Player"/> which this instance belongs to.
  /// </summary>
  public Player Player => Entity;

  protected sealed override Player CreateTemplateEntity() => null!;

  public override State NewInstance(Player entity) {
    State? newState = base.NewInstance(entity);
    newState.Index = Index;
    return newState;
  }

  protected override void ValidateType() {
    base.ValidateType();

    LoaderUtils.MustOverrideTogether(this, s => s.SaveData, s => s.LoadData);
    LoaderUtils.MustOverrideTogether(this, s => s.CopyClientState, s => s.SendClientChanges);
  }

  protected sealed override void Register() {
    ModTypeLookup<State>.Register(this);
    if (this is AnimCharacter character) {
      ModTypeLookup<AnimCharacter>.Register(character);
      character.StyleUISettings = !Main.dedServ
        ? character.GetStyleUISettings() ?? AnimCharacterStyleUISettings.Default
        : AnimCharacterStyleUISettings.Default;
    }

    StateLoader.Add(this);
  }

  public sealed override void SetupContent() => SetStaticDefaults();
}

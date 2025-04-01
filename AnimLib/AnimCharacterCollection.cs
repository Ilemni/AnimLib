using System.Diagnostics;
using System.Linq;
using AnimLib.States;
using AnimLib.Systems;
using Terraria.ModLoader.IO;

namespace AnimLib;

[DebuggerDisplay("ActiveCharacter = {ActiveCharacter?.Name}, Player = {Player.name}")]
public sealed partial class AnimCharacterCollection : StateMachine {
  public AnimCharacter? ActiveCharacter => ActiveChild as AnimCharacter;

  public IEnumerable<AnimCharacter> Characters => Children.Cast<AnimCharacter>();

  private TimeSpan _lastAnimationUpdate;

  /// <summary>
  /// Stores the colors of the player before any <see cref="AnimCharacter"/> was enabled,
  /// and restores them when no <see cref="AnimCharacter"/> is active.
  /// </summary>
  private readonly AnimCharacterStyle _vanillaStyle = new();

  /// <inheritdoc cref="AnimCharacter.IsDrawingInUI"/>
  internal bool IsDrawingInUI;

  /// <inheritdoc cref="AnimCharacter.UIAnimated"/>
  internal bool UIAnimated;

  /// <inheritdoc cref="AnimCharacter.UIAnimationCounter"/>
  internal int UIAnimationCounter = -1;

  /// <inheritdoc cref="AnimCharacter.UICategoryIndex"/>
  internal int UICategoryIndex = -1;

  /// Used to detect category changes
  internal int UICategoryIndexLastFrame = -1;

  internal int UILastCategoryIndex = -1;

  internal int UICategoryCounterStart = 0;

  internal int UICategoryAnimationCounter => UIAnimationCounter - UICategoryCounterStart;

  protected override bool SetActiveChildOnEnter => false;

  public AnimCharacter GetCharacter(int index) => GetState(index) as AnimCharacter ??
    throw new ArgumentException("Specified index does not refer to an AnimCharacter", nameof(index));

  public override void Initialize() {
    _vanillaStyle.AssignFromPlayer(Player);
  }

  public override void PostInitialize() {
    SetActive(true);
  }

  public override void RegisterChildren(List<State> statesToAdd) {
    statesToAdd.AddRange(AllStatesArray.OfType<AnimCharacter>());
  }

  /// <summary>
  /// Enables the specified <see cref="AnimCharacter"/>.
  /// </summary>
  /// <param name="character"></param>
  internal void Enable(AnimCharacter character) {
    if (ReferenceEquals(character, ActiveCharacter)) {
      return;
    }

    Main.CancelClothesWindow(quiet: true);
    if (ActiveCharacter is null) {
      _vanillaStyle.AssignFromPlayer(Player);
    }

    if (!TrySetActiveChild(character)) {
      return;
    }

    character.Style.AssignToPlayer(Player);

    ModContent.GetInstance<DebugUISystem>().TrySetActiveCharacter(this);
    if (MrPlagueRacesModExists) {
      Disable_PlagueRace();
    }
  }

  /// <summary>
  /// Disable the given <see cref="AnimCharacter"/>.
  /// If <paramref name="character"/> was <see cref="ActiveCharacter"/>,
  /// <see cref="ActiveCharacter"/> will be replaced with the next character in the stack.
  /// </summary>
  /// <param name="character">The <see cref="AnimCharacter"/> to disable.</param>
  internal void Disable(AnimCharacter character) {
    Main.CancelClothesWindow(quiet: true);
    if (!ReferenceEquals(character, ActiveCharacter)) {
      return;
    }

    character.Style.AssignFromPlayer(Player);
    ClearActiveChild();
    _vanillaStyle.AssignToPlayer(Player);

    ModContent.GetInstance<DebugUISystem>().TrySetActiveCharacter(this);
    if (MrPlagueRacesModExists) {
      Enable_PlagueRace();
    }
  }

  public override void FrameEffects() {
    if (IsDrawingInUI) {
      return;
    }

    TimeSpan currentTime = Main.gameTimeCache.TotalGameTime;
    float delta = (float)(currentTime - _lastAnimationUpdate).TotalSeconds;
    _lastAnimationUpdate = currentTime;

    if (delta != 0) {
      ActiveCharacter?.UpdateAnimations(delta);
    }
  }

  public override void PreSavePlayer() {
    if (ActiveCharacter is not null) {
      ActiveCharacter.Style.AssignFromPlayer(Player);
      _vanillaStyle.AssignToPlayer(Player);
    }
    else {
      _vanillaStyle.AssignFromPlayer(Player);
    }
  }

  public override void PostSavePlayer() {
    // character.color -> player.color
    ActiveCharacter?.Style.AssignToPlayer(Player);
  }

  public override void PostUpdate() {
    if (MrPlagueRacesModExists) {
      DisableCharacterIfRaceActive();
    }
  }

  public override void LoadData(TagCompound tag) {
    if (!tag.TryGet("activeCharacter", out TagCompound characterTag)) {
      return;
    }

    string mod = characterTag.GetString("mod");
    string name = characterTag.GetString("name");
    if (ModContent.TryFind(mod, name, out AnimCharacter template)) {
      AnimCharacter character = GetState(template);
      Enable(character);
    }

    Load_PlagueRace(tag);
  }

  public override void SaveData(TagCompound tag) {
    if (ActiveCharacter is { } character) {
      tag["activeCharacter"] = new TagCompound {
        ["mod"] = character.Mod.Name,
        ["name"] = character.Name
      };
    }

    Save_PlagueRace(tag);
  }
}

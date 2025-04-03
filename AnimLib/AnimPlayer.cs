using System.Linq;
using AnimLib.Networking;
using AnimLib.States;
using AnimLib.Systems;
using JetBrains.Annotations;
using Terraria.ModLoader.IO;

namespace AnimLib;

/// <summary>
/// Main <see cref="ModPlayer"/> class for <see cref="AnimLibMod"/>, contains and updates <see cref="AnimLib.States.State"/>.
/// </summary>
[UsedImplicitly]
public sealed class AnimPlayer : ModPlayer {
  private const string StateDataKey = "stateData";
  private const string SkinDataKey = "skinData";

  internal State[] States = null!; // NewInstance() -> StateLoader.NewInstance

  public T GetState<T>() where T : State, new() => (T)GetState(ModContent.GetInstance<T>().Index);

  public T GetState<T>(int index) where T : State {
    State result = GetState(index);
    if (result is T t) {
      return t;
    }

    throw new ArgumentException("Specified index does not refer to a State that inherits type " + typeof(T).Name);
  }

  public State GetState(State templateState) => GetState(templateState.Index);

  public State GetState(int index) {
    ArgumentOutOfRangeException.ThrowIfNegative(index);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, States.Length);
    return States[index];
  }

  public override void OnEnterWorld() {
    ModContent.GetInstance<DebugUISystem>().SetCharacters(GetState<AnimCharacterCollection>());
  }

  public override ModPlayer NewInstance(Player entity) {
    AnimPlayer newInstance = (AnimPlayer)base.NewInstance(entity);
    StateLoader.NewInstance(newInstance); // Creates and populates States array
    return newInstance;
  }

  /// <inheritdoc/>
  public override void SendClientChanges(ModPlayer clientPlayer) {
    if (States.Any(s => s.NetUpdate)) {
      ModContent.GetInstance<ModNetHandler>().StatePacketHandler.SendPacket(255, Player.whoAmI);
    }
  }

  // ReSharper disable once RedundantOverriddenMember
  public override void CopyClientState(ModPlayer targetCopy) => base.CopyClientState(targetCopy);

  public override void SaveData(TagCompound tag) {
    if (StateIO.SaveStateData(Player) is { Count: > 0 } stateData) {
      tag[StateDataKey] = stateData;
    }

    if (StateIO.SaveSkinData(Player) is { Count: > 0 } skinData) {
      tag[SkinDataKey] = skinData;
    }
  }

  public override void LoadData(TagCompound tag) {
    if (tag.TryGet(StateDataKey, out IList<TagCompound> stateData)) {
      StateIO.LoadStateData(Player, stateData);
    }

    if (tag.TryGet(SkinDataKey, out IList<TagCompound> skinData)) {
      StateIO.LoadSkinData(Player, skinData);
    }
  }
}

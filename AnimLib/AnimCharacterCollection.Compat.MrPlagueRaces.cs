using MrPlagueRaces;
using MrPlagueRaces.Common.Races;
using MrPlagueRaces.Common.Races.Human;
using Terraria.ModLoader.IO;

namespace AnimLib;

// This compatibility partial exists to handle a player's Race being enabled or disabled.
// If a character becomes enabled, any active race becomes disabled.
// If a character becomes disabled, this will attempt to restore the previous race.
// If a race becomes enabled, any active character is disabled.
// The stored race will be saved and loaded.
public sealed partial class AnimCharacterCollection {
  private const string MrPlagueRacesModName = "MrPlagueRaces";
  private const string RaceSaveString = "compat_plagueRace";

  private static bool MrPlagueRacesModExists => ModLoader.HasMod(MrPlagueRacesModName);

  [JITWhenModsEnabled(MrPlagueRacesModName)]
  private string? GetCurrentRaceName() {
    Race race = Player.GetModPlayer<MrPlagueRacesPlayer>().race;
    return race is Human ? null : race?.FullName;
  }

  private string? _storedRace;


  [JITWhenModsEnabled(MrPlagueRacesModName)]
  private void Enable_PlagueRace() {
    if (_storedRace is null || !RaceLoader.TryGetRace(_storedRace, out Race result)) {
      return;
    }

    ref Race race = ref Player.GetModPlayer<MrPlagueRacesPlayer>().race;
    if (race is Human) {
      // Assign our stored race to override MPR's default
      race = result;
      return;
    }

    // Race is already something else, replace AnimLib's saved value
    _storedRace = race.FullName;
  }

  [JITWhenModsEnabled(MrPlagueRacesModName)]
  private void Disable_PlagueRace() {
    ref Race race = ref Player.GetModPlayer<MrPlagueRacesPlayer>().race;
    if (race is not null) {
      _storedRace = race.FullName;
    }

    // Change race to Human
    RaceLoader.TryGetRace("MrPlagueRaces/Human", out race);
  }


  [JITWhenModsEnabled(nameof(MrPlagueRaces))]
  private void DisableCharacterIfRaceActive() {
    if (ActiveTime == 1) {
      // Player just became active/entered world, disable any races
      Disable_PlagueRace();
      return;
    }

    Race race = Player.GetModPlayer<MrPlagueRacesPlayer>().race;
    if (race is not Human && ActiveCharacter is { } character) {
      Disable(character);
    }
  }


  private void Load_PlagueRace(TagCompound tag) {
    if (tag.TryGet(RaceSaveString, out _storedRace) && MrPlagueRacesModExists) {
      Enable_PlagueRace();
    }
  }

  private void Save_PlagueRace(TagCompound tag) {
    string? currentRace = MrPlagueRacesModExists ? GetCurrentRaceName() ?? _storedRace : _storedRace;
    if (currentRace is not null) {
      tag[RaceSaveString] = currentRace;
    }
  }
}

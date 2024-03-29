﻿using JetBrains.Annotations;

namespace AnimLib.Animations {
  /// <summary>
  /// Used to determine how a track behaves after its last frame is played.
  /// <para>This allows a track to loop, or only play once.</para>
  /// </summary>
  [PublicAPI]
  public enum LoopMode : byte {
    /// <summary>
    /// When the last frame ends, the animation remains on the last frame until the track changes.
    /// </summary>
    None = 0,

    /// <summary>
    /// When the last frame ends, the animation loops back to the start of the track.
    /// </summary>
    Always = 1
  }
}

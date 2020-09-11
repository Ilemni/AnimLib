namespace AnimLib.Animations {
  /// <summary>
  /// Value that represents how sprites are laid out on a spritesheet.
  /// </summary>
  [System.Flags]
  public enum SpriteRangeDirection {
    /// <summary>
    /// The sprites are laid out downward, where each frame is below the previous one.
    /// If the sprites use more than one column, the next colummn is to the right of the previous column.
    /// <para>Additional columns will start at <strong>the same Y position as the start frame.</strong>.</para>
    /// See also: <seealso cref="DownUncapped"/>
    /// </summary>
    Down = 1,
    /// <summary>
    /// The sprites are laid out to the right, where each frame is to the right the previous one.
    /// If the sprites use more than one row, the next row is below the previous row.
    /// <para>Additional rows will start at <strong>the same X position as the start frame.</strong>.</para>
    /// See also: <seealso cref="RightUncapped"/>
    /// </summary>
    Right = 2,
    /// <summary>
    /// The sprites are laid out downward, where each frame is below the previous one.
    /// If the sprites use more than one column, the next colummn is to the right of the previous column.
    /// <para>Additional columns will start at <strong>the left side of the spritesheet.</strong>.</para>
    /// /// See also: <seealso cref="Down"/>
    /// </summary>
    DownUncapped = 5,
    /// <summary>
    /// The sprites are laid out to the right, where each frame is to the right the previous one.
    /// If the sprites use more than one row, the next row is below the previous row.
    /// <para>Additional rows will start at <strong>the top of the spritesheet.</strong>.</para>
    /// /// See also: <seealso cref="Right"/>
    /// </summary>
    RightUncapped = 6,
  }
}

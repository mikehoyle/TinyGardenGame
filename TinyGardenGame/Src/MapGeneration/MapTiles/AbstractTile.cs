using System;
using static TinyGardenGame.MapGeneration.MapTiles.TileFlags;

namespace TinyGardenGame.MapGeneration.MapTiles {
  [Flags]
  public enum TileFlags: uint {
    None = 0,
    ContainsWater = 1 << 0,
    CanContainWater = 1 << 1,
    IsNonTraversable = 1 << 2,
  }
  
  /**
   * Base class for all Map Tiles. This class and most subclasses are
   * *very* memory-sensitive, as there could be thousands of instances
   * pulled into memory.
   */
  public abstract class AbstractTile {
    public byte TextureVariant = 0;
    public TileFlags Flags;

    protected AbstractTile() {
      Flags = TileFlags.None;
    }

    public bool Has(TileFlags flags) {
      return (Flags & flags) == flags;
    }
  }
}
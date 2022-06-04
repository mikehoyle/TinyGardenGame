
namespace TinyGardenGame.MapGeneration.MapTiles {
  /**
   * Base class for all Map Tiles. This class and most subclasses are
   * *very* memory-sensitive, as there could be thousands of instances
   * pulled into memory.
   */
  public abstract class AbstractTile {
    public short MapX;
    public short MapY;
    public byte TextureVariant = 0;
    public bool ContainsWater = false;

    protected AbstractTile(short x, short y) {
      MapX = x;
      MapY = y;
    }
  }
}
using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class OceanTile : MapTile {
    // TODO: create dedicated ocean texture
    public override SpriteName Sprite => SpriteName.Water4;

    public OceanTile() : base() {
      CanContainWater = false;
      IsNonTraversable = true;
    }
  }
}
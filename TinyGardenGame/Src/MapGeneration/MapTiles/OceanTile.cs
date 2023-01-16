using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class OceanTile : MapTile {
    // TODO: create dedicated ocean texture
    public override Vars.Sprite.Type Sprite => Vars.Sprite.Type.Water4;

    public OceanTile() : base() {
      CanContainWater = false;
      IsNonTraversable = true;
    }
  }
}
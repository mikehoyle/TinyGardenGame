using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class RocksTile : MapTile {
    public override SpriteName Sprite {
      get {
        switch (TextureVariant) {
          case 1:
          default:
            return SpriteName.Rocks1;
          case 2:
            return SpriteName.Rocks2;
          case 3:
            return SpriteName.Rocks3;
        }
      }
    }
    
    public RocksTile() : base() {
      CanContainWater = true;
    }
  }
}
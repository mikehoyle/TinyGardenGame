using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class FlowerPatchTile : MapTile {
    public override SpriteName Sprite {
      get {
        switch (TextureVariant) {
          case 1:
          default:
            return SpriteName.FlowerPatch1;
          case 2:
            return SpriteName.FlowerPatch2;
          case 3:
            return SpriteName.FlowerPatch3;
        }
      }
    }
    
    public FlowerPatchTile() : base() {
      SetRandomTextureVariant(3);
      CanContainWater = false;
    }
  }
}
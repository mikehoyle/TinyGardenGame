using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class SandTile : MapTile {
    public override SpriteName Sprite {
      get {
        switch (TextureVariant) {
          case 1:
          default:
            return SpriteName.Sand1;
          case 2:
            return SpriteName.Sand2;
          case 3:
            return SpriteName.Sand3;
        }
      }
    }

    public SandTile() : base() {
      SetRandomTextureVariant(3);
      CanContainWater = true;
    }
  }
}
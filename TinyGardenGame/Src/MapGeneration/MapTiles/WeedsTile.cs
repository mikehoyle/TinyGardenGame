using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class WeedsTile : MapTile {
    public override SpriteName Sprite {
      get {
        switch (TextureVariant) {
          case 1:
          default:
            return SpriteName.Weeds1;
          case 2:
            return SpriteName.Weeds2;
          case 3:
            return SpriteName.Weeds3;
        }
      }
    }

    public WeedsTile() : base() {
      SetRandomTextureVariant(3);
      CanContainWater = true;
    }
  }
}
using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class DryDirtTile : MapTile {
    public override SpriteName Sprite {
      get {
        switch (TextureVariant) {
          case 1:
          default:
            return SpriteName.DryDirt1;
          case 2:
            return SpriteName.DryDirt2;
          case 3:
            return SpriteName.DryDirt3;
        }
      }
    }

    public DryDirtTile() : base() {
      SetRandomTextureVariant(3);
      CanContainWater = false;
    }
  }
}
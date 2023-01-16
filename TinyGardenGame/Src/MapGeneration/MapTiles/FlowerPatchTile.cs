using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class FlowerPatchTile : MapTile {
    public override Vars.Sprite.Type Sprite {
      get {
        switch (TextureVariant) {
          case 1:
          default:
            return Vars.Sprite.Type.FlowerPatch1;
          case 2:
            return Vars.Sprite.Type.FlowerPatch2;
          case 3:
            return Vars.Sprite.Type.FlowerPatch3;
        }
      }
    }

    public FlowerPatchTile() : base() {
      SetRandomTextureVariant(3);
      CanContainWater = false;
    }
  }
}
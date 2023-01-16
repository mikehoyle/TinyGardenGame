using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class SandTile : MapTile {
    public override Vars.Sprite.Type Sprite {
      get {
        switch (TextureVariant) {
          case 1:
          default:
            return Vars.Sprite.Type.Sand1;
          case 2:
            return Vars.Sprite.Type.Sand2;
          case 3:
            return Vars.Sprite.Type.Sand3;
        }
      }
    }

    public SandTile() : base() {
      SetRandomTextureVariant(3);
      CanContainWater = true;
    }
  }
}
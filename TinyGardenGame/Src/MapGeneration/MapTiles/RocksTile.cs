using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class RocksTile : MapTile {
    public override Vars.Sprite.Type Sprite {
      get {
        switch (TextureVariant) {
          case 1:
          default:
            return Vars.Sprite.Type.Rocks1;
          case 2:
            return Vars.Sprite.Type.Rocks2;
          case 3:
            return Vars.Sprite.Type.Rocks3;
        }
      }
    }

    public RocksTile() : base() {
      SetRandomTextureVariant(3);
      CanContainWater = false;
    }
  }
}
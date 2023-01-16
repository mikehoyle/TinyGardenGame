using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class WeedsTile : MapTile {
    public override Vars.Sprite.Type Sprite {
      get {
        switch (TextureVariant) {
          case 1:
          default:
            return Vars.Sprite.Type.Weeds1;
          case 2:
            return Vars.Sprite.Type.Weeds2;
          case 3:
            return Vars.Sprite.Type.Weeds3;
        }
      }
    }

    public WeedsTile() : base() {
      SetRandomTextureVariant(3);
      CanContainWater = true;
    }
  }
}
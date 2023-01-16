using TinyGardenGame.Core;

namespace TinyGardenGame.MapGeneration.MapTiles {
  public class DryDirtTile : MapTile {
    public override Vars.Sprite.Type Sprite {
      get {
        switch (TextureVariant) {
          case 1:
          default:
            return Vars.Sprite.Type.DryDirt1;
          case 2:
            return Vars.Sprite.Type.DryDirt2;
          case 3:
            return Vars.Sprite.Type.DryDirt3;
        }
      }
    }

    public DryDirtTile() : base() {
      SetRandomTextureVariant(3);
      CanContainWater = false;
    }
  }
}
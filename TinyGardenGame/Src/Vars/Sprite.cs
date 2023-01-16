namespace TinyGardenGame.Vars; 

public partial class Sprite {
  public bool HasAtlasRect =>  AtlasRect.Height != 0 && AtlasRect.Width != 0;

  public partial struct Inner {
    public readonly partial struct Origin {
      public Vector2 ToVec() {
        return new Vector2(X, Y);
      }
    }
    
    public readonly partial struct AtlasRect {
      public Rectangle? ToRect() {
        if (Width == 0 || Height == 0) {
          return null;
        }

        return new Rectangle(X, Y, Width, Height);
      }
    }
  }
}
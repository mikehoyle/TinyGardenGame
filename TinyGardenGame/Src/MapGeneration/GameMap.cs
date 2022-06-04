#nullable enable
using System.Drawing;
using TinyGardenGame.MapGeneration.MapTiles;

namespace TinyGardenGame.MapGeneration {
  public class GameMap {
    // Oh my memory consumption...
    public AbstractTile?[,] Map { get; set; }
    public Point OriginTile { get; }

    public bool Contains(int x, int y) {
      var translatedX = x + OriginTile.X;
      var translatedY = y + OriginTile.Y;
      return translatedX >= 0 && translatedX < Map.GetLength(0)
          && translatedY >= 0 && translatedY < Map.GetLength(1)
          && Map[translatedX,translatedY] != null;
    }
    
    public AbstractTile? this[int x, int y] {
      get => Map[x + OriginTile.X, y + OriginTile.Y];
      set => Map[x + OriginTile.X, y + OriginTile.Y] = value;
    }

    public GameMap(int width, int height) {
      Map = new AbstractTile?[width, height];
      OriginTile = new Point(width / 2, height / 2);
    }
  }
}
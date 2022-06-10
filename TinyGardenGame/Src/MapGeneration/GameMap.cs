#nullable enable
using System;
using System.Runtime.Intrinsics.X86;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using TinyGardenGame.MapGeneration.MapTiles;

namespace TinyGardenGame.MapGeneration {
  public class GameMap {
    // Oh my memory consumption...
    public AbstractTile?[,] Map { get; set; }
    public Point OriginTile { get; }

    public RectangleF Bounds { get; }

    public bool Contains(int x, int y) {
      var translatedX = x + OriginTile.X;
      var translatedY = y + OriginTile.Y;
      return translatedX >= 0 && translatedX < Map.GetLength(0)
          && translatedY >= 0 && translatedY < Map.GetLength(1)
          && Map[translatedX,translatedY] != null;
    }

    public bool TryGet(int x, int y, out AbstractTile tile) {
      if (Contains(x, y)) {
        tile = this[x, y];
        return true;
      }

      tile = null;
      return false;
    }
    
    public AbstractTile? this[int x, int y] {
      get => Map[x + OriginTile.X, y + OriginTile.Y];
      set => Map[x + OriginTile.X, y + OriginTile.Y] = value;
    }

    public GameMap(int width, int height) {
      Map = new AbstractTile?[width, height];
      OriginTile = new Point(width / 2, height / 2);
      Bounds = new RectangleF(OriginTile.X * -1, OriginTile.Y * -1, width, height);
    }

    public void ForEach(Action<int /* x */, int /* y */, AbstractTile> action) {
      for (var x = 0; x < Map.GetLength(0); x++) {
        for (var y = 0; y < Map.GetLength(1); y++) {
          if (Map[x, y] != null) {
            action(x - OriginTile.X, y - OriginTile.Y, Map[x, y]);
          }
        }
      }
    }

  }
}
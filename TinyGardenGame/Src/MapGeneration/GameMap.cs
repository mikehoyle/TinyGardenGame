#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using TinyGardenGame.MapGeneration.MapTiles;
using static TinyGardenGame.MapPlacementHelper;

namespace TinyGardenGame.MapGeneration {
  public class GameMap {
    public List<(int X, int Y)> DirtyTiles { get; }
    
    // Oh my memory consumption...
    public MapTile?[,] Map { get; }
    public Point OriginTile { get; }

    public RectangleF Bounds { get; }

    public bool Contains(int x, int y) {
      var translatedX = x + OriginTile.X;
      var translatedY = y + OriginTile.Y;
      return translatedX >= 0 && translatedX < Map.GetLength(0)
          && translatedY >= 0 && translatedY < Map.GetLength(1)
          && Map[translatedX,translatedY] != null;
    }

    public bool TryGet(int x, int y, out MapTile tile) {
      if (Contains(x, y)) {
        tile = this[x, y];
        return true;
      }

      tile = null;
      return false;
    }
    
    public MapTile? this[int x, int y] {
      get => Map[x + OriginTile.X, y + OriginTile.Y];
      set => Map[x + OriginTile.X, y + OriginTile.Y] = value;
    }

    public GameMap(int width, int height) {
      DirtyTiles = new List<(int X, int Y)>();
      Map = new MapTile?[width, height];
      OriginTile = new Point(width / 2, height / 2);
      Bounds = new RectangleF(OriginTile.X * -1, OriginTile.Y * -1, width, height);
    }

    public void MarkTileDirty(int x, int y) {
      DirtyTiles.Add((x, y));
    }

    public void ForEach(Action<int, int, MapTile> action) {
      for (var x = 0; x < Map.GetLength(0); x++) {
        for (var y = 0; y < Map.GetLength(1); y++) {
          if (Map[x, y] != null) {
            action(x - OriginTile.X, y - OriginTile.Y, Map[x, y]);
          }
        }
      }
    }

    public void ForEachTileInBounds(
        RectangleF bounds, Action<int, int, MapTile> action) {
      // +Extra margin of error on N & W to accomodate for bounds cutting off mid-tile
      var leftBound = (int)MapPlacementHelper.AbsoluteCoordToMapCoord(bounds.TopLeft).X - 1;
      var topBound = (int)MapPlacementHelper.AbsoluteCoordToMapCoord(bounds.TopRight).Y - 1;
      var rightBound = (int)MapPlacementHelper.AbsoluteCoordToMapCoord(bounds.BottomRight).X;
      var bottomBound = (int)MapPlacementHelper.AbsoluteCoordToMapCoord(bounds.BottomLeft).Y;

      for (var x = leftBound; x <= rightBound; x++) {
        for (var y = topBound; y <= bottomBound; y++) {
          if (TryGet(x, y, out var tile)) {
            action(x, y, tile);
          }
        }
      }
    }

    public List<(int X, int Y, MapTile Tile)> GetIntersectingTiles(
        System.Drawing.RectangleF target) {
      var result = new List<(int, int, MapTile)>();
      for (var x = Convert.ToInt32(Math.Floor(target.X)); x < target.Right; x++) {
        for (var y = Convert.ToInt32(Math.Floor(target.Y)); y < target.Bottom; y++) {
          if (TryGet(x, y, out var tile)) {
            result.Add((x, y, tile));
          }
        }
      }

      return result;
    }

    /**
     * Executes for each adjacent tile, starting at the East tile and moving clockwise.
     * Aggregates the results in an array indexed by Direction.
     */
    public void ForEachAdjacentTile(
        int x, int y, Action<Direction, int /* x */, int /* y */, MapTile> action) {
      foreach (var direction in new[] {
                   Direction.North, Direction.East, Direction.South, Direction.West}) {
        var adjX = x + (int)DirectionUnitVectors[direction].X;
        var adjY = y + (int)DirectionUnitVectors[direction].Y;
        if (Contains(adjX, adjY)) { 
          action(direction, adjX, adjY, this[adjX, adjY]);
        }
      }
    }
  }
}
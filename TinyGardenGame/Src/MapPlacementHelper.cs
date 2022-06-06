using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using static TinyGardenGame.MapPlacementHelper.Direction;

namespace TinyGardenGame {
  /**
   * Helps convert to and from map coords to absolute/pixel coords
   *
   * Map cardinal directions are:
   *  W    N
   *   \ /
   *   / \
   *  S    E
   *
   * Where N/S is the Y-axis and E-W is the X-axis.
   *
   * Tile origins are on the NW corner of the tile.
   * Positive directions are East and South.
   */
  public static class MapPlacementHelper {
    public enum Direction : uint {
      East = 0,
      South = 1,
      West = 2,
      North = 3,
    }
    
    public static readonly Dictionary<Direction, Vector2> DirectionUnitVectors =
        new Dictionary<Direction, Vector2> {
            { East, Vector2.UnitX },
            { South, Vector2.UnitY },
            { West, Vector2.UnitX * -1 },
            { North, Vector2.UnitY * -1 },
        };
    
    // Maps directions to their angular bounds
    // These overlap intentionally, a preference towards a direction is better than gaps,
    // And realistically the player will often be travelling directly on these bounds (as they
    // are the cardinal directions) so we need to choose one way or the other.
    public static readonly Dictionary<Direction, (Angle, Angle)> DirectionBounds =
        new Dictionary<Direction, (Angle, Angle)> {
            {
                East,
                (new Angle(-0.125f, AngleType.Revolution), new Angle(0.125f, AngleType.Revolution))
            },
            {
                South,
                (new Angle(-0.375f, AngleType.Revolution), new Angle(-0.125f, AngleType.Revolution))
            },
            {
                West,
                (new Angle(0.375f, AngleType.Revolution), new Angle(-0.375f, AngleType.Revolution))
            },
            {
                North,
                (new Angle(0.125f, AngleType.Revolution), new Angle(0.375f, AngleType.Revolution))
            },
        };
    
    // Tile location at NW / top of tile, same for map coords & absolute rendering coords
    public static Vector2 MapOrigin { get; set; } = Vector2.Zero;

    public static int TileWidthPixels => 32;
    public static int TileHeightPixels => 16;
    
    // Commonly needed so provided for convenience;
    public static float HalfTileWidthPixels => TileWidthPixels / 2.0f;
    public static float HalfTileHeightPixels => TileHeightPixels / 2.0f;

    /**
     * Converts an isometric map coordinate (where 1 unit = 1 tile)
     * into an absolute, orthographic screen coordinate.
     */
    public static Vector2 MapCoordToAbsoluteCoord(Vector2 mapCoord) {
      return MapOrigin.Translate(
          (mapCoord.X - mapCoord.Y) * HalfTileWidthPixels,
          (mapCoord.X + mapCoord.Y) * HalfTileHeightPixels);
    }
    
    /**
     * The opposite of MapCoordToAbsoluteCoord.
     */
    public static Vector2 AbsoluteCoordToMapCoord(Vector2 absoluteCoord) {      
      return MapOrigin.Translate(
          ((absoluteCoord.X / HalfTileWidthPixels) + (absoluteCoord.Y / HalfTileHeightPixels))
              / 2.0f,
          ((absoluteCoord.Y / HalfTileHeightPixels) - (absoluteCoord.X / HalfTileWidthPixels))
              / 2.0f);
    }

    public static Vector2 CenterOfMapTile<T>(T x, T y) where T : IConvertible {
      return new Vector2(
          (float)Math.Floor(x.ToSingle(null)) + 0.5f,
          (float)Math.Floor(y.ToSingle(null)) + 0.5f);
    }
    
    public static Vector2 CenterOfMapTile(Vector2 coords) {
      return CenterOfMapTile(coords.X, coords.Y);
    }

    /**
     * Executes for each adjacent tile, starting at the East tile and moving clockwise.
     * Aggregates the results in an array indexed by Direction.
     */
    public static T[] ForEachAdjacentTile<T>(
        int x, int y, Func<Direction, int /* x */, int /* y */, T> action) {
      var result = new T[4];
      foreach (var direction in new[] { North, East, South, West }) {
        result[(uint)direction] = action(
            direction,
            x + (int)DirectionUnitVectors[direction].X,
            y + (int)DirectionUnitVectors[direction].Y);
      }

      return result;
    } 
  }
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.MapGeneration.MapTiles;
using static MonoGame.Extended.AngleType;
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
      SouthEast = 1,
      South = 2,
      SouthWest = 3,
      West = 4,
      NorthWest = 5,
      North = 6,
      NorthEast = 7,
    }
    
    public static readonly Dictionary<Direction, Vector2> DirectionUnitVectors =
        new Dictionary<Direction, Vector2> {
            { East, Vector2.UnitX },
            { SouthEast, Vector2.UnitX + Vector2.UnitY},
            { South, Vector2.UnitY },
            { SouthWest, -Vector2.UnitX + Vector2.UnitY},
            { West, -Vector2.UnitX },
            { NorthWest, -Vector2.UnitX - Vector2.UnitY},
            { North, -Vector2.UnitY },
            { NorthEast, Vector2.UnitX -Vector2.UnitY},
        };
    
    // Maps directions to their angular bounds
    // These overlap intentionally, a preference towards a direction is better than gaps,
    // And realistically the player will often be travelling directly on these bounds (as they
    // are the cardinal directions) so we need to choose one way or the other.
    public static readonly Dictionary<Direction, (Angle, Angle)> DirectionBounds =
        new Dictionary<Direction, (Angle, Angle)> {
            {
                East,
                (new Angle(-1/16f, Revolution), new Angle(1/16f, Revolution))
            },
            {
                SouthEast,
                (new Angle(-3/16f, Revolution), new Angle(-1/16f, Revolution))
            },
            {
                South,
                (new Angle(-5/16f, Revolution), new Angle(-3/16f, Revolution))
            },
            {
                SouthWest,
                (new Angle(-7/16f, Revolution), new Angle(-5/16f, Revolution))
            },
            {
                West,
                (new Angle(7/16f, Revolution), new Angle(-7/16f, Revolution))
            },
            {
                NorthWest,
                (new Angle(5/16f, Revolution), new Angle(7/16f, Revolution))
            },
            {
                North,
                (new Angle(3/16f, Revolution), new Angle(5/16f, Revolution))
            },
            {
                NorthEast,
                (new Angle(1/16f, Revolution), new Angle(3/16f, Revolution))
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
  }
}
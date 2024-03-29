﻿using System;
using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using static MonoGame.Extended.AngleType;

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

    public static readonly Dictionary<Direction, Vector2> DirectionUnitVectors = new() {
        { East, Vector2.UnitX },
        { SouthEast, Vector2.UnitX + Vector2.UnitY },
        { South, Vector2.UnitY },
        { SouthWest, -Vector2.UnitX + Vector2.UnitY },
        { West, -Vector2.UnitX },
        { NorthWest, -Vector2.UnitX - Vector2.UnitY },
        { North, -Vector2.UnitY },
        { NorthEast, Vector2.UnitX - Vector2.UnitY },
    };

    // Maps directions to their angular bounds
    // These overlap intentionally, a preference towards a direction is better than gaps,
    // And realistically the player will often be travelling directly on these bounds (as they
    // are the cardinal directions) so we need to choose one way or the other.
    public static readonly Dictionary<Direction, (Angle, Angle)> DirectionBounds = new() {
        {
            East,
            (new Angle(-1 / 16f, Revolution), new Angle(1 / 16f, Revolution))
        }, {
            SouthEast,
            (new Angle(-3 / 16f, Revolution), new Angle(-1 / 16f, Revolution))
        }, {
            South,
            (new Angle(-5 / 16f, Revolution), new Angle(-3 / 16f, Revolution))
        }, {
            SouthWest,
            (new Angle(-7 / 16f, Revolution), new Angle(-5 / 16f, Revolution))
        }, {
            West,
            (new Angle(7 / 16f, Revolution), new Angle(-7 / 16f, Revolution))
        }, {
            NorthWest,
            (new Angle(5 / 16f, Revolution), new Angle(7 / 16f, Revolution))
        }, {
            North,
            (new Angle(3 / 16f, Revolution), new Angle(5 / 16f, Revolution))
        }, {
            NorthEast,
            (new Angle(1 / 16f, Revolution), new Angle(3 / 16f, Revolution))
        },
    };

    // Favor left/right on the diagonals
    public static readonly Dictionary<Direction, (Angle, Angle)> FourDirectionBounds = new() {
        {
            East,
            (new Angle(-0.125f, Revolution), new Angle(0.125f, Revolution))
        }, {
            West,
            (new Angle(0.375f, Revolution), new Angle(-0.375f, Revolution))
        }, {
            South,
            (new Angle(-0.375f, Revolution), new Angle(-0.125f, Revolution))
        }, {
            North,
            (new Angle(0.125f, Revolution), new Angle(0.375f, Revolution))
        },
    };

    // Tile location at NW / top of tile, same for map coords & absolute rendering coords
    public static Vector2 MapOrigin { get; set; } = Vector2.Zero;

    public static int TileWidthPixels => 32;
    public static int TileHeightPixels => 16;

    // Commonly needed so provided for convenience;
    public static float HalfTileWidthPixels => TileWidthPixels / 2.0f;
    public static float HalfTileHeightPixels => TileHeightPixels / 2.0f;

    public static Direction AngleToDirection(Angle angle) {
      return DirectionBounds.FirstOrDefault(
          entry => Angle.IsBetween(angle, entry.Value.Item1, entry.Value.Item2)).Key;
    }

    public static Direction AngleToFourDirection(Angle angle) {
      return FourDirectionBounds.FirstOrDefault(
          entry => Angle.IsBetween(angle, entry.Value.Item1, entry.Value.Item2)).Key;
    }

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
     * Builds a rectangle in the given direction from origin zero. Only works in
     * the simple four directions, as Rectangles won't go diagonal.
     * Parameters left & right refer to from the unit's perspective.
     */
    public static Polygon BuildDirectedHitbox(
        float farBound, float nearBound, float leftBound, float rightBound, Direction direction) {
      // Initially right-facing, i.e. angle = 0rad
      var polygon = new Polygon(new List<Vector2> {
          new(nearBound, leftBound),
          new(farBound, leftBound),
          new(farBound, rightBound),
          new(nearBound, rightBound),
      });

      polygon.Rotate(DirectionUnitVectors[direction].ToAngle());
      return polygon;
    }
  }

  public static class RectangleExtensions {
    public static Rectangle ToXna(this System.Drawing.Rectangle rect) {
      return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static System.Drawing.Rectangle ToDrawing(this Rectangle rect) {
      return new System.Drawing.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }
    
    public static System.Drawing.RectangleF ToDrawing(this RectangleF rect) {
      return new System.Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static List<PointD> ToClipperPath(this SysRectangleF rect) {
      return new List<PointD> {
          new(rect.Left, rect.Top),
          new(rect.Right, rect.Top),
          new(rect.Right, rect.Bottom),
          new(rect.Left, rect.Bottom),
      };
    }
  }

  public static class PolygonExtensions {
    public static List<PointD> ToClipperPath(this Polygon polygon) {
      return polygon.Vertices.Select(
          point => new PointD(point.X, point.Y)).ToList();
    }
  }
}
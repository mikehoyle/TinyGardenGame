using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

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
   */
  public static class MapPlacementHelper {
    // Tile location at NW / top of tile, same for map coords & absolute rendering coords
    public static Vector2 MapOrigin { get; set; } = Vector2.Zero;

    public static int TileWidthPixels => 32;
    public static int TileHeightPixels => 16;

    public static Vector2 MapCoordToAbsoluteCoord(Vector2 mapCoord) {
      // Start at origin
      var absoluteCoord = MapOrigin;
      // Translate X
      absoluteCoord = absoluteCoord.Translate(
          mapCoord.X * (TileWidthPixels / 2.0f),
          mapCoord.X * (TileHeightPixels / 2.0f)
      );
      // Translate Y
      absoluteCoord = absoluteCoord.Translate(
          mapCoord.Y * (TileWidthPixels / 2.0f),
          mapCoord.Y * (TileHeightPixels / 2.0f) * -1
      );
      return absoluteCoord;
    }

    public static Vector2 CenterOfMapTile<T>(T x, T y) where T : IConvertible {
      return new Vector2(
          (float)Math.Floor(x.ToSingle(null)) + 0.5f,
          (float)Math.Floor(y.ToSingle(null)) - 0.5f);
    }
    
    public static Vector2 CenterOfMapTile(Vector2 coords) {
      return CenterOfMapTile(coords.X, coords.Y);
    }
  }
}
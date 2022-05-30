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
   */
  public static class MapPlacementHelper {
    // Tile location at top of tile
    public static Vector2 MapOriginTileAbsoluteLocation { get; set; } = Vector2.Zero;

    public static int TileWidthPixels => 32;
    public static int TileHeightPixels => 16;

    public static Vector2 MapCoordToAbsoluteCoord(Vector2 mapCoord) {
      // Start at origin
      var originTileCenter = MapOriginTileAbsoluteLocation.Translate(0, TileHeightPixels / 2.0f);
      // Translate X
      originTileCenter = originTileCenter.Translate(
          mapCoord.X * (TileWidthPixels / 2.0f),
          mapCoord.X * (TileHeightPixels / 2.0f)
      );
      // Translate Y
      originTileCenter = originTileCenter.Translate(
          mapCoord.Y * (TileWidthPixels / 2.0f),
          mapCoord.Y * (TileHeightPixels / 2.0f) * -1
      );
      return originTileCenter;
    }
    
  }
}
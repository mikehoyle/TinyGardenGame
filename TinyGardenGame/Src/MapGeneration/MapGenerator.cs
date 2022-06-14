using Microsoft.Xna.Framework;
using TinyGardenGame.MapGeneration.MapTiles;
using static TinyGardenGame.MapPlacementHelper;

namespace TinyGardenGame.MapGeneration {
  /**
   * This is dead simple for now, but stands as a place for any future changes.
   */
  public class MapGenerator {
    private const int MapWidth = 1000;
    private const int MapHeight = 1000;

    public GameMap GenerateMap() {
      var map = new GameMap(MapWidth, MapHeight);
      for (short i = 0; i < MapWidth; i++) {
        for (short j = 0; j < MapHeight; j++) {
          map.Map[i, j] = new WeedsTile();
        }
      }

      AddTestWater(map);
      PostProcess(map);
      return map;
    }

    private void AddTestWater(GameMap map) {
      for (var x = 5; x < 8; x++) {
        for (var y = -4; y < 1; y++) {
          map[x, y].Flags |= TileFlags.ContainsWater;
        }
      }

      map[4, -2].Flags |= TileFlags.ContainsWater;
      map[7, -5].Flags |= TileFlags.ContainsWater;
      map[7, -6].Flags |= TileFlags.ContainsWater;
    }

    /**
     * A second full pass over the generated map to handle interactions that rely on surrounding
     * terrain.
     */
    private void PostProcess(GameMap map) {
      map.ForEach((x, y, tile) => {
        var surroundedWaterTiles = 0;
        map.ForEachAdjacentTile(x, y, (direction, adjX, adjY, adjTile) => {
          if (adjTile.Has(TileFlags.ContainsWater)) {
            surroundedWaterTiles++;
          }
        });
        if (surroundedWaterTiles == 4) {
          tile.Flags |= TileFlags.IsNonTraversable;
        }
      });
    }
  }
}
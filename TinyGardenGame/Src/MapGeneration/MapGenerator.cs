using TinyGardenGame.MapGeneration.MapTiles;

namespace TinyGardenGame.MapGeneration {
  /**
   * This is dead simple for now, but stands as a place for any future changes.
   */
  public class MapGenerator {
    private const int MapWidth = 200;
    private const int MapHeight = 200;
    
    public MapGenerator() {}

    public GameMap GenerateMap() {
      var map = new GameMap(MapWidth, MapHeight);
      for (short i = 0; i < MapWidth; i++) {
        for (short j = 0; j < MapHeight; j++) {
          map.Map[i, j] = new WeedsTile(
              (short)(i - map.OriginTile.X), (short)(j - map.OriginTile.Y));
        }
      }

      AddTestWater(ref map);

      return map;
    }

    private void AddTestWater(ref GameMap map) {
      for (var x = 5; x < 8; x++) {
        for (var y = -4; y < 1; y++) {
          map[x, y].ContainsWater = true;
        }
      }

      map[4, -2].ContainsWater = true;
      map[7, -5].ContainsWater = true;
      map[7, -6].ContainsWater = true;
    }
  }
}
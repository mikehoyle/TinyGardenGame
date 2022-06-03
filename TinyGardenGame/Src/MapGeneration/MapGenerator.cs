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
      for (var i = 0; i < MapWidth; i++) {
        for (var j = 0; j < MapHeight; j++) {
          map.Map[i, j] = MapTileType.Weeds;
        }
      }

      return map;
    }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TinyGardenGame.MapGeneration.MapTiles;
using TinyGardenGame.MapGeneration.Noise;
using static TinyGardenGame.MapPlacementHelper;

namespace TinyGardenGame.MapGeneration {
  /**
   * This is dead simple for now, but stands as a place for any future changes.
   */
  public class MapGenerator {
    private const int MapWidth = 1000;
    private const int MapHeight = 1000;

    public GameMap GenerateMap(Config config) {
      // First, fill the entire map with Ocean
      var map = new GameMap(MapWidth, MapHeight);
      for (short i = 0; i < MapWidth; i++) {
        for (short j = 0; j < MapHeight; j++) {
          map.Map[i, j] = new OceanTile();
        }
      }
      
      // Fill in land with noise
      GenerateLandStructure(config, map);

      // TODO: generate biomes

      ConvertLandlockedOceanToWater(map);

      //AddTestWater(map);
      PostProcess(map);
      return map;
    }

    private static void ConvertLandlockedOceanToWater(GameMap map) {
      var knownOcean = new HashSet<(int x, int y)>();
      var marked = new HashSet<(int x, int y)>();
      map.ForEach((x, y, tile) => {
        if (IsTileInland(map, knownOcean, marked, x, y)) {
          // Marked set is filled. Mark as inland water.
          // For now, consider ever water tile to also be Weeds.
          foreach (var (waterX, waterY) in marked) {
            map[waterX, waterY] = new WeedsTile {
                ContainsWater = true,
            };
          }
        } else {
          knownOcean.UnionWith(marked);
        }
        marked.Clear();
      });
    }

    /**
     * Side effect: fills the marked list
     */
    private static bool IsTileInland(
        GameMap map,
        ISet<(int x, int y)> knownOcean,
        ISet<(int x, int y)> marked,
        int x,
        int y) {
      if (!map.TryGet(x, y, out var tile)) {
        // Touching edge of map is always ocean
        return false;
      }
      if (!(tile is OceanTile)) {
        return true;
      }
      if (knownOcean.Contains((x, y))) {
        return false;
      }
      
      if (!marked.Add((x, y))) {
        return true;
      }

      var result = true;
      foreach (var direction in new[] {
                   Direction.North, Direction.East, Direction.South, Direction.West}) {
        var adjX = x + (int)DirectionUnitVectors[direction].X;
        var adjY = y + (int)DirectionUnitVectors[direction].Y;
        result = result && IsTileInland(map, knownOcean, marked, adjX, adjY);
      }

      return result;
    }

    private static void GenerateLandStructure(Config config, GameMap map) {
      var border = config.OceanBorderWidth;
      var noiseMap = SimplexNoiseGenerator.GenerateNoiseMap(
          MapWidth - (border * 2), MapHeight - (border * 2), config.MapGenerationSeed);
      
      for (short i = 0; i < noiseMap.GetLength(0); i++) {
        for (short j = 0; j < noiseMap.GetLength(1); j++) {
          if (noiseMap[i, j] > SimplexNoiseGenerator.NoiseMax * 0.2) {
            map.Map[i + border, j + border] = new RocksTile();
          }
        }
      }
    }

    private void AddTestWater(GameMap map) {
      for (var x = 5; x < 8; x++) {
        for (var y = -4; y < 1; y++) {
          map[x, y].ContainsWater = true;
        }
      }

      map[4, -2].ContainsWater = true;
      map[7, -5].ContainsWater = true;
      map[7, -6].ContainsWater = true;
    }

    /**
     * A second full pass over the generated map to handle interactions that rely on surrounding
     * terrain.
     */
    private void PostProcess(GameMap map) {
      map.ForEach((x, y, tile) => {
        if (tile.ContainsWater) {
          MapProcessor.ProcessWaterProximity(map, x, y, tile);
          MapProcessor.MarkWaterNonTraversableIfSurrounded(map, x, y, tile);
        }
      });
    }
  }
}
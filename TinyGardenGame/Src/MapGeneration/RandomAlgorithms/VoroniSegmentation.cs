using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MonoGame.Extended;
using TinyGardenGame.Config;
using TinyGardenGame.MapGeneration.MapTiles;

namespace TinyGardenGame.MapGeneration.RandomAlgorithms {
  /**
   * Segments the map into equal-ish sized biomes using a Voroni diagram.
   */
  public static class VoroniSegmentation {
    private static readonly List<(Type Type, float Weight)> BiomeWeightedProbs =
        new List<(Type, float)> {
            (typeof(DryDirtTile), 10),
            (typeof(WeedsTile), 10),
            (typeof(RocksTile), 6),
            (typeof(SandTile), 4),
            (typeof(FlowerPatchTile), 1),
        };

    private static float _totalWeight;

    /**
     * Create a grid within which we randomly select a point, to create random but roughly equal
     * biomes.
     */
    public static List<BiomeSegment> GenerateBiomes(
        Random random, int mapWidth, int mapHeight) {
      var biomeSegments = new List<BiomeSegment>();
      var gridSize = GameConfig.Config.BiomeSize;
      for (var x = 0; x < mapWidth; x += gridSize) {
        for (var y = 0; y < mapHeight; y += gridSize) {
          // Note that it's okay for a segment to be outside of the map size, as it's only used for
          // proximity calculations for each tile.
          biomeSegments.Add(new BiomeSegment(
              WeightedBiomeChoice(random),
              random.Next(x, x + gridSize),
              random.Next(y, y + gridSize)));
        }
      }

      return biomeSegments;
    }

    private static Type WeightedBiomeChoice(Random random) {
      if (_totalWeight == 0) {
        _totalWeight = BiomeWeightedProbs.Sum(x => x.Weight);
      }

      var selection = random.NextSingle(0, _totalWeight);
      var currentWeight = 0f;
      foreach (var biome in BiomeWeightedProbs) {
        currentWeight += biome.Weight;
        if (selection < currentWeight) {
          return biome.Type;
        }
      }

      Debug.WriteLine("WARNING fallthrough case in weighted random selection, shouldn't occur");
      return BiomeWeightedProbs.Last().Type;
    }
  }

  public class BiomeSegment {
    public readonly Type TileType;
    public readonly int X;
    public readonly int Y;

    public BiomeSegment(Type tileType, int x, int y) {
      TileType = tileType;
      X = x;
      Y = y;
    }

    public float ProximityTo(int x, int y) {
      return Vector2.Distance(new Vector2(X, Y), new Vector2(x, y));
    }
  }
}
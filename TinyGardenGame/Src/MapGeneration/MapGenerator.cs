using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using TinyGardenGame.MapGeneration.MapTiles;
using TinyGardenGame.MapGeneration.RandomAlgorithms;

namespace TinyGardenGame.MapGeneration {
  /**
   * This is dead simple for now, but stands as a place for any future changes.
   */
  public class MapGenerator {
    private readonly Config.Config _config;
    private readonly Random _random;

    public MapGenerator(Config.Config config) {
      _config = config;
      _random = new Random(config.MapGenerationSeed);
    }

    public GameMap GenerateMap() {
      Stopwatch benchmarkTimer = new Stopwatch();
      benchmarkTimer.Start();
      Debug.WriteLine("Generating map...");

      // First, fill the entire map with Biomes
      var map = new GameMap(_config.MapWidth, _config.MapHeight);
      var biomeSegments = VoroniSegmentation.GenerateBiomes(
          _random, _config, _config.MapWidth, _config.MapHeight);
      // OPTIMIZE: This for loop is dreadfully slow and could probably be massively sped up
      // by Voroni algorithms like Fortune's Algorithm or Jump Flood.
      // TODO: Use Lloyd Relaxation with truer randomness for better variety?
      // TODO: Rivers along Delaunay Triangle edges?
      // http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/
      for (short x = 0; x < _config.MapWidth; x++) {
        for (short y = 0; y < _config.MapHeight; y++) {
          (Type Type, float prox) closestSegment = (typeof(object), float.MaxValue);
          foreach (var segment in biomeSegments) {
            var proximity = segment.ProximityTo(x, y);
            if (proximity < closestSegment.prox) {
              closestSegment = (segment.TileType, proximity);
            }
          }

          map.Map[x, y] = (MapTile)Activator.CreateInstance(closestSegment.Type);
        }
      }

      Debug.WriteLine($"Allocating base tiles took: {benchmarkTimer.Elapsed}");

      GenerateLakes(map, biomeSegments);
      Debug.WriteLine($"Generating lakes took: {benchmarkTimer.Elapsed}");

      // Fill in land with noise
      // GenerateLandStructure(map);
      //Debug.WriteLine($"Generate land with simplex noise took: {benchmarkTimer.Elapsed}");

      // Ensure reasonable starting area.
      // AddStartingArea(map);
      // Debug.WriteLine($"Add starting area took: {benchmarkTimer.Elapsed}");

      //ConvertLandlockedOceanToWater(map);
      //Debug.WriteLine($"Converting ocean to lakes took: {benchmarkTimer.Elapsed}");

      PostProcess(map);

      Debug.WriteLine($"Post-process took: {benchmarkTimer.Elapsed}");
      return map;
    }

    /**
     * Add lakes. Currently, just add one random lake per grass biome segment.
     */
    private void GenerateLakes(GameMap map, List<BiomeSegment> biomes) {
      foreach (var biome in biomes.Where(biome => biome.TileType == typeof(WeedsTile))) {
        GenerateLake(map, biome.X, biome.Y);
      }
    }

    /**
     * Generate a lake with the following strategy:
     *  - Centered in biome (on originX/originY)
     *  - Step around a circle of base radius R at random angles
     *  - Apply a point at that angle with a variability V around radius R
     *      the variability is scaled by the radius skip.
     */
    private void GenerateLake(GameMap map, int originX, int originY) {
      if (_random.NextSingle() > _config.LakeProbabilityPerBiomeInstance) {
        return;
      }

      var currentAngle = new Angle(0, AngleType.Radian);
      var baseRadius = _random.NextSingle(_config.LakeMinRadius, _config.LakeMaxRadius);
      var radiusLowerLimit = baseRadius - (baseRadius * _config.LakeVertexVariabilityPercent);
      var radiusUpperLimit = baseRadius + (baseRadius * _config.LakeVertexVariabilityPercent);
      var currentRadius = baseRadius;

      var polygonVertices = new List<Vector2>();

      do {
        polygonVertices.Add(
            new Vector2(originX, originY) + currentAngle.ToVector(currentRadius));


        // Increment angle around the circle
        var angleAddition = _random.NextSingle(
            _config.LakeMinAngleStepRadians, _config.LakeMaxAngleStepRadians);
        currentAngle.Radians += angleAddition;

        // Allow more variation the farther we're stepping
        var variationPercent =
            (angleAddition / _config.LakeMaxAngleStepRadians) *
            _config.LakeVertexVariabilityPercent;
        var currentLowerLimit =
            Math.Max(radiusLowerLimit, currentRadius - (baseRadius * variationPercent));
        var currentUpperLimit =
            Math.Min(radiusUpperLimit, currentRadius + (baseRadius * variationPercent));
        currentRadius = _random.NextSingle(currentLowerLimit, currentUpperLimit);
      } while (currentAngle.Revolutions < 1);

      var lakePolygon = new Polygon(polygonVertices);

      // Actually apply the polygon to the map
      var bounds = lakePolygon.BoundingRectangle;
      for (int x = Math.Max(0, (int)bounds.Left);
           x <= Math.Min(map.Map.GetLength(0) - 1, (int)bounds.Right);
           x++) {
        for (int y = Math.Max(0, (int)bounds.Top);
             y <= Math.Min(map.Map.GetLength(1) - 1, (int)bounds.Bottom);
             y++) {
          if (map.Map[x, y] != null
              && map.Map[x, y].CanContainWater
              && lakePolygon.Contains(x, y)) {
            map.Map[x, y].ContainsWater = true;
          }
        }
      }
    }

    private void AddStartingArea(GameMap map) {
      var radius = _config.StartingAreaRadius;
      for (var x = -radius; x <= radius; x++) {
        for (var y = -radius; y <= radius; y++) {
          // Add fuzziness on the edges
          if (Math.Abs(x) == radius - 1 || Math.Abs(y) == radius - 1) {
            if (_random.NextDouble() > 0.8) {
              continue;
            }
          }

          if (Math.Abs(x) == radius || Math.Abs(y) == radius) {
            if (_random.NextDouble() > 0.6) {
              continue;
            }
          }

          map[x, y] = new WeedsTile();
        }
      }
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
        }
        else {
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
                   North, East, South, West
               }) {
        var adjX = x + (int)DirectionUnitVectors[direction].X;
        var adjY = y + (int)DirectionUnitVectors[direction].Y;
        result = result && IsTileInland(map, knownOcean, marked, adjX, adjY);
      }

      return result;
    }

    private void GenerateLandStructure(GameMap map) {
      var border = _config.OceanBorderWidth;
      var noiseMap = SimplexNoiseGenerator.GenerateNoiseMap(
          _config.MapWidth - (border * 2),
          _config.MapHeight - (border * 2),
          _config.MapGenerationSeed);

      for (short i = 0; i < noiseMap.GetLength(0); i++) {
        for (short j = 0; j < noiseMap.GetLength(1); j++) {
          if (noiseMap[i, j] > SimplexNoiseGenerator.NoiseMax * 0.2) {
            map.Map[i + border, j + border] = new RocksTile();
          }
        }
      }
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
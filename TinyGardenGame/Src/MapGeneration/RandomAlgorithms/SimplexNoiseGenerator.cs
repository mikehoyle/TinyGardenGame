using SimplexNoise;

namespace TinyGardenGame.MapGeneration.RandomAlgorithms {
  public static class SimplexNoiseGenerator {
    public const float NoiseMax = 255f;

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed) {
      Noise.Seed = seed;
      return Noise.Calc2D(mapWidth, mapHeight, 0.07f);
    }
  }
}
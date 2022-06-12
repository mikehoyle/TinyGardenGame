
using Microsoft.Xna.Framework;

namespace TinyGardenGame.Plants {
  public interface IAttemptPlantPlacement {
    bool AttemptPlantPlacement(PlantType type, Vector2 location);
  }
}
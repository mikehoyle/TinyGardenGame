using Microsoft.Xna.Framework;

namespace TinyGardenGame.Core.Components {
  public class MotionComponent {
    public MotionComponent(float speedTilesPerSec) {
      SpeedTilesPerSec = speedTilesPerSec;
      CurrentMotion = Vector2.Zero;
    }
    
    public Vector2 CurrentMotion { get; set; }
    
    // TODO this is currently pixels/sec. Make the name true
    public float SpeedTilesPerSec { get; set; }
  }
}
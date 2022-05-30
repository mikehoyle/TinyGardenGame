using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace TinyGardenGame.Core.Components {
  public class MotionComponent {
    public MotionComponent(float speedTilesPerSec) {
      SpeedTilesPerSec = speedTilesPerSec;
      CurrentMotion = Vector2.Zero;
    }
    
    public Vector2 CurrentMotion { get; set; }
    
    // TODO this is currently pixels/sec. Make the name true
    public float SpeedTilesPerSec { get; set; }
    
    /**
     * Adjust motion in our isometric grid based on traditional
     * orthogonal vector (as from user input).
     */
    public void SetMotionFromCardinalVector(Vector2 vector) {
      CurrentMotion = vector.Equals(Vector2.Zero) ?
          vector : vector.Rotate(0.25f * (float)Math.PI);
    }
  }
}
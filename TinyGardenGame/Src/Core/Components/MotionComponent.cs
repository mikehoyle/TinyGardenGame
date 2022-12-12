using System;
using MonoGame.Extended;

namespace TinyGardenGame.Core.Components; 

public class MotionComponent {
  public MotionComponent(float speedTilesPerSec) {
    SpeedTilesPerSec = speedTilesPerSec;
    CurrentMotion = Vector2.Zero;
  }

  public Vector2 CurrentMotion { get; set; }

  public float SpeedTilesPerSec { get; set; }

  public Angle Angle => Angle.FromVector(CurrentMotion);

  /**
   * Adjust motion in our isometric grid based on traditional
   * orthogonal vector (as from user input).
   */
  public void SetMotionFromCardinalVector(Vector2 vector) {
    CurrentMotion = vector.Equals(Vector2.Zero) ? vector : vector.Rotate(-0.25f * (float)Math.PI);
  }

  public Vector2 GetMovementVector(GameTime gameTime, Vector2 movementDirection) {
    var normalizedSpeed = SpeedTilesPerSec * gameTime.GetElapsedSeconds();
    return movementDirection * normalizedSpeed;
  }

  public void SetMotionFromAngle(GameTime gameTime, Angle angle) {
    CurrentMotion = angle.ToVector(SpeedTilesPerSec * gameTime.GetElapsedSeconds());
  }

  // Move towards a target point
  public void MoveTowards(Vector2 position, Vector2 target) { }
}

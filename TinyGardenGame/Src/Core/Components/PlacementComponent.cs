using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace TinyGardenGame.Core.Components {
  /**
   * Where/how an entity is placed on the map 
   */
  public class PlacementComponent {
    private const float Pi = (float)Math.PI;
    
    // Position is based on isometric top-right = north
    public Vector2 Position { get; set; }
    
    // With 0 == right, 180deg == left
    public Angle Rotation { get; set; }

    public Vector2 AbsolutePosition => MapPlacementHelper.MapCoordToAbsoluteCoord(Position);
    
    // TODO I'm sure this logic needs revisiting
    public Point CurrentSquare => GetSquareForPositon(Position);

    public PlacementComponent(Vector2 position, Angle rotation = new Angle()) {
      Position = position;
      Rotation = rotation;
    }

    /**
     * Adjust isometric-position based on traditional orthogonal diff
     */
    public void AdjustPositionFromCardinalVector(Vector2 vector) {
      if (!vector.Equals(Vector2.Zero)) {
        vector = vector.Rotate(0.25f * Pi);
        Rotation = Angle.FromVector(vector);
        Position += vector;
      }
    }

    public static Point GetSquareForPositon(Vector2 position) {
      return new Point((int)Math.Round(position.X), (int)Math.Round(position.Y));
    }
  }
}
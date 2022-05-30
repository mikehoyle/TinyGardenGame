using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace TinyGardenGame.Core.Components {
  /**
   * Where/how an entity is placed on the map 
   */
  public class PlacementComponent {
    
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
    
    public void SetPositionFromMotionVector(Vector2 vector) {
      if (!vector.Equals(Vector2.Zero)) {
        Rotation = Angle.FromVector(vector);
        Position += vector;
      }
    }

    public static Point GetSquareForPositon(Vector2 position) {
      return new Point((int)Math.Round(position.X), (int)Math.Round(position.Y));
    }
  }
}
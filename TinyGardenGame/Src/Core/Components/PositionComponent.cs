using System;
using MonoGame.Extended;

namespace TinyGardenGame.Core.Components;

/**
 * Where/how an entity is placed on the map
 */
public class PositionComponent {
  public PositionComponent(
      Vector2 position,
      Angle rotation = new(),
      Vector2 footprintSize = new()) {
    Position = position;
    Rotation = rotation;
    FootprintSizeInTiles = footprintSize;
  }

  // Position is based on isometric top-right = north
  public Vector2 Position { get; set; }

  // With 0 == right, 180deg == left
  public Angle Rotation { get; set; }

  // Assumes NW origin, so notably won't work for movable sprites that
  // have origin at their feet.
  public Vector2 FootprintSizeInTiles { get; }

  public Vector2 AbsolutePosition => MapCoordToAbsoluteCoord(Position);
  public Point CurrentSquare => GetSquareForPosition(Position);
  public Vector2 EffectiveRenderDepth => Position + FootprintSizeInTiles;
  public Vector2 Center => Position + FootprintSizeInTiles / 2;

  public void SetPositionFromMotionVector(Vector2 vector) {
    if (!vector.Equals(Vector2.Zero)) {
      Rotation = Angle.FromVector(vector);
      Position += vector;
    }
  }

  public Angle AngleTo(PositionComponent targetPosition) {
    return Angle.FromVector(targetPosition.Center - Center);
  }

  public static Point GetSquareForPosition(Vector2 position) {
    return new Point((int)Math.Floor(position.X), (int)Math.Floor(position.Y));
  }
}

using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace TinyGardenGame.Components {
  public class PlacementComponent {
    // Placement is based on isometric top-right = north
    public Transform2 PlacementOnMap { get; set; }

    public Vector2 Position {
      get => PlacementOnMap.Position;
      set => PlacementOnMap.Position = value;
    }
    public float Rotation {
      get => PlacementOnMap.Rotation;
      set => PlacementOnMap.Rotation = value;
    }

    public Vector2 AbsolutePosition =>
        MapPlacementHelper.MapCoordToAbsoluteCoord(PlacementOnMap.Position);

    public PlacementComponent(Transform2 placementOnMap) {
      PlacementOnMap = placementOnMap;
    }

    /**
     * Adjust isometric-position based on traditional orthogonal diff
     */
    public void AdjustPositionFromCardinalVector(Vector2 vector) {
      Position += vector.Rotate(0.25f * (float)Math.PI);
    }
  }
}
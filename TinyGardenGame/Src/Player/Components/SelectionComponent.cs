using MonoGame.Extended;
using TinyGardenGame.Core.Components;

namespace TinyGardenGame.Player.Components {
  /**
   * Defines a selectable area.
   * TODO: Allow smart-selection based on what's nearby. 
   */
  public class SelectionComponent {
    public Vector2 SelectedSquare { get; private set; }

    // Current selection strategy is simply beneath the characters feet, biasing towards facing
    // direction if on multiple tiles
    public void SetFromMapPlacement(
        PositionComponent position, CollisionFootprintComponent footprint) {
      var facingDirection = AngleToDirection(position.Rotation);
      var facingUnitVector = DirectionUnitVectors[facingDirection];
      var facingPointInDistance =
          position.Position.Translate(facingUnitVector.X * 10, facingUnitVector.Y * 10);
      var footprintRect = new RectangleF(
          position.Position.X + footprint.Footprint.Left,
          position.Position.Y + footprint.Footprint.Top,
          footprint.Footprint.Width,
          footprint.Footprint.Height);
      var selectionPoint = footprintRect.ClosestPointTo(facingPointInDistance.ToPoint());
      
      SelectedSquare = PositionComponent.GetSquareForPosition(selectionPoint).ToVector2();
    }
  }
}
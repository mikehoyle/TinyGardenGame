using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using TinyGardenGame.Core.Components;
using static TinyGardenGame.MapPlacementHelper;

namespace TinyGardenGame.Player.Components {
  /**
   * Defines a selectable area. Currently only supports adjacent selection.
   * TODO: Allow much more intuitive diagonal selection as well, and ultimately smart-selection
   *   based on what's nearby. 
   */
  public class SelectionComponent {
    
    // Maps directions to their angular bounds
    // These overlap intentionally, a preference towards a direction is better than gaps,
    // And realistically the player will often be travelling directly on these bounds (as they
    // are the cardinal directions) so we need to choose one way or the other.
    private static readonly Dictionary<Direction, (Angle, Angle)> SelectionBounds =
        new Dictionary<Direction, (Angle, Angle)> {
            {
              Direction.East,
              (new Angle(-0.125f, AngleType.Revolution), new Angle(0.125f, AngleType.Revolution))
            },
            {
              Direction.South,
              (new Angle(-0.375f, AngleType.Revolution), new Angle(-0.125f, AngleType.Revolution))
            },
            {
              Direction.West,
              (new Angle(0.375f, AngleType.Revolution), new Angle(-0.375f, AngleType.Revolution))
            },
            {
              Direction.North,
              (new Angle(0.125f, AngleType.Revolution), new Angle(0.375f, AngleType.Revolution))
            },
        };

    public Vector2 SelectedSquare { get; set; }

    public void SetFromMapPlacement(
        PositionComponent position, CollisionFootprintComponent footprint) {
      var selectionDirection = SelectionBounds
          .FirstOrDefault(
              entry => Angle.IsBetween(position.Rotation, entry.Value.Item1, entry.Value.Item2))
          .Key;
      var vector = DirectionUnitVectors[selectionDirection];
      var targetSelection = position.CurrentSquare + vector.ToPoint();
      var footprintModifier =
          vector * new Vector2(footprint.Footprint.Width / 2, footprint.Footprint.Height / 2);
      if (PositionComponent.GetSquareForPosition(position.Position + footprintModifier)
          .Equals(targetSelection)) {
        // Selecting unit overlaps target square, select one further
        targetSelection += vector.ToPoint();
      }

      SelectedSquare = targetSelection.ToVector2();
    }
  }
}
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace TinyGardenGame.Components {
  /**
   * Defines a selectable area. Currently only supports adjacent selection.
   * TODO: Allow much more intuitive diagonal selection as well.
   */
  public class SelectionComponent {
    private enum SelectionDirection {
      Up = 0,
      Down = 1,
      Left = 2,
      Right = 3,
    }
    
    // Maps directions to their angular bounds
    // These overlap intentionally, a preference towards a direction is better than gaps,
    // And realistically the player will often be travelling directly on these bounds (as they
    // are the cardinal directions) so we need to choose one way or the other.
    private static readonly Dictionary<SelectionDirection, (Angle, Angle)> SelectionBounds =
        new Dictionary<SelectionDirection, (Angle, Angle)> {
            {
              SelectionDirection.Right,
              (new Angle(-0.125f, AngleType.Revolution), new Angle(0.125f, AngleType.Revolution))
            },
            {
              SelectionDirection.Up,
              (new Angle(-0.375f, AngleType.Revolution), new Angle(-0.125f, AngleType.Revolution))
            },
            {
              SelectionDirection.Left,
              (new Angle(0.375f, AngleType.Revolution), new Angle(-0.375f, AngleType.Revolution))
            },
            {
              SelectionDirection.Down,
              (new Angle(0.125f, AngleType.Revolution), new Angle(0.375f, AngleType.Revolution))
            },
        };

    private static readonly Dictionary<SelectionDirection, Vector2> SelectionVectors =
        new Dictionary<SelectionDirection, Vector2> {
            { SelectionDirection.Right, Vector2.UnitX },
            { SelectionDirection.Down, Vector2.UnitY * -1 },
            { SelectionDirection.Left, Vector2.UnitX * -1 },
            { SelectionDirection.Up, Vector2.UnitY },
        };

    public Vector2 SelectedSquare { get; set; }

    public void SetFromMapPlacement(
        PlacementComponent placement, CollisionFootprintComponent footprint) {
      var selectionDirection = SelectionBounds
          .FirstOrDefault(
              entry => Angle.IsBetween(placement.Rotation, entry.Value.Item1, entry.Value.Item2))
          .Key;
      var vector = SelectionVectors[selectionDirection];
      var targetSelection = placement.CurrentSquare + vector.ToPoint();
      var footprintModifier =
          vector * new Vector2(footprint.Footprint.Width / 2, footprint.Footprint.Height / 2);
      if (PlacementComponent.GetSquareForPositon(placement.Position + footprintModifier)
          .Equals(targetSelection)) {
        // Selecting unit overlaps target square, select one further
        targetSelection += vector.ToPoint();
      }

      SelectedSquare = targetSelection.ToVector2();
    }
  }
}
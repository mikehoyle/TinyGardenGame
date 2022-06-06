﻿using System.Linq;
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
    public Vector2 SelectedSquare { get; set; }

    public void SetFromMapPlacement(
        PositionComponent position, CollisionFootprintComponent footprint) {
      var selectionDirection = DirectionBounds
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
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Player.Components;

namespace TinyGardenGame.Player.State.FiniteStateMachine {
  public class MovablePlayerState : BasePlayerState {
    private readonly IIsSpaceOccupied _isSpaceOccupied;
    private readonly GameMap _map;

    // TODO: Base player input on configuration
    // TODO: Controller support
    private static readonly Dictionary<PlayerAction, Vector2> MovementDirections =
        new Dictionary<PlayerAction, Vector2>() {
            { PlayerAction.MoveDown, Vector2.UnitY },
            { PlayerAction.MoveUp, Vector2.UnitY * -1 },
            { PlayerAction.MoveRight, Vector2.UnitX },
            { PlayerAction.MoveLeft, Vector2.UnitX * -1 },
        };

    public MovablePlayerState(
        PlayerState playerState,
        IIsSpaceOccupied isSpaceOccupied,
        GameMap map) : base(playerState) {
      _isSpaceOccupied = isSpaceOccupied;
      _map = map;
    }

    public override Type? Update(
        GameTime gameTime,
        HashSet<PlayerAction> triggeredActions) {
      if (triggeredActions.Contains(PlayerAction.InventorySelectionLeft)) {
        PlayerState.Inventory.CurrentlySelectedSlot--;
      }

      if (triggeredActions.Contains(PlayerAction.InventorySelectionRight)) {
        PlayerState.Inventory.CurrentlySelectedSlot++;
      }

      if (triggeredActions.Contains(PlayerAction.Attack)) {
        return typeof(BasicAttackingPlayerState);
      }

      if (triggeredActions.Contains(PlayerAction.ToggleHoverPlant)
          && PlayerState.Inventory.CurrentlySelectedItem != null) {
        return typeof(PlaceableObjectHoveringState);
      }

      if (triggeredActions.Contains(PlayerAction.DigTrench)) {
        AttemptDigTrench();
      }

      MaybeMove(gameTime, triggeredActions);
      return null;
    }

    /** Returns true if movement input is detected */
    protected bool MaybeMove(GameTime gameTime, HashSet<PlayerAction> actions) {
      // Motion controls use input directly, while more concise button-press actions use
      // input listeners.
      var movementDirection = GetMovementDirection(actions);
      var motionComponent = PlayerState.PlayerEntity.Get<MotionComponent>();
      motionComponent.SetMotionFromCardinalVector(
          motionComponent.GetMovementVector(gameTime, movementDirection));

      var moved = movementDirection != Vector2.Zero;
      var animation = moved ? AnimationComponent.Action.Run : AnimationComponent.Action.Idle;

      var position = PlayerState.PlayerEntity.Get<PositionComponent>();
      var facingDirection = AngleToDirection(position.Rotation);
      PlayerState.PlayerEntity.Attach(
          new AnimationComponent(animation, facingDirection));
      
      return moved;
    }

    private static Vector2 GetMovementDirection(HashSet<PlayerAction> actions) {
      var movementDirection = MovementDirections.Keys.Where(actions.Contains)
          .Aggregate(Vector2.Zero, (current, key) => current + MovementDirections[key]);

      if (movementDirection != Vector2.Zero) {
        movementDirection.Normalize();
      }

      return movementDirection;
    }

    private void AttemptDigTrench() {
      var selection = PlayerState.PlayerEntity.Get<SelectionComponent>().SelectedSquare;
      var (x, y) = ((int)selection.X, (int)selection.Y);
      if (!CanDigTrench(x, y)) {
        return;
      }

      if (_map.TryGet(x, y, out var t)) {
        if (!t.ContainsWater && t.CanContainWater) {
          var hasAdjacentWater = false;
          _map.ForEachAdjacentTile(x, y, (_, adjX, adjY, adjTile) => {
            if (adjTile.ContainsWater) {
              hasAdjacentWater = true;
            }
          });

          if (hasAdjacentWater) {
            t.ContainsWater = true;
            _map.MarkTileDirty(x, y);
          }
        }
      }
    }

    private bool CanDigTrench(int x, int y) {
      return !_isSpaceOccupied.IsSpaceOccupied(new SysRectangleF(x, y, 1, 1));
    }
  }
}
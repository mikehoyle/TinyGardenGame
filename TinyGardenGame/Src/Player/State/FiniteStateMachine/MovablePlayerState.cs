#nullable enable
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input.InputListeners;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;

namespace TinyGardenGame.Player.State.FiniteStateMachine {
  public class MovablePlayerState : BasePlayerState {
    // TODO: Base player input on configuration
    // TODO: Controller support
    private static readonly Dictionary<PlayerAction, Vector2> MovementDirections =
        new Dictionary<PlayerAction, Vector2>() {
            {PlayerAction.MoveDown, Vector2.UnitY},
            {PlayerAction.MoveUp, Vector2.UnitY * -1},
            {PlayerAction.MoveRight, Vector2.UnitX},
            {PlayerAction.MoveLeft, Vector2.UnitX * -1},
        };

    public MovablePlayerState(PlayerState playerState) : base(playerState) {}
    
    public override BasePlayerState? Update(
        GameTime gameTime, 
        HashSet<PlayerAction> triggeredActions) {
      if (triggeredActions.Contains(PlayerAction.Attack)) {
        return new BasicAttackingPlayerState(PlayerState);
      }
      
      var animation = HandleMoveInput(gameTime, triggeredActions) ? "run" : "idle";

      var drawable = PlayerState.PlayerEntity.Get<DrawableComponent>();
      var position = PlayerState.PlayerEntity.Get<PositionComponent>();
      var facingDirection = MapPlacementHelper.AngleToDirection(position.Rotation);
      SetAnimationFromDirection(drawable, animation, facingDirection);
      return null;
    }
    
    /** Returns true if movement is being attempted */
    protected bool HandleMoveInput(GameTime gameTime, HashSet<PlayerAction> actions) {
      // Motion controls use input directly, while more concise button-press actions use
      // input listeners.
      var movementDirection = GetMovementDirection(actions);
      var motionComponent = PlayerState.PlayerEntity.Get<MotionComponent>();
      motionComponent.SetMotionFromCardinalVector(
          GetMovementVector(gameTime, movementDirection, motionComponent.SpeedTilesPerSec));
      return movementDirection != Vector2.Zero;
    }

    private static Vector2 GetMovementVector(
        GameTime gameTime, Vector2 movementDirection, float speed) {
      var normalizedSpeed = speed * gameTime.GetElapsedSeconds();
      return movementDirection * normalizedSpeed;
    }
    
    private static Vector2 GetMovementDirection(HashSet<PlayerAction> actions) {
      var movementDirection = MovementDirections.Keys.Where(actions.Contains)
          .Aggregate(Vector2.Zero, (current, key) => current + MovementDirections[key]);
      
      if (movementDirection != Vector2.Zero) {
        movementDirection.Normalize(); 
      }
    
      return movementDirection;
    }
  }
}
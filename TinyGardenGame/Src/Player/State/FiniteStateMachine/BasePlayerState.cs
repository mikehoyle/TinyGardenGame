#nullable enable
using System;
using System.Collections.Generic;
using TinyGardenGame.Core.Components;

namespace TinyGardenGame.Player.State.FiniteStateMachine {
  public abstract class BasePlayerState {
    protected PlayerState PlayerState { get; }

    protected BasePlayerState(PlayerState playerState) {
      PlayerState = playerState;
    }

    public virtual bool MeetsEntryCondition() {
      return true;
    }

    public virtual void Enter() { }

    // TODO consider separating this into HandleInputs first, so update gets run after any changes.
    public abstract Type? Update(GameTime gameTime, HashSet<PlayerAction> actions);

    public virtual void Exit() { }

    protected void SetAnimationFromDirection(
        DrawableComponent drawable,
        string animation,
        Direction direction,
        bool loop = true) {
      // Remember, the directions are for the tile grid, not the player's viewpoint
      drawable.SpriteEffects = SpriteEffects.None;
      switch (direction) {
        case SouthEast:
          drawable.SetAnimation($"{animation}_down", loop);
          break;
        case NorthWest:
          drawable.SetAnimation($"{animation}_up", loop);
          break;
        case South:
        case SouthWest:
        case West:
          drawable.SetAnimation($"{animation}_left", loop);
          break;
        case North:
        case NorthEast:
        case East:
          drawable.SpriteEffects = SpriteEffects.FlipHorizontally;
          drawable.SetAnimation($"{animation}_left", loop);
          break;
      }
    }
  }
}
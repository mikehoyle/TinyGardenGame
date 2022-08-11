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
  }
}
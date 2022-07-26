#nullable enable
using System;
using System.Collections.Generic;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.MapGeneration;

namespace TinyGardenGame.Player.State.FiniteStateMachine {
  public class BasicAttackingPlayerState : MovablePlayerState {
    // TODO Define these in a weapon object or something when the time comes,
    //    anywhere but here.
    private const float AttackOuterRange = 1f;
    private const float AttackInnerRange = 0.5f;
    private const float AttackWidth = 1f;
    private static readonly TimeSpan AttackWindup = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan FullDuration = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan CancellableAfterDuration = TimeSpan.FromMilliseconds(300);
    private static double DamageDealt = 5;

    private TimeSpan _currentDuration;

    public BasicAttackingPlayerState(
        PlayerState playerState,
        IIsSpaceOccupied isSpaceOccupied,
        GameMap map) : base(playerState, isSpaceOccupied, map) { }

    public override void Enter() {
      _currentDuration = TimeSpan.Zero;
      var motionComponent = PlayerState.PlayerEntity.Get<MotionComponent>();
      var drawable = PlayerState.PlayerEntity.Get<DrawableComponent>();
      var position = PlayerState.PlayerEntity.Get<PositionComponent>();

      motionComponent.SetMotionFromCardinalVector(Vector2.Zero);
      var facingDirection = AngleToDirection(position.Rotation);
      PlayerState.PlayerEntity.Attach(BuildDamageSource(facingDirection));
      SetAnimationFromDirection(drawable, "attack", facingDirection, false);
    }

    public override Type? Update(GameTime gameTime, HashSet<PlayerAction> actions) {
      _currentDuration += gameTime.ElapsedGameTime;
      if (_currentDuration >= CancellableAfterDuration && MaybeMove(gameTime, actions)) {
        return typeof(MovablePlayerState);
      }

      return _currentDuration >= FullDuration ? typeof(MovablePlayerState) : null;
    }

    private DamageSourceComponent BuildDamageSource(Direction facingDirection) {
      return new DamageSourceComponent(
          BuildDirectedRect(
              AttackOuterRange,
              AttackInnerRange,
              -AttackWidth / 2,
              AttackWidth / 2, facingDirection),
          DamageDealt,
          AttackWindup);
    }
  }
}
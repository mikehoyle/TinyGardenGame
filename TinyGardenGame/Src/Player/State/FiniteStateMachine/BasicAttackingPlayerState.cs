#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using TinyGardenGame.Core.Components;
using static TinyGardenGame.MapPlacementHelper;

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
    
    private TimeSpan _currentDuration = TimeSpan.Zero;

    public BasicAttackingPlayerState(PlayerState playerState) : base(playerState) {
      var motionComponent = PlayerState.PlayerEntity.Get<MotionComponent>();
      motionComponent.SetMotionFromCardinalVector(Vector2.Zero);
      
      var position = PlayerState.PlayerEntity.Get<PositionComponent>();
      var facingDirection = AngleToDirection(position.Rotation);
      playerState.PlayerEntity.Attach(BuildDamageSource(facingDirection));
      var drawable = PlayerState.PlayerEntity.Get<DrawableComponent>();
      SetAnimationFromDirection(drawable, "attack", facingDirection, false);
    }
    
    public override BasePlayerState? Update(GameTime gameTime, HashSet<PlayerAction> actions) {
      _currentDuration += gameTime.ElapsedGameTime;
      if (_currentDuration >= CancellableAfterDuration && HandleMoveInput(gameTime, actions)) {
        return new MovablePlayerState(PlayerState);
      }
      
      return _currentDuration >= FullDuration ? new MovablePlayerState(PlayerState) : null;
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
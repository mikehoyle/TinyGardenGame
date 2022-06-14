﻿using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Player.Components;
using static TinyGardenGame.MapPlacementHelper.Direction;

namespace TinyGardenGame.Core.Systems {
  /**
   * Executes motion on entities, moving them on the map.
   *
   * The final step in the pipeline:
   * 1. PlayerInputSystem (plan motion)
   * 2. CollisionSystem (modify motion)
   * 3. MotionSystem (execute motion)
   */
  public class MotionSystem : EntityUpdateSystem {
    private ComponentMapper<PositionComponent> _positionComponentMapper;
    private ComponentMapper<MotionComponent> _motionComponentMapper;
    private ComponentMapper<SelectionComponent> _selectionComponentMapper;
    private ComponentMapper<CollisionFootprintComponent> _collisionComponentMapper;
    private ComponentMapper<DrawableComponent> _drawableComponentMapper;

    public MotionSystem() :
        base(Aspect.All(typeof(PositionComponent), typeof(MotionComponent))) { }

    public override void Initialize(IComponentMapperService mapperService) {
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
      _motionComponentMapper = mapperService.GetMapper<MotionComponent>();
      _selectionComponentMapper = mapperService.GetMapper<SelectionComponent>();
      _collisionComponentMapper = mapperService.GetMapper<CollisionFootprintComponent>();
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
    }

    public override void Update(GameTime gameTime) {
      foreach (var entity in ActiveEntities) {
        var currentMotion = _motionComponentMapper.Get(entity).CurrentMotion;
        var positionComponent = _positionComponentMapper.Get(entity);
        positionComponent.SetPositionFromMotionVector(currentMotion);
        UpdateAnimation(entity, currentMotion);
        // OPTIMIZE: These has/gets could be inefficient and warrant another system if there 
        // end up being many moving elements.
        if (_selectionComponentMapper.Has(entity) && _collisionComponentMapper.Has(entity)) {
          _selectionComponentMapper.Get(entity)
              .SetFromMapPlacement(positionComponent, _collisionComponentMapper.Get(entity));
        }
      }
    }

    private void UpdateAnimation(int entityId, Vector2 currentMotion) {
      if (_drawableComponentMapper.Has(entityId)) {
        var drawable = _drawableComponentMapper.Get(entityId);
        drawable.SpriteEffects = SpriteEffects.None;
        if (currentMotion == Vector2.Zero) {
          // TODO define these in a class
          drawable.SetAnimation("s_idle");
          return;
        }
        
        var movementDirection = MapPlacementHelper.DirectionBounds
            .FirstOrDefault(
                entry => Angle.IsBetween(
                    Angle.FromVector(currentMotion), entry.Value.Item1, entry.Value.Item2)).Key;
        switch (movementDirection) {
          case South:
          case SouthWest:
            drawable.SetAnimation("s_walk");
            break;
          case West:
          case NorthWest:
            drawable.SetAnimation("w_walk");
            break;
          case North:
          case NorthEast:
            drawable.SpriteEffects = SpriteEffects.FlipHorizontally;
            drawable.SetAnimation("w_walk");
            break;
          case East:
          case SouthEast:
            drawable.SpriteEffects = SpriteEffects.FlipHorizontally;
            drawable.SetAnimation("s_walk");
            break;
        }
      }
    }
  }
}
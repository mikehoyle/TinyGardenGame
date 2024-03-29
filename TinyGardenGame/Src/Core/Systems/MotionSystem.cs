﻿using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Player.Components;

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
        // OPTIMIZE: These has/gets could be inefficient and warrant another system if there 
        // end up being many moving elements.
        if (_selectionComponentMapper.Has(entity) && _collisionComponentMapper.Has(entity)) {
          _selectionComponentMapper.Get(entity)
              .SetFromMapPlacement(positionComponent, _collisionComponentMapper.Get(entity));
        }
      }
    }
  }
}
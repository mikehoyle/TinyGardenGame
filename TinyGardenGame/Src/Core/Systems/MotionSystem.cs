using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
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
    private ComponentMapper<PlacementComponent> _placementComponentMapper;
    private ComponentMapper<MotionComponent> _motionComponentMapper;
    private ComponentMapper<SelectionComponent> _selectionComponentMapper;
    private ComponentMapper<CollisionFootprintComponent> _collisionComponentMapper;

    public MotionSystem() :
        base(Aspect.All(typeof(PlacementComponent), typeof(MotionComponent))) { }

    public override void Initialize(IComponentMapperService mapperService) {
      _placementComponentMapper = mapperService.GetMapper<PlacementComponent>();
      _motionComponentMapper = mapperService.GetMapper<MotionComponent>();
      _selectionComponentMapper = mapperService.GetMapper<SelectionComponent>();
      _collisionComponentMapper = mapperService.GetMapper<CollisionFootprintComponent>();
    }

    public override void Update(GameTime gameTime) {
      foreach (var entity in ActiveEntities) {
        var currentMotion = _motionComponentMapper.Get(entity).CurrentMotion;
        var placementComponent = _placementComponentMapper.Get(entity);
        placementComponent.SetPositionFromMotionVector(currentMotion);
        // TODO: These has/gets could be inefficient and warrant another system if there 
        // end up being many moving elements.
        if (_selectionComponentMapper.Has(entity) && _collisionComponentMapper.Has(entity)) {
          _selectionComponentMapper.Get(entity)
              .SetFromMapPlacement(placementComponent, _collisionComponentMapper.Get(entity));
        }
      }
    }
  }
}
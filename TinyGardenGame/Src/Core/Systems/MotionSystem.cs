using System.Linq;
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
        UpdateAnimation(entity, currentMotion, positionComponent);
        // OPTIMIZE: These has/gets could be inefficient and warrant another system if there 
        // end up being many moving elements.
        if (_selectionComponentMapper.Has(entity) && _collisionComponentMapper.Has(entity)) {
          _selectionComponentMapper.Get(entity)
              .SetFromMapPlacement(positionComponent, _collisionComponentMapper.Get(entity));
        }
      }
    }

    private void UpdateAnimation(int entityId, Vector2 currentMotion, PositionComponent position) {
      if (_drawableComponentMapper.Has(entityId)) {
        var drawable = _drawableComponentMapper.Get(entityId);
        drawable.SpriteEffects = SpriteEffects.None;
        if (currentMotion == Vector2.Zero) {
          // TODO define these in a class
          var facingDirection = MapPlacementHelper.AngleToDirection(position.Rotation);
          SetAnimationFromDirection(drawable, "idle", facingDirection);
          return;
        }

        var movementDirection = MapPlacementHelper.AngleToDirection(
            Angle.FromVector(currentMotion));
        SetAnimationFromDirection(drawable, "run", movementDirection);
      }
    }

    private void SetAnimationFromDirection(
        DrawableComponent drawable, string animation, MapPlacementHelper.Direction direction) {
      // Remember, the directions are for the tile grid, not the player's viewpoint
      switch (direction) {
        case SouthEast:
          drawable.SetAnimation($"{animation}_down");
          break;
        case NorthWest:
          drawable.SetAnimation($"{animation}_up");
          break;
        case South:
        case SouthWest:
        case West:
          drawable.SetAnimation($"{animation}_left");
          break;
        case North:
        case NorthEast:
        case East:
          drawable.SpriteEffects = SpriteEffects.FlipHorizontally;
          drawable.SetAnimation($"{animation}_left");
          break;
      }
    }
  }
}
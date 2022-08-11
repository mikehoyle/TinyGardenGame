using System;
using System.Diagnostics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using TinyGardenGame.Core.Components;

namespace TinyGardenGame.Units.Systems; 

/**
 * Handle roaming. Current strategy is simple:
 *  - If stationary, pick a random direction and move.
 *  - If moving, pick a random deviation from the current direction.
 */
public class RoamEnemyAiHandler : IEnemyAiHandler {
  private readonly float _roamVariationPerSec;
  private ComponentMapper<PositionComponent> _positionMapper;
  private ComponentMapper<MotionComponent> _motionMapper;
  private readonly Random _random;

  public RoamEnemyAiHandler(float roamVariationPerSec) {
    _roamVariationPerSec = roamVariationPerSec;
    _random = new Random();
  }

  public void Initialize(IComponentMapperService mapperService) {
    _positionMapper = mapperService.GetMapper<PositionComponent>();
    _motionMapper = mapperService.GetMapper<MotionComponent>();
  }

  public void Handle(GameTime gameTime, int entity) {
    if (!_motionMapper.Has(entity)) {
      Debug.WriteLine($"Warning: cannot roam without motion component for entity {entity}");
      return;
    }

    var position = _positionMapper.Get(entity);
    var motion = _motionMapper.Get(entity);

    var movementAngle = motion.CurrentMotion == Vector2.Zero
        ? new Angle(_random.NextSingle(0, 1), AngleType.Revolution) : position.Rotation;

    var updateVariation = _roamVariationPerSec * gameTime.GetElapsedSeconds(); 
    movementAngle += new Angle(_random.NextSingle(-updateVariation, updateVariation));
    
    motion.SetMotionFromAngle(gameTime, movementAngle);
  }
}

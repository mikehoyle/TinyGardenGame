using MonoGame.Extended.Entities;
using NLog;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Plants.Components;

namespace TinyGardenGame.Units.Systems;

/**
 * Generally attempts to attack the tree, or anything it finds on the way
 */
public class AttackTreeEnemyAiHandler : IEnemyAiHandler {
  private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

  private readonly IEnemyAiHandler _backupBehavior;
  private TagManager _tagManager;
  private ComponentMapper<AnimationComponent> _animationComponentMapper;
  private ComponentMapper<MotionComponent> _motionMapper;
  private ComponentMapper<PositionComponent> _positionComponentMapper;

  public AttackTreeEnemyAiHandler(IEnemyAiHandler backupBehavior) {
    _backupBehavior = backupBehavior;
  }

  public void Handle(GameTime gameTime, int entity) {
    if (!_tagManager.IsRegistered(Tags.GreatTree)) {
      Logger.Warn("No Great Tree, AI reverting to backup behavior");
      _backupBehavior.Handle(gameTime, entity);
      return;
    }

    var greatTreePosition = _positionComponentMapper.Get(_tagManager.GetEntity(Tags.GreatTree));
    var position = _positionComponentMapper.Get(entity);
    var targetAngle = position.AngleTo(greatTreePosition);
    _motionMapper.Get(entity).SetMotionFromAngle(gameTime, targetAngle);

    // Set animation
    var direction = AngleToDirection(targetAngle);
    _animationComponentMapper.Put(
        entity,
        new AnimationComponent(AnimationComponent.Action.Run, direction));
  }

  public void Initialize(IComponentMapperService mapperService, TagManager tagManager) {
    _tagManager = tagManager;
    _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
    _motionMapper = mapperService.GetMapper<MotionComponent>();
    // TODO: handle animation differently
    _animationComponentMapper = mapperService.GetMapper<AnimationComponent>();
  }
}

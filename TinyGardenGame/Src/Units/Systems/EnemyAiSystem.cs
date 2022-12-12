using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Config;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Units.Components;

namespace TinyGardenGame.Units.Systems;

public class EnemyAiSystem : EntityUpdateSystem {
  private readonly AttackTreeEnemyAiHandler _attackTreeHandler;

  private readonly RoamEnemyAiHandler _roamHandler;
  private ComponentMapper<EnemyAiComponent> _aiComponentMapper;

  public EnemyAiSystem(ImportantEntities importantEntities) : base(
      Aspect.All(typeof(EnemyAiComponent), typeof(PositionComponent))) {
    _roamHandler = new RoamEnemyAiHandler(GameConfig.Config.AiRoamVariationRadiansPerSec);
    _attackTreeHandler = new AttackTreeEnemyAiHandler(_roamHandler, importantEntities);
  }


  public override void Initialize(IComponentMapperService mapperService) {
    _aiComponentMapper = mapperService.GetMapper<EnemyAiComponent>();

    _roamHandler.Initialize(mapperService);
    _attackTreeHandler.Initialize(mapperService);
  }

  public override void Update(GameTime gameTime) {
    foreach (var entity in ActiveEntities)
      switch (_aiComponentMapper.Get(entity).ActivityState.Peek()) {
        default:
        case EnemyAiComponent.State.Roam:
          _roamHandler.Handle(gameTime, entity);
          break;
        case EnemyAiComponent.State.AttackTree:
          _attackTreeHandler.Handle(gameTime, entity);
          break;
      }
  }
}

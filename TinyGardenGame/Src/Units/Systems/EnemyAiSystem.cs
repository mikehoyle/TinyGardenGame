using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Units.Components;

namespace TinyGardenGame.Units.Systems; 

public class EnemyAiSystem : EntityUpdateSystem {
  private ComponentMapper<EnemyAiComponent> _aiComponentMapper;
  
  private readonly RoamEnemyAiHandler _roamHandler;

  public EnemyAiSystem(Config.Config config)
      : base(Aspect.All(typeof(EnemyAiComponent), typeof(PositionComponent))) {
    _roamHandler = new RoamEnemyAiHandler(config.AiRoamVariationRadiansPerSec);
  }
  
  
  public override void Initialize(IComponentMapperService mapperService) {
    _aiComponentMapper = mapperService.GetMapper<EnemyAiComponent>();
    
    _roamHandler.Initialize(mapperService);
  }

  public override void Update(GameTime gameTime) {
    foreach (var entity in ActiveEntities) {
      switch (_aiComponentMapper.Get(entity).ActivityState) {
        default:
        case EnemyAiComponent.State.Roam:
          _roamHandler.Handle(gameTime, entity);
          break;
      }
    }
  }
}

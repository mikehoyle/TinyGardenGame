using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Plants.Components;
using TinyGardenGame.Player.Components;

namespace TinyGardenGame.Units.Systems; 

/**
 * Exists to make "important" entities more easily available. Kinda hacky.
 */
public class ImportantEntities : EntitySystem {
  private ComponentMapper<PlayerInputComponent> _playerInputMapper;
  private ComponentMapper<GreatTreeComponent> _greatTreeMapper;
  private ComponentMapper<PositionComponent> _positionMapper;
  
  public int? Player { get; private set; }
  public int? GreatTree { get; private set; }

  public ImportantEntities() : base(Aspect.Exclude()) { }
  
  
  public override void Initialize(IComponentMapperService mapperService) {
    _playerInputMapper = mapperService.GetMapper<PlayerInputComponent>();
    _greatTreeMapper = mapperService.GetMapper<GreatTreeComponent>();
    _positionMapper = mapperService.GetMapper<PositionComponent>();
  }

  protected override void OnEntityAdded(int entityId) {
    if (_greatTreeMapper.Has(entityId) && _positionMapper.Has(entityId)) {
      GreatTree = entityId;
    }
    if (_playerInputMapper.Has(entityId) && _positionMapper.Has(entityId)) {
      Player = entityId;
    }
  }

  protected override void OnEntityChanged(int entityId) {
    // For now, assume important entities are created with their marker components and never lose
    // them.
  }

  protected override void OnEntityRemoved(int entityId) {
    if (_greatTreeMapper.Has(entityId)) {
      GreatTree = null;
    }
    if (_playerInputMapper.Has(entityId)) {
      Player = null;
    }
  }
}

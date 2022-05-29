using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Components;

namespace TinyGardenGame.Systems {
  /**
   * Encapsulates debug functionality as much as possible.
   */
  public class DebugSystem : EntityUpdateSystem {
    private readonly Game _game;
    private ComponentMapper<CollisionFootprintComponent> _collisionComponent;
    private ComponentMapper<MotionComponent> _motionComponent;
    private ComponentMapper<PlacementComponent> _placementComponent;
    private ComponentMapper<Sprite> _spriteComponent;
    private ComponentMapper<SelectionComponent> _selectionComponent;
    private Entity _selectionIndicatorEntity;
    
    public Entity PlayerEntity { private get; set; } 

    public DebugSystem(Game game) : base(Aspect.One(
        typeof(CollisionFootprintComponent),
        typeof(MotionComponent),
        typeof(PlacementComponent),
        typeof(Sprite),
        typeof(SelectionComponent))) {
      _game = game;
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _collisionComponent = mapperService.GetMapper<CollisionFootprintComponent>();
      _motionComponent = mapperService.GetMapper<MotionComponent>();
      _placementComponent = mapperService.GetMapper<PlacementComponent>();
      _spriteComponent = mapperService.GetMapper<Sprite>();
      _selectionComponent = mapperService.GetMapper<SelectionComponent>();
      
      _selectionIndicatorEntity = CreateEntity();
    }

    public void LoadContent() {
      if (Config.ShowSelectionIndicator && PlayerEntity != null) {
        var tileSprites = _game.Content.Load<Texture2D>("tilesets/tile_sprites");
        var playerSelection = _selectionComponent.Get(PlayerEntity);
        _selectionIndicatorEntity
            .AttachAnd(new Sprite(new TextureRegion2D(tileSprites, 0, 0, 32, 16)) {
                Origin = new Vector2(16, 8),
            })
            .Attach(new PlacementComponent(playerSelection.SelectedSquare));
      }
    }

    public override void Update(GameTime gameTime) {
      if (PlayerEntity != null && _selectionIndicatorEntity != null) {
        var playerSelection = _selectionComponent.Get(PlayerEntity);
        var indicatorPlacement = _placementComponent.Get(_selectionIndicatorEntity);
        indicatorPlacement.Position = playerSelection.SelectedSquare;
      }
    }
  }
}
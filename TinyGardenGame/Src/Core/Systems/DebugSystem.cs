using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.State;

namespace TinyGardenGame.Core.Systems {
  /**
   * Encapsulates debug functionality as much as possible.
   */
  public class DebugSystem : EntityUpdateSystem {
    private readonly MainGame _game;
    private ComponentMapper<CollisionFootprintComponent> _collisionComponent;
    private ComponentMapper<MotionComponent> _motionComponent;
    private ComponentMapper<PositionComponent> _positionComponent;
    private ComponentMapper<Sprite> _spriteComponent;
    private ComponentMapper<SelectionComponent> _selectionComponent;
    private Entity _selectionIndicatorEntity;
    private PlayerState _playerState;

    public DebugSystem(MainGame game, PlayerState playerState) : base(Aspect.One()) {
      _game = game;
      _playerState = playerState;
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _collisionComponent = mapperService.GetMapper<CollisionFootprintComponent>();
      _motionComponent = mapperService.GetMapper<MotionComponent>();
      _positionComponent = mapperService.GetMapper<PositionComponent>();
      _spriteComponent = mapperService.GetMapper<Sprite>();
      _selectionComponent = mapperService.GetMapper<SelectionComponent>();
      
      _selectionIndicatorEntity = _game.Config.Debug.ShowSelectionIndicator ? CreateEntity() : null;
    }

    public void LoadContent() {
      LoadSelectionIndicator();
    }

    public override void Update(GameTime gameTime) {
      UpdateSelectionIndicator();
    }

    private void LoadSelectionIndicator() {
      if (_game.Config.Debug.ShowSelectionIndicator && _playerState.PlayerEntity != null) {
        var playerSelection = _selectionComponent.Get(_playerState.PlayerEntity);
        var sprite = _game.Content.LoadSprite(SpriteName.SelectedTileOverlay);
        _selectionIndicatorEntity
            .AttachAnd(new DrawableComponent(sprite, RenderLayer.Overlay))
            .Attach(new PositionComponent(playerSelection.SelectedSquare));
      }
    }

    private void UpdateSelectionIndicator() {
      if (_playerState.PlayerEntity != null && _selectionIndicatorEntity != null) {
        var playerSelection = _selectionComponent.Get(_playerState.PlayerEntity);
        var indicatorPlacement = _positionComponent.Get(_selectionIndicatorEntity);
        indicatorPlacement.Position = playerSelection.SelectedSquare;
      }
    }
  }
}
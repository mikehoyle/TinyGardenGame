using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.State;
using TinyGardenGame.Screens;

namespace TinyGardenGame.Core.Systems {
  /**
   * Encapsulates debug functionality as much as possible.
   */
  public class DebugSystem : EntityUpdateSystem {
    private readonly PrimaryGameplayScreen _screen;
    private ComponentMapper<CollisionFootprintComponent> _collisionComponent;
    private ComponentMapper<MotionComponent> _motionComponent;
    private ComponentMapper<PositionComponent> _positionComponent;
    private ComponentMapper<Sprite> _spriteComponent;
    private ComponentMapper<SelectionComponent> _selectionComponent;
    private Entity _selectionIndicatorEntity;
    private readonly PlayerState _playerState;

    public DebugSystem(PrimaryGameplayScreen screen, PlayerState playerState) : base(Aspect.One()) {
      _screen = screen;
      _playerState = playerState;


      screen.Console.SetHp += (caller, val) => _playerState.Hp.CurrentValue = val;
      screen.Console.SetEnergy += (caller, val) => _playerState.Energy.CurrentValue = val;
      screen.Console.MovePlayer += TeleportPlayer;
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _collisionComponent = mapperService.GetMapper<CollisionFootprintComponent>();
      _motionComponent = mapperService.GetMapper<MotionComponent>();
      _positionComponent = mapperService.GetMapper<PositionComponent>();
      _spriteComponent = mapperService.GetMapper<Sprite>();
      _selectionComponent = mapperService.GetMapper<SelectionComponent>();

      _selectionIndicatorEntity =
          _screen.Game.Config.Debug.ShowSelectionIndicator ? CreateEntity() : null;
    }

    public void LoadContent() {
      LoadSelectionIndicator();
    }

    public override void Update(GameTime gameTime) {
      UpdateSelectionIndicator();
    }

    private void LoadSelectionIndicator() {
      if (_screen.Game.Config.Debug.ShowSelectionIndicator && _playerState.PlayerEntity != null) {
        var playerSelection = _selectionComponent.Get(_playerState.PlayerEntity);
        var sprite = _screen.Game.Content.LoadSprite(SpriteName.SelectedTileOverlay);
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

    private void TeleportPlayer(int x, int y) {
      var position = _positionComponent.Get(_playerState.PlayerEntity);

      if (position != null) {
        position.Position = new Vector2(x, y);
      }
      else {
        _screen.Console.WriteLine("No player position component found");
      }
    }
  }
}
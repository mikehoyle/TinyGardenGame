using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Hud;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.State;
using TinyGardenGame.Screens;

namespace TinyGardenGame.Player.Systems {
  public class PlayerInputSystem : EntityUpdateSystem {
    private readonly MainGame _game;
    private readonly HeadsUpDisplay _hud;
    private readonly PrimaryGameplayScreen _screen;
    private readonly PlayerState _playerState;
    private ComponentMapper<MotionComponent> _motionComponentMapper;
    private ComponentMapper<PositionComponent> _positionComponentMapper;
    private ComponentMapper<SelectionComponent> _selectionComponentMapper;
    private ComponentMapper<CollisionFootprintComponent> _collisionComponentMapper;
    private ComponentMapper<DrawableComponent> _drawableComponentMapper;
    private readonly ControlsMapper<PlayerAction> _controlsMapper;

    public PlayerInputSystem(
        PrimaryGameplayScreen screen,
        PlayerState playerState,
        GameMap map)
        : base(Aspect.All(
            typeof(PlayerInputComponent),
            typeof(MotionComponent),
            typeof(PositionComponent),
            typeof(SelectionComponent),
            typeof(CollisionFootprintComponent))) {
      _screen = screen;
      _playerState = playerState;
      // TODO: import these from a separate config, don't hardcode
      _controlsMapper = new ControlsMapper<PlayerAction>()
          .Register(new KeyHeldCondition(Keys.Up), PlayerAction.MoveUp)
          .Register(new KeyHeldCondition(Keys.Down), PlayerAction.MoveDown)
          .Register(new KeyHeldCondition(Keys.Left), PlayerAction.MoveLeft)
          .Register(new KeyHeldCondition(Keys.Right), PlayerAction.MoveRight)
          .Register(new KeyPressedCondition(Keys.Space), PlayerAction.Attack)
          .Register(new KeyPressedCondition(Keys.Q), PlayerAction.PlacePlant)
          .Register(new KeyPressedCondition(Keys.A), PlayerAction.ToggleHoverPlant)
          .Register(new KeyPressedCondition(Keys.W), PlayerAction.DigTrench)
          .Register(
              new KeyPressedCondition(Keys.OemOpenBrackets), PlayerAction.InventorySelectionLeft)
          .Register(
              new KeyPressedCondition(Keys.OemCloseBrackets), PlayerAction.InventorySelectionRight);
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _motionComponentMapper = mapperService.GetMapper<MotionComponent>();
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
      _selectionComponentMapper = mapperService.GetMapper<SelectionComponent>();
      _collisionComponentMapper = mapperService.GetMapper<CollisionFootprintComponent>();
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
    }

    public override void Update(GameTime gameTime) {
      var triggeredActions = _controlsMapper.GetTriggeredActions();
      _playerState.Update(gameTime, triggeredActions);
    }
  }
}
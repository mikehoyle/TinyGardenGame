using MonoGame.Extended.Entities;
using MonoGame.Extended.Screens;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.Hud;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Plants.Systems;
using TinyGardenGame.Player.State;
using TinyGardenGame.Player.Systems;
using TinyGardenGame.Units.Systems;
using TinyGardenGame.Vars;

namespace TinyGardenGame.Screens {
  public class PrimaryGameplayScreen : GameScreen {
    private readonly World _world;
    private readonly DebugSystem _debugSystem;
    private readonly GameState.GameState _gameState;

    public InGameConsole Console { get; }

    public new MainGame Game { get; }

    public PrimaryGameplayScreen(MainGame game, GameMap map) : base(game) {
      Game = game;
      Console = new InGameConsole(game);
      var cameraSystem = new CameraSystem(
          game, MainGame.RenderResolutionWidth, MainGame.RenderResolutionHeight);
      var collisionSystem = new CollisionSystem(map);
      var playerState = new PlayerState();
      _gameState = new GameState.GameState();
      _debugSystem = new DebugSystem(this, playerState);
      var hud = new HeadsUpDisplay(
          game,
          playerState,
          _gameState,
          GraphicsDevice,
          MainGame.RenderResolutionWidth,
          MainGame.RenderResolutionHeight);
      var mapProcessor = new MapProcessor(game, map);
      var importantEntities = new ImportantEntities();
      _world = new WorldBuilder()
          .AddSystem(importantEntities)
          .AddSystem(new PlayerInputSystem(this, playerState, map))
          .AddSystem(new EnemyAiSystem(importantEntities))
          .AddSystem(collisionSystem)
          .AddSystem(new MotionSystem())
          .AddSystem(new GrowthSystem())
          .AddSystem(new AnimationSystem())
          .AddSystem(new DamageSystem(map))
          .AddSystem(new RenderSystem(
              game,
              GraphicsDevice,
              cameraSystem,
              _gameState,
              mapProcessor,
              hud.Draw))
          .AddSystem(cameraSystem)
          .AddSystem(_debugSystem)
          .Build();
      playerState.Initialize(_world, game, collisionSystem, map, mapProcessor);
      
      // TODO: remove this temporary test unit
      Plant.Items[Plant.Type.GreatOak].Instantiate(new Vector2(0, 3), _world.CreateEntity);
      Unit.Instantiate(Unit.Type.Inchworm, _world.CreateEntity, new Vector2(3, 3));
    }

    public override void LoadContent() {
      _debugSystem.LoadContent();
      base.LoadContent();
    }

    public override void Update(GameTime gameTime) {
      _gameState.Update(gameTime);
      _world.Update(gameTime);
    }

    public override void Draw(GameTime gameTime) {
      _world.Draw(gameTime);
    }
  }
}
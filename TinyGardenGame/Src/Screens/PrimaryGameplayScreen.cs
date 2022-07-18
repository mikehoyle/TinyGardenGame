using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Screens;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.Hud;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Plants;
using TinyGardenGame.Plants.Systems;
using TinyGardenGame.Player.State;
using TinyGardenGame.Player.State.Inventory;
using TinyGardenGame.Player.Systems;

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
      var playerState = new PlayerState(game.Config);
      _gameState = new GameState.GameState(game.Config);
      _debugSystem = new DebugSystem(this, playerState);
      var hud = new HeadsUpDisplay(
          game,
          playerState,
          _gameState,
          GraphicsDevice,
          MainGame.RenderResolutionWidth,
          MainGame.RenderResolutionHeight);
      var objectPlacementSystem =
          new ObjectPlacementSystem(this, playerState, map, collisionSystem, cameraSystem);
      _world = new WorldBuilder()
          .AddSystem(new RenderSystem(
              game,
              GraphicsDevice,
              cameraSystem,
              _gameState,
              map,
              objectPlacementSystem.Draw,
              hud.Draw))
          .AddSystem(new PlayerInputSystem(this, playerState, map, objectPlacementSystem))
          .AddSystem(objectPlacementSystem)
          .AddSystem(collisionSystem)
          .AddSystem(new MotionSystem())
          .AddSystem(new GrowthSystem(game))
          .AddSystem(cameraSystem)
          .AddSystem(_debugSystem)
          .Build();
      playerState.InitializePlayerCharacter(_world, game);
      // Test starting item
      playerState.Inventory.AddItem(InventoryItem.ReedSeeds, 5);
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
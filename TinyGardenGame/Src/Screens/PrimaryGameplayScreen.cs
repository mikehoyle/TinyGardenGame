using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.Systems;

namespace TinyGardenGame.Screens {
  public class PrimaryGameplayScreen : GameScreen {
    private TiledMap _tiledMap;
    private TiledMapRenderer _tiledMapRenderer;
    
    private readonly MainGame _game;
    private readonly World _world;
    private readonly CameraSystem _cameraSystem;
    private readonly Entity _playerEntity;
    private readonly DebugSystem _debugSystem;
    private readonly Entity _testPlant;

    public PrimaryGameplayScreen(MainGame game) : base(game) {
      _game = game;
      _cameraSystem = new CameraSystem(
          game, MainGame.RenderResolutionWidth, MainGame.RenderResolutionHeight);
      _debugSystem = new DebugSystem(game);
      _world = new WorldBuilder()
          .AddSystem(new RenderSystem(GraphicsDevice, _cameraSystem))
          .AddSystem(new PlayerInputSystem(game))
          .AddSystem(new CollisionSystem())
          .AddSystem(_cameraSystem)
          .AddSystem(_debugSystem)
          .Build();
      _playerEntity = CreatePlayerCharacter();
      // TODO remove this hacky test thing once the player can place things
      _testPlant = _world.CreateEntity();
      _debugSystem.PlayerEntity = _playerEntity;
    }
    
    public override void LoadContent() {
      // TODO probably remove or rework this clumsy dependency on Tiled.
      _tiledMap = Content.Load<TiledMap>("maps/test-map");
      _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);
      var playerSprite = new Sprite(
          new TextureRegion2D(Content.Load<Texture2D>("sprites/Old hero"),
              19, 17, 10, 15)) {
          Origin = new Vector2(5, 15),
      };
      _playerEntity.Attach(playerSprite);
      LoadTestPlant();
      _debugSystem.LoadContent();
      base.LoadContent();
    }
    
    public override void Update(GameTime gameTime) {
      _tiledMapRenderer.Update(gameTime);
      _world.Update(gameTime);
    }

    public override void Draw(GameTime gameTime) {
      GraphicsDevice.Clear(Color.CornflowerBlue);
      _tiledMapRenderer.Draw(_cameraSystem.ViewMatrix);
      _world.Draw(gameTime);
    }

    private Entity CreatePlayerCharacter() { 
      var player = _world.CreateEntity()
          .AttachAnd(new CameraFollowComponent())
          .AttachAnd(new MotionComponent(_game.Config.PlayerSpeed))
          .AttachAnd(new PlayerInputComponent())
          .AttachAnd(new PlacementComponent(new Vector2(0, 0)))
          .AttachAnd(new CollisionFootprintComponent() {
              // TODO: For now assume the player takes up a whole square.
              // In the future, refine this.
              Footprint = new RectangleF(-0.5f, -0.5f, 1f, 1f),
          })
          .AttachAnd(new SelectionComponent());
      return player;
    }

    private void LoadTestPlant() {
      var sprite = new Sprite(
          new TextureRegion2D(Content.Load<Texture2D>("sprites/test_plant_sprites"),
              0, 0, 64, 48)) {
          // TODO create helpers for these magic numbers 
          // This is the middle of the would-be north-west square
          Origin = new Vector2(32, 24),
      };
      _testPlant
          .AttachAnd(sprite)
          .AttachAnd(new PlacementComponent(new Vector2(5, -5)))
          .AttachAnd(new CollisionFootprintComponent {
              Footprint = new RectangleF(-0.5f, -0.5f, 2, 2),
          });
    }
  }
}
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
using TinyGardenGame.Components;
using TinyGardenGame.Systems;

namespace TinyGardenGame.Screens {
  public class PrimaryGameplayScreen : GameScreen {
    // TODO some consts should be in a config file, or at least their own class
    private const float PlayerSpeed = 3.0f;
    
    private new MainGame Game => (MainGame)base.Game;
    private TiledMap _tiledMap;
    private TiledMapRenderer _tiledMapRenderer;
    private World _world;
    private readonly CameraSystem _cameraSystem;
    private readonly Entity _playerEntity;

    public PrimaryGameplayScreen(MainGame game) : base(game) {
      _cameraSystem = new CameraSystem(
          game, MainGame.RenderResolutionWidth, MainGame.RenderResolutionHeight);
      _world = new WorldBuilder()
          .AddSystem(new RenderSystem(GraphicsDevice, _cameraSystem))
          .AddSystem(new PlayerInputSystem(game))
          .AddSystem(_cameraSystem)
          .Build();
      _playerEntity = CreatePlayerCharacter();
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
          .AttachAnd(new MotionComponent(PlayerSpeed))
          .AttachAnd(new PlayerInputComponent())
          .AttachAnd(new PlacementComponent(new Transform2(0, 0)));
      return player;
    }
  }
}
using Microsoft.Xna.Framework;
using MonoGame.Aseprite.Documents;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Screens;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.Hud;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Plants.Systems;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.Systems;
using AnimatedSprite = MonoGame.Aseprite.Graphics.AnimatedSprite;

namespace TinyGardenGame.Screens {
  public class PrimaryGameplayScreen : GameScreen {
    private readonly MainGame _game;
    private readonly World _world;
    private readonly Entity _playerEntity;
    private readonly DebugSystem _debugSystem;
    private readonly HeadsUpDisplay _hud;

    public PrimaryGameplayScreen(MainGame game) : base(game) {
      _game = game;
      var cameraSystem = new CameraSystem(
          game, MainGame.RenderResolutionWidth, MainGame.RenderResolutionHeight);
      _debugSystem = new DebugSystem(game);
      var map = new MapGenerator().GenerateMap();
      var collisionSystem = new CollisionSystem(map);
      _hud = new HeadsUpDisplay(
          game, GraphicsDevice, MainGame.RenderResolutionWidth, MainGame.RenderResolutionHeight);
      _world = new WorldBuilder()
          .AddSystem(new RenderSystem(game, GraphicsDevice, cameraSystem, map, _hud))
          .AddSystem(new PlayerInputSystem(game, _hud, collisionSystem.IsSpaceBuildable))
          .AddSystem(collisionSystem)
          .AddSystem(new MotionSystem())
          .AddSystem(new GrowthSystem(game))
          .AddSystem(cameraSystem)
          .AddSystem(_debugSystem)
          .Build();
      _playerEntity = CreatePlayerCharacter();
      _debugSystem.PlayerEntity = _playerEntity;
    }
    
    public override void LoadContent() {
      var playerSprite = new AnimatedSprite(
          Content.Load<AsepriteDocument>(Assets.TestAnimatedPlayerSprite)) {
          Origin = new Vector2(10, 28),
      };
      _playerEntity.Attach(new DrawableComponent(new AnimatedSpriteDrawable(playerSprite)));
      _debugSystem.LoadContent();
      base.LoadContent();
    }
    
    public override void Update(GameTime gameTime) {
      _world.Update(gameTime);
    }

    public override void Draw(GameTime gameTime) {
      _world.Draw(gameTime);
    }

    private Entity CreatePlayerCharacter() {
      var player = _world.CreateEntity()
          .AttachAnd(new CameraFollowComponent())
          .AttachAnd(new MotionComponent(_game.Config.PlayerSpeed))
          .AttachAnd(new PlayerInputComponent())
          .AttachAnd(new PositionComponent(MapPlacementHelper.CenterOfMapTile(0, 0)))
          .AttachAnd(new CollisionFootprintComponent(new RectangleF(-0.35f, -0.35f, 0.6f, 0.6f)))
          .AttachAnd(new SelectionComponent());
      return player;
    }
  }
}
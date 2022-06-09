using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Documents;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.Hud;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Plants.Components;
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
      _hud = new HeadsUpDisplay(
          Content, GraphicsDevice, MainGame.RenderResolutionWidth, MainGame.RenderResolutionHeight);
      _world = new WorldBuilder()
          .AddSystem(new RenderSystem(game, GraphicsDevice, cameraSystem, map, _hud))
          .AddSystem(new PlayerInputSystem(game, _hud))
          .AddSystem(new CollisionSystem(map))
          .AddSystem(new MotionSystem())
          .AddSystem(new GrowthSystem(game))
          .AddSystem(cameraSystem)
          .AddSystem(_debugSystem)
          .Build();
      _playerEntity = CreatePlayerCharacter();
      // TODO remove this hacky test thing once the player can place things
      CreateTestPlants();
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

    private void CreateTestPlants() {
      var spriteSheet = Content.Load<Texture2D>(Assets.TestPlantSprites);
      var widePlantSprite = new Sprite(
          new TextureRegion2D(spriteSheet, 0, 0, 64, 52)) {
          // TODO create helpers for these magic numbers 
          // This is the NW corner of the would-be north-west square
          Origin = new Vector2(32, 20),
      };
      
      _world.CreateEntity()
          .AttachAnd(new DrawableComponent(widePlantSprite))
          .AttachAnd(new PositionComponent(new Vector2(5, 5), footprintSize: new Vector2(2, 2)))
          .AttachAnd(new CollisionFootprintComponent(new RectangleF(0, 0, 2, 2)))
          .AttachAnd(new GrowthComponent(TimeSpan.FromSeconds(10)));

      var tallPlantSprite = new Sprite(
          new TextureRegion2D(spriteSheet, 96, 0, 32, 56)) {
          Origin = new Vector2(16, 40),
      };
      
      _world.CreateEntity()
          .AttachAnd(new DrawableComponent(tallPlantSprite))
          .AttachAnd(new PositionComponent(new Vector2(2, 6), footprintSize: new Vector2(1, 1)))
          .AttachAnd(new CollisionFootprintComponent(new RectangleF(0, 0, 1, 1)))
          .AttachAnd(new GrowthComponent(TimeSpan.FromSeconds(15)));
    }
  }
}
using System;
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
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.Plants.Components;
using TinyGardenGame.Plants.Systems;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.Systems;

namespace TinyGardenGame.Screens {
  public class PrimaryGameplayScreen : GameScreen {
    private readonly MainGame _game;
    private readonly World _world;
    private readonly Entity _playerEntity;
    private readonly DebugSystem _debugSystem;

    public PrimaryGameplayScreen(MainGame game) : base(game) {
      _game = game;
      var cameraSystem = new CameraSystem(
          game, MainGame.RenderResolutionWidth, MainGame.RenderResolutionHeight);
      _debugSystem = new DebugSystem(game);
      _world = new WorldBuilder()
          .AddSystem(new RenderSystem(
              GraphicsDevice, cameraSystem, Content.Load<TiledMap>(Assets.TestTiledMap)))
          .AddSystem(new PlayerInputSystem(game))
          .AddSystem(new CollisionSystem())
          .AddSystem(new MotionSystem())
          .AddSystem(new GrowthSystem())
          .AddSystem(cameraSystem)
          .AddSystem(_debugSystem)
          .Build();
      _playerEntity = CreatePlayerCharacter();
      // TODO remove this hacky test thing once the player can place things
      CreateTestPlants();
      _debugSystem.PlayerEntity = _playerEntity;
    }
    
    public override void LoadContent() {
      var playerSprite = new Sprite(
          new TextureRegion2D(Content.Load<Texture2D>(Assets.TestPlayerSprite),
              19, 17, 10, 15)) {
          Origin = new Vector2(5, 15),
      };
      _playerEntity.Attach(new DrawableComponent(playerSprite));
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
          .AttachAnd(new PlacementComponent(MapPlacementHelper.CenterOfMapTile(0, 0)))
          .AttachAnd(new CollisionFootprintComponent() {
              // TODO: For now assume the player takes up half a square,
              // that may be refined in the future
              Footprint = new RectangleF(-0.25f, -0.25f, 0.5f, 0.5f),
          })
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
          .AttachAnd(new PlacementComponent(new Vector2(5, 5), footprintSize: new Vector2(2, 2)))
          .AttachAnd(new CollisionFootprintComponent {
              Footprint = new RectangleF(0, 0, 2, 2),
          })
          .AttachAnd(new GrowthComponent(TimeSpan.FromSeconds(10)));

      var tallPlantSprite = new Sprite(
          new TextureRegion2D(spriteSheet, 96, 0, 32, 56)) {
          Origin = new Vector2(16, 40),
      };
      
      _world.CreateEntity()
          .AttachAnd(new DrawableComponent(tallPlantSprite))
          .AttachAnd(new PlacementComponent(new Vector2(2, 6), footprintSize: new Vector2(1, 1)))
          .AttachAnd(new CollisionFootprintComponent {
              Footprint = new RectangleF(0, 0, 1, 1),
          })
          .AttachAnd(new GrowthComponent(TimeSpan.FromSeconds(15)));
    }
  }
}
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using TinyGardenGame.Plants;
using TinyGardenGame.Plants.Systems;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.Systems;
using AnimatedSprite = MonoGame.Aseprite.Graphics.AnimatedSprite;

namespace TinyGardenGame.Screens {
  public class PrimaryGameplayScreen : GameScreen {
    private readonly World _world;
    private readonly DebugSystem _debugSystem;
    private readonly HeadsUpDisplay _hud;
    
    public InGameConsole Console { get; }
    
    public new MainGame Game { get; }
    public Entity PlayerEntity { get; }

    public PrimaryGameplayScreen(MainGame game, GameMap map) : base(game) {
      Game = game;
      Console = new InGameConsole(game);
      var cameraSystem = new CameraSystem(
          game, MainGame.RenderResolutionWidth, MainGame.RenderResolutionHeight);
      _debugSystem = new DebugSystem(game);
      var collisionSystem = new CollisionSystem(map);
      _hud = new HeadsUpDisplay(
          game, GraphicsDevice, MainGame.RenderResolutionWidth, MainGame.RenderResolutionHeight);
      var objectPlacementSystem =
          new ObjectPlacementSystem(this, map, collisionSystem, cameraSystem);
      _world = new WorldBuilder()
          .AddSystem(new RenderSystem(
              game, GraphicsDevice, cameraSystem, map, objectPlacementSystem.Draw, _hud.Draw))
          .AddSystem(new PlayerInputSystem(this, _hud, map, objectPlacementSystem))
          .AddSystem(objectPlacementSystem)
          .AddSystem(collisionSystem)
          .AddSystem(new MotionSystem())
          .AddSystem(new GrowthSystem(game))
          .AddSystem(cameraSystem)
          .AddSystem(_debugSystem)
          .Build();
      PlayerEntity = CreatePlayerCharacter();
      _debugSystem.PlayerEntity = PlayerEntity;
    }

    private void InitializeConsole() {
      // TODO this is referencing a compiled xna from the MonoGameConsoleCore lib that could get
      // cleaned up.
    }
    
    public override void LoadContent() {
      var playerSprite = Game.Content.LoadAnimated(SpriteName.Player);
      PlayerEntity.Attach(new DrawableComponent(new AnimatedSpriteDrawable(playerSprite)));
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
          .AttachAnd(new MotionComponent(Game.Config.PlayerSpeed))
          .AttachAnd(new PlayerInputComponent())
          .AttachAnd(new PositionComponent(MapPlacementHelper.CenterOfMapTile(0, 0)))
          .AttachAnd(new CollisionFootprintComponent(new RectangleF(-0.35f, -0.35f, 0.6f, 0.6f)))
          .AttachAnd(new SelectionComponent());
      return player;
    }
  }
}
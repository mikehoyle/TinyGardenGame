using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Player.Systems;

namespace TinyGardenGame.Core.Systems {
  /**
   * Render in ordered stages:
   * 1. Map
   * 2. Sprites (in order, back to front)
   * 3. Overlay components
   * 4. Game GUI
   */
  public class RenderSystem : EntityDrawSystem, IUpdateSystem {
    // Because we depth our sprites based on map position, these also
    // define max map size.
    private static readonly float MinDepth = -10_000_00f;
    private static readonly float MaxDepth = 10_000_00f;
    
    private readonly GraphicsDevice _graphicsDevice;
    private readonly CameraSystem _cameraSystem;
    private readonly SpriteBatch _spriteBatch;
    private ComponentMapper<Sprite> _spriteMapper;
    private ComponentMapper<PlacementComponent> _positionComponentMapper;
    private readonly TiledMapRenderer _tiledMapRenderer;

    public RenderSystem(GraphicsDevice graphicsDevice, CameraSystem cameraSystem, TiledMap map)
        : base(Aspect.All(typeof(Sprite), typeof(PlacementComponent))) {
      _graphicsDevice = graphicsDevice;
      _cameraSystem = cameraSystem;
      // TODO probably remove or rework this clumsy dependency on Tiled.
      _tiledMapRenderer = new TiledMapRenderer(graphicsDevice, map);
      _spriteBatch = new SpriteBatch(graphicsDevice);
    }
    
    public override void Initialize(IComponentMapperService mapperService) {
      _spriteMapper = mapperService.GetMapper<Sprite>();
      _positionComponentMapper = mapperService.GetMapper<PlacementComponent>();
    }

    public void Update(GameTime gameTime) {
      _tiledMapRenderer.Update(gameTime);
    }

    public override void Draw(GameTime gameTime) {
      DrawMap(gameTime);
      DrawSprites(gameTime);
    }

    private void DrawMap(GameTime gameTime) {
      _graphicsDevice.Clear(Color.CornflowerBlue);
      _tiledMapRenderer.Draw(
          _cameraSystem.ViewMatrix,
          depth: NormalizeDepth(MinDepth + 1));
    }

    private void DrawSprites(GameTime gameTime) {
      _spriteBatch.Begin(
          sortMode: SpriteSortMode.FrontToBack,
          samplerState: SamplerState.PointClamp,
          transformMatrix: _cameraSystem.Camera.GetViewMatrix());

      foreach (var entity in ActiveEntities) {
        var sprite = _spriteMapper.Get(entity);
        var absolutePosition = _positionComponentMapper.Get(entity).AbsolutePosition;
        sprite.Depth = NormalizeDepth(absolutePosition.Y);
        _spriteBatch.Draw(sprite, absolutePosition);
      }

      _spriteBatch.End();
    }

    /**
     * Clamp a value between our min depth and max depth to between 0 and 1
     * (the default viewport depths).
     */
    private static float NormalizeDepth(float depth) {
      return (depth - MinDepth) / (MaxDepth - MinDepth);
    }
  }
}
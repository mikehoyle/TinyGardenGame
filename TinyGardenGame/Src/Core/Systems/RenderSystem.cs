using System.Linq;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core.Components;
using TinyGardenGame.MapGeneration;
using TinyGardenGame.Player.Systems;

namespace TinyGardenGame.Core.Systems {
  public enum RenderLayer {
    GameObject = 1,
    Overlay = 2,
    Menu = 3,
  }

  /**
   * Render in ordered stages:
   * - Map
   * - Map overlay
   * - Sprites (in order, back to front)
   * - Overlay components
   * - HUD
   * - Menus
   */
  public class RenderSystem : EntityDrawSystem, IUpdateSystem {
    private readonly GraphicsDevice _graphicsDevice;
    private readonly CameraSystem _cameraSystem;
    private readonly GameState.GameState _gameState;
    private readonly RenderSystemDraw _drawHud;
    private readonly SpriteBatch _spriteBatch;
    private ComponentMapper<DrawableComponent> _drawableComponentMapper;
    private ComponentMapper<PositionComponent> _positionComponentMapper;
    private readonly MapProcessor _mapProcessor;
    private readonly Effect _nightEffect;
    private DepthSortComparer _depthComparer;

    public delegate void RenderSystemDraw(SpriteBatch spriteBatch, GameTime gameTime);

    public RenderSystem(
        MainGame game,
        GraphicsDevice graphicsDevice,
        CameraSystem cameraSystem,
        GameState.GameState gameState,
        MapProcessor mapProcessor,
        RenderSystemDraw drawHud)
        : base(Aspect.All(typeof(DrawableComponent), typeof(PositionComponent))) {
      _graphicsDevice = graphicsDevice;
      _cameraSystem = cameraSystem;
      _gameState = gameState;
      _nightEffect = game.Content.Load<Effect>("shaders/night_shader");
      _drawHud = drawHud;
      _spriteBatch = new SpriteBatch(graphicsDevice);
      _mapProcessor = mapProcessor;
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
      _depthComparer = new DepthSortComparer(_drawableComponentMapper, _positionComponentMapper);
    }

    public override void Draw(GameTime gameTime) {
      _spriteBatch.Begin(
          sortMode: SpriteSortMode.Deferred,
          samplerState: SamplerState.PointClamp,
          transformMatrix: _cameraSystem.ViewMatrix,
          effect: _gameState.Clock.IsNight ? _nightEffect : null);
      DrawMap();
      DrawSprites(gameTime);
      _spriteBatch.End();
      // Hud uses its own batch
      _drawHud(_spriteBatch, gameTime);
    }

    private void DrawMap() {
      _graphicsDevice.Clear(Color.CornflowerBlue);
      _mapProcessor.Draw(_spriteBatch, _cameraSystem.Camera.BoundingRectangle);
    }

    private void DrawSprites(GameTime gameTime) {
      // Sort the entities by depth
      var entities = ActiveEntities.ToList().Where(
              // Because we may draw in between updates, ensure our entity set is valid
              entity => _drawableComponentMapper.Has(entity)
                        && _positionComponentMapper.Has(entity))
          .OrderBy(entity => entity, _depthComparer);

      // And now draw in order
      foreach (var entity in entities) {
        var drawable = _drawableComponentMapper.Get(entity);
        var absolutePosition = _positionComponentMapper.Get(entity).AbsolutePosition;
        drawable.Update(gameTime);
        drawable.Draw(_spriteBatch, absolutePosition);
      }
    }

    public void Update(GameTime gameTime) {
      _mapProcessor.Update(gameTime);
    }
  }
}
using System.Linq;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.TextureAtlases;
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
    private readonly TextureRegion2D _meterEmptySprite;
    private readonly TextureRegion2D _meterFullSprite;
    private ComponentMapper<VisibleMeterComponent> _visibleMeterMapper;

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
      _drawHud = drawHud;
      _spriteBatch = new SpriteBatch(graphicsDevice);
      _mapProcessor = mapProcessor;
      
      // Content
      _nightEffect = game.Content.Load<Effect>("shaders/night_shader");
      _meterEmptySprite = game.Content.LoadSprite(Vars.Sprite.Type.LoadingBarEmpty).TextureRegion;
      _meterFullSprite = game.Content.LoadSprite(Vars.Sprite.Type.LoadingBarFull).TextureRegion;
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
      _visibleMeterMapper = mapperService.GetMapper<VisibleMeterComponent>();
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
      
      // TODO: This is a hack! It might mess up rendering of menus and stuff! Needs fixing!
      //    for now, just render in a separate pass after everything else to make up for multiple
      //    drawables being attached to a single entity. This should be in the overlay layer
      foreach (var entity in entities.Where(entity => _visibleMeterMapper.Has(entity))) {
        var absolutePosition = _positionComponentMapper.Get(entity).AbsolutePosition;
        var visibleMeter = _visibleMeterMapper.Get(entity);
        visibleMeter.Draw(_spriteBatch, absolutePosition, _meterFullSprite, _meterEmptySprite);
      }
    }

    public void Update(GameTime gameTime) {
      _mapProcessor.Update(gameTime);
    }
  }
}
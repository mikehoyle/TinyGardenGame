using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.Plants.Components;

namespace TinyGardenGame.Plants.Systems {
  public class GrowthSystem : EntityUpdateSystem {
    private readonly MainGame _game;
    private ComponentMapper<GrowthComponent> _growthComponentMapper;
    private TextureRegion2D _progressBarFullSprite;
    private TextureRegion2D _progressBarEmptySprite;
    private ComponentMapper<PositionComponent> _positionComponentMapper;
    // Maps growing entity to its progress bar
    private readonly Dictionary<int, int> _progressBars;
    private ComponentMapper<DrawableComponent> _drawableComponentMapper;

    public GrowthSystem(MainGame game)
        : base(Aspect.All(typeof(GrowthComponent), typeof(PositionComponent))) {
      _game = game;
      _progressBars = new Dictionary<int, int>();
    }
    
    public override void Initialize(IComponentMapperService mapperService) {
      _growthComponentMapper = mapperService.GetMapper<GrowthComponent>();
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
      
      var spriteTexture = _game.Content.Load<Texture2D>(Assets.TestPlantSprites);
      _progressBarEmptySprite = new TextureRegion2D(spriteTexture, 64, 0, 24, 3);
      _progressBarFullSprite = new TextureRegion2D(spriteTexture, 64, 3, 24, 3); 
    }

    public override void Update(GameTime gameTime) {
      // This is likely to evolve a lot over time as intricacies are added
      foreach (var entity in ActiveEntities) {
        var growthComponent = _growthComponentMapper.Get(entity); 
        if (growthComponent.IncrementGrowth(gameTime.ElapsedGameTime)) {
          _growthComponentMapper.Delete(entity);
          if (_progressBars.ContainsKey(entity)) {
            DestroyEntity(_progressBars[entity]);
            _progressBars.Remove(entity);
          }
          continue;
        }
        
        if (!_progressBars.ContainsKey(entity)) {
          _progressBars[entity] = CreateProgressBar(entity, growthComponent).Id;
        }

        ((ProgressBarDrawable)_drawableComponentMapper.Get(_progressBars[entity]).Drawable)
            .ProgressPercentage = growthComponent.CurrentGrowthPercentage;
      }
    }

    private Entity CreateProgressBar(int growingEntity, GrowthComponent growthComponent) {
      var targetPlacement = _positionComponentMapper.Get(growingEntity);
      var progressBarDrawable =new ProgressBarDrawable(
          _progressBarEmptySprite,
          _progressBarFullSprite,
          growthComponent.CurrentGrowthPercentage);
      return CreateEntity()
          .AttachAnd(new DrawableComponent(progressBarDrawable, RenderLayer.Overlay))
          .AttachAnd(new PositionComponent(targetPlacement.Center));
    }
  }
}
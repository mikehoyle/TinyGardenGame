using System.Collections.Generic;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Core.Systems;
using TinyGardenGame.Plants.Components;

namespace TinyGardenGame.Plants.Systems {
  public class GrowthSystem : EntityUpdateSystem {
    private ComponentMapper<GrowthComponent> _growthComponentMapper;

    private ComponentMapper<PositionComponent> _positionComponentMapper;
    private ComponentMapper<DrawableComponent> _drawableComponentMapper;
    private ComponentMapper<VisibleMeterComponent> _visibleMeterMapper;

    public GrowthSystem()
        : base(Aspect.All(typeof(GrowthComponent), typeof(PositionComponent))) {}

    public override void Initialize(IComponentMapperService mapperService) {
      _growthComponentMapper = mapperService.GetMapper<GrowthComponent>();
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
      _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
      _visibleMeterMapper = mapperService.GetMapper<VisibleMeterComponent>();
    }

    public override void Update(GameTime gameTime) {
      // This is likely to evolve a lot over time as intricacies are added
      foreach (var entity in ActiveEntities) {
        var growthComponent = _growthComponentMapper.Get(entity);
        if (growthComponent.IncrementGrowth(gameTime.ElapsedGameTime)) {
          SetAnimation(entity, growthComponent);
          _growthComponentMapper.Delete(entity);
          if (_visibleMeterMapper.Has(entity)) {
            _visibleMeterMapper.Delete(entity);
          }

          continue;
        }

        SetAnimation(entity, growthComponent);
        if (!_visibleMeterMapper.Has(entity)) {
          _visibleMeterMapper.Put(entity, new VisibleMeterComponent {
              Offset = _positionComponentMapper.Get(entity).FootprintSizeInTiles,
          });
        }

        _visibleMeterMapper.Get(entity).CurrentFillPercentage =
            growthComponent.CurrentGrowthPercentage;
      }
    }

    private void SetAnimation(int entity, GrowthComponent growthComponent) {
      if (_drawableComponentMapper.Has(entity)) {
        var drawable = _drawableComponentMapper.Get(entity);
        drawable.SetAnimation(growthComponent.CurrentGrowthAnimationName());
      }
    }
  }
}
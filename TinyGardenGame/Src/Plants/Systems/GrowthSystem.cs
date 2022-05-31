using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;
using TinyGardenGame.Plants.Components;

namespace TinyGardenGame.Plants.Systems {
  public class GrowthSystem : EntityUpdateSystem {
    private ComponentMapper<GrowthComponent> _growthComponentMapper;
    public GrowthSystem() : base(Aspect.All(typeof(GrowthComponent))) {}
    
    public override void Initialize(IComponentMapperService mapperService) {
      _growthComponentMapper = mapperService.GetMapper<GrowthComponent>();
    }

    public override void Update(GameTime gameTime) {
      // This is likely to evolve a lot over time as intricacies are added
      foreach (var entity in ActiveEntities) {
        _growthComponentMapper.Get(entity).IncrementGrowth(gameTime.ElapsedGameTime);
      }
    }
  }
}
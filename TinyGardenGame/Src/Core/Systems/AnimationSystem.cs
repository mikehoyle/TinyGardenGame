using System.Collections.Generic;
using System.Diagnostics;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core.Components;

namespace TinyGardenGame.Core.Systems; 

public class AnimationSystem : EntityUpdateSystem {
  private static Dictionary<AnimationComponent.Action, string> AnimationNames = new() {
      { AnimationComponent.Action.Idle, "idle" },
      { AnimationComponent.Action.Run, "run" },
      { AnimationComponent.Action.Attack, "attack" },
  };

  private ComponentMapper<DrawableComponent> _drawableComponentMapper;
  private ComponentMapper<AnimationComponent> _animationComponentMapper;

  public AnimationSystem()
      : base(Aspect.All(typeof(DrawableComponent), typeof(AnimationComponent))) {}
  
  public override void Initialize(IComponentMapperService mapperService) {
    _drawableComponentMapper = mapperService.GetMapper<DrawableComponent>();
    _animationComponentMapper = mapperService.GetMapper<AnimationComponent>();
  }

  public override void Update(GameTime gameTime) {
    foreach (var entity in ActiveEntities) {
      var drawable = _drawableComponentMapper.Get(entity);
      if (drawable.Drawable.PossibleAnimations.Count == 0) {
        Debug.WriteLine("Cannot animate drawable with no available animations");
        continue;
      }

      var animation = _animationComponentMapper.Get(entity);
      SetAnimationFromDirection(
          drawable,
          AnimationNames[animation.AnimationAction],
          animation.AnimationDirection,
          animation.Loop);
      
      // By default, treat animation requests as one-offs
      _animationComponentMapper.Delete(entity);
    }
  }

  private static void SetAnimationFromDirection(
      DrawableComponent drawable,
      string animation,
      Direction? direction,
      bool loop = true) {
    // Remember, the directions are for the tile grid, not the player's viewpoint
    drawable.SpriteEffects = SpriteEffects.None;
    var directionString = "";
    if (direction.HasValue) {
      switch (direction) {
        case SouthEast:
          directionString = drawable.Drawable.PossibleAnimations.Contains($"{animation}_down")
              ? "_down" : "_right";
          break;
        case NorthWest:
          directionString = drawable.Drawable.PossibleAnimations.Contains($"{animation}_up")
              ? "_up" : "_left";
          break;
        case South:
        case SouthWest:
        case West:
          directionString = "_left";
          break;
        case North:
        case NorthEast:
        case East:
          if (drawable.Drawable.PossibleAnimations.Contains($"{animation}_right")) {
            directionString = "_right";
          } else {
            drawable.SpriteEffects = SpriteEffects.FlipHorizontally;
            directionString = "_left";
          }
          break;
      }
    }

    drawable.SetAnimation($"{animation}{directionString}", loop);
  }
}

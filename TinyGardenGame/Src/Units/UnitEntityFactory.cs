using System;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Entities;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Units.Components;

namespace TinyGardenGame.Units; 

/**
 * Builds NPC units.
 */
public class UnitEntityFactory {
  private static readonly KeyedCollection<Unit.Type, Unit> Units = new(unit => unit.UnitType);

  static UnitEntityFactory() {
    Units.Add(new Unit {
        UnitType = Unit.Type.Inchworm,
        Sprite = SpriteName.Inchworm,
        CollisionFootprint = new RectangleF(-0.25f, -0.25f, 0.5f, 0.5f),
        Hp = 25,
        SpeedTilesPerSec = 1f,
        InitialBehavior = EnemyAiComponent.State.Roam,
    });
  }

  private readonly ContentManager _content;

  public UnitEntityFactory(ContentManager content) {
    _content = content;
  }
  
  public Entity Build(Unit.Type unitType, Func<Entity> createEntityFunc, Vector2 position) {
    var unit = Units[unitType];
    return createEntityFunc()
        .AttachAnd(new PositionComponent(position))
        .AttachAnd(new CollisionFootprintComponent(unit.CollisionFootprint)) 
        .AttachAnd(
            new DrawableComponent(new AnimatedSpriteDrawable(_content.LoadAnimated(unit.Sprite))))
        .AttachAnd(new MotionComponent(unit.SpeedTilesPerSec))
        .AttachAnd(new EnemyAiComponent(unit.InitialBehavior));
  }
}

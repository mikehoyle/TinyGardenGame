using System;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Units.Components;

namespace TinyGardenGame.Vars;

/// <summary>
///   Extends TOML-generated var.
/// </summary>
public partial class Unit
{
  private RectangleF CollisionFootprintRect => new(
    CollisionFootprint.X,
    CollisionFootprint.Y,
    CollisionFootprint.Width,
    CollisionFootprint.Height);

  public static Entity Build(Type unitType, Func<Entity> createEntityFunc, Vector2 position) {
    var unit = Items[unitType];
    return createEntityFunc()
      .AttachAnd(new PositionComponent(position))
      .AttachAnd(new CollisionFootprintComponent(unit.CollisionFootprintRect))
      .AttachAnd(
        new DrawableComponent(new AnimatedSpriteDrawable(
            Platform.Content.LoadAnimated(unit.Sprite.Id))))
      .AttachAnd(new MotionComponent(unit.SpeedTilesPerSec))
      .AttachAnd(new EnemyAiComponent(unit.InitalBehavior))
      .AttachAnd(
        new DamageRecipientComponent(
          unit.Hp,
          unit.CollisionFootprintRect.ToDrawing(),
          DamageRecipientComponent.Category.Enemy))
      .AttachAnd(new VisibleMeterComponent { Offset = new Vector2(0, 3) });
  }
}

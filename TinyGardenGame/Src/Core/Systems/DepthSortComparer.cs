using System.Collections.Generic;
using MonoGame.Extended.Entities;
using TinyGardenGame.Core.Components;

namespace TinyGardenGame.Core.Systems {
  /**
   * Sorts a number of drawable entities by depth.
   * TODO: Fix this potentially inconsistent sort
   */
  public class DepthSortComparer : IComparer<int> {
    private readonly ComponentMapper<DrawableComponent> _drawableComponentMapper;
    private readonly ComponentMapper<PositionComponent> _positionComponentMapper;

    public DepthSortComparer(
        ComponentMapper<DrawableComponent> drawableComponentMapper,
        ComponentMapper<PositionComponent> positionComponentMapper) {
      _drawableComponentMapper = drawableComponentMapper;
      _positionComponentMapper = positionComponentMapper;
    }

    public int Compare(int entity1, int entity2) {
      // OPTIMIZE: These lookups should be fast (O(1)), but may still be an efficiency issue
      // This is also far more continued sorting than is really necessary, as many of
      // the game components will never move.
      // In short, look here if optimization is needed
      var layer1 = _drawableComponentMapper.Get(entity1).RenderLayer;
      var layer2 = _drawableComponentMapper.Get(entity2).RenderLayer;
      if (layer1 != layer2) {
        return layer1 - layer2;
      }

      // Sprite1 is in front of Sprite2 if its SE-most point is greater (X&Y) than
      // the NW origin of Sprite2.
      // This is only sound given some assumptions about the entities
      // (that they don't overlap, and take up about a tile). Those may break in the future
      // we shall see.
      var pos1 = _positionComponentMapper.Get(entity1).Position;
      var depth1 = _positionComponentMapper.Get(entity1).EffectiveRenderDepth;
      var pos2 = _positionComponentMapper.Get(entity2).Position;
      if (pos1 == pos2) {
        return 0;
      }

      if (depth1.X == pos2.X || depth1.Y == pos2.Y) {
        return _positionComponentMapper.Get(entity2).FootprintSizeInTiles != Vector2.Zero
            ? -1
            : 0;
      }

      return ((depth1.X > pos2.X) && (depth1.Y > pos2.Y)) ? 1 : -1;
    }
  }
}
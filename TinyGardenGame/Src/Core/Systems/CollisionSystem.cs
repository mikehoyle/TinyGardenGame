using Microsoft.Xna.Framework;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using QuadTrees;
using QuadTrees.QTreeRectF;
using TinyGardenGame.Core.Components;
using TinyGardenGame.MapGeneration;
using RectangleF = System.Drawing.RectangleF;

namespace TinyGardenGame.Core.Systems {
  /**
   * Detects and resolves collisions. Note: must come after other
   * movement systems.
   */
  public class CollisionSystem : EntityUpdateSystem, IIsSpaceOccupied {
    private readonly GameMap _map;
    private ComponentMapper<PositionComponent> _positionComponentMapper;
    private ComponentMapper<CollisionFootprintComponent> _collisionComponentMapper;
    private ComponentMapper<MotionComponent> _motionComponentMapper;
    private readonly QuadTreeRectF<CollidableObject> _collisionQuadTree;
    
    public delegate bool IsSpaceBuildableDel(RectangleF target);

    public CollisionSystem(GameMap map) : base(Aspect.All(
        typeof(PositionComponent), typeof(CollisionFootprintComponent), typeof(MotionComponent))) {
      _map = map;
      _collisionQuadTree = new QuadTreeRectF<CollidableObject>(
          new RectangleF(_map.Bounds.X, _map.Bounds.Y, _map.Bounds.Width, _map.Bounds.Height));
    }
    
    public override void Initialize(IComponentMapperService mapperService) {
      _positionComponentMapper = mapperService.GetMapper<PositionComponent>();
      _collisionComponentMapper = mapperService.GetMapper<CollisionFootprintComponent>();
      _motionComponentMapper = mapperService.GetMapper<MotionComponent>();
    }

    // TODO: Handle edge of map collision
    // This currently only handles collision of moving entities (such as the player) into
    // stationary entities. That may need to expand in the future.
    public override void Update(GameTime gameTime) {
      foreach (var entity in ActiveEntities) {
        var position = _positionComponentMapper.Get(entity);
        var collisionFootprint = _collisionComponentMapper.Get(entity);
        var motion = _motionComponentMapper.Get(entity);

        var collisionRect = CollidableObject.GetCollisionRect(
            position.Position + motion.CurrentMotion, collisionFootprint);
        var correctionVec = Vector2.Zero;
        foreach (var collisionTarget in _collisionQuadTree.GetObjects(collisionRect)) {
          // This is assuming collision targets never overlap, and an entity will only
          // ever intersect two objects in different axes. It may need revisiting to not
          // overcorrect if that assumption ever fails.
          correctionVec += CalculatePenetrationVector(collisionRect, collisionTarget.Rect);
        }

        foreach (var collidingTile in _map.GetIntersectingTiles(collisionRect)) {
          // TODO fix, tile collisions are broken
          if (collidingTile.Tile.IsNonTraversable) {
            correctionVec += CalculatePenetrationVector(
                collisionRect, new RectangleF(collidingTile.X, collidingTile.Y, 1, 1));
          }
        }

        // To avoid possible overcorrecting, which might create a jitter effect, we
        // bottom out the correction at zero.
        motion.CurrentMotion = correctionVec.Length() < motion.CurrentMotion.Length()
            ? motion.CurrentMotion - correctionVec
            : Vector2.Zero;
      }
    }

    private static Vector2 CalculatePenetrationVector(RectangleF source, RectangleF target) {
      var intersectingRectangle = new RectangleF(source.X, source.Y, source.Width, source.Height);
      intersectingRectangle.Intersect(target);
      
      if (intersectingRectangle.Width < intersectingRectangle.Height) {
        var d = RectCenter(source).X < RectCenter(target).X
            ? intersectingRectangle.Width : -intersectingRectangle.Width;
        return new Vector2(d, 0);
      }
      else {
        var d = RectCenter(source).Y < RectCenter(target).Y
            ? intersectingRectangle.Height
            : -intersectingRectangle.Height;
        return new Vector2(0, d);
      }
    }

    private static Vector2 RectCenter(RectangleF rect) {
      return new Vector2(rect.X + rect.Width * 0.5f, rect.Y + rect.Height * 0.5f);
    }
    
    protected override void OnEntityAdded(int entityId) {
      if (_positionComponentMapper.Has(entityId)
          && _collisionComponentMapper.Has(entityId)
          && !_motionComponentMapper.Has(entityId)) {
        _collisionQuadTree.Add(new CollidableObject(
            entityId,
            _positionComponentMapper.Get(entityId),
            _collisionComponentMapper.Get(entityId)));
      }
    }
    
    protected override void OnEntityChanged(int entityId) {
      var dummy = new CollidableObject(entityId);
      if (_collisionQuadTree.Contains(dummy)) {
        _collisionQuadTree.Remove(dummy);
      }
      
      OnEntityAdded(entityId);
    }
    
    protected override void OnEntityRemoved(int entityId) {
      _collisionQuadTree.Remove(new CollidableObject(entityId));
    }

    public bool IsSpaceOccupied(RectangleF target) {
      return _collisionQuadTree.GetObjects(target).Count > 0;
    }
  }

  public class CollidableObject : IRectFQuadStorable {
    private readonly int _entityId;
    public RectangleF Rect { get; }

    /**
     * Dummy that should only be used for #Contains calls 
     */
    public CollidableObject(int entityId) {
      _entityId = entityId;
    }
    
    public CollidableObject(
        int entityId,
        PositionComponent position,
        CollisionFootprintComponent collisionFootprint) {
      _entityId = entityId;
      Rect = GetCollisionRect(position.Position, collisionFootprint);
    }

    public static RectangleF GetCollisionRect(
        Vector2 position, CollisionFootprintComponent collisionFootprint) {
      return new RectangleF(
          position.X + collisionFootprint.Footprint.Left,
          position.Y + collisionFootprint.Footprint.Top,
          collisionFootprint.Footprint.Width,
          collisionFootprint.Footprint.Height);
    }

    /**
     * Hashed based simply on EntityId, which is already a unique integer identifier
     * of entities, for which there should only be one CollidableObject.
     */
    public override int GetHashCode() {
      return _entityId;
    }
  }
}
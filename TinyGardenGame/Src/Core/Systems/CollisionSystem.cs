using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using TinyGardenGame.Core.Components;

namespace TinyGardenGame.Core.Systems {
  /**
   * Detects and resolves collisions. Note: must come after other
   * movement systems.
   */
  public class CollisionSystem : EntityUpdateSystem {
    private readonly KeyedCollection<int, CollisionActor> _collisionActors;
    private ComponentMapper<PlacementComponent> _placementComponentMapper;
    private ComponentMapper<CollisionFootprintComponent> _collisionMapper;
    private CollisionComponent _collisionEngine;

    public CollisionSystem() :
        base(Aspect.All(typeof(PlacementComponent), typeof(CollisionFootprintComponent))) {
      _collisionActors = new KeyedCollection<int, CollisionActor>(actor => actor.EntityId);
    }
    
    public override void Initialize(IComponentMapperService mapperService) {
      _placementComponentMapper = mapperService.GetMapper<PlacementComponent>();
      _collisionMapper = mapperService.GetMapper<CollisionFootprintComponent>();
      // Note: this is not an ECS component, but is named and made to be used as a
      // monogame game component. We aren't using it that way.
      _collisionEngine = new CollisionComponent(
          // TODO: These numbers are 100% arbitrary but should represent rough map bounds
          new RectangleF(-500f, -500f, 1000f, 1000f));
    }

    public override void Update(GameTime gameTime) {
      foreach (var entity in ActiveEntities) {
        if (_collisionActors.TryGetValue(entity, out var actor)) {
          actor.UpdateBounds(); 
        } else {
          AddCollisionActor(entity);
        }
      }

      // Always check if entities have been removed entirely.
      // I *really* wish there was a cleaner way to do this (like with events), but any useful
      // functionality is private in the Extended library.
      foreach (var removedEntity in _collisionActors.Keys.Except(ActiveEntities)) {
        RemoveCollisionActor(removedEntity);
      }

      _collisionEngine.Update(gameTime);
    }
    
    private void RemoveCollisionActor(int entityId) {
      if (_collisionActors.TryGetValue(entityId, out var actor)) {
        _collisionActors.Remove(actor);
        _collisionEngine.Remove(actor);
      }
    }

    private void AddCollisionActor(int entityId) {
      var actor = new CollisionActor(
          entityId,
          _placementComponentMapper,
          _collisionMapper);
      _collisionActors.Add(actor);
      _collisionEngine.Insert(actor);
    }
  }

  class CollisionActor : ICollisionActor {
    // We store refs to the mappers instead of the components themselves, as in theory the
    // components to any given entity could change at any time.
    private readonly ComponentMapper<PlacementComponent> _placementComponentMapper;
    private readonly ComponentMapper<CollisionFootprintComponent>
        _collisionFootprintComponentMapper;

    // Allocate local bounds to prevent heap/garbage-collector churn from
    // re-allocating rectangle struct every update, and provide cached bounds to accomodate
    // multiple queries per update.
    private RectangleF _bounds;
    
    public int EntityId { get; }
    public IShapeF Bounds => _bounds;

    public CollisionActor(
        int associatedEntity,
        ComponentMapper<PlacementComponent> placementComponentMapper,
        ComponentMapper<CollisionFootprintComponent> collisionFootprintComponentMapper) {
      EntityId = associatedEntity;
      _placementComponentMapper = placementComponentMapper;
      _collisionFootprintComponentMapper = collisionFootprintComponentMapper;
      _bounds = new RectangleF();
      UpdateBounds();
    }
    
    public void OnCollision(CollisionEventArgs collisionInfo) {
      var placementComponent = _placementComponentMapper.Get(EntityId);
      placementComponent.Position -= collisionInfo.PenetrationVector;
    }

    public void UpdateBounds() {
      var placementComponent = _placementComponentMapper.Get(EntityId);
      var collisionFootprintComponent = _collisionFootprintComponentMapper.Get(EntityId);
      _bounds.X = placementComponent.Position.X + collisionFootprintComponent.Footprint.X;
      _bounds.Y = placementComponent.Position.Y + collisionFootprintComponent.Footprint.Y;
      _bounds.Width = collisionFootprintComponent.Footprint.Width;
      _bounds.Height = collisionFootprintComponent.Footprint.Height;
    }
  }
}
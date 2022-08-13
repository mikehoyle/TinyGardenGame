using System.Collections.Generic;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using QuadTrees;
using QuadTrees.QTreeRectF;
using TinyGardenGame.Core.Components;
using TinyGardenGame.MapGeneration;

namespace TinyGardenGame.Core.Systems {
  /**
   * Handle all components that can deal and take damage.
   * OPTIMIZE: This whole class is kind of hacky an unoptimized to get around all the annoying
   *     internal-only components of Extended's ECS system.
   */
  public class DamageSystem : EntityUpdateSystem {
    private readonly HashSet<int> _damageSourceEntities;
    private readonly QuadTreeRectF<DamageReceivingEntity> _damageRecipientQuadTree;

    private ComponentMapper<DamageRecipientComponent> _damageRecipientMapper;
    private ComponentMapper<DamageSourceComponent> _damageSourceMapper;
    private ComponentMapper<PositionComponent> _positionMapper;

    public DamageSystem(GameMap map) : base(
        Aspect.One(typeof(DamageRecipientComponent), typeof(DamageSourceComponent))) {
      _damageSourceEntities = new HashSet<int>();
      _damageRecipientQuadTree = new QuadTreeRectF<DamageReceivingEntity>(
          new SysRectangleF(map.Bounds.X, map.Bounds.Y, map.Bounds.Width, map.Bounds.Height));
    }

    public override void Initialize(IComponentMapperService mapperService) {
      _damageRecipientMapper = mapperService.GetMapper<DamageRecipientComponent>();
      _damageSourceMapper = mapperService.GetMapper<DamageSourceComponent>();
      _positionMapper = mapperService.GetMapper<PositionComponent>();
    }

    public override void Update(GameTime gameTime) {
      // OPTIMIZE: This is a naive re-shaking of the tree which is not ideal to do every single
      // update pass. Consider only processing for entities on camera?
      foreach (var damageRecipient in _damageRecipientQuadTree.GetAllObjects()) {
        _damageRecipientQuadTree.Move(damageRecipient);
      }

      foreach (var entity in _damageSourceEntities) {
        if (!_damageSourceMapper.Has(entity)) {
          _damageSourceEntities.Remove(entity);
          continue;
        }

        var damageSource = _damageSourceMapper.Get(entity);
        if (damageSource.IsPersistent) {
          ApplyDamageFromSource(damageSource);
        }
        else {
          damageSource.CurrentLifetime += gameTime.ElapsedGameTime;
          if (damageSource.CurrentLifetime > damageSource.WindupTime) {
            ApplyDamageFromSource(damageSource);
            _damageSourceMapper.Delete(entity);
          }
        }
      }
    }

    protected override void OnEntityAdded(int entityId) {
      if (_damageRecipientMapper.Has(entityId) && _positionMapper.Has(entityId)) {
        _damageRecipientQuadTree.Add(
            new DamageReceivingEntity(entityId,
                _damageRecipientMapper.Get(entityId), _positionMapper.Get(entityId)));
      }

      if (_damageSourceMapper.Has(entityId)) {
        _damageSourceEntities.Add(entityId);
      }
    }

    protected override void OnEntityChanged(int entityId) {
      var dummy = new DamageReceivingEntity(entityId);
      if (_damageRecipientQuadTree.Contains(dummy)) {
        _damageRecipientQuadTree.Remove(dummy);
      }

      _damageSourceEntities.Remove(entityId);
      OnEntityAdded(entityId);
    }

    protected override void OnEntityRemoved(int entityId) {
      _damageRecipientQuadTree.Remove(new DamageReceivingEntity(entityId));
      _damageSourceEntities.Remove(entityId);
    }

    private void ApplyDamageFromSource(DamageSourceComponent source) {
      foreach (var damageRecipient in _damageRecipientQuadTree.GetObjects(source.DamageHitbox)) {
        damageRecipient.DamageRecipient.Hp -= source.DamageDealt;
        // TODO: handle HP going to zero
      }
    }
  }

  public class DamageReceivingEntity : IRectFQuadStorable {
    public DamageRecipientComponent DamageRecipient { get; }
    private readonly PositionComponent _position;
    private readonly int _entityId;
    private SysRectangleF _rect;

    public SysRectangleF Rect {
      get {
        UpdateRect();
        return _rect;
      }
    }

    public DamageReceivingEntity(int entityId) {
      _entityId = entityId;
    }

    public DamageReceivingEntity(
        int entityId, DamageRecipientComponent damageRecipient, PositionComponent position) {
      _entityId = entityId;
      DamageRecipient = damageRecipient;
      _position = position;
      UpdateRect();
    }

    private void UpdateRect() {
      _rect.X = _position.Position.X + DamageRecipient.Hitbox.X;
      _rect.Y = _position.Position.Y + DamageRecipient.Hitbox.X;
      _rect.Width = DamageRecipient.Hitbox.Width;
      _rect.Height = DamageRecipient.Hitbox.Height;
    }

    /**
     * Hashed based simply on EntityId for easy equality.
     */
    public override int GetHashCode() {
      return _entityId;
    }

    public override bool Equals(object obj) {
      return obj is DamageReceivingEntity && GetHashCode() == obj.GetHashCode();
    }
  }
}
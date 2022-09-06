using System.Collections.Generic;
using Clipper2Lib;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using NLog;
using QuadTrees;
using QuadTrees.QTreeRectF;
using TinyGardenGame.Core.Components;
using TinyGardenGame.MapGeneration;

namespace TinyGardenGame.Core.Systems;

/**
 * Handle all components that can deal and take damage.
 * OPTIMIZE: This whole class is kind of hacky an unoptimized to get around all the annoying
 * internal-only components of Extended's ECS system.
 */
public class DamageSystem : EntityUpdateSystem {
  private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
  private readonly ClipperD _clipper;
  private readonly QuadTreeRectF<DamageReceivingEntity> _damageRecipientQuadTree;

  private readonly HashSet<int> _damageSourceEntities;

  private ComponentMapper<DamageRecipientComponent> _damageRecipientMapper;
  private ComponentMapper<DamageSourceComponent> _damageSourceMapper;
  private ComponentMapper<PositionComponent> _positionMapper;
  private ComponentMapper<VisibleMeterComponent> _visibleMeterMapper;

  public DamageSystem(GameMap map) : base(
      Aspect.One(typeof(DamageRecipientComponent), typeof(DamageSourceComponent))) {
    _damageSourceEntities = new HashSet<int>();
    _damageRecipientQuadTree = new QuadTreeRectF<DamageReceivingEntity>(
        new SysRectangleF(map.Bounds.X, map.Bounds.Y, map.Bounds.Width, map.Bounds.Height));
    _clipper = new ClipperD( /* scale = */ 4);
  }

  public override void Initialize(IComponentMapperService mapperService) {
    _damageRecipientMapper = mapperService.GetMapper<DamageRecipientComponent>();
    _damageSourceMapper = mapperService.GetMapper<DamageSourceComponent>();
    _positionMapper = mapperService.GetMapper<PositionComponent>();
    _visibleMeterMapper = mapperService.GetMapper<VisibleMeterComponent>();
  }

  public override void Update(GameTime gameTime) {
    // OPTIMIZE: This is a naive re-shaking of the tree which is not ideal to do every single
    // update pass. Consider only processing for entities on camera?
    foreach (var damageRecipient in _damageRecipientQuadTree.GetAllObjects()) {
      _damageRecipientQuadTree.Move(damageRecipient);

      if (_visibleMeterMapper.Has(damageRecipient.EntityId)) {
        var recipientComponent = damageRecipient.DamageRecipient;
        _visibleMeterMapper.Get(damageRecipient.EntityId).CurrentFillPercentage =
            recipientComponent.CurrentHp / recipientComponent.MaxHp;
      }
    }

    foreach (var entity in _damageSourceEntities) {
      if (!_damageSourceMapper.Has(entity)) {
        _damageSourceEntities.Remove(entity);
        continue;
      }

      var damageSource = _damageSourceMapper.Get(entity);
      var position = _positionMapper.Get(entity);
      if (damageSource.IsPersistent) {
        ApplyDamageFromSource(damageSource, position);
      }
      else {
        damageSource.CurrentLifetime += gameTime.ElapsedGameTime;
        if (damageSource.CurrentLifetime > damageSource.WindupTime) {
          ApplyDamageFromSource(damageSource, position);
          _damageSourceMapper.Delete(entity);
        }
      }
    }
  }

  protected override void OnEntityAdded(int entityId) {
    if (_damageRecipientMapper.Has(entityId) && _positionMapper.Has(entityId))
      _damageRecipientQuadTree.Add(
          new DamageReceivingEntity(
              entityId,
              _damageRecipientMapper.Get(entityId),
              _positionMapper.Get(entityId)));

    if (_damageSourceMapper.Has(entityId)) _damageSourceEntities.Add(entityId);
  }

  protected override void OnEntityChanged(int entityId) {
    var dummy = new DamageReceivingEntity(entityId);
    if (_damageRecipientQuadTree
        .Contains(dummy)) // OPTIMIZE: this also is an unnecessary amount of churn
      _damageRecipientQuadTree.Remove(dummy);

    _damageSourceEntities.Remove(entityId);
    OnEntityAdded(entityId);
  }

  protected override void OnEntityRemoved(int entityId) {
    _damageRecipientQuadTree.Remove(new DamageReceivingEntity(entityId));
    _damageSourceEntities.Remove(entityId);
  }

  private void ApplyDamageFromSource(DamageSourceComponent source, PositionComponent position) {
    source.DamageType.Switch(
        damageSource => ApplyAoeDamage(source, damageSource, position),
        targetedSource => ApplyDamage(targetedSource.TargetEntityId, source.DamageDealt));
  }

  private void ApplyAoeDamage(
      DamageSourceComponent source,
      AoeDamageSource aoeDamageSource,
      PositionComponent position) {
    var sourcePoly = aoeDamageSource.TranslatedPoly(position.Position);
    foreach (var damageRecipient in _damageRecipientQuadTree.GetObjects(
                 sourcePoly.BoundingRectangle.ToDrawing())) {
      if (!source.TargetSet.Contains(damageRecipient.DamageRecipient.RecipientCategory)) continue;
      _clipper.Clear();
      _clipper.AddSubject(sourcePoly.ToClipperPath());
      _clipper.AddClip(damageRecipient.Rect.ToClipperPath());
      var result = new PolyTreeD();
      _clipper.Execute(ClipType.Intersection, FillRule.EvenOdd, result);
      if (result.Count > 0)
        ApplyDamage(damageRecipient.EntityId, source.DamageDealt);
      else
        Logger.Debug(
            "Damage matched in quadtree but didn't clip for entity {EntityId}",
            damageRecipient.EntityId);
    }
  }

  private void ApplyDamage(int damageRecipientEntity, double damage) {
    if (_damageRecipientMapper.Has(damageRecipientEntity)) {
      var recipient = _damageRecipientMapper.Get(damageRecipientEntity);
      recipient.CurrentHp -= damage;
      if (recipient.CurrentHp <
          0) // TODO: More elegantly handle unit death instead of just deleting them
        DestroyEntity(damageRecipientEntity);
    }
    else {
      Logger.Warn(
          "Cannot apply damage to {0}, entity has no damage recipient component",
          damageRecipientEntity);
    }
  }
}

public class DamageReceivingEntity : IRectFQuadStorable {
  private readonly PositionComponent _position;
  private SysRectangleF _rect;

  public DamageReceivingEntity(int entityId) {
    EntityId = entityId;
  }

  public DamageReceivingEntity(
      int entityId,
      DamageRecipientComponent damageRecipient,
      PositionComponent position) {
    EntityId = entityId;
    DamageRecipient = damageRecipient;
    _position = position;
    UpdateRect();
  }

  public DamageRecipientComponent DamageRecipient { get; }
  public int EntityId { get; }

  public SysRectangleF Rect {
    get {
      UpdateRect();
      return _rect;
    }
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
    return EntityId;
  }

  public override bool Equals(object obj) {
    return obj is DamageReceivingEntity && GetHashCode() == obj.GetHashCode();
  }
}

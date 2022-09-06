using System;
using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using MonoGame.Extended.Shapes;
using OneOf;

namespace TinyGardenGame.Core.Components {
  /**
   * Represent a source of damage. The idea is in most cases, this will live a short life and
   * self-destruct once the attack is over.
   */
  public class DamageSourceComponent {
    
    public double DamageDealt { get; set; }

    /** If not persistent, destroy on proc */
    public bool IsPersistent { get; set; }

    /** Lifetime of component before damage applies */
    public TimeSpan WindupTime { get; set; } = TimeSpan.Zero;

    public TimeSpan CurrentLifetime { get; set; } = TimeSpan.Zero;

    public HashSet<DamageRecipientComponent.Category> TargetSet { get; init; }

    public OneOf<AoeDamageSource, TargetedDamageSource> DamageType { get; }
    
    public DamageSourceComponent(
        int targetEntityId,
        double damageDealt,
        HashSet<DamageRecipientComponent.Category> targetSet) {
      DamageType = new TargetedDamageSource { TargetEntityId = targetEntityId };
      DamageDealt = damageDealt;
      TargetSet = targetSet;
    }

    public DamageSourceComponent(
        Polygon hitbox,
        double damageDealt,
        HashSet<DamageRecipientComponent.Category> targetSet) {
      DamageType = new AoeDamageSource { DamageHitbox = hitbox };
      DamageDealt = damageDealt;
      TargetSet = targetSet;
    }

    public DamageSourceComponent(
        Polygon hitbox,
        double damageDealt,
        TimeSpan windupTime,
        HashSet<DamageRecipientComponent.Category> targetSet,
        bool isPersistent = false) : this(hitbox, damageDealt, targetSet) {
      WindupTime = windupTime;
      IsPersistent = isPersistent;
    }
  }

  public class AoeDamageSource {
    public Polygon DamageHitbox { get; init; }
    

    public Polygon TranslatedPoly(Vector2 translation) {
      return DamageHitbox
          .TransformedCopy(translation, /* rotation = */ 0, /* scale = */ Vector2.One);
    }
  }
  
  public class TargetedDamageSource {
    public int TargetEntityId { get; init; }
  }
}
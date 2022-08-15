using System;
using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using MonoGame.Extended.Shapes;

namespace TinyGardenGame.Core.Components {
  /**
   * Represent a source of damage. The idea is in most cases, this will live a short life and
   * self-destruct once the attack is over.
   */
  public class DamageSourceComponent {
    private Polygon _damageHitbox;

    public Polygon DamageHitbox { get; set; }
    
    public double DamageDealt { get; set; }

    /** If not persistent, destroy on proc */
    public bool IsPersistent { get; set; }

    /** Lifetime of component before damage applies */
    public TimeSpan WindupTime { get; set; } = TimeSpan.Zero;

    public TimeSpan CurrentLifetime { get; set; } = TimeSpan.Zero;

    public HashSet<DamageRecipientComponent.Category> TargetSet { get; init; }

    public DamageSourceComponent(
        Polygon hitbox,
        double damageDealt,
        HashSet<DamageRecipientComponent.Category> targetSet) {
      DamageHitbox = hitbox;
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

    public Polygon TranslatedPoly(Vector2 translation) {
      DamageHitbox.Offset(translation);
      return DamageHitbox;
    }
  }
}
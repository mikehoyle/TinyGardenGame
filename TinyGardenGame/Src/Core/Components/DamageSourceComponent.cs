using System;
using System.Collections.Generic;

namespace TinyGardenGame.Core.Components {
  /**
   * Represent a source of damage. The idea is in most cases, this will live a short life and
   * self-destruct once the attack is over.
   */
  public class DamageSourceComponent {
    public SysRectangleF DamageHitbox { get; set; }
    public double DamageDealt { get; set; }

    /** If not persistent, destroy on proc */
    public bool IsPersistent { get; set; }

    /** Lifetime of component before damage applies */
    public TimeSpan WindupTime { get; set; } = TimeSpan.Zero;

    public TimeSpan CurrentLifetime { get; set; } = TimeSpan.Zero;

    public HashSet<DamageRecipientComponent.Category> TargetSet { get; init; }

    public DamageSourceComponent(
        SysRectangleF hitbox,
        double damageDealt,
        HashSet<DamageRecipientComponent.Category> targetSet) {
      DamageHitbox = hitbox;
      DamageDealt = damageDealt;
      TargetSet = targetSet;
    }

    public DamageSourceComponent(
        SysRectangleF hitbox,
        double damageDealt,
        TimeSpan windupTime,
        HashSet<DamageRecipientComponent.Category> targetSet,
        bool isPersistent = false) : this(hitbox, damageDealt, targetSet) {
      WindupTime = windupTime;
      IsPersistent = isPersistent;
    }
  }
}
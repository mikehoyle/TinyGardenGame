using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace TinyGardenGame.Core.Components {
  /**
   * Represent a source of damage. The idea is in most cases, this will live a short life and
   * self-destruct once the attack is over.
   */
  public class DamageSourceComponent {
    public RectangleF DamageHitbox { get; set; }
    public double DamageDealt { get; set; }

    /** If not persistent, destroy on proc */
    public bool IsPersistent { get; set; }
    /** Lifetime of component before damage applies */
    public TimeSpan WindupTime { get; set; } = TimeSpan.Zero;
    public TimeSpan CurrentLifetime { get; set; } = TimeSpan.Zero;

    public DamageSourceComponent(RectangleF hitbox, double damageDealt) {
      DamageHitbox = hitbox;
      DamageDealt = damageDealt;
    }

    public DamageSourceComponent(
        RectangleF hitbox, 
        double damageDealt,
        TimeSpan windupTime,
        bool isPersistent = false) : this(hitbox, damageDealt) {
      WindupTime = windupTime;
      IsPersistent = isPersistent;
    }
  }
}
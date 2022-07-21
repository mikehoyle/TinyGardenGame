using System.Drawing;
using QuadTrees.QTreeRectF;

namespace TinyGardenGame.Core.Components {
  public class DamageRecipientComponent {
    public double Hp { get; set; }
    public RectangleF Hitbox { get; }

    public DamageRecipientComponent(double hp, RectangleF hitbox) {
      Hp = hp;
      Hitbox = hitbox;
    }
  }
}
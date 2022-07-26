namespace TinyGardenGame.Core.Components {
  public class DamageRecipientComponent {
    public double Hp { get; set; }
    public SysRectangleF Hitbox { get; }

    public DamageRecipientComponent(double hp, SysRectangleF hitbox) {
      Hp = hp;
      Hitbox = hitbox;
    }
  }
}
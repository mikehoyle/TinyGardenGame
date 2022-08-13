namespace TinyGardenGame.Core.Components {
  public class DamageRecipientComponent {
    public enum Category {
      Friendly,
      Enemy,
    }
    
    public double Hp { get; set; }
    public SysRectangleF Hitbox { get; }
    
    public Category RecipientCategory { get; }

    public DamageRecipientComponent(double hp, SysRectangleF hitbox, Category category) {
      Hp = hp;
      Hitbox = hitbox;
      RecipientCategory = category;
    }
  }
}
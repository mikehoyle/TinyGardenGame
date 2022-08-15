namespace TinyGardenGame.Core.Components {
  public class DamageRecipientComponent {
    public enum Category {
      Friendly,
      Enemy,
    }
    
    public double MaxHp { get; set; }
    public double CurrentHp { get; set; }
    
    public SysRectangleF Hitbox { get; }
    
    public Category RecipientCategory { get; }

    public DamageRecipientComponent(double maxHp, SysRectangleF hitbox, Category category) {
      MaxHp = maxHp;
      CurrentHp = maxHp;
      Hitbox = hitbox;
      RecipientCategory = category;
    }
  }
}
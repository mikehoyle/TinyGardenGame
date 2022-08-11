using MonoGame.Extended;
using TinyGardenGame.Core;
using TinyGardenGame.Units.Components;

namespace TinyGardenGame.Units; 

public class Unit {
  public enum Type {
    Inchworm,
  }
  
  public Type UnitType { get; init; }
  
  public SpriteName Sprite { get; init; }
  
  public RectangleF CollisionFootprint { get; init; }
  
  public int Hp { get; init; }
  
  public float SpeedTilesPerSec { get; init; }

  public EnemyAiComponent.State InitialBehavior { get; set; } = EnemyAiComponent.State.Roam;
}

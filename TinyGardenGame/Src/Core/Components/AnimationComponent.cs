namespace TinyGardenGame.Core.Components; 

public class AnimationComponent {
  public enum Action {
    Idle,
    Run,
    Attack,
  }
  
  public Action AnimationAction { get; set; }
  
  public Direction? AnimationDirection { get; set; }
  
  public bool Loop { get; set; }

  public AnimationComponent(Action action, Direction? direction = null, bool loop = true) {
    AnimationAction = action;
    AnimationDirection = direction;
    Loop = loop;
  }
}

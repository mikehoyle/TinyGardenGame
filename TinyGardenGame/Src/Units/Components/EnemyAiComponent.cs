namespace TinyGardenGame.Units.Components; 

public class EnemyAiComponent {
  public enum State {
    Roam,
  }

  public State ActivityState { get; init; }

  public EnemyAiComponent(State state) {
    ActivityState = state;
  }
}

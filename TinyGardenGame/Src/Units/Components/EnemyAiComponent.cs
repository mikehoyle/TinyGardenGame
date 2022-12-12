using System.Collections.Generic;

namespace TinyGardenGame.Units.Components;

public class EnemyAiComponent {
  public enum State {
    Roam,
    AttackTree,
  }

  public EnemyAiComponent(State state) {
    ActivityState = new Stack<State>();
    // For now, always default to Roam state
    ActivityState.Push(State.Roam);
    if (state != State.Roam) {
      ActivityState.Push(State.AttackTree);
    }
  }

  public Stack<State> ActivityState { get; init; }
}

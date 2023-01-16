using System.Collections.Generic;
using TinyGardenGame.Vars;

namespace TinyGardenGame.Units.Components;

public class EnemyAiComponent {

  public EnemyAiComponent(EnemyAiState state) {
    ActivityState = new Stack<EnemyAiState>();
    // For now, always default to Roam state
    ActivityState.Push(EnemyAiState.Items[EnemyAiState.Type.Roam]);
    if (state.Id != EnemyAiState.Type.Roam) {
      ActivityState.Push(EnemyAiState.Items[EnemyAiState.Type.AttackTree]);
    }
  }

  public Stack<EnemyAiState> ActivityState { get; init; }
}

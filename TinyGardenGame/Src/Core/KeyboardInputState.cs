using System.Collections.Generic;

namespace TinyGardenGame.Core {
  public static class KeyboardInputState {
    private static KeyboardState _previousState;

    public static HashSet<Keys> NewlyPressedKeys { get; } = new HashSet<Keys>();
    public static HashSet<Keys> HeldKeys { get; } = new HashSet<Keys>();
    public static HashSet<Keys> NewlyReleasedKeys { get; } = new HashSet<Keys>();

    public static void Update() {
      NewlyPressedKeys.Clear();
      HeldKeys.Clear();
      NewlyReleasedKeys.Clear();
      var currentState = Keyboard.GetState();
      foreach (var key in currentState.GetPressedKeys()) {
        HeldKeys.Add(key);
        if (_previousState.IsKeyUp(key)) {
          NewlyPressedKeys.Add(key);
        }
      }

      // OPTIMIZE: this is duplicate work, but it's easy right now
      foreach (var key in _previousState.GetPressedKeys()) {
        if (currentState.IsKeyUp(key)) {
          NewlyReleasedKeys.Add(key);
        }
      }

      _previousState = currentState;
    }
  }
}
namespace TinyGardenGame.Core {
  public interface IControlTriggerCondition {
    bool IsTriggered();
  }

  public class KeyHeldCondition : IControlTriggerCondition {
    private readonly Keys _key;

    public KeyHeldCondition(Keys key) {
      _key = key;
    }

    public bool IsTriggered() {
      return KeyboardInputState.HeldKeys.Contains(_key);
    }
  }

  public class KeyPressedCondition : IControlTriggerCondition {
    private readonly Keys _key;

    public KeyPressedCondition(Keys key) {
      _key = key;
    }

    public bool IsTriggered() {
      return KeyboardInputState.NewlyPressedKeys.Contains(_key);
    }
  }

  public class KeyReleasedCondition : IControlTriggerCondition {
    private readonly Keys _key;

    public KeyReleasedCondition(Keys key) {
      _key = key;
    }

    public bool IsTriggered() {
      return KeyboardInputState.NewlyReleasedKeys.Contains(_key);
    }
  }
}
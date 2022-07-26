using System;
using System.Collections.Generic;

namespace TinyGardenGame.Core {
  public class ControlsMapper<T> where T : Enum {
    private List<(IControlTriggerCondition condition, T action)> _actionMap;

    public ControlsMapper() {
      _actionMap = new List<(IControlTriggerCondition condition, T action)>();
    }

    /**
     * Registered Actions take precedence in order of registering.
     */
    public ControlsMapper<T> Register(IControlTriggerCondition condition, T action) {
      _actionMap.Add((condition, action));
      return this;
    }

    public HashSet<T> GetTriggeredActions() {
      var result = new HashSet<T>();
      foreach (var (condition, action) in _actionMap) {
        if (condition.IsTriggered()) {
          result.Add(action);
        }
      }

      return result;
    }
  }
}
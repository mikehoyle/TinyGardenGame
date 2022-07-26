using System.Collections.Generic;

namespace TinyGardenGame.Player.State.Tools {
  /**
   * Holds the currently available tools in a linked list. The active tool is always first in the
   * list.
   */
  public class PlayerTools {
    private int _currentlySelectedTool;

    public LinkedList<Tool> Tools { get; }

    public Tool CurrentlySelectedTool => Tools.First.Value;

    public PlayerTools() {
      _currentlySelectedTool = 0;
      Tools = new LinkedList<Tool>();
      Tools.AddFirst(new HandTool());
    }

    public void SelectNextTool() {
      var first = Tools.First;
      Tools.RemoveFirst();
      Tools.AddLast(first);
    }

    public void SelectPreviousTool() {
      var last = Tools.Last;
      Tools.RemoveLast();
      Tools.AddFirst(last);
    }
  }
}
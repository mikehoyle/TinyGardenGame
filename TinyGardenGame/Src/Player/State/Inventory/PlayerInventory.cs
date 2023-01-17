#nullable enable
using System;
using TinyGardenGame.Vars;

namespace TinyGardenGame.Player.State.Inventory {
  public class PlayerInventory {
    public const int InventoryWidth = 10;

    private readonly InventorySlot?[] _inventoryContents;
    private int _currentlySelectedSlot;

    public int CurrentlySelectedSlot {
      get => _currentlySelectedSlot;
      set => _currentlySelectedSlot = value < 0 ? InventoryWidth - 1 : value % InventoryWidth;
    }

    public InventorySlot? CurrentlySelectedItem => ContentsOfSlot(CurrentlySelectedSlot);

    public PlayerInventory() {
      _inventoryContents = new InventorySlot?[InventoryWidth];
      _currentlySelectedSlot = 0;
    }

    public void AddItem(InventoryItem.Type type, uint amount = 1) {
      var firstAvailableSlot = -1;
      for (var i = 0; i < _inventoryContents.Length; i++) {
        var contents = ContentsOfSlot(i);
        if (contents == null) {
          if (firstAvailableSlot == -1) {
            firstAvailableSlot = i;
          }
        }
        else {
          if (contents.Item.Id == type) {
            contents.Count += amount;
            return;
          }
        }
      }

      if (firstAvailableSlot == -1) {
        // TODO support this
        throw new Exception("No support yet for more items than slots");
      }

      _inventoryContents[firstAvailableSlot] = new InventorySlot {
          Item = InventoryItem.Items[type],
          Count = amount,
      };
    }

    public InventorySlot? ContentsOfSlot(int slot) {
      if (slot >= _inventoryContents.Length || slot < 0) {
        return null;
      }

      if (_inventoryContents[slot] == null) {
        return null;
      }

      return _inventoryContents[slot]!.Count < 1 ? null : _inventoryContents[slot];
    }
  }
}
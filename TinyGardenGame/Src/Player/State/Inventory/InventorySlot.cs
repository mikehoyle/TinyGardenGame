#nullable enable
using TinyGardenGame.Core;
using TinyGardenGame.Vars;
using Sprite = MonoGame.Extended.Sprites.Sprite;

namespace TinyGardenGame.Player.State.Inventory {
  public class InventorySlot {
    private Sprite? _cachedSprite;

    public InventoryItem Item;
    public uint Count;

    public Plant PlantType() {
      return Item.Plant;
    }

    public Sprite GetSprite(ContentManager content) {
      _cachedSprite ??= content.LoadSprite(Item.Sprite.Id);
      return _cachedSprite;
    }

    // Returns whether the specified amount is available to be used
    public bool Expend(uint amount) {
      if (amount > Count) {
        Count = 0;
        return false;
      }

      Count -= amount;
      return true;
    }
  }
}
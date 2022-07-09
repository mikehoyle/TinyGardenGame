#nullable enable
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Sprites;
using TinyGardenGame.Core;
using TinyGardenGame.Plants;

namespace TinyGardenGame.Player.State.Inventory {
  public class InventorySlot {
    private Sprite? _cachedSprite;
    
    public InventoryItem Item;
    public uint Count;

    public PlantType PlantType() {
      return Items.ItemMap[Item].PlantType;
    }

    public Sprite GetSprite(ContentManager content) {
      _cachedSprite ??= content.LoadSprite(Items.ItemMap[Item].Sprite);
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